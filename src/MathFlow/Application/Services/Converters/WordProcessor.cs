using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using MathFlow.Infrastructure.Converters;
using MathFlow.Shared.Abstractions;

namespace MathFlow.Application.Services.Converters;

public class WordProcessor
{
    private const string FormulaXmlWithParaAttrName = "oMathPara";
    private const string FormulaXmlNoParaName = "oMath";
    
    private readonly OmmlToMathMLConverter _ommlToMathMlConverter;
    
    public WordProcessor(OmmlToMathMLConverter ommlToMathMlConverter)
    {
        _ommlToMathMlConverter = ommlToMathMlConverter;
    }
    
    public async Task<Stream> ReplaceMathMLAsync(TempFileStream inputStream)
    {
        string tempOutputPath = Path.GetTempFileName();

        try
        {
            // Copia para arquivo de saída para processamento
            File.Copy(inputStream.TempFilePath, tempOutputPath, true);

            // Processa o documento
            using (var doc = WordprocessingDocument.Open(tempOutputPath, true))
            {
                var mainPart = doc.MainDocumentPart;

                if (mainPart == null)
                    throw new InvalidOperationException("Documento sem parte principal");

                await using (var docStream = mainPart.GetStream(FileMode.Open, FileAccess.ReadWrite))
                {
                    var docXml = XDocument.Load(docStream);

                    var equations = docXml
                        .Descendants()
                        .Where(e => e.Name.LocalName is FormulaXmlWithParaAttrName or FormulaXmlNoParaName)
                        .ToArray();

                    for (int i = 0; i < equations.Length; i++)
                    {
                        var equation = await _ommlToMathMlConverter.ConvertAsync(equations[i].ToString());
                        var run = CreateXElementFromString(equation);
                        equations[i].ReplaceWith(run);
                    }

                    docStream.SetLength(0);
                    docStream.Position = 0;
                    docXml.Save(docStream);
                    Array.Clear(equations, 0, equations.Length);
                }

                doc.Save();
            }

            // Retorna stream do arquivo processado (com limpeza automática)
            return new TempFileStream(tempOutputPath);
        }
        catch
        {
            throw new InvalidOperationException("Erro ao processar documento");
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
