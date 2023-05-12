namespace ImoutoRebirth.Arachne.Infrastructure.Models.Settings;

public class SankakuSettings
{
    public string Login { get; set; } = default!;

    public string PassHash { get; set; } = default!;

    public int Delay { get; set; }
}
