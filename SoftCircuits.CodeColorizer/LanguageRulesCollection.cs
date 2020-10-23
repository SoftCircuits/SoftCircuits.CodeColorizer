// Copyright (c) 2020 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using SoftCircuits.CodeColorizer.Extensions;
using System;
using System.Collections.Generic;
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
        public LanguageRules this[string language]
        {
            get => LanguageRuleLookup.TryGetValue(language, out LanguageRules languageRules) ?
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
        public bool TryGetValue(string name, out LanguageRules rules) => LanguageRuleLookup.TryGetValue(name, out rules);

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
            XmlElement languages = doc.DocumentElement;
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
                foreach (XmlElement element in language.SelectNodes("quotes"))
                {
                    string character = element.Attributes["character"].GetValue();
                    if (character == null || character.Length != 1)
                        throw new Exception(string.Format("Language rule missing quote character attribute, or value is not exactly one character ({0})", rules.Name));
                    string escape = element.Attributes["escape"].GetValue(string.Empty);
                    if (escape.Length > 1)
                        throw new Exception(string.Format("Language rule quote escape value must be exactly one character ({0})", rules.Name));
                    rules.Quotes.Add(new QuoteInfo(character[0], (escape.Length > 0) ? (char?)escape[0] : null));
                }
                // Block Comments
                rules.BlockComments = new List<BlockCommentInfo>();
                foreach (XmlElement element in language.SelectNodes("blockComments"))
                {
                    string start = element.Attributes["start"].GetValue();
                    if (string.IsNullOrWhiteSpace(start))
                        throw new Exception(string.Format("Block comment start attribute is missing ({0})", rules.Name));
                    string end = element.Attributes["end"].GetValue();
                    if (string.IsNullOrWhiteSpace(end))
                        throw new Exception(string.Format("Block comment end attribute is missing ({0})", rules.Name));
                    rules.BlockComments.Add(new BlockCommentInfo(start, end));
                }

                // Line Comments
                rules.LineComments = new List<string>();
                foreach (XmlElement element in language.SelectNodes("lineComments"))
                    rules.LineComments.Add(element.GetValue());

                // Keywords
                XmlElement keywords = language["keywords"];
                rules.Keywords = new List<string>();
                foreach (XmlElement keyword in keywords.ChildNodes)
                    rules.Keywords.Add(keyword.GetValue());

                // Symbols
                XmlElement symbols = language["symbols"];
                rules.Symbols = new List<string>();
                foreach (XmlElement symbol in symbols.ChildNodes)
                    rules.Symbols.Add(symbol.GetValue());

                // Add to collection
                if (LanguageRuleLookup.TryGetValue(rules.Name, out LanguageRules languageRules))
                    throw new Exception($"Duplicate language name '{rules.Name}' in '{filename}'.");
                LanguageRuleLookup.Add(rules.Name, rules);
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
            XmlElement languages = (XmlElement)doc.AppendChild(doc.CreateElement("languages"));

            foreach (LanguageRules rules in LanguageRuleLookup.Values)
            {
                XmlElement language = (XmlElement)languages.AppendChild(doc.CreateElement("language"));
                language.SetAttribute("name", rules.Name);

                XmlElement element = (XmlElement)language.AppendChild(doc.CreateElement("caseSensitive"));
                element.InnerText = rules.CaseSensitive.ToString();
                element = (XmlElement)language.AppendChild(doc.CreateElement("symbolChars"));
                element.InnerText = rules.SymbolChars;
                element = (XmlElement)language.AppendChild(doc.CreateElement("symbolFirstChars"));
                element.InnerText = rules.SymbolFirstChars;
                element = (XmlElement)language.AppendChild(doc.CreateElement("operatorChars"));
                element.InnerText = rules.OperatorChars;

                if (rules.Quotes != null)
                {
                    foreach (QuoteInfo quote in rules.Quotes)
                    {
                        element = (XmlElement)language.AppendChild(doc.CreateElement("quotes"));
                        element.SetAttribute("character", quote.Character.ToString());
                        if (quote.Escape != null)
                            element.SetAttribute("escape", quote.Escape.ToString());
                    }
                }

                if (rules.BlockComments != null)
                {
                    foreach (BlockCommentInfo comment in rules.BlockComments)
                    {
                        element = (XmlElement)language.AppendChild(doc.CreateElement("blockComments"));
                        element.SetAttribute("start", comment.Start);
                        element.SetAttribute("end", comment.End);
                    }
                }

                if (rules.LineComments != null)
                {
                    foreach (string comment in rules.LineComments)
                    {
                        element = (XmlElement)language.AppendChild(doc.CreateElement("lineComments"));
                        element.InnerText = comment;
                    }
                }

                if (rules.Keywords != null)
                {
                    XmlElement keywords = (XmlElement)language.AppendChild(doc.CreateElement("keywords"));
                    foreach (string keyword in rules.Keywords)
                    {
                        element = (XmlElement)keywords.AppendChild(doc.CreateElement("keyword"));
                        element.InnerText = keyword;
                    }
                }

                if (rules.Symbols != null)
                {
                    XmlElement symbols = (XmlElement)language.AppendChild(doc.CreateElement("symbols"));
                    foreach (string symbol in rules.Symbols)
                    {
                        element = (XmlElement)symbols.AppendChild(doc.CreateElement("symbol"));
                        element.InnerText = symbol;
                    }
                }
            }

            using XmlTextWriter writer = new XmlTextWriter(filename, Encoding.UTF8);
            writer.Formatting = Formatting.Indented;
            doc.Save(writer);
        }

        #endregion

    }
}
