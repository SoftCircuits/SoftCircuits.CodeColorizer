// Copyright (c) 2020-2021 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using System.Collections.Generic;

namespace SoftCircuits.CodeColorizer
{
    /// <summary>
    /// Defines the rules for a single language.
    /// </summary>
    public class LanguageRules
    {
        public const bool DefaultCaseSensitive = true;
        public const string DefaultSymbolFirstChars = "_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string DefaultSymbolChars = "_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        public const string DefaultOperatorChars = "+-*/%&|^~<>=!";

        /// <summary>
        /// Gets or sets the name of this language.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets whether this language is case-sensitive.
        /// </summary>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// Gets or sets a string of characters that make up this language's keywords
        /// and symbol names.
        /// </summary>
        public string? SymbolChars { get; set; }

        /// <summary>
        /// Gets or sets a string of characters that can appear as the first character
        /// in this language's keywords and symbol names.
        /// </summary>
        public string? SymbolFirstChars { get; set; }

        /// <summary>
        /// Gets or sets a string of characters that make up this language's operators.
        /// Must include characters used to signify block and/or line comments.
        /// </summary>
        public string? OperatorChars { get; set; }

        /// <summary>
        /// Gets or sets a list of quote specifiers.
        /// </summary>
        public List<QuoteInfo>? Quotes { get; set; }

        /// <summary>
        /// Gets or sets a list of block comment specifiers.
        /// </summary>
        public List<BlockCommentInfo>? BlockComments { get; set; }

        /// <summary>
        /// Gets or sets a list of line comment specifiers.
        /// </summary>
        public List<string>? LineComments { get; set; }

        /// <summary>
        /// Gets or sets a list of keywords supported by this language.
        /// </summary>
        public List<string>? Keywords { get; set; }

        /// <summary>
        /// Gets or sets a list of symbol names supported by this language. Because a complete
        /// symbol list can be impractical, this list is optional. As an alternatively to
        /// setting all the symbol names, you can set the
        /// <see cref="CodeColorizer.UnclassifiedDefaultToSymbols"/> property to <c>true</c>.
        /// </summary>
        public List<string>? Symbols { get; set; }
    }
}
