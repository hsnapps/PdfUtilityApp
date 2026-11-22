using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PdfUtilityApp.Services;

namespace PdfUtilityApp.Forms
{
    public class MergePdfForm : Form
    {
        private readonly PdfService _pdfService = new();
        private bool _sortAscending = true;

        // UI Controls
        private ListView _lvFiles = null!;
        private Button _btnAddFile = null!;
        private Button _btnAddFolder = null!;
        private Button _btnMoveUp = null!;
        private Button _btnMoveDown = null!;
        private Button _btnRemove = null!;
        private Button _btnSort = null!;
        private CheckBox _chkBookmark = null!;
        private Button _btnSaveAs = null!;
        private Button _btnClose = null!;

        public MergePdfForm()
        {
            InitializeComponent();
            UpdateButtonStates();
        }

        private void InitializeComponent()
        {
            this.Text = "Merge PDF Files";
            this.Size = new Size(640, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.AllowDrop = true;

            // Drag & Drop handlers on the dialog
            this.DragEnter += MergePdfForm_DragEnter;
            this.DragDrop += MergePdfForm_DragDrop;

            // Left ListView
            _lvFiles = new ListView
            {
                Location = new Point(15, 18),
                Size = new Size(395, 365),
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                MultiSelect = true,
                HideSelection = false
            };
            _lvFiles.Columns.Add("File Name", 160);
            _lvFiles.Columns.Add("Pages", 65);
            _lvFiles.Columns.Add("Path", 165);
            _lvFiles.SelectedIndexChanged += (s, e) => UpdateButtonStates();
            this.Controls.Add(_lvFiles);

            // Right-side Top Buttons
            _btnAddFile = new Button
            {
                Text = "Add a PDF File",
                Location = new Point(425, 18),
                Size = new Size(185, 28)
            };
            _btnAddFile.Click += BtnAddFile_Click;
            this.Controls.Add(_btnAddFile);

            _btnAddFolder = new Button
            {
                Text = "Add PDF Files under a Folder",
                Location = new Point(425, 54),
                Size = new Size(185, 28)
            };
            _btnAddFolder.Click += BtnAddFolder_Click;
            this.Controls.Add(_btnAddFolder);

            var lblDragHint = new Label
            {
                Text = "You can also drag and drop PDFs\ninto this dialog.",
                Location = new Point(425, 92),
                Size = new Size(185, 36),
                Font = new Font("Segoe UI", 8.5F)
            };
            this.Controls.Add(lblDragHint);

            // Right-side Reorder / Manage Buttons
            int actionBtnX = 472;
            int actionBtnWidth = 90;
            int actionBtnHeight = 26;

            _btnMoveUp = new Button
            {
                Text = "Move Up",
                Location = new Point(actionBtnX, 165),
                Size = new Size(actionBtnWidth, actionBtnHeight)
            };
            _btnMoveUp.Click += BtnMoveUp_Click;
            this.Controls.Add(_btnMoveUp);

            _btnMoveDown = new Button
            {
                Text = "Move Down",
                Location = new Point(actionBtnX, 205),
                Size = new Size(actionBtnWidth, actionBtnHeight)
            };
            _btnMoveDown.Click += BtnMoveDown_Click;
            this.Controls.Add(_btnMoveDown);

            _btnRemove = new Button
            {
                Text = "Remove",
                Location = new Point(actionBtnX, 245),
                Size = new Size(actionBtnWidth, actionBtnHeight)
            };
            _btnRemove.Click += BtnRemove_Click;
            this.Controls.Add(_btnRemove);

            _btnSort = new Button
            {
                Text = "Sort",
                Location = new Point(actionBtnX, 285),
                Size = new Size(actionBtnWidth, actionBtnHeight)
            };
            _btnSort.Click += BtnSort_Click;
            this.Controls.Add(_btnSort);

            // Bottom Checkbox
            _chkBookmark = new CheckBox
            {
                Text = "Add Bookmark using File Name (Professional User Only)",
                Location = new Point(15, 392),
                AutoSize = true
            };
            this.Controls.Add(_chkBookmark);

            // Bottom Action Buttons
            _btnSaveAs = new Button
            {
                Text = "Save As ...",
                Location = new Point(165, 422),
                Size = new Size(85, 28)
            };
            _btnSaveAs.Click += BtnSaveAs_Click;
            this.Controls.Add(_btnSaveAs);

            _btnClose = new Button
            {
                Text = "Close",
                Location = new Point(265, 422),
                Size = new Size(85, 28)
            };
            _btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(_btnClose);
        }

        private void AddFilesToList(IEnumerable<string> filePaths)
        {
            foreach (var file in filePaths)
            {
                if (!File.Exists(file) || !file.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    int pages = _pdfService.GetPageCount(file);
                    var item = new ListViewItem(Path.GetFileName(file));
                    item.SubItems.Add(pages.ToString());
                    item.SubItems.Add(Path.GetDirectoryName(file) ?? string.Empty);
                    item.Tag = file; // Store full path in Tag for easy retrieval
                    _lvFiles.Items.Add(item);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not read '{Path.GetFileName(file)}':\n{ex.Message}",
                        "Invalid PDF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            int totalItems = _lvFiles.Items.Count;
            int selectedCount = _lvFiles.SelectedItems.Count;
            bool hasFiles = totalItems > 0;

            // Save As & Sort enabled whenever at least 1 file is in the list
            _btnSaveAs.Enabled = hasFiles;
            _btnSort.Enabled = totalItems > 1;

            // Remove enabled if at least one row is selected
            _btnRemove.Enabled = selectedCount > 0;

            // Move Up / Move Down enabled when a single item is selected and can move
            if (selectedCount == 1)
            {
                int idx = _lvFiles.SelectedIndices[0];
                _btnMoveUp.Enabled = idx > 0;
                _btnMoveDown.Enabled = idx < totalItems - 1;
            }
            else
            {
                _btnMoveUp.Enabled = false;
                _btnMoveDown.Enabled = false;
            }
        }

        private void BtnAddFile_Click(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select PDF File(s)",
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = true
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                AddFilesToList(ofd.FileNames);
            }
        }

        private void BtnAddFolder_Click(object? sender, EventArgs e)
        {
            using var fbd = new FolderBrowserDialog
            {
                Description = "Select a folder containing PDF files"
            };

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                var pdfFiles = Directory.GetFiles(fbd.SelectedPath, "*.pdf", SearchOption.TopDirectoryOnly)
                                        .OrderBy(f => f);
                AddFilesToList(pdfFiles);
            }
        }

        private void MergePdfForm_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void MergePdfForm_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] droppedPaths)
            {
                var filesToAdd = new List<string>();
                foreach (var path in droppedPaths)
                {
                    if (Directory.Exists(path))
                        filesToAdd.AddRange(Directory.GetFiles(path, "*.pdf", SearchOption.TopDirectoryOnly));
                    else if (File.Exists(path))
                        filesToAdd.Add(path);
                }
                AddFilesToList(filesToAdd);
            }
        }

        private void BtnMoveUp_Click(object? sender, EventArgs e)
        {
            if (_lvFiles.SelectedIndices.Count != 1) return;
            int idx = _lvFiles.SelectedIndices[0];
            if (idx <= 0) return;

            var item = _lvFiles.Items[idx];
            _lvFiles.Items.RemoveAt(idx);
            _lvFiles.Items.Insert(idx - 1, item);
            item.Selected = true;
            item.Focused = true;
            UpdateButtonStates();
        }

        private void BtnMoveDown_Click(object? sender, EventArgs e)
        {
            if (_lvFiles.SelectedIndices.Count != 1) return;
            int idx = _lvFiles.SelectedIndices[0];
            if (idx >= _lvFiles.Items.Count - 1) return;

            var item = _lvFiles.Items[idx];
            _lvFiles.Items.RemoveAt(idx);
            _lvFiles.Items.Insert(idx + 1, item);
            item.Selected = true;
            item.Focused = true;
            UpdateButtonStates();
        }

        private void BtnRemove_Click(object? sender, EventArgs e)
        {
            foreach (ListViewItem item in _lvFiles.SelectedItems)
            {
                _lvFiles.Items.Remove(item);
            }
            UpdateButtonStates();
        }

        private void BtnSort_Click(object? sender, EventArgs e)
        {
            var items = _lvFiles.Items.Cast<ListViewItem>().ToList();
            items = _sortAscending
                ? items.OrderBy(i => i.Text, StringComparer.OrdinalIgnoreCase).ToList()
                : items.OrderByDescending(i => i.Text, StringComparer.OrdinalIgnoreCase).ToList();

            _sortAscending = !_sortAscending;

            _lvFiles.Items.Clear();
            _lvFiles.Items.AddRange(items.ToArray());
            UpdateButtonStates();
        }

        private void BtnSaveAs_Click(object? sender, EventArgs e)
        {
            if (_lvFiles.Items.Count == 0) return;

            using var sfd = new SaveFileDialog
            {
                Title = "Save Merged PDF As",
                Filter = "PDF File (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                FileName = "Merged.pdf"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    var orderedPaths = _lvFiles.Items.Cast<ListViewItem>()
                                                     .Select(item => item.Tag!.ToString()!)
                                                     .ToList();

                    _pdfService.MergePdfs(orderedPaths, sfd.FileName, _chkBookmark.Checked);

                    this.Cursor = Cursors.Default;
                    MessageBox.Show("PDF files merged successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show($"Error merging PDF files:\n{ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}