using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Linq;

namespace Tachi.Semantic
{
	[HtmlTargetElement(Attributes = ForAttributeName + "," + WrapAttributeName)]
	public class WrapTagHelper : TagHelper
	{
		public const string ForAttributeName = "asp-for";
		public const string WrapAttributeName = "chi-wrap";
		public const string WrapClassAttributeName = "chi-wrap-class";
		public const string WrapFlagsAttributeName = "chi-wrap-flags";

		public WrapTagHelper(IHtmlGenerator generator)
		{
			if (generator == null)
				throw new ArgumentNullException(nameof(generator));

			Generator = generator;
		}

		protected IHtmlGenerator Generator { get; }

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		[HtmlAttributeName(ForAttributeName)]
		public ModelExpression For { get; set; }

		[HtmlAttributeName(WrapClassAttributeName)]
		public string Class { get; set; }

		[HtmlAttributeName(WrapFlagsAttributeName)]
		public WrapFlags Flags { get; set; } = WrapFlags.None;

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));
			if (output == null)
				throw new ArgumentNullException(nameof(output));

			// open wrapper
			var openWrap = new TagBuilder("div");
			openWrap.AddCssClass("field");
			if (!string.IsNullOrEmpty(Class))
				openWrap.AddCssClass(Class);
			if (For.ModelExplorer.Metadata.IsRequired)
				openWrap.AddCssClass("required");
			openWrap.TagRenderMode = TagRenderMode.StartTag;
			output.PreElement.AppendHtml(openWrap);

			// append label
			if (!Flags.HasFlag(WrapFlags.NoLabel))
			{
				var label = Generator.GenerateLabel(ViewContext, For.ModelExplorer, For.Name, null, null);
				output.PreElement.AppendHtml(label);
			}

			// append model state
			ModelStateEntry modelState;
			if (ViewContext.ModelState.TryGetValue(For.Name, out modelState) && modelState.Errors.Any())
			{
				// mark wrapper
				openWrap.AddCssClass("error");

				// generate message
				var error = modelState.Errors.First();
				var errorMessage = Generator.GenerateValidationMessage(ViewContext, For.ModelExplorer, For.Name, error.ErrorMessage, "span", new { @class = "ui basic red pointing label" });
				output.PostElement.AppendHtml(errorMessage);
			}

			// close wrapper
			var closeWrap = new TagBuilder("div");
			closeWrap.TagRenderMode = TagRenderMode.EndTag;
			output.PostElement.AppendHtml(closeWrap);
		}
	}
}