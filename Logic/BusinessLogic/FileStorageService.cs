using Logic.BusinessLogicInterface;
using Logic.Model;
using Microsoft.Extensions.Options;

namespace Logic.BusinessLogic
{
    // Local-disk storage, same shape as BookingBackendApi's Documents
    // approach (GUID-named files, only the relative path persisted) but
    // rooted outside wwwroot — these files (identity photos, stamped legal
    // documents) are read back only through an authenticated controller
    // action, never served as public static paths.
    public class FileStorageService : IFileStorageService
    {
        private readonly string _rootPath;

        public FileStorageService(IOptions<AppSettings> appSettings)
        {
            _rootPath = appSettings.Value.UploadsRootPath;
        }

        public async Task<string> SaveAsync(string subfolder, string originalFileName, byte[] content, CancellationToken ct = default)
        {
            var directory = Path.Combine(_rootPath, subfolder);
            Directory.CreateDirectory(directory);

            var extension = Path.GetExtension(originalFileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(directory, fileName);

            await File.WriteAllBytesAsync(fullPath, content, ct);

            return $"{subfolder}/{fileName}";
        }

        public async Task<byte[]?> ReadAsync(string relativePath, CancellationToken ct = default)
        {
            var fullPath = ResolvePath(relativePath);
            return File.Exists(fullPath) ? await File.ReadAllBytesAsync(fullPath, ct) : null;
        }

        public void Delete(string relativePath)
        {
            var fullPath = ResolvePath(relativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        private string ResolvePath(string relativePath) =>
            Path.Combine(_rootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
