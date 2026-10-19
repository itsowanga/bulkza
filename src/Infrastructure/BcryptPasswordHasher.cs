using Application.Interfaces;

public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string plainPassword) =>
        BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: 12);

    public bool Verify(string plainPassword, string storedHash) =>
        BCrypt.Net.BCrypt.Verify(plainPassword, storedHash);
}