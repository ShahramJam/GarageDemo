namespace Garage1App;

public sealed class Motorcycle : Vehicle
{
    public Motorcycle(string registrationNumber, string color, int wheels, int cylinderVolume)
        : base(registrationNumber, color, wheels) => CylinderVolume = cylinderVolume;

    public int CylinderVolume { get; }
    public override string TypeName => "Motorcycle";
    protected override string SpecificProperties => $"cylindervolym {CylinderVolume} cc";
}




