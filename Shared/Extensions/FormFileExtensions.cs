using MathFlow.Shared.Abstractions;

namespace MathFlow.Shared.Extensions;

public static class FormFileExtensions
{
    public static async Task<TempFileStream> ToStreamAsync(this IFormFile file)
    {
        var tempFile = Path.GetTempFileName();
        
        await using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
        {
            await file.CopyToAsync(fileStream);
        }
        
        return new TempFileStream(tempFile);
    }
}