// using System;
// using System.Drawing;
// using System.Windows.Forms;
using PdfUtilityApp.Forms;
using PdfUtilityApp.Services;

namespace PdfUtilityApp
{
    public class MainForm : Form
    {
        private readonly PdfService _pdfService = new();
        public bool IsLanguageSwitch { get; private set; } = false;

        public MainForm()
        {
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            // Window settings
            this.Text = "PDF Utility Tool";
            this.Size = new Size(780, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Top instruction label
            var lblHeader = new Label
            {
                Text = "Select a PDF Tool you wish to use:",
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Location = new Point(25, 25)
            };
            this.Controls.Add(lblHeader);

            // 3-column x 5-row grid for the 12 tool buttons
            var grid = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 5,
                Location = new Point(25, 65),
                Size = new Size(715, 300)
            };

            for (int c = 0; c < 3; c++)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            for (int r = 0; r < 5; r++)
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));

            // Button definitions mapped by (Row, Column)
            var tools = new (string Title, int Row, int Col, Action OnClick)[]
            {
                ("1. Merge PDF Files", 0, 0, () => { using var dlg = new MergePdfForm(); dlg.ShowDialog(this); }),
                ("2. Split or Reorder Pages", 1, 0, OpenSplitOrReorder),
                ("3. Encrypt and Decrypt Options", 2, 0, OpenEncryptDecrypt),
                ("4. Rotate or Crop PDF Pages", 3, 0, OpenRotateOrCrop),
                ("5. Reformat Multiple Pages", 4, 0, OpenReformatPages),

                ("6. Add Header and Footer", 0, 1, OpenHeaderFooter),
                ("7. Add Watermark by Text", 1, 1, OpenTextWatermark),
                ("8. Add Watermark by Image", 2, 1, OpenImageWatermark),
                ("9. Convert Images to PDF", 3, 1, () => { using var dlg = new ImagesToPdfForm(); dlg.ShowDialog(this); }),
                ("10. Convert PDF to Images", 4, 1, OpenPdfToImages),

                ("11. PDF Form Field Operations", 0, 2, OpenFormFieldOperations),
                ("12. Add PDF Information", 1, 2, OpenAddPdfInfo)
            };

            foreach (var tool in tools)
            {
                var btn = new Button
                {
                    Text = tool.Title,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(8, 10, 8, 10),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 0, 0, 0),
                    Font = new Font("Segoe UI", 9.5F),
                    UseVisualStyleBackColor = true
                };
                btn.Click += (s, e) => tool.OnClick();
                grid.Controls.Add(btn, tool.Col, tool.Row);
            }

            this.Controls.Add(grid);

            // Bottom Buttons: Exit + Language Toggle ("عربي" / "English")
            int btnWidth = 105;
            int btnHeight = 32;
            int gap = 16;
            int startX = (this.ClientSize.Width - (btnWidth * 2 + gap)) / 2;

            var btnExit = new Button
            {
                Text = "Exit",
                Size = new Size(btnWidth, btnHeight),
                Location = new Point(startX, 390),
                Font = new Font("Segoe UI", 9.5F)
            };
            btnExit.Click += (s, e) => this.Close();
            this.Controls.Add(btnExit);

            var btnLanguage = new Button
            {
                Name = "btnLanguageToggle",
                Text = LocalizationManager.IsArabic ? "English" : "عربي",
                Size = new Size(btnWidth, btnHeight),
                Location = new Point(startX + btnWidth + gap, 390),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };
            btnLanguage.Click += (s, e) =>
            {
                LocalizationManager.SetLanguage(LocalizationManager.IsArabic ? "en" : "ar");
                this.IsLanguageSwitch = true;
                this.Close();
            };
            this.Controls.Add(btnLanguage);

            // Apply active language (LTR English or RTL Arabic)
            LocalizationManager.Apply(this);
        }

        private void OpenTool(string toolName)
        {
            MessageBox.Show($"{toolName} popup will open here.", "Coming Soon");
        }

        private void OpenSplitOrReorder()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select a PDF File",
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = false
            };

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    using var dlg = new SplitReorderForm(ofd.FileName);
                    dlg.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open PDF:\n{ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenEncryptDecrypt()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select a PDF File",
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = false
            };

            if (ofd.ShowDialog(this) != DialogResult.OK) return;

            string? password = null;
            int pageCount = 0;

            try
            {
                pageCount = _pdfService.GetPageCount(ofd.FileName);
            }
            catch (PdfSharp.Pdf.IO.PdfReaderException)
            {
                // Prompt for password if the file is password-protected
                password = PromptForPassword();
                if (password == null) return;

                try
                {
                    pageCount = _pdfService.GetPageCount(ofd.FileName, password);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open PDF with the provided password:\n{ex.Message}",
                        "Invalid Password", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open PDF:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using var dlg = new EncryptDecryptForm(ofd.FileName, pageCount, password);
            dlg.ShowDialog(this);
        }

        private string? PromptForPassword()
        {
            using var prompt = new Form
            {
                Width = 340,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "Password Required",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lbl = new Label { Left = 15, Top = 15, Text = "This PDF is encrypted. Enter password:", AutoSize = true };
            var txt = new TextBox { Left = 15, Top = 42, Width = 290, UseSystemPasswordChar = true };
            var btnOk = new Button { Text = "OK", Left = 145, Width = 75, Top = 75, DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Cancel", Left = 230, Width = 75, Top = 75, DialogResult = DialogResult.Cancel };

            prompt.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
            prompt.AcceptButton = btnOk;
            prompt.CancelButton = btnCancel;

            return prompt.ShowDialog(this) == DialogResult.OK ? txt.Text : null;
        }

        private void OpenRotateOrCrop()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select a PDF File",
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = false
            };

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    using var dlg = new RotateCropForm(ofd.FileName);
                    dlg.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open PDF:\n{ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenHeaderFooter()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select a PDF File",
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = false
            };

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    using var dlg = new HeaderFooterForm(ofd.FileName);
                    dlg.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open PDF:\n{ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenTextWatermark()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select a PDF File",
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = false
            };

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    using var dlg = new TextWatermarkForm(ofd.FileName);
                    dlg.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open PDF:\n{ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenImageWatermark()
        {
            using var pdfDialog = new OpenFileDialog
            {
                Title = "Select a PDF File",
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = false
            };

            if (pdfDialog.ShowDialog(this) != DialogResult.OK) return;

            using var imgDialog = new OpenFileDialog
            {
                Title = "Select Watermark Image",
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff",
                Multiselect = false
            };

            if (imgDialog.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                using var dlg = new ImageWatermarkForm(pdfDialog.FileName, imgDialog.FileName);
                dlg.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open files:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenPdfToImages()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select a PDF File",
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = false
            };

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    using var dlg = new PdfToImagesForm(ofd.FileName);
                    dlg.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open PDF:\n{ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenFormFieldOperations()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select a PDF File",
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = false
            };

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    using var dlg = new FormFieldOperationsForm(ofd.FileName);
                    dlg.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open PDF:\n{ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenAddPdfInfo()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select a PDF File",
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = false
            };

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    using var dlg = new AddPdfInfoForm(ofd.FileName);
                    dlg.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open PDF:\n{ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void OpenReformatPages()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select a PDF File",
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = false
            };

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    using var dlg = new ReformatPagesForm(ofd.FileName);
                    dlg.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open PDF:\n{ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 
    }
}