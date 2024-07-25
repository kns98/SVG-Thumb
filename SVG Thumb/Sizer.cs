using System;
using System.Collections.Generic;

namespace SVG_Thumb
{
    class Program
    {
        static readonly Dictionary<string, Dictionary<string, (double width, double height)>> paperSizesDict =
            new()
        {
            {
                "ISO", new Dictionary<string, (double width, double height)>
                {
                    { "A0", (33.11, 46.81) },
                    { "A1", (23.39, 33.11) },
                    { "A2", (16.54, 23.39) },
                    { "A3", (11.69, 16.54) },
                    { "A4", (8.27, 11.69) },
                    { "A5", (5.83, 8.27) },
                    { "A6", (4.13, 5.83) },
                    { "A7", (2.91, 4.13) },
                    { "A8", (2.05, 2.91) },
                    { "A9", (1.46, 2.05) },
                    { "A10", (1.02, 1.46) },
                    { "B0", (39.37, 55.67) },
                    { "B1", (27.83, 39.37) },
                    { "B2", (19.69, 27.83) },
                    { "B3", (13.90, 19.69) },
                    { "B4", (9.84, 13.90) },
                    { "B5", (6.93, 9.84) },
                    { "B6", (4.92, 6.93) },
                    { "B7", (3.46, 4.92) },
                    { "B8", (2.44, 3.46) },
                    { "B9", (1.73, 2.44) },
                    { "B10", (1.22, 1.73) }
                }
            },
            {
                "US", new Dictionary<string, (double width, double height)>
                {
                    { "Letter", (8.5, 11) },
                    { "Legal", (8.5, 14) },
                    { "Tabloid", (11, 17) },
                    { "Ledger", (17, 11) },
                    { "Executive", (7.25, 10.5) },
                    { "Half Letter", (5.5, 8.5) }
                }
            },
            {
                "Asia", new Dictionary<string, (double width, double height)>
                {
                    { "Chinese D", (30.31, 42.91) },
                    { "Japanese B0", (40.55, 57.32) },
                    { "Japanese B1", (28.66, 40.55) },
                    { "Japanese B2", (20.28, 28.66) },
                    { "Japanese B3", (14.33, 20.28) },
                    { "Japanese B4", (10.12, 14.33) },
                    { "Japanese B5", (7.17, 10.12) }
                }
            }
        };



    }
}
