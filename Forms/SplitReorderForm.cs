using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PdfUtilityApp.Services;

namespace PdfUtilityApp.Forms
{
    public class SplitReorderForm : Form
    {
        private readonly PdfService _pdfService = new();
        private readonly string _filePath;
        private readonly int _totalPages;

        // UI Controls
        private Label _lblHeader = null!;
        private RadioButton _rbSplit = null!;
        private TextBox _txtFrom = null!;
        private TextBox _txtTo = null!;

        private RadioButton _rbReorder = null!;
        private TextBox _txtReorder = null!;

        private RadioButton _rbDelete = null!;
        private TextBox _txtDelete = null!;

        private CheckBox _chkKeepBookmarks = null!;
        private CheckBox _chkSaveOtherPages = null!;
        private CheckBox _chkSeparateFiles = null!;
        private TextBox _txtPagesPerFile = null!;
        private CheckBox _chkOpenExtracted = null!;

        private Button _btnSaveAs = null!;
        private Button _btnClose = null!;

        public SplitReorderForm(string filePath)
        {
            _filePath = filePath;
            _totalPages = _pdfService.GetPageCount(filePath);
            InitializeComponent();
            UpdateControlStates();
        }

        private void InitializeComponent()
        {
            this.Text = "Split or Reorder Pages";
            this.Size = new Size(465, 390);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Header label: "There are X pages in filename.pdf"
            _lblHeader = new Label
            {
                Text = $"There are {_totalPages} pages in {Path.GetFileName(_filePath)}",
                Location = new Point(22, 18),
                AutoSize = true
            };
            this.Controls.Add(_lblHeader);

            // 1. Split Pages From [1] To [1]
            _rbSplit = new RadioButton
            {
                Text = "Split Pages From",
                Location = new Point(22, 52),
                AutoSize = true,
                Checked = true
            };
            _rbSplit.CheckedChanged += (s, e) => UpdateControlStates();
            this.Controls.Add(_rbSplit);

            _txtFrom = new TextBox
            {
                Text = "1",
                Location = new Point(135, 50),
                Size = new Size(55, 23)
            };
            this.Controls.Add(_txtFrom);

            var lblTo = new Label
            {
                Text = "To",
                Location = new Point(200, 53),
                AutoSize = true
            };
            this.Controls.Add(lblTo);

            _txtTo = new TextBox
            {
                Text = _totalPages.ToString(),
                Location = new Point(225, 50),
                Size = new Size(55, 23)
            };
            this.Controls.Add(_txtTo);

            // 2. Reorder Pages
            _rbReorder = new RadioButton
            {
                Text = "Reorder Pages",
                Location = new Point(22, 90),
                AutoSize = true
            };
            _rbReorder.CheckedChanged += (s, e) => UpdateControlStates();
            this.Controls.Add(_rbReorder);

            _txtReorder = new TextBox
            {
                Text = "1",
                Location = new Point(135, 88),
                Size = new Size(290, 23)
            };
            this.Controls.Add(_txtReorder);

            var lblReorderHint = new Label
            {
                Text = "Pages Example: 1, 3-5, even, odd, reverse, last",
                Location = new Point(135, 116),
                AutoSize = true
            };
            this.Controls.Add(lblReorderHint);

            // 3. Delete Pages
            _rbDelete = new RadioButton
            {
                Text = "Delete Pages",
                Location = new Point(22, 154),
                AutoSize = true
            };
            _rbDelete.CheckedChanged += (s, e) => UpdateControlStates();
            this.Controls.Add(_rbDelete);

            _txtDelete = new TextBox
            {
                Text = "1",
                Location = new Point(135, 152),
                Size = new Size(290, 23)
            };
            this.Controls.Add(_txtDelete);

            var lblDeleteHint = new Label
            {
                Text = "Pages Example: 1, 3-5, even, odd, last",
                Location = new Point(135, 180),
                AutoSize = true
            };
            this.Controls.Add(lblDeleteHint);

            // Checkboxes Row 1
            _chkKeepBookmarks = new CheckBox
            {
                Text = "Keep Bookmarks",
                Location = new Point(22, 218),
                AutoSize = true
            };
            this.Controls.Add(_chkKeepBookmarks);

            _chkSaveOtherPages = new CheckBox
            {
                Text = "Save the other pages as a PDF file",
                Location = new Point(225, 218),
                AutoSize = true
            };
            this.Controls.Add(_chkSaveOtherPages);

            // Checkboxes Row 2
            _chkSeparateFiles = new CheckBox
            {
                Text = "Extract Pages As Separate Files",
                Location = new Point(22, 250),
                AutoSize = true
            };
            _chkSeparateFiles.CheckedChanged += (s, e) => UpdateControlStates();
            this.Controls.Add(_chkSeparateFiles);

            var lblPages = new Label
            {
                Text = "Pages",
                Location = new Point(225, 252),
                AutoSize = true
            };
            this.Controls.Add(lblPages);

            _txtPagesPerFile = new TextBox
            {
                Text = "1",
                Location = new Point(290, 249),
                Size = new Size(52, 23)
            };
            this.Controls.Add(_txtPagesPerFile);

            // Checkboxes Row 3
            _chkOpenExtracted = new CheckBox
            {
                Text = "Open the Extracted PDF",
                Location = new Point(22, 282),
                AutoSize = true,
                Checked = true
            };
            this.Controls.Add(_chkOpenExtracted);

            // Bottom Action Buttons
            _btnSaveAs = new Button
            {
                Text = "Save As ...",
                Location = new Point(92, 315),
                Size = new Size(80, 26)
            };
            _btnSaveAs.Click += BtnSaveAs_Click;
            this.Controls.Add(_btnSaveAs);

            _btnClose = new Button
            {
                Text = "Close",
                Location = new Point(188, 315),
                Size = new Size(80, 26)
            };
            _btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(_btnClose);
        }

        private void UpdateControlStates()
        {
            _txtFrom.Enabled = _rbSplit.Checked;
            _txtTo.Enabled = _rbSplit.Checked;
            _txtReorder.Enabled = _rbReorder.Checked;
            _txtDelete.Enabled = _rbDelete.Checked;

            _txtPagesPerFile.Enabled = _chkSeparateFiles.Checked;
            // Saving "other pages" makes sense for Split/Delete/Reorder subsets
            _chkSaveOtherPages.Enabled = !_chkSeparateFiles.Checked;
            if (_chkSeparateFiles.Checked)
                _chkSaveOtherPages.Checked = false;
        }

        private List<int> GetTargetPages()
        {
            if (_rbSplit.Checked)
            {
                if (!int.TryParse(_txtFrom.Text.Trim(), out int fromPage) ||
                    !int.TryParse(_txtTo.Text.Trim(), out int toPage))
                {
                    throw new ArgumentException("Please enter valid page numbers for From and To.");
                }

                if (fromPage < 1 || toPage > _totalPages || fromPage > toPage)
                {
                    throw new ArgumentOutOfRangeException(
                        $"Please enter a valid range between 1 and {_totalPages} (From <= To).");
                }

                var pages = new List<int>();
                for (int p = fromPage; p <= toPage; p++)
                    pages.Add(p);
                return pages;
            }
            else if (_rbReorder.Checked)
            {
                return _pdfService.ParsePageExpression(_txtReorder.Text, _totalPages, allowReverse: true);
            }
            else // Delete Pages
            {
                var pagesToDelete = new HashSet<int>(
                    _pdfService.ParsePageExpression(_txtDelete.Text, _totalPages, allowReverse: false));

                var remaining = new List<int>();
                for (int p = 1; p <= _totalPages; p++)
                {
                    if (!pagesToDelete.Contains(p))
                        remaining.Add(p);
                }

                if (remaining.Count == 0)
                    throw new InvalidOperationException("You cannot delete all pages from the PDF.");

                return remaining;
            }
        }

        private void BtnSaveAs_Click(object? sender, EventArgs e)
        {
            try
            {
                var targetPages = GetTargetPages();

                int? pagesPerFile = null;
                if (_chkSeparateFiles.Checked)
                {
                    if (!int.TryParse(_txtPagesPerFile.Text.Trim(), out int chunk) || chunk < 1)
                        throw new ArgumentException("Pages per separate file must be at least 1.");
                    pagesPerFile = chunk;
                }

                string defaultName = Path.GetFileNameWithoutExtension(_filePath) + "_modified.pdf";
                using var sfd = new SaveFileDialog
                {
                    Title = "Save PDF As",
                    Filter = "PDF File (*.pdf)|*.pdf",
                    DefaultExt = "pdf",
                    FileName = defaultName
                };

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                this.Cursor = Cursors.WaitCursor;

                var createdFiles = _pdfService.ExtractPagesToPdf(
                    _filePath,
                    sfd.FileName,
                    targetPages,
                    _chkKeepBookmarks.Checked,
                    pagesPerFile);

                // Save unselected ("other") pages if requested
                if (_chkSaveOtherPages.Checked && !_chkSeparateFiles.Checked)
                {
                    var selectedSet = new HashSet<int>(targetPages);
                    var otherPages = Enumerable.Range(1, _totalPages)
                                               .Where(p => !selectedSet.Contains(p))
                                               .ToList();

                    if (otherPages.Count > 0)
                    {
                        string dir = Path.GetDirectoryName(sfd.FileName) ?? string.Empty;
                        string baseName = Path.GetFileNameWithoutExtension(sfd.FileName);
                        string otherPath = Path.Combine(dir, $"{baseName}_other.pdf");

                        _pdfService.ExtractPagesToPdf(
                            _filePath,
                            otherPath,
                            otherPages,
                            _chkKeepBookmarks.Checked,
                            null);
                    }
                }

                this.Cursor = Cursors.Default;

                if (_chkOpenExtracted.Checked && createdFiles.Count > 0 && File.Exists(createdFiles[0]))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = createdFiles[0],
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show("Operation completed successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}