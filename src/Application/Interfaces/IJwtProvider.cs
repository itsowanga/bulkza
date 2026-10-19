using Domain;

namespace Application.Interfaces;

public interface IJwtProvider

{
    string GenerateToken(Merchant merchant);
}