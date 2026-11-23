using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Docnet.Core;
using Docnet.Core.Models;

namespace PdfUtilityApp.Services
{
    public class PdfEncryptionOptions
    {
        public string UserPassword { get; set; } = string.Empty;
        public string OwnerPassword { get; set; } = string.Empty;
        public bool Use128Bit { get; set; }
        public bool PermitPrint { get; set; } = true;
        public bool PermitModifyDocument { get; set; } = true;
        public bool PermitExtractContent { get; set; } = true;
        public bool PermitAnnotations { get; set; } = true;
        public bool PermitFormsFill { get; set; } = true;
        public bool PermitAccessibilityExtractContent { get; set; } = true;
        public bool PermitAssembleDocument { get; set; } = true;
        public bool PermitFullQualityPrint { get; set; } = true;
    }

    public class PageTransformInfo
    {
        public int PageNumber { get; set; }
        public double WidthInches { get; set; }
        public double HeightInches { get; set; }
        public string Orientation => WidthInches > HeightInches ? "L" : "P";
        public int CurrentRotation { get; set; }
        public int RotateBy { get; set; }
        public double LeftMarginInches { get; set; }
        public double RightMarginInches { get; set; }
        public double BottomMarginInches { get; set; }
        public double TopMarginInches { get; set; }
    }

    public class HeaderFooterBandOptions
    {
        public bool Enabled { get; set; } = true;
        public bool AddOnFirstPage { get; set; } = true;
        public bool ShowPageNumber { get; set; } = true;
        public string TextBefore { get; set; } = string.Empty;
        public string TextAfter { get; set; } = string.Empty;
        public string Alignment { get; set; } = "Left"; // Left, Center, Right
        public int PageOffset { get; set; } = 0;
    }

    public class HeaderFooterOptions
    {
        public HeaderFooterBandOptions Header { get; set; } = new();
        public HeaderFooterBandOptions Footer { get; set; } = new();
        public string FontEncode { get; set; } = "Western";
        public string FontName { get; set; } = "Times New Roman";
        public string FontStyle { get; set; } = "Regular"; // Regular, Bold, Italic, Bold Italic
        public double FontSize { get; set; } = 12.0;
        public System.Drawing.Color FontColor { get; set; } = System.Drawing.Color.Blue;
        public double MarginLeftInches { get; set; } = 0.5;
        public double MarginRightInches { get; set; } = 0.5;
        public double MarginTopInches { get; set; } = 0.5;
        public double MarginBottomInches { get; set; } = 0.5;
    }

    public class TextWatermarkOptions
    {
        public string Text { get; set; } = "WATERMARK EXAMPLE HERE";
        public string FontEncode { get; set; } = "Western";
        public string FontName { get; set; } = "Times New Roman";
        public string FontStyle { get; set; } = "Regular";
        public System.Drawing.Color FontColor { get; set; } = System.Drawing.Color.Blue;
        public System.Drawing.Color StrokeColor { get; set; } = System.Drawing.Color.Red;
        public string Layout { get; set; } = "Horizontal"; // Horizontal, Diagonal, Vertical
        public string DrawType { get; set; } = "Stroke";     // Stroke, Fill, Fill & Stroke
        public double StrokeSizePercent { get; set; } = 1.00;
        public bool IsBackground { get; set; } = true;
    }

    public class ImageWatermarkOptions
    {
        public string ImagePath { get; set; } = string.Empty;
        public double WidthPercent { get; set; } = 100.0;
        public double HeightPercent { get; set; } = 100.0;
        public double RotateDegrees { get; set; } = 0.0;
        public bool CenterOfPage { get; set; } = true;
        public double LeftInches { get; set; } = 0.5;
        public double BottomInches { get; set; } = 0.5;
        public bool IsBackground { get; set; } = true;
        public string? Url { get; set; }
    }

    public class ImageToPdfItem
    {
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public int WidthPoints { get; set; }
        public int HeightPoints { get; set; }
        public int PageCount { get; set; } = 1;
    }

    public class ImagesToPdfOptions
    {
        public bool ShowTitle { get; set; }
        public bool UseImageSizeWithDpi { get; set; }
        public int ImageSizeDpi { get; set; } = 300;
        public double PageWidthInches { get; set; } = 8.5;
        public double PageHeightInches { get; set; } = 11.0;
        public int Rows { get; set; } = 1;
        public int Columns { get; set; } = 1;
        public double MarginLeftInches { get; set; } = 0.5;
        public double MarginRightInches { get; set; } = 0.5;
        public double MarginBottomInches { get; set; } = 0.5;
        public double MarginTopInches { get; set; } = 0.5;
        public double SpaceHorizontalInches { get; set; } = 0.25;
        public double SpaceVerticalInches { get; set; } = 0.25;
        public bool UseImageCompress { get; set; }
        public int CompressDpi { get; set; } = 150;
    }

    public class PdfToImagesOptions
    {
        public int FromPage { get; set; } = 1;
        public int ToPage { get; set; } = 1;
        public int Dpi { get; set; } = 144;
        public int ColorBits { get; set; } = 24; // 24, 8 (Grayscale), 1 (B&W)
        public int PageNumberDigits { get; set; } = 1;
        public string ImageType { get; set; } = "PNG"; // PNG, JPG, TIF, GIF, BMP, EPS, WMF, EMF
        public int JpgQuality { get; set; } = 100;
        public bool SingleMultiPageTiff { get; set; } = false;
    }

    public class PdfService
    {
        /// <summary>
        /// Gets the page count, supporting optional password for encrypted PDFs.
        /// </summary>
        public int GetPageCount(string filePath, string? password = null)
        {
            using var doc = string.IsNullOrEmpty(password)
                ? PdfReader.Open(filePath, PdfDocumentOpenMode.Import)
                : PdfReader.Open(filePath, password, PdfDocumentOpenMode.Import);
            return doc.PageCount;
        }

        /// <summary>
        /// Merges multiple PDF files into a single output file, optionally adding top-level bookmarks per file.
        /// </summary>
        public void MergePdfs(IEnumerable<string> inputPaths, string outputPath, bool addBookmarks)
        {
            using var outputDocument = new PdfDocument();

            foreach (var filePath in inputPaths)
            {
                using var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
                int startPageIndex = outputDocument.PageCount;

                for (int i = 0; i < inputDocument.PageCount; i++)
                {
                    outputDocument.AddPage(inputDocument.Pages[i]);
                }

                if (addBookmarks && inputDocument.PageCount > 0)
                {
                    string bookmarkTitle = Path.GetFileNameWithoutExtension(filePath);
                    outputDocument.Outlines.Add(bookmarkTitle, outputDocument.Pages[startPageIndex], true);
                }
            }

            outputDocument.Save(outputPath);
        }

        /// <summary>
        /// Parses page expressions like "1, 3-5, even, odd, reverse, last" into 1-based page numbers.
        /// </summary>
        public List<int> ParsePageExpression(string expression, int totalPages, bool allowReverse = true)
        {
            var result = new List<int>();
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Please enter at least one page number or keyword.");

            var tokens = expression.Split(new[] { ',', '、', ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var rawToken in tokens)
            {
                string token = rawToken.Trim().ToLowerInvariant();

                if (token == "even")
                {
                    for (int p = 2; p <= totalPages; p += 2) result.Add(p);
                }
                else if (token == "odd")
                {
                    for (int p = 1; p <= totalPages; p += 2) result.Add(p);
                }
                else if (token == "reverse" && allowReverse)
                {
                    for (int p = totalPages; p >= 1; p--) result.Add(p);
                }
                else if (token == "last")
                {
                    result.Add(totalPages);
                }
                else if (token.Contains('-'))
                {
                    var parts = token.Split('-');
                    if (parts.Length != 2)
                        throw new ArgumentException($"Invalid range '{rawToken}'.");

                    int start = parts[0].Trim() == "last" ? totalPages : int.Parse(parts[0].Trim());
                    int end = parts[1].Trim() == "last" ? totalPages : int.Parse(parts[1].Trim());

                    ValidatePageNumber(start, totalPages);
                    ValidatePageNumber(end, totalPages);

                    if (start <= end)
                    {
                        for (int p = start; p <= end; p++) result.Add(p);
                    }
                    else
                    {
                        for (int p = start; p >= end; p--) result.Add(p);
                    }
                }
                else
                {
                    if (!int.TryParse(token, out int pageNum))
                        throw new ArgumentException($"Invalid page token '{rawToken}'.");

                    ValidatePageNumber(pageNum, totalPages);
                    result.Add(pageNum);
                }
            }

            return result;
        }

        private void ValidatePageNumber(int page, int totalPages)
        {
            if (page < 1 || page > totalPages)
                throw new ArgumentOutOfRangeException(nameof(page),
                    $"Page {page} is out of range. This document has {totalPages} pages.");
        }

        /// <summary>
        /// Saves a specified list of 1-based page numbers from inputPath to outputPath.
        /// Optionally splits into chunks of pagesPerFile and copies matching top-level bookmarks.
        /// </summary>
        public List<string> ExtractPagesToPdf(
            string inputPath,
            string outputPath,
            IList<int> oneBasedPages,
            bool keepBookmarks,
            int? pagesPerFile = null)
        {
            var createdFiles = new List<string>();
            using var sourceDoc = PdfReader.Open(inputPath, PdfDocumentOpenMode.Import);

            var sourcePageMap = new Dictionary<PdfPage, int>();
            for (int i = 0; i < sourceDoc.PageCount; i++)
            {
                sourcePageMap[sourceDoc.Pages[i]] = i + 1;
            }

            if (pagesPerFile.HasValue && pagesPerFile.Value > 0)
            {
                string dir = Path.GetDirectoryName(outputPath) ?? string.Empty;
                string baseName = Path.GetFileNameWithoutExtension(outputPath);
                string ext = Path.GetExtension(outputPath);

                int chunkSize = pagesPerFile.Value;
                int partIndex = 1;

                for (int i = 0; i < oneBasedPages.Count; i += chunkSize)
                {
                    var chunk = oneBasedPages.Skip(i).Take(chunkSize).ToList();
                    string partPath = Path.Combine(dir, $"{baseName}_part{partIndex}{ext}");
                    WriteSingleOutput(sourceDoc, sourcePageMap, chunk, partPath, keepBookmarks);
                    createdFiles.Add(partPath);
                    partIndex++;
                }
            }
            else
            {
                WriteSingleOutput(sourceDoc, sourcePageMap, oneBasedPages, outputPath, keepBookmarks);
                createdFiles.Add(outputPath);
            }

            return createdFiles;
        }

        private void WriteSingleOutput(
            PdfDocument sourceDoc,
            Dictionary<PdfPage, int> sourcePageMap,
            IList<int> oneBasedPages,
            string outputPath,
            bool keepBookmarks)
        {
            using var outDoc = new PdfDocument();
            var exportedPageMap = new Dictionary<int, PdfPage>();

            foreach (int p in oneBasedPages)
            {
                var addedPage = outDoc.AddPage(sourceDoc.Pages[p - 1]);
                if (!exportedPageMap.ContainsKey(p))
                {
                    exportedPageMap[p] = addedPage;
                }
            }

            if (keepBookmarks && sourceDoc.Outlines.Count > 0)
            {
                foreach (var outline in sourceDoc.Outlines)
                {
                    if (outline.DestinationPage != null &&
                        sourcePageMap.TryGetValue(outline.DestinationPage, out int origPageNum) &&
                        exportedPageMap.TryGetValue(origPageNum, out var targetPage))
                    {
                        outDoc.Outlines.Add(outline.Title, targetPage, true);
                    }
                }
            }

            outDoc.Save(outputPath);
        }

        /// <summary>
        /// Encrypts a PDF with User/Owner passwords and granular permission flags.
        /// </summary>
        public void EncryptPdf(string inputPath, string outputPath, PdfEncryptionOptions options, string? existingPassword = null)
        {
            if (string.IsNullOrEmpty(options.UserPassword) && string.IsNullOrEmpty(options.OwnerPassword))
                throw new ArgumentException("Please enter at least a User Password or an Owner Password.");

            using var sourceDoc = string.IsNullOrEmpty(existingPassword)
                ? PdfReader.Open(inputPath, PdfDocumentOpenMode.Import)
                : PdfReader.Open(inputPath, existingPassword, PdfDocumentOpenMode.Import);

            using var outDoc = new PdfDocument();
            for (int i = 0; i < sourceDoc.PageCount; i++)
            {
                outDoc.AddPage(sourceDoc.Pages[i]);
            }

            PdfSecuritySettings security = outDoc.SecuritySettings;

            if (!string.IsNullOrEmpty(options.UserPassword))
                security.UserPassword = options.UserPassword;

            // OwnerPassword is required when applying permission restrictions
            security.OwnerPassword = !string.IsNullOrEmpty(options.OwnerPassword)
                ? options.OwnerPassword
                : options.UserPassword;

            security.PermitPrint = options.PermitPrint;
            security.PermitModifyDocument = options.PermitModifyDocument;
            security.PermitExtractContent = options.PermitExtractContent || options.PermitAccessibilityExtractContent;
            security.PermitAnnotations = options.PermitAnnotations;
            security.PermitFormsFill = options.PermitFormsFill;
            security.PermitAssembleDocument = options.PermitAssembleDocument;
            security.PermitFullQualityPrint = options.PermitFullQualityPrint;

            outDoc.Save(outputPath);
        }

        /// <summary>
        /// Removes password protection and security restrictions, saving an unencrypted PDF.
        /// </summary>
        public void DecryptPdf(string inputPath, string outputPath, string? password)
        {
            using var sourceDoc = string.IsNullOrEmpty(password)
                ? PdfReader.Open(inputPath, PdfDocumentOpenMode.Import)
                : PdfReader.Open(inputPath, password, PdfDocumentOpenMode.Import);

            using var outDoc = new PdfDocument();
            for (int i = 0; i < sourceDoc.PageCount; i++)
            {
                outDoc.AddPage(sourceDoc.Pages[i]);
            }

            // A fresh PdfDocument with no passwords set saves completely unencrypted
            outDoc.Save(outputPath);
        }

        /// <summary>
        /// Reads page dimensions (in inches), orientation, and rotation for every page in the PDF.
        /// </summary>
        public List<PageTransformInfo> GetPagesTransformInfo(string inputPath)
        {
            var list = new List<PageTransformInfo>();
            using var doc = PdfReader.Open(inputPath, PdfDocumentOpenMode.Import);

            for (int i = 0; i < doc.PageCount; i++)
            {
                var page = doc.Pages[i];
                // PDFsharp page Width/Height are in points (72 points = 1 inch)
                double widthInches = Math.Round(page.Width.Point / 72.0, 5);
                double heightInches = Math.Round(page.Height.Point / 72.0, 5);

                list.Add(new PageTransformInfo
                {
                    PageNumber = i + 1,
                    WidthInches = widthInches,
                    HeightInches = heightInches,
                    CurrentRotation = page.Rotate
                });
            }

            return list;
        }

        /// <summary>
        /// Applies per-page rotation and crop box margins (in inches) and saves to outputPath.
        /// </summary>
        public void RotateAndCropPdf(string inputPath, string outputPath, IList<PageTransformInfo> pageTransforms)
        {
            using var sourceDoc = PdfReader.Open(inputPath, PdfDocumentOpenMode.Import);
            using var outDoc = new PdfDocument();

            for (int i = 0; i < sourceDoc.PageCount; i++)
            {
                var page = outDoc.AddPage(sourceDoc.Pages[i]);
                var transform = pageTransforms[i];

                // Apply rotation (normalized to 0, 90, 180, 270)
                if (transform.RotateBy != 0)
                {
                    int newRotation = (page.Rotate + transform.RotateBy) % 360;
                    if (newRotation < 0) newRotation += 360;
                    page.Rotate = newRotation;
                }

                // Apply CropBox if any margin > 0
                if (transform.LeftMarginInches != 0 ||
                    transform.RightMarginInches != 0 ||
                    transform.BottomMarginInches != 0 ||
                    transform.TopMarginInches != 0)
                {
                    double leftPts = transform.LeftMarginInches * 72.0;
                    double rightPts = transform.RightMarginInches * 72.0;
                    double bottomPts = transform.BottomMarginInches * 72.0;
                    double topPts = transform.TopMarginInches * 72.0;

                    var mediaBox = page.MediaBox;
                    double x1 = mediaBox.X1 + leftPts;
                    double y1 = mediaBox.Y1 + bottomPts;
                    double x2 = mediaBox.X2 - rightPts;
                    double y2 = mediaBox.Y2 - topPts;

                    if (x2 <= x1 || y2 <= y1)
                        throw new ArgumentException($"Crop margins on page {transform.PageNumber} exceed page dimensions.");

                    page.CropBox = new PdfRectangle(new XPoint(x1, y1), new XPoint(x2, y2));
                }
            }

            outDoc.Save(outputPath);
        }

        /// <summary>
        /// Draws customizable headers and footers with optional page numbers, alignment, and margins onto a PDF.
        /// </summary>
        public void AddHeaderAndFooter(string inputPath, string outputPath, HeaderFooterOptions opts)
        {
            WindowsFontResolver.EnsureRegistered();

            using var sourceDoc = PdfReader.Open(inputPath, PdfDocumentOpenMode.Import);
            using var outDoc = new PdfDocument();

            XFontStyleEx style = opts.FontStyle switch
            {
                "Bold" => XFontStyleEx.Bold,
                "Italic" => XFontStyleEx.Italic,
                "Bold Italic" => XFontStyleEx.BoldItalic,
                _ => XFontStyleEx.Regular
            };

            var font = new XFont(opts.FontName, opts.FontSize, style);
            var xColor = XColor.FromArgb(opts.FontColor.A, opts.FontColor.R, opts.FontColor.G, opts.FontColor.B);
            var brush = new XSolidBrush(xColor);

            double mLeft = opts.MarginLeftInches * 72.0;
            double mRight = opts.MarginRightInches * 72.0;
            double mTop = opts.MarginTopInches * 72.0;
            double mBottom = opts.MarginBottomInches * 72.0;

            for (int i = 0; i < sourceDoc.PageCount; i++)
            {
                var page = outDoc.AddPage(sourceDoc.Pages[i]);
                int pageNum = i + 1;
                bool isFirstPage = pageNum == 1;

                using var gfx = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
                double pageW = page.Width.Point;
                double pageH = page.Height.Point;
                double boxWidth = Math.Max(10, pageW - mLeft - mRight);

                if (opts.Header.Enabled && (opts.Header.AddOnFirstPage || !isFirstPage))
                {
                    string headerText = BuildBandText(opts.Header, pageNum);
                    var rect = new XRect(mLeft, mTop, boxWidth, opts.FontSize * 1.5);
                    gfx.DrawString(headerText, font, brush, rect, GetXStringFormat(opts.Header.Alignment, isHeader: true));
                }

                if (opts.Footer.Enabled && (opts.Footer.AddOnFirstPage || !isFirstPage))
                {
                    string footerText = BuildBandText(opts.Footer, pageNum);
                    double boxHeight = opts.FontSize * 1.5;
                    double yTop = Math.Max(0, pageH - mBottom - boxHeight);
                    var rect = new XRect(mLeft, yTop, boxWidth, boxHeight);
                    gfx.DrawString(footerText, font, brush, rect, GetXStringFormat(opts.Footer.Alignment, isHeader: false));
                }
            }

            outDoc.Save(outputPath);
        }

        private static string BuildBandText(HeaderFooterBandOptions band, int physicalPageNumber)
        {
            if (!band.ShowPageNumber)
                return $"{band.TextBefore} {band.TextAfter}".Trim();

            int displayNumber = physicalPageNumber + band.PageOffset;
            string left = band.TextBefore;
            string right = band.TextAfter;

            string separatorBefore = !string.IsNullOrEmpty(left) && !left.EndsWith(" ") ? " " : "";
            string separatorAfter = !string.IsNullOrEmpty(right) && !right.StartsWith(" ") ? " " : "";

            return $"{left}{separatorBefore}{displayNumber}{separatorAfter}{right}".Trim();
        }

        private static XStringFormat GetXStringFormat(string alignment, bool isHeader)
        {
            var fmt = new XStringFormat
            {
                LineAlignment = isHeader ? XLineAlignment.Near : XLineAlignment.Far
            };

            fmt.Alignment = alignment switch
            {
                "Center" => XStringAlignment.Center,
                "Right" => XStringAlignment.Far,
                _ => XStringAlignment.Near
            };

            return fmt;
        }

        /// <summary>
        /// Draws a text watermark across all pages in either Background or Foreground order,
        /// supporting Fill, Stroke (outline), or Fill & Stroke, plus Horizontal/Diagonal/Vertical angles.
        /// </summary>
        public void AddTextWatermark(string inputPath, string outputPath, TextWatermarkOptions opts)
        {
            if (string.IsNullOrWhiteSpace(opts.Text))
                throw new ArgumentException("Please enter the watermark text string.");

            WindowsFontResolver.EnsureRegistered();

            using var sourceDoc = PdfReader.Open(inputPath, PdfDocumentOpenMode.Import);
            using var outDoc = new PdfDocument();

            XFontStyleEx style = opts.FontStyle switch
            {
                "Bold" => XFontStyleEx.Bold,
                "Italic" => XFontStyleEx.Italic,
                "Bold Italic" => XFontStyleEx.BoldItalic,
                _ => XFontStyleEx.Regular
            };

            var fillColor = XColor.FromArgb(140, opts.FontColor.R, opts.FontColor.G, opts.FontColor.B);
            var strokeColor = XColor.FromArgb(180, opts.StrokeColor.R, opts.StrokeColor.G, opts.StrokeColor.B);
            var brush = new XSolidBrush(fillColor);

            var drawMode = opts.IsBackground
                ? XGraphicsPdfPageOptions.Prepend
                : XGraphicsPdfPageOptions.Append;

            for (int i = 0; i < sourceDoc.PageCount; i++)
            {
                var page = outDoc.AddPage(sourceDoc.Pages[i]);
                using var gfx = XGraphics.FromPdfPage(page, drawMode);

                double pageW = page.Width.Point;
                double pageH = page.Height.Point;

                // Auto-fit font size proportionally to page diagonal/width
                double baseTargetWidth = opts.Layout == "Diagonal"
                    ? Math.Sqrt(pageW * pageW + pageH * pageH) * 0.65
                    : pageW * 0.72;

                var testFont = new XFont(opts.FontName, 48.0, style);
                var measured = gfx.MeasureString(opts.Text, testFont);
                double fontSize = measured.Width > 0
                    ? Math.Clamp(48.0 * (baseTargetWidth / measured.Width), 12.0, 140.0)
                    : 48.0;

                var font = new XFont(opts.FontName, fontSize, style);
                var textSize = gfx.MeasureString(opts.Text, font);

                double strokeWidthPts = Math.Max(0.25, fontSize * (opts.StrokeSizePercent / 100.0));
                var pen = new XPen(strokeColor, strokeWidthPts);

                var state = gfx.Save();
                gfx.TranslateTransform(pageW / 2.0, pageH / 2.0);

                double angle = opts.Layout switch
                {
                    "Diagonal" => -Math.Atan2(pageH, pageW) * (180.0 / Math.PI),
                    "Vertical" => -90.0,
                    _ => 0.0
                };
                if (angle != 0.0)
                    gfx.RotateTransform(angle);

                // Draw centered using XGraphicsPath so both Fill and Stroke (outline) are crisp
                var path = new XGraphicsPath();
                var origin = new XPoint(-textSize.Width / 2.0, textSize.Height / 4.0);
                path.AddString(opts.Text, font.FontFamily, style, fontSize, origin, XStringFormats.Default);

                if (opts.DrawType == "Fill")
                {
                    gfx.DrawPath(brush, path);
                }
                else if (opts.DrawType == "Fill & Stroke")
                {
                    gfx.DrawPath(pen, brush, path);
                }
                else // "Stroke"
                {
                    gfx.DrawPath(pen, path);
                }

                gfx.Restore(state);
            }

            outDoc.Save(outputPath);
        }

        /// <summary>
        /// Draws an image watermark across all pages with scaling, rotation, custom positioning,
        /// Background/Foreground layering, and an optional clickable web link.
        /// </summary>
        public void AddImageWatermark(string inputPath, string outputPath, ImageWatermarkOptions opts)
        {
            if (!File.Exists(opts.ImagePath))
                throw new FileNotFoundException("Watermark image file was not found.", opts.ImagePath);

            using var sourceDoc = PdfReader.Open(inputPath, PdfDocumentOpenMode.Import);
            using var outDoc = new PdfDocument();
            using var xImg = XImage.FromFile(opts.ImagePath);

            double baseW = xImg.PixelWidth;
            double baseH = xImg.PixelHeight;
            double drawW = baseW * (opts.WidthPercent / 100.0);
            double drawH = baseH * (opts.HeightPercent / 100.0);

            var drawMode = opts.IsBackground
                ? XGraphicsPdfPageOptions.Prepend
                : XGraphicsPdfPageOptions.Append;

            for (int i = 0; i < sourceDoc.PageCount; i++)
            {
                var page = outDoc.AddPage(sourceDoc.Pages[i]);
                using var gfx = XGraphics.FromPdfPage(page, drawMode);

                double pageW = page.Width.Point;
                double pageH = page.Height.Point;

                double centerX, centerY;
                if (opts.CenterOfPage)
                {
                    centerX = pageW / 2.0;
                    centerY = pageH / 2.0;
                }
                else
                {
                    // Convert bottom-left inch coordinates to PDFsharp top-left coordinate system
                    double leftPts = opts.LeftInches * 72.0;
                    double bottomPts = opts.BottomInches * 72.0;
                    centerX = leftPts + drawW / 2.0;
                    centerY = pageH - bottomPts - drawH / 2.0;
                }

                var state = gfx.Save();
                gfx.TranslateTransform(centerX, centerY);
                if (opts.RotateDegrees != 0.0)
                    gfx.RotateTransform(opts.RotateDegrees);

                var destRect = new XRect(-drawW / 2.0, -drawH / 2.0, drawW, drawH);
                gfx.DrawImage(xImg, destRect);
                gfx.Restore(state);

                if (!string.IsNullOrWhiteSpace(opts.Url))
                {
                    double pdfBottomY = pageH - (centerY + drawH / 2.0);
                    var linkRect = new PdfRectangle(
                        new XPoint(centerX - drawW / 2.0, pdfBottomY),
                        new XPoint(centerX + drawW / 2.0, pdfBottomY + drawH));
                    page.AddWebLink(linkRect, opts.Url.Trim());
                }
            }

            outDoc.Save(outputPath);
        }

        /// <summary>
        /// Reads pixel dimensions (in points) and frame count for an image file.
        /// </summary>
        public ImageToPdfItem InspectImageFile(string filePath)
        {
            using var img = Image.FromFile(filePath);
            int frames = 1;
            try
            {
                if (img.FrameDimensionsList.Length > 0)
                {
                    var dim = new FrameDimension(img.FrameDimensionsList[0]);
                    frames = Math.Max(1, img.GetFrameCount(dim));
                }
            }
            catch { frames = 1; }

            return new ImageToPdfItem
            {
                Title = Path.GetFileNameWithoutExtension(filePath),
                FilePath = filePath,
                WidthPoints = img.Width,
                HeightPoints = img.Height,
                PageCount = frames
            };
        }

        /// <summary>
        /// Converts a list of images into a PDF document with layout grid, margins, captions, DPI sizing, and optional compression.
        /// </summary>
        public void ConvertImagesToPdf(IList<ImageToPdfItem> items, string outputPath, ImagesToPdfOptions opts)
        {
            if (items.Count == 0)
                throw new ArgumentException("Please add at least one image.");

            WindowsFontResolver.EnsureRegistered();

            using var outDoc = new PdfDocument();
            var captionFont = new XFont("Arial", 10, XFontStyleEx.Regular);
            var captionBrush = XBrushes.Black;

            if (opts.UseImageSizeWithDpi)
            {
                // Each image determines its own page size in points = (pixels / DPI) * 72
                double dpi = Math.Max(1, opts.ImageSizeDpi);
                foreach (var item in items)
                {
                    using var xImg = LoadXImage(item.FilePath, opts.UseImageCompress, opts.CompressDpi,
                        item.WidthPoints / dpi, item.HeightPoints / dpi);

                    double pageW = (item.WidthPoints / dpi) * 72.0;
                    double pageH = (item.HeightPoints / dpi) * 72.0;
                    double captionH = opts.ShowTitle ? 18.0 : 0.0;

                    var page = outDoc.AddPage();
                    page.Width = XUnit.FromPoint(pageW);
                    page.Height = XUnit.FromPoint(pageH + captionH);

                    using var gfx = XGraphics.FromPdfPage(page);
                    gfx.DrawImage(xImg, 0, 0, pageW, pageH);

                    if (opts.ShowTitle && !string.IsNullOrWhiteSpace(item.Title))
                    {
                        var titleRect = new XRect(4, pageH, pageW - 8, captionH);
                        gfx.DrawString(item.Title, captionFont, captionBrush, titleRect, XStringFormats.Center);
                    }
                }
            }
            else
            {
                double pageW = opts.PageWidthInches * 72.0;
                double pageH = opts.PageHeightInches * 72.0;
                double mLeft = opts.MarginLeftInches * 72.0;
                double mRight = opts.MarginRightInches * 72.0;
                double mTop = opts.MarginTopInches * 72.0;
                double mBottom = opts.MarginBottomInches * 72.0;
                double gapX = opts.SpaceHorizontalInches * 72.0;
                double gapY = opts.SpaceVerticalInches * 72.0;

                double usableW = pageW - mLeft - mRight - gapX * (opts.Columns - 1);
                double usableH = pageH - mTop - mBottom - gapY * (opts.Rows - 1);

                if (usableW <= 0 || usableH <= 0)
                    throw new ArgumentException("Margins and spacing exceed the selected PDF page dimensions.");

                double cellW = usableW / opts.Columns;
                double cellH = usableH / opts.Rows;
                int perPage = opts.Rows * opts.Columns;

                for (int start = 0; start < items.Count; start += perPage)
                {
                    var page = outDoc.AddPage();
                    page.Width = XUnit.FromPoint(pageW);
                    page.Height = XUnit.FromPoint(pageH);
                    using var gfx = XGraphics.FromPdfPage(page);

                    for (int slot = 0; slot < perPage && (start + slot) < items.Count; slot++)
                    {
                        var item = items[start + slot];
                        int row = slot / opts.Columns;
                        int col = slot % opts.Columns;

                        double cellX = mLeft + col * (cellW + gapX);
                        double cellY = mTop + row * (cellH + gapY);
                        double captionH = opts.ShowTitle ? 16.0 : 0.0;
                        double imgBoxH = Math.Max(10, cellH - captionH);

                        using var xImg = LoadXImage(item.FilePath, opts.UseImageCompress, opts.CompressDpi,
                            cellW / 72.0, imgBoxH / 72.0);

                        double scale = Math.Min(cellW / xImg.PointWidth, imgBoxH / xImg.PointHeight);
                        double drawW = xImg.PointWidth * scale;
                        double drawH = xImg.PointHeight * scale;
                        double drawX = cellX + (cellW - drawW) / 2.0;
                        double drawY = cellY + (imgBoxH - drawH) / 2.0;

                        gfx.DrawImage(xImg, drawX, drawY, drawW, drawH);

                        if (opts.ShowTitle && !string.IsNullOrWhiteSpace(item.Title))
                        {
                            var titleRect = new XRect(cellX, drawY + drawH + 2, cellW, captionH);
                            gfx.DrawString(item.Title, captionFont, captionBrush, titleRect, XStringFormats.TopCenter);
                        }
                    }
                }
            }

            outDoc.Save(outputPath);
        }

        private static XImage LoadXImage(string filePath, bool compress, int targetDpi, double targetWidthInches, double targetHeightInches)
        {
            if (!compress)
                return XImage.FromFile(filePath);

            using var src = Image.FromFile(filePath);
            int maxW = Math.Max(16, (int)Math.Round(targetWidthInches * targetDpi));
            int maxH = Math.Max(16, (int)Math.Round(targetHeightInches * targetDpi));

            double scale = Math.Min(1.0, Math.Min((double)maxW / src.Width, (double)maxH / src.Height));
            int newW = Math.Max(1, (int)Math.Round(src.Width * scale));
            int newH = Math.Max(1, (int)Math.Round(src.Height * scale));

            using var bmp = new Bitmap(newW, newH);
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(src, 0, 0, newW, newH);
            }

            var ms = new MemoryStream();
            var jpegEncoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
            using var encParams = new EncoderParameters(1);
            encParams.Param[0] = new EncoderParameter(Encoder.Quality, 80L);
            bmp.Save(ms, jpegEncoder, encParams);
            ms.Position = 0;

            return XImage.FromStream(ms);
        }

        /// <summary>
        /// Renders PDF pages to raster/vector image files using Docnet.Core (PDFium).
        /// </summary>
        public List<string> ConvertPdfToImages(string inputPath, string baseSavePath, PdfToImagesOptions opts)
        {
            if (opts.Dpi < 6 || opts.Dpi > 600)
                throw new ArgumentOutOfRangeException(nameof(opts.Dpi), "Image Resolution must be between 6 and 600 DPI.");

            var createdFiles = new List<string>();
            string dir = Path.GetDirectoryName(baseSavePath) ?? string.Empty;
            string baseName = Path.GetFileNameWithoutExtension(baseSavePath);
            string ext = opts.ImageType.ToLowerInvariant();

            // Docnet PageDimensions scales relative to 72 DPI base
            double scaleFactor = opts.Dpi / 72.0;
            using var docReader = DocLib.Instance.GetDocReader(
                inputPath,
                new PageDimensions(scaleFactor));

            int totalPages = docReader.GetPageCount();
            if (opts.FromPage < 1 || opts.ToPage > totalPages || opts.FromPage > opts.ToPage)
                throw new ArgumentOutOfRangeException($"Page range must be between 1 and {totalPages}.");

            var renderedBitmaps = new List<(int PageNum, Bitmap Bmp)>();

            for (int p = opts.FromPage; p <= opts.ToPage; p++)
            {
                using var pageReader = docReader.GetPageReader(p - 1);
                int width = pageReader.GetPageWidth();
                int height = pageReader.GetPageHeight();
                byte[] rawBgra = pageReader.GetImage();

                var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                var rect = new Rectangle(0, 0, width, height);
                var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
                Marshal.Copy(rawBgra, 0, bmpData.Scan0, rawBgra.Length);
                bmp.UnlockBits(bmpData);

                // Composite onto white background so transparent PDFs don't turn black in JPG/BMP
                var whiteBgBmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(whiteBgBmp))
                {
                    g.Clear(Color.White);
                    g.DrawImageUnscaled(bmp, 0, 0);
                }
                bmp.Dispose();

                Bitmap finalBmp = ApplyColorDepth(whiteBgBmp, opts.ColorBits);
                if (!ReferenceEquals(finalBmp, whiteBgBmp))
                    whiteBgBmp.Dispose();

                finalBmp.SetResolution(opts.Dpi, opts.Dpi);
                renderedBitmaps.Add((p, finalBmp));
            }

            try
            {
                if (opts.ImageType == "TIF" && opts.SingleMultiPageTiff && renderedBitmaps.Count > 0)
                {
                    string outFile = Path.Combine(dir, $"{baseName}.tif");
                    SaveMultiPageTiff(renderedBitmaps.Select(x => x.Bmp).ToList(), outFile);
                    createdFiles.Add(outFile);
                }
                else
                {
                    string padFormat = new string('0', Math.Max(1, opts.PageNumberDigits));
                    foreach (var (pageNum, bmp) in renderedBitmaps)
                    {
                        string suffix = renderedBitmaps.Count == 1 && opts.FromPage == opts.ToPage
                            ? ""
                            : $"_{pageNum.ToString(padFormat)}";
                        string outFile = Path.Combine(dir, $"{baseName}{suffix}.{ext}");
                        SaveSingleBitmap(bmp, outFile, opts.ImageType, opts.JpgQuality);
                        createdFiles.Add(outFile);
                    }
                }
            }
            finally
            {
                foreach (var (_, bmp) in renderedBitmaps)
                    bmp.Dispose();
            }

            return createdFiles;
        }

        private static Bitmap ApplyColorDepth(Bitmap source, int colorBits)
        {
            if (colorBits == 24) return source;

            var result = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
            for (int y = 0; y < source.Height; y++)
            {
                for (int x = 0; x < source.Width; x++)
                {
                    Color c = source.GetPixel(x, y);
                    int lum = (int)(c.R * 0.299 + c.G * 0.587 + c.B * 0.114);
                    if (colorBits == 1)
                        lum = lum >= 128 ? 255 : 0;
                    result.SetPixel(x, y, Color.FromArgb(lum, lum, lum));
                }
            }
            return result;
        }

        private static void SaveSingleBitmap(Bitmap bmp, string filePath, string imageType, int jpgQuality)
        {
            switch (imageType)
            {
                case "JPG":
                    var jpegEncoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                    using (var encParams = new EncoderParameters(1))
                    {
                        encParams.Param[0] = new EncoderParameter(Encoder.Quality, Math.Clamp(jpgQuality, 1, 100));
                        bmp.Save(filePath, jpegEncoder, encParams);
                    }
                    break;
                case "BMP":
                    bmp.Save(filePath, ImageFormat.Bmp);
                    break;
                case "GIF":
                    bmp.Save(filePath, ImageFormat.Gif);
                    break;
                case "TIF":
                    bmp.Save(filePath, ImageFormat.Tiff);
                    break;
                case "WMF":
                    bmp.Save(filePath, ImageFormat.Wmf);
                    break;
                case "EMF":
                    bmp.Save(filePath, ImageFormat.Emf);
                    break;
                default: // PNG (and EPS fallback raster)
                    bmp.Save(filePath, ImageFormat.Png);
                    break;
            }
        }

        private static void SaveMultiPageTiff(IList<Bitmap> pages, string outputPath)
        {
            var tiffEncoder = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Tiff.Guid);
            using var encParams = new EncoderParameters(1);

            encParams.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
            pages[0].Save(outputPath, tiffEncoder, encParams);

            encParams.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
            for (int i = 1; i < pages.Count; i++)
            {
                pages[0].SaveAdd(pages[i], encParams);
            }

            encParams.Param[0] = new EncoderParameter(Encoder.SaveFlag, (long)EncoderValue.Flush);
            pages[0].SaveAdd(encParams);
        }

        // 
    }
}