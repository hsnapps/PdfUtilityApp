using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using PdfUtilityApp.Services;

namespace PdfUtilityApp.Forms
{
    public class ImageWatermarkForm : Form
    {
        private readonly PdfService _pdfService = new();
        private readonly string _pdfPath;
        private readonly string _imagePath;
        private readonly int _totalPages;
        private readonly double _origWidthInches;
        private readonly double _origHeightInches;
        private bool _isUpdatingPosition = false;

        private Label _lblScaleSize = null!;
        private TextBox _txtWidthPct = null!;
        private TextBox _txtHeightPct = null!;
        private TextBox _txtRotate = null!;

        private CheckBox _chkCenter = null!;
        private CheckBox _chkAnyPosition = null!;
        private TextBox _txtLeft = null!;
        private TextBox _txtBottom = null!;

        private ComboBox _cmbOrder = null!;
        private CheckBox _chkUrl = null!;
        private TextBox _txtUrl = null!;

        private Button _btnSaveAs = null!;
        private Button _btnClose = null!;

        public ImageWatermarkForm(string pdfPath, string imagePath)
        {
            _pdfPath = pdfPath;
            _imagePath = imagePath;
            _totalPages = _pdfService.GetPageCount(pdfPath);

            using (var img = Image.FromFile(imagePath))
            {
                // Matches PDF 72-pt/inch coordinate space shown in screenshot
                _origWidthInches = Math.Round(img.Width / 72.0, 4);
                _origHeightInches = Math.Round(img.Height / 72.0, 4);
            }

            InitializeComponent();
            UpdateControlStates();
        }

        private void InitializeComponent()
        {
            this.Text = "Add Watermark by Image";
            this.Size = new Size(445, 385);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Header 1: PDF pages
            this.Controls.Add(new Label
            {
                Text = $"There are {_totalPages} pages in {Path.GetFileName(_pdfPath)}",
                Location = new Point(12, 12),
                AutoSize = true
            });

            // Header 2: Original Size of image
            this.Controls.Add(new Label
            {
                Text = $"Original Size of {Path.GetFileName(_imagePath)}:  Width {_origWidthInches:0.####} \",   Height {_origHeightInches:0.####} \"",
                Location = new Point(12, 42),
                AutoSize = true
            });

            // Header 3: Live Scale Size
            _lblScaleSize = new Label
            {
                Text = $"Scale Size:  Width {_origWidthInches:0.####} \",   Height {_origHeightInches:0.####} \"",
                Location = new Point(12, 72),
                AutoSize = true
            };
            this.Controls.Add(_lblScaleSize);

            // Width % & Height %
            this.Controls.Add(new Label { Text = "Width:", Location = new Point(98, 102), AutoSize = true });
            _txtWidthPct = new TextBox { Text = "100.00%", Location = new Point(144, 99), Size = new Size(96, 23) };
            _txtWidthPct.TextChanged += (s, e) => RecalculateScaleSize();
            this.Controls.Add(_txtWidthPct);

            this.Controls.Add(new Label { Text = "Height:", Location = new Point(282, 102), AutoSize = true });
            _txtHeightPct = new TextBox { Text = "100.00%", Location = new Point(328, 99), Size = new Size(96, 23) };
            _txtHeightPct.TextChanged += (s, e) => RecalculateScaleSize();
            this.Controls.Add(_txtHeightPct);

            // Rotate (Degree)
            this.Controls.Add(new Label { Text = "Rotate (Degree):", Location = new Point(12, 134), AutoSize = true });
            _txtRotate = new TextBox { Text = "0.00", Location = new Point(144, 131), Size = new Size(96, 23) };
            this.Controls.Add(_txtRotate);

            // Position
            this.Controls.Add(new Label { Text = "Position:", Location = new Point(12, 164), AutoSize = true });

            _chkCenter = new CheckBox
            {
                Text = "Center of the page",
                Location = new Point(62, 192),
                AutoSize = true,
                Checked = true
            };
            _chkCenter.CheckedChanged += PositionCheckbox_CheckedChanged;
            this.Controls.Add(_chkCenter);

            _chkAnyPosition = new CheckBox
            {
                Text = "Any Position:",
                Location = new Point(62, 224),
                AutoSize = true,
                Checked = false
            };
            _chkAnyPosition.CheckedChanged += PositionCheckbox_CheckedChanged;
            this.Controls.Add(_chkAnyPosition);

            this.Controls.Add(new Label { Text = "Left:", Location = new Point(98, 254), AutoSize = true });
            _txtLeft = new TextBox { Text = "0.5 \"", Location = new Point(144, 251), Size = new Size(96, 23) };
            this.Controls.Add(_txtLeft);

            this.Controls.Add(new Label { Text = "Bottom:", Location = new Point(282, 254), AutoSize = true });
            _txtBottom = new TextBox { Text = "0.5 \"", Location = new Point(328, 251), Size = new Size(96, 23) };
            this.Controls.Add(_txtBottom);

            // Order
            this.Controls.Add(new Label { Text = "Order:", Location = new Point(12, 286), AutoSize = true });
            _cmbOrder = new ComboBox
            {
                Location = new Point(62, 283),
                Size = new Size(96, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbOrder.Items.AddRange(new object[] { "Background", "Foreground" });
            _cmbOrder.SelectedIndex = 0;
            this.Controls.Add(_cmbOrder);

            // URL
            _chkUrl = new CheckBox { Text = "URL", Location = new Point(12, 316), AutoSize = true };
            _chkUrl.CheckedChanged += (s, e) => UpdateControlStates();
            this.Controls.Add(_chkUrl);

            _txtUrl = new TextBox
            {
                Text = "http://www.PDFill.com",
                Location = new Point(62, 314),
                Size = new Size(362, 23)
            };
            this.Controls.Add(_txtUrl);

            // Bottom Action Buttons
            _btnSaveAs = new Button
            {
                Text = "Save As ...",
                Location = new Point(78, 348),
                Size = new Size(68, 24)
            };
            _btnSaveAs.Click += BtnSaveAs_Click;
            this.Controls.Add(_btnSaveAs);

            _btnClose = new Button
            {
                Text = "Close",
                Location = new Point(182, 348),
                Size = new Size(68, 24)
            };
            _btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(_btnClose);

            this.ClientSize = new Size(436, 382);
        }

        private void PositionCheckbox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingPosition) return;
            _isUpdatingPosition = true;

            if (sender == _chkCenter)
                _chkAnyPosition.Checked = !_chkCenter.Checked;
            else if (sender == _chkAnyPosition)
                _chkCenter.Checked = !_chkAnyPosition.Checked;

            _isUpdatingPosition = false;
            UpdateControlStates();
        }

        private void UpdateControlStates()
        {
            _txtLeft.Enabled = _chkAnyPosition.Checked;
            _txtBottom.Enabled = _chkAnyPosition.Checked;
            _txtUrl.Enabled = _chkUrl.Checked;
        }

        private void RecalculateScaleSize()
        {
            double wPct = ParsePercent(_txtWidthPct.Text, fallback: 100.0);
            double hPct = ParsePercent(_txtHeightPct.Text, fallback: 100.0);

            double scaledW = Math.Round(_origWidthInches * (wPct / 100.0), 4);
            double scaledH = Math.Round(_origHeightInches * (hPct / 100.0), 4);
            _lblScaleSize.Text = $"Scale Size:  Width {scaledW:0.####} \",   Height {scaledH:0.####} \"";
        }

        private static double ParsePercent(string text, double? fallback = null)
        {
            string cleaned = text.Replace("%", "").Trim();
            if (double.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out double val) && val > 0)
                return val;
            if (fallback.HasValue)
                return fallback.Value;
            throw new ArgumentException($"Invalid percentage value: '{text}'");
        }

        private static double ParseInches(string text)
        {
            string cleaned = text.Replace("\"", "").Trim();
            if (!double.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                throw new ArgumentException($"Invalid position value: '{text}'");
            return val;
        }

        private void BtnSaveAs_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!double.TryParse(_txtRotate.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double rotateDeg))
                    throw new ArgumentException("Rotate (Degree) must be a valid number.");

                var opts = new ImageWatermarkOptions
                {
                    ImagePath = _imagePath,
                    WidthPercent = ParsePercent(_txtWidthPct.Text),
                    HeightPercent = ParsePercent(_txtHeightPct.Text),
                    RotateDegrees = rotateDeg,
                    CenterOfPage = _chkCenter.Checked,
                    LeftInches = ParseInches(_txtLeft.Text),
                    BottomInches = ParseInches(_txtBottom.Text),
                    IsBackground = (_cmbOrder.SelectedItem?.ToString() ?? "Background") == "Background",
                    Url = _chkUrl.Checked ? _txtUrl.Text : null
                };

                string defaultName = Path.GetFileNameWithoutExtension(_pdfPath) + "_img_watermarked.pdf";
                using var sfd = new SaveFileDialog
                {
                    Title = "Save Watermarked PDF As",
                    Filter = "PDF File (*.pdf)|*.pdf",
                    DefaultExt = "pdf",
                    FileName = defaultName
                };

                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                this.Cursor = Cursors.WaitCursor;
                _pdfService.AddImageWatermark(_pdfPath, sfd.FileName, opts);
                this.Cursor = Cursors.Default;

                MessageBox.Show("Image watermark added successfully!", "Success",
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