using System.Text;

namespace OrderProcessing;

// What the order page shows. Discount and delivery are nullable so a summary can
// be built, and honestly flagged, when an optional service is missing.
public record OrderSummary(Product Product, decimal Price, decimal? DiscountPercent, int? DeliveryDays)
{
    public override string ToString()
    {
        var text = new StringBuilder();
        text.AppendLine($"Order summary: {Product.Name} ({Product.Id})");
        text.AppendLine($"  Price:    {Price:C}");
        text.AppendLine(DiscountPercent is { } percent
            ? $"  Discount: {percent}% loyalty discount"
            : "  Discount: unavailable");
        text.Append(DeliveryDays is { } days
            ? $"  Delivery: about {days} days"
            : "  Delivery: delivery estimate unavailable");
        return text.ToString();
    }
}
