using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using System;
using System.Drawing;

namespace SIAlertView.Xamarin
{
    public class SIAlertBackgroundWindow : UIWindow
    {
        private SIAlertViewBackgroundStyle _Style;

        public SIAlertBackgroundWindow(SIAlertViewBackgroundStyle backgroundStyle, RectangleF frame)
            : base(frame)
        {
            _Style = backgroundStyle;
            AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
            Opaque = false;
            WindowLevel = Constants.UIWindowLevelSIAlertBackground;
        }

        public override void DrawRect(RectangleF area, UIViewPrintFormatter formatter)
        {
            using (CGContext context = UIGraphics.GetCurrentContext())
            {
                switch (_Style)
                {
                    case SIAlertViewBackgroundStyle.Gradient:
                        {
                            float[] locations = { 0.0f, 1.0f };
                            float[] colors = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.75f };
                            CGGradient gradient = new CGGradient(CGColorSpace.CreateDeviceRGB(), colors, locations);

                            PointF center = new PointF(Bounds.Size.Width / 2, Bounds.Size.Height / 2);
                            float radius = Math.Min(Bounds.Size.Width, Bounds.Size.Height);
                            context.DrawRadialGradient(gradient, center, 0, center, radius, CGGradientDrawingOptions.DrawsAfterEndLocation);
                            break;
                        }
                    case SIAlertViewBackgroundStyle.Solid:
                        {
                            UIColor.FromWhiteAlpha(0f, 0.5f).SetColor();
                            context.SetFillColorWithColor(UIColor.FromWhiteAlpha(0f, 0.5f).CGColor);
                            context.FillRect(Bounds);
                            break;
                        }
                }
            }
        }
    }
}