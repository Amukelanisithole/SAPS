using Database.Entities;

namespace Logic.BusinessLogicInterface
{
    public interface IAffidavitPdfService
    {
        // verificationPhotoBytes is embedded as part of the identity-verification
        // record on the document; null is tolerated (renders that section without an image).
        byte[] Generate(Affidavit affidavit, byte[]? verificationPhotoBytes);
    }
}
