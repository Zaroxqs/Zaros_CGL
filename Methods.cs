using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace Custom_games_launcher
{
    public static class Methods
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        public static string SaveFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Zaros_CGL",
            "gamelist.txt");

        public static string BgImageFilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Zaros_CGL",
            "bgimage.txt");

        public static List<string> LoadGameList()
        {
            if (File.Exists(SaveFilePath))
                return new List<string>(File.ReadAllLines(SaveFilePath));
            else
                return new List<string>();
        }

        public static void ButtonStyle(Button btn, Color normalColor, Color downColor)
        {
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = normalColor;
            btn.FlatAppearance.MouseDownBackColor = downColor;
        }

        public static void SaveGameList(IEnumerable<string> gameNames)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SaveFilePath));
            File.WriteAllLines(SaveFilePath, gameNames);
        }

        public static Image LoadBackgroundImage()
        {
            if (File.Exists(BgImageFilePath))
            {
                string imgPath = File.ReadAllText(BgImageFilePath);
                if (File.Exists(imgPath))
                {
                    try
                    {
                        return new Bitmap(imgPath);
                    }
                    catch { }
                }
            }
            return null;
        }

        public static void SaveBackgroundImage(string imgPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(BgImageFilePath));
            File.WriteAllText(BgImageFilePath, imgPath);
        }

        public static void ClearListFile()
        {
            try
            {
                if (File.Exists(SaveFilePath)) File.Delete(SaveFilePath);
            }
            catch { }
        }

        public static void ClearBgFile()
        {
            try
            {
                if (File.Exists(BgImageFilePath)) File.Delete(BgImageFilePath);
            }
            catch { }
        }

        public static void Btn_Click(object sender, EventArgs e)
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

        public static GraphicsPath GetRoundRectangle(Rectangle rect, int radius)
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
}