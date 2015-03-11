/*
 * Phosphorus.Five, copyright 2014 - 2015, Mother Earth, Jannah, Gaia - YOU!
 * phosphorus five is licensed as mit, see the enclosed LICENSE file for details
 */

using System.IO;
using phosphorus.core;
using phosphorus.expressions;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedMember.Global

namespace phosphorus.file.file
{
    /// <summary>
    ///     Class to help load files.
    /// </summary>
    public static class Load
    {
        /// <summary>
        ///     Loads zero or more files from disc. If file does not exist, false will be returned.
        /// </summary>
        /// <param name="context">Application context</param>
        /// <param name="e">Parameters passed into Active Event</param>
        [ActiveEvent (Name = "pf.file.load")]
        private static void pf_file_load (ApplicationContext context, ActiveEventArgs e)
        {
            // retrieving root folder of app
            var rootFolder = Common.GetRootFolder (context);

            // iterating through each file path given
            foreach (var idxFilename in XUtil.Iterate<string> (e.Args, context)) {
                // checking to see if file exists
                if (File.Exists (rootFolder + idxFilename)) {
                    // file exists, loading it as text file, and appending text into node
                    // with filename as name, and content as value
                    using (TextReader reader = File.OpenText (rootFolder + idxFilename)) {
                        e.Args.Add (new Node (idxFilename, reader.ReadToEnd ()));
                    }
                } else {
                    // file didn't exist, making sure we signal caller, by return a "false" node,
                    // where name of node is filename, and value is boolean false
                    e.Args.Add (new Node (idxFilename, false));
                }
            }
        }
    }
}