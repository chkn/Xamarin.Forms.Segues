using System;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Xamarin.Forms.Segues {

	/// <summary>
	/// A <see cref="ICommand"/> that implements navigation between <see cref="Page"/>s.
	/// </summary>
	public interface ISegue : ICommand {

		/// <summary>
		/// Gets or sets the action that this segue should perform.
		/// </summary>
		SegueAction Action { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="VisualElement"/> that serves as the source for this segue.
		/// </summary>
		/// <remarks>
		/// This property must be set before the segue is executed. It is automatically
		///  set when the segue is created in XAML using the {Segue} markup extension.
		/// </remarks>
		VisualElement SourceElement { get; set; }
	}

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
					sourcePage = GetPage (sourceElem);
				return sourcePage;
			}
		}
		Page sourcePage;

		bool IsCustomSegue => GetType () != typeof (Segue);

		// Prevents link out
		public static void Init ()
		{
		}

		internal static Task ExecuteSegueAction (ISegue seg, Page dest, bool animated)
		{
			var src = seg.SourceElement;
			if (src == null)
				throw new InvalidOperationException ($"{nameof(SourceElement)} property must be set before segue is executed");

			switch (seg.Action) {

			case SegueAction.Push: return src.Navigation.PushAsync (dest, animated);
			case SegueAction.Modal: return src.Navigation.PushModalAsync (dest, animated);

			case SegueAction.Pop: {
				var srcPage = (seg as Segue)?.SourcePage ?? GetPage (src);
				if (srcPage == null)
					throw new InvalidOperationException ($"{nameof(SourceElement)} must be on a Page");
				var nav = src.Navigation;
				if (IsTop (nav.ModalStack, srcPage))
					return nav.PopModalAsync (animated);
				if (IsTop (nav.NavigationStack, srcPage))
					return nav.PopAsync (animated);
				nav.RemovePage (srcPage);
				return Task.CompletedTask;
			}

			case SegueAction.MainPage:
				Application.Current.MainPage = dest;
				return Task.CompletedTask;
			}
			throw new NotImplementedException (seg.Action.ToString ());
		}

		Page GetDestinationForPop ()
			=> GetSecondToTop (SourceElement.Navigation.NavigationStack, SourcePage);

		static bool IsTop (IReadOnlyList<Page> stack, Page page)
		{
			var cnt = stack.Count;
			return cnt > 0 && stack [cnt - 1] == page;
		}
		static Page GetSecondToTop (IReadOnlyList<Page> stack, Page page)
		{
			var cnt = stack.Count;
			return (cnt > 1 && stack [cnt - 1] == page)? stack [cnt - 2] : null;
		}

		static Page GetPage (Element elem)
		{
			while (elem != null && !(elem is Page))
				elem = elem.Parent;
			return (Page)elem;
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
		///   <see cref="SegueAction.Pop"/> when the <see cref="SourcePage"/> is
		///   presented modally.</param>
		/// <remarks>
		/// Subclasses should override this method to perform any custom animation.
		///  Call <c>base.ExecuteAsync</c> to effect the appropriate change to the
		///  navigation stack.
		/// </remarks>
		protected virtual Task ExecuteAsync (Page destination)
			=> ExecuteSegueAction (this, destination, animated: !IsCustomSegue);

		public async void Execute (Page destination)
		{
			// Before we can call ExecuteAsync, we must ensure that the destination is
			//  passed for Pop. Our built-in seg doesn't care, but subclasses might.
			if (Action == SegueAction.Pop && IsCustomSegue)
				destination = GetDestinationForPop ();

			await ExecuteAsync (destination);
		}

		bool ICommand.CanExecute (object parameter) => CanExecute (parameter as Page);
		void ICommand.Execute (object parameter) => Execute ((Page)parameter);

		#endregion

		#region Registry

		static Dictionary<string,Type> registry;

		public static IEnumerable<string> TypeNames => registry?.Keys ?? Enumerable.Empty<string> ();

		public static void RegisterType<TSegue> (string typeName)
			where TSegue : ISegue, new()
		{
			if (registry == null)
				registry = new Dictionary<string,Type> ();
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
