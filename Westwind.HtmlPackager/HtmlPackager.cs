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
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Westwind.HtmlPackager.Utilities;


namespace Westwind.HtmlPackager
{

    /// <summary>
    /// A utility class that can package HTML and all of its dependencies
    /// into either a single file with embedded text and binary resources,
    /// or into a self contained folder that holds the HTML plus its
    /// external dependencies. 
    /// </summary>
    public class HtmlPackager
    {
        /// <summary>
        /// A Url or File to load for packaging
        /// </summary>
        public string SourceUrlOrFile { get; set; }


        /// <summary>
        /// The output path where the result HTML file is to be created.
        /// If creating external depedendencies, the dependencies are dumped
        /// into the same folder
        /// </summary>
        public string OutputPath { get; set; }

    
        /// <summary>
        /// Internal flag to determine if files are 
        /// </summary>
        bool CreateExternalFiles { get; set; }

        /// <summary>
        /// Internally tracked base URI for the file or URL so
        /// resources can be discovered and found.
        /// </summary>
        Uri BaseUri { get; set; }


        /// <summary>
        /// Internal naming counter
        /// </summary>
        int ctr = 0;

        

        #region Main API

        ///  <summary>
        ///  Packages an HTML document into a large single file package
        ///  that embeds all images, css, scripts, fonts and other url()
        ///  loaded entries into the HTML document.
        /// 
        ///  The result is a very large document that is fully self-contained        
        ///  </summary>
        ///  <param name="urlOrFile">A Web Url or fully qualified local file name</param>
        ///  <param name="basePath">
        ///  An optional basePath for the document which helps resolve relative
        ///  paths. Unless there's a special use case, you should leave this
        ///  value blank and let the default use either the value from a
        ///  BASE tag or the base location of the document.
        /// 
        ///  If the document itself contains a BASE tag this value is not used.
        ///  </param>
        /// <param name="createExternalFiles"></param>
        /// <returns>HTML string or null</returns>
        public string PackageHtml(string urlOrFile, string basePath = null, bool createExternalFiles = false)
        {            
            if (string.IsNullOrEmpty(urlOrFile))
                return urlOrFile;

            SourceUrlOrFile = urlOrFile;

            CreateExternalFiles = createExternalFiles;

            HtmlDocument doc;
            var tempFile = Path.GetTempFileName();
            if (urlOrFile.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) && urlOrFile.Contains("://"))
            {
                BaseUri = new Uri(SourceUrlOrFile);

                HtmlWeb web = null;
                try
                {
                    OnMessage("Loading source url: " + urlOrFile, MessageModes.Information);
                    web = new HtmlWeb();
                    doc = web.Load(urlOrFile);
                }
                catch (Exception ex)
                {
                    SetError($"Error loading Url: urlOrFile: {ex.Message}");
                    return null;
                }

                var docBase = doc.DocumentNode.SelectSingleNode("//base");
                if (docBase != null)
                {
                    basePath = docBase.Attributes["href"]?.Value;
                    BaseUri = new Uri(baseUri: new Uri(urlOrFile),relativeUri: basePath);
                }
                docBase?.Remove();

                ctr = 0;
                ProcessCss(doc);
                ProcessScripts(doc);
                ProcessImages(doc);
                ProcessLinks(doc);
            }
            else
            {

                try
                {
                    OnMessage("Loading source url: " + urlOrFile, MessageModes.Information);
                    doc = new HtmlDocument();
                    doc.Load(urlOrFile);
                    
                }
                catch (Exception ex)
                {
                    SetError($"Error loading HTML file: {ex.Message}");
                    return null;
                }

                var docBase = doc.DocumentNode.SelectSingleNode("//base");
                if (docBase != null)
                {
                    var url = docBase.Attributes["href"]?.Value;
                    if (url.StartsWith("file:///"))
                    {
                        var tBasePath = url.Replace("file:///", "");
                        if (!string.IsNullOrEmpty(tBasePath) && tBasePath != "\\" && tBasePath != "/")
                            basePath = tBasePath;
                    }
                }
                docBase?.Remove();

                string oldPath = Environment.CurrentDirectory;
                try
                {
                    if (string.IsNullOrEmpty(basePath))
                        basePath = Path.GetDirectoryName(urlOrFile) + "\\";

                    Directory.SetCurrentDirectory(basePath);
                    BaseUri = new Uri( basePath);

                    ctr = 0;
                    ProcessCss(doc);
                    ProcessScripts(doc);
                    ProcessImages(doc);
                    ProcessLinks(doc);
                }
                finally
                {
                    Directory.SetCurrentDirectory(oldPath);
                }
            }

            var html = doc.DocumentNode.InnerHtml;            

            if (tempFile != null)
                File.Delete(tempFile);

            return html;
        }

        /// <summary>
        /// Packages an HTML document into a large single file package
        /// that embeds all images, css, scripts, fonts and other url()
        /// loaded entries into the HTML document.
        ///
        /// The result is a very large document that is fully self-contained        
        /// </summary>
        /// <param name="urlOrFile">A Web Url or fully qualified local file name</param>
        /// <param name="basePath">
        /// An optional basePath for the document which helps resolve relative
        /// paths. Unless there's a special use case, you should leave this
        /// value blank and let the default use either the value from a
        /// BASE tag or the base location of the document.
        ///
        /// If the document itself contains a BASE tag this value is not used.
        /// </param>
        /// <returns>HTML string or null</returns>
        public bool PackageHtmlToFile(string urlOrFile, string outputFile, string basePath = null, bool createExternalFiles = false)
        {
            var html = PackageHtml(urlOrFile, basePath, createExternalFiles);
            if (string.IsNullOrEmpty(html))
                return false;

            try
            {
                var dir = Path.GetDirectoryName(outputFile);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllText(outputFile, html);
            }
            catch(Exception ex)
            {
                SetError($"Error writing out HTML file: {ex.Message}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Packages an HTML document into a file with all dependencies
        /// dumped into the file's output folder and adjusted for the
        /// same local path.       
        /// </summary>
        /// <param name="urlOrFile">A Web Url or fully qualified local file name</param>
        /// <param name="outputFile">Location for the output file. Folder is created if it doesn't exist. All dependencies are dumped into this folder</param>
        /// <param name="basePath">
        /// An optional basePath for the document which helps resolve relative
        /// paths. Unless there's a special use case, you should leave this
        /// value blank and let the default use either the value from a
        /// BASE tag or the base location of the document.
        ///
        /// If the document itself contains a BASE tag this value is not used.
        /// </param>
        /// <param name="deleteFolderContents">If true deletes folder contents first</param>
        /// <returns>HTML string or null</returns>
        public bool PackageHtmlToFolder(string urlOrFile, string outputFile, string basePath = null, bool deleteFolderContents = false)
        {
            try
            {
                OutputPath = Path.GetDirectoryName(outputFile);

                if (deleteFolderContents && Directory.Exists(OutputPath))
                {
                    foreach (var file in Directory.GetFiles(OutputPath))
                        File.Delete(file);
                }

                if (!Directory.Exists(OutputPath))
                    Directory.CreateDirectory(OutputPath);


                return PackageHtmlToFile(urlOrFile, outputFile, basePath, true);
            }
            catch (Exception ex)
            {
                SetError("An error occurred: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Packages HTML files to a zip file.
        /// </summary>
        /// <param name="urlOrFile"></param>
        /// <param name="outputZipFile"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        public bool PackageHtmlToZipFile(string urlOrFile, string outputZipFile, string basePath = null)
        {
            if (File.Exists(outputZipFile))
                File.Delete(outputZipFile);

            var folder = Path.Combine(Path.GetTempPath(), "_" + Utils.GenerateUniqueId());
            var htmlFile = Path.Combine(folder,Path.GetFileName(Path.ChangeExtension(outputZipFile, "html")));

            Directory.CreateDirectory(folder);

            if (!PackageHtmlToFolder(urlOrFile, htmlFile, basePath))
                return false;

            try
            {
                ZipFile.CreateFromDirectory(folder, outputZipFile, CompressionLevel.Fastest, false);
            }
            catch (Exception ex)
            {
                SetError("Unable to package output files: " + ex.Message);
                return false;
            }

            Directory.Delete(folder, true);

            return true;
        }

        #endregion

        #region Processors

        private void ProcessCss(HtmlDocument doc)
        {
            var links = doc.DocumentNode.SelectNodes("//link");
            if (links == null)
                return;
           
            foreach (var link in links)
            {
                var url = link.Attributes["href"]?.Value;
                var rel = link.Attributes["rel"]?.Value;

                if (rel != "stylesheet")
                    continue;
               
                if (url == null)
                    continue;

                string cssText;

                var originalUrl = url;
                Uri origUri = null;

                if (url.StartsWith("http"))
                {
                    origUri = new Uri(url);
                    OnMessage(" ==> loading css: " + origUri.AbsolutePath);

                    using (var http = new WebClient())
                    {
                        cssText = http.DownloadString(url);
                    }
                }
                else if (url.StartsWith("file:///"))
                {
                    origUri = new Uri(url);
                    OnMessage(" ==> loading css: " + origUri.AbsolutePath);
                    cssText = File.ReadAllText(WebUtility.UrlDecode(origUri.LocalPath));                   
                }
                else // Relative Path
                {
                    origUri = new Uri(BaseUri, url);
                    url = origUri.AbsoluteUri;

                    OnMessage(" ==> loading css: " + url.ToString());

                    if (url.StartsWith("http") && url.Contains("://"))
                    {
                        try{ 
                        using (var http = new WebClient())
                        {
                            cssText = http.DownloadString(url);
                        }
                        }
                        catch (Exception ex)
                        {
                            OnMessage(" ==> Error: retrieving css from " + url + "\n" +
                                      "            " + ex.Message, MessageModes.Error);
                            continue;
                        }
                    }
                    else
                    {
                        cssText = File.ReadAllText(WebUtility.UrlDecode(origUri.LocalPath));
                    }
                }

                cssText = ProcessEmbeddedUrls(cssText, url);

                if (CreateExternalFiles)
                {
                    var justFilename = Path.GetFileName(origUri.AbsolutePath);
                    string justExt = Path.GetExtension(origUri.AbsolutePath);
                    if (string.IsNullOrEmpty(justExt))
                        justFilename = Utils.GenerateUniqueId(10) + ".css";
                    
                    var fullPath = Path.Combine(OutputPath, justFilename);
                    File.WriteAllText(fullPath, cssText);
                    link.Attributes["href"].Value = justFilename;

                    OnMessage(" ==> writing css: " + url,MessageModes.Information2);
                }
                else
                {
                    var el = new HtmlNode(HtmlNodeType.Element, doc, ctr++);
                    el.Name = "style";
                    el.InnerHtml = Environment.NewLine + cssText + Environment.NewLine;

                    link.ParentNode.InsertAfter(el, link);
                    link.Remove();
                    el = null;
                }                
            }            
        }

        private void ProcessScripts(HtmlDocument doc)
        {
            var scripts = doc.DocumentNode.SelectNodes("//script");
            if (scripts == null || scripts.Count < 1)
                return;


            foreach (var script in scripts)
            {
                var url = script.Attributes["src"]?.Value;
                if (url == null)
                    continue;

                Uri origUri = null;
                
                byte[] scriptData;
                if (url.StartsWith("http"))
                {
                    origUri = new Uri(url);
                    OnMessage(" ==> loading script: " + origUri.AbsolutePath);

                    try
                    {
                        using (var http = new WebClient())
                        {
                            scriptData = http.DownloadData(url);
                        }
                    }
                    catch(Exception ex)
                    {
                        OnMessage(" ==> Error: retrieving script from " + url + "\n" +
                                  "            " + ex.Message, MessageModes.Error );
                        continue;
                    }
                }
                else if (url.StartsWith("file:///"))
                {
                    origUri = new Uri(url);
                    OnMessage(" ==> loading script: " + origUri.AbsolutePath);

                    url = WebUtility.UrlDecode(origUri.LocalPath);
                    scriptData = File.ReadAllBytes(url);                    ;
                }
                else // Relative Path
                {
                    try
                    {
                        origUri = new Uri(BaseUri, url);
                        url = origUri.AbsoluteUri;

                        OnMessage(" ==> loading script: " + origUri.AbsolutePath);

                        if (url.StartsWith("http://") || url.StartsWith("https://"))
                        {
                            using (var http = new WebClient())
                            {
                                scriptData = http.DownloadData(url);
                            }
                        }
                        else
                            scriptData = File.ReadAllBytes(WebUtility.UrlDecode(origUri.LocalPath));
                    }
                    catch
                    {
                        continue;
                    }
                }
                
                if (CreateExternalFiles)
                {
                    var justFilename = Path.GetFileName(origUri.AbsolutePath);
                    string justExt = Path.GetExtension(origUri.AbsolutePath);
                    if (string.IsNullOrEmpty(justExt))
                        justFilename = Utils.GenerateUniqueId(10) + ".js";

                    var fullPath = Path.Combine(OutputPath,justFilename);
                    File.WriteAllBytes(fullPath,scriptData);
                    script.Attributes["src"].Value = justFilename;
                    OnMessage(" ==> writing script: " + fullPath,MessageModes.Information2);
                }
                else
                {
                    string data = $"data:text/javascript;base64,{Convert.ToBase64String(scriptData)}";
                    script.Attributes["src"].Value = data;
                }
            }
        }

        private void ProcessImages(HtmlDocument doc)
        {
            var images = doc.DocumentNode.SelectNodes("//img");
            if (images == null || images.Count < 1)
                return;

            // special case for favicons
            var favIconNodes = doc.DocumentNode.SelectNodes("//link");
            if (favIconNodes != null)
            {
                foreach (var favIconNode in favIconNodes)
                {
                    var url = favIconNode.Attributes["href"]?.Value;
                    var rel = favIconNode.Attributes["rel"]?.Value;
                    if (url != null && rel.Contains("icon"))
                    {
                        var node = HtmlNode.CreateNode($"<img src='{url}' />");
                        images.Add(node);
                    }
                }
            }

            foreach (var image in images)
            {
                var url = image.Attributes["src"]?.Value;
                if (url == null)
                    continue;
                
                byte[] imageData;
                string contentType;
                Uri origUri = null;

                if (url.StartsWith("http"))
                {
                    origUri = new Uri(url);

                    OnMessage(" ==> loading image: " + origUri.AbsolutePath);

                    try
                    {
                        using (var http = new WebClient())
                        {
                            imageData = http.DownloadData(url);
                            contentType = http.ResponseHeaders[HttpResponseHeader.ContentType];
                        }
                    }
                    catch (Exception ex)
                    {
                        OnMessage(" ==> Error: retrieving image from " + url + "\n" +
                                  "            " + ex.Message, MessageModes.Error);
                        continue;
                    }
                }
                else if(url.StartsWith("file:///"))
                {
                    origUri = new Uri(url);
                    OnMessage(" ==> loading image: " + origUri.AbsolutePath);
                    url = WebUtility.UrlDecode(origUri.LocalPath);

                    try
                    {
                        imageData = File.ReadAllBytes(url);
                        contentType = Utils.GetImageMediaTypeFromFilename(url);
                    }
                    catch
                    {
                        continue;
                    }
                }
                else // Relative Path
                {
                    try
                    {
                        origUri = new Uri(BaseUri, url);
                        url = origUri.AbsoluteUri;
                        OnMessage(" ==> loading image: " + url);

                        if (url.StartsWith("http://") || url.StartsWith("https://") )
                        {
                            using (var http = new WebClient())
                            {
                                imageData = http.DownloadData(url);
                            }
                        }
                        else
                            imageData = File.ReadAllBytes(WebUtility.UrlDecode(origUri.LocalPath)); 

                        contentType = Utils.GetImageMediaTypeFromFilename(url);
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (imageData == null)
                    continue;


                var el = image;
                el.Name = "img";

                if (CreateExternalFiles)
                {
                    var ext = "png";
                    if (contentType == "image/jpeg")
                        ext = "jpg";
                    else if (contentType == "image/gif")
                        ext = "gif";


                    string justFilename = Path.GetFileName(origUri.LocalPath);
                    string justExt = Path.GetExtension(url);
                    if (string.IsNullOrEmpty(justExt))                         
                        justFilename = Utils.GenerateUniqueId(10) + "." + ext;

                    var fullPath = Path.Combine(OutputPath, justFilename);
                    File.WriteAllBytes(fullPath, imageData);
                    OnMessage(" ==> writing image: " + fullPath,MessageModes.Information2);
                    el.Attributes["src"].Value = justFilename;
                }
                else
                {
                    string data = $"data:{contentType};base64,{Convert.ToBase64String(imageData)}";
                    el.Attributes["src"].Value = data;
                }              
            }
        }

        private void ProcessLinks(HtmlDocument doc)
        {
            var links = doc.DocumentNode.SelectNodes("//a");
            if (links == null || links.Count < 1)
                return;


            foreach (var link in links)
            {
                var url = link.Attributes["href"]?.Value;
                
                if (string.IsNullOrEmpty(url) ||
                    url.StartsWith("http", comparisonType: StringComparison.InvariantCultureIgnoreCase) ||
                    url.StartsWith("#") ||
                    url.IndexOf("javascript:", StringComparison.InvariantCultureIgnoreCase) > -1)
                    continue;
                
                OnMessage("==> Fixing up link: " + url);

                try
                {
                    var uri = new Uri(BaseUri, url);                    
                    link.Name = "a";
                    var linkUrl = uri.ToString();
                    link.Attributes["href"].Value = linkUrl;

                    OnMessage("==> Fixed up with: " + uri,MessageModes.Information2);
                }
                catch
                {
                }
            }
        }


        private static Regex urlRegEx = new Regex("url\\(.*?\\)", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        /// <summary>
        /// Processes embedded url('link') links and embeds the data
        /// and returns the expanded HTML string either with embedded
        /// content, or externalized links.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="baseUrl"></param>
        /// <returns></returns>
        private string ProcessEmbeddedUrls(string html, string baseUrl)
        {
            var matches = urlRegEx.Matches(html);
            string contentType = null;
            byte[] linkData = null;

            foreach (Match match in matches)
            {
                string matched = match.Value;
                if (string.IsNullOrEmpty(matched))
                    continue;

                var url = Utils.ExtractString(matched,"(", ")")?.Trim(new char[] {'\'', '\"'}).Replace("&amp;","").Replace("quot;","");
                if (url.Contains("?"))
                    url = Utils.ExtractString(url, "", "?");

                // skip rarely used font files for size
                if (url.EndsWith(".eot") || url.EndsWith(".ttf"))
                    continue;

                OnMessage(" ==> loading embedded: " + url);
                
                if (url.StartsWith("http"))
                {
                    using (var http = new WebClient())
                    {
                        linkData = http.DownloadData(url);
                        contentType = http.ResponseHeaders[HttpResponseHeader.ContentType];
                    }
                }
                else if(url.StartsWith("file:///"))
                {
                    var baseUri = new Uri(baseUrl);
                    url = new Uri(baseUri, new Uri(url)).AbsoluteUri;
                    
                    try
                    {         
                        contentType = Utils.GetImageMediaTypeFromFilename(url);                        
                        if (contentType == "application/image")
                            continue;

                         linkData = File.ReadAllBytes(WebUtility.UrlDecode(url));                     
                    }
                    catch
                    {
                        continue;
                    }
                }
                else
                {
                    try
                    {
                        var baseUri = new Uri(baseUrl);
                        var uri = new Uri(baseUri, url);
                        url = uri.AbsoluteUri;
                        if (url.StartsWith("http") && url.Contains("://"))
                        {
                            using (var http = new WebClient())
                            {
                                linkData = http.DownloadData(url);
                            }
                        }
                        else
                            linkData = File.ReadAllBytes(WebUtility.UrlDecode(url.Replace("file:///","")));

                        contentType = Utils.GetImageMediaTypeFromFilename(url);
                    }
                    catch
                    {
                        continue;
                    }
                }

                if (linkData == null)
                    continue;

                string urlContent = null;
                if (CreateExternalFiles)
                {
                    var ext = "png";
                    if (contentType == "image/jpeg")
                        ext = "jpg";
                    else if (contentType == "image/gif")
                        ext = "gif";

                    string justFilename = Path.GetFileName(url);
                    string justExt = Path.GetExtension(url);
                    if (string.IsNullOrEmpty(justExt))
                        justFilename = Utils.GenerateUniqueId(10) + "." + ext;
                    urlContent = "url('" + justFilename + "')";

                    var outputFile = Path.Combine(OutputPath, justFilename);
                    File.WriteAllBytes(outputFile,linkData);

                    OnMessage(" ==> embedded file: " + outputFile, MessageModes.Information2);
                }
                else
                {
                    string data = $"data:{contentType};base64,{Convert.ToBase64String(linkData)}";
                    urlContent = "url('" + data + "')";
                }

                html = html.Replace(matched, urlContent);                
            }

            return html;
        }
        #endregion


        /// <summary>
        /// Event that can be intercepted to write out messages
        /// while the url is being processed
        /// </summary>
        public event Action<string, MessageModes> WriteMessage;

        /// <summary>
        /// Used internally to write messages
        /// </summary>
        /// <param name="message"></param>
        /// <param name="mode"></param>
        public void OnMessage(string message, MessageModes mode = MessageModes.Information)
        {
            if (WriteMessage != null)
                WriteMessage(message, mode);
        }

        #region Error Handler

        public string ErrorMessage { get; set; }

        protected void SetError()
        {
            SetError("CLEAR");
        }

        protected void SetError(string message)
        {
            if (message == null || message == "CLEAR")
            {
                ErrorMessage = string.Empty;
                return;
            }
            ErrorMessage += message;

            OnMessage("Error: " + message,MessageModes.Error);
        }

        protected void SetError(Exception ex, bool checkInner = false)
        {
            if (ex == null)
                ErrorMessage = string.Empty;

            Exception e = ex;
            if (checkInner)
                e = e.GetBaseException();

            ErrorMessage = e.Message;
            OnMessage("Error: " + ErrorMessage, MessageModes.Error);
        }
        #endregion
    }

    public enum MessageModes
    {
        Information,
        Information2,
        Error,
        Warning
    }
}
