using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.BulkPools;

public record CreatePoolRequest([Required]Guid ProductId, 
                                [Required] int MinimumQuantity, 
                                [Required] int MinimumMerchants, 
                                [Required] DateTimeOffset Deadline);