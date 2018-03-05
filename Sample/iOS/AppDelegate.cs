using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

using Xamarin.Forms.Segues;

namespace Sample.iOS {
	[Register ("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate {
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// Init segues and forms
			Segue.Init ();
			global::Xamarin.Forms.Forms.Init ();

			// Register our native segue types
			Segue.RegisterType<GateSegue> ("Gate");

			LoadApplication (new Sample.App ());

			return base.FinishedLaunching (app, options);
		}
	}
}
