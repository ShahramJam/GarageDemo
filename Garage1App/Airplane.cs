namespace Garage1App;

public sealed class Airplane : Vehicle
{
    public Airplane(string registrationNumber, string color, int wheels, int numberOfEngines)
        : base(registrationNumber, color, wheels) => NumberOfEngines = numberOfEngines;

    public int NumberOfEngines { get; }
    public override string TypeName => "Airplane";
    protected override string SpecificProperties => $"{NumberOfEngines} motorer";
}


