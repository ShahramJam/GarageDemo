using System;

namespace Garage1App;

public abstract class Vehicle
{
    protected Vehicle(string registrationNumber, string color, int numberOfWheels)
    {
        if (!Validator.HasValidRegistrationNumber(registrationNumber))
        {
            throw new ArgumentException(
                "Registreringsnumret måste bestå av tre bokstäver följt av tre siffror, till exempel ABc123.",
                nameof(registrationNumber));
        }
        RegistrationNumber = registrationNumber;
        Color = color;
        NumberOfWheels = numberOfWheels;
    }

    public string RegistrationNumber { get; }
    public string Color { get; }
    public int NumberOfWheels { get; }
    public abstract string TypeName { get; }
    protected abstract string SpecificProperties { get; }

    public override string ToString() =>
        $"{TypeName}: reg.nr {RegistrationNumber}, färg {Color}, " +
        $"{NumberOfWheels} hjul, {SpecificProperties}";
}

