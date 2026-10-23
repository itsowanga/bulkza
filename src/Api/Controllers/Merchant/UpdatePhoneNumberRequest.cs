using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Merchant;

public record UpdatePhoneNumberRequest([Required, MinLength(10)] string PhoneNumber);