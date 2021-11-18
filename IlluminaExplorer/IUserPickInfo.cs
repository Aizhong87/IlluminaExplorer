namespace IlluminaExplorer
{
    public interface IUserPickInfo
    {
        string Delimiter { get; set; }
        string Extension { get; set; }
        string FileName { get; set; }
        string FullPath { get; }
        string SelectedPath { get; set; }
    }
}