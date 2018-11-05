using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HtmlPackager.ConsoleApp;
using Westwind.HtmlPackager.Utilities;

namespace HtmlPackager
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0 || args[0] == "HELP" || args[0] == "/?")
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                var ver = version.Major + "." + version.Minor + (version.Build > 0 ? "." + version.Build : string.Empty);
                string options =
                    $@"
Html Packager v{version}

Syntax:
-------
HtmlPackager <sourceUrl> -o <outputFile> -x -d

Commands:
---------
HELP || /?          This help display           

Options:
--------
sourceUrl           Source Url or local file to an HTML document
-o <outputFile>     Output HTML file or Zip file (-z)
                    (if output file is not provided output is sent to StdOut)                                       
-x                  Create external dependencies in folder of HTML document
-z                  Create zip file with all dependencies included in zip
-d                  Display generated HTML page or Zip file

Examples:
---------
HtmlPackager  https://markdownmonster.west-wind.com -o c:\temp\mm_home.html -d
HtmlPackager  c:\documents\somePage.html -o c:\temp\app_saved.html -x -d
HtmlPackager  %userprofile%\Documents\myapp\somePage.html -o %TEMP%\app_saved.html
HtmlPackager  https://getbootstrap.com -o c:\temp\bootstrap.zip -z -d
";

                Console.WriteLine(options);                
            }
            else
            {
                var cmdLine = new HtmlPackagerCommandLineParser();
                cmdLine.Parse();

                var del = new HtmlPackagerProcessor(cmdLine);
                del.Process();


                if (cmdLine.DisplayHtml && !string.IsNullOrEmpty(cmdLine.TargetFile))
                    Utils.GoUrl(cmdLine.TargetFile);
            }

#if DEBUG            

            Console.WriteLine("Done. Press any key to exit...");
            Console.ReadKey();
#endif
            
        }
    }
}
