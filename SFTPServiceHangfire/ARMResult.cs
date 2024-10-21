public class ARMResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }

    // Constructor that takes two arguments
    public ARMResult(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }
}
