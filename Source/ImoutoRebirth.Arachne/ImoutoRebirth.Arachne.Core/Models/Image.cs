namespace ImoutoRebirth.Arachne.Core.Models;

public class Image
{
    public string Md5 { get; }

    public string FileName { get; }

    public Image(string md5, string fileName)
    {
        Md5 = md5;
        FileName = fileName;
    }
}