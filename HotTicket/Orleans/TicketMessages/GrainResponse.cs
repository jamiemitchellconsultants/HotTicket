namespace TicketMessages
{
    public class GrainResponse : GrainResponse<object>
    {
        public static GrainResponse SuccessResponse()
        {
            return new GrainResponse
            {
                Success = true
            };
        }

        public new static  GrainResponse FailureResponse(string errorMessage)
        {
            return new GrainResponse
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
    public class GrainResponse<T>
    {
        public bool Success {  get; set; }
        public T Result {  get; set; }      
        public string ErrorMessage { get; set; }

        public static GrainResponse<T> SuccessResponse(T t)
        {
            return new GrainResponse<T>
            {
                Result = t,
                Success = true
            };
        }

        public static GrainResponse<T> FailureResponse(T t, string errorMessage)
        {
            return new GrainResponse<T>
            {
                Result = t,
                ErrorMessage = errorMessage
            };
        }

        public static GrainResponse<T> FailureResponse(string errorMessage)
        {
            return new GrainResponse<T>
            {
                ErrorMessage = errorMessage
            };
        }
    }
}
