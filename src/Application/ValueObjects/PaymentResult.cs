namespace Application.ValueObjects;
public readonly record struct PaymentResult
{
    public bool Success {get;}
    public string? TransactionId {get;}
    public string? ErrorMessage {get;}
    private PaymentResult(bool success, string? transactionId, string? errorMessage=null)
    {
        Success = success;
        TransactionId = transactionId;
        ErrorMessage = errorMessage;
    }

    public static PaymentResult Succeeded(string transactionId) => new(true, transactionId);
    public static PaymentResult Failed(string errorMessage) => new(false, null, errorMessage);
    public override string ToString() => $"{(Success ? "Succeeded" : "Failed")}: {TransactionId ?? ErrorMessage}";
}