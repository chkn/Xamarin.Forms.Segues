using System;

using Xamarin.Forms;

namespace Xamarin.Forms.Segues {

	/// <summary>
	/// The desired action for a segue to perform.
	/// </summary>
	[TypeConverter (typeof (NavigationActionConverter))]
	public enum NavigationAction {
		/// <summary>
		/// Corresponds to calling <see cref="INavigation.PushAsync"/>.
		/// </summary>
		Push,

		/// <summary>
		/// Corresponds to calling <see cref="INavigation.PushModalAsync"/>.
		/// </summary>
		Modal,

		/// <summary>
		/// Corresponds to calling either <see cref="INavigation.PopAsync"/> or
		///  <see cref="INavigation.PopModalAsync"/>, depending on which navigation
		///  stack the source page is on.
		/// </summary>
		Pop,

		/// <summary>
		/// Corresponds to setting <see cref="Application.MainPage"/>.
		/// </summary>
		MainPage,
	}

	public class NavigationActionConverter : TypeConverter {

		public static bool TryParse (string value, out NavigationAction action)
		{
			switch (value.ToLowerInvariant ()) {
			case "push"    : action = NavigationAction.Push; return true;
			case "modal"   : action = NavigationAction.Modal; return true;
			case "pop"     : action = NavigationAction.Pop; return true;
			case "mainpage": action = NavigationAction.MainPage; return true;
			}
			action = default (NavigationAction);
			return false;
		}

		public static NavigationAction Parse (string value)
		{
			NavigationAction result;
			if (!TryParse (value, out result))
				throw new ArgumentException ();
			return result;
		}

		public override object ConvertFromInvariantString (string value) => Parse (value);
	}
}
