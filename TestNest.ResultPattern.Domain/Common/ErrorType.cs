namespace TestNest.ResultPattern.Domain.Common;

    public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Unauthorized,
    Conflict,
    Internal,
    Aggregate,
         Invalid  // Add this if needed
}