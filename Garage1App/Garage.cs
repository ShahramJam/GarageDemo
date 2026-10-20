using System;
using System.Collections.Generic;

namespace Garage1App;

public sealed class Garage
{
    private readonly Vehicle?[] _vehicles;

    public Garage(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(capacity),
                "Kapaciteten måste vara större än noll.");
        }

        _vehicles = new Vehicle[capacity];
    }

    public int Capacity => _vehicles.Length;

    public int Count
    {
        get
        {
            int count = 0;

            foreach (Vehicle? vehicle in _vehicles)
            {
                if (vehicle is not null)
                {
                    count++;
                }
            }

            return count;
        }
    }

    // Returnera en kopia så att anroparen inte kan ändra garagets array.
    public IReadOnlyList<Vehicle> Vehicles
    {
        get
        {
            List<Vehicle> vehicles = new List<Vehicle>();

            foreach (Vehicle? vehicle in _vehicles)
            {
                if (vehicle is not null)
                {
                    vehicles.Add(vehicle);
                }
            }

            return vehicles.ToArray();
        }
    }

    public bool TryPark(Vehicle vehicle, out string message)
    {
        if (Find(vehicle.RegistrationNumber) is not null)
        {
            message = "Registreringsnumret finns redan i garaget.";
            return false;
        }

        int emptyIndex = -1;

        for (int i = 0; i < _vehicles.Length; i++)
        {
            if (_vehicles[i] is null)
            {
                emptyIndex = i;
                break;
            }
        }

        if (emptyIndex < 0)
        {
            message = "Garaget är fullt.";
            return false;
        }

        _vehicles[emptyIndex] = vehicle;
        message = $"{vehicle.RegistrationNumber} har parkerats.";
        return true;
    }

    public Vehicle? Find(string registrationNumber)
    {
        foreach (Vehicle? vehicle in _vehicles)
        {
            if (vehicle is not null &&
                string.Equals(
                    vehicle.RegistrationNumber,
                    registrationNumber,
                    StringComparison.OrdinalIgnoreCase))
            {
                return vehicle;
            }
        }

        return null;
    }

    public Vehicle? Remove(string registrationNumber)
    {
        for (int i = 0; i < _vehicles.Length; i++)
        {
            if (_vehicles[i] is Vehicle vehicle &&
                string.Equals(
                    vehicle.RegistrationNumber,
                    registrationNumber,
                    StringComparison.OrdinalIgnoreCase))
            {
                _vehicles[i] = null;
                return vehicle;
            }
        }

        return null;
    }
}