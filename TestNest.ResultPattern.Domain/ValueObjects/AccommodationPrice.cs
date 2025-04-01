using TestNest.ResultPattern.Domain.Common;
using TestNest.ResultPattern.Domain.Exceptions;
using TestNest.ResultPattern.Domain.ValueObjects;
using TestNest.ResultPattern.Domain.ValueObjects.Common;

namespace TestNest.Domain.ValueObjects;

public sealed class AccommodationPrice : ValueObject
{
    public Price Price { get; }
    public decimal CleaningFee { get; }

    private static readonly Lazy<AccommodationPrice> _empty = new(() => new AccommodationPrice(Price.Empty, 0));
    private static readonly Lazy<AccommodationPrice> _zero = new(() => new AccommodationPrice(Price.Zero, 0));

    public static AccommodationPrice Empty => _empty.Value;
    public static AccommodationPrice Zero => _zero.Value;

    private AccommodationPrice() => (Price, CleaningFee) = (Price.Empty, 0);

    private AccommodationPrice(Price price, decimal cleaningFee)
        => (Price, CleaningFee) = (price, cleaningFee);

    //public static Result<AccommodationPrice> Create(decimal standardPrice, decimal peakPrice, decimal cleaningFee)
    //{
    //    var priceResult = Price.Create(standardPrice, peakPrice);
    //    return priceResult.IsSuccess
    //        ? Create(priceResult.Value!, cleaningFee)
    //        : Result<AccommodationPrice>.Failure(priceResult.ErrorType, priceResult.Errors);
    //}
    public static Result<AccommodationPrice> Create(decimal standardPrice, decimal peakPrice, decimal cleaningFee)
        => Price.Create(standardPrice, peakPrice).Bind(price => Create(price, cleaningFee));

    public static Result<AccommodationPrice> Create(Price? price, decimal cleaningFee)
    {
        var errors = new List<Error>();
        
        if (price == null)
        {
            var exception = AccommodationPriceException.NullPrice();
            errors.Add(new Error(exception.Code.ToString(), exception.Message.ToString()));
        }
        if (price == Price.Empty)
        {
            var exception = AccommodationPriceException.NullPrice();
            errors.Add(new Error(exception.Code.ToString(), exception.Message.ToString()));
        }
        if (cleaningFee < 0)
        {
            var exception = AccommodationPriceException.NegativeCleaningFee();
            errors.Add(new Error(exception.Code.ToString(), exception.Message.ToString()));
        }

        if (errors.Any())
        {
            return Result<AccommodationPrice>.Failure(ErrorType.Validation, errors);
        }

        return Result<AccommodationPrice>.Success(new AccommodationPrice(price!, cleaningFee));
    }

    public static Result<AccommodationPrice> Create(Result<Price> priceResult, decimal cleaningFee)
        => !priceResult.IsSuccess ? Result<AccommodationPrice>.Failure(priceResult.ErrorType, priceResult.Errors)
            : Create(priceResult.Value!, cleaningFee);

    //public Result<AccommodationPrice> WithPrice(Price newPrice)
    //    => newPrice is null ? Result<AccommodationPrice>.Failure(ErrorType.Validation, new Error(AccommodationPriceException.NullPrice().Code.ToString(),
    //                                                                                             AccommodationPriceException.NullPrice().Message))
    //        : Create(newPrice, CleaningFee);

    public Result<AccommodationPrice> WithPrice(Price newPrice)
    {
        if (this == Empty)
        {
            var exception = AccommodationPriceException.CannotModifyEmpty();
            return Result<AccommodationPrice>.Failure(ErrorType.Validation, new Error(exception.Code.ToString(), exception.Message));
        }

        if (newPrice is null)
        {
            var exception = AccommodationPriceException.NullPrice();
            return Result<AccommodationPrice>.Failure(ErrorType.Validation, new Error(exception.Code.ToString(), exception.Message));
        }

        return Create(newPrice, CleaningFee);
    }

    public Result<AccommodationPrice> WithPrice(Result<Price> newPriceResult)
        => newPriceResult.IsSuccess ? Create(newPriceResult.Value!, CleaningFee)
            : Result<AccommodationPrice>.Failure(newPriceResult.ErrorType, newPriceResult.Errors);

    //public Result<AccommodationPrice> WithCleaningFee(decimal newCleaningFee)
    //    => newCleaningFee < 0 ? Result<AccommodationPrice>.Failure(ErrorType.Validation, new Error(AccommodationPriceException.NegativeCleaningFee().Code.ToString(),
    //                                                                                               AccommodationPriceException.NegativeCleaningFee().Message))
    //        : Create(Price, newCleaningFee);

    public Result<AccommodationPrice> WithCleaningFee(decimal newCleaningFee)
    {
        if (this == Empty)
        {
            var exception = AccommodationPriceException.CannotModifyEmpty();
            return Result<AccommodationPrice>.Failure(ErrorType.Validation, new Error(exception.Code.ToString(), exception.Message));
        }

        if (newCleaningFee < 0)
        {
            var exception = AccommodationPriceException.NegativeCleaningFee();
            return Result<AccommodationPrice>.Failure(ErrorType.Validation, new Error(exception.Code.ToString(), exception.Message));
        }

        return Create(Price, newCleaningFee);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Price;
        yield return CleaningFee;
    }

    public override string ToString() =>
        $"{Price} + Cleaning Fee: {Price.Currency.Symbol}{CleaningFee:F2}";
}