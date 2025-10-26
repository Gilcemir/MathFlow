namespace MathFlow.Shared.Abstractions;

public class TempFileStream : FileStream
{
    public readonly string TempFilePath;

    public TempFileStream(string tempFilePath) 
        : base(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose)
    {
        TempFilePath = tempFilePath;
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            base.Dispose(disposing);
        }
        finally
        {
            // Garantia extra de limpeza
            if (File.Exists(TempFilePath))
            {
                try
                {
                    File.Delete(TempFilePath);
                }
                catch
                {
                    // Ignorar erros de limpeza
                }
            }
        }
    }
}