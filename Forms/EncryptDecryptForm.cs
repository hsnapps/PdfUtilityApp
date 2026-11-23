using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PdfUtilityApp.Services;

namespace PdfUtilityApp.Forms
{
    public class EncryptDecryptForm : Form
    {
        private readonly PdfService _pdfService = new();
        private readonly string _filePath;
        private readonly int _totalPages;
        private readonly string? _openPassword;

        private TextBox _txtUserPassword = null!;
        private TextBox _txtOwnerPassword = null!;
        private RadioButton _rb40Bit = null!;
        private RadioButton _rb128Bit = null!;

        private CheckBox _chkPrinting = null!;
        private CheckBox _chkModifyContents = null!;
        private CheckBox _chkCopy = null!;
        private CheckBox _chkModifyAnnotations = null!;

        private CheckBox _chkFillIn = null!;
        private CheckBox _chkScreenReaders = null!;
        private CheckBox _chkAssembly = null!;
        private CheckBox _chkDegradedPrinting = null!;

        private Button _btnEncryptAs = null!;
        private Button _btnDecryptAs = null!;
        private Button _btnClose = null!;

        public EncryptDecryptForm(string filePath, int totalPages, string? openPassword = null)
        {
            _filePath = filePath;
            _totalPages = totalPages;
            _openPassword = openPassword;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Encrypt and Decrypt Options";
            this.Size = new Size(465, 385);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Top file info label
            var lblHeader = new Label
            {
                Text = $"There are {_totalPages} pages in {Path.GetFileName(_filePath)}",
                Location = new Point(30, 18),
                AutoSize = true
            };
            this.Controls.Add(lblHeader);

            // User Password
            var lblUserPwd = new Label
            {
                Text = "User Password",
                Location = new Point(30, 52),
                AutoSize = true
            };
            this.Controls.Add(lblUserPwd);

            _txtUserPassword = new TextBox
            {
                Location = new Point(132, 49),
                Size = new Size(120, 23),
                UseSystemPasswordChar = false
            };
            this.Controls.Add(_txtUserPassword);

            // Owner Password
            var lblOwnerPwd = new Label
            {
                Text = "Owner Password",
                Location = new Point(30, 88),
                AutoSize = true
            };
            this.Controls.Add(lblOwnerPwd);

            _txtOwnerPassword = new TextBox
            {
                Location = new Point(132, 85),
                Size = new Size(120, 23),
                UseSystemPasswordChar = false
            };
            this.Controls.Add(_txtOwnerPassword);

            // Encryption Level
            var lblEncLevel = new Label
            {
                Text = "Encryption Level",
                Location = new Point(30, 124),
                AutoSize = true
            };
            this.Controls.Add(lblEncLevel);

            _rb40Bit = new RadioButton
            {
                Text = "40-bit RC4",
                Location = new Point(132, 122),
                AutoSize = true,
                Checked = true
            };
            this.Controls.Add(_rb40Bit);

            _rb128Bit = new RadioButton
            {
                Text = "128-bit RC4",
                Location = new Point(232, 122),
                AutoSize = true
            };
            this.Controls.Add(_rb128Bit);

            // Security Options Label
            var lblSecOptions = new Label
            {
                Text = "Security Options",
                Location = new Point(30, 158),
                AutoSize = true
            };
            this.Controls.Add(lblSecOptions);

            // Left Column Checkboxes
            int leftColX = 62;
            _chkPrinting = new CheckBox { Text = "Printing", Location = new Point(leftColX, 185), AutoSize = true, Checked = true };
            _chkModifyContents = new CheckBox { Text = "Modify Contents", Location = new Point(leftColX, 215), AutoSize = true, Checked = true };
            _chkCopy = new CheckBox { Text = "Copy", Location = new Point(leftColX, 245), AutoSize = true, Checked = true };
            _chkModifyAnnotations = new CheckBox { Text = "Modify Annotations", Location = new Point(leftColX, 275), AutoSize = true, Checked = true };

            this.Controls.AddRange(new Control[] { _chkPrinting, _chkModifyContents, _chkCopy, _chkModifyAnnotations });

            // Right Column Checkboxes
            int rightColX = 245;
            _chkFillIn = new CheckBox { Text = "Fill In", Location = new Point(rightColX, 185), AutoSize = true, Checked = true };
            _chkScreenReaders = new CheckBox { Text = "Screen Readers", Location = new Point(rightColX, 215), AutoSize = true, Checked = true };
            _chkAssembly = new CheckBox { Text = "Assembly", Location = new Point(rightColX, 245), AutoSize = true, Checked = true };
            _chkDegradedPrinting = new CheckBox { Text = "Degraded Printing", Location = new Point(rightColX, 275), AutoSize = true, Checked = true };

            this.Controls.AddRange(new Control[] { _chkFillIn, _chkScreenReaders, _chkAssembly, _chkDegradedPrinting });

            // Bottom Action Buttons
            _btnEncryptAs = new Button
            {
                Text = "Encrypt As ...",
                Location = new Point(50, 312),
                Size = new Size(72, 26)
            };
            _btnEncryptAs.Click += BtnEncryptAs_Click;
            this.Controls.Add(_btnEncryptAs);

            _btnDecryptAs = new Button
            {
                Text = "Decrypt As ...",
                Location = new Point(150, 312),
                Size = new Size(72, 26)
            };
            _btnDecryptAs.Click += BtnDecryptAs_Click;
            this.Controls.Add(_btnDecryptAs);

            _btnClose = new Button
            {
                Text = "Close",
                Location = new Point(242, 312),
                Size = new Size(72, 26)
            };
            _btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(_btnClose);
        }

        private void BtnEncryptAs_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_txtUserPassword.Text) && string.IsNullOrEmpty(_txtOwnerPassword.Text))
            {
                MessageBox.Show("Please enter a User Password or an Owner Password before encrypting.",
                    "Password Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string defaultName = Path.GetFileNameWithoutExtension(_filePath) + "_encrypted.pdf";
            using var sfd = new SaveFileDialog
            {
                Title = "Save Encrypted PDF As",
                Filter = "PDF File (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                FileName = defaultName
            };

            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                var options = new PdfEncryptionOptions
                {
                    UserPassword = _txtUserPassword.Text,
                    OwnerPassword = _txtOwnerPassword.Text,
                    Use128Bit = _rb128Bit.Checked,
                    PermitPrint = _chkPrinting.Checked,
                    PermitModifyDocument = _chkModifyContents.Checked,
                    PermitExtractContent = _chkCopy.Checked,
                    PermitAnnotations = _chkModifyAnnotations.Checked,
                    PermitFormsFill = _chkFillIn.Checked,
                    PermitAccessibilityExtractContent = _chkScreenReaders.Checked,
                    PermitAssembleDocument = _chkAssembly.Checked,
                    PermitFullQualityPrint = _chkDegradedPrinting.Checked
                };

                _pdfService.EncryptPdf(_filePath, sfd.FileName, options, _openPassword);
                this.Cursor = Cursors.Default;

                MessageBox.Show("PDF encrypted successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show($"Encryption failed:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDecryptAs_Click(object? sender, EventArgs e)
        {
            // Use the password entered in User/Owner password box if provided, otherwise fallback to the password used when opening the file
            string? pwdToUse = !string.IsNullOrEmpty(_txtOwnerPassword.Text)
                ? _txtOwnerPassword.Text
                : (!string.IsNullOrEmpty(_txtUserPassword.Text) ? _txtUserPassword.Text : _openPassword);

            string defaultName = Path.GetFileNameWithoutExtension(_filePath) + "_decrypted.pdf";
            using var sfd = new SaveFileDialog
            {
                Title = "Save Decrypted PDF As",
                Filter = "PDF File (*.pdf)|*.pdf",
                DefaultExt = "pdf",
                FileName = defaultName
            };

            if (sfd.ShowDialog(this) != DialogResult.OK) return;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                _pdfService.DecryptPdf(_filePath, sfd.FileName, pwdToUse);
                this.Cursor = Cursors.Default;

                MessageBox.Show("PDF decrypted successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show($"Decryption failed (verify password):\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}