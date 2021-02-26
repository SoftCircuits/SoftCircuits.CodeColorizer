// Copyright (c) 2020-2021 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using System.Xml;

namespace SoftCircuits.CodeColorizer.Extensions
{
    /// <summary>
    /// Extension methods for XmlElement and XmlAttribute to simplify working
    /// with XML files.
    /// </summary>
    internal static class XmlExtensions
    {
        public static string? GetValue(this XmlElement? element, string? defaultValue = default)
        {
            return element?.InnerText?.Trim() ?? defaultValue;
        }

        public static bool GetBoolValue(this XmlElement? element, bool defaultValue = default)
        {
            return ParseBoolean(element?.InnerText?.Trim(), defaultValue);
        }

        public static string? GetValue(this XmlAttribute? attribute, string? defaultValue = default)
        {
            return attribute?.Value?.Trim() ?? defaultValue;
        }

        private static bool ParseBoolean(string? s, bool defaultValue)
        {
            return !string.IsNullOrWhiteSpace(s) ? IsTrue(s) : defaultValue;
        }

        private static bool IsTrue(string s)
        {
            s = s.ToLower();
            return s == "true" || s == "yes" || s == "on" || s == "1";
        }
    }
}
