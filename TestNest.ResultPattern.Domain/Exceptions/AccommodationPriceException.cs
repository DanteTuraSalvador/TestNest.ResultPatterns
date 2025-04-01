namespace TestNest.ResultPattern.Domain.Exceptions
{
    public class AccommodationPriceException : Exception
    {
        public enum ErrorCode
        {
            NegativeCleaningFee,
            NullPrice,
            CannotModifyEmpty // New Error Code
        }

        public ErrorCode Code { get; }

        public AccommodationPriceException(ErrorCode code, string message) : base(message)
        {
            Code = code;
        }

        public static AccommodationPriceException NegativeCleaningFee()
            => new AccommodationPriceException(ErrorCode.NegativeCleaningFee, "Cleaning fee cannot be negative.");

        public static AccommodationPriceException NullPrice()
            => new AccommodationPriceException(ErrorCode.NullPrice, "Price cannot be null.");

        public static AccommodationPriceException CannotModifyEmpty()
                => new AccommodationPriceException(ErrorCode.CannotModifyEmpty, "Cannot modify an empty AccommodationPrice."); // New Exception
    }
   

}
