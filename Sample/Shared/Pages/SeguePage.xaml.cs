using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Segues;

namespace Sample {
	public partial class SeguePage : ContentPage {

		public SeguePage () => InitializeComponent ();

		async void Handle_Clicked (object sender, System.EventArgs e)
		{
			var model = (SegueModel)BindingContext;

			// Here is an example of creating and executing a segue from code:

			// First, create the segue. Here, we first try to create a custom type
			//  we've registered, falling back to the default Segue type.
			var segue = Segue.Create (model.Type) ?? new Segue ();

			// Optionally, we can override the default segue action with the one we want
			segue.Action = model.Action;

			// Create our destination page and set the binding context
			var destination = new SegueDestPage {
				BindingContext = model
			};

			// Execute the segue
			await segue.ExecuteAsync ((VisualElement)sender, destination);
		}
	}
}
