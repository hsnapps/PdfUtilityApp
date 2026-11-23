// using System;
// using System.Collections.Generic;
// using System.IO;
using PdfSharp.Fonts;

namespace PdfUtilityApp.Services
{
    public class WindowsFontResolver : IFontResolver
    {
        private static bool _isRegistered = false;
        private static readonly string FontsDir = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

        private static readonly Dictionary<string, string> FontFileMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Times New Roman#"] = "times.ttf",
            ["Times New Roman#B"] = "timesbd.ttf",
            ["Times New Roman#I"] = "timesi.ttf",
            ["Times New Roman#BI"] = "timesbi.ttf",

            ["Arial#"] = "arial.ttf",
            ["Arial#B"] = "arialbd.ttf",
            ["Arial#I"] = "ariali.ttf",
            ["Arial#BI"] = "arialbi.ttf",

            ["Courier New#"] = "cour.ttf",
            ["Courier New#B"] = "courbd.ttf",
            ["Courier New#I"] = "couri.ttf",
            ["Courier New#BI"] = "courbi.ttf",

            ["Calibri#"] = "calibri.ttf",
            ["Calibri#B"] = "calibrib.ttf",
            ["Calibri#I"] = "calibrii.ttf",
            ["Calibri#BI"] = "calibriz.ttf",

            ["Verdana#"] = "verdana.ttf",
            ["Verdana#B"] = "verdanab.ttf",
            ["Verdana#I"] = "verdanai.ttf",
            ["Verdana#BI"] = "verdanaz.ttf",

            ["Tahoma#"] = "tahoma.ttf",
            ["Tahoma#B"] = "tahomabd.ttf",
            ["Tahoma#I"] = "tahoma.ttf",
            ["Tahoma#BI"] = "tahomabd.ttf",

            ["Segoe UI#"] = "segoeui.ttf",
            ["Segoe UI#B"] = "segoeuib.ttf",
            ["Segoe UI#I"] = "segoeuii.ttf",
            ["Segoe UI#BI"] = "segoeuiz.ttf"
        };

        public static void EnsureRegistered()
        {
            if (_isRegistered) return;
            GlobalFontSettings.FontResolver = new WindowsFontResolver();
            _isRegistered = true;
        }

        public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            string suffix = (isBold, isItalic) switch
            {
                (true, true) => "#BI",
                (true, false) => "#B",
                (false, true) => "#I",
                _ => "#"
            };

            string key = familyName + suffix;
            if (FontFileMap.ContainsKey(key))
                return new FontResolverInfo(key);

            return new FontResolverInfo("Arial" + suffix);
        }

        public byte[]? GetFont(string faceName)
        {
            if (FontFileMap.TryGetValue(faceName, out string? fileName))
            {
                string fullPath = Path.Combine(FontsDir, fileName);
                if (File.Exists(fullPath))
                    return File.ReadAllBytes(fullPath);
            }

            string fallback = Path.Combine(FontsDir, "arial.ttf");
            return File.Exists(fallback) ? File.ReadAllBytes(fallback) : null;
        }
    }
}