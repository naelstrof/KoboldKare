using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace WordFilter {
    public static class StringExt {
        private static int ParserTrim(int current, string text) {
            for (var i = current; i < text.Length; i++) {
                var character = text[i];
                if (character == '<' || character == '>') {
                    return i;
                }
            }
            return text.Length;
        }

        private static readonly List<string> knownColors = new() {
            "black",
            "orange",
            "purple",
            "yellow",
            "red",
            "green",
            "blue",
            "white",
        };
        
        private static readonly List<string> knownOpeningTags = new() {
            "i",
            "b",
            "br",
            "s",
            "strikethrough",
            "sub",
            "sup",
            "u",
        };
        
        private static readonly List<string> knownClosingTags = new () {
            "color",
            "i",
            "b",
            "s",
            "strikethrough",
            "sub",
            "sup",
            "u",
        };

        private static bool TryParseStartColorTag(string tag) {
            if (!tag.StartsWith("color") || !tag.Contains('=')) {
                return false;
            }
            var indexOfEquals = tag.IndexOf('=');
            var color = tag.Substring(indexOfEquals+1, tag.Length-(indexOfEquals+1));
            if (knownColors.Contains(color.Trim('"'))) return true;
            if (color.Length != 7 && color.Length != 9) return false;
            if (color[0] != '#') return false;
            return TryParseHex(color);
        }

        private static bool TryParseHex(ReadOnlySpan<char> color) {
            if (color.Length != 7 && color.Length != 9) return false;
            if (color[0] != '#') return false;
            for (int i = 1; i < color.Length; i++) {
                if ((byte)color[i] >= '0' && (byte)color[i] <= '9') { continue; } // Is it a number?
                if ((byte)color[i] >= 'A' && (byte)color[i] <= 'Z') { continue; } // Is it A-F?
                if ((byte)color[i] >= 'a' && (byte)color[i] <= 'z') { continue; } // Is it a-f?
                return false;
            }
            return true;
        }

        private static bool ParseStartTag(ReadOnlySpan<char> tag) {
            if (TryParseHex(tag)) {
                return true;
            }

            var tagString = tag.ToString();
            if (tag.StartsWith("color")) {
                return TryParseStartColorTag(tagString);
            }
            return knownOpeningTags.Contains(tagString);
        }
        private static bool ParseEndTag(ReadOnlySpan<char> tag) {
            return knownClosingTags.Contains(tag.ToString());
        }
        
        public static string StripRichText(this string text) {
            var builder = new StringBuilder();
            var i = 0;
            while (i < text.Length) {
                var character = text[i];
                var nextIndex = ParserTrim(i+1, text);
                if (nextIndex < text.Length && character == '<' && text[nextIndex] == '>') {
                    if (text[i + 1] == '/') {
                        var tag = text.AsSpan(i + 2, (nextIndex)-(i+2));
                        if (ParseEndTag(tag)) {
                            i = nextIndex+1;
                            continue;
                        }
                    } else {
                        var tag = text.AsSpan(i + 1, (nextIndex)-(i+1));
                        if (ParseStartTag(tag)) {
                            i = nextIndex+1;
                            continue;
                        }
                    }
                }
                builder.Append(text.AsSpan(i, nextIndex-i));
                i = nextIndex;
            }
            return builder.ToString();
        }
    }
}
