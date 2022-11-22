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
        static double mouseX = 0;
        static double mouseY = 0;
        static bool mouseHeld = false;

        public RenderingTestView() {

            ClipToBounds = true;

            this.PointerMoved += (sender, args) =>
            {
                var point = args.GetCurrentPoint(this);
                mouseX = point.Position.X;
                mouseY = point.Position.Y;
                if (mouseHeld) {
                    fOffsetX -= (mouseX - fStartPanX);// move only of offset set by mouseX/mouseY
                    fOffsetY -= (mouseY - fStartPanY);

                    fStartPanX = mouseX; // reset initial mouse offset each frame
                    fStartPanY = mouseY;
                }
            };

            this.PointerPressed += (sender, args) =>
            {
                var point = args.GetCurrentPoint(this);
                if (point.Properties.IsLeftButtonPressed) {
                    fStartPanX = point.Position.X;
                    fStartPanY = point.Position.Y;
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

        static double fOffsetX = 0;
        static double fOffsetY = 0;

        static double fStartPanX = 0;
        static double fStartPanY = 0;


        static (int x, int y) WorldToScreen(float fworldX, float fworldY) {
            return ((int)(fworldX - fOffsetX), (int)(fworldY - fOffsetY));
        }

        static (double x, double y) ScreenToWorld(int fscreenX, int fscreenY) {
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
                    RenderTest(canvas);
                }
            }

            static Random rand = new Random();
            static int width = 600;
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

                    (int X1, int Y1) = WorldToScreen(x1, y1);
                    (int X2, int Y2) = WorldToScreen(x2, y2);

                    surface.DrawLine(X1, Y1, X2, Y2, paint);
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
