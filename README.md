## FileInclude

A library to generate a file from template which can include placeholder sections that reference other file (i.e., file includes). The referenced files can include similar sections to reference other files.

The library was motivated by the need to reference other files in README.md file (other MD files, example code files, etc.).

The method **GenerateFileFromTemplate** in interface **FileInclude.ITemplateProcessor** recursively compiles the template file specified in parameter templateFilePath, and all the files directly or indirectly referenced by the template file into a single file and returns the file contents in output parameter **generatedFileContents**. 

The default implementation of **FileInclude.ITemplateProcessor** is **FileInclude.TemplateProcessor** and it uses **IncludedFilePlaceHolder** element tag to locate the include place-holders. However the tag name can be changed by overriding the virtual property TemplateProcessor.FileIncludeTagName.

The method **GenerateFileFromTemplate** returns errors as a list of **FileInclude.IErrorData** objects that contain error details.

Also, the method checks and reports any possible circular references, to prevent infinite processing.

There is also an extension method **FileInclude.TemplateProcessorExtensions.GenerateFileFromTemplateAndSave** which saves the file generated from the template. The extension method checks if the generated file was modified by other applications (say manually edited) since it was auto-generated last time, to prevent data loss.

## Examples

- README.md file contents

```text

This is README.md file that references two other md files, and am xml file.

Feature2ReadMe.md is specified using a relative path. It is expected to be in folder Feature1 within the same folder where README.md is.
<IncludedFilePlaceHolder>Feature1\Feature1ReadMe.md</IncludedFilePlaceHolder>

Feature1Example.xml is specified using a relative path in relation to folder where README.md. It is expected to be in folder Examples two levels up from folder where README.md is.
<IncludedFilePlaceHolder>..\..\Examples\Feature1Example.xml</IncludedFilePlaceHolder>

**Feature2ReadMe.md** is specified using an absolute path.
<IncludedFilePlaceHolder>c:\Feature2\Feature2ReadMe.md</IncludedFilePlaceHolder>

```

- Feature1ReadMe.md file content s

```text
This is a demo of Feature1ReadMe.md that is referenced from README.md and it references "..\Feature3\README.md".

- Summary for Feature 1
...

The contents of file "..\Feature3\README.md" will replace the placeholder below.
<IncludedFilePlaceHolder>..\Feature3\README.md</IncludedFilePlaceHolder>

```
