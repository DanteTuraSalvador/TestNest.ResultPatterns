namespace TestNest.ResultPattern.Domain.Exceptions;
public sealed class CurrencyException : Exception
{
    public enum ErrorCode
    {
        InvalidCurrencyCode,
        InvalidCurrencySymbol,
        NullCurrency
    }

    private static readonly IReadOnlyDictionary<ErrorCode, string> ErrorMessages = new Dictionary<ErrorCode, string>
    {
        [ErrorCode.InvalidCurrencyCode] = "The currency code '{0}' is invalid. Valid codes are: {1}",
        [ErrorCode.InvalidCurrencySymbol] = "Currency symbol cannot be null or whitespace",
        [ErrorCode.NullCurrency] = "Currency instance cannot be null"
    };

    public ErrorCode Code { get; }

    private CurrencyException(ErrorCode code, string message)
        : base(message)
    {
        Code = code;
    }

    private CurrencyException(ErrorCode code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }

    public static CurrencyException InvalidCurrencyCode(string? code, IEnumerable<string> validCodes)
    {
        if (validCodes == null)
            throw new ArgumentNullException(nameof(validCodes));

        var validCodesList = validCodes.ToList();
        var formattedMessage = string.Format(
            ErrorMessages[ErrorCode.InvalidCurrencyCode],
            code ?? "null",
            string.Join(", ", validCodesList)
        );

        return new CurrencyException(ErrorCode.InvalidCurrencyCode, formattedMessage);
    }

    public static CurrencyException InvalidCurrencySymbol()
        => new(ErrorCode.InvalidCurrencySymbol, ErrorMessages[ErrorCode.InvalidCurrencySymbol]);

    public static CurrencyException NullCurrency()
        => new(ErrorCode.NullCurrency, ErrorMessages[ErrorCode.NullCurrency]);


}