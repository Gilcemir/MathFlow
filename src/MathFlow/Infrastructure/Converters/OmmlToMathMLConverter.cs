using Jering.Javascript.NodeJS;

namespace MathFlow.Infrastructure.Converters;

public class OmmlToMathMLConverter
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

            return result ?? string.Empty;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to convert OMML to MathML: {ex.Message}", ex);
        }
    }
}