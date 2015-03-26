/*
 * Phosphorus.Five, copyright 2014 - 2015, Mother Earth, Jannah, Gaia - YOU!
 * phosphorus five is licensed as mit, see the enclosed LICENSE file for details
 */

using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using phosphorus.core;
using phosphorus.expressions;
using MimeKit;

namespace phosphorus.net.requests
{
    public abstract class QueryStringRequest : HttpRequest
    {
        public QueryStringRequest (ApplicationContext context, Node node, string url, string method)
            : base (context, node, url, method)
        { }

        protected override string GetURL (ApplicationContext context, Node node, string url)
        {
            StringBuilder builder = new StringBuilder (url);
            bool first = url.IndexOf ("?") == -1;

            // looping through everything that's neither [cookies], nor [headers] nor [method], and has a value, and using name of
            // node as name of parameter and value of node as value, constructing a URL-Encoded URL, returning to caller
            foreach (var idxArg in node.FindAll (ix => ix.Name != "headers" && ix.Name != "cookies" && ix.Name != "method")) {

                // making sure our first argument starts with a "?", and all other arguments have "&" prepended in front of them
                if (first) {
                    first = false;
                    builder.Append ("?");
                } else {
                    builder.Append ("&");
                }

                builder.Append (HttpUtility.UrlEncode (idxArg.Name));
                builder.Append ("=");
                builder.Append (HttpUtility.UrlEncode (GetContent (context, idxArg)));
            }

            // returning Url to caller
            return builder.ToString ();
        }

        /*
         * retrieves content of parameter, which if this is a file attachment, will be the content of that file, otherwise
         * it will be the value of the parameter node given
         */
        private byte[] GetContent (ApplicationContext context, Node node)
        {
            // converting content to byte array if necessary, and returning to caller
            var content = XUtil.Single<object> (node.Value, node, context, null);
            if (content == null)
                return new byte [] { }; // no value

            var byteContent = content as byte [];
            if (byteContent != null)
                return byteContent; // content is already byte array

            // converting value to string if necessary, before retrieving byte [], and returning to caller
            return Encoding.UTF8.GetBytes (Utilities.Convert<string> (content, context));
        }
    }
}