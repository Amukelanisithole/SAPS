using Database.Entities;

namespace Logic.BusinessLogicInterface
{
    public interface IAffidavitPdfService
    {
        byte[] Generate(Affidavit affidavit, byte[]? verificationPhotoBytes);
    }
}
