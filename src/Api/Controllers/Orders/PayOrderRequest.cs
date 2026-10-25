using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Orders;

public record PayOrderRequest([Required, MinLength(1)] string CardToken);