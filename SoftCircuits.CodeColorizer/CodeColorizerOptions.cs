// Copyright (c) 2020-2021 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

namespace SoftCircuits.CodeColorizer
{
    public class CodeColorizerOptions
    {
        /// <summary>
        /// Gets or sets the format string used to colorize a tokens. The {0} placeholder
        /// will be replaced with the token, and the {1} placeholder will be replaced
        /// with the class name. This property should be set only when non-standard
        /// behavior is needed.
        /// </summary>
        public string TokenFormat { get; set; }

        /// <summary>
        /// Gets or sets whether all symbol tokens (consisting entirely of <see cref="SymbolFirstChars"/>
        /// and <see cref="SymbolChars"/> characters) that are not keywords should be classified as
        /// symbols. By default, only symbol tokens included in <see cref="Symbols"/> are classified as
        /// symbols.
        /// </summary>
        public bool UnclassifiedDefaultToSymbols { get; set; }

        /// <summary>
        /// Constructs a new <see cref="CodeColorizerOptions"/> instance.
        /// </summary>
        public CodeColorizerOptions()
        {
            TokenFormat = string.Empty;
        }
    }
}
