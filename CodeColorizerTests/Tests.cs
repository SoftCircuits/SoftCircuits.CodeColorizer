// Copyright (c) 2020-2021 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftCircuits.CodeColorizer;
using System.Collections.Generic;

namespace CodeColorizerTests
{
    [TestClass]
    public class Tests
    {
        readonly LanguageRules LanguageRules = new LanguageRules
        {
            Name = "cs",
            CaseSensitive = true,
            SymbolChars = LanguageRules.DefaultSymbolChars,
            SymbolFirstChars = LanguageRules.DefaultSymbolFirstChars,
            OperatorChars = "+-*/%&|^~<>=!",
            Quotes = new List<QuoteInfo>
            {
                new QuoteInfo('"', '"'),
                new QuoteInfo('\'', '\\'),
            },
            BlockComments = new List<BlockCommentInfo>
            {
                new BlockCommentInfo("/*", "*/"),
            },
            LineComments = new List<string>
            {
                "//",
            },
            Keywords = new List<string>
            {
                "abstract",
                "as",
                "base",
                "bool",
                "break",
                "byte",
                "case",
                "catch",
                "char",
                "checked",
                "class",
                "const",
                "continue",
                "decimal",
                "default",
                "delegate",
                "do",
                "double",
                "else",
                "enum",
                "event",
                "explicit",
                "extern",
                "false",
                "finally",
                "fixed",
                "float",
                "for",
                "foreach",
                "goto",
                "if",
                "implicit",
                "in",
                "int",
                "interface",
                "internal",
                "is",
                "lock",
                "long",
                "nameof",
                "namespace",
                "new",
                "null",
                "object",
                "operator",
                "out",
                "override",
                "params",
                "private",
                "protected",
                "public",
                "readonly",
                "ref",
                "return",
                "sbyte",
                "sealed",
                "short",
                "sizeof",
                "stackalloc",
                "static",
                "string",
                "struct",
                "switch",
                "this",
                "throw",
                "true",
                "try",
                "typeof",
                "uint",
                "ulong",
                "unchecked",
                "unsafe",
                "ushort",
                "using",
                "virtual",
                "void",
                "volatile",
                "while",
            },
            Symbols = null,
        };

        readonly string SampleCode = @"// This is a comment

int func(int i, double d)
{
    /* Another comment */
    int i = 1234;
    string s = ""abc"";
}
";

        readonly string ColoredCode = @"<span class=""comment"">// This is a comment</span>

<span class=""keyword"">int</span> func(<span class=""keyword"">int</span> i, <span class=""keyword"">double</span> d)
{
    <span class=""comment"">/* Another comment */</span>
    <span class=""keyword"">int</span> i <span class=""operator"">=</span> 1234;
    <span class=""keyword"">string</span> s <span class=""operator"">=</span> <span class=""string"">&quot;abc&quot;</span>;
}
";

        private static CodeColorizer GetColorizer(LanguageRules rules) => new CodeColorizer(rules)
        {
            CommentCssClass = "comment",
            KeywordCssClass = "keyword",
            OperatorCssClass = "operator",
            StringCssClass = "string",
            SymbolCssClass = "symbol",
        };

        [TestMethod]
        public void BasicTest()
        {
            CodeColorizer colorizer = GetColorizer(LanguageRules);
            Assert.AreEqual(ColoredCode, colorizer.Transform(SampleCode));
        }

        [TestMethod]
        public void TestComments()
        {
            CodeColorizer colorizer = GetColorizer(LanguageRules);
            colorizer.Options.TokenFormat = "{1}";
            Assert.AreEqual(" comment", colorizer.Transform(" // Comment "));
            Assert.AreEqual(" comment ", colorizer.Transform(" /* Comment */ "));
        }

        [TestMethod]
        public void TestKeywords()
        {
            CodeColorizer colorizer = GetColorizer(LanguageRules);
            colorizer.Options.TokenFormat = "{1}";
            Assert.AreEqual(" keyword i ", colorizer.Transform(" int i "));
        }

        [TestMethod]
        public void TestOperators()
        {
            CodeColorizer colorizer = GetColorizer(LanguageRules);
            colorizer.Options.TokenFormat = "{1}";
            Assert.AreEqual(" a operator b operator c ", colorizer.Transform(" a = b * c "));
        }

        [TestMethod]
        public void TestStrings()
        {
            CodeColorizer colorizer = GetColorizer(LanguageRules);
            colorizer.Options.TokenFormat = "{1}";
            Assert.AreEqual(" string ", colorizer.Transform(" \"abc\" "));
            Assert.AreEqual(" string ", colorizer.Transform(" 'a' "));
        }

        [TestMethod]
        public void TestSymbols()
        {
            CodeColorizer colorizer = GetColorizer(LanguageRules);
            colorizer.Options.TokenFormat = "{1}";
            Assert.AreEqual(" abc def ", colorizer.Transform(" abc def "));
            colorizer.Options.UnclassifiedDefaultToSymbols = true;
            Assert.AreEqual(" symbol symbol ", colorizer.Transform(" abc def "));

            LanguageRules.Symbols = new List<string>() { "abc" };
            colorizer = GetColorizer(LanguageRules);
            colorizer.Options.TokenFormat = "{1}";
            Assert.AreEqual(" symbol def ", colorizer.Transform(" abc def "));

            LanguageRules.Symbols = null;
        }

        [TestMethod]
        public void TestCaseSensitivity()
        {
            LanguageRules.Symbols = new List<string>() { "abc" };

            CodeColorizer colorizer = GetColorizer(LanguageRules);
            colorizer.Options.TokenFormat = "{1}";
            Assert.AreEqual(" INT i ", colorizer.Transform(" INT i "));
            Assert.AreEqual(" ABC def ", colorizer.Transform(" ABC def "));

            LanguageRules.CaseSensitive = false;
            LanguageRules.Symbols = new List<string>() { "abc" };
            colorizer = GetColorizer(LanguageRules);
            colorizer.Options.TokenFormat = "{1}";
            Assert.AreEqual(" keyword i ", colorizer.Transform(" INT i "));
            Assert.AreEqual(" symbol def ", colorizer.Transform(" ABC def "));

            LanguageRules.Symbols = null;
            LanguageRules.CaseSensitive = true;
        }

        [TestMethod]
        public void TestEmptyLanguage()
        {
            CodeColorizer colorizer = GetColorizer(new LanguageRules
            {
                CaseSensitive = false,
                SymbolFirstChars = null,
                SymbolChars = null,
                OperatorChars = null,
            });
            Assert.AreEqual("string s = new string(); //", colorizer.Transform("string s = new string(); //"));
        }
    }
}
