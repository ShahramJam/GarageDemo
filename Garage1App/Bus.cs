namespace Garage1App;

public sealed class Bus : Vehicle
{
    public Bus(string registrationNumber, string color, int wheels, int numberOfSeats)
        : base(registrationNumber, color, wheels) => NumberOfSeats = numberOfSeats;

    public int NumberOfSeats { get; }
    public override string TypeName => "Bus";
    protected override string SpecificProperties => $"{NumberOfSeats} sittplatser";
}



