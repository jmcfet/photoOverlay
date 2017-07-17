using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using SkiaSharp.Views.Android;
using System.Collections.Generic;
using SkiaSharp;
using Android.Graphics;
using System;

namespace PhotoOverlay
{
    [Activity(Label = "PhotoOverlay", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private Android.Hardware.Camera _camera;
        private TextureView _textureView;
        private SurfaceView _surfaceView;
        private ISurfaceHolder holder;
        SKCanvasView canvasView;
        bool MoveMode = true;
        EllipseDrawingFigure activefig = null;
        List<EllipseDrawingFigure> completedFigures = new List<EllipseDrawingFigure>();
        List<EllipseDrawingFigure> draggingFigures = new List<EllipseDrawingFigure>();
        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Fill
        };
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
             SetContentView (Resource.Layout.Main);
            _textureView = (TextureView)FindViewById(Resource.Id.textureView);
            _textureView.SurfaceTextureAvailable += _textureView_SurfaceTextureAvailable;
 //           _textureView.SurfaceTextureListener = this;
            Button addButton = FindViewById<Button>(Resource.Id.Add);
            FindViewById<Button>(Resource.Id.ShapeMove).Click += MainActivity_Click;
            canvasView = FindViewById<SKCanvasView>(Resource.Id.canvasView);
            canvasView.Touch += CanvasView_Touch;
           
            canvasView.PaintSurface += CanvasView_PaintSurface;
            addButton.Click += AddButton_Click;

        }

       

        private void AddButton_Click(object sender, System.EventArgs e)
        {
            EllipseDrawingFigure figure = new EllipseDrawingFigure
            {
                Color = SKColors.Red,
                StartPoint = ConvertToPixel(new Point(131, 132)),
                EndPoint = ConvertToPixel(new Point(200, 200))
            };
            completedFigures.Add(figure);
            canvasView.Invalidate();
        }

        private void CanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            //SKCanvas canvas = e.Surface.Canvas;
            //canvas.Clear();



            var surface = e.Surface;
            // then we get the canvas that we can draw on
            var canvas = surface.Canvas;
            // clear the canvas / view
            canvas.Clear(SKColors.Transparent);
            foreach (EllipseDrawingFigure figure in completedFigures)
            {
                paint.Color = figure.Color;
                canvas.DrawOval(figure.Rectangle, paint);
            }

            // DRAWING SHAPES

            // create the paint for the filled circle

        }
        private void _textureView_SurfaceTextureAvailable(object sender, TextureView.SurfaceTextureAvailableEventArgs e)
        {
            _camera = Android.Hardware.Camera.Open();

            try
            {
                _camera.SetPreviewTexture(e.Surface);
                _camera.SetDisplayOrientation(90);
                _camera.StartPreview();
            }
            catch (Java.IO.IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void MainActivity_Click(object sender, EventArgs e)
        {
            if (MoveMode)
            {
                MoveMode = false;
                FindViewById<Button>(Resource.Id.ShapeMove).Text = "Shape";
            }
            else
            {

                MoveMode = true;
                FindViewById<Button>(Resource.Id.ShapeMove).Text = "Move";

            }
        }

        private void CanvasView_Touch(object sender, View.TouchEventArgs e)
        {
            var action = e.Event.Action;
            switch (action)
            {
                case MotionEventActions.Down:

                    {
                        bool isDragOperation = false;
                        foreach (EllipseDrawingFigure fig in completedFigures)
                        {
                            SKPoint pt = new SKPoint(e.Event.GetX(), e.Event.GetY());
                            // Check if the finger is touching one of the ellipses
                            if (fig.IsInEllipse(pt))
                            {
                                // Tentatively assume this is a dragging operation
                                isDragOperation = true;

                                // Loop through all the figures currently being dragged
                                foreach (EllipseDrawingFigure draggedFigure in draggingFigures)
                                {
                                    activefig = fig;
                                    // If there's a match, we'll need to dig deeper
                                    if (fig == draggedFigure)
                                    {
                                        isDragOperation = false;
                                        break;
                                    }
                                }

                                if (isDragOperation)
                                {
                                    fig.LastFingerLocation = new Point((int)e.Event.GetX(), (int)e.Event.GetY());
                                    draggingFigures.Add(fig);
                                    break;
                                }

                            }

                            //if (isDragOperation)
                            //{
                            //    // Move the dragged ellipse to the end of completedFigures so it's drawn on top
                            //    EllipseDrawingFigure fig = draggingFigures[args.Id];
                            //    completedFigures.Remove(fig);
                            //    completedFigures.Add(fig);
                            //}

                            //canvasView.InvalidateSurface();

                        }
                        break;
                    }
                case MotionEventActions.Move:
                    {
                        if (activefig == null)
                            return;
                        if (MoveMode)
                        {
                            SKRect rect = activefig.Rectangle;
                            var x = e.Event.GetX();
                            var y = e.Event.GetY();
                            rect.Offset(ConvertToPixel(new Point((int)(e.Event.GetX()) - activefig.LastFingerLocation.X,
                                                                 (int)(e.Event.GetY()) - activefig.LastFingerLocation.Y)));
                            activefig.Rectangle = rect;
                            activefig.LastFingerLocation = new Point((int)(e.Event.GetX()), (int)(e.Event.GetY()));
                        }
                        else
                        {
                            Point pt = new Point((int)(e.Event.GetX()), (int)(e.Event.GetY()));
                            activefig.EndPoint = ConvertToPixel(pt);
                        }
                        canvasView.Invalidate();
                        break;
                    }


            }

        }
        SKPoint ConvertToPixel(Point pt)
        {
            return new SKPoint((float)(canvasView.CanvasSize.Width * pt.X / canvasView.Width),
                               (float)(canvasView.CanvasSize.Height * pt.Y / canvasView.Height));
        }

    }
}

