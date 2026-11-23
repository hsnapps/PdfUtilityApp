using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using PdfUtilityApp.Services;

namespace PdfUtilityApp.Forms
{
    public class ReformatPagesForm : Form
    {
        private readonly PdfService _pdfService = new();
        private readonly string _filePath;
        private readonly int _totalPages;
        private bool _isUpdatingSize = false;

        private readonly Dictionary<string, (double W, double H)> _paperSizes = new()
        {
            ["Letter"] = (8.5, 11.0),
            ["Legal"] = (8.5, 14.0),
            ["Tabloid"] = (11.0, 17.0),
            ["A3"] = (11.69, 16.54),
            ["A4"] = (8.27, 11.69),
            ["A5"] = (5.83, 8.27),
            ["Custom"] = (8.5, 11.0)
        };

        // Controls
        private ComboBox _cmbPageSize = null!;
        private RadioButton _rbPortrait = null!;
        private RadioButton _rbLandscape = null!;
        private TextBox _txtWidth = null!;
        private TextBox _txtHeight = null!;

        private ComboBox _cmbRows = null!;
        private ComboBox _cmbColumns = null!;

        private TextBox _txtMarginLeft = null!;
        private TextBox _txtMarginRight = null!;
        private TextBox _txtMarginBottom = null!;
        private TextBox _txtMarginTop = null!;

        private TextBox _txtSpaceHorizontal = null!;
        private TextBox _txtSpaceVertical = null!;

        private CheckBox _chkShowNoteLines = null!;
        private CheckBox _chkShowPageBorder = null!;
        private Button _btnChangeColor = null!;
        private TextBox _txtLineWidth = null!;

        private Button _btnSaveAs = null!;
        private Button _btnClose = null!;

        public ReformatPagesForm(string filePath)
        {
            _filePath = filePath;
            _totalPages = _pdfService.GetPageCount(filePath);
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Reformat Multiple Pages";
            this.Size = new Size(465, 425);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Header
            var lblHeader = new Label
            {
                Text = $"There are {_totalPages} pages in {Path.GetFileName(_filePath)}",
                Location = new Point(12, 12),
                AutoSize = true
            };
            this.Controls.Add(lblHeader);

            // New PDF Page Size
            var lblPageSize = new Label { Text = "New PDF Page Size:", Location = new Point(12, 42), AutoSize = true };
            this.Controls.Add(lblPageSize);

            _cmbPageSize = new ComboBox
            {
                Location = new Point(146, 39),
                Size = new Size(85, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (var key in _paperSizes.Keys)
                _cmbPageSize.Items.Add(key);
            _cmbPageSize.SelectedIndex = 0; // Letter
            _cmbPageSize.SelectedIndexChanged += (s, e) => UpdatePageDimensionsFromPreset();
            this.Controls.Add(_cmbPageSize);

            _rbPortrait = new RadioButton { Text = "Portrait", Location = new Point(265, 40), AutoSize = true, Checked = true };
            _rbPortrait.CheckedChanged += (s, e) => { if (_rbPortrait.Checked) UpdatePageDimensionsFromPreset(); };
            this.Controls.Add(_rbPortrait);

            _rbLandscape = new RadioButton { Text = "Landscape", Location = new Point(342, 40), AutoSize = true };
            _rbLandscape.CheckedChanged += (s, e) => { if (_rbLandscape.Checked) UpdatePageDimensionsFromPreset(); };
            this.Controls.Add(_rbLandscape);

            // Width / Height
            var lblWidth = new Label { Text = "Width:", Location = new Point(68, 72), AutoSize = true };
            this.Controls.Add(lblWidth);

            _txtWidth = new TextBox { Text = "8.5 \"", Location = new Point(146, 69), Size = new Size(85, 23), ReadOnly = true };
            this.Controls.Add(_txtWidth);

            var lblHeight = new Label { Text = "Height:", Location = new Point(265, 72), AutoSize = true };
            this.Controls.Add(lblHeight);

            _txtHeight = new TextBox { Text = "11 \"", Location = new Point(342, 69), Size = new Size(85, 23), ReadOnly = true };
            this.Controls.Add(_txtHeight);

            // Original PDF Page Layout
            var lblLayout = new Label { Text = "Original PDF Page Layout:", Location = new Point(12, 104), AutoSize = true };
            this.Controls.Add(lblLayout);

            var lblRows = new Label { Text = "Rows:", Location = new Point(68, 134), AutoSize = true };
            this.Controls.Add(lblRows);

            _cmbRows = new ComboBox { Location = new Point(146, 131), Size = new Size(85, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            for (int i = 1; i <= 8; i++) _cmbRows.Items.Add(i.ToString());
            _cmbRows.SelectedIndex = 1; // 2
            this.Controls.Add(_cmbRows);

            var lblColumns = new Label { Text = "Columns:", Location = new Point(265, 134), AutoSize = true };
            this.Controls.Add(lblColumns);

            _cmbColumns = new ComboBox { Location = new Point(342, 131), Size = new Size(85, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            for (int i = 1; i <= 8; i++) _cmbColumns.Items.Add(i.ToString());
            _cmbColumns.SelectedIndex = 0; // 1
            this.Controls.Add(_cmbColumns);

            // Margins
            var lblMargins = new Label { Text = "Margin from Original PDF Page to New PDF Page Edge:", Location = new Point(12, 166), AutoSize = true };
            this.Controls.Add(lblMargins);

            this.Controls.Add(new Label { Text = "Left:", Location = new Point(68, 195), AutoSize = true });
            _txtMarginLeft = new TextBox { Text = "0.5 \"", Location = new Point(146, 192), Size = new Size(85, 23) };
            this.Controls.Add(_txtMarginLeft);

            this.Controls.Add(new Label { Text = "Right:", Location = new Point(265, 195), AutoSize = true });
            _txtMarginRight = new TextBox { Text = "0.5 \"", Location = new Point(342, 192), Size = new Size(85, 23) };
            this.Controls.Add(_txtMarginRight);

            this.Controls.Add(new Label { Text = "Bottom:", Location = new Point(68, 224), AutoSize = true });
            _txtMarginBottom = new TextBox { Text = "0.5 \"", Location = new Point(146, 221), Size = new Size(85, 23) };
            this.Controls.Add(_txtMarginBottom);

            this.Controls.Add(new Label { Text = "Top:", Location = new Point(265, 224), AutoSize = true });
            _txtMarginTop = new TextBox { Text = "0.5 \"", Location = new Point(342, 221), Size = new Size(85, 23) };
            this.Controls.Add(_txtMarginTop);

            // Space between Original PDF Pages
            var lblSpacing = new Label { Text = "Space between Original PDF Pages", Location = new Point(12, 255), AutoSize = true };
            this.Controls.Add(lblSpacing);

            this.Controls.Add(new Label { Text = "Horizontal:", Location = new Point(68, 285), AutoSize = true });
            _txtSpaceHorizontal = new TextBox { Text = "0.25 \"", Location = new Point(146, 282), Size = new Size(85, 23) };
            this.Controls.Add(_txtSpaceHorizontal);

            this.Controls.Add(new Label { Text = "Vertical:", Location = new Point(265, 285), AutoSize = true });
            _txtSpaceVertical = new TextBox { Text = "0.25 \"", Location = new Point(342, 282), Size = new Size(85, 23) };
            this.Controls.Add(_txtSpaceVertical);

            // Checkboxes & Line Styling
            _chkShowNoteLines = new CheckBox { Text = "Show Note Lines", Location = new Point(12, 318), AutoSize = true };
            this.Controls.Add(_chkShowNoteLines);

            _chkShowPageBorder = new CheckBox { Text = "Show Original PDF Page's Border", Location = new Point(265, 318), AutoSize = true };
            this.Controls.Add(_chkShowPageBorder);

            this.Controls.Add(new Label { Text = "Line Color", Location = new Point(12, 347), AutoSize = true });

            _btnChangeColor = new Button
            {
                Text = "Change Color",
                Location = new Point(146, 343),
                Size = new Size(85, 23),
                BackColor = Color.Blue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Standard
            };
            _btnChangeColor.Click += BtnChangeColor_Click;
            this.Controls.Add(_btnChangeColor);

            this.Controls.Add(new Label { Text = "Line Width", Location = new Point(265, 347), AutoSize = true });
            _txtLineWidth = new TextBox { Text = "0.0138889 \"", Location = new Point(342, 344), Size = new Size(85, 23) };
            this.Controls.Add(_txtLineWidth);

            // Bottom Action Buttons
            _btnSaveAs = new Button
            {
                Text = "Save As ...",
                Location = new Point(98, 378),
                Size = new Size(68, 24)
            };
            _btnSaveAs.Click += BtnSaveAs_Click;
            this.Controls.Add(_btnSaveAs);

            _btnClose = new Button
            {
                Text = "Close",
                Location = new Point(192, 378),
                Size = new Size(68, 24)
            };
            _btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(_btnClose);
        }

        private void UpdatePageDimensionsFromPreset()
        {
            if (_isUpdatingSize) return;
            _isUpdatingSize = true;

            string preset = _cmbPageSize.SelectedItem?.ToString() ?? "Letter";
            bool isCustom = preset == "Custom";
            _txtWidth.ReadOnly = !isCustom;
            _txtHeight.ReadOnly = !isCustom;

            if (!isCustom && _paperSizes.TryGetValue(preset, out var dims))
            {
                double w = _rbPortrait.Checked ? dims.W : dims.H;
                double h = _rbPortrait.Checked ? dims.H : dims.W;
                _txtWidth.Text = $"{w} \"";
                _txtHeight.Text = $"{h} \"";
            }

            _isUpdatingSize = false;
        }

        private void BtnChangeColor_Click(object? sender, EventArgs e)
        {
            using var cd = new ColorDialog
            {
                Color = _btnChangeColor.BackColor,
                FullOpen = true
            };

            if (cd.ShowDialog(this) == DialogResult.OK)
            {
                _btnChangeColor.BackColor = cd.Color;
                // Keep label readable against dark or light colors
                double brightness = (cd.Color.R * 0.299 + cd.Color.G * 0.587 + cd.Color.B * 0.114);
                _btnChangeColor.ForeColor = brightness > 140 ? Color.Black : Color.White;
            }
        }

        private static double ParseInches(string text)
        {
            string cleaned = text.Replace("\"", "").Trim();
            if (!double.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out double val) || val < 0)
                throw new ArgumentException($"Invalid measurement value: '{text}'");
            return val;
        }

        private void BtnSaveAs_Click(object? sender, EventArgs e)
        {
            try
            {
                var opts = new ReformatPagesOptions
                {
                    PageWidthInches = ParseInches(_txtWidth.Text),
                    PageHeightInches = ParseInches(_txtHeight.Text),
                    Rows = int.Parse(_cmbRows.SelectedItem!.ToString()!),
                    Columns = int.Parse(_cmbColumns.SelectedItem!.ToString()!),
                    MarginLeftInches = ParseInches(_txtMarginLeft.Text),
                    MarginRightInches = ParseInches(_txtMarginRight.Text),
                    MarginBottomInches = ParseInches(_txtMarginBottom.Text),
                    MarginTopInches = ParseInches(_txtMarginTop.Text),
                    HorizontalSpaceInches = ParseInches(_txtSpaceHorizontal.Text),
                    VerticalSpaceInches = ParseInches(_txtSpaceVertical.Text),
                    ShowNoteLines = _chkShowNoteLines.Checked,
                    ShowPageBorder = _chkShowPageBorder.Checked,
                    LineColor = _btnChangeColor.BackColor,
                    LineWidthInches = ParseInches(_txtLineWidth.Text)
                };

                string defaultName = Path.GetFileNameWithoutExtension(_filePath) + "_reformatted.pdf";
                using var sfd = new SaveFileDialog
                {
                    Title = "Save Reformatted PDF As",
                    Filter = "PDF File (*.pdf)|*.pdf",
                    DefaultExt = "pdf",
                    FileName = defaultName
                };

                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                this.Cursor = Cursors.WaitCursor;
                _pdfService.ReformatMultiplePages(_filePath, sfd.FileName, opts);
                this.Cursor = Cursors.Default;

                MessageBox.Show("PDF pages reformatted successfully!", "Success",
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