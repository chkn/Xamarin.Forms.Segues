using System;

using Xamarin.Forms;

namespace Xamarin.Forms.Segues {

	/// <summary>
	/// The desired action for a segue to perform.
	/// </summary>
	[TypeConverter (typeof (SegueActionConverter))]
	public enum SegueAction {
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

	public class SegueActionConverter : TypeConverter {

		public static bool TryParse (string value, out SegueAction action)
		{
			switch (value.ToLowerInvariant ()) {
			case "push"    : action = SegueAction.Push; return true;
			case "modal"   : action = SegueAction.Modal; return true;
			case "pop"     : action = SegueAction.Pop; return true;
			case "mainpage": action = SegueAction.MainPage; return true;
			}
			action = default (SegueAction);
			return false;
		}

		public static SegueAction Parse (string value)
		{
			SegueAction result;
			if (!TryParse (value, out result))
				throw new ArgumentException ();
			return result;
		}

		public override object ConvertFromInvariantString (string value) => Parse (value);
	}
}
