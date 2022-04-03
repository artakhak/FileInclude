using System.Text;
using OROptimizer.Utilities;

namespace FileInclude;

/// <inheritdoc />
public class TemplateProcessor : ITemplateProcessor
{
    /// <inheritdoc />
    public IReadOnlyList<IErrorData> GenerateFileFromTemplate(string templateFilePath, out string generatedFileContents)
    {
        return GenerateFileFromTemplate(templateFilePath, new IndentedReplacedTextTransformer(), out generatedFileContents);
    }

    /// <inheritdoc />
    public IReadOnlyList<IErrorData> GenerateFileFromTemplate(string templateFilePath, IReplacedTextTransformer replacedTextTransformer, out string generatedFileContents)
    {
        var errors = new List<IErrorData>();
        generatedFileContents = string.Empty;

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

        TryGenerate(templateFilePath, templateFileContents, replacedTextTransformer, visitedFilePaths, errors, out generatedFileContents);
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

    private void TryGenerate(string templateFilePath, string templateFileText, IReplacedTextTransformer replacedTextTransformer, List<string> visitedFilePaths,
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
                                        this.TryGenerate(absoluteFilePathData.absoluteFilePath, referencedFileContents ?? String.Empty, replacedTextTransformer, visitedFilePaths, errors, out var referencedTemplateText);
                                       
                                        generatedTextStrBldr.Append(replacedTextTransformer.TransformReplacedText( 
                                            referencedTemplateText, templateFilePath, templateFileText,
                                            indexOfIncludeStart,
                                            absoluteFilePathData.absoluteFilePath));
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