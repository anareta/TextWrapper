using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextWrapper;

namespace TextWrapperTest
{
    [TestClass]
    public class TextUtilityTest
    {
        [TestMethod]
        public void Wrap_全角文字の改行()
        {
            string lineFeed = Environment.NewLine;

            var content = @"あいうえおあいうえおあいうえおあいうえおあいうえおあいうえおあいうえおあいうえおあいうえおあいうえお";

            var result = TextUtility.Wrap(content, "", 20);

            var lines = result.Split(new[] { lineFeed }, StringSplitOptions.None);

            Assert.AreEqual("あいうえおあいうえお", lines[0].Trim());
        }

        [TestMethod]
        public void Wrap_サロゲートペアの改行()
        {
            string lineFeed = Environment.NewLine;

            var content = @"あいうえお𠀋𠀋𠀋𠀋𠀋𠀋いうえお𠀋いうえお";

            var result = TextUtility.Wrap(content, "", 20);

            var lines = result.Split(new[] { lineFeed }, StringSplitOptions.None);

            Assert.AreEqual("あいうえお𠀋𠀋𠀋𠀋𠀋", lines[0].Trim());
            Assert.AreEqual("𠀋いうえお𠀋いうえお", lines[1].Trim());
        }

        [TestMethod]
        public void Wrap_URLは改行しない()
        {
            string lineFeed = Environment.NewLine;
            var url1 = @"https://docs.microsoft.com/ja-jp/visualstudio/designers/getting-started-with-wpf?view=vs-2019";
            var url2 = @"https://aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

            var content = url1 + lineFeed + url2;

            var result = TextUtility.Wrap(content, "  ", 30);

            var lines = result.Split(new[] { lineFeed }, StringSplitOptions.None);

            Assert.AreEqual(url1, lines[0].Trim());
            Assert.AreEqual(url2, lines[1].Trim());
        }

        [TestMethod]
        public void Wrap_空白位置で改行()
        {
            string lineFeed = Environment.NewLine;
            var url1_1 = @"a aa aaa aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var url1_2 = @"bbbbbbbbbbbbbbbbb";
            var url1 = url1_1 + " " + url1_2;
            var url2 = @"nnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnnn";
            var content = url1 + lineFeed + url2;

            var result = TextUtility.Wrap(content, "  ", 30);

            var lines = result.Split(new[] { lineFeed }, StringSplitOptions.None);
            Assert.AreEqual(url1_1, lines[0].Trim());
            Assert.AreEqual(url1_2, lines[1].Trim());
        }

        [TestMethod]
        public void Wrap_インデント_元データに改行なし()
        {
            string lineFeed = Environment.NewLine;

            var content = @"あいうえおあいうえおあいうえお";

            var result = TextUtility.Wrap(content, "  ", 12);

            var lines = result.Split(new[] { lineFeed }, StringSplitOptions.None);

            Assert.AreEqual("  あいうえお", lines[0]);
            Assert.AreEqual("  あいうえお", lines[1]);
            Assert.AreEqual("  あいうえお", lines[2]);
        }

        [TestMethod]
        public void Wrap_インデント_元データに改行あり()
        {
            string lineFeed = Environment.NewLine;

            var content = "あいうえお" + lineFeed + "あいうえお" + lineFeed + lineFeed + "あいうえお";

            var result = TextUtility.Wrap(content, "  ", 30);

            var lines = result.Split(new[] { lineFeed }, StringSplitOptions.None);

            Assert.AreEqual("  あいうえお", lines[0]);
            Assert.AreEqual("  あいうえお", lines[1]);
            Assert.AreEqual("  あいうえお", lines[3]);
        }

        [TestMethod]
        public void Wrap_行数()
        {
            string lineFeed = Environment.NewLine;

            var content = "あいうえお" + lineFeed + lineFeed + "あいうえお";

            var result = TextUtility.Wrap(content, "", 30);

            var lines = result.Split(new[] { lineFeed }, StringSplitOptions.None);

            Assert.AreEqual(3, lines.Length);
        }

        [TestMethod]
        public void Wrap_囲み_全体()
        {
            string lineFeed = Environment.NewLine;

            var content = @"<あいうえおあいうえおあいうえおあいうえおあいうえおあいうえおあいうえおあいうえおあいうえお>";

            var result = TextUtility.Wrap(content, "", 10);

            var lines = result.Split(new[] { lineFeed }, StringSplitOptions.None);

            Assert.AreEqual(content, lines[0]);
        }

        [TestMethod]
        public void Wrap_囲み_前後に文字あり()
        {
            string lineFeed = Environment.NewLine;

            var content1 = @"ん<あいうえおあいうえおあいうえおあいうえおあいうえおあいうえおあいうえおあいうえおあいうえお>";
            var content2 = @"かきくけこ";

            var result = TextUtility.Wrap(content1 + content2, "", 10);

            var lines = result.Split(new[] { lineFeed }, StringSplitOptions.None);

            Assert.AreEqual(content1, lines[0]);
            Assert.AreEqual(content2, lines[1]);
        }

        [TestMethod]
        public void Wrap_囲み_終端なし()
        {
            string lineFeed = Environment.NewLine;

            var content = @"<あいうえおあいうえおあいうえおあいうえおあいうえおあいうえおあいうえおあいうえおあいうえお";

            var result = TextUtility.Wrap(content, "", 10);

            var lines = result.Split(new[] { lineFeed }, StringSplitOptions.None);

            Assert.AreEqual("<あいうえお", lines[0]);
        }

        [TestMethod]
        public void Wrap_禁則_行頭()
        {
            string lineFeed = Environment.NewLine;

            var content = @"あいうえ」かきくけっさしすせそ";

            var result = TextUtility.Wrap(content, "", 8);

            var lines = result.Split(new[] { lineFeed }, StringSplitOptions.None);

            Assert.AreEqual("あいうえ」", lines[0]);
            Assert.AreEqual("かきくけっ", lines[1]);
        }

        [TestMethod]
        public void Wrap_禁則_行末()
        {
            string lineFeed = Environment.NewLine;

            var content = @"あいうえ「かきく【けこさしすせそ";

            var result = TextUtility.Wrap(content, "", 10);

            var lines = result.Split(new[] { lineFeed }, StringSplitOptions.None);

            Assert.AreEqual("あいうえ", lines[0]);
            Assert.AreEqual("「かきく", lines[1]);
            Assert.AreEqual("【けこさし", lines[2]);
            Assert.AreEqual("すせそ", lines[3]);
        }
    }
}

