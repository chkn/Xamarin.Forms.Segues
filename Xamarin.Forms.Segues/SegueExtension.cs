using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Segues {

	[ContentProperty (nameof (SegueExtension.Action))]
	public class SegueExtension : BindableObject, IMarkupExtension<ISegue> {

		public static readonly BindableProperty ActionProperty =
			BindableProperty.Create (nameof (Action), typeof (string), typeof (SegueExtension));

		public static readonly BindableProperty TypeProperty =
			BindableProperty.Create (nameof (Type), typeof (string), typeof (SegueExtension));

		/// <summary>
		/// Gets or sets the action that this segue will perform.
		/// </summary>
		/// <remarks>
		/// Values should correspond to values of the <see cref="SegueAction"/> enumeration. 
		/// </remarks>
		public string Action {
			get => (string)GetValue (ActionProperty);
			set => SetValue (ActionProperty, value);
		}

		/// <summary>
		/// Gets or sets the type of custom segue to be used, or <c>null</c> to use the default
		///  segue type for the given <see cref="Action"/>. 
		/// </summary>
		/// <value>The type name.</value>
		/// <remarks>Type names for custom segues must be registered by calling <see cref="Segue.RegisterType"/>.</remarks>
		public string Type {
			get => (string)GetValue (TypeProperty);
			set => SetValue (TypeProperty, value);
		}

		public ISegue ProvideValue (IServiceProvider serviceProvider)
		{
			var targetProvider = serviceProvider.GetService (typeof (IProvideValueTarget)) as IProvideValueTarget;
			if (targetProvider == null)
				throw new ArgumentException ();

			var target = targetProvider.TargetObject as VisualElement;
			if (target == null)
				XamlError ("Segue may only be used on a VisualElement", serviceProvider);

			var segue = Segue.Create (Type) ?? new Segue ();
			if (segue == null)
				XamlError ($"Unknown segue type \"{Type}\"", serviceProvider);

			if (Action != null) {
				SegueAction action;
				if (!SegueActionConverter.TryParse (Action, out action))
					XamlError ($"Unknown segue action \"{Action}\"", serviceProvider);
				segue.Action = action;
			}

			segue.SourceElement = target;
			return segue;
		}

		object IMarkupExtension.ProvideValue (IServiceProvider serviceProvider) => ProvideValue (serviceProvider);

		static void XamlError (string message, IServiceProvider serviceProvider)
		{
			var lineInfoProvider = serviceProvider.GetService (typeof (IXmlLineInfoProvider)) as IXmlLineInfoProvider;
			var lineInfo = lineInfoProvider?.XmlLineInfo ?? new XmlLineInfo ();
			throw new XamlParseException (message, lineInfo);
		}
	}
}
