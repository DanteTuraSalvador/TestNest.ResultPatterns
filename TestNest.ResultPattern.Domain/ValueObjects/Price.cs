using TestNest.ResultPattern.Domain.Common;
using TestNest.ResultPattern.Domain.Exceptions;
using TestNest.ResultPattern.Domain.ValueObjects.Common;

namespace TestNest.ResultPattern.Domain.ValueObjects;
public sealed class Price : ValueObject
{
    private static readonly Lazy<Price> _lazyEmpty = new(() => new Price());
    private static readonly Lazy<Price> _lazyZero = new(() => new Price(0, 0));

    public static Price Empty => _lazyEmpty.Value;
    public static Price Zero => _lazyZero.Value;

    public decimal StandardPrice { get; }
    public decimal PeakPrice { get; }
    public Currency Currency => Currency.PHP; // Fixed to PHP

    private Price() => (StandardPrice, PeakPrice) = (0, 0);
    private Price(decimal standardPrice, decimal peakPrice) =>
        (StandardPrice, PeakPrice) = (standardPrice, peakPrice);

    public static Result<Price> Create(decimal standardPrice, decimal peakPrice)
    {
        if (standardPrice < 0) return Result<Price>.Failure(ErrorType.Validation, PriceException.NegativeStandardPrice().Message);
        if (peakPrice < 0) return Result<Price>.Failure(ErrorType.Validation, PriceException.NegativePeakPrice().Message);
        if (peakPrice < standardPrice) return Result<Price>.Failure(ErrorType.Validation, PriceException.PeakBelowStandard().Message);

        return Result<Price>.Success(new Price(standardPrice, peakPrice));
    }

    public Result<Price> WithStandardPrice(decimal newStandardPrice) => Create(newStandardPrice, PeakPrice);
    public Result<Price> WithPeakPrice(decimal newPeakPrice) => Create(StandardPrice, newPeakPrice);

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return StandardPrice;
        yield return PeakPrice;
        yield return Currency;
    }

    public override string ToString() => $"{Currency.Symbol}{StandardPrice:F2} / {Currency.Symbol}{PeakPrice:F2} (Peak)";
}