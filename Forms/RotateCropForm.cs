using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PdfUtilityApp.Services;

namespace PdfUtilityApp.Forms
{
    public class RotateCropForm : Form
    {
        private readonly PdfService _pdfService = new();
        private readonly string _filePath;
        private readonly List<PageTransformInfo> _pages;
        private bool _isUpdatingUi = false;

        // Top Filter & Page Selection Controls
        private ComboBox _cmbSelectPage = null!;
        private CheckBox _chkLandscapeOnly = null!;
        private CheckBox _chkPortraitOnly = null!;
        private CheckBox _chkAllPages = null!;
        private CheckBox _chkOddPage = null!;
        private CheckBox _chkEvenPage = null!;

        // Rotate Controls
        private CheckBox _chkRotateBy = null!;
        private ComboBox _cmbRotateAngle = null!;

        // Crop Box Controls
        private CheckBox _chkCropBox = null!;
        private TextBox _txtLeftMargin = null!;
        private TextBox _txtRightMargin = null!;
        private TextBox _txtBottomMargin = null!;
        private TextBox _txtTopMargin = null!;

        // Bottom Grid & Buttons
        private ListView _lvPages = null!;
        private Button _btnSaveAs = null!;
        private Button _btnClose = null!;

        public RotateCropForm(string filePath)
        {
            _filePath = filePath;
            _pages = _pdfService.GetPagesTransformInfo(filePath);
            InitializeComponent();
            PopulatePageListView();
            UpdateControlStates();
        }

        private void InitializeComponent()
        {
            this.Text = "Rotate or Crop PDF Pages";
            this.Size = new Size(655, 455);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Header
            var lblHeader = new Label
            {
                Text = $"There are {_pages.Count} pages in {Path.GetFileName(_filePath)}",
                Location = new Point(14, 14),
                AutoSize = true
            };
            this.Controls.Add(lblHeader);

            // Row 1: Select Page + Landscape/Portrait filters
            var lblSelectPage = new Label
            {
                Text = "Select Page:",
                Location = new Point(14, 44),
                AutoSize = true
            };
            this.Controls.Add(lblSelectPage);

            _cmbSelectPage = new ComboBox
            {
                Location = new Point(195, 40),
                Size = new Size(112, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            for (int i = 1; i <= _pages.Count; i++)
                _cmbSelectPage.Items.Add(i.ToString());
            if (_cmbSelectPage.Items.Count > 0)
                _cmbSelectPage.SelectedIndex = 0;
            _cmbSelectPage.SelectedIndexChanged += CmbSelectPage_SelectedIndexChanged;
            this.Controls.Add(_cmbSelectPage);

            _chkLandscapeOnly = new CheckBox
            {
                Text = "Landscape Page only",
                Location = new Point(362, 42),
                AutoSize = true
            };
            _chkLandscapeOnly.CheckedChanged += FilterCheckbox_CheckedChanged;
            this.Controls.Add(_chkLandscapeOnly);

            _chkPortraitOnly = new CheckBox
            {
                Text = "Portrait Page Only",
                Location = new Point(508, 42),
                AutoSize = true
            };
            _chkPortraitOnly.CheckedChanged += FilterCheckbox_CheckedChanged;
            this.Controls.Add(_chkPortraitOnly);

            // Row 2: All Pages / Odd Page / Even Page
            _chkAllPages = new CheckBox
            {
                Text = "All Pages",
                Location = new Point(195, 74),
                AutoSize = true
            };
            _chkAllPages.CheckedChanged += FilterCheckbox_CheckedChanged;
            this.Controls.Add(_chkAllPages);

            _chkOddPage = new CheckBox
            {
                Text = "Odd Page",
                Location = new Point(362, 74),
                AutoSize = true
            };
            _chkOddPage.CheckedChanged += FilterCheckbox_CheckedChanged;
            this.Controls.Add(_chkOddPage);

            _chkEvenPage = new CheckBox
            {
                Text = "Even Page",
                Location = new Point(508, 74),
                AutoSize = true
            };
            _chkEvenPage.CheckedChanged += FilterCheckbox_CheckedChanged;
            this.Controls.Add(_chkEvenPage);

            // Row 3: Rotate by
            _chkRotateBy = new CheckBox
            {
                Text = "Rotate by",
                Location = new Point(14, 108),
                AutoSize = true
            };
            _chkRotateBy.CheckedChanged += (s, e) => { UpdateControlStates(); ApplyCurrentSettingsToTargetPages(); };
            this.Controls.Add(_chkRotateBy);

            _cmbRotateAngle = new ComboBox
            {
                Location = new Point(195, 105),
                Size = new Size(112, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbRotateAngle.Items.AddRange(new object[] { "0 Degree", "90 Degree", "180 Degree", "270 Degree" });
            _cmbRotateAngle.SelectedIndex = 0;
            _cmbRotateAngle.SelectedIndexChanged += (s, e) => ApplyCurrentSettingsToTargetPages();
            this.Controls.Add(_cmbRotateAngle);

            // Row 4 & 5: Crop Box & Margins
            _chkCropBox = new CheckBox
            {
                Text = "Crop Box:",
                Location = new Point(14, 142),
                AutoSize = true
            };
            _chkCropBox.CheckedChanged += (s, e) => { UpdateControlStates(); ApplyCurrentSettingsToTargetPages(); };
            this.Controls.Add(_chkCropBox);

            var lblLeftMargin = new Label { Text = "Left Margin:", Location = new Point(108, 143), AutoSize = true };
            this.Controls.Add(lblLeftMargin);

            _txtLeftMargin = new TextBox { Text = "0 \"", Location = new Point(195, 140), Size = new Size(112, 23) };
            _txtLeftMargin.TextChanged += (s, e) => ApplyCurrentSettingsToTargetPages();
            this.Controls.Add(_txtLeftMargin);

            var lblRightMargin = new Label { Text = "Right Margin:", Location = new Point(418, 143), AutoSize = true };
            this.Controls.Add(lblRightMargin);

            _txtRightMargin = new TextBox { Text = "0 \"", Location = new Point(508, 140), Size = new Size(112, 23) };
            _txtRightMargin.TextChanged += (s, e) => ApplyCurrentSettingsToTargetPages();
            this.Controls.Add(_txtRightMargin);

            var lblBottomMargin = new Label { Text = "Bottom Margin:", Location = new Point(108, 175), AutoSize = true };
            this.Controls.Add(lblBottomMargin);

            _txtBottomMargin = new TextBox { Text = "0 \"", Location = new Point(195, 172), Size = new Size(112, 23) };
            _txtBottomMargin.TextChanged += (s, e) => ApplyCurrentSettingsToTargetPages();
            this.Controls.Add(_txtBottomMargin);

            var lblTopMargin = new Label { Text = "Top Margin:", Location = new Point(418, 175), AutoSize = true };
            this.Controls.Add(lblTopMargin);

            _txtTopMargin = new TextBox { Text = "0 \"", Location = new Point(508, 172), Size = new Size(112, 23) };
            _txtTopMargin.TextChanged += (s, e) => ApplyCurrentSettingsToTargetPages();
            this.Controls.Add(_txtTopMargin);

            // Page Details ListView
            _lvPages = new ListView
            {
                Location = new Point(14, 210),
                Size = new Size(612, 165),
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = false,
                HideSelection = false
            };
            _lvPages.Columns.Add("Page", 45);
            _lvPages.Columns.Add("W", 68);
            _lvPages.Columns.Add("H", 68);
            _lvPages.Columns.Add("Orient", 50);
            _lvPages.Columns.Add("Rotation", 62);
            _lvPages.Columns.Add("Rotate By", 68);
            _lvPages.Columns.Add("L Margin", 60);
            _lvPages.Columns.Add("R Margin", 60);
            _lvPages.Columns.Add("B Margin", 60);
            _lvPages.Columns.Add("T Margin", 60);
            _lvPages.SelectedIndexChanged += LvPages_SelectedIndexChanged;
            this.Controls.Add(_lvPages);

            // Bottom Action Buttons
            _btnSaveAs = new Button
            {
                Text = "Save As ...",
                Location = new Point(188, 385),
                Size = new Size(74, 26)
            };
            _btnSaveAs.Click += BtnSaveAs_Click;
            this.Controls.Add(_btnSaveAs);

            _btnClose = new Button
            {
                Text = "Close",
                Location = new Point(286, 385),
                Size = new Size(74, 26)
            };
            _btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(_btnClose);
        }

        private void PopulatePageListView()
        {
            _lvPages.BeginUpdate();
            _lvPages.Items.Clear();

            foreach (var p in _pages)
            {
                var item = new ListViewItem(p.PageNumber.ToString());
                item.SubItems.Add($"{p.WidthInches} \"");
                item.SubItems.Add($"{p.HeightInches} \"");
                item.SubItems.Add(p.Orientation);
                item.SubItems.Add(p.CurrentRotation.ToString());
                item.SubItems.Add(p.RotateBy.ToString());
                item.SubItems.Add($"{p.LeftMarginInches} \"");
                item.SubItems.Add($"{p.RightMarginInches} \"");
                item.SubItems.Add($"{p.BottomMarginInches} \"");
                item.SubItems.Add($"{p.TopMarginInches} \"");
                _lvPages.Items.Add(item);
            }

            if (_lvPages.Items.Count > 0)
                _lvPages.Items[0].Selected = true;

            _lvPages.EndUpdate();
        }

        private void RefreshListViewValues()
        {
            for (int i = 0; i < _pages.Count; i++)
            {
                var p = _pages[i];
                var item = _lvPages.Items[i];
                item.SubItems[5].Text = p.RotateBy.ToString();
                item.SubItems[6].Text = $"{p.LeftMarginInches} \"";
                item.SubItems[7].Text = $"{p.RightMarginInches} \"";
                item.SubItems[8].Text = $"{p.BottomMarginInches} \"";
                item.SubItems[9].Text = $"{p.TopMarginInches} \"";
            }
        }

        private void UpdateControlStates()
        {
            bool anyBatchFilter = _chkAllPages.Checked || _chkOddPage.Checked || _chkEvenPage.Checked ||
                                  _chkLandscapeOnly.Checked || _chkPortraitOnly.Checked;
            _cmbSelectPage.Enabled = !anyBatchFilter;

            _cmbRotateAngle.Enabled = _chkRotateBy.Checked;

            bool cropEnabled = _chkCropBox.Checked;
            _txtLeftMargin.Enabled = cropEnabled;
            _txtRightMargin.Enabled = cropEnabled;
            _txtBottomMargin.Enabled = cropEnabled;
            _txtTopMargin.Enabled = cropEnabled;
        }

        private void FilterCheckbox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingUi) return;
            _isUpdatingUi = true;

            // Keep mutually exclusive pairs sensible
            if (sender == _chkAllPages && _chkAllPages.Checked)
            {
                _chkOddPage.Checked = false;
                _chkEvenPage.Checked = false;
            }
            else if ((sender == _chkOddPage || sender == _chkEvenPage) && ((CheckBox)sender).Checked)
            {
                _chkAllPages.Checked = false;
            }

            if (sender == _chkLandscapeOnly && _chkLandscapeOnly.Checked)
                _chkPortraitOnly.Checked = false;
            else if (sender == _chkPortraitOnly && _chkPortraitOnly.Checked)
                _chkLandscapeOnly.Checked = false;

            _isUpdatingUi = false;
            UpdateControlStates();
            ApplyCurrentSettingsToTargetPages();
        }

        private void CmbSelectPage_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingUi || _cmbSelectPage.SelectedIndex < 0) return;
            int idx = _cmbSelectPage.SelectedIndex;

            _isUpdatingUi = true;
            _lvPages.Items[idx].Selected = true;
            _lvPages.EnsureVisible(idx);
            LoadPageIntoInputs(_pages[idx]);
            _isUpdatingUi = false;
        }

        private void LvPages_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingUi || _lvPages.SelectedIndices.Count == 0) return;
            int idx = _lvPages.SelectedIndices[0];

            _isUpdatingUi = true;
            _cmbSelectPage.SelectedIndex = idx;
            LoadPageIntoInputs(_pages[idx]);
            _isUpdatingUi = false;
        }

        private void LoadPageIntoInputs(PageTransformInfo p)
        {
            _cmbRotateAngle.SelectedIndex = p.RotateBy switch
            {
                90 => 1,
                180 => 2,
                270 => 3,
                _ => 0
            };
            _txtLeftMargin.Text = $"{p.LeftMarginInches} \"";
            _txtRightMargin.Text = $"{p.RightMarginInches} \"";
            _txtBottomMargin.Text = $"{p.BottomMarginInches} \"";
            _txtTopMargin.Text = $"{p.TopMarginInches} \"";
        }

        private IEnumerable<PageTransformInfo> GetMatchingPages()
        {
            bool anyBatchFilter = _chkAllPages.Checked || _chkOddPage.Checked || _chkEvenPage.Checked ||
                                  _chkLandscapeOnly.Checked || _chkPortraitOnly.Checked;

            if (!anyBatchFilter)
            {
                int idx = Math.Max(0, _cmbSelectPage.SelectedIndex);
                return new[] { _pages[idx] };
            }

            return _pages.Where(p =>
            {
                if (_chkOddPage.Checked && p.PageNumber % 2 == 0) return false;
                if (_chkEvenPage.Checked && p.PageNumber % 2 != 0) return false;
                if (_chkLandscapeOnly.Checked && p.Orientation != "L") return false;
                if (_chkPortraitOnly.Checked && p.Orientation != "P") return false;
                return true;
            });
        }

        private void ApplyCurrentSettingsToTargetPages()
        {
            if (_isUpdatingUi) return;

            int angle = _chkRotateBy.Checked
                ? _cmbRotateAngle.SelectedIndex * 90
                : 0;

            double left = _chkCropBox.Checked ? ParseInches(_txtLeftMargin.Text) : 0;
            double right = _chkCropBox.Checked ? ParseInches(_txtRightMargin.Text) : 0;
            double bottom = _chkCropBox.Checked ? ParseInches(_txtBottomMargin.Text) : 0;
            double top = _chkCropBox.Checked ? ParseInches(_txtTopMargin.Text) : 0;

            foreach (var p in GetMatchingPages())
            {
                if (_chkRotateBy.Checked)
                    p.RotateBy = angle;

                if (_chkCropBox.Checked)
                {
                    p.LeftMarginInches = left;
                    p.RightMarginInches = right;
                    p.BottomMarginInches = bottom;
                    p.TopMarginInches = top;
                }
            }

            RefreshListViewValues();
        }

        private static double ParseInches(string text)
        {
            string cleaned = text.Replace("\"", "").Trim();
            return double.TryParse(cleaned, NumberStyles.Float, CultureInfo.InvariantCulture, out double val) && val >= 0
                ? val
                : 0;
        }

        private void BtnSaveAs_Click(object? sender, EventArgs e)
        {
            string defaultName = Path.GetFileNameWithoutExtension(_filePath) + "_rotated_cropped.pdf";
            using var sfd = new SaveFileDialog
            {
                Title = "Save Modified PDF As",
                Filter = "PDF File (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                FileName = defaultName
            };

            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                _pdfService.RotateAndCropPdf(_filePath, sfd.FileName, _pages);
                this.Cursor = Cursors.Default;

                MessageBox.Show("PDF pages updated successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show($"Error saving PDF:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}