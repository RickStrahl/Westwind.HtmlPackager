using System;

namespace HtmlPackager.ConsoleApp
{
    public class HtmlPackagerCommandLineParser : CommandLineParser
    {

        /// <summary>
        /// A Url or local file that contains HTML
        /// </summary>
        public string SourceUrl { get; set; }

        /// <summary>
        /// The target HTML file that output is generated to.
        /// If using the GenerateFolder flag the path of the
        /// the file is used to store the dependencies.
        ///
        /// If TargetFile is missing the output is dumped to
        /// the Standardout
        /// </summary>
        public string TargetFile { get; set; }
        

        /// <summary>
        /// If true causes any document dependencies to be written
        /// into the same folder as the TargetFile. Ignored
        /// if TargetFile is missing
        /// </summary>
        public bool ExternalDependencies { get; set; }

        /// <summary>
        /// If true creates a self contained zip file of the html file plus
        /// all of its dependencies.
        /// </summary>
        public bool ZipDependencies { get; set; }

        /// <summary>
        /// Determines whether verbose messages will be written
        /// as each file is processed.
        /// </summary>
        public bool Verbose { get; set; }

        /// <summary>
        /// When true displays the result HTML in the default
        /// browser.        
        /// </summary>
        public bool DisplayHtml { get; set; }
        
        public HtmlPackagerCommandLineParser(string[] args = null, string cmdLine = null)
            : base(args, cmdLine)
        {
        }

        public override void Parse()
        {
            SourceUrl = Arguments[0];
            ExternalDependencies = ParseParameterSwitch("-x");
            ZipDependencies = ParseParameterSwitch("-z");
            DisplayHtml = ParseParameterSwitch("-d");
            TargetFile = ParseStringParameterSwitch("-o", null);
            Verbose = ParseParameterSwitch("-v");

            if (string.IsNullOrEmpty(TargetFile))            
                ExternalDependencies = false;

            if (SourceUrl.IndexOf("http",StringComparison.InvariantCultureIgnoreCase) == -1  && SourceUrl.Contains("%"))            
                SourceUrl = Environment.ExpandEnvironmentVariables(SourceUrl);

            if (!string.IsNullOrEmpty(TargetFile) && TargetFile.Contains("%"))
                TargetFile = Environment.ExpandEnvironmentVariables(TargetFile);
        }

    }
}
