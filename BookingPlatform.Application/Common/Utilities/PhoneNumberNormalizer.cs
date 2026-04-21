namespace BookingPlatform.Application.Common.Utilities;

public static class PhoneNumberNormalizer
{
    public static string Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return string.Empty;
        }

        var digits = phone.Where(char.IsDigit).ToArray();

        return new string(digits);
    }
}