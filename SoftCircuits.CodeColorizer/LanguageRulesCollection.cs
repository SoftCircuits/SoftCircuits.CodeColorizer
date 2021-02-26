// Copyright (c) 2020-2021 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using SoftCircuits.CodeColorizer.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Xml;

namespace SoftCircuits.CodeColorizer
{
    /// <summary>
    /// Class to hold a collection of <see cref="LanguageRules"/>.
    /// </summary>
    public class LanguageRulesCollection
    {
        private readonly Dictionary<string, LanguageRules> LanguageRuleLookup;

        /// <summary>
        /// Initializes a new <see cref="LanguageRulesCollection"/> instance.
        /// </summary>
        public LanguageRulesCollection()
        {
            LanguageRuleLookup = new Dictionary<string, LanguageRules>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initializes a new <see cref="LanguageRulesCollection"/> instance and loads the
        /// specified languages file.
        /// </summary>
        /// <param name="filename">Languages file to load.</param>
        public LanguageRulesCollection(string filename)
        {
            LanguageRuleLookup = new Dictionary<string, LanguageRules>(StringComparer.OrdinalIgnoreCase);
            Load(filename);
        }

        /// <summary>
        /// Gets the language rules for the specified language. Returns null if the specified
        /// language is not in the collection. The language name is not case-sensitive.
        /// </summary>
        /// <param name="language">Name of the language for which to return the rules.</param>
        public LanguageRules? this[string language]
        {
            get => LanguageRuleLookup.TryGetValue(language, out LanguageRules? languageRules) ?
                languageRules :
                null;
        }

        /// <summary>
        /// Returns the number of languages contained in this collection.
        /// </summary>
        public int Count => LanguageRuleLookup.Count;

        /// <summary>
        /// Gets the value associated with the specified name.
        /// </summary>
        /// <param name="name">Name of the language to get.</param>
        /// <param name="rules">Returns the requested language if this method returns <c>true</c>.</param>
        /// <returns>True if the requested language was found; otherwise, false.</returns>
#if NETSTANDARD2_0
        public bool TryGetValue(string name, out LanguageRules rules) => LanguageRuleLookup.TryGetValue(name, out rules);
#else
        public bool TryGetValue(string name, [MaybeNullWhen(false)] out LanguageRules rules) => LanguageRuleLookup.TryGetValue(name, out rules);
#endif

        #region File operations

        /// <summary>
        /// Loads a collection of language rules from the specified XML file. Overwrites any languages
        /// already in the collection.
        /// </summary>
        /// <param name="filename">Name of the language file to read the rules from.</param>
        public void Load(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));
            if (!File.Exists(filename))
                throw new FileNotFoundException("Unable to load language rules file", filename);

            // Clear any existing language rules
            LanguageRuleLookup.Clear();

            // Load the specified XML file
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            XmlElement? languages = doc.DocumentElement;
            if (languages != null)
            {
                foreach (XmlElement language in languages.ChildNodes)
                {
                    LanguageRules rules = new LanguageRules()
                    {
                        Name = language.Attributes["name"].GetValue(),
                        CaseSensitive = language["caseSensitive"].GetBoolValue(LanguageRules.DefaultCaseSensitive),
                        SymbolChars = language["symbolChars"].GetValue(LanguageRules.DefaultSymbolChars),
                        SymbolFirstChars = language["symbolFirstChars"].GetValue(LanguageRules.DefaultSymbolFirstChars),
                        OperatorChars = language["operatorChars"].GetValue(LanguageRules.DefaultOperatorChars),

                    };
                    if (string.IsNullOrWhiteSpace(rules.Name))
                        throw new Exception("Language rule is missing name attribute");

                    // Quotes
                    rules.Quotes = new List<QuoteInfo>();
                    XmlNodeList? nodes = language.SelectNodes("quotes");
                    if (nodes != null)
                    {
                        foreach (XmlElement element in nodes)
                        {
                            string? character = element.Attributes["character"].GetValue();
                            if (character == null || character.Length != 1)
                                throw new Exception(string.Format("Language rule missing quote character attribute, or value is not exactly one character ({0})", rules.Name));
                            string? escape = element.Attributes["escape"].GetValue(string.Empty);
                            if (escape == null || escape.Length != 1)
                                throw new Exception(string.Format("Language rule missing quote escape value, or value is not exactly one character ({0})", rules.Name));
                            rules.Quotes.Add(new QuoteInfo(character[0], (escape.Length > 0) ? (char?)escape[0] : null));
                        }
                    }

                    // Block Comments
                    rules.BlockComments = new List<BlockCommentInfo>();
                    nodes = language.SelectNodes("blockComments");
                    if (nodes != null)
                    {
                        foreach (XmlElement element in nodes)
                        {
                            string? start = element.Attributes["start"].GetValue();
                            if (string.IsNullOrWhiteSpace(start))
                                throw new Exception(string.Format("Block comment start attribute is missing ({0})", rules.Name));
                            string? end = element.Attributes["end"].GetValue();
                            if (string.IsNullOrWhiteSpace(end))
                                throw new Exception(string.Format("Block comment end attribute is missing ({0})", rules.Name));
                            rules.BlockComments.Add(new BlockCommentInfo(start, end));
                        }
                    }

                    // Line Comments
                    rules.LineComments = new List<string>();
                    nodes = language.SelectNodes("lineComments");
                    if (nodes != null)
                    {
                        foreach (XmlElement element in nodes)
                        {
                            string? value = element.GetValue();
                            if (value != null)
                                rules.LineComments.Add(value);
                        }
                    }

                    // Keywords
                    XmlElement? keywords = language["keywords"];
                    rules.Keywords = new List<string>();
                    if (keywords != null)
                    {
                        foreach (XmlElement keyword in keywords.ChildNodes)
                        {
                            string? value = keyword.GetValue();
                            if (value != null)
                                rules.Keywords.Add(value);
                        }
                    }

                    // Symbols
                    XmlElement? symbols = language["symbols"];
                    rules.Symbols = new List<string>();
                    if (symbols != null)
                    {
                        foreach (XmlElement symbol in symbols.ChildNodes)
                        {
                            string? s = symbol.GetValue();
                            if (!string.IsNullOrEmpty(s))
                                rules.Symbols.Add(s);
                        }
                    }

                    // Add to collection
                    if (LanguageRuleLookup.TryGetValue(rules.Name, out LanguageRules? languageRules))
                        throw new Exception($"Duplicate language name '{rules.Name}' in '{filename}'.");
                    LanguageRuleLookup.Add(rules.Name, rules);
                }
            }
        }

        /// <summary>
        /// Writes the current list of language rules to the specified file.
        /// </summary>
        /// <param name="filename">Name of the language file to save the rules to.</param>
        public void Save(string filename)
        {
            if (filename == null)
                throw new ArgumentNullException(nameof(filename));

            XmlDocument doc = new XmlDocument();
            XmlElement languages = doc.CreateElement("languages");
            doc.AppendChild(languages);

            foreach (LanguageRules rules in LanguageRuleLookup.Values)
            {
                XmlElement language = doc.CreateElement("language");
                languages.AppendChild(language);
                language.SetAttribute("name", rules.Name);

                XmlElement element = doc.CreateElement("caseSensitive");
                language.AppendChild(element);
                element.InnerText = rules.CaseSensitive.ToString();
                element = doc.CreateElement("symbolChars");
                language.AppendChild(element);
                element.InnerText = rules.SymbolChars ?? string.Empty;
                element = doc.CreateElement("symbolFirstChars");
                language.AppendChild(element);
                element.InnerText = rules.SymbolFirstChars ?? string.Empty;
                element = doc.CreateElement("operatorChars");
                language.AppendChild(element);
                element.InnerText = rules.OperatorChars ?? string.Empty;

                if (rules.Quotes != null)
                {
                    foreach (QuoteInfo quote in rules.Quotes)
                    {
                        element = doc.CreateElement("quotes");
                        language.AppendChild(element);
                        if (quote.Escape != null)
                            element.SetAttribute("escape", quote.Escape.ToString());
                    }
                }

                if (rules.BlockComments != null)
                {
                    foreach (BlockCommentInfo comment in rules.BlockComments)
                    {
                        element = doc.CreateElement("blockComments");
                        language.AppendChild(element);
                        element.SetAttribute("start", comment.Start);
                        element.SetAttribute("end", comment.End);
                    }
                }

                if (rules.LineComments != null)
                {
                    foreach (string comment in rules.LineComments)
                    {
                        element = doc.CreateElement("lineComments");
                        language.AppendChild(element);
                        element.InnerText = comment;
                    }
                }

                if (rules.Keywords != null)
                {
                    XmlElement keywords = doc.CreateElement("keywords");
                    language.AppendChild(keywords);
                    foreach (string keyword in rules.Keywords)
                    {
                        element = doc.CreateElement("keyword");
                        keywords.AppendChild(element);
                        element.InnerText = keyword;
                    }
                }

                if (rules.Symbols != null)
                {
                    XmlElement symbols = doc.CreateElement("symbols");
                    language.AppendChild(symbols);
                    foreach (string symbol in rules.Symbols)
                    {
                        element = doc.CreateElement("symbol");
                        symbols.AppendChild(element);
                        element.InnerText = symbol;
                    }
                }
            }

            using XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8)
            {
                Formatting = Formatting.Indented
            };
            doc.Save(writer);
        }

#endregion

    }
}
