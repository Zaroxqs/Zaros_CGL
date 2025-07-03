using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Custom_games_launcher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        // stuff for the pannel
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private List<string> gameNames = new List<string>();

        // for persistent list
        private static string SaveFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Zaros_CGL",
            "gamelist.txt");

        // for persistent bg image
        private static string BgImageFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Zaros_CGL",
            "bgimage.txt");


        private void LoadGameList() // put the txt file for games in a list wanker
        {
            if (File.Exists(SaveFilePath))
            {
                gameNames = File.ReadAllLines(SaveFilePath).ToList();
            }
            else
            {
                gameNames = new List<string>();
            }
        }

        private void SaveGameList() // you can read probably
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SaveFilePath));
            File.WriteAllLines(SaveFilePath, gameNames);
        }

        private void LoadBackground() // methoad to load the background quite obviously
        {
            if (File.Exists(BgImageFilePath))
            {
                string imgPath = File.ReadAllText(BgImageFilePath);
                if (File.Exists(imgPath))
                {
                    try
                    {
                        this.BackgroundImage = new Bitmap(imgPath);
                    }
                    catch { }
                }
            }
        }

        // button class for the game icons
        public class RoundedButton : Button
        {
            public int CornerRadius { get; set; } = 20;

            protected override void OnPaint(PaintEventArgs pevent)
            {
                base.OnPaint(pevent);
                var rect = this.ClientRectangle;
                var path = GetRoundRectangle(rect, CornerRadius);
                this.Region = new Region(path);

                using (var pen = new Pen(this.BackColor, 2))
                {
                    pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    pevent.Graphics.DrawPath(pen, path);
                }
            }

            private GraphicsPath GetRoundRectangle(Rectangle rect, int radius)
            {
                var path = new GraphicsPath();
                path.AddArc(rect.Left, rect.Top, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Top, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.Left, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();
                return path;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            // round corners
            int radius = 30;
            var path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(new Rectangle(0, 0, radius, radius), 180, 90);
            path.AddArc(new Rectangle(this.Width - radius, 0, radius, radius), 270, 90);
            path.AddArc(new Rectangle(this.Width - radius, this.Height - radius, radius, radius), 0, 90);
            path.AddArc(new Rectangle(0, this.Height - radius, radius, radius), 90, 90);
            path.CloseFigure();
            this.Region = new Region(path);

            // button shenanigans
            ExitButton.FlatAppearance.BorderSize = 0;
            ExitButton.FlatAppearance.MouseOverBackColor = ExitButton.BackColor;
            ExitButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(90, 0, 120);
            FolderButton.FlatAppearance.BorderSize = 0;
            FolderButton.FlatAppearance.MouseOverBackColor = FolderButton.BackColor;
            FolderButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(90, 0, 120);
            ClearButton.FlatAppearance.MouseOverBackColor = ClearButton.BackColor;
            ClearButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(90, 0, 120);
            ClearButton.FlatAppearance.BorderSize = 0;
            BackgroundButton.FlatAppearance.MouseOverBackColor = ClearButton.BackColor;
            BackgroundButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(90, 0, 120);
            BackgroundButton.FlatAppearance.BorderSize = 0;

            // load stuff
            LoadGameList();
            GenerateButtons();
            LoadBackground();
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            // draggable panel
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private void FolderButton_Click(object sender, EventArgs e)
        {
            using (var d = folderBrowserDialog1)
            {
                if (d.ShowDialog() == DialogResult.OK)
                {
                    using (var dialog = new FolderBrowserDialog())
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            gameNames.Clear();
                            string[] subfolders = Directory.GetDirectories(dialog.SelectedPath);
                            gameNames.AddRange(subfolders);
                            SaveGameList();
                            GenerateButtons();
                        }
                    }
                }
            }
        }

        private void Btn_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag is ValueTuple<string, string> tag)
            {
                string exePath = tag.Item1;
                string folderPath = tag.Item2;

                try
                {
                    if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
                    {
                        System.Diagnostics.Process.Start(exePath);
                    }
                    else
                    {
                        System.Diagnostics.Process.Start("explorer.exe", folderPath);
                    }
                }
                catch
                {
                    try { System.Diagnostics.Process.Start("explorer.exe", folderPath); }
                    catch { MessageBox.Show("Could not open folder or launch executable."); }
                }
            }
        }

        private void GenerateButtons()
        {
            panelButtons.Controls.Clear();
            int btnSize = 100;
            int margin = 20;
            int x = margin;
            int y = margin;
            int buttonsPerRow = (panelButtons.Width - margin) / (btnSize + margin);

            for (int i = 0; i < gameNames.Count; i++)
            {
                string folderPath = gameNames[i];
                string folderName = Path.GetFileName(folderPath);

                string exePath = null;
                Bitmap iconBitmap = null;

                if (Directory.Exists(folderPath))
                {
                    string[] exes = Directory.GetFiles(folderPath, "*.exe");
                    if (exes.Length > 0)
                    {
                        exePath = exes[0];
                        try
                        {
                            using (Icon icon = Icon.ExtractAssociatedIcon(exePath))
                            {
                                if (icon != null)
                                    iconBitmap = icon.ToBitmap();
                            }
                        }
                        catch { /* ignore */ }
                    }
                }

                var btn = new RoundedButton
                {
                    Width = btnSize,
                    Height = btnSize,
                    CornerRadius = 15,
                    Text = "",
                    Location = new Point(x, y),
                    BackColor = Color.LightBlue,
                    FlatStyle = FlatStyle.Flat,
                    Image = iconBitmap,
                    ImageAlign = ContentAlignment.MiddleCenter,
                    TextAlign = ContentAlignment.BottomCenter,
                    Tag = (exePath, folderPath) 
                };

                new ToolTip().SetToolTip(btn, folderName);

                btn.Click += Btn_Click;

                panelButtons.Controls.Add(btn);

                x += btnSize + margin;
                if ((i + 1) % buttonsPerRow == 0)
                {
                    x = margin;
                    y += btnSize + margin;
                }
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                }
            }
            catch { }

            try
            {
                if (File.Exists(BgImageFilePath))
                {
                    File.Delete(BgImageFilePath);
                }
            }
            catch { }

            gameNames.Clear();

            panelButtons.Controls.Clear();
            this.BackgroundImage = null;
        }

        private void BackgroundButton_Click(object sender, EventArgs e)
        {
            openFileDialog1.Title = "Select Image";
            openFileDialog1.Filter = "Image Files|*.jpg;*.jpeg;*.png;";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (this.BackgroundImage != null)
                        this.BackgroundImage.Dispose();

                    this.BackgroundImage = new Bitmap(openFileDialog1.FileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(BgImageFilePath));
                    File.WriteAllText(BgImageFilePath, openFileDialog1.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception setting image:\n" + ex.Message);
                }
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void openFileDialog1_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
