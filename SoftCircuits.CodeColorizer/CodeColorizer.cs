// Copyright (c) 2020-2021 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using SoftCircuits.Parsing.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;

namespace SoftCircuits.CodeColorizer
{
    /// <summary>
    /// Class to colorize source code by inserting HTML markup around language tokens.
    /// </summary>
    public class CodeColorizer
    {
        private readonly ParsingHelper Helper;

        #region Private language settings

        /// <summary>
        /// Characters legal in keyword and symbol names.
        /// </summary>
        private HashSet<char>? SymbolChars;

        /// <summary>
        /// Characters legal as first character in keyword and symbol names.
        /// </summary>
        private HashSet<char>? SymbolFirstChars;

        /// <summary>
        /// Characters that make up this language's operators. Must include characters
        /// used to define comments.
        /// </summary>
        private HashSet<char>? OperatorChars;

        /// <summary>
        /// List of keywords for the current language.
        /// </summary>
        private HashSet<string>? Keywords;

        /// <summary>
        /// List of symbols for the current language.
        /// </summary>
        private HashSet<string>? Symbols;

        /// <summary>
        /// List of block-comment specifiers for this language.
        /// </summary>
        private List<BlockCommentInfo>? BlockComments;

        /// <summary>
        /// List of line-comment specifiers for this language.
        /// </summary>
        private List<string>? LineComments;

        /// <summary>
        /// List of quote specifiers for this language.
        /// </summary>
        private List<QuoteInfo>? Quotes;

        internal CharComparer? CharComparer { get; private set; }
        internal StringComparer? StringComparer { get; private set; }
        internal StringComparison StringComparison { get; private set; }

        #endregion

        /// <summary>
        /// Gets the colorizer options.
        /// </summary>
        public CodeColorizerOptions Options { get; private set; }

        /// <summary>
        /// Gets or sets the class name applied to keywords.
        /// </summary>
        public string? KeywordCssClass { get; set; }

        /// <summary>
        /// Gets or sets the class name applied to symbols.
        /// </summary>
        public string? SymbolCssClass { get; set; }

        /// <summary>
        /// Gets or sets the class name applied to string literals.
        /// </summary>
        public string? StringCssClass { get; set; }

        /// <summary>
        /// Gets or sets the class name applied to operators.
        /// </summary>
        public string? OperatorCssClass { get; set; }

        /// <summary>
        /// Gets or sets the class name applied to comments.
        /// </summary>
        public string? CommentCssClass { get; set; }

        /// <summary>
        /// Initializes a new <see cref="CodeColorizer"/> instance.
        /// </summary>
        /// <param name="rules">The language rules to use. Note that the rules
        /// are copied and so any changes to the rules after calling this
        /// constructor will not be observed.</param>
        public CodeColorizer(LanguageRules rules)
        {
            Helper = new ParsingHelper(null);
            Options = new CodeColorizerOptions
            {
                TokenFormat = "<span class=\"{1}\">{0}</span>",
                UnclassifiedDefaultToSymbols = false,
            };
            SetLanguage(rules);
        }

        /// <summary>
        /// Changes the current language rules.
        /// </summary>
        /// <param name="rules">The language rules to use. Note that the rules
        /// are copied and so any changes to the rules after calling this
        /// constructor will not be observed.</param>
        public void SetLanguage(LanguageRules rules)
        {
            if (rules == null)
                throw new ArgumentNullException(nameof(rules));

            if (rules.CaseSensitive)
            {
                CharComparer = CharComparer.Ordinal;
                StringComparer = StringComparer.Ordinal;
                StringComparison = StringComparison.Ordinal;
            }
            else
            {
                CharComparer = CharComparer.OrdinalIgnoreCase;
                StringComparer = StringComparer.OrdinalIgnoreCase;
                StringComparison = StringComparison.OrdinalIgnoreCase;
            }

            SymbolChars = new HashSet<char>(rules.SymbolChars ?? string.Empty, CharComparer);
            SymbolFirstChars = new HashSet<char>(rules.SymbolFirstChars ?? string.Empty, CharComparer);
            OperatorChars = new HashSet<char>(rules.OperatorChars ?? string.Empty, CharComparer);
            Keywords = new HashSet<string>(rules.Keywords ?? Enumerable.Empty<string>(), StringComparer);
            Symbols = new HashSet<string>(rules.Symbols ?? Enumerable.Empty<string>(), StringComparer);
            BlockComments = rules.BlockComments ?? new List<BlockCommentInfo>();
            LineComments = rules.LineComments ?? new List<string>();
            Quotes = rules.Quotes ?? new List<QuoteInfo>();

            // Sort comments/line comments so that longer tokens come first. This results in us using
            // the longest matching token.
            BlockComments.Sort(CompareBlockComments);
            LineComments.Sort(CompareLineComments);

            QuoteEscapeChar = ParsingHelper.NullChar;
            CommentEnd = string.Empty;
        }

        // Used to sort block-comment tokens.
        private static int CompareBlockComments(BlockCommentInfo info, BlockCommentInfo info2)
        {
            return (info2.Start.Length - info.Start.Length);
        }

        // Used to sort line-comment tokens.
        private static int CompareLineComments(string token, string token2)
        {
            return (token2.Length - token.Length);
        }

        /// <summary>
        /// Adds color styling to the given source code using the current language settings and
        /// options.
        /// </summary>
        /// <param name="sourceCode">Source code to format.</param>
        /// <returns>The transformed source code.</returns>
        public string Transform(string sourceCode)
        {
            Dictionary<TokenType, string?> cssClassLookup = new Dictionary<TokenType, string?>
            {
                [TokenType.Keyword] = KeywordCssClass,
                [TokenType.Symbol] = SymbolCssClass,
                [TokenType.String] = StringCssClass,
                [TokenType.Operator] = OperatorCssClass,
                [TokenType.Comment] = CommentCssClass,
            };

            Debug.Assert(Options != null);
            Helper.Reset(sourceCode);
            StringBuilder builder = new StringBuilder();
            for (LanguageToken token = ParseNext(); token.Type != TokenType.EndOfText; token = ParseNext())
            {
                token.Value = WebUtility.HtmlEncode(token.Value);
                if (cssClassLookup.TryGetValue(token.Type, out string? style) && !string.IsNullOrWhiteSpace(style))
                    builder.AppendFormat(Options.TokenFormat, token.Value, style);
                else
                    builder.Append(token.Value);
            }

            return builder.ToString();
        }

        #region Parsing routines

        /// <summary>
        /// Token that signifies the end of a multiline comment. Valid only after
        /// <see cref="IsBlockCommentStart(ParsingHelper)"/> returns true.
        /// </summary>
        private string? CommentEnd;

        /// <summary>
        /// If this character appears within a string and is immediately followed by a quote
        /// character, the quote character is considered part of the string instead of the
        /// string terminator. Valid only after <see cref="IsQuoteChar(char)"/> returns true.
        /// </summary>
        private char? QuoteEscapeChar;

        /// <summary>
        /// Parses and returns the next token from the current text.
        /// </summary>
        private LanguageToken ParseNext()
        {
            Debug.Assert(Helper != null);
            Debug.Assert(Options != null);

            if (Helper.EndOfText)
                return new LanguageToken(TokenType.EndOfText);

            char c = Helper.Peek();
            if (char.IsWhiteSpace(c))
                return new LanguageToken(TokenType.Unclassified, ConsumeWhiteSpace());
            else if (IsBlockCommentStart())
                return new LanguageToken(TokenType.Comment, ConsumeBlockComment());
            else if (IsLineCommentStart())
                return new LanguageToken(TokenType.Comment, ConsumeLineComment());
            else if (IsSymbolFirstChar(c))
            {
                // Keyword or symbol
                string s = ConsumeSymbol();
                if (IsKeyword(s))
                    return new LanguageToken(TokenType.Keyword, s);
                else if (Options.UnclassifiedDefaultToSymbols || IsSymbol(s))
                    return new LanguageToken(TokenType.Symbol, s);
                else
                    return new LanguageToken(TokenType.Unclassified, s);
            }
            else if (IsQuoteChar(c))
                return new LanguageToken(TokenType.String, ConsumeQuotedString());
            else if (IsOperatorChar(c))
                return new LanguageToken(TokenType.Operator, ConsumeOperator());

            return new LanguageToken(TokenType.Unclassified, Helper.ParseCharacter());
        }

        /// <summary>
        /// Returns true if c can appear within symbols and keywords
        /// </summary>
        /// <param name="c">Character to compare</param>
        private bool IsSymbolChar(char c) => SymbolChars?.Contains(c) ?? false;

        /// <summary>
        /// Returns true if c can appear as the first character within symbols and keywords
        /// </summary>
        /// <param name="c">Character to compare</param>
        private bool IsSymbolFirstChar(char c) => SymbolFirstChars?.Contains(c) ?? false;

        /// <summary>
        /// Returns true if s is a legal keyword
        /// </summary>
        /// <param name="s">String to compare</param>
        private bool IsKeyword(string s) => Keywords?.Contains(s) ?? false;

        /// <summary>
        /// Returns true if s is a defined symbol
        /// </summary>
        /// <param name="s">String to compare</param>
        internal bool IsSymbol(string s) => Symbols?.Contains(s) ?? false;

        /// <summary>
        /// Returns true if c can appear within language operators.
        /// </summary>
        /// <param name="c">Character to compare</param>
        /// <returns>True if <paramref name="c"/> is a valid operator character.</returns>
        internal bool IsOperatorChar(char c) => OperatorChars?.Contains(c) ?? false;

        /// <summary>
        /// Returns <c>true</c> if <see cref="Helper"/> is at the start of a line comment.
        /// </summary>
        /// <returns>True if <see cref="Helper"/> is at the start of a line comment.</returns>
        private bool IsLineCommentStart()
        {
            Debug.Assert(Helper != null);

            if (LineComments != null)
            {
                foreach (string lineComment in LineComments)
                {
                    if (Helper.MatchesCurrentPosition(lineComment))
                        return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns <c>true</c> if <see cref="Helper"/> is at the start of a block comment. If
        /// <c>true</c> is returned, this method also sets <see cref="CommentEnd"/> to the comment
        /// terminator that corresponds to this comment start string.
        /// </summary>
        /// <returns>True if <see cref="Helper"/> is at the start of a block comment.</returns>
        private bool IsBlockCommentStart()
        {
            Debug.Assert(Helper != null);

            if (BlockComments != null)
            {
                foreach (BlockCommentInfo comment in BlockComments)
                {
                    if (Helper.MatchesCurrentPosition(comment.Start))
                    {
                        CommentEnd = comment.End;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns <c>true</c> if <paramref name="c"/> is a quote character that delimits
        /// string literals. If <c>true</c> is returned, this method also sets
        /// <see cref="QuoteEscapeChar"/> to the escape character that corresponds to this
        /// quote character.
        /// </summary>
        /// <param name="c">Character to compare.</param>
        /// <returns>True if <paramref name="c"/> is a quote character.</returns>
        private bool IsQuoteChar(char c)
        {
            if (Quotes != null)
            {
                foreach (QuoteInfo quote in Quotes)
                {
                    if (quote.Character == c)
                    {
                        QuoteEscapeChar = quote.Escape;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Consumes all whitespace characters.
        /// </summary>
        /// <returns>The consumed characters</returns>
        private string ConsumeWhiteSpace()
        {
            Debug.Assert(char.IsWhiteSpace(Helper.Peek()));
            return Helper.ParseWhile(char.IsWhiteSpace);
        }

        /// <summary>
        /// Consumes all characters in a block comment.
        /// </summary>
        /// <returns>The consumed characters</returns>
        private string ConsumeBlockComment()
        {
            Debug.Assert(IsBlockCommentStart());
            return Helper.ParseTo(CommentEnd, StringComparison, true);
        }

        /// <summary>
        /// Consumes all characters in a line comment.
        /// </summary>
        /// <returns>The consumed characters</returns>
        private string ConsumeLineComment()
        {
            Debug.Assert(IsLineCommentStart());
            return Helper.ParseToEndOfLine();
        }

        /// <summary>
        /// Consumes all symbol characters.
        /// </summary>
        /// <returns>The consumed characters</returns>
        private string ConsumeSymbol()
        {
            Debug.Assert(IsSymbolFirstChar(Helper.Peek()));
            return Helper.ParseWhile(IsSymbolChar);
        }

        /// <summary>
        /// Consumes a quoted string.
        /// </summary>
        /// <returns>The consumed characters</returns>
        private string ConsumeQuotedString()
        {
            Debug.Assert(IsQuoteChar(Helper.Peek()));
            return Helper.ParseQuotedText(QuoteEscapeChar, true, true);
        }

        /// <summary>
        /// Consumes all operator characters.
        /// </summary>
        /// <returns>The consumed characters</returns>
        private string ConsumeOperator()
        {
            Debug.Assert(IsOperatorChar(Helper.Peek()));
            return Helper.ParseWhile(IsOperatorChar);
        }

        #endregion

    }
}
