using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using PdfUtilityApp.Services;

namespace PdfUtilityApp.Forms
{
    public class TextWatermarkForm : Form
    {
        private readonly PdfService _pdfService = new();
        private readonly string _filePath;
        private readonly int _totalPages;
        private static readonly string PresetsFilePath =
            Path.Combine(AppContext.BaseDirectory, "watermark_presets.json");

        private List<WatermarkPreset> _presets = new();

        private TextBox _txtString = null!;
        private ComboBox _cmbFontEncode = null!;
        private ComboBox _cmbFontName = null!;
        private ComboBox _cmbFontStyle = null!;
        private Button _btnFontColor = null!;
        private Button _btnStrokeColor = null!;
        private ComboBox _cmbLayout = null!;
        private ComboBox _cmbDrawType = null!;
        private TextBox _txtStrokeSize = null!;
        private ComboBox _cmbOrder = null!;

        private ComboBox _cmbPresetName = null!;
        private ComboBox _cmbSelectString = null!;
        private Button _btnSavePreset = null!;
        private Button _btnRemovePreset = null!;

        private Button _btnSaveAs = null!;
        private Button _btnClose = null!;

        public TextWatermarkForm(string filePath)
        {
            _filePath = filePath;
            _totalPages = _pdfService.GetPageCount(filePath);
            InitializeComponent();
            LoadPresets();
        }

        private void InitializeComponent()
        {
            this.Text = "Add Watermark by Text";
            this.Size = new Size(445, 455);
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

            // String
            this.Controls.Add(new Label { Text = "String:", Location = new Point(12, 42), AutoSize = true });
            _txtString = new TextBox
            {
                Text = "WATERMARK EXAMPLE HERE",
                Location = new Point(84, 39),
                Size = new Size(326, 23)
            };
            this.Controls.Add(_txtString);

            // Font Encode
            this.Controls.Add(new Label { Text = "Font Encode:", Location = new Point(12, 74), AutoSize = true });
            _cmbFontEncode = new ComboBox
            {
                Location = new Point(84, 71),
                Size = new Size(112, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbFontEncode.Items.AddRange(new object[] { "Western", "Unicode (UTF-8)", "Arabic (Windows-1256)" });
            _cmbFontEncode.SelectedIndex = 0;
            this.Controls.Add(_cmbFontEncode);

            // Font Name & Style
            this.Controls.Add(new Label { Text = "Font Name:", Location = new Point(12, 106), AutoSize = true });
            _cmbFontName = new ComboBox
            {
                Location = new Point(84, 103),
                Size = new Size(112, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbFontName.Items.AddRange(new object[]
            {
                "Times New Roman", "Arial", "Calibri", "Courier New", "Segoe UI", "Tahoma", "Verdana"
            });
            _cmbFontName.SelectedIndex = 0;
            this.Controls.Add(_cmbFontName);

            this.Controls.Add(new Label { Text = "Font Style:", Location = new Point(232, 106), AutoSize = true });
            _cmbFontStyle = new ComboBox
            {
                Location = new Point(296, 103),
                Size = new Size(114, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbFontStyle.Items.AddRange(new object[] { "Regular", "Bold", "Italic", "Bold Italic" });
            _cmbFontStyle.SelectedIndex = 0;
            this.Controls.Add(_cmbFontStyle);

            // Font Color & Stroke Color
            this.Controls.Add(new Label { Text = "Font Color:", Location = new Point(12, 138), AutoSize = true });
            _btnFontColor = new Button
            {
                Text = "Change Color",
                Location = new Point(84, 135),
                Size = new Size(112, 23),
                BackColor = Color.Blue,
                ForeColor = Color.White
            };
            _btnFontColor.Click += (s, e) => PickColor(_btnFontColor);
            this.Controls.Add(_btnFontColor);

            this.Controls.Add(new Label { Text = "Stroke Color:", Location = new Point(232, 138), AutoSize = true });
            _btnStrokeColor = new Button
            {
                Text = "Change Color",
                Location = new Point(296, 135),
                Size = new Size(114, 23),
                BackColor = Color.Red,
                ForeColor = Color.White
            };
            _btnStrokeColor.Click += (s, e) => PickColor(_btnStrokeColor);
            this.Controls.Add(_btnStrokeColor);

            // Layout & Draw Type
            this.Controls.Add(new Label { Text = "Layout:", Location = new Point(12, 170), AutoSize = true });
            _cmbLayout = new ComboBox
            {
                Location = new Point(84, 167),
                Size = new Size(112, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbLayout.Items.AddRange(new object[] { "Horizontal", "Diagonal", "Vertical" });
            _cmbLayout.SelectedIndex = 0;
            this.Controls.Add(_cmbLayout);

            this.Controls.Add(new Label { Text = "Draw Type:", Location = new Point(232, 170), AutoSize = true });
            _cmbDrawType = new ComboBox
            {
                Location = new Point(296, 167),
                Size = new Size(114, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbDrawType.Items.AddRange(new object[] { "Stroke", "Fill", "Fill & Stroke" });
            _cmbDrawType.SelectedIndex = 0;
            this.Controls.Add(_cmbDrawType);

            // Stroke Size
            this.Controls.Add(new Label { Text = "Stroke Size:", Location = new Point(12, 202), AutoSize = true });
            _txtStrokeSize = new TextBox
            {
                Text = "1.00",
                Location = new Point(84, 199),
                Size = new Size(46, 23)
            };
            this.Controls.Add(_txtStrokeSize);

            this.Controls.Add(new Label
            {
                Text = "percent of the actual font size",
                Location = new Point(148, 202),
                AutoSize = true
            });

            // Order
            this.Controls.Add(new Label { Text = "Order:", Location = new Point(12, 236), AutoSize = true });
            _cmbOrder = new ComboBox
            {
                Location = new Point(84, 233),
                Size = new Size(112, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbOrder.Items.AddRange(new object[] { "Background", "Foreground" });
            _cmbOrder.SelectedIndex = 0;
            this.Controls.Add(_cmbOrder);

            // Save Options GroupBox
            var grpSave = new GroupBox
            {
                Text = "Save Options (Professional User Only)",
                Location = new Point(12, 268),
                Size = new Size(398, 95)
            };

            grpSave.Controls.Add(new Label { Text = "Name:", Location = new Point(24, 26), AutoSize = true });
            _cmbPresetName = new ComboBox
            {
                Location = new Point(72, 23),
                Size = new Size(112, 23),
                DropDownStyle = ComboBoxStyle.DropDown
            };
            grpSave.Controls.Add(_cmbPresetName);

            grpSave.Controls.Add(new Label { Text = "Select String", Location = new Point(208, 26), AutoSize = true });
            _cmbSelectString = new ComboBox
            {
                Location = new Point(284, 23),
                Size = new Size(110, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbSelectString.SelectedIndexChanged += CmbSelectString_SelectedIndexChanged;
            grpSave.Controls.Add(_cmbSelectString);

            _btnSavePreset = new Button
            {
                Text = "Save",
                Location = new Point(72, 56),
                Size = new Size(112, 23)
            };
            _btnSavePreset.Click += BtnSavePreset_Click;
            grpSave.Controls.Add(_btnSavePreset);

            _btnRemovePreset = new Button
            {
                Text = "Remove",
                Location = new Point(284, 56),
                Size = new Size(110, 23)
            };
            _btnRemovePreset.Click += BtnRemovePreset_Click;
            grpSave.Controls.Add(_btnRemovePreset);

            this.Controls.Add(grpSave);

            // Bottom Action Buttons
            _btnSaveAs = new Button
            {
                Text = "Save As ...",
                Location = new Point(76, 376),
                Size = new Size(68, 25)
            };
            _btnSaveAs.Click += BtnSaveAs_Click;
            this.Controls.Add(_btnSaveAs);

            _btnClose = new Button
            {
                Text = "Close",
                Location = new Point(182, 376),
                Size = new Size(68, 25)
            };
            _btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(_btnClose);
        }

        private void PickColor(Button targetBtn)
        {
            using var cd = new ColorDialog { Color = targetBtn.BackColor, FullOpen = true };
            if (cd.ShowDialog(this) == DialogResult.OK)
            {
                targetBtn.BackColor = cd.Color;
                double brightness = cd.Color.R * 0.299 + cd.Color.G * 0.587 + cd.Color.B * 0.114;
                targetBtn.ForeColor = brightness > 140 ? Color.Black : Color.White;
            }
        }

        private void LoadPresets()
        {
            try
            {
                if (File.Exists(PresetsFilePath))
                {
                    _presets = JsonSerializer.Deserialize<List<WatermarkPreset>>(File.ReadAllText(PresetsFilePath)) ?? new();
                }
            }
            catch { _presets = new(); }

            RefreshPresetDropdowns();
        }

        private void RefreshPresetDropdowns()
        {
            _cmbPresetName.Items.Clear();
            _cmbSelectString.Items.Clear();
            foreach (var p in _presets)
            {
                _cmbPresetName.Items.Add(p.Name);
                _cmbSelectString.Items.Add(p.Name);
            }
            _cmbSelectString.Enabled = _presets.Count > 0;
            _btnRemovePreset.Enabled = _presets.Count > 0;
        }

        private void BtnSavePreset_Click(object? sender, EventArgs e)
        {
            string name = _cmbPresetName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a Name for this watermark preset.", "Name Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _presets.RemoveAll(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            _presets.Add(new WatermarkPreset
            {
                Name = name,
                Text = _txtString.Text,
                FontEncode = _cmbFontEncode.SelectedItem?.ToString() ?? "Western",
                FontName = _cmbFontName.SelectedItem?.ToString() ?? "Times New Roman",
                FontStyle = _cmbFontStyle.SelectedItem?.ToString() ?? "Regular",
                FontColorArgb = _btnFontColor.BackColor.ToArgb(),
                StrokeColorArgb = _btnStrokeColor.BackColor.ToArgb(),
                Layout = _cmbLayout.SelectedItem?.ToString() ?? "Horizontal",
                DrawType = _cmbDrawType.SelectedItem?.ToString() ?? "Stroke",
                StrokeSize = _txtStrokeSize.Text,
                Order = _cmbOrder.SelectedItem?.ToString() ?? "Background"
            });

            File.WriteAllText(PresetsFilePath, JsonSerializer.Serialize(_presets));
            RefreshPresetDropdowns();
            _cmbSelectString.SelectedItem = name;
        }

        private void BtnRemovePreset_Click(object? sender, EventArgs e)
        {
            string? selected = _cmbSelectString.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected)) return;

            _presets.RemoveAll(p => p.Name.Equals(selected, StringComparison.OrdinalIgnoreCase));
            File.WriteAllText(PresetsFilePath, JsonSerializer.Serialize(_presets));
            RefreshPresetDropdowns();
        }

        private void CmbSelectString_SelectedIndexChanged(object? sender, EventArgs e)
        {
            string? selected = _cmbSelectString.SelectedItem?.ToString();
            var preset = _presets.FirstOrDefault(p => p.Name == selected);
            if (preset == null) return;

            _cmbPresetName.Text = preset.Name;
            _txtString.Text = preset.Text;
            _cmbFontEncode.SelectedItem = preset.FontEncode;
            _cmbFontName.SelectedItem = preset.FontName;
            _cmbFontStyle.SelectedItem = preset.FontStyle;

            var fc = Color.FromArgb(preset.FontColorArgb);
            _btnFontColor.BackColor = fc;
            _btnFontColor.ForeColor = (fc.R * 0.299 + fc.G * 0.587 + fc.B * 0.114) > 140 ? Color.Black : Color.White;

            var sc = Color.FromArgb(preset.StrokeColorArgb);
            _btnStrokeColor.BackColor = sc;
            _btnStrokeColor.ForeColor = (sc.R * 0.299 + sc.G * 0.587 + sc.B * 0.114) > 140 ? Color.Black : Color.White;

            _cmbLayout.SelectedItem = preset.Layout;
            _cmbDrawType.SelectedItem = preset.DrawType;
            _txtStrokeSize.Text = preset.StrokeSize;
            _cmbOrder.SelectedItem = preset.Order;
        }

        private void BtnSaveAs_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!double.TryParse(_txtStrokeSize.Text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double strokePct) || strokePct < 0)
                    throw new ArgumentException("Stroke Size must be a valid non-negative number.");

                var opts = new TextWatermarkOptions
                {
                    Text = _txtString.Text,
                    FontEncode = _cmbFontEncode.SelectedItem?.ToString() ?? "Western",
                    FontName = _cmbFontName.SelectedItem?.ToString() ?? "Times New Roman",
                    FontStyle = _cmbFontStyle.SelectedItem?.ToString() ?? "Regular",
                    FontColor = _btnFontColor.BackColor,
                    StrokeColor = _btnStrokeColor.BackColor,
                    Layout = _cmbLayout.SelectedItem?.ToString() ?? "Horizontal",
                    DrawType = _cmbDrawType.SelectedItem?.ToString() ?? "Stroke",
                    StrokeSizePercent = strokePct,
                    IsBackground = (_cmbOrder.SelectedItem?.ToString() ?? "Background") == "Background"
                };

                string defaultName = Path.GetFileNameWithoutExtension(_filePath) + "_watermarked.pdf";
                using var sfd = new SaveFileDialog
                {
                    Title = "Save Watermarked PDF As",
                    Filter = "PDF File (*.pdf)|*.pdf",
                    DefaultExt = "pdf",
                    FileName = defaultName
                };

                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                this.Cursor = Cursors.WaitCursor;
                _pdfService.AddTextWatermark(_filePath, sfd.FileName, opts);
                this.Cursor = Cursors.Default;

                MessageBox.Show("Text watermark added successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private class WatermarkPreset
        {
            public string Name { get; set; } = "";
            public string Text { get; set; } = "";
            public string FontEncode { get; set; } = "Western";
            public string FontName { get; set; } = "Times New Roman";
            public string FontStyle { get; set; } = "Regular";
            public int FontColorArgb { get; set; }
            public int StrokeColorArgb { get; set; }
            public string Layout { get; set; } = "Horizontal";
            public string DrawType { get; set; } = "Stroke";
            public string StrokeSize { get; set; } = "1.00";
            public string Order { get; set; } = "Background";
        }
    }
}