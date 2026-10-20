using System.Collections.Generic;
namespace Garage1App;

public interface IGarageService
{
    bool HasGarage { get; }
    int Capacity { get; }
    int Count { get; }

    // void CreateGarage(int capacity);
    void SetGarage(Garage garage);
    IReadOnlyList<Vehicle> GetAllVehicles();
    IReadOnlyDictionary<string, int> GetCountsByType();
    bool TryPark(Vehicle vehicle, out string message);
    Vehicle? FindByRegistrationNumber(string registrationNumber);
    Vehicle? RemoveByRegistrationNumber(string registrationNumber);
    IReadOnlyList<Vehicle> Search(string? type, string? color, int? numberOfWheels);
}




















