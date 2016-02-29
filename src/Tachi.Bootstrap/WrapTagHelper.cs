using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Linq;

namespace Tachi.Bootstrap
{
	[HtmlTargetElement(TagName)]
	public class HandlebarsTemplateTagHelper : TagHelper
	{
		public const string TagName = "handlebars-template";

		public HandlebarsTemplateTagHelper(IHtmlGenerator generator)
		{
			if (generator == null)
				throw new ArgumentNullException(nameof(generator));

			Generator = generator;
		}

		protected IHtmlGenerator Generator { get; }

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			output.TagName = "script";
			output.Attributes.SetAttribute("type", "text/x-handlebars-template");
		}
	}

	[HtmlTargetElement(Attributes = MetaAttributeName)]
	public class MetaTagHelper : TagHelper
	{
		public const string MetaAttributeName = "chi-meta";

		public MetaTagHelper(IHtmlGenerator generator)
		{
			if (generator == null)
				throw new ArgumentNullException(nameof(generator));

			Generator = generator;
		}

		protected IHtmlGenerator Generator { get; }

		[HtmlAttributeName(MetaAttributeName)]
		public ModelExpression Meta { get; set; }

		[HtmlAttributeNotBound]
		[ViewContext]
		public ViewContext ViewContext { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));
			if (output == null)
				throw new ArgumentNullException(nameof(output));

			if (Meta.ModelExplorer.Metadata.IsRequired)
				output.Attributes.SetAttribute(new TagHelperAttribute("chi-meta-required", "name", true));
		}
	}

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
			openWrap.AddCssClass("form-group");
			if (!string.IsNullOrEmpty(Class))
				openWrap.AddCssClass(Class);
			if (For.ModelExplorer.Metadata.IsRequired)
				openWrap.AddCssClass("required");
			openWrap.TagRenderMode = TagRenderMode.StartTag;
			output.PreElement.AppendHtml(openWrap);

			// append label
			if (!Flags.HasFlag(WrapFlags.NoLabel))
			{
				var label = Generator.GenerateLabel(ViewContext, For.ModelExplorer, For.Name, null, new { @class = "control-label" });
				output.PreElement.AppendHtml(label);
			}

			// update input
			TagHelperAttribute attribute;
			if (output.Attributes.TryGetAttribute("form-control", out attribute))
				output.Attributes.SetAttribute("class", attribute.Value + " form-control");
			else
				output.Attributes.SetAttribute("class", "form-control");

			// append model state
			ModelStateEntry modelState;
			if (ViewContext.ModelState.TryGetValue(For.Name, out modelState) && modelState.Errors.Any())
			{
				// mark wrapper
				openWrap.AddCssClass("has-error");

				// generate message
				var error = modelState.Errors.First();

				output.Attributes.SetAttribute("title", error.ErrorMessage);

				//var errorMessage = Generator.GenerateValidationMessage(ViewContext, For.Name, error.ErrorMessage, "span", new { @class = "ui basic red pointing label" });
				//output.PostElement.Append(errorMessage);
			}

			// close wrapper
			var closeWrap = new TagBuilder("div");
			closeWrap.TagRenderMode = TagRenderMode.EndTag;
			output.PostElement.AppendHtml(closeWrap);
		}
	}
}
