using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Xamarin.Forms.Segues {

	static class Extensions {

		static readonly BindableProperty PreviousPageProperty
			= BindableProperty.CreateAttached ("PreviousPage", typeof (Page), typeof (Extensions), null);

		public static Page GetPreviousPage (this Page page)
			=> (Page)page.GetValue (PreviousPageProperty);

		public static void SetPreviousPage (this Page page, Page previous)
			=> page.SetValue (PreviousPageProperty, previous);

		public static bool IsTop (this IReadOnlyList<Page> stack, Page page)
		{
			var cnt = stack.Count;
			return cnt > 0 && stack [cnt - 1] == page;
		}

		public static T GetNextIfTop<T> (this IReadOnlyList<T> stack, T page)
			where T : class
		{
			var cnt = stack.Count;
			return (cnt > 1 && stack [cnt - 1] == page)? stack [cnt - 2] : null;
		}

		public static Page GetPage (this Element elem)
		{
			while (elem != null && !(elem is Page))
				elem = elem.Parent;
			return (Page)elem;
		}
	}
}
