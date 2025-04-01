using Xunit;
using FluentAssertions;
using TestNest.ResultPattern.Domain.Exceptions;
using TestNest.Domain.ValueObjects;
using TestNest.ResultPattern.Domain.Common;
using TestNest.ResultPattern.Domain.ValueObjects;

namespace TestNest.ResultPattern.Tests
{
    public class EstablishmentAccommodationTests
    {
        // Test for successful creation of EstablishmentAccommodation with valid price
        [Fact]
        public void Create_WithValidPrice_ReturnsSuccess()
        {
            // Arrange
            var validPrice = AccommodationPrice.Create(100m, 150m, 50m).Value!;

            // Act
            var result = EstablishmentAccommodation.Create(validPrice);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Price.Should().Be(validPrice);
        }

        // Test for unsuccessful creation of EstablishmentAccommodation when price is empty
        [Fact]
        public void Create_WithEmptyPrice_ReturnsFailure()
        {
            // Act
            var result = EstablishmentAccommodation.Create(AccommodationPrice.Empty);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle();
            result.Errors[0].Message.Should().Be("Accommodation price is invalid.");
        }

        // Test for creating EstablishmentAccommodation using Result<AccommodationPrice>
        [Fact]
        public void Create_WithPriceResult_ReturnsSuccess()
        {
            // Arrange
            var priceResult = AccommodationPrice.Create(100m, 150m, 50m);

            // Act
            var result = EstablishmentAccommodation.Create(priceResult);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Price.Should().Be(priceResult.Value!);
        }

        // Test for unsuccessful creation of EstablishmentAccommodation when price result is a failure
        [Fact]
        public void Create_WithInvalidPriceResult_ReturnsFailure()
        {
            // Arrange
            var invalidPriceResult = Result<AccommodationPrice>.Failure(ErrorType.Validation,
                new Error(PriceException.NullPrice().Code.ToString(), PriceException.NullPrice().Message));

            // Act
            var result = EstablishmentAccommodation.Create(invalidPriceResult);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle();
            result.Errors[0].Message.Should().Be("Price cannot be null."); // Update this to match the actual message
        }

        // Test for updating price of an EstablishmentAccommodation with valid new price
        [Fact]
        public void UpdatePrice_WithValidPrice_ReturnsSuccess()
        {
            // Arrange
            var validPrice = AccommodationPrice.Create(100m, 150m, 50m).Value!;
            var establishmentAccommodation = EstablishmentAccommodation.Create(validPrice).Value!;

            // Act
            var newPrice = AccommodationPrice.Create(120m, 180m, 60m).Value!;
            var result = establishmentAccommodation.UpdatePrice(newPrice);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Price.Should().Be(newPrice);
        }

        // Test for unsuccessful update of price when new price is empty
        [Fact]
        public void UpdatePrice_WithEmptyPrice_ReturnsFailure()
        {
            // Arrange
            var validPrice = AccommodationPrice.Create(100m, 150m, 50m).Value!;
            var establishmentAccommodation = EstablishmentAccommodation.Create(validPrice).Value!;

            // Act
            var result = establishmentAccommodation.UpdatePrice(AccommodationPrice.Empty);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle();
            result.Errors[0].Message.Should().Be("Accommodation price is invalid.");
        }

        // Test for updating price using Result<AccommodationPrice> (successful case)
        [Fact]
        public void UpdatePrice_WithPriceResult_ReturnsSuccess()
        {
            // Arrange
            var validPrice = AccommodationPrice.Create(100m, 150m, 50m).Value!;
            var establishmentAccommodation = EstablishmentAccommodation.Create(validPrice).Value!;

            var newPriceResult = AccommodationPrice.Create(120m, 180m, 60m);

            // Act
            var result = establishmentAccommodation.UpdatePrice(newPriceResult);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Price.Should().Be(newPriceResult.Value!);
        }

        // Test for unsuccessful update of price using Result<AccommodationPrice> (failure case)
        [Fact]
        public void UpdatePrice_WithInvalidPriceResult_ReturnsFailure()
        {
            // Arrange
            var validPrice = AccommodationPrice.Create(100m, 150m, 50m).Value!;
            var establishmentAccommodation = EstablishmentAccommodation.Create(validPrice).Value!;

            var invalidPriceResult = Result<AccommodationPrice>.Failure(ErrorType.Validation,
                new Error(PriceException.NullPrice().Code.ToString(), PriceException.NullPrice().Message));

            // Act
            var result = establishmentAccommodation.UpdatePrice(invalidPriceResult);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle();
            result.Errors[0].Message.Should().Be("Price cannot be null."); // Update the test to expect this message
        }

        // Test for empty EstablishmentAccommodation instance (use of Empty() method)
        [Fact]
        public void Empty_ReturnsEmptyEstablishmentAccommodation()
        {
            // Act
            var emptyAccommodation = EstablishmentAccommodation.Empty();

            // Assert
            emptyAccommodation.Should().NotBeNull();
            emptyAccommodation.Price.Should().Be(AccommodationPrice.Empty);
        }

        // Test for creating EstablishmentAccommodation with valid price using AccommodationPrice.Create
        [Fact]
        public void Create_WithAccommodationPriceCreate_ReturnsSuccess()
        {
            // Act
            var establishmentAccommodationResult = EstablishmentAccommodation.Create(AccommodationPrice.Create(100m, 150m, 50m));

            // Assert
            establishmentAccommodationResult.IsSuccess.Should().BeTrue();
            establishmentAccommodationResult.Value.Should().NotBeNull();
            establishmentAccommodationResult.Value.Price.Should().BeEquivalentTo(AccommodationPrice.Create(100m, 150m, 50m).Value!);
        }

        // Test for creating EstablishmentAccommodation with invalid price using AccommodationPrice.Create
        [Fact]
        public void Create_WithInvalidAccommodationPrice_ReturnsFailure()
        {
            // Act
            var establishmentAccommodationResult = EstablishmentAccommodation.Create(AccommodationPrice.Create(-100m, 150m, 50m)); // Invalid price

            // Assert
            establishmentAccommodationResult.IsSuccess.Should().BeFalse();
            establishmentAccommodationResult.Errors.Should().NotBeEmpty();
            establishmentAccommodationResult.Errors.Should().Contain(e => e.Code == "NegativeStandardPrice"); // Check for the actual error code
        }

        [Fact]
        public void Create_WithInvalidStandardPrice_ReturnsFailure()
        {
            // Act
            var establishmentAccommodationResult = EstablishmentAccommodation.Create(AccommodationPrice.Create(-100m, 150m, 50m)); // Invalid standard price

            // Assert
            establishmentAccommodationResult.IsSuccess.Should().BeFalse();
            establishmentAccommodationResult.Errors.Should().Contain(e => e.Code == PriceException.NegativeStandardPrice().Code.ToString());
            establishmentAccommodationResult.Errors.Should().Contain(e => e.Message == "Standard price cannot be negative.");
        }

        [Fact]
        public void Create_WithInvalidPeakPrice_ReturnsFailure()
        {
            // Act
            var establishmentAccommodationResult = EstablishmentAccommodation.Create(AccommodationPrice.Create(100m, -150m, 50m)); // Invalid peak price

            // Assert
            establishmentAccommodationResult.IsSuccess.Should().BeFalse();
            establishmentAccommodationResult.Errors.Should().Contain(e => e.Code == PriceException.NegativePeakPrice().Code.ToString());
            establishmentAccommodationResult.Errors.Should().Contain(e => e.Message == "Peak price cannot be negative.");
        }

        [Fact]
        public void Create_WithInvalidPeakPriceBelowStandard_ReturnsFailure()
        {
            // Act
            var establishmentAccommodationResult = EstablishmentAccommodation.Create(AccommodationPrice.Create(100m, 50m, 50m)); // Invalid peak price below standard price

            // Assert
            establishmentAccommodationResult.IsSuccess.Should().BeFalse();
            establishmentAccommodationResult.Errors.Should().Contain(e => e.Code == PriceException.PeakBelowStandard().Code.ToString());
            establishmentAccommodationResult.Errors.Should().Contain(e => e.Message == "Peak price cannot be less than Standard Price.");  // Corrected message here
        }

        [Fact]
        public void Create_WithNegativeCleaningFee_ReturnsFailure()
        {
            // Act
            var establishmentAccommodationResult = EstablishmentAccommodation.Create(AccommodationPrice.Create(100m, 150m, -50m)); // Invalid cleaning fee

            // Assert
            establishmentAccommodationResult.IsSuccess.Should().BeFalse();
            establishmentAccommodationResult.Errors.Should().Contain(e => e.Code == AccommodationPriceException.NegativeCleaningFee().Code.ToString());
            establishmentAccommodationResult.Errors.Should().Contain(e => e.Message == "Cleaning fee cannot be negative.");
        }

       
    }
}
