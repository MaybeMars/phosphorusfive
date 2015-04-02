/*
 * Phosphorus.Five, copyright 2014 - 2015, Mother Earth, Jannah, Gaia - YOU!
 * Phosphorus.Five is licensed under the terms of the MIT license, see the enclosed LICENSE file for details.
 */

using System;
using System.IO;
using System.Web;
using phosphorus.core;
using phosphorus.expressions;
using MimeKit;

namespace phosphorus.web.ui.response.echo
{
    /// <summary>
    ///     Content echo response.
    /// 
    ///     This echo response will simply concatenate all parameters in order, and transfer back to the client over the current HTTP response.
    /// </summary>
    public class EchoResponseContent : EchoResponse, IEchoResponse
    {
        public void Echo (ApplicationContext context, Node node, HttpResponse response)
        {
            // putting all parameters into body of response, as a single object
            foreach (var idxArg in GetParameters (node)) {
                if (idxArg.GetExChildValue ("is-file", context, false)) {

                    // item is a file
                    var fileName = idxArg.GetExValue<string> (context);
                    if (fileName != null) {
                        using (FileStream fileStream = File.OpenRead (EchoResponse.GetBasePath (context) + fileName)) {
                            fileStream.CopyTo (response.OutputStream);
                        }
                    }
                } else {

                    // serializing value of node
                    var byteValue =  idxArg.GetExValue<byte[]> (context, null);
                    if (byteValue != null)
                        response.OutputStream.Write (byteValue, 0, byteValue.Length);
                }
            }
        }
    }
}
