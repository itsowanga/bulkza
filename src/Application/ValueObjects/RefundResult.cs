namespace Application.ValueObjects;

public readonly record struct RefundResult 
{
    public bool Success {get;}
    public Guid TransactionId {get;}
    public string? ErrorMessage {get;}

    private RefundResult(bool success, Guid transactionId, string? errorMessage=null)
    {
        Success = success;
        ErrorMessage = errorMessage;
        TransactionId = transactionId;
    }

    public static RefundResult Succeeded(Guid transactionId) => new(true, transactionId);
    public static RefundResult Failed(string errorMessage) => new(false, default, errorMessage:errorMessage);
    public override string ToString() => $"{(Success ? "Succeeded" : "Failed")}: {TransactionId.ToString() ?? ErrorMessage}";
    
}
