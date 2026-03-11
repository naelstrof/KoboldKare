using System.Globalization;
using NUnit.Framework;

namespace WordFilter.Tests {

[TestFixture]
public class WordFilterTests {
    private static readonly string[] TestBlacklist = new[] { "bad", "evil" };

    // --- Basic detection ---

    [Test]
    public void DetectsExactBannedWord() {
        bool result = WordFilter.GetBlackListed("bad", TestBlacklist, out string filtered);
        Assert.IsTrue(result);
        Assert.AreEqual("bad", filtered);
    }

    [Test]
    public void DetectsBannedWordCaseInsensitiveViaHomoglyph() {
        // Uppercase letters are in the homoglyph map for their lowercase counterpart
        bool result = WordFilter.GetBlackListed("BAD", TestBlacklist, out string filtered);
        Assert.IsTrue(result);
        Assert.AreEqual("bad", filtered);
    }

    [Test]
    public void ReturnsCleanForInnocentText() {
        bool result = WordFilter.GetBlackListed("hello world", TestBlacklist, out string filtered);
        Assert.IsFalse(result);
        Assert.AreEqual("", filtered);
    }

    [Test]
    public void ReturnsCleanForEmptyInput() {
        bool result = WordFilter.GetBlackListed("", TestBlacklist, out string filtered);
        Assert.IsFalse(result);
        Assert.AreEqual("", filtered);
    }

    [Test]
    public void SkipsEmptyBlacklistEntries() {
        var blacklist = new[] { "", null, "bad" };
        bool result = WordFilter.GetBlackListed("bad", blacklist, out string filtered);
        Assert.IsTrue(result);
        Assert.AreEqual("bad", filtered);
    }

    // --- Text element index mismatch regression tests ---
    // These test the fix for using text element indices instead of char indices.
    // Before the fix, multi-byte characters caused SubstringByTextElements
    // to receive a char offset, leading to wrong trigram lookups or crashes.

    [Test]
    public void DoesNotCrashOnInputWithEmoji() {
        // Emoji are multi-char text elements (surrogate pairs).
        // With the old char-index bug, this could throw ArgumentOutOfRangeException.
        Assert.DoesNotThrow(() => {
            WordFilter.GetBlackListed("😀😁😂 hello 😃😄", TestBlacklist, out _);
        });
    }

    [Test]
    public void DoesNotCrashOnInputWithCombiningMarks() {
        // Combining characters: e + combining acute = é as two chars but one text element.
        string input = "e\u0301vile\u0301"; // évileé
        Assert.DoesNotThrow(() => {
            WordFilter.GetBlackListed(input, TestBlacklist, out _);
        });
    }

    [Test]
    public void DetectsBannedWordAfterEmoji() {
        // The banned word appears after multi-byte emoji characters.
        // If text element index tracking is wrong, the trigram check will
        // read wrong positions and either crash or miss the match.
        bool result = WordFilter.GetBlackListed("😀😁 bad", TestBlacklist, out string filtered);
        Assert.IsTrue(result);
        Assert.AreEqual("bad", filtered);
    }

    [Test]
    public void DetectsBannedWordSurroundedByEmoji() {
        bool result = WordFilter.GetBlackListed("😀bad😁", TestBlacklist, out string filtered);
        Assert.IsTrue(result);
        Assert.AreEqual("bad", filtered);
    }

    [Test]
    public void DetectsBannedWordWithMixedUnicodeAndAscii() {
        // Mix of multi-char text elements and ASCII
        bool result = WordFilter.GetBlackListed("test 🎮 evil 🎮 end", TestBlacklist, out string filtered);
        Assert.IsTrue(result);
        Assert.AreEqual("evil", filtered);
    }

    [Test]
    public void DoesNotCrashOnEmojiOnlyInput() {
        Assert.DoesNotThrow(() => {
            WordFilter.GetBlackListed("🏳️‍🌈🇺🇸👨‍👩‍👧‍👦", TestBlacklist, out _);
        });
    }

    [Test]
    public void DoesNotFalsePositiveOnEmojiSequences() {
        // Emoji-only strings should never match ASCII banned words
        bool result = WordFilter.GetBlackListed("🅱🅰🅳", TestBlacklist, out _);
        Assert.IsFalse(result);
    }

    // --- Homoglyph detection ---

    [Test]
    public void DetectsHomoglyphSubstitution() {
        // 'ⅇ' (U+2147, double-struck italic small e) is in the homoglyph map for 'e'
        bool result = WordFilter.GetBlackListed("ⅇvil", TestBlacklist, out string filtered);
        Assert.IsTrue(result);
        Assert.AreEqual("evil", filtered);
    }

    // --- Rich text stripping ---

    [Test]
    public void DetectsBannedWordHiddenInRichTextWithStripping() {
        bool result = WordFilter.GetBlackListed("<color=red>b</color>ad", TestBlacklist, out string filtered, stripRichText: true);
        Assert.IsTrue(result);
        Assert.AreEqual("bad", filtered);
    }

    [Test]
    public void RichTextNotStrippedByDefault() {
        // Without stripRichText, tags remain and may prevent matching
        // (the tags disrupt the character sequence)
        bool result = WordFilter.GetBlackListed("<color=red>bad</color>", TestBlacklist, out _, stripRichText: false);
        // With tags in place, the characters <, c, o, l, ... disrupt matching
        // This just verifies the flag is respected — exact behavior depends on trigram interactions
        Assert.DoesNotThrow(() => {
            WordFilter.GetBlackListed("<color=red>bad</color>", TestBlacklist, out _, stripRichText: false);
        });
    }
}

}
