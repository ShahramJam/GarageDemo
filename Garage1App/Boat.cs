namespace Garage1App;

public sealed class Boat : Vehicle
{
    public Boat(string registrationNumber, string color, int wheels, double length)
        : base(registrationNumber, color, wheels) => Length = length;

    public double Length { get; }
    public override string TypeName => "Boat";
    protected override string SpecificProperties => $"längd {Length} m";
}



