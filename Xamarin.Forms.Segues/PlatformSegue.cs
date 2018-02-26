#if !NETSTANDARD2_0
using System;

#if __IOS__
using Xamarin.Forms.Platform.iOS;
#else
#error Unknown platform
#endif

namespace Xamarin.Forms.Segues {

	public abstract class PlatformSegue : PlatformEffect {

		protected override void OnAttached ()
		{
		}

		protected override void OnDetached ()
		{
		}
	}
}
#endif