public class FileItem
{
    public string Name { get; set; }
    public bool IsDirectory { get; set; }
    public string Path { get; set; }  // Для перехода по директориям
}
