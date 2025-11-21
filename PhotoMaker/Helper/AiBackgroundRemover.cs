

using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace PhotoMaker.Helpers
{
    public static class AiBackgroundRemover
    {
        private static InferenceSession _session;

        /// <summary>
        /// Load U2NET-Human-Seg ONNX model
        /// </summary>
        private static void EnsureModelLoaded()
        {
            if (_session != null)
                return;

            string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                @"App_Data\Models\u2net_human_seg.onnx");

            if (!File.Exists(modelPath))
                throw new FileNotFoundException("Could not find ONNX model at: " + modelPath);

            _session = new InferenceSession(modelPath);

            // Debug (run once)
            foreach (var o in _session.OutputMetadata.Keys)
                System.Diagnostics.Debug.WriteLine("HUMAN SEG OUTPUT → " + o);

            foreach (var i in _session.InputMetadata.Keys)
                System.Diagnostics.Debug.WriteLine("HUMAN SEG INPUT → " + i);
        }

        /// <summary>
        /// Background removal using u2net_human_seg
        /// </summary>
        public static Bitmap RemoveBackground(Bitmap input)
        {
            EnsureModelLoaded();

            // Resize to model input (320x320)
            Bitmap resized = new Bitmap(input, new Size(320, 320));

            float[] inputTensor = new float[3 * 320 * 320];
            int idxR = 0, idxG = 320 * 320, idxB = 2 * 320 * 320;

            for (int y = 0; y < 320; y++)
            {
                for (int x = 0; x < 320; x++)
                {
                    Color p = resized.GetPixel(x, y);
                    inputTensor[idxR++] = p.R / 255f;
                    inputTensor[idxG++] = p.G / 255f;
                    inputTensor[idxB++] = p.B / 255f;
                }
            }

            var tensor = new DenseTensor<float>(inputTensor, new[] { 1, 3, 320, 320 });

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(_session.InputMetadata.Keys.First(), tensor)
            };

            // Run inference once
            var results = _session.Run(inputs);

            // Human seg always returns 1 tensor (1×1×320×320)
            var maskTensor = results.First().AsEnumerable<float>().ToArray();

            Bitmap maskBmp = new Bitmap(320, 320, PixelFormat.Format32bppArgb);

            int index = 0;
            for (int y = 0; y < 320; y++)
            {
                for (int x = 0; x < 320; x++)
                {
                    float m = maskTensor[index++];   // Already 0–1
                    int a = (int)(m * 255);

                    maskBmp.SetPixel(x, y, Color.FromArgb(a, 255, 255, 255));
                }
            }

            // Resize mask to original dimensions
            Bitmap maskResized = new Bitmap(maskBmp, input.Size);

            // Final transparent output
            Bitmap output = new Bitmap(input.Width, input.Height, PixelFormat.Format32bppArgb);

            for (int y = 0; y < input.Height; y++)
            {
                for (int x = 0; x < input.Width; x++)
                {
                    Color orig = input.GetPixel(x, y);
                    Color m = maskResized.GetPixel(x, y);

                    output.SetPixel(x, y, Color.FromArgb(m.A, orig.R, orig.G, orig.B));
                }
            }

            return output;
        }

        /// <summary>
        /// Optional solid background (blue, white etc.)
        /// </summary>
        public static Bitmap ApplySolidBackground(Bitmap transparentImage, Color bgColor)
        {
            Bitmap output = new Bitmap(transparentImage.Width, transparentImage.Height);

            using (Graphics g = Graphics.FromImage(output))
            {
                g.Clear(bgColor);
                g.DrawImage(transparentImage, 0, 0,
                    transparentImage.Width, transparentImage.Height);
            }

            return output;
        }
    }
}
