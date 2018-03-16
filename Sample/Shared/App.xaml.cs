using Xamarin.Forms;
using Xamarin.Forms.Segues;

// This fails because it's not smart enough to detect ISegue inherits from ICommand
//[assembly:Xamarin.Forms.Xaml.XamlCompilation(Xamarin.Forms.Xaml.XamlCompilationOptions.Compile)]

namespace Sample {
	public partial class App : Application {
		public App ()
		{
			// Register all our cross-platform custom segue types and give them succinct names...
			Segue.RegisterType<DownUpSegue> ("DownUp");
			Segue.RegisterType<FadeInOutSegue> ("Fade");
			Segue.RegisterType<FlipSegue> ("Flip");
			Segue.RegisterType<SpinnerSegue> ("Spinner");

			InitializeComponent ();
		}
	}
}
