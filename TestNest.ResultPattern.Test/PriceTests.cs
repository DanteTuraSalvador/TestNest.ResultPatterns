using FluentAssertions;
using TestNest.ResultPattern.Domain.Common;
using TestNest.ResultPattern.Domain.Exceptions;
using TestNest.ResultPattern.Domain.ValueObjects;
using Xunit;

namespace TestNest.Tests.Domain.ValueObjects
{
    public class PriceTests
    {
        [Fact]
        public void Price_ToString_ShouldReturnFormattedString()
        {
            // Arrange
            var price = Price.Create(100, 150).Value;

            // Act
            var result = price.ToString();

            // Assert
            result.Should().Be("₱100.00 / ₱150.00 (Peak)");
        }

        [Fact]
        public void Price_ShouldBeImmutable()
        {
            // Arrange
            var price = Price.Create(100, 150).Value;

            // Act
            var newPriceResult = price.WithStandardPrice(120);

            // Assert
            newPriceResult.IsSuccess.Should().BeTrue(); // Ensure operation succeeded
            var newPrice = newPriceResult.Value;

            newPrice.Should().NotBeSameAs(price);
            price.StandardPrice.Should().Be(100); // Original price remains unchanged
            newPrice.StandardPrice.Should().Be(120);
        }


        [Fact]
        public void Price_InstancesWithSameValues_ShouldBeEqual()
        {
            // Arrange
            var price1 = Price.Create(100, 150).Value;
            var price2 = Price.Create(100, 150).Value;

            // Assert
            price1.Should().Be(price2);
        }

        [Fact]
        public void Price_InstancesWithDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            var price1 = Price.Create(100, 150).Value;
            var price2 = Price.Create(200, 250).Value;

            // Assert
            price1.Should().NotBe(price2);
        }


        [Fact]
        public void Price_Zero_ShouldHaveZeroValues()
        {
            // Act
            var zeroPrice = Price.Zero;

            // Assert
            zeroPrice.StandardPrice.Should().Be(0);
            zeroPrice.PeakPrice.Should().Be(0);
        }

        [Fact]
        public void Price_Empty_ShouldHaveZeroValues()
        {
            // Act
            var emptyPrice = Price.Empty;

            // Assert
            emptyPrice.StandardPrice.Should().Be(0);
            emptyPrice.PeakPrice.Should().Be(0);
        }


        [Fact]
        public void Create_ShouldSucceed_WhenPricesAreValid()
        {
            // Arrange
            decimal standardPrice = 100;
            decimal peakPrice = 150;

            // Act
            var result = Price.Create(standardPrice, peakPrice);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.StandardPrice.Should().Be(standardPrice);
            result.Value.PeakPrice.Should().Be(peakPrice);
        }

        [Fact]
        public void Create_ShouldFail_WhenStandardPriceIsNegative()
        {
            // Arrange
            decimal standardPrice = -50;
            decimal peakPrice = 100;

            // Act
            var result = Price.Create(standardPrice, peakPrice);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(PriceException.NegativeStandardPrice().Message);
        }

        [Fact]
        public void Create_ShouldFail_WhenPeakPriceIsNegative()
        {
            // Arrange
            decimal standardPrice = 100;
            decimal peakPrice = -50;

            // Act
            var result = Price.Create(standardPrice, peakPrice);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(PriceException.NegativePeakPrice().Message);
        }

        [Fact]
        public void Create_ShouldFail_WhenPeakPriceIsLessThanStandardPrice()
        {
            // Arrange
            decimal standardPrice = 150;
            decimal peakPrice = 100;

            // Act
            var result = Price.Create(standardPrice, peakPrice);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(PriceException.PeakBelowStandard().Message);
        }

        [Fact]
        public void WithStandardPrice_ShouldReturnNewInstance_WhenStandardPriceIsUpdated()
        {
            // Arrange
            var price = Price.Create(100, 150).Value;

            // Act
            var result = price.WithStandardPrice(120);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.StandardPrice.Should().Be(120);
            result.Value.PeakPrice.Should().Be(150);
        }

        [Fact]
        public void WithStandardPrice_ShouldFail_WhenNewStandardPriceIsNegative()
        {
            // Arrange
            var price = Price.Create(100, 150).Value;

            // Act
            var result = price.WithStandardPrice(-10);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(PriceException.NegativeStandardPrice().Message);
        }

        [Fact]
        public void WithPeakPrice_ShouldReturnNewInstance_WhenPeakPriceIsUpdated()
        {
            // Arrange
            var price = Price.Create(100, 150).Value;

            // Act
            var result = price.WithPeakPrice(180);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.StandardPrice.Should().Be(100);
            result.Value.PeakPrice.Should().Be(180);
        }

        [Fact]
        public void WithPeakPrice_ShouldFail_WhenNewPeakPriceIsNegative()
        {
            // Arrange
            var price = Price.Create(100, 150).Value;

            // Act
            var result = price.WithPeakPrice(-20);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(PriceException.NegativePeakPrice().Message);
        }

        [Fact]
        public void WithPeakPrice_ShouldFail_WhenNewPeakPriceIsLessThanStandardPrice()
        {
            // Arrange
            var price = Price.Create(100, 150).Value;

            // Act
            var result = price.WithPeakPrice(80);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(PriceException.PeakBelowStandard().Message);
        }
    }
}
