# Westwind.HtmlPackager

### 0.1.9
* September 6th, 2020

* **Add support for .NET 5.0**  
Fix up project to build for .NET 5.0. Targets are now .NET 4.5+ and .NET 5.0 and later.

* **Fix local file baseUrl fix ups**  
Fix errors that would occur if documents don't have baseUrl values embedded in the header. Added additional logic to deduce base Url from `base` or document path (local files specifically).



### 0.1.6.5
*November 6th, 2018*

* **Add .NET SDK Global Tool and Cross Platform Support**  
You can now install `dotnet install -g dotnet-htmlpackager` and then run `htmlpackager` globally on all supported .NET platforms. 

* **Fix up link Paths**  
Fix up link paths so that the relative paths for links in the document turned into absolute paths.

* **Add Verbose Mode**  
Verbose mode now shows links as they are processed and written out to disk for a more interactive display of progress.

### 0.1.4
*November 1st, 2018*

* **Add display switch to command line processor**  
You can now use the `-d` switch to preview the generated HTML or zip file.

* **Add Zip File Packaging**  
You can now package the generated HTML file into a self-contained zip file using the `-z` switch.

### 0.1.3
*September 8th, 2018*

* **Fix Basepath dereferencing**  
Fixed issue with root base paths (`/`) by properly fixing up the base path URL as a relative URL.


