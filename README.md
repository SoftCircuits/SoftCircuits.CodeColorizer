<a href="https://www.buymeacoffee.com/jonathanwood" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/black_img.png" alt="Buy Me A Coffee" style="height: 37px !important;width: 170px !important;" ></a>

# Code Colorizer

[![NuGet version (SoftCircuits.CodeColorizer)](https://img.shields.io/nuget/v/SoftCircuits.CodeColorizer.svg?style=flat-square)](https://www.nuget.org/packages/SoftCircuits.CodeColorizer/)

```
Install-Package SoftCircuits.CodeColorizer
```

## Overview

Code Colorizer is a .NET class library to convert source code to HTML with syntax coloring. The library is language-agnostic, meaning that the the same code is used for all supported languages. Only the language rules change for each language.

## Language Rules

Each language is defined by creating a `LanguageRules` object that describes the language. The `LanguageRulesCollection` can store the rules for any number of languages, and read and write them to an XML file using the `Save()` and `Load()` methods. A link to a sample XML file is down below. I can give you a head start on defining the languages you want to support.

The XML file contains the following elements.

| Rule | Purpose
| ---- | ----
| name | The name of this language.
| caseSensitive | Determines if this language is case-sensitive (boolean).
| symbolChars | Characters that make up language keywords and symbol names.
| symbolFirstChars | Characters that can appear as the first character in language keywords and symbol names.
| operatorChars | Characters that can appear within language operators. Must include all characters used to signify comments.
| quotes | Single character used denote string literals. Also supports an optional escape character. If a string contains the escape character followed by the quote, that quote is assumed to be part of the string and not the terminator). If the language supports more than one quote type (such as " and '), you can include multiple quotes rules.
| blockComments | Defines strings to delimit block comments. If the language supports multiple block comments delimiters, you can include multiple blockComment rules.
| lineComments | String that starts a line comment (characters to the end of the line are assumed to be a comment). If the language supports multiple line comment operators, you can include multiple lineComment rules.
| keywords | Lists all the keywords supported by this language.
| symbols | Lists all the symbol names supported by this language. For example, the names of custom types (those not defined by the language itself) could be included in this list. Because this list could be incredibly long and require frequent updates, it is often not used.

## Usage

The following example loads the language rules file from "LanguageRules.xml". It then creates an instance of `Colorizer` and sets it's CSS class properties. These properties hold the names of the CSS classes used to style each element of the language. Finally, the code calls the `Colorize()` method to convert the input to HTML.

```cs
LanguageRulesCollection Languages = new LanguageRulesCollection();
Languages.Load("LanguageRules.xml");

CodeColorizer colorizer = new CodeColorizer(Languages["cs"]);
colorizer.CommentCssClass = "Comment_Class";
colorizer.KeywordCssClass = "Keyword_Class";
colorizer.OperatorCssClass = "Operator_Class";
colorizer.StringCssClass = "String_Class";
colorizer.SymbolCssClass = "Symbol_Class";
string html = colorizer.Transform(sourceCode);
```

The code above creates a `CodeColorizer` instance, passing one of the languages stored in a `LanguageRulesCollection`. It then defines the CSS class names for each token type. Finally, it calls the `Transform()` method to convert the source code.

The resulting output will be the source code will markup added to implement the class names specified in the previous step. In addition, the output will be HTML encoded. The output is formatted to appear correctly when placed within a `<pre>...</pre>` block on a web page.

## Resources

- [Sample LanguageRules File](/Languages.xml)

## More Information
For additional information and a discussion of the source code, please see my article [Colorizing Source Code](http://www.blackbeltcoder.com/Articles/strings/colorizing-source-code).
