// Copyright (c) 2020-2021 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftCircuits.CodeColorizer;
using System.IO;
using System.Text;

namespace CodeColorizerTests
{
    [TestClass]
    public class FileTest
    {
        //[TestMethod]
        //public void RunFileTest()
        //{
        //    string languagesFile = "D:\\Users\\jwood\\source\\repos\\SoftCircuits.CodeColorizer\\LanguageRules.xml";
        //    string sourceFile = "D:\\Users\\jwood\\Desktop\\Source.txt";

        //    LanguageRulesCollection rules = new LanguageRulesCollection(languagesFile);
        //    CodeColorizer colorizer = new CodeColorizer(rules["cs"])
        //    {
        //        KeywordCssClass = "keyword",
        //        SymbolCssClass = "symbol",
        //        StringCssClass = "string",
        //        OperatorCssClass = "operator",
        //        CommentCssClass = "comment",
        //    };
        //    //colorizer.Options.UnclassifiedDefaultToSymbols = true;

        //    string sourceCode = File.ReadAllText(sourceFile);
        //    StringBuilder builder = new StringBuilder();
        //    builder.AppendLine("<html>");
        //    builder.AppendLine("<head>");
        //    builder.AppendLine("<style>");
        //    builder.AppendLine("  .keyword { color: blue; }");
        //    builder.AppendLine("  .symbol { color: purple; }");
        //    builder.AppendLine("  .string { color: red; }");
        //    builder.AppendLine("  .operator { color: black; background-color: yellow; }");
        //    builder.AppendLine("  .comment { color: green; }");
        //    builder.AppendLine("</style>");
        //    builder.AppendLine("</head>");
        //    builder.AppendLine("<body>");
        //    builder.AppendLine("<pre>");
        //    builder.Append(colorizer.Transform(sourceCode));
        //    builder.AppendLine("</pre>");
        //    builder.AppendLine("</body>");
        //    builder.AppendLine("</html>");

        //    string htmlFile = Path.ChangeExtension(sourceFile, "html");
        //    File.WriteAllText(htmlFile, builder.ToString());

        //    //rules.Save($"{languagesFile} 2");
        //}
    }
}
