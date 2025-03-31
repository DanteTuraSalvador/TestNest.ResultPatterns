using TestNest.ResultPattern.Domain.Common;
using TestNest.ResultPattern.Domain.Exceptions;
using TestNest.ResultPattern.Domain.ValueObjects.Common;

namespace TestNest.ResultPattern.Domain.ValueObjects;
public sealed class Currency : ValueObject
{
    private static readonly HashSet<string> ValidCurrencyCodes = new() { "USD", "PHP", "EUR", "GBP", "JPY" };

    public static readonly Currency PHP = new("PHP", "₱");
    public static readonly Currency USD = new("USD", "$");
    public static readonly Currency EUR = new("EUR", "€");
    public static readonly Currency GBP = new("GBP", "£");
    public static readonly Currency JPY = new("JPY", "¥");

    public static Currency Default { get; } = PHP;
    private static readonly Lazy<Currency> _lazyEmpty = new(() => new Currency());
    public static Currency Empty => _lazyEmpty.Value;

    public string Code { get; }
    public string Symbol { get; }

    private Currency() => (Code, Symbol) = (string.Empty, string.Empty);
    private Currency(string code, string symbol) => (Code, Symbol) = (code, symbol);

    public static Result<Currency> Create(string code, string symbol)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3 || !ValidCurrencyCodes.Contains(code))
            return Result<Currency>.Failure(ErrorType.Validation, CurrencyException.InvalidCurrencyCode(code, ValidCurrencyCodes).Message);

        if (string.IsNullOrWhiteSpace(symbol))
            return Result<Currency>.Failure(ErrorType.Validation, CurrencyException.InvalidCurrencySymbol().Message);

        return Result<Currency>.Success(new Currency(code, symbol));
    }

    public static Result<Currency> Parse(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result<Currency>.Failure(ErrorType.Validation, "Currency code cannot be empty.");

        return code.ToUpperInvariant() switch
        {
            "PHP" => Result<Currency>.Success(PHP),
            "USD" => Result<Currency>.Success(USD),
            "EUR" => Result<Currency>.Success(EUR),
            "GBP" => Result<Currency>.Success(GBP),
            "JPY" => Result<Currency>.Success(JPY),
            _ => Result<Currency>.Failure(ErrorType.Validation, CurrencyException.InvalidCurrencyCode(code, ValidCurrencyCodes).Message)
        };
    }

    public Result<Currency> WithUpdatedSymbol(string newSymbol)
    {
        if (string.IsNullOrWhiteSpace(newSymbol))
            return Result<Currency>.Failure(ErrorType.Validation, CurrencyException.InvalidCurrencySymbol().Message);

        return Result<Currency>.Success(new Currency(this.Code, newSymbol));
    }

    public bool IsEmpty() => this == Empty;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Code;
        yield return Symbol;
    }

    public override string ToString() => IsEmpty() ? "[Empty Currency]" : $"{Symbol} ({Code})";

    public static IReadOnlyCollection<string> GetValidCurrencyCodes() => ValidCurrencyCodes.ToList().AsReadOnly();
}
