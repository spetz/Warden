using System.Collections.Generic;
using Microsoft.AspNet.Html.Abstractions;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Warden.Web.Core.Domain;
using Warden.Web.Core.Extensions;

namespace Warden.Web.Extensions
{
    public static class UrlHelperExtensions
    {
        public static IHtmlContent Paginate(this IUrlHelper urlHelper, PagedResultBase viewModel,
            string action) => urlHelper.Paginate(viewModel, action, string.Empty);

        public static IHtmlContent Paginate(this IUrlHelper urlHelper, PagedResultBase viewModel,
            string action, string controller)
        {
            var listTag = new TagBuilder("ul");
            listTag.AddCssClass("pagination");
            var previousPageTag = GetPreviousPageTag(urlHelper, viewModel, action, controller);
            listTag.InnerHtml.Append(previousPageTag);
            var pageTags = GetPageTags(urlHelper, viewModel, action, controller);
            foreach (var pageTag in pageTags)
            {
                listTag.InnerHtml.Append(pageTag);
            }
            var nextPageTag = GetNextPageTag(urlHelper, viewModel, action, controller);
            listTag.InnerHtml.Append(nextPageTag);

            return listTag;
        }

        private static TagBuilder GetPreviousPageTag(this IUrlHelper urlHelper, PagedResultBase viewModel,
            string action, string controller)
        {
            var listItemTag = new TagBuilder("li");
            var linkTag = new TagBuilder("a");
            var iconTag = new TagBuilder("i");
            iconTag.AddCssClass("material-icons");
            iconTag.InnerHtml.Append("chevron_left");
            if (viewModel.CurrentPage > 1)
            {
                linkTag.MergeAttribute("href", GetUrl(urlHelper, action, controller,
                    viewModel.CurrentPage - 1, viewModel.ResultsPerPage));
            }
            else
            {
                listItemTag.AddCssClass("disabled");
            }

            linkTag.InnerHtml.Append(iconTag);
            listItemTag.InnerHtml.Append(linkTag);

            return listItemTag;
        }

        private static IEnumerable<TagBuilder> GetPageTags(this IUrlHelper urlHelper, PagedResultBase viewModel,
            string action, string controller)
        {
            var totalPages = viewModel.TotalPages <= 0 ? 1 : viewModel.TotalPages;
            var currentPage = viewModel.CurrentPage <= 0 ? 1 : viewModel.CurrentPage;
            for (var i = 1; i <= totalPages; i++)
            {
                var isCurrentPage = currentPage == i;
                var listItemTag = new TagBuilder("li");
                var linkTag = new TagBuilder("a");
                linkTag.MergeAttribute("href", GetUrl(urlHelper, action, controller, i, viewModel.ResultsPerPage));
                linkTag.AddCssClass("item");
                if (isCurrentPage)
                    linkTag.AddCssClass("white-text");

                var pageClass = isCurrentPage ? "custom" : "waves-effect";
                listItemTag.AddCssClass(pageClass);

                linkTag.InnerHtml.Append(i.ToString());
                listItemTag.InnerHtml.Append(linkTag);

                yield return listItemTag;
            }
        }

        private static TagBuilder GetNextPageTag(this IUrlHelper urlHelper, PagedResultBase viewModel,
            string action, string controller)
        {
            var totalPages = viewModel.TotalPages <= 0 ? 1 : viewModel.TotalPages;
            var currentPage = viewModel.CurrentPage <= 0 ? 1 : viewModel.CurrentPage;
            var listItemTag = new TagBuilder("li");
            var linkTag = new TagBuilder("a");
            var iconTag = new TagBuilder("i");
            iconTag.AddCssClass("material-icons");
            iconTag.InnerHtml.Append("chevron_right");
            if (currentPage < totalPages)
            {
                linkTag.MergeAttribute("href", GetUrl(urlHelper, action, controller,
                    currentPage + 1, viewModel.ResultsPerPage));
            }
            else
            {
                listItemTag.AddCssClass("disabled");
            }

            linkTag.InnerHtml.Append(iconTag);
            listItemTag.InnerHtml.Append(linkTag);

            return listItemTag;
        }

        private static string GetUrl(IUrlHelper urlHelper, string action, string controller,
            int page, int results) => controller.Empty()
                ? urlHelper.Action(action, new {page, results})
                : urlHelper.Action(action, controller, new {page, results});
    }
}