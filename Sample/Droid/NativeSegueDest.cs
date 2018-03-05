
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Sample {
	public class NativeSegueDest : View {

		// FIXME: This is obsolete; we'll need to figure out Android
		public NativeSegueDest () : this (Xamarin.Forms.Forms.Context)
		{
		}

		public NativeSegueDest (Context context) :
			base (context)
		{
			Initialize ();
		}

		public NativeSegueDest (Context context, IAttributeSet attrs) :
			base (context, attrs)
		{
			Initialize ();
		}

		public NativeSegueDest (Context context, IAttributeSet attrs, int defStyle) :
			base (context, attrs, defStyle)
		{
			Initialize ();
		}

		void Initialize ()
		{
		}
	}
}
