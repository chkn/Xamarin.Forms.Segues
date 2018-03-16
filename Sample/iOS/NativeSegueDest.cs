using System;

using UIKit;

using Xamarin.Forms.Segues;

namespace Sample {
	public partial class NativeSegueDest : UIViewController {
		public NativeSegueDest () : base ("NativeSegueDest", null)
		{
		}

		async partial void UIButton260_TouchUpInside (UIButton sender)
		{
			// Here's an example of using a PlatformSegue from code to segue
			//  from a native iOS UIViewController to a Forms Page:
			var seg = new GateSegue {
				Action = SegueAction.Pop
			};

			// Execute the segue. Destination can be omitted because this is a Pop.
			await seg.ExecuteAsync (this);
		}
	}
}

