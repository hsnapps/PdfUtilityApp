using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace PdfUtilityApp.Services
{
    public class PdfService
    {
        /// <summary>
        /// Reads the total page count of a PDF file without loading the full document for modification.
        /// </summary>
        public int GetPageCount(string filePath)
        {
            using var doc = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
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

                // Add bookmark pointing to the first page of this imported PDF
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

            // Build lookup of source page object -> 1-based page number for bookmark mapping
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
    }
}