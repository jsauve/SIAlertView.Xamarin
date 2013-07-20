using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SIAlert.Xamarin
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
        public float ContainerWidth { get; set; }
        public float ButtonHeight { get; set; }
        public float ButtonMargin { get; set; }
        /// <summary>
        /// This property is only honored if the Cancel button is the last item in the list (at the bottom). Otherwise, the regular ButtonMargin will be used.
        /// </summary>
        public float CancelButtonMarginTop { get; set; }
        public float ContentMarginLeft { get; set; }
        public float ContentMarginTop { get; set; }
        public float ContentMarginBottom { get; set; }
        public int MinimumMessageLineCount { get; set; }
        public int MaximumMessageLineCount { get; set; }

        private static Random _Random = new Random();

        private static Func<float> GetRandomAngle = () =>
        {
            double range = (double)100f - (double)0f;
            double sample = _Random.NextDouble();
            double scaled = (sample * range) + float.MinValue;
            float f = (float)scaled;
            return (f - 50f) / 100f;
        };

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
            ContainerWidth = Constants.CONTAINER_WIDTH;
            ButtonHeight = Constants.BUTTON_HEIGHT;
            ButtonMargin = Constants.GAP;
            CancelButtonMarginTop = Constants.CANCEL_BUTTON_PADDING_TOP;
            ContentMarginLeft = Constants.CONTENT_PADDING_LEFT;
            ContentMarginTop = Constants.CONTENT_PADDING_TOP;
            ContentMarginBottom = Constants.CONTENT_PADDING_BOTTOM;
            MinimumMessageLineCount = Constants.MESSAGE_MIN_LINE_COUNT;
            MaximumMessageLineCount = Constants.MESSAGE_MAX_LINE_COUNT;
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

        private bool _AlwaysStackButtons;
        public bool AlwaysStackButtons
        {
            get
            {
                return _AlwaysStackButtons;
            }
            set
            {
                _AlwaysStackButtons = value;
                InvalidateLayout();
            }
        }

        public Action<SIAlertView> WillShowHandler;
        public Action<SIAlertView> DidShowHandler;
        public Action<SIAlertView> WillDismissHandler;
        public Action<SIAlertView> DidDismissHandler;

        public void AddButton(string title, SIAlertViewButtonType type, Action handler)
        {
            SIAlertItem item = new SIAlertItem();
            item.Title = title;
            item.Type = type;
            item.Action = handler;
            _Items.Add(item);
        }

        public void AddButton(string title, SIAlertViewButtonType type)
        {
            AddButton(title, type, null);
        }

        public void AddButton(SIAlertItem alertItem)
        {
            _Items.Add(alertItem);
        }

        public void Show()
        {
            _OldKeyWindow = UIApplication.SharedApplication.KeyWindow;

            if (!SharedQueue.Contains(this))
            {
                SharedQueue.Add(this);
            }

            if (IsAnimating)
            {
                return; // wait for next turn
            }

            if (_IsVisible)
            {
                return;
            }

            if (CurrentAlertView != null && CurrentAlertView.IsVisible)
            {
                SIAlertView alert = CurrentAlertView;
                alert.DismissAnimated(true, false);
                return;
            }

            if (this.WillShowHandler != null)
            {
                this.WillShowHandler.Invoke(this);
            }

            NSNotificationCenter.DefaultCenter.PostNotificationName(Constants.SIAlertViewWillShowNotification, this, null);

            this._IsVisible = true;

            SetAnimating(true);
            SetCurrentAlertView(this);

            // transition background
            this.ShowBackground();

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

            this.TransitionInCompletion(() =>
            {
                if (this.DidShowHandler != null)
                {
                    this.DidShowHandler.Invoke(this);
                }

                NSNotificationCenter.DefaultCenter.PostNotificationName(Constants.SIAlertViewDidShowNotification, this, null);

                SetAnimating(false);

                int index = SharedQueue.IndexOf(this);
                if (index < SharedQueue.Count - 1)
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
                SetAnimating(true);

                this.TransitionOutCompletion(() => DismissComplete(cleanup));

                if (SharedQueue.Count == 1)
                {
                    HideBackgroundAnimated(true);
                }
            }
            else
            {
                DismissComplete(cleanup);

                if (SharedQueue.Count == 0)
                {
                    HideBackgroundAnimated(true);
                }
            }

            this._OldKeyWindow.MakeKeyWindow();
            this._OldKeyWindow.Hidden = false;
        }

        private void DismissComplete(bool cleanup)
        {
            this._IsVisible = false;

            this.TearDown();

            SetCurrentAlertView(null);

            SIAlertView nextAlertView = null;
            int index = SharedQueue.IndexOf(this);
            if (index > -1 && index < SharedQueue.Count() - 1)
            {
                nextAlertView = SharedQueue[index + 1];
            }

            if (cleanup)
            {
                SharedQueue.Remove(this);
            }

            SetAnimating(false);

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
                if (SharedQueue.Count > 0)
                {
                    SIAlertView alert = SharedQueue.Last();
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
                        rect = new RectangleF(rect.Location.X, locationY, rect.Size.Width, rect.Size.Height);
                        this._ContainerView.Frame = rect;
                        UIView.Animate(
                            duration: 0.3f,
                            delay: 0f,
                            options: UIViewAnimationOptions.TransitionNone,
                            animation: () => { this._ContainerView.Frame = originalRect; },
                            completion: () => { if (action != null) action.Invoke(); });
                    }
                    break;

                case SIAlertViewTransitionStyle.SlideFromTop:
                    {
                        RectangleF rect = this._ContainerView.Frame;
                        RectangleF originalRect = rect;
                        float locationY = -rect.Size.Height;
                        rect = new RectangleF(rect.Location.X, locationY, rect.Size.Width, rect.Size.Height);
                        this._ContainerView.Frame = rect;
                        UIView.Animate(
                            duration: 0.3f,
                            delay: 0f,
                            options: UIViewAnimationOptions.TransitionNone,
                            animation: () => { this._ContainerView.Frame = originalRect; },
                            completion: () => { if (action != null) action.Invoke(); });
                    }
                    break;

                case SIAlertViewTransitionStyle.Fade:
                    {
                        this._ContainerView.Alpha = 0f;
                        UIView.Animate(
                            duration: 0.3f,
                            delay: 0f,
                            options: UIViewAnimationOptions.TransitionNone,
                            animation: () => { this._ContainerView.Alpha = 1f; },
                            completion: () => { if (action != null) action.Invoke(); });
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

                // for now, has the same body as the SlideFromTop case;
                case SIAlertViewTransitionStyle.DropDown:
                    {
                        RectangleF rect = this._ContainerView.Frame;
                        RectangleF originalRect = rect;
                        float locationY = -rect.Size.Height;
                        rect = new RectangleF(rect.Location.X, locationY, rect.Size.Width, rect.Size.Height);
                        this._ContainerView.Frame = rect;
                        UIView.Animate(
                            duration: 0.3f,
                            delay: 0f,
                            options: UIViewAnimationOptions.TransitionNone,
                            animation: () => { this._ContainerView.Frame = originalRect; },
                            completion: () => { if (action != null) action.Invoke(); });
                    }
                    break;

                default:
                    break;
            }
        }

        public void TransitionOutCompletion(NSAction action)
        {
            switch (this.TransitionStyle)
            {
                case SIAlertViewTransitionStyle.SlideFromBottom:
                    {
                        RectangleF rect = this._ContainerView.Frame;
                        var locationY = this.Bounds.Size.Height;
                        rect = new RectangleF(rect.Location.X, locationY, rect.Size.Width, rect.Size.Height);
                        UIView.Animate(
                            duration: 0.3f,
                            delay: 0f,
                            options: UIViewAnimationOptions.CurveEaseIn,
                            animation: () => { this._ContainerView.Frame = rect; },
                            completion: () => { if (action != null) action.Invoke(); });
                    }
                    break;

                case SIAlertViewTransitionStyle.SlideFromTop:
                    {
                        RectangleF rect = this._ContainerView.Frame;
                        var locationY = -rect.Size.Height;
                        rect = new RectangleF(rect.Location.X, locationY, rect.Size.Width, rect.Size.Height);
                        UIView.Animate(
                            duration: 0.3f,
                            delay: 0f,
                            options: UIViewAnimationOptions.CurveEaseIn,
                            animation: () => { this._ContainerView.Frame = rect; },
                            completion: () => { if (action != null) action.Invoke(); });
                    }
                    break;

                case SIAlertViewTransitionStyle.Fade:
                    {
                        UIView.Animate(
                            duration: 0.25f,
                            delay: 0f,
                            options: UIViewAnimationOptions.CurveEaseIn,
                            animation: () => { this._ContainerView.Alpha = 0f; },
                            completion: () => { if (action != null) action.Invoke(); });
                    }
                    break;

                //case SIAlertViewTransitionStyle,Bounce:
                //{
                //    CAKeyframeAnimation *animation = [CAKeyframeAnimation animationWithKeyPath:@"transform.scale"];
                //    animation.values = @[@(1), @(1.2), @(0.01)];
                //    animation.keyTimes = @[@(0), @(0.4), @(1)];
                //    animation.timingFunctions = @[[CAMediaTimingFunction functionWithName:kCAMediaTimingFunctionEaseInEaseOut], [CAMediaTimingFunction functionWithName:kCAMediaTimingFunctionEaseOut]];
                //    animation.duration = 0.35;
                //    animation.delegate = self;
                //    [animation setValue:completion forKey:@"handler"];
                //    [self.containerView.layer addAnimation:animation forKey:@"bounce"];

                //    self.containerView.transform = CGAffineTransformMakeScale(0.01, 0.01);
                //}
                //    break;

                case SIAlertViewTransitionStyle.DropDown:
                    {
                        PointF point = this._ContainerView.Center;
                        PointF newPoint = new PointF(point.X, point.Y + this.Bounds.Size.Height);
                        UIView.Animate(
                            duration: 0.3f,
                            delay: 0f,
                            options: UIViewAnimationOptions.CurveEaseIn,
                            animation: () =>
                            {
                                this._ContainerView.Center = newPoint;
                                float angle = GetRandomAngle();
                                this._ContainerView.Transform = CGAffineTransform.MakeRotation(angle);
                            },
                            completion: () => { if (action != null) action.Invoke(); });
                    }
                    break;

                default:
                    break;
            }
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
            float left = (this.Bounds.Size.Width - ContainerWidth) * 0.5f;
            float top = (this.Bounds.Size.Height - height) * 0.5f;
            this._ContainerView.Transform = CGAffineTransform.MakeIdentity();
            this._ContainerView.Frame = new RectangleF(left, top, ContainerWidth, height);
            this._ContainerView.Layer.ShadowPath = UIBezierPath.FromRoundedRect(this._ContainerView.Bounds, this._ContainerView.Layer.CornerRadius).CGPath;

            float y = ContentMarginTop;

            if (this._TitleLabel != null)
            {
                this._TitleLabel.Text = this.Title;
                float h = this.HeightForTitleLabel;
                this._TitleLabel.Frame = new RectangleF(ContentMarginLeft, y, this._ContainerView.Bounds.Size.Width - ContentMarginLeft * 2f, h);
                y += h;
            }

            if (this._MessageLabel != null)
            {
                if (y > ContentMarginTop)
                {
                    y += ButtonMargin;
                }
                this._MessageLabel.Text = this._Message;
                float h = this.HeightForMessageLabel;
                this._MessageLabel.Frame = new RectangleF(ContentMarginLeft, y, this._ContainerView.Bounds.Size.Width - ContentMarginLeft * 2f, h);
                y += h;
            }

            if (this._Items.Count > 0)
            {
                if (y > ContentMarginTop)
                {
                    y += ButtonMargin;
                }
                if (this._Items.Count == 2 && !_AlwaysStackButtons)
                {
                    float width = (this._ContainerView.Bounds.Size.Width - ContentMarginLeft * 2f - ButtonMargin) * 0.5f;
                    UIButton button = this._Buttons[0];
                    button.Frame = new RectangleF(ContentMarginLeft, y, width, ButtonHeight);
                    button = this._Buttons[1];
                    button.Frame = new RectangleF(ContentMarginLeft + width + ButtonMargin, y, width, ButtonHeight);
                }
                else
                {
                    int i = 0;
                    foreach (var button in this._Buttons)
                    {
                        button.Frame = new RectangleF(ContentMarginLeft, y, this._ContainerView.Bounds.Size.Width - ContentMarginLeft * 2f, ButtonHeight);
                        if (this._Buttons.Count > 1)
                        {
                            if (i == this._Buttons.Count - 1 && this._Items[i].Type == SIAlertViewButtonType.Cancel)
                            {
                                RectangleF rect = button.Frame;
                                var locationY = rect.Location.Y;
                                locationY += CancelButtonMarginTop;
                                button.Frame = new RectangleF(rect.Location.X, locationY, rect.Size.Width, rect.Size.Height);
                            }
                            y += ButtonHeight + ButtonMargin;
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
                float height = ContentMarginTop;
                if (!string.IsNullOrWhiteSpace(this._Title))
                {
                    height += this.HeightForTitleLabel;
                }
                if (!string.IsNullOrWhiteSpace(this._Message))
                {
                    if (height > ContentMarginTop)
                    {
                        height += ButtonMargin;
                    }
                    height += this.HeightForMessageLabel;
                }
                if (this._Items.Count > 0)
                {
                    if (height > ContentMarginTop)
                    {
                        height += ButtonMargin;
                    }
                    if (this._Items.Count <= 2 && !_AlwaysStackButtons)
                    {
                        height += ButtonHeight;
                    }
                    else
                    {
                        height += (ButtonHeight + ButtonMargin) * this._Items.Count - ButtonMargin;
                        if (this._Buttons.Count > 2 && this._Items.Last().Type == SIAlertViewButtonType.Cancel)
                        {
                            height += CancelButtonMarginTop;
                        }
                    }
                }
                height += ContentMarginBottom;
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

                    float forWidth = ContainerWidth - ContentMarginLeft * 2;

                    if (Int32.Parse(UIDevice.CurrentDevice.SystemVersion.Split('.')[0]) < 6)
                        minFontSize = this._TitleLabel.MinimumFontSize;
                    else
                        minFontSize = this._TitleLabel.Font.PointSize * this._TitleLabel.MinimumScaleFactor;

                    float actualFontSize = this._TitleLabel.Font.PointSize; // this only exsists because the StringSize() method requires it as a ref parameter. We don't actually use the value after the method has been called.
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
                float minHeight = MinimumMessageLineCount * this._MessageLabel.Font.LineHeight;

                if (_MessageLabel != null)
                {
                    float maxHeight = MaximumMessageLineCount * this._MessageLabel.Font.LineHeight;

                    SizeF size =
                        new NSString(_Message)
                        .StringSize(
                            _MessageLabel.Font,
                            new SizeF(ContainerWidth - ContentMarginLeft * 2, maxHeight),
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
        private static SIAlertView CurrentAlertView
        {
            get { return _CurrentAlertView; }
        }

        private static void SetCurrentAlertView(SIAlertView alertView)
        {
            _CurrentAlertView = alertView;
        }

        private static bool IsAnimating
        {
            get { return _IsAnimating; }
        }

        private static void SetAnimating(bool animating)
        {
            _IsAnimating = animating;
        }

        public void ShowBackground()
        {
            if (_BackgroundWindow == null)
            {
                _BackgroundWindow = new SIAlertBackgroundWindow(CurrentAlertView.BackgroundStyle, UIScreen.MainScreen.Bounds);
                _BackgroundWindow.MakeKeyAndVisible();
                _BackgroundWindow.Alpha = 0f;
                UIView.Animate(
                    duration: 0.3f,
                    delay: 0f, options: UIViewAnimationOptions.CurveEaseInOut,
                    animation: () => { _BackgroundWindow.Alpha = 1; },
                    completion: () => { });
            }
        }

        public void HideBackgroundAnimated(bool animated)
        {
            if (_BackgroundWindow != null)
            {
                if (!animated)
                {
                    _BackgroundWindow.RemoveFromSuperview();
                    _BackgroundWindow = null;
                    return;
                }

                UIView.Animate(
                    duration: 0.3f,
                    delay: 0f,
                    options: UIViewAnimationOptions.CurveEaseInOut,
                    animation: () => { _BackgroundWindow.Alpha = 0; },
                    completion: () => {
                        if (_BackgroundWindow != null)
                        {
                            _BackgroundWindow.RemoveFromSuperview();
                            _BackgroundWindow = null;
                        }
                    });
            }
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
                if (this._TitleLabel != null)
                {
                    this._TitleLabel.RemoveFromSuperview();
                    this._TitleLabel = null;
                }
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
                    this._MessageLabel.Lines = MaximumMessageLineCount;
                    this._ContainerView.AddSubview(this._MessageLabel);
                }

                this._MessageLabel.Text = this._Message;
            }
            else
            {
                if (this._MessageLabel != null)
                {
                    this._MessageLabel.RemoveFromSuperview();
                    this._MessageLabel = null;
                }
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

            // both custom background images images must be present, otherwise we use the defaults
            if (item.BackgroundImageNormal == null || item.BackgroundImageHighlighted == null)
            {
                switch (item.Type)
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
            }
            else
            {
                normalImage = item.BackgroundImageNormal;
                highlightedImage = item.BackgroundImageHighlighted;
            }

            // both images must be present, othrwise we'll fall back to the specified background color
            if (normalImage != null && highlightedImage != null)
            {
                float hInset = (float)Math.Floor(normalImage.Size.Width / 2);
                float vInset = (float)Math.Floor(normalImage.Size.Height / 2);
                UIEdgeInsets insets = new UIEdgeInsets(vInset, hInset, vInset, hInset);
                if (normalImage != null)
                {
                    normalImage = normalImage.CreateResizableImage(insets);
                    button.SetBackgroundImage(normalImage, UIControlState.Normal);
                }
                if (highlightedImage != null)
                {
                    highlightedImage = highlightedImage.CreateResizableImage(insets);
                    button.SetBackgroundImage(highlightedImage, UIControlState.Highlighted);
                }
            }
            else if (item.BackgroundColor != null)
            {
                button.BackgroundColor = item.BackgroundColor;
            }

            if (item.TextColorNormal != null)
            {
                button.SetTitleColor(item.TextColorNormal, UIControlState.Normal);
            }
            if (item.TextColorHIghlighted != null)
            {
                button.SetTitleColor(item.TextColorHIghlighted, UIControlState.Highlighted);
            }
            
            button.TouchUpInside += (o, s) => { ButtonAction(button); };
            return button;
        }

        public void ButtonAction(UIButton button)
        {
            SetAnimating(true); // set this flag to YES in order to prevent showing another alert in action block
            SIAlertItem item = this._Items[button.Tag];
            if (item.Action != null)
            {
                item.Action();
            }
            this.DismissAnimated(true);
        }
    }
}
