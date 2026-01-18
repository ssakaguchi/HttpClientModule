namespace HttpClientWPF.FileDialogService
{
    public interface IOpenFileDialogService
    {
        public string Title { get; set; }
        public string Filter { get; set; }

        public string FilePath { get; set; }

        public bool? OpenFileDialog();
    }
}
