#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 
 *          http://www.west-wind.com/
 * 
 * Created: 8/29/2018
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
using System.Reflection;
using Westwind.HtmlPackager;
using Westwind.HtmlPackager.ConsoleApp;
using Westwind.HtmlPackager.Utilities;

namespace HtmlPackager.ConsoleApp
{
    public class HtmlPackagerProcessor
    {
        HtmlPackagerCommandLineParser Parser { get; set; }

        public HtmlPackagerProcessor(HtmlPackagerCommandLineParser parser = null)
        {
            Parser = parser;
        }

        public bool Process()
        {
            var consoleColor = Console.ForegroundColor;

            var packager = new Westwind.HtmlPackager.HtmlPackager();
            if (Parser.Verbose)
            {
                packager.WriteMessage += WriteConsole;
            }            

            if (string.IsNullOrEmpty(Parser.TargetFile))
            {
                string output = packager.PackageHtml(Parser.SourceUrl);
                if (output == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: Failed to create HTML output. ");
                    Console.WriteLine(packager.ErrorMessage);
                    Console.ForegroundColor = consoleColor;
                    return false;
                }
                Console.WriteLine(output);
                return true;
            }

            var version = Assembly.GetEntryAssembly().GetName().Version;
            var ver = version.Major + "." + version.Minor + (version.Build > 0 ? "." + version.Build : string.Empty);
            ConsoleHelper.WriteWrappedHeader($"West Wind HTML Packager v{ver}");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Processing {Parser.SourceUrl}...");


            if (Parser.ZipDependencies)
            {
                if (!packager.PackageHtmlToZipFile(Parser.SourceUrl, Parser.TargetFile))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: Failed to create Zip file");
                    Console.WriteLine(packager.ErrorMessage);
                    Console.ForegroundColor = consoleColor;
                    return false;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Done. Created {Parser.TargetFile} Zip file output.");
                Console.ForegroundColor = consoleColor;

                if (Parser.DisplayHtml)                
                    Utils.GoUrl(Parser.TargetFile);                
                return true;
            }

            if (Parser.ExternalDependencies)
            {
                if (!packager.PackageHtmlToFolder(Parser.SourceUrl, Parser.TargetFile, null,
                    Parser.ExternalDependencies))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: Failed to create HTML output.");
                    Console.WriteLine(packager.ErrorMessage);
                    Console.ForegroundColor = consoleColor;
                    return false;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Done. Created {Parser.TargetFile} plus dependencies.");
                Console.ForegroundColor = consoleColor;

                if (Parser.DisplayHtml)
                    Utils.GoUrl(Parser.TargetFile);

                return true;
            }


            if (!packager.PackageHtmlToFile(Parser.SourceUrl, Parser.TargetFile, null, Parser.ExternalDependencies))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Failed to create HTML output.");
                Console.ForegroundColor = consoleColor;
                return false;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Done. Created {Parser.TargetFile}.");
            Console.ForegroundColor = consoleColor;

            if (Parser.DisplayHtml)
                Utils.GoUrl(Parser.TargetFile);

            return true;
        }

        private static void WriteConsole(string msg, MessageModes mode)
        {
            var consoleColor = Console.ForegroundColor;

            if (mode == Westwind.HtmlPackager.MessageModes.Error)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (mode == Westwind.HtmlPackager.MessageModes.Warning)
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            else if (mode == Westwind.HtmlPackager.MessageModes.Information)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (mode == Westwind.HtmlPackager.MessageModes.Information2)
                Console.ForegroundColor = ConsoleColor.Green;
            else
                Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(msg);

            Console.ForegroundColor = consoleColor;
        }

        /// <summary>
        /// Event that allows you to override the output that is sent 
        /// by this class. If not set output is sent to the Console.
        /// </summary>
        public event Action<string> ShowMessage;

        public virtual void OnShowMessage(string message)
        {
            
            if (ShowMessage != null)
                ShowMessage(message);
            else
                Console.WriteLine(message);
        }

      
    }

}
