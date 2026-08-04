namespace EXE02_Backend_RE_CAFE.Interfaces
{
    public interface IStoryHtmlSanitizer
    {
        string SanitizeAndValidate(string rawHtml, string fieldName);
    }
}
