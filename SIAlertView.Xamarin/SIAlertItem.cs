using System;

namespace SIAlertView.Xamarin
{
    public class SIAlertItem
    {
        public string Title { get; set; }
        public SIAlertViewButtonType Type { get; set; }
        public Action<SIAlertView> Action { get; set; }
    }
}