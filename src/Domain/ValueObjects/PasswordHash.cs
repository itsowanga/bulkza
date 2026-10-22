using System.Data;

namespace Domain.ValueObjects;
public readonly record struct PasswordHash
{
    public string Password {get;}

    private PasswordHash(string password)
    {
        if(string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Please enter a valid password.");
        }

        if(password.Length > 50)
        {
            throw new ArgumentException("Password length cannot be greater than 50.");
        }

        Password = password;
    }

    public static PasswordHash Create(string password) => new(password);
    public override string ToString() => "**********";
}