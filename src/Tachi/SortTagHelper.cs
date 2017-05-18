using System;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Tachi
{
	[HtmlTargetElement(Attributes = SortAttributeName)]
	public class SortTagHelper : TagHelper
	{
		public const string SortAttributeName = "chi-sort";
		public const string SortKeyAttributeName = "chi-sort-key";
		public const string SortFlagsAttributeName = "chi-sort-flags";

		public const char Up = '▲';
		public const char Down = '▼';

		public SortTagHelper(IHtmlGenerator generator, IUrlHelper urlHelper)
		{
			if (generator == null)
				throw new ArgumentNullException(nameof(generator));
			if (urlHelper == null)
				throw new ArgumentNullException(nameof(urlHelper));

			Generator = generator;
			UrlHelper = urlHelper;
		}

		protected IHtmlGenerator Generator { get; }

		protected IUrlHelper UrlHelper { get;}

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		[HtmlAttributeName(SortAttributeName)]
		public string Name { get; set; }

		[HtmlAttributeName(SortKeyAttributeName)]
		public string Key { get; set; } = "sort";

		[HtmlAttributeName(SortFlagsAttributeName)]
		public SortFlags Flags { get; set; } = SortFlags.None;

		private string BuildUrl(bool descending)
		{
			var request = ViewContext.HttpContext.Request;

			var name = Name;
			if (descending)
				name += Down;

			var query = new QueryBuilder();

			var handled = false;
			foreach (var pair in request.Query)
			{
				if (string.Equals(pair.Key, Key, StringComparison.OrdinalIgnoreCase))
				{
					query.Add(Key, name);
					handled = true;
				}
				else
					query.Add(pair.Key, pair.Value.AsEnumerable());
			}

			if (!handled)
				query.Add(Key, name);

			return request.PathBase.Add(request.Path) + query.ToString();
		}

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));
			if (output == null)
				throw new ArgumentNullException(nameof(output));

			var current = ViewContext.HttpContext.Request.Query[Key]
				.FirstOrDefault();

			var isDescending = false;
			if (current != null && current[current.Length - 1] == Down)
			{
				isDescending = true;
				current = current.Substring(0, current.Length - 1);
			}
			var isCurrent = string.Equals(current, Name, StringComparison.OrdinalIgnoreCase);

			// append caret
			if (!Flags.HasFlag(SortFlags.NoCaret) && isCurrent)
			{
				// <i class="icon caret down"></i>
				var icon = new TagBuilder("i");
				icon.AddCssClass("icon caret");
				if (isDescending)
					icon.AddCssClass("down");
				else
					icon.AddCssClass("up");
				output.PostContent.AppendHtml(icon);
			}

			output.Attributes.SetAttribute("href", BuildUrl(isCurrent && !isDescending));
		}
	}

	[Flags]
	public enum SortFlags
	{
		None = 0,
		NoCaret = 1
	}
}
