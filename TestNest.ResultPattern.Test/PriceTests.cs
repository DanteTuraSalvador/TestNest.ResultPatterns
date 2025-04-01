using TestNest.ResultPattern.Domain.Common;
using TestNest.ResultPattern.Domain.Exceptions;
using TestNest.ResultPattern.Domain.ValueObjects;

namespace TestNest.ResultPattern.Test
{
    public class PriceTests
    {
        private const decimal ValidStandardPrice = 100m;
        private const decimal ValidPeakPrice = 150m;

        #region Creation Tests

        [Fact]
        public void Create_WithValidPrices_ReturnsSuccessResult()
        {
            // Act
            var result = Price.Create(ValidStandardPrice, ValidPeakPrice);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ValidStandardPrice, result.Value!.StandardPrice);
            Assert.Equal(ValidPeakPrice, result.Value.PeakPrice);
        }

        [Fact]
        public void Create_WithNegativeStandardPrice_ReturnsFailureResult()
        {
            // Arrange
            const decimal negativeStandardPrice = -50m;

            // Act
            var result = Price.Create(negativeStandardPrice, ValidPeakPrice);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Single(result.Errors);
            Assert.Equal(
                PriceException.ErrorCode.NegativeStandardPrice.ToString(),
                result.Errors[0].Code);
        }

        [Fact]
        public void Create_WithNegativePeakPrice_ReturnsFailureResult()
        {
            // Arrange
            const decimal negativePeakPrice = -50m;

            // Act
            var result = Price.Create(ValidStandardPrice, negativePeakPrice);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Single(result.Errors);
            Assert.Equal(
                PriceException.ErrorCode.NegativePeakPrice.ToString(),
                result.Errors[0].Code);
        }

        [Fact]
        public void Create_WithPeakBelowStandard_ReturnsFailureResult()
        {
            // Arrange
            const decimal lowerPeakPrice = 50m;

            // Act
            var result = Price.Create(ValidStandardPrice, lowerPeakPrice);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Single(result.Errors);
            Assert.Equal(
                PriceException.ErrorCode.PeakBelowStandard.ToString(),
                result.Errors[0].Code);
        }

        [Fact]
        public void Create_WithAllThreeInvalidConditions_ReturnsAllThreeErrors()
        {
            // Arrange
            const decimal negativeStandardPrice = -50m;
            const decimal negativePeakPrice = -100m; // Also less than standard price

            // Act
            var result = Price.Create(negativeStandardPrice, negativePeakPrice);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(2, result.Errors.Count); // Ensure there are two errors
            Assert.Contains(result.Errors, e => e.Code == PriceException.ErrorCode.NegativeStandardPrice.ToString());
            Assert.Contains(result.Errors, e => e.Code == PriceException.ErrorCode.NegativePeakPrice.ToString());
        }

        [Fact]
        public void Create_WithValidStandardButInvalidPeak_ReturnsOnlyPeakErrors()
        {
            // Arrange
            const decimal validStandardPrice = 100m;
            const decimal negativePeakPrice = -50m;

            // Act
            var result = Price.Create(validStandardPrice, negativePeakPrice);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Single(result.Errors);
            Assert.Equal(PriceException.ErrorCode.NegativePeakPrice.ToString(), result.Errors[0].Code);
        }

        [Fact]
        public void Create_WithNegativeStandardPriceOnly_ReturnsSingleError()
        {
            var result = Price.Create(-50m, 100m);

            Assert.False(result.IsSuccess);
            Assert.Single(result.Errors);
            Assert.Equal(PriceException.ErrorCode.NegativeStandardPrice.ToString(),
                        result.Errors[0].Code);
        }

        [Fact]
        public void Create_WithNegativePeakPriceOnly_ReturnsSingleError()
        {
            var result = Price.Create(50m, -100m);

            Assert.False(result.IsSuccess);
            Assert.Single(result.Errors);
            Assert.Equal(PriceException.ErrorCode.NegativePeakPrice.ToString(),
                        result.Errors[0].Code);
        }

        [Fact]
        public void Create_WithMultipleInvalidPrices_ReturnsFailureWithAllErrors()
        {
            // Test case that triggers all three error conditions
            const decimal negativeStandardPrice = -50m;
            const decimal negativePeakPrice = -100m;

            var result = Price.Create(negativeStandardPrice, negativePeakPrice);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);

            // Verify we got exactly 2 errors
            Assert.Equal(2, result.Errors.Count);

            // Verify each specific error exists
            var errorCodes = result.Errors.Select(e => e.Code).ToList();
            Assert.Contains(PriceException.ErrorCode.NegativeStandardPrice.ToString(), errorCodes);
            Assert.Contains(PriceException.ErrorCode.NegativePeakPrice.ToString(), errorCodes);
        }

        #endregion

        #region Static Properties Tests

        [Fact]
        public void Empty_ReturnsPriceWithZeroValues()
        {
            // Act
            var price = Price.Empty;

            // Assert
            Assert.Equal(0m, price.StandardPrice);
            Assert.Equal(0m, price.PeakPrice);
        }

        [Fact]
        public void Zero_ReturnsPriceWithZeroValues()
        {
            // Act
            var price = Price.Zero;

            // Assert
            Assert.Equal(0m, price.StandardPrice);
            Assert.Equal(0m, price.PeakPrice);
        }

        #endregion

        #region With Methods Tests

        [Fact]
        public void WithStandardPrice_WithValidPrice_ReturnsSuccessResult()
        {
            // Arrange
            var originalPrice = Price.Create(ValidStandardPrice, ValidPeakPrice).Value;
            const decimal newStandardPrice = 120m;

            // Act
            var result = originalPrice!.WithStandardPrice(newStandardPrice);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(newStandardPrice, result.Value!.StandardPrice);
            Assert.Equal(ValidPeakPrice, result.Value.PeakPrice);
        }

        [Fact]
        public void WithStandardPrice_WithInvalidPrice_ReturnsFailureResult()
        {
            // Arrange
            var originalPrice = Price.Create(ValidStandardPrice, ValidPeakPrice).Value;
            const decimal invalidPrice = -50m;

            // Act
            var result = originalPrice!.WithStandardPrice(invalidPrice);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
        }

        [Fact]
        public void WithPeakPrice_WithValidPrice_ReturnsSuccessResult()
        {
            // Arrange
            var originalPrice = Price.Create(ValidStandardPrice, ValidPeakPrice).Value;
            const decimal newPeakPrice = 200m;

            // Act
            var result = originalPrice!.WithPeakPrice(newPeakPrice);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(ValidStandardPrice, result.Value!.StandardPrice);
            Assert.Equal(newPeakPrice, result.Value.PeakPrice);
        }

        [Fact]
        public void WithPeakPrice_WithInvalidPrice_ReturnsFailureResult()
        {
            // Arrange
            var originalPrice = Price.Create(ValidStandardPrice, ValidPeakPrice).Value;
            const decimal invalidPrice = -50m;

            // Act
            var result = originalPrice!.WithPeakPrice(invalidPrice);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
        }

        #endregion

        #region Value Object Behavior Tests

        [Fact]
        public void Equals_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var price1 = Price.Create(ValidStandardPrice, ValidPeakPrice).Value;
            var price2 = Price.Create(ValidStandardPrice, ValidPeakPrice).Value;

            // Act & Assert
            Assert.True(price1!.Equals(price2));
        }

        [Fact]
        public void Equals_WithDifferentValues_ReturnsFalse()
        {
            // Arrange
            var price1 = Price.Create(ValidStandardPrice, ValidPeakPrice).Value;
            var price2 = Price.Create(ValidStandardPrice + 10m, ValidPeakPrice).Value;

            // Act & Assert
            Assert.False(price1!.Equals(price2));
        }

        [Fact]
        public void GetHashCode_ForEqualObjects_ReturnsSameValue()
        {
            // Arrange
            var price1 = Price.Create(ValidStandardPrice, ValidPeakPrice).Value;
            var price2 = Price.Create(ValidStandardPrice, ValidPeakPrice).Value;

            // Act & Assert
            Assert.Equal(price1!.GetHashCode(), price2!.GetHashCode());
        }

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var price = Price.Create(ValidStandardPrice, ValidPeakPrice).Value;
            var expected = $"₱{ValidStandardPrice:F2} / ₱{ValidPeakPrice:F2} (Peak)";

            // Act & Assert
            Assert.Equal(expected, price!.ToString());
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Create_WithZeroPrices_ReturnsSuccessResult()
        {
            // Act
            var result = Price.Create(0m, 0m);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(0m, result.Value!.StandardPrice);
            Assert.Equal(0m, result.Value.PeakPrice);
        }

        [Fact]
        public void Create_WithEqualStandardAndPeakPrices_ReturnsSuccessResult()
        {
            // Arrange
            const decimal equalPrice = 100m;

            // Act
            var result = Price.Create(equalPrice, equalPrice);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(equalPrice, result.Value!.StandardPrice);
            Assert.Equal(equalPrice, result.Value.PeakPrice);
        }

        #endregion

        #region Bind & Map Tests

        [Fact]
        public void Bind_WithValidPrice_TransformsSuccessfully()
        {
            // Arrange
            var priceResult = Price.Create(100m, 150m);  // This is your Price value object

            // Act
            var totalPriceResult = priceResult
                .Bind(price => Result<decimal>.Success(price.StandardPrice + price.PeakPrice));  // Binding transformation

            // Assert
            Assert.True(totalPriceResult.IsSuccess);
            Assert.Equal(250m, totalPriceResult.Value);
        }

        [Fact]
        public void Map_WithValidPrice_TransformsSuccessfully()
        {
            // Arrange
            var priceResult = Price.Create(100m, 150m);  // This is your Price value object

            // Act
            var totalPriceResult = priceResult
                .Map(price => price.StandardPrice + price.PeakPrice);  // Mapping transformation

            // Assert
            Assert.True(totalPriceResult.IsSuccess);
            Assert.Equal(250m, totalPriceResult.Value);
        }

        #endregion
    }
}
