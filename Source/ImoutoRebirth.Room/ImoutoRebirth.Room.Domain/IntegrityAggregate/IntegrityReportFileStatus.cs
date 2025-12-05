using System.Diagnostics.CodeAnalysis;

namespace ImoutoRebirth.Room.Domain.IntegrityAggregate;

public class IntegrityReportFileStatus
{
    private IntegrityReportFileStatus(
        Guid fileId,
        string expectedFullPath,
        string fileName,
        bool isPresent,
        string expectedHash,
        string? actualHash,
        string? readingProblem)
    {
        FileId = fileId;
        ExpectedFullPath = expectedFullPath;
        FileName = fileName;
        IsPresent = isPresent;
        ExpectedHash = expectedHash;
        ActualHash = actualHash;
        ReadingProblem = readingProblem;
    }

    public Guid FileId { get; }

    public string ExpectedFullPath { get; }

    public string FileName { get; }

    [MemberNotNullWhen(true, nameof(ActualHash))]
    public bool IsPresent { get; }

    public string ExpectedHash { get; }

    public string? ActualHash { get; }

    public string? ReadingProblem { get; }

    public IntegrityStatus Status => IsPresent
        ? ExpectedHash == ActualHash
            ? IntegrityStatus.Ok
            : IntegrityStatus.HashMismatch
        : IntegrityStatus.Missing;

    public static IntegrityReportFileStatus Create(
        CollectionFile file,
        bool isPresent,
        string? actualHash,
        string? readingProblem)
    {
        return new IntegrityReportFileStatus(
            file.Id,
            file.Path,
            Path.GetFileName(file.Path),
            isPresent,
            file.Md5,
            actualHash,
            readingProblem);
    }

    public static IntegrityReportFileStatus Restore(
        Guid fileId,
        string expectedFullPath,
        string fileName,
        bool isPresent,
        string expectedHash,
        string? actualHash,
        string? readingProblem)
        => new(fileId, expectedFullPath, fileName, isPresent, expectedHash, actualHash, readingProblem);
}
