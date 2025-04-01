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
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(code) || code.Length != 3 || !ValidCurrencyCodes.Contains(code))
        {
            var exception = CurrencyException.InvalidCurrencyCode();
            errors.Add(new Error(exception.Code.ToString(), exception.Message));
        }

        if (string.IsNullOrWhiteSpace(symbol))
        {
            var exception = CurrencyException.InvalidCurrencySymbol();
            errors.Add(new Error(exception.Code.ToString(), exception.Message));
        }

        return errors.Any() ? Result<Currency>.Failure(ErrorType.Validation, errors)
            : Result<Currency>.Success(new Currency(code, symbol));
    }

    private static Currency? GetCurrencyByCode(string code) => code switch
    {
        "PHP" => PHP,
        "USD" => USD,
        "EUR" => EUR,
        "GBP" => GBP,
        "JPY" => JPY,
        _ => null
    };

    public static Result<Currency> Parse(string code)
    {
        var currency = GetCurrencyByCode(code.ToUpperInvariant());
        return string.IsNullOrWhiteSpace(code) || currency == null
            ? Result<Currency>.Failure(ErrorType.Validation,
                new Error(CurrencyException.InvalidCurrencyCode().Code.ToString(), CurrencyException.InvalidCurrencyCode().Message))
            : Result<Currency>.Success(currency!);
    }

    //public static Result<Currency> Parse(string code)
    //{
    //    if (string.IsNullOrWhiteSpace(code))
    //    {
    //        var exception = CurrencyException.InvalidCurrencyCode();
    //        return Result<Currency>.Failure(ErrorType.Validation,
    //            new Error(exception.Code.ToString(), exception.Message));
    //    }

    //    code = code.ToUpperInvariant();
    //    var currency = GetCurrencyByCode(code);

    //    if (currency == null)
    //    {
    //        var exception = CurrencyException.InvalidCurrencyCode();
    //        return Result<Currency>.Failure(ErrorType.Validation,
    //            new Error(exception.Code.ToString(), exception.Message));
    //    }

    //    return Result<Currency>.Success(currency);
    //}

    public Result<Currency> WithUpdatedSymbol(string newSymbol)
        => string.IsNullOrWhiteSpace(newSymbol) ? Result<Currency>.Failure(ErrorType.Validation, new Error(CurrencyException.InvalidCurrencySymbol().Code.ToString(), 
                                                                                                           CurrencyException.InvalidCurrencySymbol().Message))
            : Result<Currency>.Success(new Currency(this.Code, newSymbol));

    //public Result<Currency> WithUpdatedSymbol(string newSymbol)
    //{
    //    var errors = new List<Error>();

    //    if (string.IsNullOrWhiteSpace(newSymbol))
    //    {
    //        var exception = CurrencyException.InvalidCurrencySymbol();
    //        return Result<Currency>.Failure(ErrorType.Validation,
    //            new Error(exception.Code.ToString(), exception.Message)
    //        );
    //    }

    //    return Result<Currency>.Success(new Currency(this.Code, newSymbol));
    //}

    public bool IsEmpty() => this == Empty;

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Code;
        yield return Symbol;
    }

    public override string ToString() => IsEmpty() ? "[Empty Currency]" : $"{Symbol} ({Code})";

    public static IReadOnlyCollection<string> GetValidCurrencyCodes() => ValidCurrencyCodes.ToList().AsReadOnly();
}