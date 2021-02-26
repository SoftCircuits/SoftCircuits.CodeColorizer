// Copyright (c) 2020-2021 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

namespace SoftCircuits.CodeColorizer
{
    /// <summary>
    /// Class to hold the quote and escape characters of a string constant.
    /// </summary>
    public class QuoteInfo
    {
        /// <summary>
        /// Character used as the quote character.
        /// </summary>
        public char Character { get; set; }

        /// <summary>
        /// Character used to escape the quote character. When parsing a quoted string,
        /// if a quote character is preceded immediately by this character, the quote
        /// character is considered part of the string and not the string terminator.
        /// Set to null for no escape character, in which case the string is terminated
        /// when the first matching quote is found.
        /// </summary>
        public char? Escape { get; set; }

        /// <summary>
        /// Constructs a new <see cref="QuoteInfo"/> instance.
        /// </summary>
        /// <param name="character">Character used as quote character.</param>
        /// <param name="escape">Optional character used to escape the quote
        /// character.</param>
        public QuoteInfo(char character, char? escape = null)
        {
            Character = character;
            Escape = escape;
        }
    }
}
