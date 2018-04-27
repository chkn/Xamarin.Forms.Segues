#if !NETSTANDARD2_0
using System;
using System.Windows.Input;
using System.Threading.Tasks;

namespace Xamarin.Forms.Segues {
	#if __IOS__
	using Xamarin.Forms.Platform.iOS;
	using Platform = Xamarin.Forms.Platform.iOS.Platform;
	using NativePage = UIKit.UIViewController;
	#else
	#error Unknown platform
	#endif

	// FIXME: This needs to be better integrated into each platform impl.
	internal sealed class PlatformSegue : Segue {

		protected NativePage NativeSource { get; private set; }

		protected internal PlatformSegue ()
		{
		}

		NativePage GetDestinationForPop ()
			=>
			#if __IOS__
				NativeSource.PresentingViewController ??
				NativeSource.NavigationController?.ViewControllers.GetNextIfTop (NativeSource) ??
				NativeSource.NavigationController?.ViewControllers.GetNextIfTop (NativeSource.ParentViewController);
			#else
			#error Unknown platform
			#endif

		internal static bool IsNativePage (object obj) => obj is NativePage;
		internal static bool IsNativePage (Type ty) => typeof (NativePage).IsAssignableFrom (ty);

		#region ICommand

		protected Task ExecuteAsync (NativePage destination)
		{
			switch (Action) {

				case NavigationAction.Push: {
					#if __IOS__
						var nav = NativeSource.NavigationController;
						if (nav == null)
							throw new InvalidOperationException ("Cannot Push if source is not contained in NavigationPage or UINavigationController");

						// FIXME: Taskify
						nav.PushViewController (destination, true);
						return Task.CompletedTask;
					#else
					#error Unknown platform
					#endif
				}

				case NavigationAction.Modal: {
					#if __IOS__
						return NativeSource.PresentViewControllerAsync (destination, true);
					#else
					#error Unknown platform
					#endif
				}

				case NavigationAction.Pop: {
					#if __IOS__
						var pres = NativeSource.PresentingViewController;
						if (pres != null)
							return pres.DismissViewControllerAsync (true);

						var nav = NativeSource.NavigationController;
						if (nav == null)
							throw new InvalidOperationException ("Cannot Pop if source is not pushed or modal");

						// FIXME: Taskify
						nav.PopViewController (true);
						return Task.CompletedTask;
					#else
					#error Unknown platform
					#endif
				}

				//case NavigationAction.MainPage:
			}
			throw new NotImplementedException (Action.ToString ());
		}

		Task ExecuteNative (NativePage native)
		{
			#if __IOS__
			// KLUDGE! Size the destination to the source to ensure things look right
			if (native != null) {
				native.View.Frame = NativeSource.View.Frame;
				native.View.LayoutIfNeeded ();
			}
			#endif
			return ExecuteAsync (native);
		}

		/// <summary>
		/// Executes this <see cref="PlatformSegue"/> with the specified <see cref="Page"/> destination.
		/// </summary>
		/// <remarks>
		/// The base implementation converts the page to the native type and calls
		///  <see cref="ExecuteAsync(NativePage)"/>. Override this method to provide special handling
		///  for <see cref="Page"/> destinations.
		///
		/// Subclasses can override this method to perform any custom animation desired for <see cref="Page"/>
		///  destinations. Call <c>base.ExecuteAsync</c> or <see cref="ExecuteAsync(Page, bool)"/>
		///  to effect the appropriate change to the navigation stack.
		/// </remarks>
		protected override Task ExecuteAsync (Page destination)
			=> ExecuteNative (GetOrCreateRenderer (destination));

		internal override async Task ExecuteInternal (object destination)
		{
			if (NativeSource == null) {
				if (SourcePage == null)
					throw new InvalidOperationException ("Source element must be on a Page");
				NativeSource = GetOrCreateRenderer (SourcePage);
			}
			try {
				if (Action == NavigationAction.Pop) {
					// Clear out our previous page on the source page to prevent stale data
					SourcePage?.SetPreviousPage (null);
				} else if (destination == null) {
					throw new ArgumentNullException (nameof (destination), $"May only be null for {nameof (NavigationAction.Pop)}");
				} else if (destination is Page page) {
					page.SetPreviousPage (SourcePage);
				}
	
				switch (destination) {
	
				case Page page: await ExecuteAsync (page); break;
				case NativePage native: await ExecuteNative (native); break;
				default: throw new ArgumentException ();
				}
			} finally {
				NativeSource = null;
			}
		}

		#endregion

		static NativePage GetOrCreateRenderer (Page page)
		{
			var renderer = Platform.GetRenderer (page);
			if (renderer == null) {
				renderer = Platform.CreateRenderer (page);
				Platform.SetRenderer (page, renderer);
			}
			return renderer
			#if __IOS__
				.ViewController
			#endif
			;
		}
	}
}
#endif // !NETSTANDARD2_0