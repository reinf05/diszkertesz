namespace diszkerteszAPI.Models
{

    public class ServiceResult
    {
        public string? ErrorMessage { get; set; }
        public string? MetaData { get; set; }
        public bool Success => string.IsNullOrEmpty(ErrorMessage);

        public static ServiceResult SuccessResult(string? metadata = null)
        {
            return new ServiceResult
            {
                MetaData = metadata
            };
        }

        public static ServiceResult FailureResult(string errorMessage)
        {
            return new ServiceResult
            {
                ErrorMessage = errorMessage
            };
        }
    }
    public class ServiceResult<T> : ServiceResult
    {
        public T? Data { get; set; }


        public static ServiceResult<T> SuccessResult(T data, string? metadata = null)
        {
            return new ServiceResult<T>
            {
                Data = data,
                MetaData = metadata
            };
        }

        public static ServiceResult<T> FailureResult(string errorMessage)
        {
            return new ServiceResult<T>
            {
                ErrorMessage = errorMessage
            };
        }
    }
}
