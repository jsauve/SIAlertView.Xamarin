using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

namespace SIAlertView.Xamarin
{
    public class SIAlertView : UIView
    {
        // private fields
        private List<SIAlertItem> _Items;
        private UIWindow _OldKeyWindow;
        private UIWindow _AlertWindow;
        private UILabel _TitleLabel;
        private UILabel _MessageLabel;
        private UIView _ContainerView;
        private List<UIButton> _Buttons;

        // public properties. Set them to your heart's content!
        public UIColor ViewBackgroundColor { get; set; }
        public UIColor TitleColor { get; set; }
        public UIFont TitleFont { get; set; }
        public UIColor MessageColor { get; set; }
        public UIFont MessageFont { get; set; }
        public UIFont ButtonFont { get; set; }
        public float CornerRadius { get; set; }
        public float ShadowRadius { get; set; }
        public SIAlertViewTransitionStyle TransitionStyle { get; set; } // default is SIAlertViewTransitionStyleSlideFromBottom
        public SIAlertViewBackgroundStyle BackgroundStyle { get; set; } // default is SIAlertViewButtonTypeGradient

        /// <summary>
        /// Instantiate SIAlertView without a title or message
        /// </summary>
        public SIAlertView() : this(null, null) { }

        /// <summary>
        /// Instntiate SIAlertView with title and message
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public SIAlertView(string title, string message)
        {
            _Title = title;
            _Message = message;
            _Items = new List<SIAlertItem>();

            ViewBackgroundColor = UIColor.White;
            TitleColor = UIColor.Black;
            MessageColor = UIColor.DarkGray;
            TitleFont = UIFont.BoldSystemFontOfSize(20);
            MessageFont = UIFont.SystemFontOfSize(16);
            ButtonFont = UIFont.SystemFontOfSize(UIFont.ButtonFontSize);
            CornerRadius = 2f;
            ShadowRadius = 8f;
        }

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                InvalidateLayout();
            }
        }

        private string _Message;
        public string Message
        {
            get { return _Message; }
            set
            {
                _Message = value;
                InvalidateLayout();
            }
        }

        public Action<SIAlertView> WillShowHandler;
        public Action<SIAlertView> DidShowHandler;
        public Action<SIAlertView> WillDismissHandler;
        public Action<SIAlertView> DidDismissHandler;

        public void AddButton(string title, SIAlertViewButtonType type, Action<SIAlertView> handler)
        {
            SIAlertItem item = new SIAlertItem();
            item.Title = title;
            item.Type = type;
            item.Action = handler;
            _Items.Add(item);
        }

        public void Show()
        {
            _OldKeyWindow = UIApplication.SharedApplication.KeyWindow;

            if (!SIAlertView.SharedQueue.Contains(this))
            {
                SIAlertView.SharedQueue.Add(this);
            }
    
            if (SIAlertView.IsAnimating)
            {
                return; // wait for next turn
            }
    
            if (_IsVisible)
            {
                return;
            }

            if (CurrentAlertView != null && CurrentAlertView.IsVisible)
            {
                SIAlertView alert = SIAlertView.CurrentAlertView;
                alert.DismissAnimated(true, false);
                return;
            }
    
            if (this.WillShowHandler != null)
            {
                this.WillShowHandler.Invoke(this);
            }

            NSNotificationCenter.DefaultCenter.PostNotificationName(Constants.SIAlertViewWillShowNotification, this, null);
    
            this._IsVisible = true;

            SIAlertView.SetAnimating(true);
            SIAlertView.SetCurrentAlertView(this);
    
            // transition background
            SIAlertView.ShowBackground();
    
            SIAlertVIewController viewController = new SIAlertVIewController();
            viewController.AlertView = this;
    
            if (this._AlertWindow == null)
            {
                UIWindow window = new UIWindow(UIScreen.MainScreen.Bounds);
                window.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
                window.Opaque = false;
                window.WindowLevel = Constants.UIWindowLevelSIAlert;
                window.RootViewController = viewController;
                this._AlertWindow = window;
            }

            this._AlertWindow.MakeKeyAndVisible();
    
            this.ValidateLayout();

            this.TransitionInCompletion(() => {
                if (this.DidShowHandler != null)
                {
                    this.DidShowHandler.Invoke(this);
                }

                NSNotificationCenter.DefaultCenter.PostNotificationName(Constants.SIAlertViewDidShowNotification, this, null);

                SIAlertView.SetAnimating(false);

                int index = SIAlertView.SharedQueue.IndexOf(this);
                if (index < SIAlertView.SharedQueue.Count - 1)
                {
                    this.DismissAnimated(true, false);
                }
            });
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            ValidateLayout();
        }

        public void DismissAnimated(bool animated)
        {
            DismissAnimated(animated, true);
        }

        public void DismissAnimated(bool animated, bool cleanup)
        {
            bool isVisible = this._IsVisible;

            if (isVisible)
            {
                if (this.WillDismissHandler != null)
                {
                    this.WillDismissHandler.Invoke(this);
                }

                NSNotificationCenter.DefaultCenter.PostNotificationName(Constants.SIAlertViewWillDismissNotification, this, null);
            }

            if (animated && isVisible)
            {
                SIAlertView.SetAnimating(true);
                this.TransitionOutCompletion(() => DismissComplete(cleanup));

                if (SIAlertView.SharedQueue.Count == 1)
                {
                    SIAlertView.HideBackgroundAnimated(true);
                }
            }
            else
            {
                DismissComplete(cleanup);

                if (SIAlertView.SharedQueue.Count == 0)
                {
                    SIAlertView.HideBackgroundAnimated(true);
                }
            }

            this._OldKeyWindow.MakeKeyWindow();
            this._OldKeyWindow.Hidden = false;
        }

        private void DismissComplete(bool cleanup)
        {
            this._IsVisible = false;

            this.TearDown();

            SIAlertView.SetCurrentAlertView(null);

            SIAlertView nextAlertView = null;
            int index = SIAlertView.SharedQueue.IndexOf(this);
            if (index > -1 && index < SIAlertView.SharedQueue.Count() - 1)
            {
                nextAlertView = SIAlertView.SharedQueue[index + 1];
            }

            if (cleanup)
            {
                SIAlertView.SharedQueue.Remove(this);
            }

            SIAlertView.SetAnimating(false);

            if (_IsVisible)
            {
                if (this.DidDismissHandler != null)
                {
                    this.DidDismissHandler.Invoke(this);
                }
                NSNotificationCenter.DefaultCenter.PostNotificationName(Constants.SIAlertViewDidDismissNotification, this, null);
            }

            // check if we should show next alert
            if (_IsVisible)
            {
                return;
            }

            if (nextAlertView != null)
            {
                nextAlertView.Show();
            }
            else
            {
                // show last alert view
                if (SIAlertView.SharedQueue.Count > 0)
                {
                    SIAlertView alert = SIAlertView.SharedQueue.Last();
                    alert.Show();
                }
            }
        }

        private void TransitionInCompletion(NSAction action)
        {
            // convenience Func for float-to-NSNumber conversion
            Func<float, NSNumber> f = (x) => { return NSNumber.FromFloat(x); };

            Func<string, CAMediaTimingFunction> t = (s) => { return CAMediaTimingFunction.FromName(s); };

            switch (this.TransitionStyle) 
            {
                case SIAlertViewTransitionStyle.SlideFromBottom:
                {
                    RectangleF originalRect = this._ContainerView.Frame;
                    RectangleF rect = originalRect;
                    float locationY = this.Bounds.Size.Height;
                    this._ContainerView.Frame = new RectangleF(rect.Location.X, locationY, rect.Size.Width, rect.Size.Height);
                    UIView.Animate(
                        duration: 0.3f, 
                        delay: 0f, 
                        options: UIViewAnimationOptions.CurveEaseInOut, 
                        animation: () => { this._ContainerView.Frame = originalRect; }, 
                        completion: () => { if (action != null )action.Invoke(); });
                }
                break;

                case SIAlertViewTransitionStyle.SlideFromTop:
                {
                    RectangleF rect = this._ContainerView.Frame;
                    RectangleF originalRect = rect;
                    float locationY = -rect.Size.Height;
                    this._ContainerView.Frame = new RectangleF(rect.Location.X, locationY, rect.Size.Width, rect.Size.Height);
                    UIView.Animate(
                        duration: 0.3f,
                        delay: 0f,
                        options: UIViewAnimationOptions.CurveEaseInOut,
                        animation: () => { this._ContainerView.Frame = originalRect; },
                        completion: () => { if (action != null )action.Invoke(); });
                }
                break;

                case SIAlertViewTransitionStyle.Fade:
                {
                    this._ContainerView.Alpha = 0f;
                    UIView.Animate(
                        duration: 0.3f,
                        delay: 0f,
                        options: UIViewAnimationOptions.CurveEaseInOut,
                        animation: () => { this._ContainerView.Alpha = 1f; },
                        completion: () => { if (action != null )action.Invoke(); });
                }
                break;

                // These two remaining transition styles are not yet supported because I have not yet figured how to properly wire up the Delegate property on CAKeyFrameAnimation

                //case SIAlertViewTransitionStyle.Bounce:
                //{
                //    CAKeyFrameAnimation animation = CAKeyFrameAnimation.GetFromKeyPath(@"transform.scale");
                //    animation.Values = new NSNumber[] { f(0.01f), f(1.2f), f(0.9f), f(1f) };
                //    animation.KeyTimes = new NSNumber[] { f(0f), f(0.4f), f(0.6f), f(1f) };
                //    animation.TimingFunctions = new CAMediaTimingFunction[] { t(CAMediaTimingFunction.Linear.ToString()), t(CAMediaTimingFunction.Linear.ToString()), t(CAMediaTimingFunction.EaseOut.ToString()) };
                //    animation.Duration = 0.5f;
                //    animation.Delegate = 


                //    CAKeyframeAnimation *animation = [CAKeyframeAnimation animationWithKeyPath:@"transform.scale"];
                //    animation.values = @[@(0.01), @(1.2), @(0.9), @(1)];
                //    animation.keyTimes = @[@(0), @(0.4), @(0.6), @(1)];
                //    animation.timingFunctions = @[[CAMediaTimingFunction functionWithName:kCAMediaTimingFunctionLinear], [CAMediaTimingFunction functionWithName:kCAMediaTimingFunctionLinear], [CAMediaTimingFunction functionWithName:kCAMediaTimingFunctionEaseOut]];
                //    animation.duration = 0.5;
                //    animation.delegate = self;
                //    [animation setValue:completion forKey:@"handler"];
                //    [self.containerView.layer addAnimation:animation forKey:@"bouce"];
                //}
                //break;

                //case SIAlertViewTransitionStyle.DropDown:
                //{
                //    CGFloat y = self.containerView.center.y;
                //    CAKeyframeAnimation *animation = [CAKeyframeAnimation animationWithKeyPath:@"position.y"];
                //    animation.values = @[@(y - self.bounds.size.height), @(y + 20), @(y - 10), @(y)];
                //    animation.keyTimes = @[@(0), @(0.5), @(0.75), @(1)];
                //    animation.timingFunctions = @[[CAMediaTimingFunction functionWithName:kCAMediaTimingFunctionEaseOut], [CAMediaTimingFunction functionWithName:kCAMediaTimingFunctionLinear], [CAMediaTimingFunction functionWithName:kCAMediaTimingFunctionEaseOut]];
                //    animation.duration = 0.4;
                //    animation.delegate = self;
                //    [animation setValue:completion forKey:@"handler"];
                //    [self.containerView.layer addAnimation:animation forKey:@"dropdown"];
                //}
                //break;

                default:
                break;
            }
        }

        public void TransitionOutCompletion(NSAction action)
        {
            action.Invoke();
        }

        public void InvalidateLayout()
        {
            this._IsLayoutDirty = true;
            this.SetNeedsLayout();
        }

        public void ResetTransition()
        {
            this._ContainerView.Layer.RemoveAllAnimations();
        }

        private bool _IsVisible;
        public bool IsVisible
        {
            get
            {
                return _IsVisible;
            }
        }

        private bool _IsLayoutDirty;
        public bool IsLayoutDirty
        {
            get
            {
                return _IsLayoutDirty;
            }
        }

        public void ValidateLayout()
        {
            if (!this._IsLayoutDirty) 
                return;
            
            this._IsLayoutDirty = false;
    
            float height = this.PreferredHeight;
            float left = (this.Bounds.Size.Width - Constants.CONTAINER_WIDTH) * 0.5f;
            float top = (this.Bounds.Size.Height - height) * 0.5f;
            this._ContainerView.Transform = CGAffineTransform.MakeIdentity();
            this._ContainerView.Frame = new RectangleF(left, top, Constants.CONTAINER_WIDTH, height);
            this._ContainerView.Layer.ShadowPath = UIBezierPath.FromRoundedRect(this._ContainerView.Bounds, this._ContainerView.Layer.CornerRadius).CGPath;
    
            float y = Constants.CONTENT_PADDING_TOP;

            if (this._TitleLabel != null)
            {
                this._TitleLabel.Text = this.Title;
                float h = this.HeightForTitleLabel;
                this._TitleLabel.Frame = new RectangleF(Constants.CONTENT_PADDING_LEFT, y, this._ContainerView.Bounds.Size.Width - Constants.CONTENT_PADDING_LEFT * 2f, height);
                y += h;
            }

            if (this._MessageLabel != null)
            {
                if (y > Constants.CONTENT_PADDING_TOP)
                {
                    y += Constants.GAP;
                }
                this._MessageLabel.Text = this._Message;
                float h = this.HeightForMessageLabel;
                this._MessageLabel.Frame = new RectangleF(Constants.CONTENT_PADDING_LEFT, y, this._ContainerView.Bounds.Size.Width - Constants.CONTENT_PADDING_LEFT * 2f, height);
                y += h;
            }

            if (this._Items.Count > 0)
            {
                if (y > Constants.CONTENT_PADDING_TOP)
                {
                    y += Constants.GAP;
                }
                if (this._Items.Count == 2)
                {
                    float width = (this._ContainerView.Bounds.Size.Width - Constants.CONTENT_PADDING_LEFT * 2f - Constants.GAP) * 0.5f;
                    UIButton button = this._Buttons[0];
                    button.Frame = new RectangleF(Constants.CONTENT_PADDING_LEFT, y, width, Constants.BUTTON_HEIGHT);
                    button = this._Buttons[1];
                    button.Frame = new RectangleF(Constants.CONTENT_PADDING_LEFT + width + Constants.GAP, y, width, Constants.BUTTON_HEIGHT);
                }
                else
                {
                    int i = 0;
                    foreach(var button in this._Buttons)
                    {
                        button.Frame = new RectangleF(Constants.CONTENT_PADDING_LEFT, y, this._ContainerView.Bounds.Size.Width - Constants.CONTENT_PADDING_LEFT * 2f, Constants.BUTTON_HEIGHT);
                        if (this._Buttons.Count > 1)
                        {
                            if (i == this._Buttons.Count - 1 && this._Items[i].Type == SIAlertViewButtonType.Cancel)
                            {
                                RectangleF rect = button.Frame;
                                var locationY = rect.Location.Y;
                                locationY += Constants.CANCEL_BUTTON_PADDING_TOP;
                                button.Frame = new RectangleF(rect.Location.Y, locationY, rect.Size.Width, rect.Size.Height);
                            }
                            y += Constants.BUTTON_HEIGHT + Constants.GAP;
                        }
                        i++;
                    }
                }
            }
        }

        public float PreferredHeight
        {
            get
            {
                float height = Constants.CONTENT_PADDING_TOP;
	            if (!string.IsNullOrWhiteSpace(this._Title)) {
		            height += this.HeightForTitleLabel;
	            }
                if (!string.IsNullOrWhiteSpace(this._Message)) {
                    if (height > Constants.CONTENT_PADDING_TOP) {
                        height += Constants.GAP;
                    }
                    height += this.HeightForMessageLabel;
                }
                if (this._Items.Count > 0) {
                    if (height > Constants.CONTENT_PADDING_TOP) {
                        height += Constants.GAP;
                    }
                    if (this._Items.Count <= 2) {
                        height += Constants.BUTTON_HEIGHT;
                    } else {
                        height += (Constants.BUTTON_HEIGHT + Constants.GAP) * this._Items.Count - Constants.GAP;
                        if (this._Buttons.Count > 2 && this._Items.Last().Type == SIAlertViewButtonType.Cancel) {
                            height += Constants.CANCEL_BUTTON_PADDING_TOP;
                        }
                    }
                }
                height += Constants.CONTENT_PADDING_BOTTOM;
	            return height;
            }
        }

        public float HeightForTitleLabel
        {
            get
            {
                if (_TitleLabel != null)
                {
                    float minFontSize;

                    float forWidth = Constants.CONTAINER_WIDTH - Constants.CONTENT_PADDING_LEFT * 2;

                    if (Int32.Parse(UIDevice.CurrentDevice.SystemVersion.Split('.')[0]) < 6)
                        minFontSize = this._TitleLabel.Font.PointSize * this._TitleLabel.MinimumScaleFactor;
                    else
                        minFontSize = this._TitleLabel.MinimumFontSize;

                    float actualFontSize = 0f; // this only exsists because the StringSize() method requires it as a ref parameter. We don't actually use the value after the method has been called.
                    SizeF size = new NSString(_Title).StringSize(this._TitleLabel.Font, minFontSize, ref actualFontSize, forWidth, _TitleLabel.LineBreakMode);

                    return size.Height;
                }

                return 0;
            }
        }

        public float HeightForMessageLabel
        {
            get
            {
                float minHeight = Constants.MESSAGE_MIN_LINE_COUNT * this._MessageLabel.Font.LineHeight;

                if (_MessageLabel != null) 
                {
                    float maxHeight = Constants.MESSAGE_MAX_LINE_COUNT * this._MessageLabel.Font.LineHeight;

                    SizeF size = 
                        new NSString(_Message)
                        .StringSize(
                            _MessageLabel.Font, 
                            new SizeF(Constants.CONTAINER_WIDTH - Constants.CONTENT_PADDING_LEFT * 2, maxHeight),
                            _MessageLabel.LineBreakMode);

                    return Math.Max(minHeight, size.Height);
                }
                return minHeight;
            }
        }

        private static bool _IsAnimating;
        private static SIAlertBackgroundWindow _BackgroundWindow;

        private static List<SIAlertView> _SharedQueue;
        public static List<SIAlertView> SharedQueue
        {
            get
            {
                if (_SharedQueue == null)
                {
                    _SharedQueue = new List<SIAlertView>();
                }

                return _SharedQueue;
            }
        }

        private static SIAlertView _CurrentAlertView;
        public static SIAlertView CurrentAlertView
        {
            get { return _CurrentAlertView; }
        }

        public static void SetCurrentAlertView(SIAlertView alertView)
        {
            _CurrentAlertView = alertView;
        }

        public static bool IsAnimating
        {
            get { return _IsAnimating; }
        }

        public static void SetAnimating(bool animating)
        {
            _IsAnimating = animating;
        }

        public static void ShowBackground()
        {
            if (_BackgroundWindow == null)
            {
                _BackgroundWindow = new SIAlertBackgroundWindow(SIAlertView.CurrentAlertView.BackgroundStyle, UIScreen.MainScreen.Bounds);
                _BackgroundWindow.MakeKeyAndVisible();
                _BackgroundWindow.Alpha = 0;
                UIView.Animate(0.3, () => { _BackgroundWindow.Alpha = 1; });
            }
        }

        public static void HideBackgroundAnimated(bool animated)
        {
            if (!animated)
            {
                _BackgroundWindow.RemoveFromSuperview();
                _BackgroundWindow = null;
                return;
            }

            UIView.Animate(
                0.3,
                0,
                UIViewAnimationOptions.CurveEaseInOut,
                () => { _BackgroundWindow.Alpha = 0; },
                () => { _BackgroundWindow.RemoveFromSuperview(); _BackgroundWindow = null; });
        }

        public void Setup()
        {
            SetupContainerView();
            UpdateTitleLabel();
            UpdateMessageLabel();
            SetupButtons();
            InvalidateLayout();
        }

        public void TearDown()
        {
            if (_ContainerView != null)
            {
                _ContainerView.RemoveFromSuperview();
                _ContainerView = null;
            }
            _TitleLabel = null;
            _MessageLabel = null;
            _Buttons.Clear();
            if (_AlertWindow != null)
            {
                _AlertWindow.RemoveFromSuperview();
                _AlertWindow = null;
            }
        }

        public void SetupContainerView()
        {
            _ContainerView = new UIView(this.Bounds);
            _ContainerView.BackgroundColor = ViewBackgroundColor != null ? ViewBackgroundColor : UIColor.White;
            _ContainerView.Layer.CornerRadius = this.CornerRadius;
            _ContainerView.Layer.ShadowOffset = SizeF.Empty;
            _ContainerView.Layer.ShadowRadius = this.ShadowRadius;
            _ContainerView.Layer.ShadowOpacity = 0.5f;
            AddSubview(this._ContainerView);
        }

        public void UpdateTitleLabel()
        {
	        if (!string.IsNullOrWhiteSpace(this._Title)) 
            {
		        if (this._TitleLabel == null)
                {
                    this._TitleLabel = new UILabel(this.Bounds);
			        this._TitleLabel.TextAlignment = UITextAlignment.Center;
                    this._TitleLabel.BackgroundColor = UIColor.Clear;
			        this._TitleLabel.Font = TitleFont;
                    this._TitleLabel.TextColor = TitleColor;
                    this._TitleLabel.AdjustsFontSizeToFitWidth = true;

                    if (Int32.Parse(UIDevice.CurrentDevice.SystemVersion.Split('.')[0]) < 6)
                        this._TitleLabel.MinimumScaleFactor = 0.75f;
                    else
                        this._TitleLabel.MinimumScaleFactor = this._TitleLabel.Font.PointSize * 0.75f;

                    this._ContainerView.AddSubview(this._TitleLabel);
		        }

                this._TitleLabel.Text = this._Title;
	        } 
            else 
            {
                this._TitleLabel.RemoveFromSuperview();
                this._TitleLabel = null;
	        }

            this.InvalidateLayout();
        }

        public void UpdateMessageLabel()
        {
            if (!String.IsNullOrWhiteSpace(this._Message)) 
            {
                if (this._MessageLabel == null) 
                {
                    this._MessageLabel = new UILabel(this.Bounds);
                    this._MessageLabel.TextAlignment = UITextAlignment.Center;
                    this._MessageLabel.BackgroundColor = UIColor.Clear;
                    this._MessageLabel.Font = this.MessageFont;
                    this._MessageLabel.TextColor = this.MessageColor;
                    this._MessageLabel.Lines = Constants.MESSAGE_MAX_LINE_COUNT;
                    this._ContainerView.AddSubview(this._MessageLabel);
                }

                this._MessageLabel.Text = this._Message;
            } 
            else 
            {
                this._MessageLabel.RemoveFromSuperview();
                this._MessageLabel = null;
            }

            this.InvalidateLayout();
        }

        public void SetupButtons()
        {
            this._Buttons = new List<UIButton>();

            int i = 0;
            foreach (var item in this._Items)
            {
                UIButton button = ButtonForItemIndex(i);
                this._Buttons.Add(button);
                this._ContainerView.AddSubview(button);
                i++;
            }
        }

        public UIButton ButtonForItemIndex(int index)
        {
            SIAlertItem item = this._Items[index];
            UIButton button = UIButton.FromType(UIButtonType.Custom);
            button.Tag = index;
            button.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
            button.TitleLabel.Font = this.ButtonFont;
            button.SetTitle(item.Title, UIControlState.Normal);
            UIImage normalImage = null;
            UIImage highlightedImage = null;

            switch(item.Type)
            {
                case SIAlertViewButtonType.Cancel:
			        normalImage = UIImage.FromBundle(@"Images/SIAlertView.bundle/button-cancel");
			        highlightedImage = UIImage.FromBundle(@"Images/SIAlertView.bundle/button-cancel-d");
                    button.SetTitleColor(UIColor.FromWhiteAlpha(0.3f, 1f), UIControlState.Normal);
                    button.SetTitleColor(UIColor.FromWhiteAlpha(0.3f, 1f), UIControlState.Highlighted);
			        break;
		        case SIAlertViewButtonType.Destructive:
			        normalImage = UIImage.FromBundle(@"Images/SIAlertView.bundle/button-destructive");
			        highlightedImage = UIImage.FromBundle(@"Images/SIAlertView.bundle/button-destructive-d");
                    button.SetTitleColor(UIColor.White, UIControlState.Normal);
                    button.SetTitleColor(UIColor.FromWhiteAlpha(1f, 0.8f), UIControlState.Highlighted);
			        break;
		        case SIAlertViewButtonType.Default:
		        default:
			        normalImage = UIImage.FromBundle(@"Images/SIAlertView.bundle/button-default");
			        highlightedImage = UIImage.FromBundle(@"Images/SIAlertView.bundle/button-default-d");
                    button.SetTitleColor(UIColor.FromWhiteAlpha(0.4f, 1f), UIControlState.Normal);
                    button.SetTitleColor(UIColor.FromWhiteAlpha(0.4f, 0.8f), UIControlState.Highlighted);
			        break;
            }

            float hInset = (float)Math.Floor(normalImage.Size.Width / 2);
            float vInset = (float)Math.Floor(normalImage.Size.Height / 2);
            UIEdgeInsets insets = new UIEdgeInsets(vInset, hInset, vInset, hInset);
            normalImage = normalImage.CreateResizableImage(insets);
            highlightedImage = highlightedImage.CreateResizableImage(insets);
            button.SetBackgroundImage(normalImage, UIControlState.Normal);
            button.SetBackgroundImage(highlightedImage, UIControlState.Highlighted);
            button.TouchUpInside += (o, s) => { ButtonAction(button); };
            return button;
        }

        public void ButtonAction(UIButton button)
        {
            SIAlertView.SetAnimating(true); // set this flag to YES in order to prevent showing another alert in action block
            SIAlertItem item = this._Items[button.Tag];
            if (item.Action != null)
            {
                item.Action(this);
            }
            this.DismissAnimated(true);
        }
    } 
}
