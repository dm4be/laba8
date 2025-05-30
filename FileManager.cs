using System;
using System.IO;
using System.Windows.Forms;

namespace TextEditor
{
    public class FileManager
    {
        private string currentFilePath = string.Empty;
        private bool isTextChanged = false;

        public string CurrentFilePath => currentFilePath;
        public bool IsTextChanged => isTextChanged;

        public void MarkTextChanged() => isTextChanged = true;

        public void NewFile(RichTextBox inputArea)
        {
            if (CheckUnsavedChanges(inputArea))
            {
                inputArea.Clear();
                currentFilePath = string.Empty;
                isTextChanged = false;
            }
        }

        public void OpenFile(RichTextBox inputArea)
        {
            if (!CheckUnsavedChanges(inputArea)) return;

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    currentFilePath = ofd.FileName;
                    inputArea.Text = File.ReadAllText(currentFilePath);
                    isTextChanged = false;
                }
            }
        }

        public void SaveFile(RichTextBox inputArea)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                SaveFileAs(inputArea);
            }
            else
            {
                File.WriteAllText(currentFilePath, inputArea.Text);
                isTextChanged = false;
            }
        }

        public void SaveFileAs(RichTextBox inputArea)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    currentFilePath = sfd.FileName;
                    SaveFile(inputArea);
                }
            }
        }

        public bool CheckUnsavedChanges(RichTextBox inputArea)
        {
            if (!isTextChanged) return true;
            var result = MessageBox.Show("Сохранить изменения?", "Внимание", MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.Yes)
            {
                SaveFile(inputArea);
                return true;
            }
            else if (result == DialogResult.No)
            {
                return true;
            }
            return false;
        }

        public void ExitApplication(RichTextBox inputArea)
        {
            if (CheckUnsavedChanges(inputArea))
            {
                Application.Exit();
            }
        }
    }
}
