using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace Westwind.HtmlPackager.Test
{

    /// <summary>
    /// To run these tests make sure an image is on the clipboard
    /// </summary>
    [TestClass]
    public class HtmlPackagerTests
    {
        private string InputFile;

        public HtmlPackagerTests()
        {
            // specify a test file here - I'm using output from Markdown Monster so i can
            // easily open/generate different files to test output against
            InputFile = Path.Combine(Path.GetTempPath(), "_MarkdownMonster_Preview.html");
                        
        }

        [TestMethod]
        public void PackageFromFileTest()
        {
            var packager = new HtmlPackager();
            string packaged = packager.PackageHtml(InputFile);

            string outputFile = InputFile.Replace(".html", "_PACKAGED.html");
            File.WriteAllText(outputFile, packaged);
            
            Console.WriteLine(packaged);

            Assert.IsNotNull(packaged);

#if !NETCORE
            ShellUtils.GoUrl(outputFile);
#endif

        }

        [TestMethod]
        public void PackageFromWebTest()
        {
            var packager = new HtmlPackager();
            string packaged = packager.PackageHtml("https://weblog.west-wind.com");

            string outputFile = InputFile.Replace(".html", "_PACKAGED.html");
            File.WriteAllText(outputFile, packaged);

            Console.WriteLine(packaged);

            Assert.IsNotNull(packaged);

#if !NETCORE
            ShellUtils.GoUrl(outputFile);
#endif
        }

        [TestMethod]
        public void PackageLooseFilesLocalTest()
        {
            var packager = new HtmlPackager();
            string outputFile = @"c:\temp\GeneratedHtml\Output.html";
            bool result = packager.PackageHtmlToFolder(@"c:\temp\tmpFiles\_MarkdownMonster_Preview.html", outputFile,
                null, true);
            Assert.IsTrue(result);

#if !NETCORE
            ShellUtils.GoUrl(outputFile);
#endif


        }

        [TestMethod]
        public void PackageLooseFilesWebUrlTest()
        {
            var packager = new HtmlPackager();
            string outputFile = @"c:\temp\GeneratedHtml\Output.html";
            bool result = packager.PackageHtmlToFolder("http://west-wind.com/", outputFile, null, true);
            Assert.IsTrue(result);

            ShellUtils.GoUrl(outputFile);



        }
    }
}
