# SIAlertView.Xamarin
SIAlertView.Xamarin is a C#/Mono Xamarin.iOS port of Sumi Interactive's [SIAlertView](https://github.com/Sumi-Interactive/SIAlertView) for native iOS. It's basically a hand-rolled UIAlertView.

A couple of the transition effects are not yet working. I'm hoping to have that fixed soon.

I also need to ensure that the action delegates on the button clicks are working optimally.

Feel free to fix any problems and issue a pull request! Thanks!

# Usage

```csharp

SIAlertView alertView = new SIAlertView("SIAlertView", "Sumi Interactive");

alertView.AddButton("Button1", SIAlertViewButtonType.Default, (alert) => { Log("Button1 clicked!"); });

alertView.AddButton("Button2", SIAlertViewButtonType.Default, (alert) => { Log("Button2 clicked!"); });

alertView.AddButton("Button3", SIAlertViewButtonType.Default, (alert) => { Log("Button3 clicked!"); });

alertView.TransitionStyle = SIAlertViewTransitionStyle.Fade;

alertView.Show();

```
