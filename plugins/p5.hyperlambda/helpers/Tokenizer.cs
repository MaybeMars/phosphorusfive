/*
 * Phosphorus Five, copyright 2014 - 2017, Thomas Hansen, thomas@gaiasoul.com
 * 
 * This file is part of Phosphorus Five.
 *
 * Phosphorus Five is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 3, as published by
 * the Free Software Foundation.
 *
 *
 * Phosphorus Five is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Phosphorus Five.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * If you cannot for some reasons use the GPL license, Phosphorus
 * Five is also commercially available under Quid Pro Quo terms. Check 
 * out our website at http://gaiasoul.com for more details.
 */

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using p5.core;

namespace p5.hyperlambda.helpers
{
    /*
     * Class used internally to tokenize Hyperlambda.
     */
    public sealed class Tokenizer : IDisposable
    {
        readonly StringReader _reader;
        bool _disposed;

        /*
         * Ctor taking Hyperlambda as input. Use the "Tokens" property after creation to parse and access p5 lambda 
         * created from Hyperlambda
         */
        public Tokenizer (string hyperlambda) { _reader = new StringReader (hyperlambda); }

        /*
         * Returns p5 lambda tokens created from Hyperlambda passed in through CTOR
         */
        internal IEnumerable<Token> Tokens
        {
            get
            {
                var previousToken = new Token (Token.TokenType.CarriageReturn, "\r\n"); // We start out with a CR/LF token
                while (true) {

                    // Retrieving next token
                    var token = NextToken (previousToken);

                    // Checking if we're done (EOF)
                    if (token == null)
                        yield break;

                    // Storing previous token as we proceed
                    previousToken = token;

                    // Returning currently iterated token
                    yield return token;
                }
            }
        }

        /*
         * Private implementation of IDisposable interface
         */
        void IDisposable.Dispose ()
        {
            Dispose (true);
        }

        /*
         * Actual implementation of Dispose method
         */
        void Dispose (bool disposing)
        {
            if (!_disposed && disposing) {
                _disposed = true;
                _reader.Dispose ();
            }
        }

        /*
         * Retrieves next hyperlambda token from text reader
         */
        Token NextToken (Token previousToken)
        {
            var nextChar = _reader.Peek ();
            if ((nextChar == ':') &&
                (previousToken == null || previousToken.Type == Token.TokenType.Spacer || previousToken.Type == Token.TokenType.CarriageReturn))
                return new Token (Token.TokenType.Name, ""); // empty name
            if ((nextChar == '\r' || nextChar == '\n' || nextChar == -1) && previousToken.Type == Token.TokenType.Separator)
                return new Token (Token.TokenType.TypeOrContent, ""); // empty name
            if (nextChar == -1)
                return null; // end of stream

            nextChar = _reader.Read ();
            if (nextChar == '/' && (_reader.Peek () == '/' || _reader.Peek () == '*') &&
                (previousToken == null || previousToken.Type == Token.TokenType.CarriageReturn || previousToken.Type == Token.TokenType.Spacer))
                return SkipCommentToken ();
            if (nextChar == ':')
                return new Token (Token.TokenType.Separator, ":");
            if (nextChar == ' ') {
                if (previousToken.Type == Token.TokenType.CarriageReturn) {
                    return NextSpaceToken ();
                }
                // Whitespace only carry semantics as first token in each line in Hyperlambda content
                // therefor we must "left-trim" the reader, before retrieving next token
                TrimReader ();
                return NextToken (previousToken);
            }
            if (nextChar == '\r') {
                return NextCrlfToken ();
            }
            if (nextChar == '\n') {
                return new Token (Token.TokenType.CarriageReturn, "\r\n"); // normalizing carriage returns
            }
            return NextDefaultToken (nextChar, previousToken);
        }

        /*
         * Skips the comment token starting at current position of reader
         */
        Token SkipCommentToken ()
        {
            var nextChar = _reader.Read ();
            if (nextChar == '/') {

                // Single line comment
                _reader.ReadLine ();
            } else if (nextChar == '*') {

                // Multiple lines comment
                while (true) {
                    nextChar = _reader.Read ();
                    if (nextChar == -1)
                        throw new ArgumentException ("Unclosed multiple lines comment in Hyperlambda");
                    if (nextChar == '*') {
                        nextChar = _reader.Read ();
                        if (nextChar == '/')
                            break;
                    }
                }
            } else {

                // Syntax Error
                throw new ArgumentException ("Syntax error in comment of Hyperlambda, unknown comment type");
            }
            return new Token (Token.TokenType.CarriageReturn, "\r\n");
        }

        /*
         * Trims reader until reader head is at first non-space character
         */
        void TrimReader ()
        {
            var nextChar = _reader.Peek ();
            while (nextChar == ' ') {
                _reader.Read ();
                nextChar = _reader.Peek ();
            }
        }

        /*
         * Reads and validates next space token ("  ") from text reader
         */
        Token NextSpaceToken ()
        {
            var buffer = " ";
            var nextChar = _reader.Peek ();
            while (nextChar == ' ') {
                buffer += (char) _reader.Read ();
                nextChar = _reader.Peek ();
            }
            return new Token (Token.TokenType.Spacer, buffer);
        }

        /*
         * Reads and validates next carriage return / line feed token ("\r\n" or "\n")
         */
        Token NextCrlfToken ()
        {
            var nextChar = _reader.Read ();
            if (nextChar == -1)
                throw new ArgumentException ("Syntax error in hyperlambda, carriage return character found, but no new line character found at end of file");
            if (nextChar != '\n')
                throw new ArgumentException ("Syntax error in hyperlambda, carriage return character found, but no new line character found");
            return new Token (Token.TokenType.CarriageReturn, "\r\n");
        }

        /*
         * Reads next "default token" from text reader, can be string, multiline string, or simply legal unescaped characters
         */
        Token NextDefaultToken (int nextChar, Token previousToken)
        {
            if (nextChar == '"') {
                return new Token (GetTokenType (previousToken), Utilities.ReadSingleLineStringLiteral (_reader)); // single line string literal
            }
            if (nextChar == '@') {
                if ((char) _reader.Peek () == '"') {
                    _reader.Read (); // multiline string literal, skipping '"' part
                    return new Token (GetTokenType (previousToken), Utilities.ReadMultiLineStringLiteral (_reader));
                }
            }

            // Default token type, no string quoting here
            var builder = new StringBuilder ();
            builder.Append ((char) nextChar);
            nextChar = _reader.Peek ();
            while (nextChar != -1 && "\r\n:".IndexOf ((char) nextChar) == -1) {
                builder.Append ((char) _reader.Read ());
                nextChar = _reader.Peek ();
            }

            // Whitespace has no semantics, and are not part of tokens, except if within string literals, or before name token type
            return new Token (GetTokenType (previousToken), builder.ToString ().Trim ());
        }

        /*
         * Returns the curent token's type according to the previous token type
         */
        Token.TokenType GetTokenType (Token previousToken)
        {
            if (previousToken != null && previousToken.Type == Token.TokenType.Separator)
                return Token.TokenType.TypeOrContent;
            return Token.TokenType.Name;
        }
    }
}
