using SkiaSharp;
using SkiaSharp.HarfBuzz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using HB = HarfBuzzSharp;

namespace HarfBuzz.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int Iterations = 1000;
        private const string SampleText = "This is a sample line of text to draw.";
        private const string FontFace = "Arial";
        private new const float FontSize = 18;

        private static readonly SKTypeface _typeFace = SKTypeface.FromFamilyName(FontFace);
        private static readonly SKFont _font = _typeFace.ToFont();
        private static readonly SKPaint _paint = new SKPaint(_font)
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            TextAlign = SKTextAlign.Left,
            TextSize = FontSize,
        };

        private static readonly SKShaper _hbShaper = new SKShaper(SKTypeface.FromFamilyName(FontFace));

        private SKBitmap _swapBitmap;
        private SKCanvas _swapCanvas;

        public MainWindow()
        {
            _swapBitmap = new SKBitmap(300, 100, true);
            _swapCanvas = new SKCanvas(_swapBitmap);

            InitializeComponent();
        }

        private void OnBareSkiaSharpTestClick(object sender, RoutedEventArgs e)
        {
            bool clear = ClearCanvasCheckbox.IsChecked == true;

            Stopwatch sw = new();
            sw.Start();

            for (int i = 0; i < Iterations; i++)
            {
                if (clear)
                    _swapCanvas.Clear(SKColors.White);

                _swapCanvas.DrawText(SampleText, 0, _paint.TextSize, _paint);
            }

            sw.Stop();

            ResultText.Text = $"Elapsed time for {Iterations} iterations: {sw.ElapsedMilliseconds}ms";
            DrawingElement.InvalidateVisual();
        }

        private void OnHarfBuzzShapingTest(object sender, RoutedEventArgs e)
        {
            bool clear = ClearCanvasCheckbox.IsChecked == true;

            Stopwatch sw = new();
            sw.Start();

            for (int i = 0; i < Iterations; i++)
            {
                if (clear)
                    _swapCanvas.Clear(SKColors.White);

                var result = _hbShaper.Shape(SampleText, 0, FontSize, _paint);

                using var builder = new SKTextBlobBuilder();
                var run = builder.AllocatePositionedRun(_paint.ToFont(), result.Codepoints.Length);

                // copy the glyphs
                var glyphs = run.GetGlyphSpan();
                var positions = run.GetPositionSpan();

                for (int g = 0; g < result.Codepoints.Length; g++)
                {
                    glyphs[g] = (ushort)result.Codepoints[g];
                    positions[g] = result.Points[g];
                }

                // build
                using var textBlob = builder.Build();

                // draw the text
                _swapCanvas.DrawText(textBlob, 0, 0, _paint);
            }

            sw.Stop();

            ResultText.Text = $"Elapsed time for {Iterations} iterations: {sw.ElapsedMilliseconds}ms";
            DrawingElement.InvalidateVisual();
        }

        private void OnPaintSurface(object sender, SkiaSharp.Views.Desktop.SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Blue);
            canvas.DrawBitmap(_swapBitmap, 0, 0);
        }
    }
}