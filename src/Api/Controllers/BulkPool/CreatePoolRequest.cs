using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.BulkPool;

public record CreatePoolRequest([Required]Guid ProductId, 
                                [Required] int MinimumQuantity, 
                                [Required] int MinimumMerchants, 
                                [Required] DateTimeOffset Deadline);