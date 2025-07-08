using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using ImoutoRebirth.Common.WPF.ValidationAttributes;

namespace ImoutoRebirth.Navigator.ViewModel.SettingsSlice;

internal partial class SourceFolderVM : FolderVM
{
    [ObservableProperty]
    private bool _checkFormat;

    [ObservableProperty]
    private bool _checkNameHash;

    [ObservableProperty]
    private bool _tagsFromSubfolder;

    [ObservableProperty]
    private bool _addTagFromFileName;

    [ObservableProperty]
    private bool _isWebhookUploadEnabled;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    [NotifyDataErrorInfo]
    [CustomValidation(typeof(SourceFolderVM), nameof(ValidateWebhookUploadUrl))]
    private string? _webhookUploadUrl;

    public ObservableCollection<string> SupportedExtensionsRaw { get; }

    public SourceFolderVM(
        Guid? id, 
        string path, 
        bool checkFormat, 
        bool checkNameHash, 
        IEnumerable<string>? extensions, 
        bool tagsFromSubfolder, 
        bool addTagFromFilename,
        bool isWebhookUploadEnabled,
        string? webhookUploadUrl)
        : base(id, path)
    {
        CheckFormat = checkFormat;
        CheckNameHash = checkNameHash;
        TagsFromSubfolder = tagsFromSubfolder;
        AddTagFromFileName = addTagFromFilename;
        IsWebhookUploadEnabled = isWebhookUploadEnabled;
        WebhookUploadUrl = webhookUploadUrl;

        SupportedExtensionsRaw = extensions != null
            ? new ObservableCollection<string>(extensions) 
            : new ObservableCollection<string>();
    }

    public string SupportedExtensions
    {
        get => string.Join(";", SupportedExtensionsRaw);
        set
        {
            try
            {
                var list = value.Split(';').ToList();
                SupportedExtensionsRaw.Clear();
                foreach (var item in list)
                {
                    SupportedExtensionsRaw.Add(item);
                }
            }
            catch
            {
                // ignore
            }
        }
    }

    public static ValidationResult ValidateWebhookUploadUrl(string? webhookUploadUrl, ValidationContext context)
    {
        var instance = (SourceFolderVM)context.ObjectInstance;

        if (!instance.IsWebhookUploadEnabled)
            return ValidationResult.Success!;

        if (string.IsNullOrWhiteSpace(webhookUploadUrl))
            return new("The WebhookUploadUrl field is required when WebhookUpload is enabled.");

        return ValidationResult.Success!;
    }
}
