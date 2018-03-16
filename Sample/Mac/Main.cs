using AppKit;

namespace Sample.Mac {
	static class MainClass {
		static void Main (string [] args)
		{
			NSApplication.Init ();

			// Set our app delegate since we're not using a storyboard
			NSApplication.SharedApplication.Delegate = new AppDelegate ();
			// Prevent NRE in Forms by setting MainMenu to something
			NSApplication.SharedApplication.MainMenu = new NSMenu ();

			NSApplication.Main (args);
		}
	}
}
