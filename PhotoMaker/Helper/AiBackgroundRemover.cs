using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace PhotoMaker.Helpers
{
    public static class AiBackgroundRemover
    {
        private static InferenceSession _session;

        /// <summary>
        /// Metadata so we can map the 320x320 mask back to the original image.
        /// </summary>
        private class ResizeMetadata
        {
            public int OriginalWidth { get; set; }
            public int OriginalHeight { get; set; }
            public int ResizedWidth { get; set; }
            public int ResizedHeight { get; set; }
            public int OffsetX { get; set; }
            public int OffsetY { get; set; }
        }

        /// <summary>
        /// Load U2NET-Human-Seg ONNX model (once).
        /// </summary>
        private static void EnsureModelLoaded()
        {
            if (_session != null)
                return;

            string modelPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                @"App_Data\Models\u2net_human_seg.onnx");

            if (!File.Exists(modelPath))
                throw new FileNotFoundException("Could not find ONNX model at: " + modelPath);

            _session = new InferenceSession(modelPath);

            // Optional debug (only once)
            foreach (var o in _session.OutputMetadata.Keys)
                System.Diagnostics.Debug.WriteLine("HUMAN SEG OUTPUT → " + o);

            foreach (var i in _session.InputMetadata.Keys)
                System.Diagnostics.Debug.WriteLine("HUMAN SEG INPUT → " + i);
        }

        /// <summary>
        /// Prepares input for U2NET:
        ///   - keeps aspect ratio, resizes so Height = 320
        ///   - pads to 320x320 with black
        ///   - returns padded bitmap + metadata.
        /// </summary>
        private static Bitmap PrepareInput(Bitmap original, out ResizeMetadata meta)
        {
            const int targetSize = 320;

            meta = new ResizeMetadata
            {
                OriginalWidth = original.Width,
                OriginalHeight = original.Height
            };

            // Scale: fix height = 320, width proportional
            float scale = (float)targetSize / original.Height;
            int resizedWidth = (int)(original.Width * scale);
            int resizedHeight = targetSize;

            meta.ResizedWidth = resizedWidth;
            meta.ResizedHeight = resizedHeight;

            // First resize with aspect ratio
            Bitmap resized = new Bitmap(resizedWidth, resizedHeight, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.DrawImage(original, 0, 0, resizedWidth, resizedHeight);
            }

            // Then pad to 320x320 (center horizontally, full height)
            Bitmap padded = new Bitmap(targetSize, targetSize, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(padded))
            {
                g.Clear(Color.Black); // recommended padding color for U2NET

                int offsetX = (targetSize - resizedWidth) / 2;
                int offsetY = (targetSize - resizedHeight) / 2; // usually 0 for height=320

                meta.OffsetX = offsetX;
                meta.OffsetY = offsetY;

                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(resized, offsetX, offsetY, resizedWidth, resizedHeight);
            }

            resized.Dispose();
            return padded;
        }

        /// <summary>
        /// Runs u2net_human_seg on the image and returns a transparent Bitmap.
        /// </summary>
        public static Bitmap RemoveBackground(Bitmap input)
        {
            EnsureModelLoaded();

            ResizeMetadata meta;
            Bitmap modelInput = PrepareInput(input, out meta);   // 320x320 padded

            try
            {
                // Build NCHW tensor: [1,3,320,320]
                const int size = 320;
                float[] inputTensor = new float[3 * size * size];
                int plane = size * size;
                int idxR = 0;
                int idxG = plane;
                int idxB = plane * 2;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        Color p = modelInput.GetPixel(x, y);
                        float r = p.R / 255f;
                        float g = p.G / 255f;
                        float b = p.B / 255f;

                        inputTensor[idxR++] = r;
                        inputTensor[idxG++] = g;
                        inputTensor[idxB++] = b;
                    }
                }

                var tensor = new DenseTensor<float>(inputTensor, new[] { 1, 3, size, size });

                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor(_session.InputMetadata.Keys.First(), tensor)
                };

                // Run inference
                using (var results = _session.Run(inputs))
                {
                    // u2net_human_seg returns 1 output: [1,1,320,320]
                    float[] maskTensor = results.First().AsEnumerable<float>().ToArray();

                    // Convert 1D to 2D mask for easier indexing
                    float[,] mask2D = new float[size, size];
                    for (int i = 0; i < size * size; i++)
                    {
                        int y = i / size;
                        int x = i % size;
                        mask2D[y, x] = maskTensor[i];
                    }

                    // Crop the mask back to the resized area (remove padding)
                    Bitmap maskResized = new Bitmap(meta.ResizedWidth, meta.ResizedHeight, PixelFormat.Format32bppArgb);
                    for (int y = 0; y < meta.ResizedHeight; y++)
                    {
                        for (int x = 0; x < meta.ResizedWidth; x++)
                        {
                            float m = mask2D[y + meta.OffsetY, x + meta.OffsetX]; // 0–1
                            if (m < 0f) m = 0f;
                            if (m > 1f) m = 1f;

                            int a = (int)(m * 255f);
                            maskResized.SetPixel(x, y, Color.FromArgb(a, 255, 255, 255));
                        }
                    }

                    // Scale mask back to original image size (no distortion)
                    Bitmap maskOriginal = new Bitmap(meta.OriginalWidth, meta.OriginalHeight, PixelFormat.Format32bppArgb);
                    using (Graphics g = Graphics.FromImage(maskOriginal))
                    {
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.DrawImage(maskResized, 0, 0, meta.OriginalWidth, meta.OriginalHeight);
                    }

                    maskResized.Dispose();

                    // Compose final transparent image
                    Bitmap output = new Bitmap(meta.OriginalWidth, meta.OriginalHeight, PixelFormat.Format32bppArgb);

                    for (int y = 0; y < meta.OriginalHeight; y++)
                    {
                        for (int x = 0; x < meta.OriginalWidth; x++)
                        {
                            Color orig = input.GetPixel(x, y);
                            Color mPix = maskOriginal.GetPixel(x, y);

                            // Alpha from mask
                            Color outPix = Color.FromArgb(mPix.A, orig.R, orig.G, orig.B);
                            output.SetPixel(x, y, outPix);
                        }
                    }

                    maskOriginal.Dispose();
                    return output;
                }
            }
            finally
            {
                modelInput.Dispose();
            }
        }

        /// <summary>
        /// Optional: Apply a solid background (white, blue, etc.) to the transparent result.
        /// </summary>
        public static Bitmap ApplySolidBackground(Bitmap transparentImage, Color bgColor)
        {
            Bitmap output = new Bitmap(transparentImage.Width, transparentImage.Height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(output))
            {
                g.Clear(bgColor);
                g.CompositingMode = CompositingMode.SourceOver;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;

                g.DrawImage(
                    transparentImage,
                    0,
                    0,
                    transparentImage.Width,
                    transparentImage.Height);
            }

            return output;
        }

        /// <summary>
        /// Optional: free ONNX session on app shutdown.
        /// </summary>
        public static void DisposeModel()
        {
            _session?.Dispose();
            _session = null;
        }
    }
}
