using System;
using System.IO;
using System.Runtime.CompilerServices;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Essentials;
using Xamarin.Forms;
using SKSvg = SkiaSharp.Extended.Svg.SKSvg;

namespace IMIKN_MAP.Controls
{
    public class SvgImage : Frame
    {

        private readonly SKCanvasView _canvasView = new SKCanvasView();

        private SKPicture _svgPicture;

        public static readonly BindableProperty SourceProperty = BindableProperty.Create(
            nameof(Source), typeof(string), typeof(SvgImage), default(string), propertyChanged: SourceCanvas);

        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public SvgImage()
        {
            Padding = new Thickness(0);
            BackgroundColor = Color.Transparent;
            HasShadow = false;
            Content = _canvasView;
            _canvasView.PaintSurface += CanvasViewOnPaintSurface;
        }

        private static void SourceCanvas(BindableObject bindable, object oldvalue, object newvalue)
        {
            SvgImage svgSvgImage = bindable as SvgImage;
            svgSvgImage?.LoadSvgPicture();
            svgSvgImage?._canvasView.InvalidateSurface();
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
            float ratio = bounds.Width > bounds.Height || info.Width > info.Height
                ? info.Height / bounds.Height
                : info.Width / bounds.Width;

            canvas.Scale(ratio);
            canvas.Translate(-bounds.MidX, -bounds.MidY);

            canvas.DrawPicture(_svgPicture);
        }
    }
}