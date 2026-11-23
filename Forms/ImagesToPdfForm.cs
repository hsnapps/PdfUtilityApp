using System.Globalization;
using PdfUtilityApp.Services;

namespace PdfUtilityApp.Forms
{
    public class ImagesToPdfForm : Form
    {
        private readonly PdfService _pdfService = new();
        private bool _isUpdatingSize = false;

        private static readonly string[] SupportedExtensions =
            { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff" };

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

        private Button _btnAddImage = null!;
        private Button _btnAddFolder = null!;
        private ListView _lvImages = null!;
        private CheckBox _chkShowTitle = null!;
        private Button _btnMoveUp = null!;
        private Button _btnMoveDown = null!;
        private Button _btnRemove = null!;

        private CheckBox _chkUseImageSize = null!;
        private ComboBox _cmbImageDpi = null!;

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

        private CheckBox _chkUseCompress = null!;
        private ComboBox _cmbCompressDpi = null!;

        private Button _btnSaveAs = null!;
        private Button _btnClose = null!;

        public ImagesToPdfForm()
        {
            InitializeComponent();
            UpdateControlStates();
        }

        private void InitializeComponent()
        {
            this.Text = "Convert Images to PDF";
            this.Size = new Size(495, 535);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.AllowDrop = true;

            this.DragEnter += ImagesToPdfForm_DragEnter;
            this.DragDrop += ImagesToPdfForm_DragDrop;

            // Top instructions
            this.Controls.Add(new Label
            {
                Text = "Select images (Click the title to edit)",
                Location = new Point(12, 12),
                AutoSize = true
            });
            this.Controls.Add(new Label
            {
                Text = "You can drag and drop image files into this dialog.",
                Location = new Point(225, 12),
                AutoSize = true
            });

            // Add Buttons
            _btnAddImage = new Button
            {
                Text = "Add an Image",
                Location = new Point(12, 34),
                Size = new Size(150, 23)
            };
            _btnAddImage.Click += BtnAddImage_Click;
            this.Controls.Add(_btnAddImage);

            _btnAddFolder = new Button
            {
                Text = "Add all Images under a Folder",
                Location = new Point(240, 34),
                Size = new Size(177, 23)
            };
            _btnAddFolder.Click += BtnAddFolder_Click;
            this.Controls.Add(_btnAddFolder);

            // Image ListView (Title is editable on click)
            _lvImages = new ListView
            {
                Location = new Point(12, 65),
                Size = new Size(405, 115),
                View = View.Details,
                FullRowSelect = true,
                LabelEdit = true,
                HideSelection = false
            };
            _lvImages.Columns.Add("Title", 130);
            _lvImages.Columns.Add("Name", 130);
            _lvImages.Columns.Add("Size (point)", 85);
            _lvImages.Columns.Add("Pages", 55);
            _lvImages.SelectedIndexChanged += (s, e) => UpdateControlStates();
            _lvImages.AfterLabelEdit += LvImages_AfterLabelEdit;
            this.Controls.Add(_lvImages);

            // Right side of ListView
            _chkShowTitle = new CheckBox
            {
                Text = "Show Title",
                Location = new Point(422, 66),
                AutoSize = true
            };
            this.Controls.Add(_chkShowTitle);

            _btnMoveUp = new Button { Text = "Move Up", Location = new Point(424, 96), Size = new Size(78, 23) };
            _btnMoveUp.Click += BtnMoveUp_Click;
            this.Controls.Add(_btnMoveUp);

            _btnMoveDown = new Button { Text = "Move Down", Location = new Point(424, 126), Size = new Size(78, 23) };
            _btnMoveDown.Click += BtnMoveDown_Click;
            this.Controls.Add(_btnMoveDown);

            _btnRemove = new Button { Text = "Remove", Location = new Point(424, 156), Size = new Size(78, 23) };
            _btnRemove.Click += BtnRemove_Click;
            this.Controls.Add(_btnRemove);

            // Use Image Size with DPI
            _chkUseImageSize = new CheckBox
            {
                Text = "Use Image Size to decide Page Size with DPI:",
                Location = new Point(12, 192),
                AutoSize = true
            };
            _chkUseImageSize.CheckedChanged += (s, e) => UpdateControlStates();
            this.Controls.Add(_chkUseImageSize);

            _cmbImageDpi = new ComboBox
            {
                Location = new Point(272, 190),
                Size = new Size(82, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbImageDpi.Items.AddRange(new object[] { "72", "96", "150", "200", "300", "600" });
            _cmbImageDpi.SelectedItem = "300";
            this.Controls.Add(_cmbImageDpi);

            // New PDF Page Size
            this.Controls.Add(new Label { Text = "New PDF Page Size:", Location = new Point(12, 222), AutoSize = true });
            _cmbPageSize = new ComboBox
            {
                Location = new Point(146, 219),
                Size = new Size(82, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            foreach (var k in _paperSizes.Keys) _cmbPageSize.Items.Add(k);
            _cmbPageSize.SelectedIndex = 0; // Letter
            _cmbPageSize.SelectedIndexChanged += (s, e) => UpdatePageDimensionsFromPreset();
            this.Controls.Add(_cmbPageSize);

            _rbPortrait = new RadioButton { Text = "Portrait", Location = new Point(272, 220), AutoSize = true, Checked = true };
            _rbPortrait.CheckedChanged += (s, e) => { if (_rbPortrait.Checked) UpdatePageDimensionsFromPreset(); };
            this.Controls.Add(_rbPortrait);

            _rbLandscape = new RadioButton { Text = "Landscape", Location = new Point(398, 220), AutoSize = true };
            _rbLandscape.CheckedChanged += (s, e) => { if (_rbLandscape.Checked) UpdatePageDimensionsFromPreset(); };
            this.Controls.Add(_rbLandscape);

            this.Controls.Add(new Label { Text = "Width:", Location = new Point(52, 250), AutoSize = true });
            _txtWidth = new TextBox { Text = "8.5 \"", Location = new Point(146, 247), Size = new Size(82, 23), ReadOnly = true };
            this.Controls.Add(_txtWidth);

            this.Controls.Add(new Label { Text = "Height:", Location = new Point(272, 250), AutoSize = true });
            _txtHeight = new TextBox { Text = "11 \"", Location = new Point(398, 247), Size = new Size(82, 23), ReadOnly = true };
            this.Controls.Add(_txtHeight);

            // Image Layout
            this.Controls.Add(new Label { Text = "Image Layout:", Location = new Point(12, 280), AutoSize = true });
            this.Controls.Add(new Label { Text = "Rows:", Location = new Point(52, 308), AutoSize = true });
            _cmbRows = new ComboBox { Location = new Point(146, 305), Size = new Size(82, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            for (int i = 1; i <= 8; i++) _cmbRows.Items.Add(i.ToString());
            _cmbRows.SelectedIndex = 0;
            this.Controls.Add(_cmbRows);

            this.Controls.Add(new Label { Text = "Columns:", Location = new Point(272, 308), AutoSize = true });
            _cmbColumns = new ComboBox { Location = new Point(398, 305), Size = new Size(82, 23), DropDownStyle = ComboBoxStyle.DropDownList };
            for (int i = 1; i <= 8; i++) _cmbColumns.Items.Add(i.ToString());
            _cmbColumns.SelectedIndex = 0;
            this.Controls.Add(_cmbColumns);

            // Margins
            this.Controls.Add(new Label { Text = "Margin from Image to New PDF Page Edge:", Location = new Point(12, 338), AutoSize = true });

            this.Controls.Add(new Label { Text = "Left:", Location = new Point(52, 366), AutoSize = true });
            _txtMarginLeft = new TextBox { Text = "0.5 \"", Location = new Point(146, 363), Size = new Size(82, 23) };
            this.Controls.Add(_txtMarginLeft);

            this.Controls.Add(new Label { Text = "Right:", Location = new Point(272, 366), AutoSize = true });
            _txtMarginRight = new TextBox { Text = "0.5 \"", Location = new Point(398, 363), Size = new Size(82, 23) };
            this.Controls.Add(_txtMarginRight);

            this.Controls.Add(new Label { Text = "Bottom:", Location = new Point(52, 394), AutoSize = true });
            _txtMarginBottom = new TextBox { Text = "0.5 \"", Location = new Point(146, 391), Size = new Size(82, 23) };
            this.Controls.Add(_txtMarginBottom);

            this.Controls.Add(new Label { Text = "Top:", Location = new Point(272, 394), AutoSize = true });
            _txtMarginTop = new TextBox { Text = "0.5 \"", Location = new Point(398, 391), Size = new Size(82, 23) };
            this.Controls.Add(_txtMarginTop);

            // Space between Images
            this.Controls.Add(new Label { Text = "Space between Images:", Location = new Point(12, 424), AutoSize = true });

            this.Controls.Add(new Label { Text = "Horizontal:", Location = new Point(52, 452), AutoSize = true });
            _txtSpaceHorizontal = new TextBox { Text = "0.25 \"", Location = new Point(146, 449), Size = new Size(82, 23) };
            this.Controls.Add(_txtSpaceHorizontal);

            this.Controls.Add(new Label { Text = "Vertical:", Location = new Point(272, 452), AutoSize = true });
            _txtSpaceVertical = new TextBox { Text = "0.25 \"", Location = new Point(398, 449), Size = new Size(82, 23) };
            this.Controls.Add(_txtSpaceVertical);

            // Image Compress
            _chkUseCompress = new CheckBox
            {
                Text = "Use Image Compress  (Professional User Only)",
                Location = new Point(12, 482),
                AutoSize = true
            };
            _chkUseCompress.CheckedChanged += (s, e) => UpdateControlStates();
            this.Controls.Add(_chkUseCompress);

            _cmbCompressDpi = new ComboBox
            {
                Location = new Point(295, 480),
                Size = new Size(82, 23),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbCompressDpi.Items.AddRange(new object[] { "72", "96", "150", "200", "300" });
            _cmbCompressDpi.SelectedItem = "150";
            this.Controls.Add(_cmbCompressDpi);

            // Bottom Action Buttons
            _btnSaveAs = new Button
            {
                Text = "Save As ...",
                Location = new Point(116, 512),
                Size = new Size(66, 24)
            };
            _btnSaveAs.Click += BtnSaveAs_Click;
            this.Controls.Add(_btnSaveAs);

            _btnClose = new Button
            {
                Text = "Close",
                Location = new Point(212, 512),
                Size = new Size(66, 24)
            };
            _btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(_btnClose);

            this.ClientSize = new Size(520, 548);
        }

        private void AddImagesToList(IEnumerable<string> paths)
        {
            foreach (var file in paths)
            {
                if (!File.Exists(file)) continue;
                string ext = Path.GetExtension(file).ToLowerInvariant();
                if (!SupportedExtensions.Contains(ext)) continue;

                try
                {
                    var info = _pdfService.InspectImageFile(file);
                    var item = new ListViewItem(info.Title);
                    item.SubItems.Add(Path.GetFileName(file));
                    item.SubItems.Add($"{info.WidthPoints}x{info.HeightPoints}");
                    item.SubItems.Add(info.PageCount.ToString());
                    item.Tag = info;
                    _lvImages.Items.Add(item);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not read image '{Path.GetFileName(file)}':\n{ex.Message}",
                        "Invalid Image", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            UpdateControlStates();
        }

        private void LvImages_AfterLabelEdit(object? sender, LabelEditEventArgs e)
        {
            if (e.Label != null && _lvImages.Items[e.Item].Tag is ImageToPdfItem info)
            {
                info.Title = e.Label;
            }
        }

        private void UpdateControlStates()
        {
            int count = _lvImages.Items.Count;
            int selCount = _lvImages.SelectedItems.Count;

            _btnSaveAs.Enabled = count > 0;
            _btnRemove.Enabled = selCount > 0;

            if (selCount == 1)
            {
                int idx = _lvImages.SelectedIndices[0];
                _btnMoveUp.Enabled = idx > 0;
                _btnMoveDown.Enabled = idx < count - 1;
            }
            else
            {
                _btnMoveUp.Enabled = false;
                _btnMoveDown.Enabled = false;
            }

            _cmbImageDpi.Enabled = _chkUseImageSize.Checked;
            bool fixedPageMode = !_chkUseImageSize.Checked;

            _cmbPageSize.Enabled = fixedPageMode;
            _rbPortrait.Enabled = fixedPageMode;
            _rbLandscape.Enabled = fixedPageMode;
            _txtWidth.Enabled = fixedPageMode;
            _txtHeight.Enabled = fixedPageMode;
            _cmbRows.Enabled = fixedPageMode;
            _cmbColumns.Enabled = fixedPageMode;
            _txtMarginLeft.Enabled = fixedPageMode;
            _txtMarginRight.Enabled = fixedPageMode;
            _txtMarginBottom.Enabled = fixedPageMode;
            _txtMarginTop.Enabled = fixedPageMode;
            _txtSpaceHorizontal.Enabled = fixedPageMode;
            _txtSpaceVertical.Enabled = fixedPageMode;

            _cmbCompressDpi.Enabled = _chkUseCompress.Checked;
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

        private void BtnAddImage_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select Image(s)",
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff",
                Multiselect = true
            };
            if (ofd.ShowDialog(this) == DialogResult.OK)
                AddImagesToList(ofd.FileNames);
        }

        private void BtnAddFolder_Click(object? sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog { Description = "Select a folder containing images" };
            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                var files = Directory.GetFiles(fbd.SelectedPath, "*.*", SearchOption.TopDirectoryOnly)
                                     .OrderBy(f => f);
                AddImagesToList(files);
            }
        }

        private void ImagesToPdfForm_DragEnter(object? sender, DragEventArgs e)
        {
            e.Effect = (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                ? DragDropEffects.Copy
                : DragDropEffects.None;
        }

        private void ImagesToPdfForm_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] dropped)
            {
                var list = new List<string>();
                foreach (var p in dropped)
                {
                    if (Directory.Exists(p))
                        list.AddRange(Directory.GetFiles(p, "*.*", SearchOption.TopDirectoryOnly));
                    else if (File.Exists(p))
                        list.Add(p);
                }
                AddImagesToList(list);
            }
        }

        private void BtnMoveUp_Click(object? sender, EventArgs e)
        {
            if (_lvImages.SelectedIndices.Count != 1) return;
            int idx = _lvImages.SelectedIndices[0];
            if (idx <= 0) return;

            var item = _lvImages.Items[idx];
            _lvImages.Items.RemoveAt(idx);
            _lvImages.Items.Insert(idx - 1, item);
            item.Selected = true;
            UpdateControlStates();
        }

        private void BtnMoveDown_Click(object? sender, EventArgs e)
        {
            if (_lvImages.SelectedIndices.Count != 1) return;
            int idx = _lvImages.SelectedIndices[0];
            if (idx >= _lvImages.Items.Count - 1) return;

            var item = _lvImages.Items[idx];
            _lvImages.Items.RemoveAt(idx);
            _lvImages.Items.Insert(idx + 1, item);
            item.Selected = true;
            UpdateControlStates();
        }

        private void BtnRemove_Click(object? sender, EventArgs e)
        {
            foreach (ListViewItem item in _lvImages.SelectedItems)
                _lvImages.Items.Remove(item);
            UpdateControlStates();
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
                var items = _lvImages.Items.Cast<ListViewItem>()
                                           .Select(i => (ImageToPdfItem)i.Tag!)
                                           .ToList();

                var opts = new ImagesToPdfOptions
                {
                    ShowTitle = _chkShowTitle.Checked,
                    UseImageSizeWithDpi = _chkUseImageSize.Checked,
                    ImageSizeDpi = int.Parse(_cmbImageDpi.SelectedItem!.ToString()!),
                    PageWidthInches = ParseInches(_txtWidth.Text),
                    PageHeightInches = ParseInches(_txtHeight.Text),
                    Rows = int.Parse(_cmbRows.SelectedItem!.ToString()!),
                    Columns = int.Parse(_cmbColumns.SelectedItem!.ToString()!),
                    MarginLeftInches = ParseInches(_txtMarginLeft.Text),
                    MarginRightInches = ParseInches(_txtMarginRight.Text),
                    MarginBottomInches = ParseInches(_txtMarginBottom.Text),
                    MarginTopInches = ParseInches(_txtMarginTop.Text),
                    SpaceHorizontalInches = ParseInches(_txtSpaceHorizontal.Text),
                    SpaceVerticalInches = ParseInches(_txtSpaceVertical.Text),
                    UseImageCompress = _chkUseCompress.Checked,
                    CompressDpi = int.Parse(_cmbCompressDpi.SelectedItem!.ToString()!)
                };

                using var sfd = new SaveFileDialog
                {
                    Title = "Save PDF As",
                    Filter = "PDF File (*.pdf)|*.pdf",
                    DefaultExt = "pdf",
                    FileName = "ImagesToPdf.pdf"
                };

                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                this.Cursor = Cursors.WaitCursor;
                _pdfService.ConvertImagesToPdf(items, sfd.FileName, opts);
                this.Cursor = Cursors.Default;

                MessageBox.Show("Images converted to PDF successfully!", "Success",
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