using System.Diagnostics;
using MathFlow.Services.Coverters;
using MathFlow.Shared.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MathFlow.Pages;

public class Converter : PageModel
{
    private readonly ILogger<Converter> _logger;
    private readonly WordProcessor _wordProcessor;
    
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public ProcessingStats? ProcessingStats { get; set; }

    public Converter(ILogger<Converter> logger, WordProcessor wordProcessor)
    {
        _logger = logger;
        _wordProcessor = wordProcessor;
    }

    public void OnGet()
    {
        
    }
    public async Task<IActionResult> OnPostAsync(IFormFile uploadedFile)
    {
        // Validações
        if (uploadedFile.Length == 0)
        {
            ErrorMessage = "Por favor, selecione um arquivo.";
            return Page();
        }

        if (!uploadedFile.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = "Apenas arquivos .docx são aceitos.";
            return Page();
        }

        // Limite de tamanho (ex: 10MB)
        const long maxFileSize = 10 * 1024 * 1024;
        if (uploadedFile.Length > maxFileSize)
        {
            ErrorMessage = $"O arquivo é muito grande. Tamanho máximo: {maxFileSize / 1024 / 1024}MB";
            return Page();
        }

        try
        {
            var stopwatch = Stopwatch.StartNew();

            await using var inputStream = await uploadedFile.ToStreamAsync();

            var result = await _wordProcessor.ReplaceMathMLAsync(inputStream);

            stopwatch.Stop();
            
            // Estatísticas do processamento
            ProcessingStats = new ProcessingStats
            {
                FormulasConverted = 0, //todo
                ProcessingTime = $"{stopwatch.ElapsedMilliseconds}ms",
                FileSize = FormatFileSize(uploadedFile.Length)
            };

            _logger.LogInformation("Documento processado com sucesso em {Time}ms", 
                stopwatch.ElapsedMilliseconds);
            
            return File(
                result, 
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                uploadedFile.FileName
            );

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao processar documento");
            ErrorMessage = $"Erro inesperado: {ex.Message}";
            return Page();
        }
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}


// Classe para estatísticas de processamento
public class ProcessingStats
{
    public int FormulasConverted { get; set; }
    public string ProcessingTime { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
}
