using DesignStudio.Domain.Pricing;

namespace DesignStudio.Pricing;

public interface IPricingCalculator
{
    decimal CalculateSubtotal(IEnumerable<PricingItem> items);
}

public sealed class PricingCalculator : IPricingCalculator
{
    public decimal CalculateSubtotal(IEnumerable<PricingItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return items.Sum(x => x.TotalCost);
    }
}
