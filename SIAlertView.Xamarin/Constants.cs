internal class Constants
{
    public const string SIAlertViewWillShowNotification = @"SIAlertViewWillShowNotification";
    public const string SIAlertViewDidShowNotification = @"SIAlertViewDidShowNotification";
    public const string SIAlertViewWillDismissNotification = @"SIAlertViewWillDismissNotification";
    public const string SIAlertViewDidDismissNotification = @"SIAlertViewDidDismissNotification";

    public const float UIWindowLevelSIAlert = 1999.0f;  // don't overlap system's alert
    public const float UIWindowLevelSIAlertBackground = 1998.0f; // below the alert window

    public const int MESSAGE_MIN_LINE_COUNT = 2;
    public const int MESSAGE_MAX_LINE_COUNT = 5;

    public const float GAP = 10.0f;
    public const float CANCEL_BUTTON_PADDING_TOP = 5.0f;
    public const float CONTENT_PADDING_LEFT = 10.0f;
    public const float CONTENT_PADDING_TOP = 12.0f;
    public const float CONTENT_PADDING_BOTTOM = 10.0f;
    public const float BUTTON_HEIGHT = 44.0f;
    public const float CONTAINER_WIDTH = 300.0f;
}