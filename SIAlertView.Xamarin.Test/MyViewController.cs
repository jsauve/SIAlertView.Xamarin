using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace SIAlertView.Xamarin.Test
{
    public class MyViewController : UIViewController
    {
        UIButton button;
        float buttonWidth = 200;
        float buttonHeight = 50;

        public MyViewController()
        {
        }

        public override void ViewDidLoad()
        {
            Action<string> Log = (s) => { System.Diagnostics.Debug.WriteLine(s); };

            base.ViewDidLoad();

            View.Frame = UIScreen.MainScreen.Bounds;
            View.BackgroundColor = UIColor.White;
            View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

            button = UIButton.FromType(UIButtonType.RoundedRect);

            button.Frame = new RectangleF(
                View.Frame.Width / 2 - buttonWidth / 2,
                View.Frame.Height / 2 - buttonHeight / 2,
                buttonWidth,
                buttonHeight);

            button.SetTitle("Click me", UIControlState.Normal);

            button.TouchUpInside += (object sender, EventArgs e) =>
            {
                //// instantiate the alert with a title and a message
                //SIAlertView alert = new SIAlertView("Nice work!", "Blah Blah Blah");

                //// Customize the alert view with these nifty properties!
                ///*
                //alert.ViewBackgroundColor = UIColor.Red;
                //alert.TitleColor = UIColor.Blue;
                //alert.TitleFont = UIFont.BoldSystemFontOfSize(18f);
                //alert.MessageColor = UIColor.Green;
                //alert.MessageFont = UIFont.ItalicSystemFontOfSize(14f);
                //alert.ButtonFont = UIFont.SystemFontOfSize(12f);
                //alert.CornerRadius = 15f; // change the alert view box corner radius. Default is 2f.
                //alert.ShadowRadius = 15f; // change the alert view box shadow radius. Default is 8f.
                //alert.TransitionStyle =  // default is SIAlertViewTransitionStyleSlideFromBottom
                //alert.BackgroundStyle =  // default is SIAlertViewButtonTypeGradient
                //*/

                //// Add a normal button that simply dismisses the alert view
                //alert.AddButton("OK!", SIAlertViewButtonType.Default, (x) => { x.DismissAnimated(true); });

                //// show it!
                //alert.Show();
            };

            button.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin |
                UIViewAutoresizing.FlexibleBottomMargin;

            View.AddSubview(button);
        }

    }
}

