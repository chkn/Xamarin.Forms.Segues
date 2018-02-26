using System;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Segues;

namespace Sample {

	public class FadeInOutSegue : Segue {
		protected override async Task ExecuteAsync (Page destination)
		{
			// Save inital values
			var origSrcOpacity = SourcePage.Opacity;
			var origDestOpacity = destination.Opacity;

			// Fade out source and set dest to transparent
			await SourcePage.FadeTo (0);
			destination.Opacity = 0;

			// Update the navigation stack
			await base.ExecuteAsync (destination);

			// Restore opacities
			SourcePage.Opacity = origSrcOpacity;
			await destination.FadeTo (origDestOpacity);
		}
	}
}
