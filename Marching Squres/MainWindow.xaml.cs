using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
#region C
using DRV = System.Windows.Media.DrawingVisual;
using DC = System.Windows.Media.DrawingContext;
using System.Diagnostics;
#endregion

namespace Marching_Squres
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double SCALE = 1440 / 256d;
        byte[] pixels;
        int w;
        int h;
        DRV drv;

        public MainWindow()
        {
            InitializeComponent();
            init();
        }

        private void init()
        {
            BitmapImage bmp = new BitmapImage(new Uri(@"pack://application:,,,/example.bmp"));
            img.Source = bmp;
            int stride = bmp.PixelWidth * bmp.Format.BitsPerPixel / 8;
            byte[] pixels = new byte[bmp.PixelHeight * stride];
            bmp.CopyPixels(pixels, stride, 0);

            drc.Width = stride * SCALE;
            drc.Height = bmp.PixelHeight * SCALE;

            DRV drv = new DRV();
            drc.AddVisual(drv);

            this.pixels = pixels;
            w = stride;
            h = bmp.PixelHeight;
            this.drv = drv;

            using (var dc = drv.RenderOpen())
            {
                for (int i = 0; i < 256; i += 16)
                {
                    process(pixels, stride, bmp.PixelHeight, i, dc);
                }
            }
        }

        private void process(byte[] pixels, int w, int h, double th, DC dc)
        {
            Pen p = new Pen(new SolidColorBrush(Color.FromRgb((byte)th, (byte)(0x99 + th / 4), (byte)(0xFF - th))), 8);
            p.Freeze();

            /*
             * *************************************************************************************
             *    并行For循环或许不是最快的方法，需要涉及到线程调度
             *    当数据量足够大时可以考虑使用此方法
             *    使用指针也许会更快，但由于当初考虑在Java中使用，因此C#版本仅作演示，这里也不使用指针
             * *************************************************************************************
             * */

            //FragMeta[] meta = new FragMeta[(w - 1) * (h - 1)];
            //Parallel.For(1, w * h, i =>
            //{
            //    if (i < w || i % w == 0) return;
            //    int r = i / w;
            //    int c = i % w;
            //    Vertex v = Vertex.NONE;
            //    int p1 = pixels[(r - 1) * w + c - 1];
            //    int p2 = pixels[(r - 1) * w + c];
            //    int p3 = pixels[r * w + c - 1];
            //    int p4 = pixels[r * w + c];
            //    v |= pixels[(r - 1) * w + c - 1] > th ? Vertex.LEFT_TOP : Vertex.NONE;
            //    v |= pixels[(r - 1) * w + c] > th ? Vertex.RIGHT_TOP : Vertex.NONE;
            //    v |= pixels[r * w + c - 1] > th ? Vertex.LEFT_BOTTOM : Vertex.NONE;
            //    v |= pixels[r * w + c] > th ? Vertex.RIGHT_BOTTOM : Vertex.NONE;

            //    meta[(r - 1) * (w - 1) + (c - 1)] = new FragMeta(c, r, v, p);
            //});

            //return meta;

            //为了保证速度，在不包括绘图的算法层面已经经过性能优化
            //我们几乎摒弃所有乘除运算，只保留加减和位运算
            Vertex v;
            int head = 0, idx;
            byte l1, l2, r1, r2;
            for (int r = 1; r < h; r++)
            {
                l1 = pixels[head];
                head += w;
                l2 = pixels[head];
                for (int c = 1; c < w; c++)
                {
                    v = Vertex.NONE;
                    idx = head + c;

                    r1 = pixels[idx - w];
                    r2 = pixels[idx];
                    if (l1 > th) v |= Vertex.LEFT_TOP;
                    if (r1 > th) v |= Vertex.RIGHT_TOP;
                    if (l2 > th) v |= Vertex.LEFT_BOTTOM;
                    if (r2 > th) v |= Vertex.RIGHT_BOTTOM;
                    l1 = r1;
                    l2 = r2;

                    drawIsoLine(c, r, v, p, dc);
                }
            }
        }

        private void drawIsoLine(int x, int y, Vertex v, Pen p, DC dc)
        {
            switch ((int)v)
            {
                case 0b0001:
                    dc.DrawLine(p, machining(new Point(x - .5, y)), machining(new Point(x, y + .5)));
                    break;
                case 0b0010:
                    dc.DrawLine(p, machining(new Point(x, y + .5)), machining(new Point(x + .5, y)));
                    break;
                case 0b0011:
                    dc.DrawLine(p, machining(new Point(x - .5, y)), machining(new Point(x + .5, y)));
                    break;
                case 0b0100:
                    dc.DrawLine(p, machining(new Point(x, y - .5)), machining(new Point(x + .5, y)));
                    break;
                case 0b0101:
                    dc.DrawLine(p, machining(new Point(x - .5, y)), machining(new Point(x, y - .5)));
                    dc.DrawLine(p, machining(new Point(x, y + .5)), machining(new Point(x + .5, y)));
                    break;
                case 0b0110:
                    dc.DrawLine(p, machining(new Point(x, y - .5)), machining(new Point(x, y + .5)));
                    break;
                case 0b0111:
                    dc.DrawLine(p, machining(new Point(x - .5, y)), machining(new Point(x, y - .5)));
                    break;
                case 0b1000:
                    dc.DrawLine(p, machining(new Point(x - .5, y)), machining(new Point(x, y - .5)));
                    break;
                case 0b1001:
                    dc.DrawLine(p, machining(new Point(x, y - .5)), machining(new Point(x, y + .5)));
                    break;
                case 0b1010:
                    dc.DrawLine(p, machining(new Point(x - .5, y)), machining(new Point(x, y + .5)));
                    dc.DrawLine(p, machining(new Point(x, y - .5)), machining(new Point(x + .5, y)));
                    break;
                case 0b1011:
                    dc.DrawLine(p, machining(new Point(x, y - .5)), machining(new Point(x + .5, y)));
                    break;
                case 0b1100:
                    dc.DrawLine(p, machining(new Point(x - .5, y)), machining(new Point(x + .5, y)));
                    break;
                case 0b1101:
                    dc.DrawLine(p, machining(new Point(x, y + .5)), machining(new Point(x + .5, y)));
                    break;
                case 0b1110:
                    dc.DrawLine(p, machining(new Point(x - .5, y)), machining(new Point(x, y + .5)));
                    break; 
            }
        }

        private Point machining(Point pnt)
        {
            pnt.X *= SCALE;
            pnt.Y *= SCALE;
            return pnt;
        }

        [Flags]
        private enum Vertex
        {
            NONE           = 0,
            LEFT_TOP       = 8,
            RIGHT_TOP      = 4,
            RIGHT_BOTTOM   = 2,
            LEFT_BOTTOM    = 1
        }

        private void slider_VC(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            using (var dc = drv.RenderOpen())
            {
                process(pixels, w, h, slider.Value, dc);
            }
        }

        //private class FragMeta
        //{
        //    public int x { get; }
        //    public int y { get; }
        //    public Vertex v { get; }
        //    public Pen p { get; }

        //    public FragMeta(int x, int y, Vertex v, Pen p)
        //    {
        //        this.x = x;
        //        this.y = y;
        //        this.v = v;
        //        this.p = p;
        //    }
        //}
    }
}
