namespace Domain.Enums;
public enum BulkPoolStatus
{
    Open, 
    ThresholdMet, 
    Locked, 
    Finalized, 
    Expired, 
    Cancelled
}