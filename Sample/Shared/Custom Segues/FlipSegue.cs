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
			var origDestRy = dest.RotationY;

			// Animate SourcePage and set (but do not animate) dest page
			await src.RotateYTo (90, length: 150);
			dest.RotationY = -90;

			// Actually switch pages
			await base.ExecuteAsync (destination);

			// IMPORTANT! Restore source content to original settings, otherwise
			//  things will look weird if we pop back to it.
			src.RotationY = origSrcRy;

			// Animate up destination content
			await dest.RotateYTo (origDestRy, length: 150);
		}
	}
}
