using TestNest.ResultPattern.Domain.ValueObjects;
using System.Text;
using TestNest.Domain.ValueObjects;
using TestNest.ResultPattern.Domain.Common;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8; // Ensure proper encoding for special characters
        Console.WriteLine("=== Price, AccommodationPrice & EstablishmentAccommodation Demo ===\n");

        // 1️⃣ Creating a valid price
        Console.WriteLine("[1] Creating a valid Price");
        Console.WriteLine("> Code: var validPriceResult = Price.Create(100, 150);");
        var validPriceResult = Price.Create(100, 150);
        ShowPriceResult(validPriceResult);

        // 2️⃣ Creating an invalid price (Negative Standard Price)
        Console.WriteLine("\n[2] Creating an invalid Price (Negative Standard Price)");
        Console.WriteLine("> Code: var invalidPrice1 = Price.Create(-50, 100);");
        var invalidPrice1 = Price.Create(-50, 100);
        ShowPriceResult(invalidPrice1);

        // 3️⃣ Creating a valid AccommodationPrice
        Console.WriteLine("\n[3] Creating a valid AccommodationPrice");
        Console.WriteLine("> Code: var validAccommodationPrice = AccommodationPrice.Create(validPriceResult.Value, 50m);");
        var validAccommodationPrice = AccommodationPrice.Create(validPriceResult.Value, 50m);
        ShowAccommodationResult(validAccommodationPrice);

        // 4️⃣ Trying to create an invalid AccommodationPrice (Negative Cleaning Fee)
        Console.WriteLine("\n[4] Creating an invalid AccommodationPrice (Negative Cleaning Fee)");
        Console.WriteLine("> Code: var invalidAccommodation1 = AccommodationPrice.Create(validPriceResult.Value, -10m);");
        var invalidAccommodation1 = AccommodationPrice.Create(validPriceResult.Value, -10m);
        ShowAccommodationResult(invalidAccommodation1);

        // 5️⃣ Creating a valid EstablishmentAccommodation
        Console.WriteLine("\n[5] Creating a valid EstablishmentAccommodation");
        Console.WriteLine("> Code: var establishmentAccommodation = EstablishmentAccommodation.Create(validAccommodationPrice.Value);");
        var establishmentAccommodation = EstablishmentAccommodation.Create(validAccommodationPrice.Value);
        ShowEstablishmentAccommodationResult(establishmentAccommodation);

        // 6️⃣ Trying to create an invalid EstablishmentAccommodation with empty price
        Console.WriteLine("\n[6] Creating an invalid EstablishmentAccommodation (Empty Price)");
        Console.WriteLine("> Code: var emptyPriceAccommodation = EstablishmentAccommodation.Create(AccommodationPrice.Empty);");
        var emptyPriceAccommodation = EstablishmentAccommodation.Create(AccommodationPrice.Empty);
        ShowEstablishmentAccommodationResult(emptyPriceAccommodation);

        // 7️⃣ Updating the price of EstablishmentAccommodation successfully
        Console.WriteLine("\n[7] Updating the Price of EstablishmentAccommodation");
        Console.WriteLine("> Code: var updatedAccommodation = establishmentAccommodation.Value.UpdatePrice(AccommodationPrice.Create(validPriceResult.Value, 75m).Value);");
        var updatedAccommodation = establishmentAccommodation.Value.UpdatePrice(AccommodationPrice.Create(validPriceResult.Value, 75m).Value);
        ShowEstablishmentAccommodationResult(updatedAccommodation);

        // 8️⃣ Trying to update the price of EstablishmentAccommodation with empty price
        Console.WriteLine("\n[8] Attempting to Update EstablishmentAccommodation with Empty Price");
        Console.WriteLine("> Code: var failedUpdate = establishmentAccommodation.Value.UpdatePrice(AccommodationPrice.Empty);");
        var failedUpdate = establishmentAccommodation.Value.UpdatePrice(AccommodationPrice.Empty);
        ShowEstablishmentAccommodationResult(failedUpdate);


        Console.WriteLine("\n=== Demo Completed ===");
        Console.ReadKey();
    }

    // Helper method to show Price result
    static void ShowPriceResult(Result<Price> result)
    {
        if (result.IsSuccess)
        {
            Console.WriteLine($"  - Success: {result.Value}");
        }
        else
        {
            Console.WriteLine($"  - Error: {string.Join(", ", result.Errors)}");
        }
    }

    // Helper method to show AccommodationPrice result
    static void ShowAccommodationResult(Result<AccommodationPrice> result)
    {
        if (result.IsSuccess)
        {
            Console.WriteLine($"  - Success: {result.Value}");
        }
        else
        {
            Console.WriteLine($"  - Error: {string.Join(", ", result.Errors)}");
        }
    }

    // Helper method to show EstablishmentAccommodation result
    static void ShowEstablishmentAccommodationResult(Result<EstablishmentAccommodation> result)
    {
        if (result.IsSuccess)
        {
            Console.WriteLine($"  - Success: {result.Value}");
        }
        else
        {
            Console.WriteLine($"  - Error: {string.Join(", ", result.Errors)}");
        }
    }

    // Helper method to show Bind result
    static void ShowBindResult(Result<decimal> result)
    {
        if (result.IsSuccess)
        {
            Console.WriteLine($"  - Total Price: {result.Value}");
        }
        else
        {
            Console.WriteLine($"  - Error: {string.Join(", ", result.Errors)}");
        }
    }

    // Helper method to show Map result
    static void ShowMapResult(Result<string> result)
    {
        if (result.IsSuccess)
        {
            Console.WriteLine($"  - Formatted: {result.Value}");
        }
        else
        {
            Console.WriteLine($"  - Error: {string.Join(", ", result.Errors)}");
        }
    }
}
