namespace Application.ValueObjects;

public readonly record struct RefundResult 
{
    public bool Success {get;}
    public string? ErrorMessage {get;}

    private RefundResult(bool success, string? errorMessage=null)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    public static RefundResult Succeeded() => new(true);
    public static RefundResult Failed(string errorMessage) => new(false, errorMessage);
    public override string ToString() => $"{(Success ? "Succeeded" : "Failed:")}";
    
}
