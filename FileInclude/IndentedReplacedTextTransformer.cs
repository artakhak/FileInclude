﻿using System.Text;

namespace FileInclude;

/// <summary>
/// An implementation of <see cref="IReplacedTextTransformer"/> that appends an indent to each on-empty line in replaced text so that the
/// text replaced in template file is indented.
/// For example if template file "c:\README.md" has the following text:
///
/// Some text
/// -Feature1: &lt;IncludedFilePlaceHolder&gt;Feature1\README.md&lt;/IncludedFilePlaceHolder&gt;
/// -Feature2:...
/// 
/// And the file Feature1\README.md had the following content
/// This is
/// cool feature 1.
///
/// Using <see cref="IndentedReplacedTextTransformer"/> with <see cref="ITemplateProcessor.GenerateFileFromTemplate(string, out string)"/> will result in the following text
/// if the processed template is "c:\README.md":
///
/// -Feature1: This is
///            cool feature 1.
/// -Feature2:...
/// </summary>
public class IndentedReplacedTextTransformer : IReplacedTextTransformer
{
    /// <inheritdoc />
    public string TransformReplacedText(string replacedText, string templateFilePath, string templateFileContents, int indexOfReplacedTextInTemplateFile, string referencedFilePath)
    {
        var replacedTextLines = replacedText.Split(Environment.NewLine);

        if (replacedTextLines.Length == 1)
            return replacedText;

        LinkedList<char> indentCharacters = new LinkedList<char>();

        for (var charInd = indexOfReplacedTextInTemplateFile - 1; charInd >= 0; --charInd)
        {
            var currentChar = templateFileContents[charInd];

            // Lets check if current character is party of line break
            if (currentChar == Environment.NewLine[^1])
            {
                // If there is only one character in line break, we are done
                if (Environment.NewLine.Length == 1)
                    break;

                if (charInd - (Environment.NewLine.Length - 1) >= 0)
                {
                    var isLineBreak = true;

                    for (var i = 1; i < Environment.NewLine.Length; ++i)
                    {
                        if (templateFileContents[charInd - i] != Environment.NewLine[Environment.NewLine.Length - 1 - i])
                        {
                            isLineBreak = false;
                            break;
                        }
                    }

                    if (isLineBreak)
                        break;
                }
            }
            
            // If we got here, line break was not reached yet.
            
            // If the current character is a white space, it might be a space or a tab, so lets just add the character as it is
            // Otherwise, lets add a space
            indentCharacters.AddFirst(char.IsWhiteSpace(currentChar) ? currentChar : ' ');
        }

        var indentStrBuilder = new StringBuilder(indentCharacters.Count);
        foreach (var character in indentCharacters)
            indentStrBuilder.Append(character);

        var transformedReplacedText = new StringBuilder(replacedText.Length + replacedTextLines.Length * indentStrBuilder.Length + 10);

        var indent = indentStrBuilder.ToString();
        
        for (var lineIndex = 0; lineIndex < replacedTextLines.Length; ++lineIndex)
        {
            var line = replacedTextLines[lineIndex];

            if (lineIndex > 0)
            {
                transformedReplacedText.AppendLine();
                
                if (line.Length > 0)
                    transformedReplacedText.Append(indent);
            }

            transformedReplacedText.Append(line);
        }

        return transformedReplacedText.ToString();
    }
}