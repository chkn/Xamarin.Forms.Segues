using System;
using System.ComponentModel;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Segues;

namespace Sample {

	public class SpinnerSegue : Segue {

		public static readonly BindableProperty MessageProperty =
			BindableProperty.Create (nameof (Message), typeof (string), typeof (SpinnerSegue), "Loading...");

		public string Message {
			get => (string)GetValue (MessageProperty);
			set => SetValue (MessageProperty, value);
		}

		bool canExecute;

		public SpinnerSegue ()
		{
			PropertyChanged += HandlePropertyChanged;
		}

		private void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof (SourcePage))
				return;
			var canExecuteNow = SourcePage is ContentPage;
			if (canExecuteNow != canExecute) {
				canExecute = canExecuteNow;
				RaiseCanExecuteChanged ();
			}
		}

		// We require the source page to be a ContentPage..
		// Note that we listen for a property changed event above to invalidate this..
		protected override bool CanExecuteOverride (Type destinationType)
			=> canExecute;

		protected override async Task ExecuteAsync (Page destination)
		{
			var src = (ContentPage)SourcePage;
			var content = src.Content;

			// Add an overlay to dim the source page
			var layout = new AbsoluteLayout ();
			src.Content = layout;

			// Save previous values, if any
			Rectangle? bounds = null;
			AbsoluteLayoutFlags? flags = null;
			if (content.IsSet (AbsoluteLayout.LayoutBoundsProperty))
				bounds = AbsoluteLayout.GetLayoutBounds (content);
			if (content.IsSet (AbsoluteLayout.LayoutFlagsProperty))
				flags = AbsoluteLayout.GetLayoutFlags (content);

			// Set new values
			AbsoluteLayout.SetLayoutBounds (content, new Rectangle (0, 0, 1, 1));
			AbsoluteLayout.SetLayoutFlags (content, AbsoluteLayoutFlags.All);
			layout.Children.Add (content);

			var overlay = new BoxView {
				Opacity = 0,
				BackgroundColor = new Color (0.5, 0.5, 0.5, 0.7)
			};
			AbsoluteLayout.SetLayoutBounds (overlay, new Rectangle (0, 0, 1, 1));
			AbsoluteLayout.SetLayoutFlags (overlay, AbsoluteLayoutFlags.All);
			layout.Children.Add (overlay);

			var spinnerLayout = new StackLayout {
				Orientation = StackOrientation.Vertical,
				BackgroundColor = Color.White,
				Padding = 25,
				IsVisible = false
			};

			var spinner = new ActivityIndicator {
				IsRunning = true,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.CenterAndExpand
			};
			spinnerLayout.Children.Add (spinner);

			var label = new Label {
				BindingContext = this,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.End
			};
			label.SetBinding (Label.TextProperty, new Binding (nameof (Message)));
			spinnerLayout.Children.Add (label);

			AbsoluteLayout.SetLayoutBounds (spinnerLayout, new Rectangle (0.5, 0.5, -1, -1));
			AbsoluteLayout.SetLayoutFlags (spinnerLayout, AbsoluteLayoutFlags.PositionProportional);
			layout.Children.Add (spinnerLayout);

			// Animate in..
			await overlay.FadeTo (1, easing: Easing.SinIn);
			spinnerLayout.IsVisible = true;

			await DoWork ();

			// Now do the segue with default animation
			await ExecuteAsync (destination, useDefaultAnimation: true);

			// Restore the source page afterward
			layout.Children.Remove (content);
			if (bounds.HasValue)
				AbsoluteLayout.SetLayoutBounds (content, bounds.Value);
			if (flags.HasValue)
				AbsoluteLayout.SetLayoutFlags (content, flags.Value);
			src.Content = content;
		}

		// A real implementation would do some async work here..
		protected virtual Task DoWork ()
		{
			return Task.Delay (TimeSpan.FromSeconds (2));
		}
	}
}
