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
        /// Loads the ONNX model (u2net.onnx)
        /// </summary>
        private static void EnsureModelLoaded()
        {
            if (_session != null)
                return;

            string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                @"App_Data\Models\u2net.onnx");

            if (!File.Exists(modelPath))
                throw new FileNotFoundException("Could not find ONNX model at: " + modelPath);

            _session = new InferenceSession(modelPath);
        }

        /// <summary>
        /// Removes background using U2Net ONNX – returns transparent PNG-like bitmap.
        /// </summary>
        public static Bitmap RemoveBackground(Bitmap input)
        {
            EnsureModelLoaded();

            // Resize to 320x320 — U2Net requirement
            Bitmap resized = new Bitmap(input, new Size(320, 320));

            // Convert to tensor
            float[] inputTensor = new float[3 * 320 * 320];
            int index = 0;

            for (int y = 0; y < 320; y++)
            {
                for (int x = 0; x < 320; x++)
                {
                    Color pixel = resized.GetPixel(x, y);

                    inputTensor[index++] = pixel.R / 255f;
                    inputTensor[index++] = pixel.G / 255f;
                    inputTensor[index++] = pixel.B / 255f;
                }
            }

            var shape = new int[] { 1, 3, 320, 320 };
            var tensor = new DenseTensor<float>(inputTensor, shape);

            var inputs = new List<NamedOnnxValue>();
            inputs.Add(NamedOnnxValue.CreateFromTensor("input", tensor));

            IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = _session.Run(inputs);

            // Get mask
            var maskTensor = results.First().AsEnumerable<float>();

            Bitmap output = new Bitmap(input.Width, input.Height, PixelFormat.Format32bppArgb);

            // Apply mask
            int i = 0;
            foreach (float m in maskTensor)
            {
                int xx = (i % 320) * input.Width / 320;
                int yy = (i / 320) * input.Height / 320;

                Color orig = input.GetPixel(xx, yy);
                int alpha = (int)(m * 255);

                output.SetPixel(xx, yy, Color.FromArgb(alpha, orig.R, orig.G, orig.B));

                i++;
            }

            return output;
        }

        /// <summary>
        /// Applies a solid background (white/blue/etc.) to a transparent image.
        /// </summary>
        public static Bitmap ApplySolidBackground(Bitmap transparentImage, Color bgColor)
        {
            Bitmap output = new Bitmap(transparentImage.Width, transparentImage.Height);

            using (Graphics g = Graphics.FromImage(output))
            {
                // Fill background
                g.Clear(bgColor);

                // Draw image on top
                g.DrawImage(transparentImage, 0, 0, transparentImage.Width, transparentImage.Height);
            }

            return output;
        }
    }
}
