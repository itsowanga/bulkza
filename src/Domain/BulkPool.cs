using Domain.Enums;
using Domain.Events;
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
    private readonly List<Participant> _participants;
    private readonly List<IDomainEvent> _domainEvents;
    public BulkPool( PoolId poolId,
                     ProductId productId,
                     Quantity minimumQuantity,
                     int minimumMerchants,
                     DateTimeOffset deadline
                     )
    {
        if (poolId == default)
        {
            throw new ArgumentException($"{nameof(poolId)} cannot be the default value.", nameof(poolId));
        }

        if (productId == default)
        {
            throw new ArgumentException($"{nameof(productId)} cannot be the default value.", nameof(productId));
        }

        if (minimumQuantity == default)
        {
            throw new ArgumentException($"{nameof(minimumQuantity)} cannot be the default value.", nameof(minimumQuantity));
        }
        
        CreatedAt = DateTimeOffset.UtcNow;
        ValidateDeadline(deadline);
        ValidateMerchants(minimumMerchants);
        PoolId = poolId;
        ProductId = productId;        
        MinimumQuantity = minimumQuantity;
        MinimumMerchants = minimumMerchants;
        Deadline = deadline;
        _participants = [];
        CurrentQuantity = 0;
        Status = BulkPoolStatus.Open;
        _domainEvents = [];
        
    }

    private void ValidateDeadline(DateTimeOffset deadline)
    {
        if (deadline <= CreatedAt)
        {
            throw new ArgumentException($"{nameof(deadline)} must be into the future.", nameof(deadline));
        }

    }

    private static void ValidateMerchants(int minimumMerchants)
    {
        if (minimumMerchants < 1)
        {
            throw new ArgumentException($"{nameof(minimumMerchants)} number of merchants should be at least one.", nameof(minimumMerchants));
        }
    }


    public IReadOnlyList<Participant> Participants => _participants;
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;
    public void ClearDomainEvents() => _domainEvents.Clear();
    public void Join (MerchantId merchantId, Quantity quantity)
    {
        if (Status != BulkPoolStatus.Open)
        {
            throw new InvalidOperationException($"{nameof(Status)} is not open.");
        }

        if (Deadline <= DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException($"{nameof(Deadline)} has passed.");
        }

    
        foreach (var participant in _participants)
        {
            if(participant.MerchantId == merchantId)
            {
                throw new InvalidOperationException($"{nameof(participant)} has already joined the pool.");
            }
        }

        _participants.Add(Participant.Create(merchantId, quantity));
        CurrentQuantity += quantity.Value;
        _domainEvents.Add(new MerchantJoinedPool(EventId.New(), DateTimeOffset.UtcNow, PoolId, merchantId, quantity));

        if (CurrentQuantity >= MinimumQuantity.Value && _participants.Count >= MinimumMerchants)
        {
            Status = BulkPoolStatus.ThresholdMet;
            _domainEvents.Add(new PoolThresholdReached(EventId.New(), DateTimeOffset.UtcNow, PoolId, CurrentQuantity, _participants.Count));
        }
        
    }

    public void Leave(MerchantId merchantId)
    {
        if ((Status != BulkPoolStatus.Open && Status != BulkPoolStatus.ThresholdMet) || DateTimeOffset.UtcNow > Deadline)
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
        _domainEvents.Add(new MerchantLeftPool(EventId.New(), DateTimeOffset.UtcNow, PoolId, merchantId, Quantity.Create(quantity)));

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
        _domainEvents.Add(new PoolLocked(EventId.New(), DateTimeOffset.UtcNow, PoolId));

    }

    public void Finalise()
    {
        if (Status != BulkPoolStatus.Locked)
        {
            throw new InvalidOperationException("Pool can only be finalized once it has been locked.");
        }

        Status = BulkPoolStatus.Finalized;
        _domainEvents.Add(new PoolFinalized(EventId.New(), DateTimeOffset.UtcNow, PoolId));
    }

    public void Expire()
    {
        if (DateTimeOffset.UtcNow <= Deadline || (Status != BulkPoolStatus.Open && Status != BulkPoolStatus.ThresholdMet))
        {
            throw new InvalidOperationException("Pool needs to be open or threshold met state to expire.");
            
        }

        Status = BulkPoolStatus.Expired;
        _domainEvents.Add(new PoolExpired(EventId.New(), DateTimeOffset.UtcNow, PoolId));
    }

        public void Cancel()
    {
        if (Status != BulkPoolStatus.Open && Status != BulkPoolStatus.ThresholdMet)
        {
            throw new InvalidOperationException("Pool needs to be open or threshold met state to be cancelled.");
            
        }

        Status = BulkPoolStatus.Cancelled;
        _domainEvents.Add(new PoolCancelled(EventId.New(), DateTimeOffset.UtcNow, PoolId));

    }

 }