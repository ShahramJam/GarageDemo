namespace Garage1App;

public sealed class Car : Vehicle
{
    public Car(string registrationNumber, string color, int wheels, string fuelType)
        : base(registrationNumber, color, wheels) => FuelType = fuelType;

    public string FuelType { get; }
    public override string TypeName => "Car";
    protected override string SpecificProperties => $"bränsletyp {FuelType}";
}





