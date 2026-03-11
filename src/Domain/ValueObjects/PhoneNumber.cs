using System.Text.RegularExpressions;

namespace Domain.ValueObjects;

public sealed record PhoneNumber
{
    private static readonly Regex PhoneRegex = new(@"^\+?\d{7,15}$", RegexOptions.Compiled);

    public string Value { get; }
    public string CountryCode { get; }
    public string NationalNumber { get; }
    public PhoneNumber() { }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Le numéro de téléphone ne peut pas être vide.", nameof(value));

        var cleaned = Regex.Replace(value, @"\D", "");

        string countryCode;
        string nationalNumber;

        if (value.StartsWith("+"))
        {
            countryCode = cleaned.Substring(0, Math.Min(3, cleaned.Length - 7));
            nationalNumber = cleaned.Substring(countryCode.Length);
        }
        else
        {
            countryCode = "1";
            nationalNumber = cleaned;
        }

        var fullValue = $"+{countryCode}{nationalNumber}";

        if (!PhoneRegex.IsMatch(fullValue))
            throw new ArgumentException("Numéro de téléphone invalide.", nameof(value));

        CountryCode = countryCode;
        NationalNumber = nationalNumber;
        Value = fullValue;
    }

    public PhoneNumber(string countryCode, string nationalNumber)
    {
        if (string.IsNullOrWhiteSpace(countryCode) || string.IsNullOrWhiteSpace(nationalNumber))
            throw new ArgumentException("Code pays et numéro national sont obligatoires.");

        countryCode = countryCode.StartsWith("+") ? countryCode[1..] : countryCode;
        nationalNumber = Regex.Replace(nationalNumber, @"\D", "");

        var fullValue = $"+{countryCode}{nationalNumber}";

        if (!PhoneRegex.IsMatch(fullValue))
            throw new ArgumentException("Numéro de téléphone invalide.");

        CountryCode = countryCode;
        NationalNumber = nationalNumber;
        Value = fullValue;
    }

    public string ToInternationalFormat() => $"+{CountryCode} {NationalNumber}";

    public override string ToString() => Value;
}