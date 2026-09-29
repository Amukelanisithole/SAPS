namespace Logic.BusinessLogicInterface
{
    public interface IFileStorageService
    {
        Task<string> SaveAsync(string subfolder, string originalFileName, byte[] content, CancellationToken ct = default);

        Task<byte[]?> ReadAsync(string relativePath, CancellationToken ct = default);

        void Delete(string relativePath);
    }
}
