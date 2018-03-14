#if !NETSTANDARD2_0
using System;
using System.Windows.Input;
using System.Threading.Tasks;

#if __IOS__
using Xamarin.Forms.Platform.iOS;
using NativePage = UIKit.UIViewController;
#else
#error Unknown platform
#endif

namespace Xamarin.Forms.Segues {

	public class PlatformSegue : PlatformEffect, ISegue {

		public SegueAction Action { get; set; }

		VisualElement ISegue.SourceElement {
			get => sourceElem;
			set => sourceElem = value;
		}
		VisualElement sourceElem;

		bool IsCustomSegue => GetType () != typeof (PlatformSegue);

		bool attaching;

		protected NativePage NativeSource { get; private set; }

		protected internal PlatformSegue ()
		{
		}

		void Attach ()
		{
			if (sourceElem == null)
				return;
			var page = sourceElem.GetPage ();
			if (page == null)
				throw new InvalidOperationException ("Source element must be on a Page");
			attaching = true;
			page.Effects.Add (this);
		}
		protected override void OnAttached ()
		{
			if (!attaching)
				throw new NotSupportedException ($"{nameof(PlatformSegue)} cannot be added as an Effect");
			attaching = false;

			NativeSource =
				#if __IOS__
				// FIXME: kludge
				(NativePage)Container.NextResponder;
				#else
				Container;
				#endif
		}

		void Detach ()
		{
			if (IsAttached)
				Element.Effects.Remove (this);
		}
		protected override void OnDetached ()
		{
			NativeSource = null;
			attaching = false;
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

		internal static bool IsNativePage (object obj)
			=> obj is NativePage;

		static NativePage ConvertToNative (Page page)
		{
			return
			#if __IOS__
				page?.CreateViewController ();
			#else
			#error Unknown platform
			#endif
		}

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
		/// Subclasses that override this method should call
		///  <see cref="RaiseCanExecuteChanged"/> as appropriate.
		/// </remarks>
		public virtual bool CanExecute (NativePage destination)
			=> destination != null || Action == SegueAction.Pop;

		/// <summary>
		/// Returns a value indicating if this <see cref="PlatformSegue"/> can be executed
		///  with the given destination.
		/// </summary>
		/// <remarks>
		/// The base method converts the given <see cref="Page"/> into a native type and
		///  calls the native overload of this method.
		/// <para/>
		/// Subclasses that override this method should call
		///  <see cref="RaiseCanExecuteChanged"/> as appropriate.
		/// </remarks>
		public virtual bool CanExecute (Page destination)
			=> CanExecute (ConvertToNative (destination));

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
			var animated = !IsCustomSegue;
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
		/// <param name="destination">
		///  Destination <see cref="Page"/>. May be <c>null</c> in the case of <see cref="SegueAction.Pop"/>.
		/// </param>
		/// <remarks>
		/// The base implementation converts the page to the native type and calls
		///  <see cref="ExecuteAsync(NativePage)"/>. Override this method to provide special handling
		///  for <see cref="Page"/> destinations.
		///
		/// Subclasses can override this method to perform any custom animation desired for <see cref="Page"/>
		///  destinations. Call <c>base.ExecuteAsync</c> to effect the appropriate change to the
		///  navigation stack.
		/// </remarks>
		protected virtual Task ExecuteAsync (Page destination)
			=> ExecuteNative (ConvertToNative (destination));

		internal async Task ExecuteInternal (VisualElement sourceElement, object destination)
		{
			var originalSource = sourceElem;
			sourceElem = sourceElement;
			try {
				Attach ();

				// Ensure that the destination is passed for Pop.
				//  Our built-in seg doesn't care, but subclasses might.
				if (Action == SegueAction.Pop && IsCustomSegue)
					destination = GetDestinationForPop ();

				if (destination == null) {
					await ExecuteAsync ((Page)null);
				} else {
					switch (destination) {

					case Page page: await ExecuteAsync (page); break;
					case NativePage native: await ExecuteNative (native); break;
					default: throw new ArgumentException ();
					}
				}
			} finally {
				sourceElem = originalSource;
				Detach ();
			}
		}

		public Task ExecuteAsync (VisualElement sourceElement, NativePage destination)
		{
			if (sourceElement == null)
				throw new ArgumentNullException (nameof (sourceElement));
			return ExecuteInternal (sourceElement, destination);
		}

		public Task ExecuteAsync (VisualElement sourceElement, Page destination) {
			if (sourceElement == null)
				throw new ArgumentNullException (nameof (sourceElement));
			return ExecuteInternal (sourceElement, destination);
		}

		public async Task ExecuteAsync (NativePage nativeSource, Page destination)
		{
			if (nativeSource == null)
				throw new ArgumentNullException (nameof (nativeSource));

			NativeSource = nativeSource;
			try {
				await ExecuteInternal (null, destination);
			} finally {
				NativeSource = null;
			}
		}

		bool ICommand.CanExecute (object parameter)
		{
			if (parameter == null)
				return CanExecute ((Page)null);

			switch (parameter) {

			// FIXME: Ideally, we could know the type created by the template w/o instantiating it
			//  (we can't instantiate it here)
			case DataTemplate _: return true;
			case Page page: return CanExecute (page);
			case NativePage native: return CanExecute (native);
			}
			return false;
		}
		async void ICommand.Execute (object parameter)
		{
			if (sourceElem == null)
				throw new InvalidOperationException ("SourceElement property must be set before segue is executed");

			if (parameter is DataTemplate template)
				parameter = template.CreateContent ();

			await ExecuteInternal (sourceElem, parameter);
		}

		#endregion
	}
}
#endif