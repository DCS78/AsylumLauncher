using System.Collections.Concurrent;

namespace AsylumLauncher.Data.Display
{
    internal class ImageTooltip : ToolTip
    {
        private static readonly ConcurrentDictionary<Control, Image> ImageCache = new();

        public ImageTooltip()
        {
            OwnerDraw = true;
            Popup += new PopupEventHandler(OnPopup);
            Draw += new DrawToolTipEventHandler(OnDraw);
        }

        private void OnPopup(object sender, PopupEventArgs e)
        {
            e.ToolTipSize = new Size(512, 512);
        }

        private void OnDraw(object sender, DrawToolTipEventArgs e)
        {
            using Graphics g = e.Graphics;
            using SolidBrush b = new(Color.LightGray);
            g.FillRectangle(b, e.Bounds);

            if (e.AssociatedControl != null)
            {
                var img = GetCachedImage(e.AssociatedControl);
                g.DrawImage(img, 0, 0);
            }
        }

        private static Image GetCachedImage(Control control)
        {
            return ImageCache.GetOrAdd(control, ctrl =>
            {
                var img = SelectImage(ctrl);
                var scaledImg = ScaleImage(img, 512, 512);
                img.Dispose();
                return scaledImg;
            });
        }

        private static Image SelectImage(Control control)
        {
            var imageMap = new Dictionary<Control, Image>
            {
                { Program.MainWindow.DefaultColorButton, Properties.Resources.Default_2 },
                { Program.MainWindow.NoirColorButton, Properties.Resources.Monochrome_2 },
                { Program.MainWindow.MutedColorButton, Properties.Resources.Muted_2 },
                { Program.MainWindow.LowContrastColorButton, Properties.Resources.Log_1_2 },
                { Program.MainWindow.VividColorButton, Properties.Resources.Log_2_2 }
            };

            return imageMap.TryGetValue(control, out var image) ? image : Properties.Resources.High_Contrast_2;
        }

        private static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(maxWidth, maxHeight);
            using (var graphics = Graphics.FromImage(newImage))
            {
                // Calculate x and y which center the image
                int y = maxHeight / 2 - newHeight / 2;
                int x = maxWidth / 2 - newWidth / 2;

                // Draw image on x and y with newWidth and newHeight
                graphics.DrawImage(image, x, y, newWidth, newHeight);
            }

            return newImage;
        }
    }
}
