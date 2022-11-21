using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;

namespace LatexDrawingEditor.Views
{
    public class RenderingTestView : UserControl
    {
        public RenderingTestView() {
            ClipToBounds = true;
        }

        class CustomDrawOp : ICustomDrawOperation
        {
            private readonly FormattedText _noSkiaText;
            public CustomDrawOp(Rect bounds, FormattedText noSkia)
            {
                _noSkiaText = noSkia;
                Bounds = bounds;
            }

            public void Dispose() { /*No-op*/ }

            public Rect Bounds { get; }
            public bool HitTest(Point p) => false;
            public bool Equals(ICustomDrawOperation other) => false;

            static Stopwatch St = Stopwatch.StartNew();

            public async void Render(IDrawingContextImpl context)
            {
                SKCanvas? canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;

                if (canvas == null)
                {

                    Dispatcher.UIThread.Post(() => {
                        using (var tmpCtx = new DrawingContext(context, false))
                        {
                            FormattedText noSkia = _noSkiaText;
                            var col = new SolidColorBrush(Colors.Red);
                            var pos = new Point(100, 10);
                            tmpCtx.DrawText(col, pos, noSkia);
                        }
                    });

                }
                else
                {
                    canvas.Save();
                    // create the first shader
                    var colors = new SKColor[] {
                        new SKColor(0, 255, 255),
                        new SKColor(255, 0, 255),
                        new SKColor(255, 255, 0),
                        new SKColor(0, 255, 255)
                    };

                    var lightPosition = new SKPoint(
                        (float)(Bounds.Width / 2 + Math.Cos(St.Elapsed.TotalSeconds) * Bounds.Width / 4),
                        (float)(Bounds.Height / 2 + Math.Sin(St.Elapsed.TotalSeconds) * Bounds.Height / 4));

                    //  using (var sweep = SKShader.CreateSweepGradient(new SKPoint((int)Bounds.Width / 2, (int)Bounds.Height / 2), colors, null))

                    using (var shader = SKShader.CreatePerlinNoiseFractalNoise(0.05f, 0.05f, 4, 0))
                    //using (var shader = SKShader.CreateCompose(sweep, turbulence, SKBlendMode.SrcATop))


                    using (var paint = new SKPaint { Shader = shader })

                        canvas.DrawPaint(paint);

                    using (var pseudoLight = SKShader.CreateRadialGradient(
                        lightPosition,
                        (float)(Bounds.Width / 4),
                        new[] {
                            new SKColor(255, 200, 200, 100),
                            new SKColor(40,40,40, 250)
                        },
                        SKShaderTileMode.Clamp))
                    using (var paint = new SKPaint
                    {
                        Shader = pseudoLight
                    })
                        canvas.DrawPaint(paint);

                    RenderTest(canvas);
                    canvas.Restore();
                }
            }
            static Random rand = new Random();
            static int width = 400;
            static int height = 500;
            private void RenderTest(SKCanvas surface)
            {
                byte alpha = 128;
                var paint = new SKPaint();
                paint.StrokeWidth = 5;
                paint.IsAntialias = true;

                for (int i = 0; i < 1_0000; i++)
                {
                    float x1 = (float)(rand.NextDouble() * width);
                    float x2 = (float)(rand.NextDouble() * width);
                    float y1 = (float)(rand.NextDouble() * height);
                    float y2 = (float)(rand.NextDouble() * height);

                    paint.Color = new SKColor(
                        red: (byte)(rand.NextDouble() * 255),
                        green: (byte)(rand.NextDouble() * 255),
                        blue: (byte)(rand.NextDouble() * 255),
                        alpha: alpha
                    );

                    surface.DrawLine(x1, y1, x2, y2, paint);
                }

            }
        }


        public override void Render(DrawingContext context)
        {

            var noSkia = new FormattedText("Current rendering API is not Skia", Typeface.Default, 20, TextAlignment.Center,
            TextWrapping.Wrap, new Size(Bounds.Width, 200));

            context.Custom(new CustomDrawOp(new Rect(0, 0, Bounds.Width, Bounds.Height), noSkia));

            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }


    }
}
