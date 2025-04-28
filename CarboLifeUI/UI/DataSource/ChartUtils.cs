using SkiaSharp;
using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CarboLifeUI.UI
{
    public static class ChartUtils
    {
        public static Bitmap ControlToImage(Visual target, double dpiX, double dpiY)
        {
            if (target == null)
            {
                return null;
            }
            // render control content
            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)(bounds.Width * dpiX / 96.0), //(bounds.Width * dpiX / 96.0),
                                                            (int)(bounds.Height * dpiY / 96.0), //(bounds.Height * dpiY / 96.0)
                                                            dpiX,
                                                            dpiY,
                                                            PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext ctx = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(target);
                ctx.DrawRectangle(vb, null, new Rect(new System.Windows.Point(), bounds.Size));
            }
            rtb.Render(dv);

            //convert image format
            MemoryStream stream = new MemoryStream();
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(stream);

            return new Bitmap(stream);
        }

        internal static Bitmap CanvasToImage(Canvas target, int v1, int v2)
        {
            if (target == null)
            {
                return null;
            }
            Bitmap myBitmap = null;

            // Measure and arrange the canvas
            System.Windows.Size size = new System.Windows.Size(target.ActualWidth, target.ActualHeight);
            target.Measure(size);
            target.Arrange(new Rect(size));
            target.UpdateLayout();
            target.ClipToBounds = true;
            target.Background = System.Windows.Media.Brushes.White;

            // Create the bitmap
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)size.Width, (int)size.Height, 96d, 96d, PixelFormats.Pbgra32);

            renderBitmap.Render(target);
            
            // Encode to PNG
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using (Stream s = new MemoryStream())
            {
                encoder.Save(s);
                myBitmap = new Bitmap(s);
            }

            if (myBitmap != null)
            {
                return myBitmap;
            }
            else
            {
                return null;
            }

        }
    }
}