namespace FileInclude;

/// <summary>
/// An implementation of <see cref="IReplacedTextTransformer"/> that does no transformation of text replaced in template file.
/// </summary>
public class NoTransformationReplacedTextTransformer : IReplacedTextTransformer
{
    /// <inheritdoc />
    public string TransformReplacedText(string replacedText, string templateFilePath, string templateFileContents, int indexOfReplacedTextInTemplateFile, string referencedFilePath)
    {
        return replacedText;
    }
}