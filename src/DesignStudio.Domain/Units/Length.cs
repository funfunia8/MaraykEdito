namespace DesignStudio.Domain.Units;

/// <summary>
/// Internal length representation. Millimetres are the current architectural decision
/// for the first foundation milestone; UI conversions remain external.
/// </summary>
public readonly record struct Length(double Millimeters)
{
    public static Length FromMillimeters(double value) => new(value);
    public static Length FromCentimeters(double value) => new(value * 10.0);
    public static Length FromMeters(double value) => new(value * 1000.0);
    public double ToMillimeters() => Millimeters;
}
