using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Custom_games_launcher
{
    public partial class Launcher : Form
    {

        private List<string> gameNames = new List<string>();

        public Launcher()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public class RoundedButton : Button
        {
            public int CornerRadius { get; set; } = 20;

            protected override void OnPaint(PaintEventArgs pevent)
            {
                base.OnPaint(pevent);
                var rect = this.ClientRectangle;
                var path = Methods.GetRoundRectangle(rect, CornerRadius);
                this.Region = new Region(path);

                using (var pen = new Pen(this.BackColor, 2))
                {
                    pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    pevent.Graphics.DrawPath(pen, path);
                }
            }
        }

        private void Launcher_Load(object sender, EventArgs e)
        {
            // round corners
            var path = Methods.GetRoundRectangle(new Rectangle(0, 0, this.Width, this.Height), 30);
            this.Region = new Region(path);

            // making buttons not ugly
            Color downColor = Color.FromArgb(90, 0, 120);
            Methods.ButtonStyle(ExitButton, ExitButton.BackColor, downColor);
            Methods.ButtonStyle(FolderButton, FolderButton.BackColor, downColor);
            Methods.ButtonStyle(ClearButton, ClearButton.BackColor, downColor);
            Methods.ButtonStyle(BackgroundButton, ClearButton.BackColor, downColor);

            // load stuff
            gameNames = Methods.LoadGameList();
            GenerateButtons();
            this.BackgroundImage = Methods.LoadBackgroundImage();
        }

        private void GenerateButtons()
        {
            PanelButtons.Controls.Clear();
            int btnSize = 100;
            int margin = 20;
            int x = margin;
            int y = margin;
            int buttonsPerRow = (PanelButtons.Width - margin) / (btnSize + margin);

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
                                if (icon != null) iconBitmap = icon.ToBitmap();
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

                btn.Click +=  Methods.Btn_Click;

                PanelButtons.Controls.Add(btn);

                x += btnSize + margin;
                if ((i + 1) % buttonsPerRow == 0)
                {
                    x = margin;
                    y += btnSize + margin;
                }
            }
        }


        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(this.Handle, Methods.WM_NCLBUTTONDOWN, Methods.HTCAPTION, 0);
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void BackgroundButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select Image";
                dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        BackgroundImage?.Dispose();
                        BackgroundImage = new Bitmap(dialog.FileName);
                        Methods.ClearBgFile();
                        Methods.SaveBackgroundImage(dialog.FileName);
                    }
                    catch { }
                }
            }
        }

        private void FolderButton_Click(object sender, EventArgs e)
        {
                    using (var dialog = new FolderBrowserDialog())
                    {
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            gameNames.Clear();
                            string[] subfolders = Directory.GetDirectories(dialog.SelectedPath);
                            gameNames.AddRange(subfolders);
                            Methods.SaveGameList(gameNames);
                            GenerateButtons();
                        }
                    }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            Methods.ClearListFile();
            Methods.ClearBgFile();
            PanelButtons.Controls?.Clear();
            BackgroundImage = null;
        }



        // ... [other event handlers, using Methods where needed] ...
    }
}