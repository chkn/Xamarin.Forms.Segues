using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace Sample {
	public partial class NativeSegueDest : AppKit.NSViewController {
		#region Constructors

		// Called when created from unmanaged code
		public NativeSegueDest (IntPtr handle) : base (handle)
		{
		}

		public NativeSegueDest () : base ("NativeSegueDest", null)
		{
		}

		#endregion
	}
}
