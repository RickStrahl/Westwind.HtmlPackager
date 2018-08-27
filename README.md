# Westwind.HtmlPackager

A small C# utility class used to package HTML pages into a self contained package. There are two modes:

* **Package to a single file**  
All related resources are downloaded and embedded as into the HTML content. CSS is embedded as text while images, scripts and url resources are embedded as base64 data links. The result is one very large HTML document that contains all resources embedded.

* **HTML file plus loose Resource Files**  
Produces a folder with an HTML file and all CSS, Script, images, fonts as loose file.

The class is small and self contained and can be easily added to another project. Depends on `HtmlAgilityPack`.

### Usage

#### Capture URL to embedded HTML as String
The following captures the HTML to a single file string from a local file on disk:

```cs
var inputFile = Path.Combine(Path.GetTempPath(), "_MarkdownMonster_Preview.html");
var packager = new HtmlPackager();
string packaged = packager.PackageHtml(inputFile);

string outputFile = InputFile.Replace(".html", "_PACKAGED.html");

File.WriteAllText(outputFile, packaged);

Console.WriteLine(packaged);
```

#### Capture File URL to embedded HTML as File

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

#### Capture File Url to HTML File + Loose Resources

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
