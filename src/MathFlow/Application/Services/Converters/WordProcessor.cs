using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using MathFlow.Infrastructure.Converters;
using MathFlow.Shared.Abstractions;
using Microsoft.Extensions.Logging;

namespace MathFlow.Application.Services.Converters;

public class WordProcessor
{
    private const string FormulaXmlWithParaAttrName = "oMathPara";
    private const string FormulaXmlNoParaName = "oMath";
    
    private readonly OmmlToMathMLConverter _ommlToMathMlConverter;
    private readonly ILogger<WordProcessor> _logger;
    
    public WordProcessor(OmmlToMathMLConverter ommlToMathMlConverter, ILogger<WordProcessor> logger)
    {
        _ommlToMathMlConverter = ommlToMathMlConverter;
        _logger = logger;
    }
    
    public async Task<Stream> ReplaceMathMLAsync(TempFileStream inputStream)
    {
        string tempOutputPath = Path.GetTempFileName();

        try
        {
            _logger.LogInformation("Copiando arquivo temporário para processamento...");
            // Copia para arquivo de saída para processamento
            File.Copy(inputStream.TempFilePath, tempOutputPath, true);

            _logger.LogInformation("Abrindo documento Word para processamento...");
            // Processa o documento
            using (var doc = WordprocessingDocument.Open(tempOutputPath, true))
            {
                var mainPart = doc.MainDocumentPart;

                if (mainPart == null)
                    throw new InvalidOperationException("Documento sem parte principal");

                _logger.LogInformation("Carregando XML do documento...");
                await using (var docStream = mainPart.GetStream(FileMode.Open, FileAccess.ReadWrite))
                {
                    var docXml = XDocument.Load(docStream);

                    var equations = docXml
                        .Descendants()
                        .Where(e => e.Name.LocalName is FormulaXmlWithParaAttrName or FormulaXmlNoParaName)
                        .ToArray();

                    _logger.LogInformation("Encontradas {Count} equações para converter", equations.Length);

                    for (int i = 0; i < equations.Length; i++)
                    {
                        _logger.LogDebug("Convertendo equação {Current}/{Total}", i + 1, equations.Length);
                        var equation = await _ommlToMathMlConverter.ConvertAsync(equations[i].ToString());
                        var run = CreateXElementFromString(equation);
                        equations[i].ReplaceWith(run);
                    }

                    _logger.LogInformation("Salvando documento processado...");
                    docStream.SetLength(0);
                    docStream.Position = 0;
                    docXml.Save(docStream);
                    Array.Clear(equations, 0, equations.Length);
                }

                doc.Save();
            }

            _logger.LogInformation("Documento processado com sucesso. Retornando stream...");
            // Retorna stream do arquivo processado (com limpeza automática)
            return new TempFileStream(tempOutputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar documento: {Message}", ex.Message);
            throw new InvalidOperationException($"Erro ao processar documento: {ex.Message}", ex);
        }
    }

    private static XElement CreateXElementFromString(string input)
    {
        XNamespace w = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        XElement run = new XElement(
            w + "r",
            new XElement(
                w + "rPr",
                new XElement(
                    w + "rFonts",
                    new XAttribute(w + "ascii", "Times New Roman"),
                    new XAttribute(w + "hAnsi", "Times New Roman"),
                    new XAttribute(w + "cs", "Times New Roman"),
                    new XAttribute(w + "val", "12")
                ),
                new XElement(
                    w + "t",
                    input
                )
            )
        );
        return run;
    }
}
