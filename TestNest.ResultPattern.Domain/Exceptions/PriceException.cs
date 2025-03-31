namespace TestNest.ResultPattern.Domain.Exceptions;
public sealed class PriceException : Exception
{
    public enum ErrorCode
    {
        NegativeStandardPrice,
        NegativePeakPrice,
        NegativeCleaningFee,
        InvalidCurrencyCode,
        EmptyCurrencySymbol,
        NullCurrency,
        NullPrice,
        PeakBelowStandard,
    }

    private static readonly Dictionary<ErrorCode, string> ErrorMessages = new()
            {
                { ErrorCode.NegativeStandardPrice, "Standard price cannot be negative." },
                { ErrorCode.NegativePeakPrice, "Peak price cannot be negative." },
                { ErrorCode.NegativeCleaningFee, "Cleaning fee cannot be negative." },
                { ErrorCode.InvalidCurrencyCode, "Invalid currency code." },
                { ErrorCode.EmptyCurrencySymbol, "Currency symbol cannot be empty." },
                { ErrorCode.NullCurrency, "Currency cannot be null." },
                { ErrorCode.NullPrice, "Price cannot be null." },
                { ErrorCode.PeakBelowStandard, "Peak price cannot be less than Standard Price." }
            };

    public ErrorCode Code { get; }

    // Constructor accepting the error code
    public PriceException(ErrorCode code)
        : base(ErrorMessages[code])
    {
        Code = code;
    }

    // Constructor accepting the error code and custom message
    public PriceException(ErrorCode code, string message)
        : base(message)
    {
        Code = code;
    }

    // Static helper methods for each error case
    public static PriceException NegativeStandardPrice()
        => new PriceException(ErrorCode.NegativeStandardPrice);

    public static PriceException NegativePeakPrice()
        => new PriceException(ErrorCode.NegativePeakPrice);

    public static PriceException NegativeCleaningFee()
        => new PriceException(ErrorCode.NegativeCleaningFee);

    public static PriceException InvalidCurrency(string code, IEnumerable<string> validCodes)
        => new PriceException(
            ErrorCode.InvalidCurrencyCode,
            $"Invalid currency code: {code}. Allowed values: {string.Join(", ", validCodes)}."
        );

    public static PriceException EmptyCurrencySymbol()
        => new PriceException(ErrorCode.EmptyCurrencySymbol);

    public static PriceException NullCurrency()
        => new PriceException(ErrorCode.NullCurrency);

    public static PriceException NullPrice()
        => new PriceException(ErrorCode.NullPrice);

    public static PriceException PeakBelowStandard()
 => new PriceException(ErrorCode.PeakBelowStandard);
}