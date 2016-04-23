//using Microsoft.AspNet.Mvc;
//using Microsoft.AspNet.Mvc.Rendering;
//using Microsoft.AspNet.Routing;
//using Warden.Web.Core.Domain;

//namespace Warden.Web.Extensions
//{
//    public static class UrlHelperExtensions
//    {
//        public static HtmlString Paginate(this IUrlHelper urlHelper, PagedResultBase viewModel,
//            string routeName, PagedQueryBase query = null)
//        {
//            var listTag = new TagBuilder("ul");
//            listTag.AddCssClass("pagination");
//            var results = viewModel.ResultsPerPage;

//            var previousPageItem = new TagBuilder("li");
//            var previousPageTag = new TagBuilder("a");
//            var previousPageIcon = new TagBuilder("i");
//            previousPageIcon.AddCssClass("material-icons");
//            previousPageIcon.SetInnerText("chevron_left");
//            if (viewModel.CurrentPage > 1)
//            {
//                previousPageTag.MergeAttribute("href", QueryUrl(urlHelper, routeName,
//                    viewModel.CurrentPage - 1, results, query));
//            }
//            else
//            {
//                previousPageItem.AddCssClass("disabled");
//            }

//            previousPageTag.InnerHtml += previousPageIcon.ToString();
//            previousPageItem.InnerHtml += previousPageTag.ToString();
//            listTag.InnerHtml += previousPageItem.ToString();
//            var totalPages = viewModel.TotalPages <= 0 ? 1 : viewModel.TotalPages;
//            var currentPage = viewModel.CurrentPage <= 0 ? 1 : viewModel.CurrentPage;

//            for (var i = 1; i <= totalPages; i++)
//            {
//                var pageItem = new TagBuilder("li");
//                var pageTag = new TagBuilder("a");
//                pageTag.MergeAttribute("href", QueryUrl(urlHelper, routeName, i, results, query));
//                pageTag.AddCssClass("item");
//                var pageClass = currentPage == i ? "active" : "waves-effect";
//                pageItem.AddCssClass(pageClass);

//                pageTag.SetInnerText(i.ToString());
//                pageItem.InnerHtml += pageTag.ToString();
//                listTag.InnerHtml += pageItem.ToString();
//            }


//            var nextPageItem = new TagBuilder("li");
//            var nextPageTag = new TagBuilder("a");
//            var nextPageIcon = new TagBuilder("i");
//            nextPageIcon.AddCssClass("material-icons");
//            nextPageIcon.SetInnerText("chevron_right");
//            if (currentPage < totalPages)
//            {
//                nextPageTag.MergeAttribute("href", QueryUrl(urlHelper, routeName,
//                    currentPage + 1, results, query));
//            }
//            else
//            {
//                nextPageItem.AddCssClass("disabled");
//            }

//            nextPageTag.InnerHtml += nextPageIcon.ToString();
//            nextPageItem.InnerHtml += nextPageTag.ToString();
//            listTag.InnerHtml += nextPageItem.ToString();

//            return new HtmlString(listTag.ToString());
//        }

//        private static string QueryUrl(IUrlHelper urlHelper, string routeName,
//            int page, int results, PagedQueryBase query = null)
//        {
//            if (query == null)
//                return urlHelper.RouteUrl(routeName, new { page, results });

//            var args = new RouteValueDictionary
//            {
//                ["page"] = page,
//                ["results"] = results
//            };

//            return urlHelper.RouteUrl(routeName, args);
//        }
//    }
//}