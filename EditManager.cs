using System.Windows.Forms;

namespace TextEditor
{
    public class EditManager
    {
        public void Undo(RichTextBox inputArea)
        {
            if (inputArea.CanUndo)
                inputArea.Undo();
        }

        public void Redo(RichTextBox inputArea)
        {
            if (inputArea.CanRedo)
                inputArea.Redo();
        }

        public void Cut(RichTextBox inputArea)
        {
            inputArea.Cut();
        }

        public void Copy(RichTextBox inputArea)
        {
            inputArea.Copy();
        }

        public void Paste(RichTextBox inputArea)
        {
            inputArea.Paste();
        }

        public void Delete(RichTextBox inputArea)
        {
            if (inputArea.SelectionLength > 0)
                inputArea.SelectedText = "";
        }

        public void SelectAll(RichTextBox inputArea)
        {
            inputArea.SelectAll();
        }
    }
}
