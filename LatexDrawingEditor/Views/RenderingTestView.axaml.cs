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
using System.Collections.Generic;
using SkiaSharp;

namespace LatexDrawingEditor.Views
{
    public partial class RenderingTestView : UserControl
    {
        static float mouseX = 0; // todo change to double
        static float mouseY = 0;
        static bool mouseHeld = false;

        public RenderingTestView() {

            ClipToBounds = true;

            this.PointerMoved += (sender, args) =>
            {
                var point = args.GetCurrentPoint(this);
                mouseX = (float)point.Position.X;
                mouseY = (float)point.Position.Y;
                if (mouseHeld)
                {
                    fOffsetX -= (mouseX - fStartPanX);
                    fOffsetY -= (mouseY - fStartPanY);

                    fStartPanX = mouseX;
                    fStartPanY = mouseY;
                }
            };

            this.PointerPressed += (sender, args) =>
            {
                var point = args.GetCurrentPoint(this);
                if (point.Properties.IsLeftButtonPressed) {
                    fStartPanX = (float)point.Position.X;
                    fStartPanY = (float)point.Position.Y;
                    mouseHeld = true;
                }
            };

            this.PointerReleased += (sender, args) =>
            {
                var point = args.GetCurrentPoint(this);
                if (point.Properties.PointerUpdateKind == Avalonia.Input.PointerUpdateKind.LeftButtonReleased)
                {
                    mouseHeld = false;
                }
            };


            InitializeComponent();
        }

        static float fOffsetX = 0;
        static float fOffsetY = 0;

        static float fStartPanX = 0;
        static float fStartPanY = 0;


        static (int x, int y) WorldToScreen(float fworldX, float fworldY) {
            return ((int)(fworldX - fOffsetX), (int)(fworldY - fOffsetY));
        }

        static (float x, float y) ScreenToWorld(float fscreenX, float fscreenY) {
            return ((fscreenX + fOffsetX), (fscreenY + fOffsetY));
        }

        class CustomDrawOp : ICustomDrawOperation
        {
            public void Dispose() { /*No-op*/ }

            public Rect Bounds { get; }

            public CustomDrawOp(Rect bounds) {
                Bounds = bounds;
            }
            public bool HitTest(Point p) => false;
            public bool Equals(ICustomDrawOperation other) => false;


            public List<(int x, int y)> verts = new List<(int x, int y)>() { (50, 40), (10, 10), (80, 0)};

            private void NoSkiaFallback(IDrawingContextImpl context) {
                Dispatcher.UIThread.Post(() => {
                    using (var tmpCtx = new DrawingContext(context, false))
                    {
                        var noSkia = new FormattedText("Current rendering API is not Skia", Typeface.Default, 20, TextAlignment.Center,
                        TextWrapping.Wrap, new Size(Bounds.Width, 200));
                        // todo: fix explodes on window resize, maybe remove completely
                        tmpCtx.DrawText(new SolidColorBrush(Colors.Red), new Point(100, 10), noSkia);
                    }
                });
            }

            public void Render(IDrawingContextImpl context)
            {
                SKCanvas? canvas = (context as ISkiaDrawingContextImpl)?.SkCanvas;

                if (canvas == null)
                    NoSkiaFallback(context);
                else
                {
                    canvas.Save();

                    using (var shader = SKShader.CreatePerlinNoiseFractalNoise(0.05f, 0.05f, 4, 0))
                    using (var paint = new SKPaint { Shader = shader })

                    canvas.DrawPaint(paint);

                    var vertPaint = new SKPaint();
                    vertPaint.StrokeWidth = 5;
                    vertPaint.IsAntialias = true;

                    foreach (var vert in verts) {
                        vertPaint.Color = new (255, 0, 0);

                        (int X, int Y) screenPos = WorldToScreen(vert.x, vert.y);


                        canvas.DrawCircle((screenPos.X) + (float)Bounds.Width/2, (screenPos.Y) + (float)Bounds.Height / 2, 20, vertPaint);
                    }

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
            context.Custom(new CustomDrawOp(new Rect(0, 0, Bounds.Width, Bounds.Height)));
            Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
        }


    }
}
