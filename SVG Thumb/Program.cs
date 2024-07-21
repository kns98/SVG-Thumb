using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Svg.Skia;

namespace SvgProcessingApp
{
    class Program
    {
        static int dpi = 600;
        static float inchToPixel = dpi / 2.54f; // Convert DPI to pixels per inch

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
            if (svgFiles.Length == 0)
            {
                Console.WriteLine("No SVG files found to process.");
                return;
            }

            int thumbnailSize = (int)(1.5 * inchToPixel); // 1.5 inches
            int margin = (int)(0.5 * inchToPixel); // 0.5 inches margin
            int rows = 7;
            int cols = 5;
            int thumbnailsPerPage = rows * cols;
            int width = (int)(8.5 * inchToPixel);
            int height = (int)(11 * inchToPixel);

            var groupedSvgFiles = GroupSvgFilesByProportion(svgFiles);
            int pageNumber = 1;

            foreach (var group in groupedSvgFiles)
            {
                ProcessSvgGroup(group, directoryPath, width, height, thumbnailSize, margin, rows, cols, thumbnailsPerPage, ref pageNumber);
            }
        }

        private static List<List<string>> GroupSvgFilesByProportion(string[] svgFiles)
        {
            var groups = new List<List<string>>();
            var currentGroup = new List<string>();
            float? currentProportion = null;

            foreach (var svgFilePath in svgFiles)
            {
                var svg = new SKSvg();
                svg.Load(svgFilePath);

                if (svg.Picture != null)
                {
                    float width = svg.Picture.CullRect.Width;
                    float height = svg.Picture.CullRect.Height;
                    float proportion = width / height;

                    if (currentProportion == null || Math.Abs(currentProportion.Value - proportion) < 0.1)
                    {
                        currentGroup.Add(svgFilePath);
                        currentProportion = proportion;
                    }
                    else
                    {
                        groups.Add(new List<string>(currentGroup));
                        currentGroup.Clear();
                        currentGroup.Add(svgFilePath);
                        currentProportion = proportion;
                    }
                }
            }

            if (currentGroup.Count > 0)
            {
                groups.Add(currentGroup);
            }

            return groups;
        }

        private static void ProcessSvgGroup(List<string> svgGroup, string directoryPath, int width, int height, int thumbnailSize, int margin, int rows, int cols, int thumbnailsPerPage, ref int pageNumber)
        {
            int counter = 0;
            using (var surface = SKSurface.Create(new SKImageInfo(width, height)))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White);

                int x = margin;
                int y = margin;
                int imageHeightWithText = thumbnailSize + (int)(0.2 * inchToPixel); // Thumbnail + text height

                foreach (var svgFilePath in svgGroup)
                {
                    if (counter > 0 && counter % thumbnailsPerPage == 0)
                    {
                        SavePage(surface, directoryPath, ref pageNumber);
                        canvas.Clear(SKColors.White);
                        x = margin;
                        y = margin;
                    }

                    var svg = new SKSvg();
                    svg.Load(svgFilePath);

                    if (svg.Picture != null)
                    {
                        float originalWidth = svg.Picture.CullRect.Width;
                        float originalHeight = svg.Picture.CullRect.Height;
                        float scale = Math.Min((float)thumbnailSize / originalWidth, (float)thumbnailSize / originalHeight);

                        using (var bitmap = SKPictureExtensions.ToBitmap(svg.Picture, SKColors.Transparent, scale, scale, SKColorType.Rgba8888, SKAlphaType.Unpremul, SKColorSpace.CreateSrgb()))
                        {
                            float offsetX = (thumbnailSize - bitmap.Width) / 2f;
                            float offsetY = (thumbnailSize - bitmap.Height) / 2f;
                            canvas.DrawBitmap(bitmap, x + offsetX, y + offsetY);

                            var paint = new SKPaint
                            {
                                Color = SKColors.Black,
                                IsAntialias = true,
                                TextSize = 10 * inchToPixel / 72f // 10pt font size
                            };

                            string fileName = Path.GetFileName(svgFilePath);
                            canvas.DrawText(fileName, x, y + thumbnailSize + paint.TextSize, paint);

                            Console.WriteLine($"Processed: {fileName}");

                            x += thumbnailSize + margin;
                            if (x + thumbnailSize + margin > width)
                            {
                                x = margin;
                                y += imageHeightWithText + margin;
                            }

                            counter++;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Failed to load SVG file: {svgFilePath}");
                    }
                }

                SavePage(surface, directoryPath, ref pageNumber);
            }
        }

        private static void SavePage(SKSurface surface, string directoryPath, ref int pageNumber)
        {
            using (var image = surface.Snapshot())
            using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 100))
            {
                string outputFilePath = Path.Combine(directoryPath, $"thumbnail_sheet_page_{pageNumber}.jpg");
                using (var stream = File.OpenWrite(outputFilePath))
                {
                    data.SaveTo(stream);
                }
                Console.WriteLine($"Saved page {pageNumber} to {outputFilePath}");
                pageNumber++;
            }
        }
    }
}
