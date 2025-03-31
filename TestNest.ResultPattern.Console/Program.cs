using TestNest.ResultPattern.Domain.ValueObjects;
using TestNest.ResultPattern.Domain.Common;
using System.Text;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8; // Ensure proper encoding for special characters
        Console.WriteLine("=== Price Value Object Demo ===\n");

        // 1️⃣ Creating a valid price
        Console.WriteLine("[1] Creating a valid price");
        Console.WriteLine("> Code: var validPriceResult = Price.Create(100, 150);");
        var validPriceResult = Price.Create(100, 150);
        ShowResult(validPriceResult);

        // 2️⃣ Creating an invalid price (Negative Standard Price)
        Console.WriteLine("\n[2] Creating an invalid price (Negative Standard Price)");
        Console.WriteLine("> Code: var invalidPriceResult1 = Price.Create(-50, 100);");
        var invalidPriceResult1 = Price.Create(-50, 100);
        ShowResult(invalidPriceResult1);

        // 3️⃣ Creating an invalid price (Negative Peak Price)
        Console.WriteLine("\n[3] Creating an invalid price (Negative Peak Price)");
        Console.WriteLine("> Code: var invalidPriceResult2 = Price.Create(100, -50);");
        var invalidPriceResult2 = Price.Create(100, -50);
        ShowResult(invalidPriceResult2);

        // 4️⃣ Creating an invalid price (Peak Price < Standard Price)
        Console.WriteLine("\n[4] Creating an invalid price (Peak Price < Standard Price)");
        Console.WriteLine("> Code: var invalidPriceResult3 = Price.Create(150, 100);");
        var invalidPriceResult3 = Price.Create(150, 100);
        ShowResult(invalidPriceResult3);

        // 5️⃣ Updating the standard price successfully
        if (validPriceResult.IsSuccess)
        {
            Console.WriteLine("\n[5] Updating the standard price");
            Console.WriteLine("> Code: var updatedStandardPrice = validPriceResult.Value.WithStandardPrice(120);");
            var updatedStandardPrice = validPriceResult.Value.WithStandardPrice(120);
            ShowResult(updatedStandardPrice);
        }

        // 6️⃣ Trying to update standard price with a negative value
        if (validPriceResult.IsSuccess)
        {
            Console.WriteLine("\n[6] Attempting to set a negative standard price");
            Console.WriteLine("> Code: var failedUpdate = validPriceResult.Value.WithStandardPrice(-10);");
            var failedUpdate = validPriceResult.Value.WithStandardPrice(-10);
            ShowResult(failedUpdate);
        }

        // 7️⃣ Updating the peak price successfully
        if (validPriceResult.IsSuccess)
        {
            Console.WriteLine("\n[7] Updating the peak price");
            Console.WriteLine("> Code: var updatedPeakPrice = validPriceResult.Value.WithPeakPrice(180);");
            var updatedPeakPrice = validPriceResult.Value.WithPeakPrice(180);
            ShowResult(updatedPeakPrice);
        }

        // 8️⃣ Trying to update peak price with a value lower than standard price
        if (validPriceResult.IsSuccess)
        {
            Console.WriteLine("\n[8] Attempting to set peak price lower than standard price");
            Console.WriteLine("> Code: var failedPeakUpdate = validPriceResult.Value.WithPeakPrice(80);");
            var failedPeakUpdate = validPriceResult.Value.WithPeakPrice(80);
            ShowResult(failedPeakUpdate);
        }

        // 9️⃣ Price equality check
        Console.WriteLine("\n[9] Comparing two price objects with same values");
        Console.WriteLine("> Code: var price1 = Price.Create(100, 150).Value;");
        var price1 = Price.Create(100, 150).Value;

        Console.WriteLine("> Code: var price2 = Price.Create(100, 150).Value;");
        var price2 = Price.Create(100, 150).Value;

        Console.WriteLine($"  - Are They Equal? {price1.Equals(price2)} (Expected: True)");

        Console.WriteLine("\n[10] Comparing two price objects with different values");
        Console.WriteLine("> Code: var price3 = Price.Create(200, 250).Value;");
        var price3 = Price.Create(200, 250).Value;

        Console.WriteLine($"  - Are They Equal? {price1.Equals(price3)} (Expected: False)");

        // 🔟 Displaying zero price
        Console.WriteLine("\n[11] Displaying zero price");
        Console.WriteLine("> Code: Console.WriteLine(Price.Zero);");
        Console.WriteLine($"  - Zero Price: {Price.Zero}");

        // 1️⃣2️⃣ Displaying empty price
        Console.WriteLine("\n[12] Displaying empty price");
        Console.WriteLine("> Code: Console.WriteLine(Price.Empty);");
        Console.WriteLine($"  - Empty Price: {Price.Empty}");

        Console.WriteLine("\n=== Demo Completed ===");

        Console.ReadKey();
    }

    // Helper method to show result
    static void ShowResult(Result<Price> result)
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
}
