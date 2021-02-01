using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.Numerics;
using System.Security.Cryptography;
using System.Xml.Schema;
using MathNet.Numerics;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;
using Window = System.Windows.Window;

namespace Mandelbrot
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[] pixels;
        private PixelFormat pf = PixelFormats.Bgr32;
        private int width = 800;
        private int height = 800;
        private double xmin = -2;
        private double xmax = 1;
        private double ymin = -1.5;
        private double ymax = 1.5;
        private List<byte[]> map;
        private int nMax = 800;
        private BitmapSource btm;
        private Rectangle rectangle;
        private DispatcherTimer timer;
        private Point positionI;

        public MainWindow()
        {
            InitializeComponent();
            map = new List<byte[]>();
            Mapping();
            MandelbrotColor();
        }

        private int suiteMColor(Complex c)
        {
            Complex z = new Complex(0,0);
            int n = 0;
            while (n < nMax && z.Magnitude < 2)
            {
                n++;
                z = z * z + c;
            }

            return n;
        }

        private void Mapping()
        {
            map = new List<byte[]>(); ;
            for (int i = 0; i < 800; i++)
            {
                var color = new byte[4];
                var x = (double) i / 800.0;
                color[0] = (byte)((-Math.Exp(-10 * x) + 1) * 255);
                color[1] = (byte) (((-Math.Exp(-5*x)+1)/1.1)*255);
                color[2] = (byte) ((2*x*(1-x))*255);
                color[3] = Byte.MaxValue;
                map.Add(color);
            }
        }
        /*
        BitmapEncoder encoder = new PngBitmapEncoder();
        var fileStream = new FileStream("mandel.png",FileMode.Create);
        encoder.Frames.Add(BitmapFrame.Create(btm));
        encoder.Save(fileStream);*/

        private void MandelbrotColor()
        {
            var listeX = Generate.LinearSpaced(width, xmin, xmax);
            var listeY = Generate.LinearSpaced(height, ymin, ymax);
            byte black = Byte.MinValue;
            byte white = Byte.MaxValue;
            pixels = new byte[width * height * 4];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Complex c = new Complex(listeX[j],listeY[i]);
                    var n = suiteMColor(c);
                    var color = map[n%800];
                    for (int k = 0; k < 4; k++)
                    {
                        pixels[(i * width + j) * 4 + k] = color[k];
                    }
                }
            }
            btm = BitmapSource.Create(width, height, 96d, 96d, pf, null, pixels, width * 4);
            canvas.Source = btm;
        }

        private void BtnDown(object sender, MouseButtonEventArgs e)
        {
            rectangle = new Rectangle();
            rectangle.Stroke = Brushes.AliceBlue;
            positionI = e.GetPosition(dessin);
            Canvas.SetTop(rectangle,positionI.Y);
            Canvas.SetLeft(rectangle,positionI.X);
            dessin.Children.Add(rectangle);
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += UpdateRect;
            timer.Start();
        }

        private void UpdateRect(object sender, EventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                Point positionF = Mouse.GetPosition(dessin);
                var w = positionF.X - positionI.X;
                var h = positionF.Y - positionI.Y;
                var pad = Math.Max(w, h);
                if (pad > 0)
                {
                    rectangle.Width = pad;
                    rectangle.Height = pad;
                }
            }
            else
            {
                timer.Stop();
                dessin.Children.Clear();
                SetCoord();
                MandelbrotColor();
            }
        }

        private void SetCoord()
        {
            var padx = xmax - xmin;
            var pady = ymax - ymin;
            xmin = xmin + padx * (positionI.X / dessin.ActualWidth);
            xmax = xmin + padx * (rectangle.Height / dessin.ActualWidth);
            ymin = ymin + pady * (positionI.Y / dessin.ActualHeight);
            ymax = ymin + pady * (rectangle.Height / dessin.ActualHeight);
            nMax += (int) (1-(rectangle.Height / dessin.ActualHeight)) * 100;
        }
    }
}
