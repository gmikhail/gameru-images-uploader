using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GameruImagesUploader
{
    /// <summary>
    /// Логика взаимодействия для ColorPickerWindow.xaml
    /// </summary>
    public partial class ColorPickerWindow : Window
    {
        internal Color color;

        public ColorPickerWindow()
        {
            InitializeComponent();
        }

        private void ImageColors_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateColor(GetColorFromImage());
            }
        }

        private void ImageColors_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UpdateColor(GetColorFromImage());
        }

        private Color GetColorFromImage()
        {
            var color = Colors.Transparent;
            try
            {
                BitmapSource bitmapSource = ImageColors.Source as BitmapSource;
                if (bitmapSource != null)
                {
                    double x = Mouse.GetPosition(ImageColors).X;
                    x *= bitmapSource.PixelWidth / ImageColors.ActualWidth;
                    if ((int)x > bitmapSource.PixelWidth - 1)
                        x = bitmapSource.PixelWidth - 1;
                    else if (x < 0) x = 0;
                    double y = Mouse.GetPosition(ImageColors).Y;
                    y *= bitmapSource.PixelHeight / ImageColors.ActualHeight;
                    if ((int)y > bitmapSource.PixelHeight - 1)
                        y = bitmapSource.PixelHeight - 1;
                    else if (y < 0) y = 0;
                    CroppedBitmap cb = new CroppedBitmap(bitmapSource, new Int32Rect((int)x, (int)y, 1, 1));
                    byte[] pixels = new byte[4];
                    cb.CopyPixels(pixels, 4, 0);
                    if (pixels[3] == byte.MaxValue)
                    {
                        color = Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Add(ex);
            }
            return color;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            string name = ((Slider)sender).Name; 
            if(name == SliderR.Name)
            {
                color.R = (byte)e.NewValue;
            }
            else if(name == SliderG.Name)
            {
                color.G = (byte)e.NewValue;
            }
            else if (name == SliderB.Name)
            {
                color.B = (byte)e.NewValue;
            }
            UpdateColor(color);
        }

        public void UpdateColor(Color color)
        {
            this.color = color;
            RectangleCurrentColor.Fill = new SolidColorBrush(color);
            SliderR.Value = color.R;
            SliderG.Value = color.G;
            SliderB.Value = color.B;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}