using FluentAssertions;
using System.Text.RegularExpressions;
using TestNest.Domain.ValueObjects;
using TestNest.ResultPattern.Domain.Exceptions;
using TestNest.ResultPattern.Domain.ValueObjects;

namespace TestNest.ResultPattern.Test;
public class AccommodationPriceTests
{
    #region Creation Tests
    [Fact]
    public void Create_ValidParameters_ReturnsSuccess()
    {
        // Arrange
        var price = Price.Create(100m, 150m).Value!;
        const decimal cleaningFee = 50m;

        // Act
        var result = AccommodationPrice.Create(price, cleaningFee);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Price.Should().Be(price);
        result.Value.CleaningFee.Should().Be(cleaningFee);
    }

    [Theory]
    [MemberData(nameof(InvalidPriceTestData))]
    public void Create_InvalidPrice_ReturnsFailure(Price? price)
    {
        // Arrange
        const decimal cleaningFee = 50m;

        // Act
        var result = AccommodationPrice.Create(price, cleaningFee);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.Code == AccommodationPriceException.NullPrice().Code.ToString());
    }

    public static IEnumerable<object[]> InvalidPriceTestData()
    {
        yield return new object[] { null };
        yield return new object[] { Price.Empty };
    }

    [Fact]
    public void Create_NegativeCleaningFee_ReturnsFailure()
    {
        // Arrange
        var price = Price.Create(100m, 150m).Value!;
        const decimal invalidFee = -10m;

        // Act
        var result = AccommodationPrice.Create(price, invalidFee);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.Code == AccommodationPriceException.NegativeCleaningFee().Code.ToString());
    }

#endregion

    #region WithPrice Tests

    [Fact]
    public void WithPrice_ValidNewPrice_UpdatesSuccessfully()
    {
        // Arrange
        var original = AccommodationPrice.Create(100m, 150m, 50m).Value!;
        var newPrice = Price.Create(200m, 250m).Value!;

        // Act
        var result = original.WithPrice(newPrice);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Price.Should().Be(newPrice);
        result.Value.CleaningFee.Should().Be(original.CleaningFee);
    }

    [Fact]
    public void WithPrice_ResultFailure_PropagatesError()
    {
        // Arrange
        var original = AccommodationPrice.Create(100m, 150m, 50m).Value!;
        var invalidPriceResult = Price.Create(-10m, 150m);

        // Act
        var result = original.WithPrice(invalidPriceResult);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region WithCleaningFee Tests

    [Fact]
    public void WithCleaningFee_ValidFee_UpdatesSuccessfully()
    {
        // Arrange
        var original = AccommodationPrice.Create(100m, 150m, 50m).Value!;
        const decimal newFee = 75m;

        // Act
        var result = original.WithCleaningFee(newFee);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CleaningFee.Should().Be(newFee);
        result.Value.Price.Should().Be(original.Price);
    }

    [Fact]
    public void WithCleaningFee_NegativeFee_ReturnsFailure()
    {
        // Arrange
        var original = AccommodationPrice.Create(100m, 150m, 50m).Value!;
        const decimal invalidFee = -10m;

        // Act
        var result = original.WithCleaningFee(invalidFee);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region Static Properties & Formatting

    [Fact]
    public void Empty_HasCorrectDefaults()
    {
        var empty = AccommodationPrice.Empty;
        empty.Price.Should().Be(Price.Empty);
        empty.CleaningFee.Should().Be(0);
    }

    [Fact]
    public void Zero_HasCorrectDefaults()
    {
        var zero = AccommodationPrice.Zero;
        zero.Price.Should().Be(Price.Zero);
        zero.CleaningFee.Should().Be(0);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var price = AccommodationPrice.Create(100m, 150m, 50m).Value!;
        price.ToString().Should().Contain("+ Cleaning Fee:");
    }

    #endregion

    #region Additional Boundary Tests

    [Theory]
    [InlineData(0.01, 0.01, 0.01)]        // Minimum non-zero values
    [InlineData(1, 1, 0)]                 // Zero cleaning fee
    [InlineData(100, 150, 50)]            // Normal values
    [InlineData(999999.99, 999999.99, 0)] // Large values with zero fee
    public void Create_ValidEdgeCases_ReturnsSuccess(
     decimal standardPrice,
     decimal peakPrice,
     decimal cleaningFee)
    {
        var result = AccommodationPrice.Create(standardPrice, peakPrice, cleaningFee);
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(0, 0, 0)] // Zero prices should fail
    [InlineData(-0.01, 1, 1)] // Negative standard price
    [InlineData(1, -0.01, 1)] // Negative peak price
    public void Create_InvalidEdgeCases_ReturnsFailure(
        decimal standardPrice,
        decimal peakPrice,
        decimal cleaningFee)
    {
        var result = AccommodationPrice.Create(standardPrice, peakPrice, cleaningFee);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Create_UsingBindMethod_PropagatesErrors()
    {
        var result = AccommodationPrice.Create(-100m, 150m, 50m);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.Code == PriceException.ErrorCode.NegativeStandardPrice.ToString());
    }

    #endregion

    [Theory]
    [InlineData(100, 99, 50)] // Peak < Standard
    public void Create_PeakBelowStandard_ReturnsFailure(
    decimal standardPrice,
    decimal peakPrice,
    decimal cleaningFee)
    {
        var result = AccommodationPrice.Create(standardPrice, peakPrice, cleaningFee);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.Code == PriceException.ErrorCode.PeakBelowStandard.ToString());
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var price1 = AccommodationPrice.Create(100m, 150m, 50m).Value!;
        var price2 = AccommodationPrice.Create(100m, 150m, 50m).Value!;

        price1.Should().Be(price2);
        price1.GetHashCode().Should().Be(price2.GetHashCode());
    }

    [Fact]
    public void WithCleaningFee_ReturnsNewInstance()
    {
        var original = AccommodationPrice.Create(100m, 150m, 50m).Value!;
        var modified = original.WithCleaningFee(75m).Value!;

        modified.Should().NotBeSameAs(original);
        modified.CleaningFee.Should().Be(75m);
    }

    [Fact]
    public void Create_NegativeCleaningFee_ReturnsCorrectMessage()
    {
        var result = AccommodationPrice.Create(100m, 150m, -10m);

        result.Errors.Should().Contain(e =>
            e.Message == AccommodationPriceException.NegativeCleaningFee().Message);
    }

    [Fact]
    public void ToString_IncludesCurrencySymbol()
    {
        // Arrange
        var price = AccommodationPrice.Create(100m, 150m, 50m).Value!;
        var currencySymbol = price.Price.Currency.Symbol; // Get actual symbol

        // Escape the symbol for regex if needed (e.g., "+" or "$")
        var escapedSymbol = Regex.Escape(currencySymbol);

        // Build regex pattern dynamically
        var pattern = $@"{escapedSymbol}\d+\.\d{{2}} / {escapedSymbol}\d+\.\d{{2}} \(Peak\) \+ Cleaning Fee: {escapedSymbol}\d+\.\d{{2}}";

        // Act & Assert
        price.ToString().Should().MatchRegex(pattern);
    }

    [Fact]
    public void WithCleaningFee_OnEmpty_ReturnsFailure()
    {
        var result = AccommodationPrice.Empty.WithCleaningFee(50m);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void WithPrice_Null_ReturnsFailure()
    {
        var original = AccommodationPrice.Create(100m, 150m, 50m).Value!;
        var result = original.WithPrice(null);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Code == AccommodationPriceException.NullPrice().Code.ToString());
    }

    [Fact]
    public void WithCleaningFee_MinValue_ReturnsFailure()
    {
        var original = AccommodationPrice.Create(100m, 150m, 50m).Value!;
        var result = original.WithCleaningFee(decimal.MinValue);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Zero_ShouldRemainUnchanged()
    {
        var zero = AccommodationPrice.Zero;
        var modified = zero.WithCleaningFee(100m);

        zero.Should().BeSameAs(AccommodationPrice.Zero);
        modified.IsSuccess.Should().BeFalse(); // Should fail because Zero is immutable
    }

    [Fact]
    public void WithPrice_ZeroPrice_ReturnsFailure()
    {
        var original = AccommodationPrice.Create(100m, 150m, 50m).Value!;
        var result = original.WithPrice(Price.Zero);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        // Arrange
        var price = AccommodationPrice.Create(100m, 150m, 50m).Value!;
        var symbol = price.Price.Currency.Symbol;

        // Act
        var result = price.ToString();

        // Assert
        result.Should().Contain($"{symbol}100.00 / {symbol}150.00 (Peak)");
        result.Should().Contain($"+ Cleaning Fee: {symbol}50.00");
    }
    [Fact]
    public void WithPrice_OnEmpty_ReturnsFailure()
    {
        var result = AccommodationPrice.Empty.WithPrice(Price.Create(100m, 150m).Value!);
        result.IsSuccess.Should().BeFalse();
    }
}