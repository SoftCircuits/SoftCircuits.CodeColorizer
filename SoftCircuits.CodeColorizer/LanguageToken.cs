// Copyright (c) 2020-2021 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

namespace SoftCircuits.CodeColorizer
{
    /// <summary>
    /// Represents a single token parsed by <see cref="LanguageParser"/>.
    /// </summary>
    internal class LanguageToken
    {
        /// <summary>
        /// The type of token.
        /// </summary>
        public TokenType Type { get; set; }

        /// <summary>
        /// The token value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Constructs a new <see cref="LanguageToken"/> instance.
        /// </summary>
        public LanguageToken()
        {
            Type = TokenType.EndOfText;
            Value = string.Empty;
        }

        /// <summary>
        /// Constructs a new <see cref="LanguageToken"/> instance.
        /// </summary>
        /// <param name="tokenType">The token type.</param>
        /// <param name="value">The token value.</param>
        public LanguageToken(TokenType tokenType, string value = "")
        {
            Type = tokenType;
            Value = value;
        }
    }
}
