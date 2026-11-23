using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PdfUtilityApp.Services;

namespace PdfUtilityApp.Forms
{
    public class FormFieldOperationsForm : Form
    {
        private readonly PdfService _pdfService = new();
        private readonly string _filePath;
        private readonly int _totalPages;

        private RadioButton _rbDeleteAll = null!;
        private RadioButton _rbFlattenAll = null!;
        private RadioButton _rbListAll = null!;
        private RadioButton _rbEditSingle = null!;

        private TextBox _txtFieldName = null!;
        private TextBox _txtFieldValue = null!;

        private Button _btnSaveAs = null!;
        private Button _btnClose = null!;

        public FormFieldOperationsForm(string filePath)
        {
            _filePath = filePath;
            _totalPages = _pdfService.GetPageCount(filePath);
            InitializeComponent();
            UpdateControlStates();
        }

        private void InitializeComponent()
        {
            this.Text = "PDF Form Field Operations";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Header
            this.Controls.Add(new Label
            {
                Text = $"There are {_totalPages} pages in {Path.GetFileName(_filePath)}",
                Location = new Point(22, 16),
                AutoSize = true
            });

            // GroupBox: Form Operation
            var grpOp = new GroupBox
            {
                Text = "Form Operation",
                Location = new Point(22, 46),
                Size = new Size(415, 195)
            };

            _rbDeleteAll = new RadioButton
            {
                Text = "Delete all PDF Form Fields",
                Location = new Point(16, 26),
                AutoSize = true,
                Checked = true
            };
            _rbDeleteAll.CheckedChanged += (s, e) => UpdateControlStates();
            grpOp.Controls.Add(_rbDeleteAll);

            _rbFlattenAll = new RadioButton
            {
                Text = "Flatten all PDF Form Fields",
                Location = new Point(16, 62),
                AutoSize = true
            };
            _rbFlattenAll.CheckedChanged += (s, e) => UpdateControlStates();
            grpOp.Controls.Add(_rbFlattenAll);

            _rbListAll = new RadioButton
            {
                Text = "List all PDF Form Fields's Name and Value",
                Location = new Point(16, 98),
                AutoSize = true
            };
            _rbListAll.CheckedChanged += (s, e) => UpdateControlStates();
            grpOp.Controls.Add(_rbListAll);

            _rbEditSingle = new RadioButton
            {
                Text = "PDF Editor for a Single Field",
                Location = new Point(16, 134),
                AutoSize = true
            };
            _rbEditSingle.CheckedChanged += (s, e) => UpdateControlStates();
            grpOp.Controls.Add(_rbEditSingle);

            // Name & Value inputs for Single Field Editor
            grpOp.Controls.Add(new Label { Text = "Name:", Location = new Point(72, 164), AutoSize = true });
            _txtFieldName = new TextBox { Location = new Point(125, 161), Size = new Size(92, 23) };
            grpOp.Controls.Add(_txtFieldName);

            grpOp.Controls.Add(new Label { Text = "Value:", Location = new Point(232, 164), AutoSize = true });
            _txtFieldValue = new TextBox { Location = new Point(282, 161), Size = new Size(92, 23) };
            grpOp.Controls.Add(_txtFieldValue);

            this.Controls.Add(grpOp);

            // Bottom Action Buttons
            _btnSaveAs = new Button
            {
                Text = "Save As ...",
                Location = new Point(92, 255),
                Size = new Size(95, 26)
            };
            _btnSaveAs.Click += BtnSaveAs_Click;
            this.Controls.Add(_btnSaveAs);

            _btnClose = new Button
            {
                Text = "Close",
                Location = new Point(208, 255),
                Size = new Size(95, 26)
            };
            _btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(_btnClose);

            this.ClientSize = new Size(460, 298);
        }

        private void UpdateControlStates()
        {
            bool isSingleEdit = _rbEditSingle.Checked;
            _txtFieldName.Enabled = isSingleEdit;
            _txtFieldValue.Enabled = isSingleEdit;
        }

        private void BtnSaveAs_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_rbListAll.Checked)
                {
                    var fields = _pdfService.ListAllFormFields(_filePath);
                    if (fields.Count == 0)
                    {
                        MessageBox.Show("No form fields were found in this PDF.", "Form Fields",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    using var sfdTxt = new SaveFileDialog
                    {
                        Title = "Save Form Fields List As",
                        Filter = "Text File (*.txt)|*.txt",
                        DefaultExt = "txt",
                        FileName = Path.GetFileNameWithoutExtension(_filePath) + "_fields.txt"
                    };

                    if (sfdTxt.ShowDialog(this) != DialogResult.OK) return;

                    var sb = new StringBuilder();
                    sb.AppendLine($"Form Fields in {Path.GetFileName(_filePath)} ({fields.Count} total):");
                    sb.AppendLine(new string('-', 50));
                    foreach (var (name, val) in fields)
                    {
                        sb.AppendLine($"{name} = {val}");
                    }
                    File.WriteAllText(sfdTxt.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show($"Saved {fields.Count} form field(s) to:\n{sfdTxt.FileName}", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string suffix = _rbDeleteAll.Checked ? "_no_fields.pdf"
                    : _rbFlattenAll.Checked ? "_flattened.pdf"
                    : "_edited_field.pdf";

                using var sfd = new SaveFileDialog
                {
                    Title = "Save PDF As",
                    Filter = "PDF File (*.pdf)|*.pdf",
                    DefaultExt = "pdf",
                    FileName = Path.GetFileNameWithoutExtension(_filePath) + suffix
                };

                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                this.Cursor = Cursors.WaitCursor;

                if (_rbDeleteAll.Checked)
                {
                    _pdfService.DeleteAllFormFields(_filePath, sfd.FileName);
                }
                else if (_rbFlattenAll.Checked)
                {
                    _pdfService.FlattenAllFormFields(_filePath, sfd.FileName);
                }
                else if (_rbEditSingle.Checked)
                {
                    _pdfService.EditSingleFormField(_filePath, sfd.FileName, _txtFieldName.Text, _txtFieldValue.Text);
                }

                this.Cursor = Cursors.Default;
                MessageBox.Show("PDF form field operation completed successfully!", "Success",
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