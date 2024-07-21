using System;
using System.IO;
using SkiaSharp;
using Svg.Skia;

namespace SvgProcessingApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: SvgProcessingApp <directoryPath>");
                return;
            }

            string directoryPath = args[0];
            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Directory does not exist: {directoryPath}");
                return;
            }

            var svgFiles = Directory.GetFiles(directoryPath, "*.svg");
            int thumbnailSize = 150; // 1.5 inches at 100 DPI
            int rows = 7;
            int cols = 5;
            int thumbnailCount = rows * cols;

            int width = 850;  // 8.5 inches at 100 DPI
            int height = 1100; // 11 inches at 100 DPI

            using (var surface = SKSurface.Create(new SKImageInfo(width, height)))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White);

                int x = 0;
                int y = 0;
                int counter = 0;

                foreach (var svgFilePath in svgFiles)
                {
                    if (counter >= thumbnailCount)
                        break;

                    var svg = new SKSvg();
                    svg.Load(svgFilePath);

                    if (svg.Picture != null)
                    {
                        float originalWidth = svg.Picture.CullRect.Width;
                        float scaleX = (float)thumbnailSize / originalWidth;
                        float scaleY = scaleX; // Maintain aspect ratio

                        using (var bitmap = SKPictureExtensions.ToBitmap(svg.Picture, SKColors.Transparent, scaleX, scaleY, SKColorType.Rgba8888, SKAlphaType.Unpremul, SKColorSpace.CreateSrgb()))
                        {
                            canvas.DrawBitmap(bitmap, x, y);
                            Console.WriteLine("Processed file : " + svgFilePath);
                            x += thumbnailSize;
                            counter++;

                            if (counter % cols == 0)
                            {
                                x = 0;
                                y += thumbnailSize;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to load SVG file: {svgFilePath}");
                    }
                }

                using (var image = surface.Snapshot())
                using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 100))
                {
                    string outputFilePath = Path.Combine(directoryPath, "thumbnail_sheet.jpg");
                    using (var stream = File.OpenWrite(outputFilePath))
                    {
                        data.SaveTo(stream);
                    }
                    Console.WriteLine($"Thumbnail sheet saved to {outputFilePath}");
                }
            }
        }
    }
}
