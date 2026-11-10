namespace __PROJECT__.Application.Common;

public static class PasswordPolicy
{
    public static string? Validate(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            return "Password must be at least 8 characters.";
        }

        if (!password.Any(char.IsUpper))
        {
            return "Password must contain an uppercase letter.";
        }

        if (!password.Any(char.IsLower))
        {
            return "Password must contain a lowercase letter.";
        }

        if (!password.Any(char.IsDigit))
        {
            return "Password must contain a number.";
        }

        if (password.All(char.IsLetterOrDigit))
        {
            return "Password must contain a special character.";
        }

        return null;
    }
}
