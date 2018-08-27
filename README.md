# Westwind.HtmlPackager

A small C# utility class used to package HTML pages into a self contained package. 

The packager works with:

* Web Urls
* Local HTML Files

There are two packaging modes:

* **Package to a single file**  
All related resources are downloaded and embedded as into the HTML content. CSS is embedded as text while images, scripts and url resources are embedded as base64 data links. The result is one very large HTML document that contains all resources embedded.

* **HTML file plus loose Resource Files**  
Produces a folder with an HTML file and all CSS, Script, images, fonts as loose file.

The library is small and self contained and can be easily added to another project with two source files, or you can use **NuGet** to include it. Depends on [HtmlAgilityPack](http://html-agility-pack.net/) for HTML parsing.

### Installation via NuGet
You can install this library from NuGet:

```
install-package Westwind.HtmlPackager
```

or 

```
c:> dotnet add package Westwind.HtmlPackager
```

## Usage
The following are various usage examples for packaging HTML from files or Web URLs into the two package formats.

### Capture HTML File to embedded HTML as String
The following captures the HTML to a single file string from a local file on disk:

```cs
var inputFile = Path.Combine(Path.GetTempPath(), "_MarkdownMonster_Preview.html");
var packager = new HtmlPackager();
string packaged = packager.PackageHtml(inputFile);

string outputFile = InputFile.Replace(".html", "_PACKAGED.html");

File.WriteAllText(outputFile, packaged);

Console.WriteLine(packaged);
```

### Capture HTML File to embedded HTML as File

```cs
var inputFile = Path.Combine(Path.GetTempPath(), "_MarkdownMonster_Preview.html");
string outputFile = InputFile.Replace(".html", "_PACKAGED.html");

var packager = new HtmlPackager();
string packaged = packager.PackageHtmlToFile(inputFile,outputFile);


// display html in browser
Utils.GoUrl(outputFile);
```

#### Capture Web Url to single File
```cs
var packager = new HtmlPackager();
string packaged = packager.PackageHtml("https://west-wind.com");

string outputFile = InputFile.Replace(".html", "_PACKAGED.html");
File.WriteAllText(outputFile, packaged);

ShellUtils.GoUrl(outputFile);
```

#### Capture File to HTML File + Loose Resources

```cs
var packager = new HtmlPackager();
string outputFile = @"c:\temp\GeneratedHtml\Output.html";
bool result = packager.PackageHtmlToFolder(@"c:\temp\tmpFiles\_MarkdownMonster_Preview.html", outputFile,
    null, true);
Assert.IsTrue(result);

// Display html in browser
Utils.GoUrl(outputFile);
```

#### Capture Web Url to HTML File + Loose Resources

```cs
var packager = new HtmlPackager();
string outputFile = @"c:\temp\GeneratedHtml\Output.html";
bool result = packager.PackageHtmlToFolder("http://west-wind.com/", outputFile, null, true);

ShellUtils.GoUrl(outputFile);
```

## License
The Westwind.HtmlPackager library is licensed under the [MIT License](https://opensource.org/licenses/MIT) and there's no charge to use, integrate or modify the code for this project. You are free to use it in personal, commercial, government and any other type of application.

All source code is copyright West Wind Technologies, regardless of changes made to them. Any source code modifications must leave the original copyright code headers intact.

## Warranty Disclaimer: No Warranty!
IN NO EVENT SHALL THE AUTHOR, OR ANY OTHER PARTY WHO MAY MODIFY AND/OR REDISTRIBUTE THIS PROGRAM AND DOCUMENTATION, BE LIABLE FOR ANY COMMERCIAL, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES ARISING OUT OF THE USE OR INABILITY TO USE THE PROGRAM INCLUDING, BUT NOT LIMITED TO, LOSS OF DATA OR DATA BEING RENDERED INACCURATE OR LOSSES SUSTAINED BY YOU OR LOSSES SUSTAINED BY THIRD PARTIES OR A FAILURE OF THE PROGRAM TO OPERATE WITH ANY OTHER PROGRAMS, EVEN IF YOU OR OTHER PARTIES HAVE BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.