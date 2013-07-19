using MonoTouch.UIKit;

namespace SIAlertView.Xamarin
{
    public class SIAlertVIewController : UIViewController
    {
        private SIAlertView _AlertView;
        public SIAlertView AlertView
        {
            get { return _AlertView; }
            set { _AlertView = value; }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _AlertView.Setup();
        }

        public override void LoadView()
        {
            this.View = _AlertView;
        }

        public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
        {
            _AlertView.ResetTransition();
            _AlertView.InvalidateLayout();
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);
        }

        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return base.GetSupportedInterfaceOrientations();
        }

        public override bool ShouldAutorotate()
        {
            return true;
        }
    }
}