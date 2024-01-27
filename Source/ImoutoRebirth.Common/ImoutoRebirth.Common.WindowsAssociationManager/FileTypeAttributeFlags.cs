namespace ImoutoRebirth.Common.WindowsAssociationManager;

[Flags]
public enum FileTypeAttributeFlags
{
    FTA_None = 0,

    FTA_Exclude = 1,

    FTA_Show = 2,

    FTA_HasExtension = 4,

    FTA_NoEdit = 8,

    FTA_NoRemove = 16,

    FTA_NoNewVerb = 32,

    FTA_NoEditVerb = 64,

    FTA_NoRemoveVerb = 128,

    FTA_NoEditDesc = 256,

    FTA_NoEditIcon = 512,

    FTA_NoEditDflt = 1024,

    FTA_NoEditVerbCmd = 2048,

    FTA_NoEditVerbExe = 4096,

    FTA_NoDDE = 8192,

    FTA_NoEditMIME = 32768,

    FTA_OpenIsSafe = 65536,

    FTA_AlwaysUnsafe = 131072,

    FTA_NoRecentDocs = 1048576,

    FTA_SafeForElevation = 2097152,

    FTA_AlwaysUseDirectInvoke = 4194304
}
