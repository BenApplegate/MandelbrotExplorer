using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MandelbrotExplorer
{
    class Program
    {
        
        
        private static Form _form;

        private static double _scale = 5;
        private static double _focusX = 0;
        private static double _focusY = 0;
        
        static void Main(string[] args)
        {
            _form = new Form();
            _form.Size = new Size(1920, 1080);
            _form.Paint += OnPaint;
            _form.MouseWheel += OnScroll;
            _form.KeyDown += KeyDownEvent;
            
            Application.Run(_form);
        }

        static void KeyDownEvent(object sender, KeyEventArgs e)
        {
            
        }
        static void OnScroll(object sender, MouseEventArgs e)
        {
            
        }
        static void OnPaint(object sender, PaintEventArgs e)
        {

        }
    }
}