using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Garage1App;

public sealed class GarageService : IGarageService
{
    private readonly ILogger<GarageService> _logger;
    private readonly IRepository _repository;
    private Garage? _garage;

    public GarageService(ILogger<GarageService> logger, IRepository repository)
    {
        _logger = logger;
        _repository = repository;
        _garage = _repository.Load();
    }

    public static Garage Create(int capacity)
    {
        return new Garage(capacity);
    }

    public bool HasGarage => _garage is not null;
    public int Capacity => _garage?.Capacity ?? 0;
    public int Count => _garage?.Count ?? 0;

    public IReadOnlyList<Vehicle> GetAllVehicles()
    {
        try
        {
            return _garage?.Vehicles ?? Array.Empty<Vehicle>();
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Ett fel uppstod vid GetAllVehicles.");
            throw;
        }
    }

    public IReadOnlyDictionary<string, int> GetCountsByType()
    {
        try
        {
            Dictionary<string, int> counts = new Dictionary<string, int>();

            foreach (Vehicle vehicle in GetAllVehicles())
            {
                if (counts.TryGetValue(vehicle.TypeName, out int count))
                {
                    counts[vehicle.TypeName] = count + 1;
                }
                else
                {
                    counts[vehicle.TypeName] = 1;
                }
            }

            return counts;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Ett fel uppstod vid GetCountsByType.");
            throw;
        }
    }

    public bool TryPark(Vehicle vehicle, out string message)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(vehicle);

            if (_garage is null)
            {
                message = "Skapa ett garage först.";
                return false;
            }
            if (!_garage.TryPark(vehicle, out message))
            {
                return false;
            }

            try
            {
                _repository.Save(_garage);
                return true;
            }
            catch
            {
                _garage.Remove(vehicle.RegistrationNumber);
                throw;
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Ett oväntat fel uppstod när ett fordon skulle parkeras.");

            message = "Ett oväntat fel uppstod när fordonet skulle parkeras.";
            return false;
        }
    }

    public Vehicle? FindByRegistrationNumber(string registrationNumber)
    {
        try
        {
            return _garage?.Find(registrationNumber);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Ett fel uppstod vid FindByRegistrationNumber.");
            throw;
        }
    }

    public Vehicle? RemoveByRegistrationNumber(string registrationNumber)
    {
        try
        {
            Garage? garage = _garage;
            Vehicle? borttaget = garage?.Remove(registrationNumber);

            if (borttaget is not null && garage is not null)
            {
                try
                {
                    _repository.Save(garage);
                }
                catch
                {
                    garage.TryPark(borttaget, out _);
                    throw;
                }
            }

            return borttaget;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Ett fel uppstod vid RemoveByRegistrationNumber.");
            throw;
        }
    }

    public IReadOnlyList<Vehicle> Search(
        string? type,
        string? color,
        int? numberOfWheels)
    {
        try
        {
            List<Vehicle> resultat = new List<Vehicle>();

            foreach (Vehicle vehicle in GetAllVehicles())
            {
                if (!string.IsNullOrWhiteSpace(type) &&
                    !string.Equals(
                        vehicle.TypeName,
                        type.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(color) &&
                    !string.Equals(
                        vehicle.Color,
                        color.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (numberOfWheels.HasValue &&
                    vehicle.NumberOfWheels != numberOfWheels.Value)
                {
                    continue;
                }

                resultat.Add(vehicle);
            }

            return resultat.ToArray();


        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Ett fel uppstod vid Search.");
            throw;
        }
    }

    public void SetGarage(Garage garage)
    {
        ArgumentNullException.ThrowIfNull(garage);
        _repository.Save(garage);
        _garage = garage;
    }


}