using Domain.Enums;
using Domain.ValueObjects;

namespace Domain;
public class BulkPool
{
    public PoolId PoolId {get; private set;}
    public ProductId ProductId {get; private set;}
    public BulkPoolStatus Status {get; private set;}
    public int CurrentQuantity {get; private set;}
    public Quantity MinimumQuantity {get; private set;}
    public int MinimumMerchants {get; private set;}
    public DateTimeOffset Deadline {get; private set;}
    public DateTimeOffset CreatedAt {get; private set;}
    private List<Participant> _participants;

    public BulkPool( PoolId poolId,
                     ProductId productId,
                     Quantity minimumQuantity,
                     int minimumMerchants,
                     DateTimeOffset deadline
                     )
    {
                
        PoolId = poolId;
        ProductId = productId;        
        MinimumQuantity = minimumQuantity;
        MinimumMerchants = minimumMerchants;
        Deadline = deadline;
        CreatedAt = DateTimeOffset.UtcNow;
        _participants = new List<Participant>();
        CurrentQuantity = 0;
        Status = BulkPoolStatus.Open;
        ValidateDeadline(deadline);
        ValidateMerchants(minimumMerchants);
        
    }

    private void ValidateDeadline(DateTimeOffset date)
    {
        if (date <= CreatedAt)
        {
            throw new ArgumentException("Deadline must be into the future.");
        }

    }

    private static void ValidateMerchants(int minimumMerchants)
    {
        if (minimumMerchants < 1)
        {
            throw new ArgumentException("Minimum number of merchants should be at least one.");
        }
    }


    public IReadOnlyList<Participant> Participants => _participants;
    public void Join (MerchantId merchantId, Quantity quantity)
    {
        if (Status != BulkPoolStatus.Open)
        {
            throw new InvalidOperationException("Pool is not open for joining.");
        }

        if (Deadline <= DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("Pool's deadline has passed.");
        }

    
        foreach (var participant in _participants)
        {
            if(participant.MerchantId == merchantId)
            {
                throw new InvalidOperationException("Participant has already joined the pool.");
            }
        }

        _participants.Add(Participant.Create(merchantId, quantity));
        CurrentQuantity += quantity.Value;

        if (CurrentQuantity >= MinimumQuantity.Value && _participants.Count >= MinimumMerchants)
        {
            Status = BulkPoolStatus.ThresholdMet;
        }
        
    }

    public void Leave(MerchantId merchantId)
    {
        if (Status != BulkPoolStatus.Open && Status != BulkPoolStatus.ThresholdMet)
        {
            throw new InvalidOperationException("Pool cannot be left.");
        }

        int quantity = 0;
        Participant foundParticipant = default;

        foreach (var participant in _participants)
        {
            if(participant.MerchantId == merchantId)
            {
                quantity = participant.Quantity.Value;
                foundParticipant = participant;
                break;
            }
        }   

        if (foundParticipant == default)
        {
            throw new InvalidOperationException("Participant is not joined in this pool.");
        }

        _participants.Remove(foundParticipant);

         CurrentQuantity -= quantity;

         if (CurrentQuantity < MinimumQuantity.Value || _participants.Count < MinimumMerchants)
        {
            Status = BulkPoolStatus.Open;
        }
        }

   

    public void Lock()
    {
        if(Status != BulkPoolStatus.ThresholdMet)
        {
            throw new InvalidOperationException("Pool can only be locked once its threshold has been met.");

        }

        Status = BulkPoolStatus.Locked;

    }

    public void Finalize()
    {
        if (Status != BulkPoolStatus.Locked)
        {
            throw new InvalidOperationException("Pool can only be finalized once it has been locked.");
        }

        Status = BulkPoolStatus.Finalized;
    }

    public void Expire()
    {
        if (DateTimeOffset.UtcNow <= Deadline || (Status != BulkPoolStatus.Open && Status != BulkPoolStatus.ThresholdMet))
        {
            throw new InvalidOperationException("Pool needs to be open or threshold met state to expire.");

        }

        Status = BulkPoolStatus.Expired;
    }

        public void Cancel()
    {
        if (Status != BulkPoolStatus.Open && Status != BulkPoolStatus.ThresholdMet)
        {
            throw new InvalidOperationException("Pool needs to be open or threshold met state to be cancelled.");

        }

        Status = BulkPoolStatus.Cancelled;
    }

 }