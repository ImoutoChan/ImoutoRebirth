namespace ImoutoRebirth.Arachne.Core.Models;

public class Image
{
    public string Md5 { get; }

    public Image(string md5)
    {
        Md5 = md5;
    }
}