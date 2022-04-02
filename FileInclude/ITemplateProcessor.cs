// This software is part of the ExtendibleTreeStructure library
// Copyright © 2018 ExtendibleTreeStructure Contributors
// http://oroptimizer.com
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System.Text;
using OROptimizer.Utilities;

namespace FileInclude
{
    /// <summary>
    /// Generates a file from template which can include placeholder sections that reference other file (i.e., file includes).
    /// The referenced files can include similar sections to reference other files
    /// The library was motivated by the need to reference other files in README.md file (other MD files, example code files, etc.). 
    /// The method <see cref="GenerateFileFromTemplate"/> recursively compiles the template file specified in parameter templateFilePath, and all the files directly or indirectly referenced by the template file into a single file and returns the file contents in output parameter generatedFileContents. 
    /// The default implementation is <see cref="TemplateProcessor"/>  and it uses 'IncludedFilePlaceHolder' element tag to locate the include place-holders. However the tag name can be changed by overriding the virtual property <see cref="TemplateProcessor.FileIncludeTagName"/>.
    /// The method <see cref="GenerateFileFromTemplate"/> returns errors as a list of <see cref="IErrorData"/> objects that contain error details.
    /// Also, the method checks and reports any possible circular references, to prevent infinite processing.
    /// Example of file includes in MD files:
    /// ## MyCoolLib Summary
    /// &lt;IncludedFilePlaceHolder&gt;SomeFeature1\README.md&lt;/IncludedFilePlaceHolder&gt;
    /// &lt;IncludedFilePlaceHolder&gt;c:\SomeFeature2\FEATURE2README.md&lt;/IncludedFilePlaceHolder&gt;
    /// 
    /// ```xml
    /// &lt;IncludedFilePlaceHolder&gt;..\..\Example.xml&lt;/IncludedFilePlaceHolder&gt;
    /// ```
    /// </summary>
    public interface ITemplateProcessor
    {
        /// <summary>
        /// Generates a file from template which can include placeholder sections that reference other file (i.e., file includes).
        /// The referenced files can include similar sections to reference other files
        /// The method recursively compiles the template file specified in parameter <paramref name="templateFilePath"/>, and all the files directly or indirectly referenced by the template file into a single file and returns the file contents in output parameter <paramref name="generatedFileContents"/>. 
        /// The default implementation is <see cref="TemplateProcessor.GenerateFileFromTemplate(string, out string)"/>  and it uses 'IncludedFilePlaceHolder' element tag to locate the include place-holders. However the tag name can be changed by overriding the virtual property <see cref="TemplateProcessor.FileIncludeTagName"/>.
        /// The method returns errors as a list of <see cref="IErrorData"/> objects that contain error details.
        /// Also, the method checks and reports any possible circular references, to prevent infinite processing.
        /// Example of file includes in MD files:
        /// ## MyCoolLib Summary
        /// &lt;IncludedFilePlaceHolder&gt;SomeFeature1\README.md&lt;/IncludedFilePlaceHolder&gt;
        /// &lt;IncludedFilePlaceHolder&gt;c:\SomeFeature2\FEATURE2README.md&lt;/IncludedFilePlaceHolder&gt;
        /// 
        /// ```xml
        /// &lt;IncludedFilePlaceHolder&gt;..\..\Example.xml&lt;/IncludedFilePlaceHolder&gt;
        /// ```
        /// </summary>
        /// <param name="templateFilePath">The template file absolute path.</param>
        /// <param name="generatedFileContents">The generated file contents.</param>
        /// <returns>Returns list of errors if any.</returns>
        IReadOnlyList<IErrorData> GenerateFileFromTemplate(string templateFilePath, out string generatedFileContents);
    }

    /// <summary>
    /// Extensions for <see cref="ITemplateProcessor"/>
    /// </summary>
    public static class TemplateProcessorExtensions
    {
        /// <summary>
        /// Extension method for <see cref="ITemplateProcessor"/> that generates a file from template specified in <paramref name="templateFilePath"/> using
        /// <see cref="ITemplateProcessor.GenerateFileFromTemplate(string, out string)"/> and saves the generated file to file specified in parameter <paramref name="generatedFilePath"/>. 
        /// Example of file includes in MD files:
        /// ## MyCoolLib Summary
        /// &lt;IncludedFilePlaceHolder&gt;SomeFeature1\README.md&lt;/IncludedFilePlaceHolder&gt;
        /// &lt;IncludedFilePlaceHolder&gt;c:\SomeFeature2\FEATURE2README.md&lt;/IncludedFilePlaceHolder&gt;
        /// 
        /// ```xml
        /// &lt;IncludedFilePlaceHolder&gt;..\..\Example.xml&lt;/IncludedFilePlaceHolder&gt;
        /// ```
        /// </summary>
        /// <param name="templateProcessor">An instance of <see cref="ITemplateProcessor"/>.</param>
        /// <param name="templateFilePath">An absolute file path of the template file.</param>
        /// <param name="generatedFilePath">Generated file absolute path, or path relative to directory of template file <paramref name="templateFilePath"/>.
        /// Examples are "SomeFeature1\README.md", "..\SomeFeature1\README.md", "c:\MyCoolLib\README.md".
        /// </param>
        /// <returns>Returns list of errors if any.</returns>
        public static IReadOnlyList<IErrorData> GenerateFileFromTemplateAndSave(this ITemplateProcessor templateProcessor, string templateFilePath, string generatedFilePath)
        {
            var errors = new List<IErrorData>();

            var templateFileFolderPath = Path.GetDirectoryName(templateFilePath)!;

            var absoluteFilePathData = FilePathHelpers.TryGetAbsoluteFilePath(templateFileFolderPath, generatedFilePath);

            if (!absoluteFilePathData.isSuccess)
            {
                if (absoluteFilePathData.errorMessage == null)
                    absoluteFilePathData.errorMessage = $"Could not calculate absolute file path from parameters '{nameof(templateFilePath)}' and '{nameof(generatedFilePath)}' passed to method '{typeof(TemplateProcessorExtensions).FullName}.{nameof(GenerateFileFromTemplateAndSave)}'.";

                errors.Add(new ErrorData(ErrorCode.CouldNotCalculateAbsoluteFilePath, absoluteFilePathData.errorMessage, generatedFilePath));
                return errors;
            }

            try
            {
                if (Directory.Exists(absoluteFilePathData.absoluteFilePath))
                {
                    errors.Add(new ErrorData(ErrorCode.CouldNotCalculateAbsoluteFilePath,
                        $"Absolute file path calculated from parameters '{nameof(templateFilePath)}' and '{nameof(generatedFilePath)}' passed to method '{typeof(TemplateProcessorExtensions).FullName}.{nameof(GenerateFileFromTemplateAndSave)}' is a directory. Expected a file.", generatedFilePath));
                    return errors;
                }
            }
            catch (Exception e)
            {
                errors.Add(new ErrorData(ErrorCode.CouldNotCalculateAbsoluteFilePath, e, templateFilePath));
                return errors;
            }

            if (absoluteFilePathData.absoluteFilePath == null)
            {
                errors.Add(new ErrorData(ErrorCode.FileDoesNotExist, "Implementation error.", generatedFilePath));
                return errors;
            }

            if (string.Equals(templateFilePath, absoluteFilePathData.absoluteFilePath, StringComparison.InvariantCultureIgnoreCase))
            {
                errors.Add(new ErrorData(ErrorCode.TemplateFilePathIsTheSameAsGeneratedFilePath,
                    $"The template file path in parameter '{nameof(templateFilePath)}' of method '{typeof(TemplateProcessorExtensions).FullName}.{nameof(GenerateFileFromTemplateAndSave)}' matches the path of the file to be generated from the template. Make sure the values of parameters '{nameof(templateFilePath)}' and '{nameof(generatedFilePath)}' are correct.",
                    generatedFilePath));
                return errors;
            }

            string fileModificationDataFilePath = $"{absoluteFilePathData.absoluteFilePath}.generationdata";

            const string lastModifiedDateParamName = "LastModifiedDate";

            try
            {
                if (File.Exists(absoluteFilePathData.absoluteFilePath))
                {
                    string GetFileWasModifiedErrorMessage() =>
                        $"File '{absoluteFilePathData.absoluteFilePath}' was modified by other application. Please backup the file, then rename or delete it and try again.";

                    if (!File.Exists(fileModificationDataFilePath))
                    {
                        errors.Add(new ErrorData(ErrorCode.FileGeneratedFromTemplateWasModifiedAfterLastGeneration,
                            GetFileWasModifiedErrorMessage(), absoluteFilePathData.absoluteFilePath));
                        return errors;
                    }

                    using (var streamReader = new StreamReader(fileModificationDataFilePath))
                    {
                        var fileModificationData = streamReader.ReadToEnd();

                        long? lastGenerationDate = null;

                        var index = fileModificationData.IndexOf(lastModifiedDateParamName, StringComparison.Ordinal);

                        if (index >= 0)
                        {
                            index = fileModificationData.IndexOf(":", index + lastModifiedDateParamName.Length, StringComparison.Ordinal);
                            if (index >= 0)
                            {
                                if (long.TryParse(fileModificationData.Substring(index + 1).Trim(), out var lastGenerationDateLocal))
                                {
                                    lastGenerationDate = lastGenerationDateLocal;
                                }
                            }
                        }

                        if (lastGenerationDate == null || lastGenerationDate.Value != new FileInfo(absoluteFilePathData.absoluteFilePath).LastWriteTimeUtc.Ticks)
                        {
                            errors.Add(new ErrorData(ErrorCode.FileGeneratedFromTemplateWasModifiedAfterLastGeneration,
                                GetFileWasModifiedErrorMessage(), absoluteFilePathData.absoluteFilePath));
                            return errors;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ErrorData(ErrorCode.FileGeneratedFromTemplateWasModifiedAfterLastGeneration, ex, absoluteFilePathData.absoluteFilePath));
                return errors;
            }

            try
            {
                errors.AddRange(templateProcessor.GenerateFileFromTemplate(templateFilePath, out var generatedFileContents));

                using (var streamWriter = new StreamWriter(absoluteFilePathData.absoluteFilePath, false))
                    streamWriter.Write(generatedFileContents);

                try
                {
                    using (var saveDataStreamWriter = new StreamWriter(fileModificationDataFilePath, false))
                        saveDataStreamWriter.Write($"{lastModifiedDateParamName}:{ new FileInfo(absoluteFilePathData.absoluteFilePath).LastWriteTimeUtc.Ticks}");
                }
                catch (Exception ex2)
                {
                    errors.Add(new ErrorData(ErrorCode.FailedToSaveFileGenerationData, ex2, absoluteFilePathData.absoluteFilePath));
                    return errors;
                }

                return errors;
            }
            catch (Exception ex)
            {
                errors.Add(new ErrorData(ErrorCode.FailedToSaveFileGeneratedFromTemplate, ex, absoluteFilePathData.absoluteFilePath));
            }

            return errors;
        }
    }

    /// <inheritdoc />
    public class TemplateProcessor : ITemplateProcessor
    {
        /// <inheritdoc />
        public IReadOnlyList<IErrorData> GenerateFileFromTemplate(string templateFilePath, out string generatedText)
        {
            var errors = new List<IErrorData>();
            generatedText = string.Empty;

            var visitedFilePaths = new List<string>();

            if (!TryLoadFile(templateFilePath, out var templateFileContents, out Exception? e, out string? errorMessage, out var errorCode))
            {
                if (errorCode == null)
                    errorCode = ErrorCode.FileFailedToLoad;

                if (e != null)
                {
                    errors.Add(new ErrorData(errorCode.Value, e, templateFilePath));
                }

                else
                {
                    if (errorMessage == null)
                        errorMessage = "Failed to load the file";

                    errors.Add(new ErrorData(errorCode.Value, errorMessage, templateFilePath));
                }

                return errors;
            }

            if (templateFileContents == null)
            {
                errors.Add(new ErrorData(ErrorCode.FileFailedToLoad, "Implementation error.", templateFilePath));
                return errors;
            }

            TryGenerate(templateFilePath, templateFileContents, visitedFilePaths, errors, out generatedText);
            return errors;
        }

        /// <summary>
        /// The tag used 
        /// </summary>
        protected virtual string FileIncludeTagName => "IncludedFilePlaceHolder";

        private bool TryLoadFile(string templateFilePath, out string? templateFileContents, out Exception? e, out string? errorMessage, out ErrorCode? errorCode)
        {
            templateFileContents = null;
            e = null;
            errorMessage = null;
            errorCode = null;

            if (!File.Exists(templateFilePath))
            {
                errorCode = ErrorCode.FileDoesNotExist;
                errorMessage = $"File '{templateFilePath}' does not exist.";
                return false;
            }

            try
            {
                using (var streamReader = new StreamReader(templateFilePath))
                {
                    templateFileContents = streamReader.ReadToEnd();
                    return true;
                }
            }
            catch (Exception e2)
            {
                e = e2;
                errorCode = ErrorCode.FileFailedToLoad;
            }

            return true;
        }

        private void TryGenerate(string templateFilePath, string templateFileText, List<string> visitedFilePaths,
            List<IErrorData> errors, out string generatedText)
        {
            generatedText = string.Empty;

            var fileIncludeOpeningTagName = $"<{FileIncludeTagName}>";
            var fileIncludeClosingTagName = $"</{FileIncludeTagName}>";

            var generatedTextStrBldr = new StringBuilder();
            var currentPosition = 0;
            var recordedUpToPosition = 0;

            void AppendText(int positionInTemplateToAppendUpTo)
            {
                if (positionInTemplateToAppendUpTo > recordedUpToPosition)
                {
                    generatedTextStrBldr.Append(templateFileText.Substring(recordedUpToPosition, positionInTemplateToAppendUpTo - recordedUpToPosition));
                    recordedUpToPosition = positionInTemplateToAppendUpTo;
                }
            }

            visitedFilePaths.Add(templateFilePath);

            try
            {
                while (true)
                {
                    var indexOfIncludeStart = templateFileText.IndexOf(fileIncludeOpeningTagName, currentPosition, StringComparison.Ordinal);

                    if (indexOfIncludeStart < 0)
                    {
                        AppendText(templateFileText.Length);
                        generatedText = generatedTextStrBldr.ToString();
                        return;
                    }

                    AppendText(indexOfIncludeStart);

                    var indexOfIncludeEnd = templateFileText.IndexOf(fileIncludeClosingTagName,
                        indexOfIncludeStart + fileIncludeOpeningTagName.Length, StringComparison.Ordinal);

                    if (indexOfIncludeEnd < 0)
                    {
                        errors.Add(new ErrorData(ErrorCode.ClosingTagMissing,
                            $"Missing closing tag for opening tag '{this.FileIncludeTagName}' in template file.", templateFilePath)
                        {
                            ErrorPosition = indexOfIncludeStart
                        });

                        // Add the rest of the text after the error message is added.
                        AppendText(templateFileText.Length);

                        generatedText = generatedTextStrBldr.ToString();
                        return;
                    }

                    var filePathPosition = indexOfIncludeStart + fileIncludeOpeningTagName.Length;

                    var includedTemplateRelativePath = templateFileText.Substring(filePathPosition, indexOfIncludeEnd - filePathPosition).Trim();

                    var folderPath = Path.GetDirectoryName(templateFilePath)!;

                    var absoluteFilePathData =
                        FilePathHelpers.TryGetAbsoluteFilePath(folderPath, includedTemplateRelativePath);


                    if (absoluteFilePathData.isSuccess)
                    {
                        if (absoluteFilePathData.absoluteFilePath != null)
                        {
                            try
                            {
                                if (!Directory.Exists(absoluteFilePathData.absoluteFilePath))
                                {
                                    var indexOfFileInVisitedFiles = visitedFilePaths.FindLastIndex(x =>
                                        string.Equals(x, absoluteFilePathData.absoluteFilePath, StringComparison.InvariantCultureIgnoreCase));

                                    if (indexOfFileInVisitedFiles < 0)
                                    {
                                        if (TryLoadFile(absoluteFilePathData.absoluteFilePath, out var referencedFileContents, out Exception? loadReferencedFileException, out string? loadReferencedFileErrorMessage, out var errorCode))
                                        {
                                            this.TryGenerate(absoluteFilePathData.absoluteFilePath, referencedFileContents ?? String.Empty, visitedFilePaths, errors, out var referencedTemplateText);
                                            generatedTextStrBldr.Append(referencedTemplateText);
                                        }
                                        else
                                        {
                                            errorCode ??= ErrorCode.FileFailedToLoad;

                                            if (loadReferencedFileException != null)
                                                errors.Add(new ErrorData(errorCode.Value, loadReferencedFileException, templateFilePath)
                                                {
                                                    ErrorPosition = filePathPosition
                                                });
                                            else if (loadReferencedFileErrorMessage != null)
                                                errors.Add(new ErrorData(errorCode.Value, loadReferencedFileErrorMessage, templateFilePath)
                                                {
                                                    ErrorPosition = filePathPosition
                                                });


                                            errors.Add(new ErrorData(ErrorCode.FailedToLoadReferencedFile,
                                                $"Failed to load the referenced file '{absoluteFilePathData.absoluteFilePath}' specified as '{includedTemplateRelativePath}' in element '{FileIncludeTagName}'.", templateFilePath)
                                            {
                                                ErrorPosition = filePathPosition
                                            });
                                        }
                                    }
                                    else if (indexOfFileInVisitedFiles == visitedFilePaths.Count - 1)
                                    {
                                        errors.Add(new ErrorData(ErrorCode.TemplateFileReferencesItself,
                                            $"File '{templateFilePath}' references itself. Invalid included file path in element '{FileIncludeTagName}' is '{includedTemplateRelativePath}'.",
                                            templateFilePath)
                                        {
                                            ErrorPosition = filePathPosition
                                        });
                                    }
                                    else
                                    {
                                        // File1=>File2=>File3=>File2
                                        var circularReferenceErrorMessageStrBldr = new StringBuilder();

                                        circularReferenceErrorMessageStrBldr.AppendLine($"File '{absoluteFilePathData.absoluteFilePath}' referenced from '{visitedFilePaths.Last()}' results in the following circular references:");

                                        var circularReferencesList = new List<string>(visitedFilePaths.TakeLast(visitedFilePaths.Count - indexOfFileInVisitedFiles));
                                        circularReferencesList.Add(absoluteFilePathData.absoluteFilePath);

                                        for (var i = 0; i < circularReferencesList.Count - 1; ++i)
                                            circularReferenceErrorMessageStrBldr.AppendLine($"\tFile '{circularReferencesList[i]}' references '{circularReferencesList[i + 1]}'");

                                        var circularReferenceErrorMessage = circularReferenceErrorMessageStrBldr.ToString();

                                        generatedTextStrBldr.Append(circularReferenceErrorMessage);
                                        errors.Add(new ErrorData(ErrorCode.CircularReferences, circularReferenceErrorMessage,
                                            templateFilePath)
                                        {
                                            ErrorPosition = filePathPosition
                                        });
                                    }
                                }
                                else
                                {
                                    errors.Add(new ErrorData(ErrorCode.CouldNotCalculateAbsoluteFilePath,
                                        $"Failed to calculate absolute file path from path '{includedTemplateRelativePath}'. The specified path is a directory. Expected a file.", templateFilePath)
                                    {
                                        ErrorPosition = filePathPosition
                                    });
                                }
                            }
                            catch (Exception e)
                            {
                                errors.Add(new ErrorData(ErrorCode.CouldNotCalculateAbsoluteFilePath, e, templateFilePath)
                                {
                                    ErrorPosition = filePathPosition
                                });

                            }
                        }
                        else
                        {
                            errors.Add(new ErrorData(ErrorCode.FileDoesNotExist,
                                "Implementation error.", templateFilePath)
                            {
                                ErrorPosition = filePathPosition
                            });
                        }
                    }
                    else
                    {
                        errors.Add(new ErrorData(ErrorCode.CouldNotCalculateAbsoluteFilePath,
                            $"Failed to calculate absolute file path from path '{includedTemplateRelativePath}'. Original error: {absoluteFilePathData.absoluteFilePath}", templateFilePath)
                        {
                            ErrorPosition = filePathPosition
                        });
                    }

                    currentPosition = indexOfIncludeEnd + fileIncludeClosingTagName.Length;
                    recordedUpToPosition = currentPosition;
                }
            }
            finally
            {
                visitedFilePaths.RemoveAt(visitedFilePaths.Count - 1);
            }
        }
    }
}
