using PdfUtilityApp.Services;

namespace PdfUtilityApp.Forms
{
    public class PdfToImagesForm : Form
    {
        private readonly PdfService _pdfService = new();
        private readonly string _filePath;
        private readonly int _totalPages;

        private TextBox _txtFromPage = null!;
        private TextBox _txtToPage = null!;
        private TextBox _txtResolution = null!;
        private ComboBox _cmbColorBits = null!;
        private ComboBox _cmbPageDigits = null!;

        private RadioButton _rbPng = null!;
        private RadioButton _rbEps = null!;
        private RadioButton _rbJpg = null!;
        private RadioButton _rbTif = null!;

        private RadioButton _rbGif = null!;
        private RadioButton _rbWmf = null!;
        private TextBox _txtJpgQuality = null!;
        private CheckBox _chkSingleMultiPageTiff = null!;

        private RadioButton _rbBmp = null!;
        private RadioButton _rbEmf = null!;

        private Button _btnSaveAsImage = null!;
        private Button _btnClose = null!;

        public PdfToImagesForm(string filePath)
        {
            _filePath = filePath;
            _totalPages = _pdfService.GetPageCount(filePath);
            InitializeComponent();
            UpdateControlStates();
        }

        private void InitializeComponent()
        {
            this.Text = "Convert PDF to Images";
            this.Size = new Size(445, 345);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Header
            this.Controls.Add(new Label
            {
                Text = $"There are {_totalPages} pages in {Path.GetFileName(_filePath)}",
                Location = new Point(12, 12),
                AutoSize = true
            });

            // Select Page from ... To ...
            this.Controls.Add(new Label { Text = "Select Page from", Location = new Point(22, 42), AutoSize = true });
            _txtFromPage = new TextBox { Text = "1", Location = new Point(195, 39), Size = new Size(48, 23) };
            _txtFromPage.TextChanged += (s, e) => UpdateControlStates();
            this.Controls.Add(_txtFromPage);

            this.Controls.Add(new Label { Text = "To", Location = new Point(256, 42), AutoSize = true });
            _txtToPage = new TextBox { Text = _totalPages.ToString(), Location = new Point(288, 39), Size = new Size(48, 23) };
            _txtToPage.TextChanged += (s, e) => UpdateControlStates();
            this.Controls.Add(_txtToPage);

            // Image Resolution
            this.Controls.Add(new Label { Text = "Image Resolution (6-600 dpi):", Location = new Point(22, 74), AutoSize = true });
            _txtResolution = new TextBox { Text = "144", Location = new Point(195, 71), Size = new Size(48, 23) };
            this.Controls.Add(_txtResolution);

            // Color Bits & Page Number Digits
            this.Controls.Add(new Label { Text = "Color Bits", Location = new Point(22, 106), AutoSize = true });
            _cmbColorBits = new ComboBox
            {
                Location = new Point(195, 103),
                Size = new Size(52, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbColorBits.Items.AddRange(new object[] { "24", "8", "1" });
            _cmbColorBits.SelectedIndex = 0;
            this.Controls.Add(_cmbColorBits);

            this.Controls.Add(new Label { Text = "Page Number Digits:", Location = new Point(265, 106), AutoSize = true });
            _cmbPageDigits = new ComboBox
            {
                Location = new Point(392, 103),
                Size = new Size(52, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbPageDigits.Items.AddRange(new object[] { "1", "2", "3", "4", "5", "6" });
            _cmbPageDigits.SelectedIndex = 0;
            this.Controls.Add(_cmbPageDigits);

            // Image Type GroupBox
            var grpImageType = new GroupBox
            {
                Text = "Image Type",
                Location = new Point(22, 134),
                Size = new Size(422, 134)
            };

            // Column 1
            _rbPng = new RadioButton { Text = "PNG", Location = new Point(12, 22), AutoSize = true, Checked = true };
            _rbEps = new RadioButton { Text = "EPS", Location = new Point(12, 50), AutoSize = true };
            _rbJpg = new RadioButton { Text = "JPG", Location = new Point(12, 78), AutoSize = true };
            _rbTif = new RadioButton { Text = "TIF", Location = new Point(12, 106), AutoSize = true };

            // Column 2
            _rbGif = new RadioButton { Text = "GIF", Location = new Point(142, 22), AutoSize = true };
            _rbWmf = new RadioButton { Text = "WMF", Location = new Point(142, 50), AutoSize = true };

            grpImageType.Controls.Add(new Label { Text = "JPG Quality", Location = new Point(142, 80), AutoSize = true });
            _txtJpgQuality = new TextBox { Text = "100", Location = new Point(248, 77), Size = new Size(46, 23) };
            grpImageType.Controls.Add(_txtJpgQuality);

            _chkSingleMultiPageTiff = new CheckBox
            {
                Text = "Single File with Multiple Pages",
                Location = new Point(142, 106),
                AutoSize = true
            };
            grpImageType.Controls.Add(_chkSingleMultiPageTiff);

            // Column 3
            _rbBmp = new RadioButton { Text = "BMP", Location = new Point(248, 22), AutoSize = true };
            _rbEmf = new RadioButton { Text = "EMF", Location = new Point(248, 50), AutoSize = true };

            var allRadios = new[] { _rbPng, _rbEps, _rbJpg, _rbTif, _rbGif, _rbWmf, _rbBmp, _rbEmf };
            foreach (var rb in allRadios)
            {
                rb.CheckedChanged += (s, e) => UpdateControlStates();
                grpImageType.Controls.Add(rb);
            }

            this.Controls.Add(grpImageType);

            // Bottom Buttons
            _btnSaveAsImage = new Button
            {
                Text = "Save As Image ...",
                Location = new Point(95, 278),
                Size = new Size(125, 26)
            };
            _btnSaveAsImage.Click += BtnSaveAsImage_Click;
            this.Controls.Add(_btnSaveAsImage);

            _btnClose = new Button
            {
                Text = "Close",
                Location = new Point(240, 278),
                Size = new Size(80, 26)
            };
            _btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(_btnClose);

            this.ClientSize = new Size(466, 318);
        }

        private string GetSelectedImageType()
        {
            if (_rbPng.Checked) return "PNG";
            if (_rbEps.Checked) return "EPS";
            if (_rbJpg.Checked) return "JPG";
            if (_rbTif.Checked) return "TIF";
            if (_rbGif.Checked) return "GIF";
            if (_rbWmf.Checked) return "WMF";
            if (_rbBmp.Checked) return "BMP";
            if (_rbEmf.Checked) return "EMF";
            return "PNG";
        }

        private void UpdateControlStates()
        {
            string type = GetSelectedImageType();
            _txtJpgQuality.Enabled = type == "JPG";
            _chkSingleMultiPageTiff.Enabled = type == "TIF";
            if (type != "TIF")
                _chkSingleMultiPageTiff.Checked = false;

            // Color bits only applies to raster formats (PNG, JPG, TIF, BMP)
            _cmbColorBits.Enabled = type is "PNG" or "JPG" or "TIF" or "BMP";

            // Page number digits enabled when exporting multiple pages
            bool multiPage = int.TryParse(_txtFromPage.Text.Trim(), out int f) &&
                             int.TryParse(_txtToPage.Text.Trim(), out int t) &&
                             (t > f);
            _cmbPageDigits.Enabled = multiPage && !_chkSingleMultiPageTiff.Checked;
        }

        private void BtnSaveAsImage_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(_txtFromPage.Text.Trim(), out int fromPage) ||
                    !int.TryParse(_txtToPage.Text.Trim(), out int toPage))
                {
                    throw new ArgumentException("Please enter valid page numbers for From and To.");
                }

                if (!int.TryParse(_txtResolution.Text.Trim(), out int dpi) || dpi < 6 || dpi > 600)
                {
                    throw new ArgumentOutOfRangeException("Image Resolution must be between 6 and 600 dpi.");
                }

                if (!int.TryParse(_txtJpgQuality.Text.Trim(), out int jpgQual) || jpgQual < 1 || jpgQual > 100)
                {
                    throw new ArgumentOutOfRangeException("JPG Quality must be between 1 and 100.");
                }

                string imgType = GetSelectedImageType();
                string ext = imgType.ToLowerInvariant();
                string defaultBase = Path.GetFileNameWithoutExtension(_filePath);

                using var sfd = new SaveFileDialog
                {
                    Title = "Save Image As",
                    Filter = $"{imgType} Image (*.{ext})|*.{ext}",
                    DefaultExt = ext,
                    FileName = $"{defaultBase}.{ext}"
                };

                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                var opts = new PdfToImagesOptions
                {
                    FromPage = fromPage,
                    ToPage = toPage,
                    Dpi = dpi,
                    ColorBits = int.Parse(_cmbColorBits.SelectedItem!.ToString()!),
                    PageNumberDigits = int.Parse(_cmbPageDigits.SelectedItem!.ToString()!),
                    ImageType = imgType,
                    JpgQuality = jpgQual,
                    SingleMultiPageTiff = _chkSingleMultiPageTiff.Checked
                };

                this.Cursor = Cursors.WaitCursor;
                var files = _pdfService.ConvertPdfToImages(_filePath, sfd.FileName, opts);
                this.Cursor = Cursors.Default;

                MessageBox.Show($"Successfully exported {files.Count} image file(s)!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}