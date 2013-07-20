using MonoTouch.UIKit;
using System;

namespace SIAlert.Xamarin
{
    public class SIAlertItem
    {
        public string Title { get; set; }
        public SIAlertViewButtonType Type { get; set; }
        public Action Action { get; set; }

        public UIColor BackgroundColor { get; set; }

        public UIColor TextColorNormal { get; set; }
        public UIColor TextColorHIghlighted { get; set; }

        public UIImage BackgroundImageNormal { get; set; }
        public UIImage BackgroundImageHighlighted { get; set; }

        public SIAlertItem() { }

        public SIAlertItem(string title, SIAlertViewButtonType type, Action action)
        {
            Title = title;
            Type = type;
            Action = action;
        }
    }
}