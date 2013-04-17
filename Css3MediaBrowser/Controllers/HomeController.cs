using Css3MediaBrowser.Models;
using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Css3MediaBrowser.Controllers
{
    public class HomeController : Controller
    {
        private IOAuthCredentials credentials = new SessionStateCredentials();
        private MvcAuthorizer auth;
        private TwitterContext twitterCtx;

        //public ActionResult Index()
        //{
        //    return GetTweets("sumitkm");
        //}

        public ActionResult Index(string screenName = "sumitkm")
        {
            if (!string.IsNullOrEmpty(screenName))
            {
                return GetTweets(screenName);
            }
            else
            {
                return GetTweets("sumitkm");
            }
        }

        private ActionResult GetTweets(string screenName)
        {
            if (credentials.ConsumerKey == null || credentials.ConsumerSecret == null)
            {
                credentials.ConsumerKey = ConfigurationManager.AppSettings["twitterConsumerKey"];
                credentials.ConsumerSecret = ConfigurationManager.AppSettings["twitterConsumerSecret"];
            }

            auth = new MvcAuthorizer
            {
                Credentials = credentials
            };

            auth.CompleteAuthorization(Request.Url);

            if (!auth.IsAuthorized)
            {
                Uri specialUri = new Uri(Request.Url.ToString());
                return auth.BeginAuthorization(specialUri);
            }
            IEnumerable<TweetViewModel> friendTweets = new List<TweetViewModel>();
            if (string.IsNullOrEmpty(screenName))
            {
                return View(friendTweets);
            }
            twitterCtx = new TwitterContext(auth);
            friendTweets =
                (from tweet in twitterCtx.Status
                 where tweet.Type == StatusType.User &&
                       tweet.ScreenName == screenName &&
                       tweet.IncludeEntities == true
                 select new TweetViewModel
                 {
                     ImageUrl = tweet.User.ProfileImageUrl,
                     ScreenName = tweet.User.Identifier.ScreenName,
                     MediaUrl = GetTweetMediaUrl(tweet),
                     Tweet = tweet.Text
                 })
                .ToList();
            return View(friendTweets);
        }

        private string GetTweetMediaUrl(Status status)
        {
            if (status.Entities != null && status.Entities.MediaMentions.Count > 0)
            {
                return status.Entities.MediaMentions[0].MediaUrlHttps;
            }
            return "";
        }
    }
}
