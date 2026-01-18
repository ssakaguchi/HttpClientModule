using Microsoft.Win32;

namespace HttpClientWPF.FileDialogService
{
    public class OpenFileDialogService : IOpenFileDialogService
    {
        private OpenFileDialog _openFileDialog = new();

        public string Title
        {
            get { return _openFileDialog.Title; }
            set { _openFileDialog.Title = value; }
        }

        public string Filter
        {
            get { return _openFileDialog.Filter; }
            set { _openFileDialog.Filter = value; }
        }

        public string FilePath
        {
            get { return _openFileDialog.FileName; }
            set { _openFileDialog.FileName = value; }
        }

        public bool? OpenFileDialog()
        {
            return _openFileDialog.ShowDialog();
        }
    }
}
