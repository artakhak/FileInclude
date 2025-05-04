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
/// Transforms text that replaces the placeholder in template file.
/// For example if the template file "x:\README.md" has text
/// &lt;IncludedFilePlaceHolder&gt;SomeFeature1\README.md&lt;/IncludedFilePlaceHolder&gt;
/// we can process the text generated from file "SomeFeature1\README.md" to add some line breaks, spaces, etc.
/// Note: the text in file "SomeFeature1\README.md" might be a template itself, in which case its placeholders will be replaced too. 
/// </summary>
public interface IReplacedTextTransformer
{
    /// <summary>
    /// Transforms text that replaces the placeholder in template file.
    /// For example if the template file has text
    /// &lt;IncludedFilePlaceHolder&gt;SomeFeature1\README.md&lt;/IncludedFilePlaceHolder&gt;
    /// we can process the text generated from file "SomeFeature1\README.md" to add some line breaks, spaces, etc.
    /// Note: the text in file "SomeFeature1\README.md" might be a template itself, in which case its placeholders will be replaced too. 
    /// </summary>
    /// <param name="replacedText">Text that replaces the placeholder &lt;IncludedFilePlaceHolder&gt;RelativeFilePath&lt;/IncludedFilePlaceHolder&gt; and that should be transformed.</param>
    /// <param name="templateFilePath">Absolute file path of the template that contains a referenced file path Template file absolute path. This is the fi</param>
    /// <param name="templateFileContents">Template file contents. The processor can use this along with <paramref name="indexOfReplacedTextInTemplateFile"/> to lookup data related to
    /// replaced text in <paramref name="replacedText"/>. For example <see cref="IndentedReplacedTextTransformer"/> uses these parameter values to determine the indentation.
    /// </param>
    /// <param name="indexOfReplacedTextInTemplateFile">Index of replaced text <paramref name="replacedText"/> in template file.</param>
    /// <param name="referencedFilePath">Absolute path of the file referenced in template, from which the text <paramref name="replacedText"/> is loaded.</param>
    /// <returns>Returns text that can be either the same as the text in <paramref name="replacedText"/> or can be a text transformed from <paramref name="replacedText"/> (say text wil line break, etc).</returns>
    string TransformReplacedText(string replacedText, string templateFilePath, string templateFileContents, int indexOfReplacedTextInTemplateFile, string referencedFilePath);
}
