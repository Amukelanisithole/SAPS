namespace Logic.BusinessLogicInterface
{
    public interface IFileStorageService
    {
        // Writes content under {UploadsRootPath}/{subfolder}/, naming the
        // file with a fresh GUID so callers never collide, and returns the
        // path relative to UploadsRootPath (forward-slashed) to persist.
        Task<string> SaveAsync(string subfolder, string originalFileName, byte[] content, CancellationToken ct = default);

        Task<byte[]?> ReadAsync(string relativePath, CancellationToken ct = default);

        void Delete(string relativePath);
    }
}
