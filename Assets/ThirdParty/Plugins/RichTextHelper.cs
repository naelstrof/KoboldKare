//  Copyright 2018 Park Juhyung <majecty@gmail.com>
//  Permission is hereby granted, free of charge, to any person obtaining a 
//  copy of this software and associated documentation files (the "Software"), 
//  to deal in the Software without restriction, including without limitation 
//  the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//  and/or sell copies of the Software, and to permit persons to whom the 
//  Software is furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in 
//  all copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS 
//  OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
//  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//  DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace RichTextSubstringHelper
{
    public static class StringExt
    {
        public static string RichTextSubString(this string text, int length, bool withTags = true)
        {
            var m = new RichTextSubStringMaker(text);
            for (int i = 0; i < length; i++)
            {
                m.Consume();
            }
            if (withTags) {
                return m.GetRichText();
            } else {
                return m.GetText();
            }
        }

        public static int RichTextLength(this string text)
        {
            var m = new RichTextSubStringMaker(text);
            var length = 0;
            while (m.IsConsumable())
            {
                if (m.Consume())
                {
                    length += 1;
                }
            }
            return length;
        }
    }

    class RichTextTag
    {
        public string tagName;
        public string endTag
        {
            get
            {
                return "</" + tagName + ">";
            }
        }
    }

    public class RichTextSubStringMaker
    {
        private string originalText;
        private string middleText;
        private string textWithoutTags;
        private Stack<RichTextTag> tagStack;
        private int consumedLength;

        private static readonly char[] tagBrackets = new char[2] { '<', '>' };

        public RichTextSubStringMaker(string original)
        {
            this.originalText = original;
            middleText = "";
            tagStack = new Stack<RichTextTag>();
            consumedLength = 0;
        }
        public string GetText() {
            return textWithoutTags;
        }

        public string GetRichText()
        {
            if (tagStack.Count == 0)
            {
                return middleText;
            }

            var ret = middleText;
            // Caution! we iterate tags from the last item of the stack.
            // copiedQueue's first item is tagStack's last item.
            var copiedQueue = new Queue<RichTextTag>(tagStack);
            while (copiedQueue.Count != 0)
            {
                ret += copiedQueue.Dequeue().endTag;
            }
            return ret;
        }

        public bool IsConsumable()
        {
            return consumedLength < originalText.Length;
        }

        // Return if real character is consumed
        // If line's last part is closing tag, this function return false when consumed last closing tag.
        public bool Consume()
        {
            Debug.Assert(IsConsumable());
            var peekedOriginChar = PeekNextOriginChar();
            // Only consume the start tag if a tag closing bracket (>) was found.
            var isStartTag = peekedOriginChar == '<' && IsNextTagBracketClosing(consumedLength + 1);
            var isEndTag = peekedOriginChar == '<' && PeekNextNextOriginChar() == '/';
            if (isStartTag || isEndTag)
            {
                if (isEndTag)
                {
                    ConsumeEndTag();
                }
                else if (isStartTag)
                {
                    ConsumeStartTag();
                }

                if (IsConsumable())
                {
                    return Consume();
                }
                else
                {
                    return false;
                }
            }
            else
            {
                textWithoutTags += peekedOriginChar;
                ConsumeRawChar();
                return true;
            }
        }

        private char? PeekNextNextOriginChar()
        {
            if (originalText.Length <= consumedLength + 1)
            {
                return null;
            }
            return originalText[consumedLength + 1];
        }

        private char PeekNextOriginChar()
        {
            return originalText[consumedLength];
        }

        // Returns if the first tag bracket found in the string is '>', as opposed to '<'
        private bool IsNextTagBracketClosing(int charIndexToStartSearch = 0)
        {
            int bracketIndex = originalText.IndexOfAny(tagBrackets, charIndexToStartSearch);

            return bracketIndex != -1 && originalText[bracketIndex] == '>';
        }

        private void ConsumeStartTag()
        {
            Debug.Assert(PeekNextOriginChar() == '<');
            var tagName = "";
            bool tagNameComplete = false;

            ConsumeRawChar();

            while (true)
            {
                var consumedChar = ConsumeRawChar();
                if (consumedChar == null)
                {
                    Debug.LogError("Cannot close start tag");
                    return;
                }

                if (consumedChar == '>')
                {
                    if (tagName == "")
                    {
                        Debug.LogWarning("Empty tag name");
                    }

                    tagStack.Push(new RichTextTag
                    {
                        tagName = tagName
                    });
                    return;
                }

                if (!tagNameComplete)
                {
                    if (!char.IsLetterOrDigit(consumedChar.Value))
                    {
                        tagNameComplete = true;
                    }
                    else
                    {
                        tagName += consumedChar;
                    }
                }
            }
        }

        private void ConsumeEndTag()
        {
            Debug.Assert(PeekNextOriginChar() == '<');
            Debug.Assert(PeekNextNextOriginChar() == '/');
            var tagName = "";
            bool tagNameComplete = false;

            ConsumeRawChar();
            ConsumeRawChar();

            while (true)
            {
                var consumedChar = ConsumeRawChar();
                if (consumedChar == null)
                {
                    Debug.LogError("Cannot close start tag");
                    return;
                }

                if (consumedChar == '>')
                {
                    if (tagName == "")
                    {
                        Debug.LogWarning("Empty tag name");
                    }

                    if (tagStack.Count == 0)
                    {
                        Debug.LogError("Could not pop tag " + tagName);
                    }

                    if (tagStack.Peek().tagName != tagName)
                    {
                        Debug.LogError("Could not pop tag " + tagName + " expeted " + tagStack.Peek().tagName);
                    }

                    tagStack.Pop();
                    return;
                }

                if (!tagNameComplete)
                {
                    if (!char.IsLetterOrDigit(consumedChar.Value))
                    {
                        tagNameComplete = true;
                    }
                    else
                    {
                        tagName += consumedChar;
                    }
                }
            }
        }

        private char? ConsumeRawChar()
        {
            if (consumedLength > originalText.Length)
            {
                return null;
            }
            var peeked = PeekNextOriginChar();
            this.middleText += peeked;
            this.consumedLength += 1;
            return peeked;
        }
    }
}
