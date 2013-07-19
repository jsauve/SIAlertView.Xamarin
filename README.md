# SIAlertView.Xamarin
SIAlertView.Xamarin is a C#/Mono Xamarin.iOS port of Sumi Interactive's [SIAlertView](https://github.com/Sumi-Interactive/SIAlertView) for native iOS. It's basically a hand-rolled UIAlertView.

Not all of the features work completely. The title and message don't seem to be working reliably. Neither is the gradient background, or the dismissing animations. I hope to get these working sometime in the near future.

Feel free to fix any problems and issue a pull request! Thanks!

# Usage

```csharp

SIAlertView alertView = new SIAlertView("SIAlertView", "Sumi Interactive");

alertView.AddButton("Button1", SIAlertViewButtonType.Default, (alert) => { Log("Button1 clicked!"); alert.DismissAnimated(true); });

alertView.AddButton("Button2", SIAlertViewButtonType.Default, (alert) => { Log("Button2 clicked!"); alert.DismissAnimated(true); });

alertView.AddButton("Button3", SIAlertViewButtonType.Default, (alert) => { Log("Button3 clicked!"); alert.DismissAnimated(true); });

alertView.TransitionStyle = SIAlertViewTransitionStyle.Fade;

alertView.Show();

```
