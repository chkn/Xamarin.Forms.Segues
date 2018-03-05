using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Segues;

namespace Sample {

	public class DownUpSegue : Segue {
		protected override async Task ExecuteAsync (Page destination)
		{
			// The translation doesn't always work for Page on iOS,
			//  so operate on the Content if it is a ContentPage
			var src = ((VisualElement)(SourcePage as ContentPage)?.Content) ?? SourcePage;
			var dest = ((VisualElement)(destination as ContentPage)?.Content) ?? destination;

			// Save initial values
			var origSrcTy = src.TranslationY;
			var origDestTy = 0d;
			if (dest != null) {
				origDestTy = dest.TranslationY;
				dest.TranslationY = src.Height;
			}

			// Animate SourcePage to bottom and also set (but do not animate) destination page to bottom
			await src.TranslateTo (0, src.Height, easing: Easing.SpringIn);

			// Actually switch pages
			await base.ExecuteAsync (destination);

			// IMPORTANT! Restore source content to original settings, otherwise
			//  things will look weird if we pop back to it.
			src.TranslationY = origSrcTy;

			// Animate up destination content
			if (dest != null)
				await dest.TranslateTo (0, origDestTy, easing: Easing.SpringOut);
		}
	}
}
