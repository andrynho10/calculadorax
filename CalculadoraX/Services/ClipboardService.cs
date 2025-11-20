using System.Windows;

namespace CalculadoraX.Services;

public class ClipboardService : IClipboardService
{
    public void SetText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        Clipboard.SetText(text);
    }
}
