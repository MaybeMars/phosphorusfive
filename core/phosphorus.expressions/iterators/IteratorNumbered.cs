/*
 * Phosphorus.Five, copyright 2014 - 2015, Mother Earth, Jannah, Gaia - YOU!
 * phosphorus five is licensed as mit, see the enclosed LICENSE file for details
 */

using System.Collections.Generic;
using System.Linq;
using phosphorus.core;

namespace phosphorus.expressions.iterators
{
    /// <summary>
    ///     Iterator for returning the n'th children of previous iterator result.
    /// 
    ///     Will return the n'th children node of the nodes in the previous iterator's result.
    /// 
    ///     Example, will return the 3rd child node from previous result-set;
    ///     <pre>/2</pre>
    /// </summary>
    public class IteratorNumbered : Iterator
    {
        private readonly int _number;

        /// <summary>
        ///     Initializes a new instance of the <see cref="phosphorus.expressions.iterators.IteratorNumbered" /> class.
        /// </summary>
        /// <param name="number">The n'th child to return, if it exists, from previous result-set.</param>
        public IteratorNumbered (int number)
        {
            _number = number;
        }

        public override IEnumerable<Node> Evaluate
        {
            get { return from idxCurrent in Left.Evaluate where idxCurrent.Count > _number select idxCurrent [_number]; }
        }
    }
}