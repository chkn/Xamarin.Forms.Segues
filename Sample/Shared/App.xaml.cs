using Xamarin.Forms;
using Xamarin.Forms.Segues;

// This fails because it's not smart enough to detect ISegue inherits from ICommand
//[assembly:Xamarin.Forms.Xaml.XamlCompilation(Xamarin.Forms.Xaml.XamlCompilationOptions.Compile)]

namespace Sample {
	public partial class App : Application {
		public App ()
		{
			// First, we initialize the Segue runtime.
			Segue.Init ();

			// Next, we must register all our custom types and give them succinct names...
			Segue.RegisterType<DownUpSegue> ("DownUp");
			Segue.RegisterType<FadeInOutSegue> ("Fade");

			InitializeComponent ();
		}
	}
}
