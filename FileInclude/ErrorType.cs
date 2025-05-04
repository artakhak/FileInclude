// This software is part of the FileInclude library
// Copyright © 2018 FileInclude Contributors
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

namespace FileInclude;

/// <summary>
/// Error type.
/// </summary>
public enum ErrorCode
{
    /// <summary>
    /// Absolute file path could not be calculated.
    /// </summary>
    CouldNotCalculateAbsoluteFilePath,

    /// <summary>
    /// File does not exist.
    /// </summary>
    FileDoesNotExist,

    /// <summary>
    /// File failed to load.
    /// </summary>
    FileFailedToLoad,

    /// <summary>
    /// File referenced in element "ReadMeInclude" failed to load.
    /// </summary>
    FailedToLoadReferencedFile,

    /// <summary>
    /// Template file path is the same as the file path where generated file is saved.
    /// </summary>
    TemplateFilePathIsTheSameAsGeneratedFilePath,

    /// <summary>
    /// The file specified in element in element "ReadMeInclude" in template file references the template file where it is specified.
    /// </summary>
    TemplateFileReferencesItself,

    /// <summary>
    /// The file specified in element in element "ReadMeInclude" results in circular references.
    /// </summary>
    CircularReferences,

    /// <summary>
    /// Failed to save file generated from template.
    /// </summary>
    FailedToSaveFileGeneratedFromTemplate,

    /// <summary>
    /// Template file has an opening tag "ReadMeInclude" but does not have a closing tag.
    /// </summary>
    ClosingTagMissing,

    /// <summary>
    /// Error reported when a change to the generated file is detected which was not done by this library (most probably manual edit).
    /// In this case, the file should be backed up and renamed or deleted, so that the 
    /// </summary>
    FileGeneratedFromTemplateWasModifiedAfterLastGeneration,

    /// <summary>
    /// This error happens when the file generated from template is saved, however data about generation fails to save.
    /// This is not a critical error, since by the time this error happens, the file is saved.
    /// </summary>
    FailedToSaveFileGenerationData
}
