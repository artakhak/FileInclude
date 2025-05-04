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
/// Error data.
/// </summary>
public interface IErrorData
{
    /// <summary>
    /// Error code.
    /// </summary>
    ErrorCode ErrorCode { get; }

    /// <summary>
    /// Error message.
    /// </summary>
    string ErrorMessage { get; }

    /// <summary>
    /// If the value is not null, thrown exception.
    /// </summary>
    Exception? Exception { get; }

    /// <summary>
    /// Absolute file path that caused the error.
    /// </summary>
    string SourceFilePath { get; }

    /// <summary>
    /// Error position in file <see cref="SourceFilePath"/>.
    /// The value is null if the file was not loaded (i.e., the file  does not exist, etc.).
    /// </summary>
    int? ErrorPosition { get; }
}

/// <inheritdoc />
internal class ErrorData : IErrorData
{
    public ErrorData(ErrorCode errorCode, string errorMessage, string sourceFilePath)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        SourceFilePath = sourceFilePath;
    }

    public ErrorData(ErrorCode errorCode, Exception exception, string sourceFilePath)
    {
        ErrorCode = errorCode;
        Exception = exception;
        SourceFilePath = sourceFilePath;
        ErrorMessage = exception.Message;
    }

    public ErrorCode ErrorCode { get; }
    public string ErrorMessage { get; }
    public Exception? Exception { get; }
    public string SourceFilePath { get; }
    public int? ErrorPosition { get; set; }
}
