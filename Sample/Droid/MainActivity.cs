﻿using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Xamarin.Forms.Segues;

namespace Sample.Droid {
	[Activity (Label = "Sample.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
		protected override void OnCreate (Bundle bundle)
		{
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate (bundle);

			// Init segues and forms
			Segue.Init ();
			global::Xamarin.Forms.Forms.Init (this, bundle);

			// Register our native segue types
			// FIXME: Gate is not implemented for Android, so just use default segue type
			Segue.RegisterType<Segue> ("Gate");

			LoadApplication (new App ());
		}
	}
}
