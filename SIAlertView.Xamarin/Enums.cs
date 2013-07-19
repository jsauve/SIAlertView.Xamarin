namespace SIAlertView.Xamarin
{
    public enum SIAlertViewButtonType
    {
        Default = 0,
        Destructive,
        Cancel
    }

    public enum SIAlertViewBackgroundStyle
    {
        Gradient = 0,
        Solid
    }

    public enum SIAlertViewTransitionStyle
    {
        SlideFromBottom = 0,
        SlideFromTop,
        Fade,
        //Bounce,   // Not yet supported because I have not yet figured out how to properly wire up the Delegate property on a CAKeyFrameAnimation
        //DropDown  // Not yet supported because I have not yet figured out how to properly wire up the Delegate property on a CAKeyFrameAnimation
    }
}