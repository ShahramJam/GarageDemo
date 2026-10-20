using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Garage1App;

public sealed class FileRepository : IRepository
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public FileRepository()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Garage1App",
            "garage-state.json"))
    {
    }

    public FileRepository(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        _filePath = Path.GetFullPath(filePath);
    }

    public Garage? Load()
    {
        if (!File.Exists(_filePath))
        {
            return null;
        }

        string json = File.ReadAllText(_filePath);
        GarageState state = JsonSerializer.Deserialize<GarageState>(json, SerializerOptions)
            ?? throw new InvalidDataException("Garagefilen innehåller ingen data.");

        if (state.Vehicles is null)
        {
            throw new InvalidDataException("Garagefilen saknar fordonslistan.");
        }

        Garage garage = new Garage(state.Capacity);

        foreach (VehicleState? vehicleState in state.Vehicles)
        {
            if (vehicleState is null)
            {
                throw new InvalidDataException("Garagefilen innehåller ett tomt fordon.");
            }

            Vehicle vehicle = CreateVehicle(vehicleState);
            if (!garage.TryPark(vehicle, out string message))
            {
                throw new InvalidDataException($"Fordonet kunde inte läsas in: {message}");
            }
        }

        return garage;
    }

    public void Save(Garage garage)
    {
        ArgumentNullException.ThrowIfNull(garage);

        GarageState state = new GarageState
        {
            Capacity = garage.Capacity
        };

        foreach (Vehicle vehicle in garage.Vehicles)
        {
            state?.Vehicles?.Add(CreateVehicleState(vehicle));
        }

        string? directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string temporaryPath = _filePath + ".tmp";
        string json = JsonSerializer.Serialize(state, SerializerOptions);

        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, _filePath, overwrite: true);
    }

    private static Vehicle CreateVehicle(VehicleState state)
    {
        string registrationNumber = state.RegistrationNumber
            ?? throw new InvalidDataException("Ett fordon saknar registreringsnummer.");
        string color = state.Color
            ?? throw new InvalidDataException("Ett fordon saknar färg.");

        return state.Type switch
        {
            "Car" => new Car(
                registrationNumber,
                color,
                state.NumberOfWheels,
                state.FuelType
                    ?? throw new InvalidDataException("Bilen saknar bränsletyp.")),
            "Bus" => new Bus(
                registrationNumber,
                color,
                state.NumberOfWheels,
                state.NumberOfSeats
                    ?? throw new InvalidDataException("Bussen saknar antal sittplatser.")),
            "Motorcycle" => new Motorcycle(
                registrationNumber,
                color,
                state.NumberOfWheels,
                state.CylinderVolume
                    ?? throw new InvalidDataException("Motorcykeln saknar cylindervolym.")),
            "Airplane" => new Airplane(
                registrationNumber,
                color,
                state.NumberOfWheels,
                state.NumberOfEngines
                    ?? throw new InvalidDataException("Flygplanet saknar antal motorer.")),
            "Boat" => new Boat(
                registrationNumber,
                color,
                state.NumberOfWheels,
                state.Length
                    ?? throw new InvalidDataException("Båten saknar längd.")),
            _ => throw new InvalidDataException(
                $"Okänd fordonstyp: {state.Type ?? "(saknas)"}")
        };
    }

    private static VehicleState CreateVehicleState(Vehicle vehicle)
    {
        VehicleState state = new VehicleState
        {
            Type = vehicle.TypeName,
            RegistrationNumber = vehicle.RegistrationNumber,
            Color = vehicle.Color,
            NumberOfWheels = vehicle.NumberOfWheels
        };

        switch (vehicle)
        {
            case Car car:
                state.FuelType = car.FuelType;
                break;
            case Bus bus:
                state.NumberOfSeats = bus.NumberOfSeats;
                break;
            case Motorcycle motorcycle:
                state.CylinderVolume = motorcycle.CylinderVolume;
                break;
            case Airplane airplane:
                state.NumberOfEngines = airplane.NumberOfEngines;
                break;
            case Boat boat:
                state.Length = boat.Length;
                break;
            default:
                throw new NotSupportedException(
                    $"Fordonstypen {vehicle.GetType().Name} stöds inte.");
        }

        return state;
    }

    private sealed class GarageState
    {
        public GarageState()
        {
        }

        public int Capacity { get; set; }
        public List<VehicleState?>? Vehicles { get; set; } = new List<VehicleState?>();
    }

    private sealed class VehicleState
    {
        public VehicleState()
        {
        }

        public string? Type { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Color { get; set; }
        public int NumberOfWheels { get; set; }
        public string? FuelType { get; set; }
        public int? NumberOfSeats { get; set; }
        public int? CylinderVolume { get; set; }
        public int? NumberOfEngines { get; set; }
        public double? Length { get; set; }
    }
}
