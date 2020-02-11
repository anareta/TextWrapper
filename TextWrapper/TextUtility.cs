using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace TextWrapper
{
    public static class TextUtility
    {
        /// <summary>
        /// テキストを折り返す
        /// </summary>
        /// <param name="text">元テキスト</param>
        /// <param name="indent">テキスト全体に適用するインデント</param>
        /// <param name="maxWidth">インデントの分を含んだ折り返し幅（ASCII範囲の文字を1、それ以外の文字を2とカウント）</param>
        public static string Wrap(string text, string indent, int maxWidth)
        {
            var result = new StringBuilder();

            var lineWidth = maxWidth;
            if (maxWidth - indent.Width() < maxWidth / 2)
            {
                // インデントが長すぎて文章がmaxWidthの半分にも満たない場合は最低限の長さを確保する
                lineWidth = indent.Width() + maxWidth / 2;
            }

            var index = 0;

            // 改行コードが２文字だと都合が悪いので一時的に置き換え
            var content = text.Trim(Environment.NewLine.ToCharArray()).Replace(Environment.NewLine, "\r");

            while (index < content.Length)
            {
                if (result.Length > 0)
                {
                    result.LineBreak();
                }

                // コンテンツのすべての文字を処理しきるまで行ごとにループ
                var hasNewLine = false;
                var line = new StringBuilder(indent);

                while (true)
                {
                    // １文字ずつlineバッファに追加する

                    var currentChar = content.SafeSubstring(index, 1);

                    if (currentChar == "\r")
                    {
                        // 次に追加する文字が改行コードならループを抜ける（折り返す）
                        hasNewLine = true;
                        ++index; // "\r"は追加せずにスキップ
                        break;
                    }

                    // 1文字分だけ追加
                    line.Append(currentChar);
                    ++index;

                    if (currentChar == "<")
                    {
                        // この記号があったときは終端記号までまとめて追加する
                        var close = content.SafeSubstring(index).IndexOf('>');
                        if (close != -1)
                        {
                            line.Append(content.SafeSubstring(index, close + 1));
                            index += close + 1;
                        }
                    }

                    var nextChar = content.SafeSubstring(index, 1);

                    if (nextChar == "\r")
                    {
                        // 次に追加する文字が改行コードならループを抜ける（折り返す）
                        hasNewLine = true;
                        ++index; // "\r"は追加せずにスキップ
                        break;
                    }

                    if ((line.Width() >= lineWidth || index >= content.Length)
                        &&
                        CanLineBreak(currentChar, nextChar))
                    {
                        break;
                    }
                }

                if (!hasNewLine)
                {
                    // 行頭禁則の対応
                    while (content.SafeSubstring(index, 1).IsStart行頭禁則文字())
                    {
                        line.Append(content.SafeSubstring(index, 1));
                        ++index;
                    }

                    // 行末禁則の対応
                    if (line.ToString().SafeSubstring(line.Length - 1, 1).IsEnd行末禁則文字())
                    {
                        line = new StringBuilder(line.ToString().SafeSubstring(0, line.Length - 1));
                        --index;
                    }
                }

                if (!string.IsNullOrWhiteSpace(line.ToString()))
                {
                    result.Append(line);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// 前後の文字から折り返しが可能か判定する
        /// </summary>
        /// <param name="prevChar">判定したい位置の１つ前の文字</param>
        /// <param name="nextChar">判定したい位置の１つ後の文字</param>
        private static bool CanLineBreak(string prevChar, string nextChar)
        {
            // ルール：空白文字を除く幅1文字が連続している途中では折り返さない（例：URLの途中、英文の単語の途中）

            if (string.IsNullOrWhiteSpace(nextChar))
            {
                // 後続に文字がないか、半角スペースの場合は折り返し可能
                return true;
            }

            // 最後に追加した文字が幅１
            if (prevChar == "\r")
            {
                // 改行コード
                return true;
            }

            if (prevChar.Width() == 1)
            {
                if (nextChar.Width() == 1)
                {
                    // 幅１文字が連続している場合
                    return false;
                }

                // 幅１文字と幅２文字の境界
                return true;
            }

            // 最後に追加した文字が幅２文字
            return true;
        }

    }

    internal static class StringEx
    {
        /// <summary>
        /// ASCII範囲の文字を１、ASCII範囲外の文字を２とカウントする
        /// </summary>
        internal static int Width(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return 0;
            }

            var enumerator = StringInfo.GetTextElementEnumerator(s);
            var enc = Encoding.UTF8;
            int count = 0;

            while (enumerator.MoveNext())
            {
                // UTF-8で１バイトならASCII範囲の文字
                count += (enc.GetByteCount(enumerator.GetTextElement()) > 1 ? 2 : 1);
            }

            return count;
        }

        /// <summary>
        /// ASCII範囲の文字を１、ASCII範囲外の文字を２とカウントする
        /// </summary>
        internal static int Width(this StringBuilder s)
        {
            return s.ToString().Width();
        }

        /// <summary>
        ///サロゲートペアや結合文字に対応したLength
        /// </summary>
        public static int LengthInTextElements(this string str)
        {
            return new StringInfo(string.IsNullOrEmpty(str) ? string.Empty : str)
                .LengthInTextElements;
        }

        /// <summary>
        ///サロゲートペアや結合文字に対応したElementAt
        /// </summary>
        public static string ElementAtInTextElements(this string str, int index)
        {
            return new StringInfo(string.IsNullOrEmpty(str) ? string.Empty : str)
                .SubstringByTextElements(index, 1);
        }

        /// <summary>
        ///サロゲートペアや結合文字に対応したSubstring
        /// </summary>
        public static string SubstringByTextElements(this string str, int startingTextElement, int lengthInTextElements)
        {
            return new StringInfo(string.IsNullOrEmpty(str) ? string.Empty : str)
                .SubstringByTextElements(startingTextElement, lengthInTextElements);
        }

        /// <summary>
        ///サロゲートペアや結合文字に対応したSubstring
        /// </summary>
        public static string SubstringByTextElements(this string str, int startingTextElement)
        {
            return new StringInfo(string.IsNullOrEmpty(str) ? string.Empty : str)
                .SubstringByTextElements(startingTextElement);
        }

        /// <summary>
        /// 末尾に改行を追加する
        /// </summary>
        internal static StringBuilder LineBreak(this StringBuilder s, int times = 1)
        {
            for (int i = 0; i < times; i++)
            {
                s.Append(Environment.NewLine);
            }
            return s;
        }

        /// <summary>
        /// 例外を出さないSubstring（例外を出すケースでは空文字を返す）
        /// </summary>
        internal static string SafeSubstring(this string s, int startIndex)
        {
            if (startIndex < 0)
            {
                startIndex = 0;
            }

            if (startIndex > s.LengthInTextElements() - 1)
            {
                return "";
            }

            return s.SubstringByTextElements(startIndex);

        }

        /// <summary>
        /// 例外を出さないSubstring（例外を出すケースでは空文字を返す）
        /// </summary>
        internal static string SafeSubstring(this string s, int startIndex, int length)
        {
            if (startIndex < 0)
            {
                startIndex = 0;
            }

            if (length < 1)
            {
                return "";
            }

            if (startIndex > s.LengthInTextElements() - 1)
            {
                return "";
            }

            if (startIndex + length > s.LengthInTextElements())
            {
                return s;
            }

            return s.SubstringByTextElements(startIndex, length);
        }

        /// <summary>
        /// 文字列が行頭禁則文字から始まっていればtrue
        /// </summary>
        internal static bool IsStart行頭禁則文字(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            return 行頭禁則.Any(forbidden => s.ElementAtInTextElements(0) == new string(forbidden, 1));
        }

        /// <summary>
        /// 文字列が行末禁則文字で終わっていればtrue
        /// </summary>
        internal static bool IsEnd行末禁則文字(this string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            return 行末禁則.Any(forbidden => s.ElementAtInTextElements(s.LengthInTextElements() - 1) == new string(forbidden, 1));
        }

        private static readonly string 行頭禁則 = "。.?!‼⁇⁈⁉,)）]｝、〕〉》」』】〙〗〟’”｠»ゝゞ\"ーァィゥェォッャュョヮヵヶぁぃぅぇぉっゃゅょゎゕゖㇰㇱㇲㇳㇴㇵㇶㇷㇸㇹㇷ゚ㇺㇻㇼㇽㇾㇿ々〻";

        private static readonly string 行末禁則 = "(（[｛〔〈《「『【〘〖〝‘“｟«\"";

    }
}
