﻿using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Segues;

namespace Sample {

	public class SegueModel : BindableObject {

		public static readonly BindableProperty ActionProperty =
			BindableProperty.Create (nameof (Action), typeof (NavigationAction), typeof (SegueModel), default (NavigationAction));

		public static readonly BindableProperty TypeProperty =
			BindableProperty.Create (nameof (Type), typeof (string), typeof (SegueModel), "Default");

		public NavigationAction Action {
			get => (NavigationAction)GetValue (ActionProperty);
			set => SetValue (ActionProperty, value);
		}

		public string Type {
			get => (string)GetValue (TypeProperty);
			set => SetValue (TypeProperty, value);
		}

		public IEnumerable<string> TypeNames {
			get {
				yield return "Default";
				foreach (var nm in Segue.TypeNames)
					yield return nm;
			}
		}
	}
}
