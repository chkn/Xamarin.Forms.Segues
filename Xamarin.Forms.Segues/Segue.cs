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
	public class Segue : BindableObject, ICommand {

		public static readonly BindableProperty ActionProperty =
			BindableProperty.Create (nameof (Action), typeof (NavigationAction), typeof (Segue), default (NavigationAction));

		public static readonly BindableProperty SourceElementProperty =
			BindableProperty.Create (nameof (SourceElement), typeof (VisualElement), typeof (Segue), propertyChanged: (s, _, __) => {
				// FIXME: Also listen for value's parent to change to invalidate this?
				// FIXME: It could be a different element on the same page
				var seg = (Segue)s;
				seg.sourcePage = null;
				seg.OnPropertyChanged (nameof (SourcePage));
			});

		public NavigationAction Action {
			get => (NavigationAction)GetValue (ActionProperty);
			set => SetValue (ActionProperty, value);
		}

		public VisualElement SourceElement {
			get => (VisualElement)GetValue (SourceElementProperty);
			set => SetValue (SourceElementProperty, value);
		}

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
					sourcePage = SourceElement.GetPage ();
				return sourcePage;
			}
		}
		Page sourcePage;

		bool IsSubclass => GetType () != typeof (Segue);

		// FIXME: Make this work for modal
		Page GetDestinationForPop ()
		{
			var nav = SourceElement.Navigation;
			return nav.ModalStack.GetNextIfTop (sourcePage)
			    ?? nav.NavigationStack.GetNextIfTop (SourcePage)
			    // Sometimes the above may not give us back the page, if for instance it's a modal,
			    //  so also check for our attached property..
			    ?? SourcePage.GetPreviousPage ()
			    // Otherwise, fall back to MainPage (FIXME)
			    ?? Application.Current.MainPage;
		}

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
		///  with the given <see cref="Type"/> of destination.
		/// </summary>
		/// <remarks>
		/// This method calls <see cref="CanExecuteOverride(Type)"/> after ensuring
		///  the given <see cref="Type"/> derives from the proper base type.
		/// </remarks>
		public bool CanExecute (Type destinationType)
			=> typeof (Page).IsAssignableFrom (destinationType)
			&& CanExecuteOverride (destinationType);

		/// <summary>
		/// Returns a value indicating if this <see cref="Segue"/> can be executed
		///  with the given destination <see cref="Page"/>.
		/// </summary>
		/// <remarks>
		/// This method calls <see cref="CanExecuteOverride(Type)"/> with the <see cref="Type"/>
		///   of the passed destination <see cref="Page"/>.
		/// </remarks>
		public bool CanExecute (Page destination)
			// Can call CanExecuteOverride directly since we know it derives from Page
			=> CanExecuteOverride (destination.GetType ());

		/// <summary>
		/// Returns a value indicating if this <see cref="Segue"/> can be executed
		///  with the given <see cref="Type"/> of destination.
		/// <remarks>
		/// Subclasses that override this method should call
		///  <see cref="RaiseCanExecuteChanged"/> as appropriate.
		/// </remarks>
		protected virtual bool CanExecuteOverride (Type destinationType)
			=> true;

		/// <summary>
		/// Executes this <see cref="Segue"/> with the specified destination.
		/// </summary>
		/// <remarks>
		/// Subclasses should override this method to perform any custom animation.
		///  Call <c>base.ExecuteAsync</c> or <see cref="ExecuteAsync(Page, bool)"/>
		///  to effect the appropriate change to the navigation stack.
		/// </remarks>
		protected virtual Task ExecuteAsync (Page destination)
			// Animate the default segue type, but allow subclasses to define
			//  their own animations..
			=> ExecuteAsync (destination, useDefaultAnimation: !IsSubclass);

		protected Task ExecuteAsync (Page destination, bool useDefaultAnimation)
		{
			switch (Action) {

				case NavigationAction.Push:
					return SourceElement.Navigation.PushAsync (destination, useDefaultAnimation);
				case NavigationAction.Modal:
					return SourceElement.Navigation.PushModalAsync (destination, useDefaultAnimation);

				case NavigationAction.Pop: {
						var srcPage = SourcePage;
						if (srcPage == null)
							throw new InvalidOperationException ($"{nameof (SourceElement)} must be on a Page");
						var nav = srcPage.Navigation;
						if (nav.ModalStack.IsTop (srcPage))
							return nav.PopModalAsync (useDefaultAnimation);
						if (nav.NavigationStack.IsTop (srcPage))
							return nav.PopAsync (useDefaultAnimation);
						nav.RemovePage (srcPage);
						return Task.CompletedTask;
					}

				case NavigationAction.MainPage:
					Application.Current.MainPage = destination;
					return Task.CompletedTask;
			}
			throw new NotImplementedException (Action.ToString ());
		}

		internal virtual Task ExecuteInternal (object destination)
		{
			#if !NETSTANDARD2_0
			// If this is the native object, and this is not a subclass of Segue,
			//  we will delegate to PlatformSegue
			if (PlatformSegue.IsNativePage (destination)) {
				if (IsSubclass)
					throw new ArgumentException ($"Custom segues cannot handle native objects");
				return new PlatformSegue {
					Action = Action,
					SourceElement = SourceElement
				}.ExecuteInternal (destination);
			}
			#endif
			var page = (Page)destination;
			if (Action == NavigationAction.Pop) {
				// Before we can call ExecuteAsync, we must ensure that the destination is
				//  passed for Pop for subclasses (built-in segs don't care)
				if (IsSubclass)
					page = GetDestinationForPop ();
				// Clear out our previous page on the source page to prevent stale data
				SourcePage?.SetPreviousPage (null);
			} else if (destination == null) {
				throw new ArgumentNullException (nameof (destination), $"May only be null for {nameof (NavigationAction.Pop)}");
			} else {
				page.SetPreviousPage (SourcePage);
			}
			return ExecuteAsync (page);
		}

		/// <summary>
		/// Executes this <see cref="Segue"/> with the specified source and destination.
		/// </summary>
		/// <param name="destination">
		///  Destination <see cref="Page"/>. Should be omitted (<c>null</c>) if and only if
		///   <see cref="Segue.Action"/> is set to <see cref="NavigationAction.Pop"/>.
		/// </param>
		public async Task ExecuteAsync (VisualElement sourceElement, Page destination = null)
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
			// We do allow parameter to be null for a Pop (we'll determine the destination if we're executed)
			if (parameter == null)
				return Action == NavigationAction.Pop;

			// FIXME: Ideally, we could know the type created by the template w/o instantiating it
			//  (we can't instantiate it here)
			if (parameter is DataTemplate)
				return true;

			var ty = (parameter as Type) ?? parameter.GetType ();

			#if !NETSTANDARD2_0
			// If this is the native object, we will delegate to PlatformSegue
			if (!IsSubclass && PlatformSegue.IsNativePage (ty))
				return true;
			#endif

			return CanExecute (ty);
		}

		async void ICommand.Execute (object parameter)
		{
			if (SourceElement == null)
				throw new InvalidOperationException ($"{nameof (SourceElement)} property must be set before segue is executed");

			if (parameter is DataTemplate template)
				parameter = template.CreateContent ();

			if (parameter is Type ty)
				parameter = Activator.CreateInstance (ty);

			await ExecuteInternal (parameter);
		}

		#endregion

		#region Registry

		static Dictionary<string, Type> registry;

		public static IEnumerable<string> TypeNames => registry?.Keys ?? Enumerable.Empty<string> ();

		public static void RegisterType<TSegue> (string typeName)
			where TSegue : Segue, new()
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
		public static Segue Create (string typeName)
		{
			if (typeName != null && registry != null && registry.TryGetValue (typeName, out var type))
				return (Segue)Activator.CreateInstance (type);

			return null;
		}

		#endregion
	}
}
