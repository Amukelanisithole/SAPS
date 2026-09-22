using static Logic.Enums.Enums;

namespace Logic.Model
{
    public class ResultModel
    {
        public ResultModel()
        {
            ResultId = EResultValue.Fail;
            Message = "Failed";
        }

        public long ResultId { get; set; }
        public string Message { get; set; }
        public string? Data { get; set; }
    }

    public class ResultModel<T>
    {
        public ResultModel()
        {
            ResultId = EResultValue.Fail;
            Message = "Failed";
        }

        public long ResultId { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
    }
}
