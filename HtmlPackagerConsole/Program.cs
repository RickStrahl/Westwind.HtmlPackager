using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HtmlPackager.ConsoleApp;
using Westwind.HtmlPackager.ConsoleApp;
using Westwind.HtmlPackager.Utilities;

namespace HtmlPackager
{
    class Program
    {
        static void Main(string[] args)
        {
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 |
            //                                       SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;

            if (args == null || args.Length == 0 || args[0] == "HELP" || args[0] == "/?")
            {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                var ver = version.Major + "." + version.Minor +
                          (version.Build > 0 ? "." + version.Build : string.Empty);
                string options =
                    $@"
Syntax:
-------
htmlpackager <sourceUrl> -o <outputFile> [-x|-z] -d -v

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
-v                  Verbose messages

Examples:
---------
htmlpackager  https://github.com -o /temp/github_home.html
htmlpackager  /documents/somePage.html -o /temp/app_saved.html -x -d
htmlpackager  %userprofile%/Documents/myapp/somePage.html -o %TEMP%/app_saved.html
htmlpackager  https://github.com -o /temp/github-home.zip -z -d
";
                ConsoleHelper.WriteWrappedHeader($"West Wind Html Packager v{ver}");
                Console.WriteLine(options);
            }
            else
            {
                var cmdLine = new HtmlPackagerCommandLineParser();
                cmdLine.Parse();

                var processor = new HtmlPackagerProcessor(cmdLine);
                processor.Process();

                if (cmdLine.DisplayHtml && !string.IsNullOrEmpty(cmdLine.TargetFile))
                    Utils.GoUrl(Path.GetFullPath(cmdLine.TargetFile));
            }

#if DEBUG

            Console.WriteLine("Done. Press any key to exit...");
            Console.ReadKey();
#endif

        }

    }
}
