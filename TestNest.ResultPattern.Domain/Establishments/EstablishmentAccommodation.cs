using TestNest.Domain.ValueObjects;
using TestNest.ResultPattern.Domain.Common;
using TestNest.ResultPattern.Domain.Exceptions;

public sealed class EstablishmentAccommodation
{
    public AccommodationPrice Price { get; }

    private static readonly Lazy<EstablishmentAccommodation> _empty = new(() => new EstablishmentAccommodation());
    public static EstablishmentAccommodation Empty() => _empty.Value;

    private EstablishmentAccommodation() => Price = AccommodationPrice.Empty;

    private EstablishmentAccommodation(AccommodationPrice price) => Price = price;

    // Create method accepts a Result<AccommodationPrice>
    public static Result<EstablishmentAccommodation> Create(Result<AccommodationPrice> priceResult)
    {
        if (priceResult.IsSuccess)
        {
            return Result<EstablishmentAccommodation>.Success(new EstablishmentAccommodation(priceResult.Value!));
        }

        return Result<EstablishmentAccommodation>.Failure(priceResult.ErrorType, priceResult.Errors);
    }

    // The method to create EstablishmentAccommodation directly from AccommodationPrice
    public static Result<EstablishmentAccommodation> Create(AccommodationPrice price)
    {
        // Handle validation for price directly in the entity
        if (price == AccommodationPrice.Empty)
        {
            return Result<EstablishmentAccommodation>.Failure(ErrorType.Validation, new Error(EstablishmentAccommodationException.InvalidAccommodationPrice().Code.ToString(),
                                                                                          EstablishmentAccommodationException.InvalidAccommodationPrice().Message));
        }

        return Result<EstablishmentAccommodation>.Success(new EstablishmentAccommodation(price));
    }

    // Update the price with an AccommodationPrice directly
    public Result<EstablishmentAccommodation> UpdatePrice(AccommodationPrice newPrice)
    {
        if (newPrice == AccommodationPrice.Empty)
        {
            return Result<EstablishmentAccommodation>.Failure(ErrorType.Validation, new Error(EstablishmentAccommodationException.InvalidAccommodationPrice().Code.ToString(),
                                                                                        EstablishmentAccommodationException.InvalidAccommodationPrice().Message));
        }

        return Result<EstablishmentAccommodation>.Success(new EstablishmentAccommodation(newPrice));
    }

    // Update price with a Result<AccommodationPrice>
    public Result<EstablishmentAccommodation> UpdatePrice(Result<AccommodationPrice> newPriceResult)
    {
        if (newPriceResult.IsSuccess)
        {
            return Result<EstablishmentAccommodation>.Success(new EstablishmentAccommodation(newPriceResult.Value!));
        }

        return Result<EstablishmentAccommodation>.Failure(newPriceResult.ErrorType, newPriceResult.Errors);
    }
}
