// Electro Gryphon Games - 2016

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UPersian.Utils;

namespace UPersian.Components
{
    /// <summary>
    /// Supports RTL and middle-eastern languages text
    /// </summary>
    [AddComponentMenu("UI/RtlText")]
    public class RtlText : Text
    {
        protected char LineEnding = '\n';

        /// <summary>
        /// Original text which user sets via editor.
        /// You sould use this value if you want need original string. (to use in a third-party)
        /// </summary>
        public string BaseText
        {
            get { return base.text; }
        }

        /// <summary>
        /// get: Return RTL fixed string
        /// set: Sets base.text
        /// </summary>
        public override string text
        {
            get
            {
                // Populate base text in rect transform and calculate number of lines.
                string baseText = base.text;
                cachedTextGenerator.Populate(baseText, GetGenerationSettings(rectTransform.rect.size));
                // Make list of lines
                List<UILineInfo> lines = cachedTextGenerator.lines as List<UILineInfo>;
                if (lines == null) return null;
                string linedText = "";
                for (int i = 0; i < lines.Count; i++)
                {
                    // Find Start and Length of RTL line and append Line Ending character.
                    if (i < lines.Count - 1)
                    {
                        int startIndex = lines[i].startCharIdx;
                        int length = lines[i + 1].startCharIdx - lines[i].startCharIdx;
                        linedText += baseText.Substring(startIndex, length);
                        if (linedText.Length > 0 &&
                            linedText[linedText.Length - 1] != '\n' &&
                            linedText[linedText.Length - 1] != '\r')
                        {
                            linedText += LineEnding;
                        }
                    }
                    else
                    {
                        // For the Last line, we only need startIndex and line continues to the end.
                        linedText += baseText.Substring(lines[i].startCharIdx);
                        //if (resizeTextForBestFit) linedText += '\n';
                    }
                }
                return linedText.RtlFix();
            }
            set { base.text = value; }
        }
    }
}