using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using PdfUtilityApp.Services;

namespace PdfUtilityApp.Forms
{
    public class HeaderFooterForm : Form
    {
        private readonly PdfService _pdfService = new();
        private readonly string _filePath;
        private readonly int _totalPages;
        private static readonly string SettingsFilePath =
            Path.Combine(AppContext.BaseDirectory, "last_header_footer.json");

        // Header controls
        private CheckBox _chkAddHeader = null!;
        private GroupBox _grpHeader = null!;
        private CheckBox _chkHeaderFirstPage = null!;
        private CheckBox _chkHeaderShowNum = null!;
        private TextBox _txtHeaderBefore = null!;
        private TextBox _txtHeaderAfter = null!;
        private ComboBox _cmbHeaderAlign = null!;
        private TextBox _txtHeaderOffset = null!;

        // Footer controls
        private CheckBox _chkAddFooter = null!;
        private GroupBox _grpFooter = null!;
        private CheckBox _chkFooterFirstPage = null!;
        private CheckBox _chkFooterShowNum = null!;
        private TextBox _txtFooterBefore = null!;
        private TextBox _txtFooterAfter = null!;
        private ComboBox _cmbFooterAlign = null!;
        private TextBox _txtFooterOffset = null!;

        // Text properties
        private ComboBox _cmbFontEncode = null!;
        private ComboBox _cmbFontName = null!;
        private ComboBox _cmbFontStyle = null!;
        private TextBox _txtFontSize = null!;
        private Button _btnFontColor = null!;

        // Margins
        private TextBox _txtMarginLeft = null!;
        private TextBox _txtMarginRight = null!;
        private TextBox _txtMarginTop = null!;
        private TextBox _txtMarginBottom = null!;

        // Bottom buttons
        private Button _btnResumeLast = null!;
        private Button _btnSaveAs = null!;
        private Button _btnClose = null!;

        public HeaderFooterForm(string filePath)
        {
            _filePath = filePath;
            _totalPages = _pdfService.GetPageCount(filePath);
            InitializeComponent();
            UpdateGroupStates();
        }

        private void InitializeComponent()
        {
            this.Text = "Add Header and Footer";
            this.Size = new Size(495, 535);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // File info header
            var lblFileInfo = new Label
            {
                Text = $"There are {_totalPages} pages in {Path.GetFileName(_filePath)}",
                Location = new Point(12, 12),
                AutoSize = true
            };
            this.Controls.Add(lblFileInfo);

            // Top Enable Checkboxes
            _chkAddHeader = new CheckBox
            {
                Text = "Add Header",
                Location = new Point(12, 38),
                AutoSize = true,
                Checked = true
            };
            _chkAddHeader.CheckedChanged += (s, e) => UpdateGroupStates();
            this.Controls.Add(_chkAddHeader);

            _chkAddFooter = new CheckBox
            {
                Text = "Add Footer",
                Location = new Point(245, 38),
                AutoSize = true,
                Checked = true
            };
            _chkAddFooter.CheckedChanged += (s, e) => UpdateGroupStates();
            this.Controls.Add(_chkAddFooter);

            // Header GroupBox
            _grpHeader = new GroupBox
            {
                Text = "Header",
                Location = new Point(12, 65),
                Size = new Size(225, 195)
            };
            BuildBandGroup(_grpHeader, "Header Page", $"of {_totalPages}.",
                out _chkHeaderFirstPage, out _chkHeaderShowNum,
                out _txtHeaderBefore, out _txtHeaderAfter,
                out _cmbHeaderAlign, out _txtHeaderOffset);
            this.Controls.Add(_grpHeader);

            // Footer GroupBox
            _grpFooter = new GroupBox
            {
                Text = "Footer",
                Location = new Point(245, 65),
                Size = new Size(225, 195)
            };
            BuildBandGroup(_grpFooter, "Footer Page", $"of {_totalPages}.",
                out _chkFooterFirstPage, out _chkFooterShowNum,
                out _txtFooterBefore, out _txtFooterAfter,
                out _cmbFooterAlign, out _txtFooterOffset);
            this.Controls.Add(_grpFooter);

            // Text Properties Section
            var lblTextProps = new Label { Text = "Text Properties:", Location = new Point(12, 272), AutoSize = true };
            this.Controls.Add(lblTextProps);

            this.Controls.Add(new Label { Text = "Font Encode:", Location = new Point(28, 298), AutoSize = true });
            _cmbFontEncode = new ComboBox
            {
                Location = new Point(112, 295),
                Size = new Size(112, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbFontEncode.Items.AddRange(new object[] { "Western", "Unicode (UTF-8)", "Arabic (Windows-1256)" });
            _cmbFontEncode.SelectedIndex = 0;
            this.Controls.Add(_cmbFontEncode);

            this.Controls.Add(new Label { Text = "Font Name:", Location = new Point(28, 328), AutoSize = true });
            _cmbFontName = new ComboBox
            {
                Location = new Point(112, 325),
                Size = new Size(112, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbFontName.Items.AddRange(new object[]
            {
                "Times New Roman", "Arial", "Calibri", "Courier New", "Segoe UI", "Tahoma", "Verdana"
            });
            _cmbFontName.SelectedIndex = 0;
            this.Controls.Add(_cmbFontName);

            this.Controls.Add(new Label { Text = "Font Style:", Location = new Point(268, 328), AutoSize = true });
            _cmbFontStyle = new ComboBox
            {
                Location = new Point(345, 325),
                Size = new Size(112, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbFontStyle.Items.AddRange(new object[] { "Regular", "Bold", "Italic", "Bold Italic" });
            _cmbFontStyle.SelectedIndex = 0;
            this.Controls.Add(_cmbFontStyle);

            this.Controls.Add(new Label { Text = "Font Size:", Location = new Point(28, 358), AutoSize = true });
            _txtFontSize = new TextBox
            {
                Text = "12.0",
                Location = new Point(112, 355),
                Size = new Size(56, 23)
            };
            this.Controls.Add(_txtFontSize);

            this.Controls.Add(new Label { Text = "Font Color:", Location = new Point(268, 358), AutoSize = true });
            _btnFontColor = new Button
            {
                Text = "Change Color",
                Location = new Point(345, 355),
                Size = new Size(112, 23),
                BackColor = Color.Blue,
                ForeColor = Color.White
            };
            _btnFontColor.Click += BtnFontColor_Click;
            this.Controls.Add(_btnFontColor);

            // Margin From Edges Section
            var lblMargins = new Label { Text = "Margin From Edges:", Location = new Point(12, 388), AutoSize = true };
            this.Controls.Add(lblMargins);

            this.Controls.Add(new Label { Text = "Left:", Location = new Point(28, 412), AutoSize = true });
            _txtMarginLeft = new TextBox { Text = "0.5 \"", Location = new Point(112, 409), Size = new Size(56, 23) };
            this.Controls.Add(_txtMarginLeft);

            this.Controls.Add(new Label { Text = "Right:", Location = new Point(268, 412), AutoSize = true });
            _txtMarginRight = new TextBox { Text = "0.5 \"", Location = new Point(345, 409), Size = new Size(56, 23) };
            this.Controls.Add(_txtMarginRight);

            this.Controls.Add(new Label { Text = "Top:", Location = new Point(28, 438), AutoSize = true });
            _txtMarginTop = new TextBox { Text = "0.5 \"", Location = new Point(112, 435), Size = new Size(56, 23) };
            this.Controls.Add(_txtMarginTop);

            this.Controls.Add(new Label { Text = "Bottom:", Location = new Point(268, 438), AutoSize = true });
            _txtMarginBottom = new TextBox { Text = "0.5 \"", Location = new Point(345, 435), Size = new Size(56, 23) };
            this.Controls.Add(_txtMarginBottom);

            // Resume to Last Options Button
            _btnResumeLast = new Button
            {
                Text = "Resume to Last Options",
                Location = new Point(175, 464),
                Size = new Size(140, 24)
            };
            _btnResumeLast.Click += BtnResumeLast_Click;
            this.Controls.Add(_btnResumeLast);

            // Bottom Save As & Close Buttons
            _btnSaveAs = new Button
            {
                Text = "Save As ...",
                Location = new Point(110, 494),
                Size = new Size(68, 24)
            };
            _btnSaveAs.Click += BtnSaveAs_Click;
            this.Controls.Add(_btnSaveAs);

            _btnClose = new Button
            {
                Text = "Close",
                Location = new Point(210, 494),
                Size = new Size(68, 24)
            };
            _btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(_btnClose);

            // Adjust client height cleanly to fit bottom buttons
            this.ClientSize = new Size(482, 526);
        }

        private static void BuildBandGroup(
            GroupBox grp,
            string defaultBefore,
            string defaultAfter,
            out CheckBox chkFirstPage,
            out CheckBox chkShowNum,
            out TextBox txtBefore,
            out TextBox txtAfter,
            out ComboBox cmbAlign,
            out TextBox txtOffset)
        {
            chkFirstPage = new CheckBox { Text = "Add on First Page", Location = new Point(18, 22), AutoSize = true, Checked = true };
            grp.Controls.Add(chkFirstPage);

            chkShowNum = new CheckBox { Text = "Show Page Number", Location = new Point(18, 46), AutoSize = true, Checked = true };
            grp.Controls.Add(chkShowNum);

            grp.Controls.Add(new Label { Text = "Text before Page Number:", Location = new Point(18, 70), AutoSize = true });
            txtBefore = new TextBox { Text = defaultBefore, Location = new Point(26, 88), Size = new Size(185, 22) };
            grp.Controls.Add(txtBefore);

            grp.Controls.Add(new Label { Text = "Text after Number:", Location = new Point(18, 114), AutoSize = true });
            txtAfter = new TextBox { Text = defaultAfter, Location = new Point(26, 132), Size = new Size(185, 22) };
            grp.Controls.Add(txtAfter);

            grp.Controls.Add(new Label { Text = "Alignment", Location = new Point(18, 160), AutoSize = true });
            cmbAlign = new ComboBox { Location = new Point(120, 157), Size = new Size(92, 22), DropDownStyle = ComboBoxStyle.DropDownList };
            cmbAlign.Items.AddRange(new object[] { "Left", "Center", "Right" });
            cmbAlign.SelectedIndex = 0;
            grp.Controls.Add(cmbAlign);

            // Page Offset sits right below Alignment inside the group box
            grp.Height = 212;
            grp.Controls.Add(new Label { Text = "Page Offset:", Location = new Point(18, 186), AutoSize = true });
            txtOffset = new TextBox { Text = "0", Location = new Point(120, 183), Size = new Size(92, 22) };
            grp.Controls.Add(txtOffset);
        }

        private void UpdateGroupStates()
        {
            _grpHeader.Enabled = _chkAddHeader.Checked;
            _grpFooter.Enabled = _chkAddFooter.Checked;
        }

        private void BtnFontColor_Click(object? sender, EventArgs e)
        {
            using var cd = new ColorDialog
            {
                Color = _btnFontColor.BackColor,
                FullOpen = true
            };

            if (cd.ShowDialog(this) == DialogResult.OK)
            {
                _btnFontColor.BackColor = cd.Color;
                double brightness = cd.Color.R * 0.299 + cd.Color.G * 0.587 + cd.Color.B * 0.114;
                _btnFontColor.ForeColor = brightness > 140 ? Color.Black : Color.White;
            }
        }

        private HeaderFooterOptions BuildOptionsFromUi()
        {
            if (!_chkAddHeader.Checked && !_chkAddFooter.Checked)
                throw new InvalidOperationException("Please enable at least Add Header or Add Footer.");

            if (!double.TryParse(_txtFontSize.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double fontSize) || fontSize <= 0)
                throw new ArgumentException("Please enter a valid positive Font Size.");

            if (!int.TryParse(_txtHeaderOffset.Text.Trim(), out int headerOffset))
                throw new ArgumentException("Header Page Offset must be an integer.");

            if (!int.TryParse(_txtFooterOffset.Text.Trim(), out int footerOffset))
                throw new ArgumentException("Footer Page Offset must be an integer.");

            return new HeaderFooterOptions
            {
                Header = new HeaderFooterBandOptions
                {
                    Enabled = _chkAddHeader.Checked,
                    AddOnFirstPage = _chkHeaderFirstPage.Checked,
                    ShowPageNumber = _chkHeaderShowNum.Checked,
                    TextBefore = _txtHeaderBefore.Text,
                    TextAfter = _txtHeaderAfter.Text,
                    Alignment = _cmbHeaderAlign.SelectedItem?.ToString() ?? "Left",
                    PageOffset = headerOffset
                },
                Footer = new HeaderFooterBandOptions
                {
                    Enabled = _chkAddFooter.Checked,
                    AddOnFirstPage = _chkFooterFirstPage.Checked,
                    ShowPageNumber = _chkFooterShowNum.Checked,
                    TextBefore = _txtFooterBefore.Text,
                    TextAfter = _txtFooterAfter.Text,
                    Alignment = _cmbFooterAlign.SelectedItem?.ToString() ?? "Left",
                    PageOffset = footerOffset
                },
                FontEncode = _cmbFontEncode.SelectedItem?.ToString() ?? "Western",
                FontName = _cmbFontName.SelectedItem?.ToString() ?? "Times New Roman",
                FontStyle = _cmbFontStyle.SelectedItem?.ToString() ?? "Regular",
                FontSize = fontSize,
                FontColor = _btnFontColor.BackColor,
                MarginLeftInches = ParseInches(_txtMarginLeft.Text),
                MarginRightInches = ParseInches(_txtMarginRight.Text),
                MarginTopInches = ParseInches(_txtMarginTop.Text),
                MarginBottomInches = ParseInches(_txtMarginBottom.Text)
            };
        }

        private static double ParseInches(string text)
        {
            string cleaned = text.Replace("\"", "").Trim();
            if (!double.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out double val) || val < 0)
                throw new ArgumentException($"Invalid margin value: '{text}'");
            return val;
        }

        private void BtnSaveAs_Click(object? sender, EventArgs e)
        {
            try
            {
                var opts = BuildOptionsFromUi();

                string defaultName = Path.GetFileNameWithoutExtension(_filePath) + "_header_footer.pdf";
                using var sfd = new SaveFileDialog
                {
                    Title = "Save PDF As",
                    Filter = "PDF File (*.pdf)|*.pdf",
                    DefaultExt = "pdf",
                    FileName = defaultName
                };

                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                this.Cursor = Cursors.WaitCursor;
                _pdfService.AddHeaderAndFooter(_filePath, sfd.FileName, opts);
                SaveLastOptions(opts);
                this.Cursor = Cursors.Default;

                MessageBox.Show("Header and Footer added successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SaveLastOptions(HeaderFooterOptions opts)
        {
            try
            {
                var dto = new SavedHeaderFooterDto
                {
                    HeaderEnabled = opts.Header.Enabled,
                    HeaderFirstPage = opts.Header.AddOnFirstPage,
                    HeaderShowNum = opts.Header.ShowPageNumber,
                    HeaderBefore = opts.Header.TextBefore,
                    HeaderAfter = opts.Header.TextAfter,
                    HeaderAlign = opts.Header.Alignment,
                    HeaderOffset = opts.Header.PageOffset,
                    FooterEnabled = opts.Footer.Enabled,
                    FooterFirstPage = opts.Footer.AddOnFirstPage,
                    FooterShowNum = opts.Footer.ShowPageNumber,
                    FooterBefore = opts.Footer.TextBefore,
                    FooterAfter = opts.Footer.TextAfter,
                    FooterAlign = opts.Footer.Alignment,
                    FooterOffset = opts.Footer.PageOffset,
                    FontEncode = opts.FontEncode,
                    FontName = opts.FontName,
                    FontStyle = opts.FontStyle,
                    FontSize = opts.FontSize,
                    FontColorArgb = opts.FontColor.ToArgb(),
                    MarginLeft = opts.MarginLeftInches,
                    MarginRight = opts.MarginRightInches,
                    MarginTop = opts.MarginTopInches,
                    MarginBottom = opts.MarginBottomInches
                };
                File.WriteAllText(SettingsFilePath, JsonSerializer.Serialize(dto));
            }
            catch { /* Ignore persistence errors */ }
        }

        private void BtnResumeLast_Click(object? sender, EventArgs e)
        {
            if (!File.Exists(SettingsFilePath))
            {
                MessageBox.Show("No previously saved options found yet.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var dto = JsonSerializer.Deserialize<SavedHeaderFooterDto>(File.ReadAllText(SettingsFilePath));
                if (dto == null) return;

                _chkAddHeader.Checked = dto.HeaderEnabled;
                _chkHeaderFirstPage.Checked = dto.HeaderFirstPage;
                _chkHeaderShowNum.Checked = dto.HeaderShowNum;
                _txtHeaderBefore.Text = dto.HeaderBefore;
                _txtHeaderAfter.Text = dto.HeaderAfter;
                _cmbHeaderAlign.SelectedItem = dto.HeaderAlign;
                _txtHeaderOffset.Text = dto.HeaderOffset.ToString();

                _chkAddFooter.Checked = dto.FooterEnabled;
                _chkFooterFirstPage.Checked = dto.FooterFirstPage;
                _chkFooterShowNum.Checked = dto.FooterShowNum;
                _txtFooterBefore.Text = dto.FooterBefore;
                _txtFooterAfter.Text = dto.FooterAfter;
                _cmbFooterAlign.SelectedItem = dto.FooterAlign;
                _txtFooterOffset.Text = dto.FooterOffset.ToString();

                _cmbFontEncode.SelectedItem = dto.FontEncode;
                _cmbFontName.SelectedItem = dto.FontName;
                _cmbFontStyle.SelectedItem = dto.FontStyle;
                _txtFontSize.Text = dto.FontSize.ToString("0.0", CultureInfo.InvariantCulture);

                var c = Color.FromArgb(dto.FontColorArgb);
                _btnFontColor.BackColor = c;
                double brightness = c.R * 0.299 + c.G * 0.587 + c.B * 0.114;
                _btnFontColor.ForeColor = brightness > 140 ? Color.Black : Color.White;

                _txtMarginLeft.Text = $"{dto.MarginLeft} \"";
                _txtMarginRight.Text = $"{dto.MarginRight} \"";
                _txtMarginTop.Text = $"{dto.MarginTop} \"";
                _txtMarginBottom.Text = $"{dto.MarginBottom} \"";

                UpdateGroupStates();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not restore last options:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private class SavedHeaderFooterDto
        {
            public bool HeaderEnabled { get; set; }
            public bool HeaderFirstPage { get; set; }
            public bool HeaderShowNum { get; set; }
            public string HeaderBefore { get; set; } = "";
            public string HeaderAfter { get; set; } = "";
            public string HeaderAlign { get; set; } = "Left";
            public int HeaderOffset { get; set; }
            public bool FooterEnabled { get; set; }
            public bool FooterFirstPage { get; set; }
            public bool FooterShowNum { get; set; }
            public string FooterBefore { get; set; } = "";
            public string FooterAfter { get; set; } = "";
            public string FooterAlign { get; set; } = "Left";
            public int FooterOffset { get; set; }
            public string FontEncode { get; set; } = "Western";
            public string FontName { get; set; } = "Times New Roman";
            public string FontStyle { get; set; } = "Regular";
            public double FontSize { get; set; } = 12.0;
            public int FontColorArgb { get; set; }
            public double MarginLeft { get; set; }
            public double MarginRight { get; set; }
            public double MarginTop { get; set; }
            public double MarginBottom { get; set; }
        }
    }
}