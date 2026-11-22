using System;
using System.Collections.Generic;
using System.IO;
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
    }
}