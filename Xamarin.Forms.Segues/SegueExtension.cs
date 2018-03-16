using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Segues {

	[ContentProperty (nameof (SegueExtension.Action))]
	public class SegueExtension : IMarkupExtension<Segue> {

		/// <summary>
		/// Gets or sets the action that this segue will perform.
		/// </summary>
		/// <remarks>
		/// Values should correspond to values of the <see cref="SegueAction"/> enumeration. 
		/// </remarks>
		public string Action { get; set; }

		/// <summary>
		/// Gets or sets the type of custom segue to be created.
		/// </summary>
		/// <value>The type name, as passed to <see cref="Segue.RegisterType"/>, or <c>null</c> to use the default type, <see cref="Segue"/>.</value>
		/// <remarks>Type names for custom segues must be registered by calling <see cref="Segue.RegisterType"/>.</remarks>
		public string Type { get; set; }

		public Segue ProvideValue (IServiceProvider serviceProvider)
		{
			var targetProvider = serviceProvider.GetService (typeof (IProvideValueTarget)) as IProvideValueTarget;
			if (targetProvider == null)
				throw new ArgumentException ();

			var target = targetProvider.TargetObject as VisualElement;
			if (target == null)
				XamlError ("Segue may only be used on a VisualElement", serviceProvider);

			var segue = (Type == null)? new Segue () : Segue.Create (Type);
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
