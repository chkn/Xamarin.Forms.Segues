using System;
using System.Windows.Input;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace Xamarin.Forms.Segues {

	/// <summary>
	/// A <see cref="ICommand"/> that implements navigation.
	/// </summary>
	public interface ISegue : ICommand {

		/// <summary>
		/// Gets or sets the action that this segue should perform.
		/// </summary>
		SegueAction Action { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="VisualElement"/> that serves as the source for this segue.
		/// </summary>
		VisualElement SourceElement { get; set; }

		/// <summary>
		/// Returns a value indicating if this <see cref="ISegue"/> can be executed
		///  with the given <see cref="Type"/> of destination.
		/// </summary>
		bool CanExecute (Type destinationType);

		/// <summary>
		/// Sets <see cref="SourceElement"/> and executes this segue.
		/// </summary>
		Task ExecuteAsync (VisualElement sourceElement, Page destination);
	}

	public static class SegueExtensions {

		/// <summary>
		/// Returns a value indicating if this <see cref="ISegue"/> can be executed
		///  with the given destination <see cref="Page"/>.
		/// </summary>
		/// <remarks>
		/// This method calls <see cref="ISegue.CanExecute(Type)"/> with the <see cref="Type"/>
		///   of the passed destination <see cref="Page"/>.
		/// </remarks>
		public static bool CanExecute (this ISegue segue, Page destination)
			=> segue.CanExecute (destination.GetType ());
	}
}
