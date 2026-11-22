using System;
using System.Drawing;
using System.Windows.Forms;
using PdfUtilityApp.Forms;

namespace PdfUtilityApp
{
    public class MainForm : Form
    {
        public MainForm()
        {
            InitializeLayout();
        }

        private void InitializeLayout()
        {
            // Window settings
            this.Text = "PDF Utility Tool";
            this.Size = new Size(780, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            // Top instruction label
            var lblHeader = new Label
            {
                Text = "Select a PDF Tool you wish to use:",
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Location = new Point(25, 25)
            };
            this.Controls.Add(lblHeader);

            // 3-column x 5-row grid for the 12 tool buttons
            var grid = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 5,
                Location = new Point(25, 65),
                Size = new Size(715, 300)
            };

            for (int c = 0; c < 3; c++)
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            for (int r = 0; r < 5; r++)
                grid.RowStyles.Add(new RowStyle(SizeType.Percent, 20F));

            // Button definitions mapped by (Row, Column)
            var tools = new (string Title, int Row, int Col, Action OnClick)[]
            {
                ("1. Merge PDF Files", 0, 0, () => { using var dlg = new MergePdfForm(); dlg.ShowDialog(this); }),
                ("2. Split or Reorder Pages", 1, 0, OpenSplitOrReorder),
                ("3. Encrypt and Decrypt Options", 2, 0, () => OpenTool("Encrypt and Decrypt Options")),
                ("4. Rotate or Crop PDF Pages", 3, 0, () => OpenTool("Rotate or Crop PDF Pages")),
                ("5. Reformat Multiple Pages", 4, 0, () => OpenTool("Reformat Multiple Pages")),

                ("6. Add Header and Footer", 0, 1, () => OpenTool("Add Header and Footer")),
                ("7. Add Watermark by Text", 1, 1, () => OpenTool("Add Watermark by Text")),
                ("8. Add Watermark by Image", 2, 1, () => OpenTool("Add Watermark by Image")),
                ("9. Convert Images to PDF", 3, 1, () => OpenTool("Convert Images to PDF")),
                ("10. Convert PDF to Images", 4, 1, () => OpenTool("Convert PDF to Images")),

                ("11. PDF Form Field Operations", 0, 2, () => OpenTool("PDF Form Field Operations")),
                ("12. Add PDF Information", 1, 2, () => OpenTool("Add PDF Information"))
            };

            foreach (var tool in tools)
            {
                var btn = new Button
                {
                    Text = tool.Title,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(8, 10, 8, 10),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(10, 0, 0, 0),
                    Font = new Font("Segoe UI", 9.5F),
                    UseVisualStyleBackColor = true
                };
                btn.Click += (s, e) => tool.OnClick();
                grid.Controls.Add(btn, tool.Col, tool.Row);
            }

            this.Controls.Add(grid);

            // Exit Button
            var btnExit = new Button
            {
                Text = "Exit",
                Size = new Size(100, 32),
                Location = new Point((this.ClientSize.Width - 100) / 2, 390),
                Font = new Font("Segoe UI", 9.5F)
            };
            btnExit.Click += (s, e) => this.Close();
            this.Controls.Add(btnExit);
        }

        private void OpenTool(string toolName)
        {
            MessageBox.Show($"{toolName} popup will open here.", "Coming Soon");
        }

        private void OpenSplitOrReorder()
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select a PDF File",
                Filter = "PDF Files (*.pdf)|*.pdf",
                Multiselect = false
            };

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    using var dlg = new SplitReorderForm(ofd.FileName);
                    dlg.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open PDF:\n{ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}