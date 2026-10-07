namespace Domain;
public class BulkPool
{
    public PoolId PoolId {get; private set;}
    public ProductId ProductId {get; private set;}
    public BulkPoolStatus Status {get; private set;}
    public Quantity CurrentQuantity {get; private set;}
    public Quantity MinimumQuantity {get; private set;}
    public int MinimumMerchants {get; private set;}
    public DateTimeOffset Deadline {get; private set;}
    public DateTimeOffset CreatedAt {get; private set;}
    public IReadOnlyList<Participant> Participants {get; private set;}

    public BulkPool( PoolId poolId,
                     ProductId productId,
                     BulkPoolStatus status,
                     Quantity currentQuantity,
                     Quantity minimumQuantity,
                     int minimumMerchants,
                     DateTimeOffset deadline,
                     IReadOnlyList<Participant> participants)
    {
        
        if(!Enum.IsDefined(status))
        {
            throw new ArgumentException("Invalid bulk pool status.");
        }

        ValidateDeadline(deadline);
        ValidateMerchants(minimumMerchants);
        ValidateParticipants(participants, currentQuantity.Value);

        PoolId = poolId;
        ProductId = productId;
        Status = status;
        CurrentQuantity = currentQuantity;
        
        MinimumQuantity = minimumQuantity;
        MinimumMerchants = minimumMerchants;
        Deadline = deadline;
        CreatedAt = DateTimeOffset.UtcNow;
        Participants = participants;
        
    }

    private static void ValidateDeadline(DateTimeOffset date)
    {
        if (date.CompareTo(DateTimeOffset.UtcNow) != 1)
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

    private static void ValidateParticipants(IReadOnlyList<Participant> participants, int currentQuantity)
    {
        HashSet<MerchantId> merchantIdList = [];
        int quantitySum= 0;

        ArgumentNullException.ThrowIfNull(participants);

        foreach (var participant in participants)
        {
           
            if (participant.Equals(default(Participant)))
            {
                throw new ArgumentException($"{nameof(participants)} cannot contain a default participant.");
            }

            MerchantId merchantId = participant.MerchantId;

            if (merchantIdList.Contains(merchantId))
            {
                throw new ArgumentException($"{nameof(participants)} contains duplicate merchant ids.");
            }
            merchantIdList.Add(merchantId);
            quantitySum += participant.Quantity.Value;
        }


        if (quantitySum != currentQuantity)
        {
            throw new ArgumentException("Sum of participant quantities must equal current quantity.", nameof(participants));
        }
    }
}