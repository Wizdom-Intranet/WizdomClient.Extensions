using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Wizdom.Client.Extensions
{
    public class Noticeboard
    {
        private WizdomClient _wizdomClient;
        public Noticeboard(WizdomClient wizdomClient)
        {
            _wizdomClient = wizdomClient;
        }

        public async System.Threading.Tasks.Task<Items> GetItemsAsync(
            string filters = null,
            int skip = 0,
            int take = 25,
            string searchTerm = null,
            int maxCommentsToGet = 0,
            int maxLikesToGet = 0,
            int maxTotalCount = 100,
            string preferredLanguage = "",
            string select = null,
            string expand = null)
        {
            return await _wizdomClient.GetObjectAsync<Items>($"/api/wizdom/noticeboard/v3/items?filters={HttpUtility.UrlEncode(filters ?? "")}&skip={skip}&take={take}&searchTerm={HttpUtility.UrlEncode(searchTerm ?? "") }&maxCommentsToGet={maxCommentsToGet}&maxLikesToGet={maxLikesToGet}&maxTotalCount={maxTotalCount}&preferredLanguage={HttpUtility.UrlEncode(preferredLanguage)}{(select != null ? "&select=" + HttpUtility.UrlEncode(select) : "")}{(select != null ? "&expand=" + HttpUtility.UrlEncode(expand) : "")}");
        }
    }

    public class Items
    {
        public Item[] data { get; set; }
        public int totalCount { get; set; }
    }

    public class Item
    {
        public int id { get; set; }
        public string heading { get; set; }
        public string summary { get; set; }
        public string content { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public DateTime created { get; set; }
        public DateTime modified { get; set; }
        public User author { get; set; }
        public User alternateAuthor { get; set; }
        public Contenttype contentType { get; set; }
        public Extendedproperties extendedProperties { get; set; }
        public Channel[] channels { get; set; }
        public object mentions { get; set; }
        public Likes likes { get; set; }
        public Comments comments { get; set; }
        public bool readByCurrentUser { get; set; }
        public bool editAllowedByCurrentUser { get; set; }
        public bool likedByCurrentUser { get; set; }
        public Translation[] translations { get; set; }
        public object currentLanguage { get; set; }
        public string[] availableLanguages { get; set; }
    }

    public class User
    {
        public string loginName { get; set; }
        public int id { get; set; }
        public string displayName { get; set; }
    }

    public class Contenttype
    {
        public string name { get; set; }
        public int id { get; set; }
        public Metadata metaData { get; set; }
    }

    public class Metadata
    {
        public int plusDays { get; set; }
        public bool isVisible { get; set; }
        public bool useRollupImage { get; set; }
        public bool useAlternativeAuthor { get; set; }
        public string rollupImage { get; set; }
    }

    public class Extendedproperties
    {
        public string ImageUrl { get; set; }
        public object AuthorEmail { get; set; }
        public string AuthorName { get; set; }
        public string ItemUrl { get; set; }
        public string picture { get; set; }
    }

    public class Likes
    {
        public Like[] data { get; set; }
        public int totalCount { get; set; }
    }

    public class Like
    {
        public int Id { get; set; }
        public User Author { get; set; }
    }

    public class Comments
    {
        public Comment[] data { get; set; }
        public int totalCount { get; set; }
    }

    public class Comment
    {
        public bool LikedByCurrentUser { get; set; }

        public int Id { get; set; }
        public string Content { get; set; }
        public User Author { get; set; }
        public Comments Replies { get; set; }
        public DateTimeOffset Posted { get; set; }
        public DateTimeOffset? Modified { get; set; }
        public string Picture { get; set; }
        public string Status { get; set; }
        public Likes Likes { get; set; }
        public bool CurrentUserCanEdit { get; set; }
        public bool CurrentUserCanDelete { get; set; }
    }

    public class Channel
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool editorsCanOnlyEditOwn { get; set; }
    }

    public class Translation
    {
        public string languageCode { get; set; }
        public Translatedvalues translatedValues { get; set; }
    }

    public class Translatedvalues
    {
        public string heading { get; set; }
        public string content { get; set; }
        public string summary { get; set; }
    }























}
