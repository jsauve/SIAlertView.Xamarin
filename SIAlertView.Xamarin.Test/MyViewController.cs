using MonoTouch.UIKit;
using System;
using System.Drawing;

namespace SIAlert.Xamarin.Test
{
    public class MyViewController : UIViewController
    {
        UIButton button1;
        UIButton button2;
        UIButton button3;
        UIButton button4;
        UIButton button5;

        float buttonWidth = 250f;
        float buttonHeight = 50f;

        public MyViewController() { }

        public override void ViewDidLoad()
        {
            Action<string> Log = (s) => { System.Diagnostics.Debug.WriteLine(s); };

            base.ViewDidLoad();

            View.Frame = UIScreen.MainScreen.Bounds;
            View.BackgroundColor = UIColor.White;
            View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

            button1 = UIButton.FromType(UIButtonType.RoundedRect);
            button2 = UIButton.FromType(UIButtonType.RoundedRect);
            button3 = UIButton.FromType(UIButtonType.RoundedRect);
            button4 = UIButton.FromType(UIButtonType.RoundedRect);
            button5 = UIButton.FromType(UIButtonType.RoundedRect);

            button1.Frame = new RectangleF(
                View.Frame.Width / 2 - buttonWidth / 2,
                100f,
                buttonWidth,
                buttonHeight);

            button2.Frame = new RectangleF(
                View.Frame.Width / 2 - buttonWidth / 2,
                160f,
                buttonWidth,
                buttonHeight);

            button3.Frame = new RectangleF(
                View.Frame.Width / 2 - buttonWidth / 2,
                210f,
                buttonWidth,
                buttonHeight);

            button4.Frame = new RectangleF(
                View.Frame.Width / 2 - buttonWidth / 2,
                270f,
                buttonWidth,
                buttonHeight);

            button5.Frame = new RectangleF(
                View.Frame.Width / 2 - buttonWidth / 2,
                330f,
                buttonWidth,
                buttonHeight);


            button1.SetTitle("1 button, slide down", UIControlState.Normal);

            button2.SetTitle("2 buttons, slide up", UIControlState.Normal);

            button3.SetTitle("3 buttons, slide down", UIControlState.Normal);

            button4.SetTitle("2 buttons stacked, fade", UIControlState.Normal);

            button5.SetTitle("No message, slide up", UIControlState.Normal);


            button1.TouchUpInside += (object sender, EventArgs e) =>
            {
                // instantiate the alert with a title and a message
                SIAlertView alert = new SIAlertView("Nice work!", "Congratulations on clicking the button! You truly are a credit to your species.");

                alert.TransitionStyle = SIAlertViewTransitionStyle.SlideFromTop;
                alert.CornerRadius = 10f;
                alert.ShadowRadius = 5f;

                // Add a normal button that simply dismisses the alert view
                alert.AddButton("Thank you!", SIAlertViewButtonType.Default, () => { });

                // show it!
                alert.Show();

                //Customize the alert view with these nifty properties!
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
                //alert.ContainerWidth = 200f;
                //alert.ButtonHeight = 30f;
                //alert.ButtonMargin = 5f;
                //alert.CancelButtonMarginTop = 15f;
                //alert.ContentMarginLeft = 20f;
                //alert.ContentMarginTop = 20f;
                //alert.ContentMarginBottom = 20f;
                //alert.MinimumMessageLineCount = 2;
                //alert.MaximumMessageLineCount = 10;
                //*/
            };

            button1.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin |
                UIViewAutoresizing.FlexibleBottomMargin;

            View.AddSubview(button1);


            button2.TouchUpInside += (object sender, EventArgs e) =>
            {
                // instantiate the alert with a title and a message
                SIAlertView alert = new SIAlertView("Nice work!", "Congratulations on clicking the button! You truly are a credit to your species.");

                alert.TransitionStyle = SIAlertViewTransitionStyle.SlideFromBottom;
                alert.CornerRadius = 10f;
                alert.ShadowRadius = 5f;

                // Add a normal button that simply dismisses the alert view
                alert.AddButton("Thank you!", SIAlertViewButtonType.Default, () => { });
                
                alert.AddButton("Bugger Off!", SIAlertViewButtonType.Destructive, () => { });

                // show it!
                alert.Show();
            };

            button2.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin |
                UIViewAutoresizing.FlexibleBottomMargin;

            View.AddSubview(button2);


            button3.TouchUpInside += (object sender, EventArgs e) =>
            {
                // instantiate the alert with a title and a message
                SIAlertView alert = new SIAlertView("Nice work!", "Congratulations on clicking the button! You truly are a credit to your species.");

                alert.TransitionStyle = SIAlertViewTransitionStyle.SlideFromTop;
                alert.CornerRadius = 10f;
                alert.ShadowRadius = 5f;

                // Add a normal button that simply dismisses the alert view
                alert.AddButton("Thank you!", SIAlertViewButtonType.Default, () => { });
                
                alert.AddButton("Bugger Off!", SIAlertViewButtonType.Destructive, () => { });

                alert.AddButton("Cancel", SIAlertViewButtonType.Cancel, () => { });

                // show it!
                alert.Show();
            };

            button1.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin |
                UIViewAutoresizing.FlexibleBottomMargin;

            View.AddSubview(button3);


            button4.TouchUpInside += (object sender, EventArgs e) =>
            {
                // instantiate the alert with a title and a message
                SIAlertView alert = new SIAlertView("Nice work!", "Congratulations on clicking the button! You truly are a credit to your species.");

                alert.TransitionStyle = SIAlertViewTransitionStyle.Fade;
                alert.CornerRadius = 10f;
                alert.ShadowRadius = 5f;
                alert.AlwaysStackButtons = true;

                // Add a normal button that simply dismisses the alert view
                alert.AddButton("Thank you!", SIAlertViewButtonType.Default, () => { });
                
                alert.AddButton("Bugger Off!", SIAlertViewButtonType.Destructive, () => { });

                // show it!
                alert.Show();
            };

            button1.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin |
                UIViewAutoresizing.FlexibleBottomMargin;

            View.AddSubview(button4);


            button5.TouchUpInside += (object sender, EventArgs e) =>
            {
                // instantiate the alert with a title and a message
                SIAlertView alert = new SIAlertView("Nice work!", null);

                alert.TransitionStyle = SIAlertViewTransitionStyle.SlideFromBottom;
                alert.CornerRadius = 10f;
                alert.ShadowRadius = 5f;
                alert.MinimumMessageLineCount = 0;
                

                // Add a normal button that simply dismisses the alert view
                alert.AddButton("Thank you!", SIAlertViewButtonType.Default, () => { });
                
                alert.AddButton("Bugger Off!", SIAlertViewButtonType.Destructive, () => { });

                // show it!
                alert.Show();
            };

            button1.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin |
                UIViewAutoresizing.FlexibleBottomMargin;

            View.AddSubview(button5);
        }
    }
}

