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
		/// Sets <see cref="SourceElement"/> and executes this segue.
		/// </summary>
		Task ExecuteAsync (VisualElement sourceElement, Page destination);
	}
}
