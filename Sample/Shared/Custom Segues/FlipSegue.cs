using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Segues;

namespace Sample {

	public class FlipSegue : Segue {
		protected override async Task ExecuteAsync (Page destination)
		{
			var src = SourcePage;
			var dest = destination;

			// Save initial values
			var origSrcRy = src.RotationY;
			var origDestRy = 0d;
			if (dest != null) {
				origDestRy = dest.RotationY;
				dest.RotationY = -90;
			}

			// Animate SourcePage
			await src.RotateYTo (90, length: 150);

			// Actually switch pages
			await base.ExecuteAsync (destination);

			// IMPORTANT! Restore source content to original settings, otherwise
			//  things will look weird if we pop back to it.
			src.RotationY = origSrcRy;

			// Animate up destination content
			if (dest != null)
				await dest.RotateYTo (origDestRy, length: 150);
		}
	}
}
