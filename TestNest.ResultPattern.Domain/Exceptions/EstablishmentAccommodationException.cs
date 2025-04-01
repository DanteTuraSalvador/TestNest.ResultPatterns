namespace TestNest.ResultPattern.Domain.Exceptions;
 public class EstablishmentAccommodationException : Exception
{
    public enum ErrorCode
    {
        InvalidAccommodationPrice
    }

    public ErrorCode Code { get; }

    public EstablishmentAccommodationException(ErrorCode code, string message) : base(message)
    {
        Code = code;
    }

    // You can add more specific exceptions here
    public static EstablishmentAccommodationException InvalidAccommodationPrice()
        => new EstablishmentAccommodationException(ErrorCode.InvalidAccommodationPrice, "Accommodation price is invalid.");
}