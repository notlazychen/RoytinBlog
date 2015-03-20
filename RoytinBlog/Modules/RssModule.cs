using RoytinBlog.Core;
using RoytinBlog.Core.Cache;
using RoytinBlog.Core.ViewProjections.Home;
using RoytinBlog.Features;
using RoytinBlog.Responses;
using System;

namespace RoytinBlog.Modules
{
    public class RssModule : BaseNancyModule
    {
        private readonly IViewProjectionFactory _viewProjectionFactory;

        private readonly ICache _cache;

        public RssModule(IViewProjectionFactory viewProjectionFactory, ICache cache)
        {
            _viewProjectionFactory = viewProjectionFactory;
            _cache = cache;

            Get["/rss"] = _ => GetRecentPostsRss();
        }

        private dynamic GetRecentPostsRss()
        {
            var cacheKey = "rss";
            var rss = _cache.Get<RssResponse>(cacheKey);
            if (rss == null)
            {
                var recentPosts = _viewProjectionFactory.Get<RecentBlogPostsBindingModel, RecentBlogPostsViewModel>(new RecentBlogPostsBindingModel()
                                                                                                      {
                                                                                                          Page = 1,
                                                                                                          Take = 30
                                                                                                      });

                rss = new RssResponse(recentPosts.Posts, Settings.WebsiteName, new Uri(AppConfiguration.Current.SiteUrl));
                _cache.Add(cacheKey, rss, 60 * 5);
            }
            return rss;
        }
    }
}