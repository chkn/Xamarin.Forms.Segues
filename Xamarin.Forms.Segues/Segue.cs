using System;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Xamarin.Forms.Segues {

	/// <summary>
	/// A navigational transition from one <see cref="Page"/> to another.
	/// </summary>
	/// <remarks>
	/// Custom segues can be created by subclassing this class. During application initialization,
	///  call <see cref="Register"/> to register your subclass. Then, create them in XAML using
	///  the {Segue} markup extension, or in code using the <see cref="Create"/> method. 
	/// </remarks>
	public class Segue : ISegue {

		public SegueAction Action { get; set; }

		public VisualElement SourceElement {
			get => sourceElem;
			set {
				if (sourceElem != value) {
					sourceElem = value;
					sourcePage = null;
				}
			}
		}
		VisualElement sourceElem;

		/// <summary>
		/// A convenience property to return the <see cref="Page"/> of the <see cref="SourceElement"/>.
		/// </summary>
		/// <remarks>
		/// This property is lazily computed by walking up the parent-child hierarchy from the
		///  <see cref="SourceElement"/> until an ancestor <see cref="Page"/> is encountered.
		/// If no ancestor <see cref="Page"/> is encountered, or if <see cref="SourceElement"/> is
		///  <c>null</c>, this property returns <c>null</c>.
		/// </remarks>
		public Page SourcePage {
			get {
				if (sourcePage == null)
					sourcePage = sourceElem.GetPage ();
				return sourcePage;
			}
		}
		Page sourcePage;

		bool IsCustomSegue => GetType () != typeof (Segue);

		// FIXME: Make this work for modal
		Page GetDestinationForPop ()
			=> SourceElement.Navigation.NavigationStack.GetNextIfTop (SourcePage);

		// Prevents link out
		public static void Init ()
		{
		}

		#region ICommand

		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Called by subclasses to raise the <see cref="CanExecuteChanged"/> event. 
		/// </summary>
		protected void RaiseCanExecuteChanged ()
			=> CanExecuteChanged?.Invoke (this, EventArgs.Empty);

		/// <summary>
		/// Returns a value indicating if this <see cref="Segue"/> can be executed
		///  with the given destination <see cref="Page"/>.
		/// </summary>
		/// <remarks>
		/// Subclasses that override this method should call
		///  <see cref="RaiseCanExecuteChanged"/> as appropriate.
		/// </remarks>
		public virtual bool CanExecute (Page destination)
			=> destination != null || Action == SegueAction.Pop;

		/// <summary>
		/// Executes this <see cref="Segue"/> with the specified destination.
		/// </summary>
		/// <param name="destination">
		///  Destination <see cref="Page"/>. May be <c>null</c> in the case of
		///   <see cref="SegueAction.Pop"/> when the <see cref="SourcePage"/>
		///   was not pushed.
		/// </param>
		/// <remarks>
		/// Subclasses should override this method to perform any custom animation.
		///  Call <c>base.ExecuteAsync</c> to effect the appropriate change to the
		///  navigation stack.
		/// </remarks>
		protected virtual Task ExecuteAsync (Page destination)
		{
			var animated = !IsCustomSegue;
			switch (Action) {

				case SegueAction.Push:
					return SourceElement.Navigation.PushAsync (destination, animated);
				case SegueAction.Modal:
					return SourceElement.Navigation.PushModalAsync (destination, animated);

				case SegueAction.Pop: {
						var srcPage = SourcePage;
						if (srcPage == null)
							throw new InvalidOperationException ($"{nameof (SourceElement)} must be on a Page");
						var nav = srcPage.Navigation;
						if (nav.ModalStack.IsTop (srcPage))
							return nav.PopModalAsync (animated);
						if (nav.NavigationStack.IsTop (srcPage))
							return nav.PopAsync (animated);
						nav.RemovePage (srcPage);
						return Task.CompletedTask;
					}

				case SegueAction.MainPage:
					Application.Current.MainPage = destination;
					return Task.CompletedTask;
			}
			throw new NotImplementedException (Action.ToString ());
		}

		Task ExecuteInternal (Page destination)
		{
			// Before we can call ExecuteAsync, we must ensure that the destination is
			//  passed for Pop. Our built-in seg doesn't care, but subclasses might.
			if (Action == SegueAction.Pop && IsCustomSegue)
				destination = GetDestinationForPop ();

			return ExecuteAsync (destination);
		}

		public async Task ExecuteAsync (VisualElement sourceElement, Page destination)
		{
			if (sourceElement == null)
				throw new ArgumentNullException (nameof (sourceElement));

			var originalSource = SourceElement;
			SourceElement = sourceElement;
			try {
				await ExecuteInternal (destination);
			} finally {
				SourceElement = originalSource;
			}
		}

		bool ICommand.CanExecute (object parameter)
		{
			// FIXME: Ideally, we could know the type created by the template w/o instantiating it
			//  (we can't instantiate it here)
			if (parameter is DataTemplate)
				return true;

			#if !NETSTANDARD2_0
			// If this is the native object, we will delegate to PlatformSegue
			if (PlatformSegue.IsNativePage (parameter))
				return !IsCustomSegue;
			#endif

			return CanExecute (parameter as Page);
		}

		async void ICommand.Execute (object parameter)
		{
			if (SourceElement == null)
				throw new InvalidOperationException ($"{nameof (SourceElement)} property must be set before segue is executed");

			if (parameter is DataTemplate template)
				parameter = template.CreateContent ();

			#if !NETSTANDARD2_0
			// If this is the native object, we will delegate to PlatformSegue
			if (PlatformSegue.IsNativePage (parameter)) {
				if (IsCustomSegue)
					throw new ArgumentException ("Custom segues must derive from PlatformSegue to handle native objects", nameof(parameter));

				var platSeg = new PlatformSegue {
					Action = Action
				};
				await platSeg.ExecuteInternal (SourceElement, parameter);
				return;
			}
			#endif
			await ExecuteInternal ((Page)parameter);
		}

		#endregion

		#region Registry

		static Dictionary<string, Type> registry;

		public static IEnumerable<string> TypeNames => registry?.Keys ?? Enumerable.Empty<string> ();

		public static void RegisterType<TSegue> (string typeName)
			where TSegue : ISegue, new()
		{
			if (registry == null)
				registry = new Dictionary<string, Type> ();
			registry [typeName] = typeof (TSegue);
		}

		/// <summary>
		/// Gets a new instance of the custom segue registered under the given type name, or null
		///  if none was registered under that name.
		/// </summary>
		/// <remarks>
		/// The segue type must have previously been registered by calling <see cref="RegisterType"/>.
		/// </remarks>
		/// <returns>The segue, or null.</returns>
		public static ISegue Create (string typeName)
		{
			if (typeName != null && registry != null && registry.TryGetValue (typeName, out var type))
				return (ISegue)Activator.CreateInstance (type);

			return null;
		}

		#endregion
	}
}
