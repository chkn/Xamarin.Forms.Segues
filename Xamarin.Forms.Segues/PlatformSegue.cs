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
	public class PlatformSegue : Segue {

		protected NativePage NativeSource { get; private set; }

		bool IsSubclass => GetType () != typeof (PlatformSegue);

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

		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Called by subclasses to raise the <see cref="CanExecuteChanged"/> event. 
		/// </summary>
		protected void RaiseCanExecuteChanged ()
			=> CanExecuteChanged?.Invoke (this, EventArgs.Empty);

		/// <summary>
		/// Returns a value indicating if this <see cref="PlatformSegue"/> can be executed
		///  with the given destination.
		/// </summary>
		/// <remarks>
		/// <remarks>
		/// This method calls <see cref="CanExecute(Type)"/> with the <see cref="Type"/>
		///   of the passed destination.
		/// </remarks>
		public bool CanExecute (NativePage destination)
			// Can call CanExecuteOverride directly since we know it derives from NativePage
			=> CanExecuteOverride (destination.GetType());

		// This modifies the behavior of CanExecute(Type) to allow types that derive from NativePage
		//  to be passed along to CanExecuteOverride.
		internal override bool CanExecuteInternal (Type ty)
			=> (IsNativePage (ty) || typeof (Page).IsAssignableFrom (ty))
			&& CanExecuteOverride (ty);

		/// <summary>
		/// Executes this <see cref="PlatformSegue"/> with the specified native destination.
		/// </summary>
		/// <param name="destination">
		///  Destination. May be <c>null</c> in the case of <see cref="SegueAction.Pop"/>.
		/// </param>
		/// <remarks>
		/// Subclasses should override this method to perform any custom animation.
		///  Call <c>base.ExecuteAsync</c> to effect the appropriate change to the
		///  navigation stack.
		/// </remarks>
		protected virtual Task ExecuteAsync (NativePage destination)
		{
			// Animate the default segue type, but allow subclasses to define
			//  their own animations..
			var animated = !IsSubclass;
			switch (Action) {

				case SegueAction.Push: {
					#if __IOS__
						var nav = NativeSource.NavigationController;
						if (nav == null)
							throw new InvalidOperationException ("Cannot Push if source is not contained in NavigationPage or UINavigationController");

						// FIXME: Taskify
						nav.PushViewController (destination, animated);
						return Task.CompletedTask;
					#else
					#error Unknown platform
					#endif
				}

				case SegueAction.Modal: {
					#if __IOS__
						return NativeSource.PresentViewControllerAsync (destination, animated);
					#else
					#error Unknown platform
					#endif
				}

				case SegueAction.Pop: {
					#if __IOS__
						var pres = NativeSource.PresentingViewController;
						if (pres != null)
							return pres.DismissViewControllerAsync (animated);

						var nav = NativeSource.NavigationController;
						if (nav == null)
							throw new InvalidOperationException ("Cannot Pop if source is not pushed or modal");

						// FIXME: Taskify
						nav.PopViewController (animated);
						return Task.CompletedTask;
					#else
					#error Unknown platform
					#endif
				}

				//case SegueAction.MainPage:
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
		///  destinations. Call <c>base.ExecuteAsync</c> to effect the appropriate change to the
		///  navigation stack.
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
				if (Action == SegueAction.Pop) {
					// Before we can call ExecuteAsync, we must ensure that the destination is
					//  passed for Pop for subclasses (built-in segs don't care)
					if (IsSubclass)
						destination = GetDestinationForPop ();
					// Clear out our previous page on the source page to prevent stale data
					SourcePage?.SetPreviousPage (null);
				} else if (destination == null) {
					throw new ArgumentNullException (nameof (destination), $"May only be null for {nameof (SegueAction.Pop)}");
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

		/// <summary>
		/// Executes this <see cref="Segue"/> with the specified source and destination.
		/// </summary>
		public Task ExecuteAsync (VisualElement sourceElement, NativePage destination)
		{
			if (sourceElement == null)
				throw new ArgumentNullException (nameof (sourceElement));
			SourceElement = sourceElement;
			return ExecuteInternal (destination);
		}

		/// <summary>
		/// Executes this <see cref="Segue"/> with the specified source and destination.
		/// </summary>
		/// <param name="destination">
		///  Destination <see cref="Page"/>. Should be omitted (<c>null</c>) if and only if
		///   <see cref="Segue.Action"/> is set to <see cref="SegueAction.Pop"/>.
		/// </param>
		public Task ExecuteAsync (NativePage nativeSource, Page destination = null)
		{
			if (nativeSource == null)
				throw new ArgumentNullException (nameof (nativeSource));
			NativeSource = nativeSource;
			return ExecuteInternal (destination);
		}

		/// <summary>
		/// Executes this <see cref="Segue"/> with the specified source and destination.
		/// </summary>
		public Task ExecuteAsync (NativePage nativeSource, NativePage nativeDestination)
		{
			if (nativeSource == null)
				throw new ArgumentNullException (nameof (nativeSource));
			NativeSource = nativeSource;
			return ExecuteInternal (nativeDestination);
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