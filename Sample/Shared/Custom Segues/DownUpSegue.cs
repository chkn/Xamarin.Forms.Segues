using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Segues;

namespace Sample {

	public class DownUpSegue : Segue {
		protected override async Task ExecuteAsync (Page destination)
		{
			// Save initial values
			var origSrcTy = SourcePage.TranslationY;
			var origDestTy = 0d;
			if (destination != null) {
				origDestTy = destination.TranslationY;
				destination.TranslationY = SourcePage.Height;
			}

			// Animate SourcePage to bottom and also set (but do not animate) destination page to bottom
			await SourcePage.TranslateTo (0, SourcePage.Height, easing: Easing.SpringIn);

			// Actually switch pages
			await base.ExecuteAsync (destination);

			// IMPORTANT! Restore source content!
			SourcePage.TranslationY = origSrcTy;

			// Animate up destination content
			if (destination != null)
				await destination.TranslateTo (0, origDestTy, easing: Easing.SpringOut);
		}
	}
}
