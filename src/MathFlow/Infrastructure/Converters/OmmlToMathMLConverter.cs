using System.Text.RegularExpressions;
using Jering.Javascript.NodeJS;

namespace MathFlow.Infrastructure.Converters;

public partial class OmmlToMathMLConverter
{
    private readonly INodeJSService _nodeJsService;
    private const string SCRIPT_PATH = "converter.js";

    public OmmlToMathMLConverter(INodeJSService nodeJsService)
    {
        _nodeJsService = nodeJsService;
    }

    public async Task<string> ConvertAsync(string omml)
    {
        if (string.IsNullOrWhiteSpace(omml))
        {
            throw new ArgumentException("OMML cannot be null or empty", nameof(omml));
        }

        try
        {
            var result = await _nodeJsService.InvokeFromFileAsync<string>(
                SCRIPT_PATH,
                args: new[] { omml }
            );

            return ManipulateHtml(result);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to convert OMML to MathML: {ex.Message}", ex);
        }
    }
    private static string ManipulateHtml(string? html)
    {
        if(string.IsNullOrEmpty(html))
            return string.Empty;
        // Substituir todas as ocorrências de <tagName> por <mml:tagName>
        html = MyRegex().Replace(html, "<mml:$1");

        // Substituir todas as ocorrências de </tagName> por </mml:tagName>
        html = MyRegex1().Replace(html, "</mml:$1>");
        html = MyRegex2().Replace(html, "");
        return html;
    }

    [GeneratedRegex(@"<(\w+)")]
    private static partial Regex MyRegex();
    [GeneratedRegex(@"<\/(\w+)>")]
    private static partial Regex MyRegex1();
    [GeneratedRegex(@"\s\s+")]
    private static partial Regex MyRegex2();
}