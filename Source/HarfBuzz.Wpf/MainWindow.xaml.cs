using SkiaSharp;
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
        private const string Text = "This is a sample line of text to draw.";
        private const string FontFace = "Arial";
        private new const int FontSize = 12;

        private static SKFont _font = SKTypeface.FromFamilyName(FontFace).ToFont();
        private static SKPaint _paint = new SKPaint(_font)
        {
            Color = SKColors.Black,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            TextAlign = SKTextAlign.Left,
            TextSize = FontSize,
        };

        private static SKBitmap _swapBitmap = new SKBitmap(300, 100, true);
        private static SKCanvas _swapCanvas = new SKCanvas(_swapBitmap);

        private static HB.Font _hbFont = new HB.Font(new HB.Face(HB.Blob.FromFile(Environment.GetFolderPath(Environment.SpecialFolder.Fonts) + "\\arial.ttf"), 0));

        private static HB.Buffer _hbBuffer = new();

        public MainWindow()
        {
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

                _swapCanvas.DrawText(Text, 0, _paint.TextSize, _paint);
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

                _hbBuffer.ClearContents();
                _hbBuffer.Direction = HB.Direction.LeftToRight;
                _hbBuffer.AddUtf16(Text);
                _hbFont.Shape(_hbBuffer);

                var glyphsInfos = _hbBuffer.GetGlyphInfoSpan();
                var glyphPositions = _hbBuffer.GetGlyphPositionSpan();

                int x = FontSize;
                int y = 0;

                for (int g = 0; g < glyphsInfos.Length; g++)
                {
                    var info = glyphsInfos[g];
                    var position = glyphPositions[g];

                    _swapCanvas.DrawText(char.ConvertFromUtf32((int)info.Codepoint), x + position.XOffset, y + position.YOffset, _paint);

                    x += position.XAdvance;
                    y += position.YAdvance;
                }

                _swapCanvas.DrawText(Text, 0, _paint.TextSize, _paint);
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