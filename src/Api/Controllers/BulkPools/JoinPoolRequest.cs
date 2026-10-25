using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.BulkPools;

public record JoinPoolRequest([Required] int Quantity);