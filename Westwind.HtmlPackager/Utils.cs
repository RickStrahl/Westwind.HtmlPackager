#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 
 *          http://www.west-wind.com/
 * 
 * Created: 08/28/2018
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Westwind.HtmlPackager.Utilities
{
    public class Utils
    {
        /// <summary>
        /// Extracts a string from between a pair of delimiters. Only the first 
        /// instance is found.
        /// </summary>
        /// <param name="source">Input String to work on</param>
        /// <param name="StartDelim">Beginning delimiter</param>
        /// <param name="endDelim">ending delimiter</param>
        /// <param name="CaseInsensitive">Determines whether the search for delimiters is case sensitive</param>
        /// <returns>Extracted string or ""</returns>
        internal static string ExtractString(string source,
            string beginDelim,
            string endDelim,
            bool caseSensitive = false,
            bool allowMissingEndDelimiter = false,
            bool returnDelimiters = false)
        {
            int at1, at2;

            if (string.IsNullOrEmpty(source))
                return string.Empty;

            if (caseSensitive)
            {
                at1 = source.IndexOf(beginDelim);
                if (at1 == -1)
                    return string.Empty;

                at2 = source.IndexOf(endDelim, at1 + beginDelim.Length);
            }
            else
            {
                //string Lower = source.ToLower();
                at1 = source.IndexOf(beginDelim, 0, source.Length, StringComparison.OrdinalIgnoreCase);
                if (at1 == -1)
                    return string.Empty;

                at2 = source.IndexOf(endDelim, at1 + beginDelim.Length, StringComparison.OrdinalIgnoreCase);
            }

            if (allowMissingEndDelimiter && at2 < 0)
            {
                if (!returnDelimiters)
                    return source.Substring(at1 + beginDelim.Length);

                return source.Substring(at1);
            }

            if (at1 > -1 && at2 > 1)
            {
                if (!returnDelimiters)
                    return source.Substring(at1 + beginDelim.Length, at2 - at1 - beginDelim.Length);

                return source.Substring(at1, at2 - at1 + endDelim.Length);
            }

            return string.Empty;
        }


        /// <summary>
        /// Generates a unique Id as a string of up to 16 characters.
        /// Based on a GUID and the size takes that subset of a the
        /// Guid's 16 bytes to create a string id.
        /// 
        /// String Id contains numbers and lower case alpha chars 36 total.
        /// 
        /// Sizes: 6 gives roughly 99.97% uniqueness. 
        ///        8 gives less than 1 in a million doubles.
        ///        16 will give near full GUID strength uniqueness
        /// </summary>
        /// <param name="stringSize">Number of characters to generate between 8 and 16</param>
        /// <param name="additionalCharacters">Any additional characters you allow in the string. 
        /// You can add upper case letters and symbols which are not included in the default
        /// which includes only digits and lower case letters.
        /// </param>
        /// <returns></returns>        
        internal static string GenerateUniqueId(int stringSize = 8, string additionalCharacters = null)
        {
            string chars = "abcdefghijkmnopqrstuvwxyz1234567890" + (additionalCharacters ?? string.Empty);
            StringBuilder result = new StringBuilder(stringSize);
            int count = 0;


            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                result.Append(chars[b % (chars.Length)]);
                count++;
                if (count >= stringSize)
                    break;
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns the media type for a given file based on its extension
        /// on a filename or url passed in.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        internal static string GetMediaTypeFromFilename(string file)
        {
            if (string.IsNullOrEmpty(file))
                return file;

            string ext = System.IO.Path.GetExtension(file).ToLower();
            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".gif":
                    return "image/gif";
                case ".bmp":
                    return "image/bmp";
                case ".tif":
                case ".tiff":
                    return "image/tiff";
                case ".woff":
                    return "application/font-woff";
                case ".svg":
                    return "image/svg+xml";
                case ".woff2":
                    return "font/woff2";
                case ".au":
                case ".snd":
                    return "audio/basic";
                case ".mp3":
                    return "audio/mpeg";
                case ".mp4":
                    return "audio/mp4";
                case ".wav":
                    return "audio/vnd.wav";
                default:
                    return "application/image";
            }
        }

        /// <summary>
        /// Uses the Shell Extensions to launch a program based on URL moniker or file name
        /// Basically a wrapper around ShellExecute
        /// </summary>
        /// <param name="url">Any URL Moniker that the Windows Shell understands (URL, Word Docs, PDF, Email links etc.)</param>
        /// <returns></returns>
        public static int GoUrl(string url)
        {
            string TPath = Path.GetTempPath();

            ProcessStartInfo info = new ProcessStartInfo();
            info.UseShellExecute = true;
            info.Verb = "Open";
            info.WorkingDirectory = TPath;
            info.FileName = url;

            Process process = new Process();
            process.StartInfo = info;
            process.Start();

            return 0;

        }
    }
}
