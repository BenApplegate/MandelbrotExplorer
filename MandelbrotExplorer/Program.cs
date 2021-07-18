using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MandelbrotExplorer
{
    class Program
    {
        
        [DllImport("Mandelbrot.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr runMandelbrot(int _width, int _height, double scale, double offsetX, double offsetY, int iterations, int threshold);

        [DllImport("Mandelbrot.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void clearMem(IntPtr ptr);
        
        private static Form _form;
        private static Label _text;

        private static double _scale = 5;
        private static int _scrolls = 0;
        private static double _focusX = 0;
        private static double _focusY = 0;
        private static int _iterations = 5000;
        
        static void Main(string[] args)
        {
            _form = new Form();
            _text = new Label();
            _form.Size = new Size(1920, 1080);
            _form.Paint += OnPaint;
            _form.MouseWheel += OnScroll;
            _form.KeyDown += KeyDownEvent;
            _form.FormBorderStyle = FormBorderStyle.None;
            _form.TopMost = true;
            
            _text.AutoSize = true;
            _text.Location = new Point(50, 50);
            _text.Font = new Font(FontFamily.GenericSansSerif, 10);
            _form.Controls.Add(_text);
            
            Application.Run(_form);
        }

        static void KeyDownEvent(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Escape)
            {
               _form.Close();
               return;
            }
            else if (e.KeyCode == Keys.R)
            {
                _focusX = 0;
                _focusY = 0;
                _scale = 5;
                _scrolls = 0;
                _iterations = 250;
                _form.Invalidate();
            }
        }
        static void OnScroll(object sender, MouseEventArgs e)
        {
            
            if (e.Delta > 120)
            {
                return;
            }
            
            int x, y;
            double pointedX, pointedY;
            x = e.X - (_form.ClientSize.Width / 2);
            y = e.Y - (_form.ClientSize.Height / 2);

            pointedX = x / (_form.ClientSize.Width / _scale) + _focusX;
            pointedY = y / (_form.ClientSize.Width / _scale) + _focusY;

            _focusX = (pointedX + _focusX) / 2;
            _focusY = (pointedY + _focusY) / 2;
            
            _scrolls += e.Delta * 2;
            _iterations = 250 + _scrolls * 5;
            if(5 * Math.Exp(-.001 * _scrolls) > 0)
                _scale = 5 * Math.Exp(-.001 * _scrolls);

            _text.Text = $"Current viewing position: ({_focusX}, {_focusY}), Current scale: {_scale}";
            
            _form.Refresh();
            
        }
        static void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.Gray, _form.ClientRectangle);
            
            Bitmap bitmap = new Bitmap(_form.ClientSize.Width, _form.ClientSize.Height);

            IntPtr ptr = runMandelbrot(_form.ClientSize.Width, _form.ClientSize.Height, _scale, _focusX, _focusY, _iterations, 4);
            int[] tempArr = new int[_form.ClientSize.Width * _form.ClientSize.Height];

            Marshal.Copy(ptr, tempArr, 0, _form.ClientSize.Width * _form.ClientSize.Height);

            for (int y = 0; y < _form.ClientSize.Height; y++)
            {
                for (int x = 0; x < _form.ClientSize.Width; x++)
                {
                    if (tempArr[(y * _form.ClientSize.Width) + x] < 0)
                    {
                        bitmap.SetPixel(x, y, Color.Black);
                    }
                    else
                    {
                        HsvToRgb(tempArr[(y * _form.ClientSize.Width) + x] * 2, 1, 1, out int r, out int g, out int b);
                        bitmap.SetPixel(x, y, Color.FromArgb(r,g,b));
                    }
                }
            }
            clearMem(ptr);
            
            e.Graphics.DrawImage(bitmap, Point.Empty);
            
        }
        
        static void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
        {    
            double H = h;
            while (H < 0) { H += 360; };
            while (H >= 360) { H -= 360; };
            double R, G, B;
            if (V <= 0)
            { R = G = B = 0; }
            else if (S <= 0)
            {
                R = G = B = V;
            }
            else
            {
                double hf = H / 60.0;
                int i = (int)Math.Floor(hf);
                double f = hf - i;
                double pv = V * (1 - S);
                double qv = V * (1 - S * f);
                double tv = V * (1 - S * (1 - f));
                switch (i)
                {

                    // Red is the dominant color

                    case 0:
                        R = V;
                        G = tv;
                        B = pv;
                        break;

                    // Green is the dominant color

                    case 1:
                        R = qv;
                        G = V;
                        B = pv;
                        break;
                    case 2:
                        R = pv;
                        G = V;
                        B = tv;
                        break;

                    // Blue is the dominant color

                    case 3:
                        R = pv;
                        G = qv;
                        B = V;
                        break;
                    case 4:
                        R = tv;
                        G = pv;
                        B = V;
                        break;

                    // Red is the dominant color

                    case 5:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                    case 6:
                        R = V;
                        G = tv;
                        B = pv;
                        break;
                    case -1:
                        R = V;
                        G = pv;
                        B = qv;
                        break;

                    // The color is not defined, we should throw an error.

                    default:
                        //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                        R = G = B = V; // Just pretend its black/white
                        break;
                }
            }
            r = Clamp((int)(R * 255.0));
            g = Clamp((int)(G * 255.0));
            b = Clamp((int)(B * 255.0));
            
            int Clamp(int i)
            {
                if (i < 0) return 0;
                if (i > 255) return 255;
                return i;
            }
        }
    }
}