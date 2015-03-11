
/*
 * Phosphorus.Five, copyright 2014 - 2015, Mother Earth, Jannah, Gaia - YOU!
 * Phosphorus.Five is licensed under the terms of the MIT license, see the enclosed LICENSE file for details.
 */

using System.Web;
using phosphorus.core;
using phosphorus.expressions;
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global

namespace phosphorus.web.ui.response
{
    /// <summary>
    /// helper to manipulate the HTTP response
    /// </summary>
    public static class Headers
    {
        /// <summary>
        /// changes or removes existing HTTP headers, or adds new HTTP headers to response
        /// </summary>
        /// <param name="context"><see cref="phosphorus.core.ApplicationContext"/> for Active Event</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "pf.web.headers.set")]
        private static void pf_web_headers_set (ApplicationContext context, ActiveEventArgs e)
        {
            if (e.Args.Count == 0) {

                // "remove headers" invocation, looping through all headers user wish to remove
                foreach (var idx in XUtil.Iterate<string> (e.Args, context)) {
                    HttpContext.Current.Response.Headers.Remove (idx);
                }
            } else {

                // adding header(s) invocation
                var value = e.Args.LastChild.Get<string> (context);
                foreach (var idx in XUtil.Iterate<string> (e.Args, context)) {
                    HttpContext.Current.Response.AddHeader (idx, value);
                }
            }
        }
    }
}
