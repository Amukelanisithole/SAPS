using Logic.Model;

namespace Logic.BusinessLogicInterface
{
    public interface IDocketPdfService
    {
        byte[] Generate(DocketPdfModel model);
    }
}
