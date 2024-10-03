using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace TransparentOverlay
{
    public partial class Form1 : Form
    {
        private int lineLength = 192;
        private Color lineColor = Color.Red;
        private float lineWidth = 5f;
        private float angle = 45f;

        private NumericUpDown lengthInput;
        private NumericUpDown widthInput;
        private NumericUpDown angleInput;
        private bool inputsVisible = false;

        private Panel configPanel;

        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Lime;
            this.TransparencyKey = Color.Lime;
            this.WindowState = FormWindowState.Maximized;

            this.Paint += new PaintEventHandler(Form1_Paint);
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);

            CreateControls();
            LoadConfigurations();
        }

        private void CreateControls()
        {
            Label lengthLabel = new Label() { Text = "Line Length:", Location = new Point(10, 10), AutoSize = true };
            lengthInput = new NumericUpDown() { Location = new Point(100, 10), Width = 60, Minimum = 10, Maximum = 500, Value = lineLength };
            lengthInput.ValueChanged += (s, e) => { lineLength = (int)lengthInput.Value; Invalidate(); };

            Label widthLabel = new Label() { Text = "Line Width:", Location = new Point(10, 40), AutoSize = true };
            widthInput = new NumericUpDown() { Location = new Point(100, 40), Width = 60, Minimum = 1, Maximum = 20, Value = (decimal)lineWidth };
            widthInput.ValueChanged += (s, e) => { lineWidth = (float)widthInput.Value; Invalidate(); };

            Label angleLabel = new Label() { Text = "Line Angle (°):", Location = new Point(10, 70), AutoSize = true };
            angleInput = new NumericUpDown() { Location = new Point(100, 70), Width = 60, Minimum = 0, Maximum = 360, Value = (decimal)angle };
            angleInput.ValueChanged += (s, e) => { angle = (float)angleInput.Value; Invalidate(); };

            lengthLabel.Visible = false;
            lengthInput.Visible = false;
            widthLabel.Visible = false;
            widthInput.Visible = false;
            angleLabel.Visible = false;
            angleInput.Visible = false;

            this.Controls.Add(lengthLabel);
            this.Controls.Add(lengthInput);
            this.Controls.Add(widthLabel);
            this.Controls.Add(widthInput);
            this.Controls.Add(angleLabel);
            this.Controls.Add(angleInput);

            configPanel = new Panel
            {
                Location = new Point(10, 100),
                Size = new Size(100, 200),
                AutoScroll = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(configPanel);
        }

        private void LoadConfigurations()
        {
            string configDirectory = "configs";
            if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }

            string[] configFiles = Directory.GetFiles(configDirectory, "*.txt");
            Console.WriteLine($"Found {configFiles.Length} configuration files.");
            int buttonY = 10;

            foreach (string file in configFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                Button configButton = new Button()
                {
                    Text = fileName,
                    Location = new Point(0, buttonY),
                    Width = configPanel.Width - 20,
                    Height = 30,
                    BackColor = Color.LightGray,
                    FlatStyle = FlatStyle.Flat
                };

                configButton.Click += (s, e) => LoadConfig(file);
                configPanel.Controls.Add(configButton);
                buttonY += 40;
            }

            if (buttonY == 10)
            {
                Console.WriteLine("No configuration buttons were added.");
            }
        }

        private void LoadConfig(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            try
            {
                foreach (string line in lines)
                {
                    var parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        var property = parts[0].Trim();
                        var value = parts[1].Trim();

                        switch (property)
                        {
                            case "Line Length":
                                lineLength = int.Parse(value);
                                lengthInput.Value = lineLength;
                                break;
                            case "Line Width":
                                lineWidth = float.Parse(value);
                                widthInput.Value = (decimal)lineWidth;
                                break;
                            case "Angle":
                                angle = float.Parse(value);
                                angleInput.Value = (decimal)angle;
                                break;
                        }
                    }
                }
                Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading config from '{filePath}': {ex.Message}");
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            int startX = this.ClientSize.Width / 2;
            int startY = this.ClientSize.Height / 2;

            float radianAngle = angle * (float)Math.PI / 180f;
            int endX = startX + (int)(lineLength * Math.Cos(radianAngle));
            int endY = startY + (int)(lineLength * Math.Sin(radianAngle));

            DrawBloomEffect(g, startX, startY, endX, endY, lineWidth, lineColor);

            using (Pen pen = new Pen(lineColor, lineWidth))
            {
                g.DrawLine(pen, startX, startY, endX, endY);
            }
        }

        private void DrawBloomEffect(Graphics g, int startX, int startY, int endX, int endY, float lineWidth, Color lineColor)
        {
            using (Pen pen = new Pen(lineColor, lineWidth))
            {
                g.DrawLine(pen, startX, startY, endX, endY);
            }

            using (Brush glowBrush = new SolidBrush(Color.FromArgb(50, lineColor.R, lineColor.G, lineColor.B)))
            {
                using (GraphicsPath path = new GraphicsPath())
                {
                    path.AddLine(startX, startY, endX, endY);
                    g.FillPath(glowBrush, path);
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.Alt && e.Shift)
            {
                inputsVisible = !inputsVisible;
                ToggleInputs(inputsVisible);
                e.Handled = true;
            }
        }

        private void ToggleInputs(bool visible)
        {
            foreach (Control control in this.Controls)
            {
                if (control is Label || control is NumericUpDown)
                {
                    control.Visible = visible;
                }
            }

            configPanel.Visible = visible;
            Invalidate();
        }
    }
}
