using AppKit;
using Foundation;
using CoreGraphics;

using Xamarin.Forms.Segues;
using Xamarin.Forms.Platform.MacOS;

namespace Sample.Mac {
	[Register ("AppDelegate")]
	public class AppDelegate : FormsApplicationDelegate {

		NSWindow window;
		public override NSWindow MainWindow => window;

		public override void DidFinishLaunching (NSNotification notification)
		{
			// Init segues and forms
			Segue.Init ();
			Xamarin.Forms.Forms.Init ();

			// Start the app
			window = new NSWindow (
				new CGRect (0, 0, 400, 600),
				NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable | NSWindowStyle.Titled,
				NSBackingStore.Buffered,
				deferCreation: false);
			LoadApplication (new App ());
			base.DidFinishLaunching (notification);
			window.Center ();
		}

		public override bool ApplicationShouldTerminateAfterLastWindowClosed (NSApplication sender)
			=> true;
	}
}
