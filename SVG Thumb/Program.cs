using System;
using System.IO;
using System.Collections.Generic;
using SkiaSharp;
using Svg.Skia;

namespace SvgProcessingApp
{
    class Program
    {
        static float pageWidthInches = 0;
        static float pageHeightInches = 0;
        static float thumbnailSizeInches = 0;
        //static string directoryPath = "C:\\Users\\kevin\\Documents\\GitHub\\MyArt";
        static string directoryPath = ".";
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                  Console.WriteLine("Usage: SvgProcessingApp <directoryPath> [pageWidthInches] [pageHeightInches] [thumbnailSizeInches]");
                  pageWidthInches = args.Length > 1 ? float.Parse(args[1]) : 8.5f;
                  pageHeightInches = args.Length > 2 ? float.Parse(args[2]) : 11f;
                  thumbnailSizeInches = args.Length > 3 ? float.Parse(args[3]) : 1.5f;
                  string directoryPath = args[0];
                  return;
            }

            pageWidthInches = 8.5f;
            pageHeightInches = 11.0f;
            thumbnailSizeInches = 1.5f;

            if (!Directory.Exists(directoryPath))
            {
                Console.WriteLine($"Directory does not exist: {directoryPath}");
                return;
            }



            string[] svgFiles = Directory.GetFiles(directoryPath, "*.svg", SearchOption.AllDirectories);
            if (svgFiles.Length == 0)
            {
                Console.WriteLine("No SVG files found to process.");
                return;
            }

            int dpi = 600;
            float inchToPixel = dpi / 2.54f; // Convert DPI to pixels per inch
            int thumbnailSize = (int)(thumbnailSizeInches * inchToPixel);
            int margin = (int)(0.5 * inchToPixel); // 0.5 inches margin
            int rows = (int)((pageHeightInches * inchToPixel - 2 * margin) / thumbnailSize);
            int cols = (int)((pageWidthInches * inchToPixel - 2 * margin) / thumbnailSize);
            int thumbnailsPerPage = rows * cols;
            int width = (int)(pageWidthInches * inchToPixel);
            int height = (int)(pageHeightInches * inchToPixel);

            var groupedSvgFiles = GroupSvgFilesByProportion(svgFiles);
            int pageNumber = 1;
            string catalogDirectory = Path.Combine(directoryPath, "catalogue");
            Directory.CreateDirectory(catalogDirectory);

            using (var surface = CreateNewSurface(width, height))
            {
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.White);

                int x = margin;
                int y = margin;
                int imageHeightWithText = thumbnailSize + (int)(0.2 * inchToPixel); // Thumbnail + text height
                int counter = 0;

                foreach (var group in groupedSvgFiles)
                {
                    foreach (var svgFilePath in group)
                    {
                        if (counter > 0 && counter % thumbnailsPerPage == 0)
                        {
                            SavePage(surface, catalogDirectory, ref pageNumber);
                            Console.WriteLine("Saved page" + pageNumber);
                            canvas = surface.Canvas;
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

                                Console.WriteLine("Drew bitmap for " + svgFilePath);

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
                }

                if (counter % thumbnailsPerPage != 0)
                {
                    SavePage(surface, catalogDirectory, ref pageNumber);
                }
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

                Console.WriteLine("Loaded for grouping " + svgFilePath);

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

        private static SKSurface CreateNewSurface(int width, int height)
        {
            return SKSurface.Create(new SKImageInfo(width, height));
        }

        private static void SavePage(SKSurface surface, string catalogDirectory, ref int pageNumber)
        {
            using (var image = surface.Snapshot())
            using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 100))
            {
                string outputFilePath = Path.Combine(catalogDirectory, $"thumbnail_sheet_page_{pageNumber}.jpg");
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
