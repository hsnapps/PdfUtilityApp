using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PdfUtilityApp.Services;

namespace PdfUtilityApp.Forms
{
    public class AddPdfInfoForm : Form
    {
        private readonly PdfService _pdfService = new();
        private readonly string _filePath;
        private readonly int _totalPages;
        private readonly PdfDocumentMetadataInfo _initialMeta;

        private TextBox _txtTitle = null!;
        private TextBox _txtAuthor = null!;
        private TextBox _txtSubject = null!;
        private TextBox _txtKeywords = null!;
        private TextBox _txtCreated = null!;
        private TextBox _txtCreator = null!;
        private TextBox _txtProducer = null!;
        private ComboBox _cmbPdfVersion = null!;

        private Button _btnSaveAs = null!;
        private Button _btnCancel = null!;

        public AddPdfInfoForm(string filePath)
        {
            _filePath = filePath;
            _totalPages = _pdfService.GetPageCount(filePath);
            _initialMeta = _pdfService.GetPdfMetadata(filePath);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Add PDF Information";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Header
            this.Controls.Add(new Label
            {
                Text = $"There are {_totalPages} pages in {Path.GetFileName(_filePath)}",
                Location = new Point(14, 14),
                AutoSize = true
            });

            int labelX = 32;
            int inputX = 165;
            int inputW = 240;
            int y = 44;
            int rowStep = 31;

            // Title
            this.Controls.Add(new Label { Text = "Title:", Location = new Point(labelX, y + 3), AutoSize = true });
            _txtTitle = new TextBox { Text = _initialMeta.Title, Location = new Point(inputX, y), Size = new Size(inputW, 23) };
            this.Controls.Add(_txtTitle);
            y += rowStep;

            // Author
            this.Controls.Add(new Label { Text = "Author:", Location = new Point(labelX, y + 3), AutoSize = true });
            _txtAuthor = new TextBox { Text = _initialMeta.Author, Location = new Point(inputX, y), Size = new Size(inputW, 23) };
            this.Controls.Add(_txtAuthor);
            y += rowStep;

            // Subject
            this.Controls.Add(new Label { Text = "Subject:", Location = new Point(labelX, y + 3), AutoSize = true });
            _txtSubject = new TextBox { Text = _initialMeta.Subject, Location = new Point(inputX, y), Size = new Size(inputW, 23) };
            this.Controls.Add(_txtSubject);
            y += rowStep;

            // Keywords
            this.Controls.Add(new Label { Text = "Keywords:", Location = new Point(labelX, y + 3), AutoSize = true });
            _txtKeywords = new TextBox { Text = _initialMeta.Keywords, Location = new Point(inputX, y), Size = new Size(inputW, 23) };
            this.Controls.Add(_txtKeywords);
            y += rowStep;

            // Created
            this.Controls.Add(new Label { Text = "Created", Location = new Point(labelX, y + 3), AutoSize = true });
            _txtCreated = new TextBox { Text = _initialMeta.CreatedRaw, Location = new Point(inputX, y), Size = new Size(inputW, 23) };
            this.Controls.Add(_txtCreated);
            y += rowStep;

            // Creator
            this.Controls.Add(new Label { Text = "Creator:", Location = new Point(labelX, y + 3), AutoSize = true });
            _txtCreator = new TextBox { Text = _initialMeta.Creator, Location = new Point(inputX, y), Size = new Size(inputW, 23) };
            this.Controls.Add(_txtCreator);
            y += rowStep;

            // Producer (Read-only)
            this.Controls.Add(new Label { Text = "Producer:", Location = new Point(labelX, y + 3), AutoSize = true });
            _txtProducer = new TextBox
            {
                Text = _initialMeta.Producer,
                Location = new Point(inputX, y),
                Size = new Size(inputW, 23),
                ReadOnly = true
            };
            this.Controls.Add(_txtProducer);
            y += rowStep;

            // PDF Version (Read-only display combo matching mockup)
            this.Controls.Add(new Label { Text = "PDF Version:", Location = new Point(labelX, y + 3), AutoSize = true });
            _cmbPdfVersion = new ComboBox
            {
                Location = new Point(inputX, y),
                Size = new Size(inputW, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Enabled = false
            };
            _cmbPdfVersion.Items.Add(_initialMeta.PdfVersion);
            _cmbPdfVersion.SelectedIndex = 0;
            this.Controls.Add(_cmbPdfVersion);
            y += 42;

            // Bottom Buttons
            _btnSaveAs = new Button
            {
                Text = "Save As",
                Location = new Point(92, y),
                Size = new Size(80, 26)
            };
            _btnSaveAs.Click += BtnSaveAs_Click;
            this.Controls.Add(_btnSaveAs);

            _btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(192, y),
                Size = new Size(80, 26)
            };
            _btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(_btnCancel);

            this.ClientSize = new Size(440, y + 42);
        }

        private void BtnSaveAs_Click(object? sender, EventArgs e)
        {
            string defaultName = Path.GetFileNameWithoutExtension(_filePath) + "_info.pdf";
            using var sfd = new SaveFileDialog
            {
                Title = "Save PDF As",
                Filter = "PDF File (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                FileName = defaultName
            };

            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                var updated = new PdfDocumentMetadataInfo
                {
                    Title = _txtTitle.Text,
                    Author = _txtAuthor.Text,
                    Subject = _txtSubject.Text,
                    Keywords = _txtKeywords.Text,
                    CreatedRaw = _txtCreated.Text,
                    Creator = _txtCreator.Text
                };

                _pdfService.UpdatePdfMetadata(_filePath, sfd.FileName, updated);
                this.Cursor = Cursors.Default;

                MessageBox.Show("PDF information updated successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show($"Error saving PDF information:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}