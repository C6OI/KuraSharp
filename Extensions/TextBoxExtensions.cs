using Avalonia.Controls;

namespace KuraSharp.Extensions; 

public static class TextBoxExtensions {
    public static void ToggleVisible(this TextBox textBox, char passChar) =>
        textBox.PasswordChar = textBox.PasswordChar == passChar ? char.MinValue : passChar;
}
