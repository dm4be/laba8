using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace TextEditor
{
    public class HelpManager
    {
        public void ShowHelp()
        {
            string resourceName = "TextEditor.Resources.help.html"; // Имя ресурса

            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                MessageBox.Show("Файл справки не найден.");
                return;
            }

            // Читаем из ресурса
            using StreamReader reader = new StreamReader(stream);
            string htmlContent = reader.ReadToEnd();

            // Создаем временный HTML-файл
            string tempFile = Path.Combine(Path.GetTempPath(), "compiler_help.html");
            File.WriteAllText(tempFile, htmlContent);

            // Открываем в браузере по умолчанию
            Process.Start(new ProcessStartInfo
            {
                FileName = tempFile,
                UseShellExecute = true
            });
        }

        public void ShowAbout()
        {
            MessageBox.Show("Текстовый редактор версии 1.0\nРазработан в 2025 году Полетаевым С.Е.", "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public void ShowGrammar()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "grammar.html");
            if (!File.Exists(tempPath))
            {
                using (Stream stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("TextEditor.Resources.grammar.html"))
                using (StreamReader reader = new StreamReader(stream))
                {
                    File.WriteAllText(tempPath, reader.ReadToEnd());
                }
            }
            System.Diagnostics.Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
        }
        public void ShowClassification()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "classification.html");
            if (!File.Exists(tempPath))
            {
                using (Stream stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("TextEditor.Resources.classification.html"))
                using (StreamReader reader = new StreamReader(stream))
                {
                    File.WriteAllText(tempPath, reader.ReadToEnd());
                }
            }
            System.Diagnostics.Process.Start(new ProcessStartInfo(tempPath) { UseShellExecute = true });
        }
    }
}
