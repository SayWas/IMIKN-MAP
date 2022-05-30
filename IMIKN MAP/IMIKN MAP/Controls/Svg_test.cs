using System;
using System.IO;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace IMIKN_MAP.Controls
{
    public class Svg_Image : Frame
    {

        private readonly SKCanvasView _canvasView = new SKCanvasView();

        private SKPicture _svgPicture;

        private float _scale = 1f;
        private SKMatrix _canvasTranslateMatrix;

        private float _x;
        private float _y;

        private float _xGestureStart;
        private float _yGestureStart;

        public static readonly BindableProperty SourceProperty = BindableProperty.Create(
            nameof(Source), typeof(string), typeof(SvgImage), default(string), propertyChanged: RedrawCanvas);

        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public Svg_Image()
        {
            Padding = new Thickness(0);
            Content = _canvasView;
            _canvasView.PaintSurface += CanvasViewOnPaintSurface;
            InitializeGestures();
        }

        private static void RedrawCanvas(BindableObject bindable, object oldvalue, object newvalue)
        {
            Svg_Image SvgImage = bindable as Svg_Image;
            SvgImage?.LoadSvgPicture();
            SvgImage?._canvasView.InvalidateSurface();
        }

        private void LoadSvgPicture()
        {
            using (Stream stream = GetType().Assembly.GetManifestResourceStream("IMIKN_MAP.Resources." + Source))
            {
                SKSvg svg = new SKSvg();
                svg.Load(stream);

                _svgPicture = svg.Picture;
            }
        }

        private void InitializeGestures()
        {
            PanGestureRecognizer panGestureRecognizer = new PanGestureRecognizer();
            panGestureRecognizer.PanUpdated += MovePicture;

            PinchGestureRecognizer pinchGestureRecognizer = new PinchGestureRecognizer();
            pinchGestureRecognizer.PinchUpdated += ZoomPicture;

            TapGestureRecognizer doubleTapGestureRecognizer = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
            doubleTapGestureRecognizer.Tapped += ZoomToFit;

            _canvasView.GestureRecognizers.Add(panGestureRecognizer);
            _canvasView.GestureRecognizers.Add(pinchGestureRecognizer);
            _canvasView.GestureRecognizers.Add(doubleTapGestureRecognizer);
        }

        private void MovePicture(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _xGestureStart = _x;
                    _yGestureStart = _y;
                    break;
                case GestureStatus.Running:
                    UpdateImageProperties((float)e.TotalX + _xGestureStart, (float)e.TotalY + _yGestureStart);
                    break;
            }
        }

        private void ZoomPicture(object sender, PinchGestureUpdatedEventArgs e)
        {
            switch (e.Status)
            {
                case GestureStatus.Running:
                    float pinchX = (float)(e.ScaleOrigin.X * Width);
                    float pinchY = (float)(e.ScaleOrigin.Y * Height);

                    float newScale = _scale * (float)e.Scale;
                    float scaleRatio = newScale / _scale;

                    float translatedX = pinchX - _canvasTranslateMatrix.TransX;
                    float translatedY = pinchY - _canvasTranslateMatrix.TransY;

                    float newX = translatedX - scaleRatio * (translatedX - _x);
                    float newY = translatedY - scaleRatio * (translatedY - _y);

                    UpdateImageProperties(newX, newY, newScale);
                    break;
            }
        }

        private void ZoomToFit(object sender, EventArgs e)
        {
            UpdateImageProperties(0, 0, 1);
        }

        private void UpdateImageProperties(float x, float y, float? newScale = null)
        {
            _x = x;
            _y = y;
            _scale = newScale ?? _scale;
            _canvasView.InvalidateSurface();
        }

        private void CanvasViewOnPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKCanvas canvas = args.Surface.Canvas;
            canvas.Clear();

            if (string.IsNullOrEmpty(Source))
                return;

            if (_svgPicture == null)
                return;

            SKImageInfo info = args.Info;
            canvas.Translate(info.Width / 2f, info.Height / 2f);

            SKRect bounds = _svgPicture.CullRect;
            float ratio = bounds.Width > bounds.Height
                ? info.Height / bounds.Height
                : info.Width / bounds.Width;

            canvas.Scale(ratio);
            canvas.Translate(-bounds.MidX, -bounds.MidY);
            _canvasTranslateMatrix = canvas.TotalMatrix;
            canvas.Scale(_scale);

            float scaledX = _x / canvas.TotalMatrix.ScaleX;
            float scaledY = _y / canvas.TotalMatrix.ScaleY;

            SKMatrix pictureTranslation = SKMatrix.CreateTranslation(scaledX, scaledY);
            canvas.DrawPicture(_svgPicture, ref pictureTranslation);
        }
    }
}