using Garage1App;
using Microsoft.Extensions.Logging.Abstractions;

namespace Garage1AppTests;

[Collection(ConsoleCollection.Name)]
public sealed class AppManagerTests
{
    [Fact]
    public void Run_WhenNoGarageAndUserExits_PrintsExitMessage()
    {
        FakeGarageService garageService = new FakeGarageService();

        string output = RunWithInput(garageService, "0");

        Assert.Contains("Programmet avslutas.", output);
    }

    [Fact]
    public void Run_CreateGarage_StoresGarageAndConfirmsCapacity()
    {
        FakeGarageService garageService = new FakeGarageService();

        string output = RunWithInput(garageService, "1", "3", "n", "0");

        Assert.True(garageService.HasGarage);
        Assert.Equal(3, garageService.Capacity);
        Assert.Contains("Garage med 3 platser har skapats.", output);
    }

    [Fact]
    public void Run_ParkVehicle_CreatesVehicleFromEnteredDetails()
    {
        FakeGarageService garageService = new FakeGarageService();
        garageService.SetGarage(new Garage(2));

        string output = RunWithInput(
            garageService,
            "3", "3", "CAR-1", "Blue", "4", "Electric", "0");

        Car parkedCar = Assert.IsType<Car>(Assert.Single(garageService.GetAllVehicles()));
        Assert.Equal("CAR-1", parkedCar.RegistrationNumber);
        Assert.Equal("Blue", parkedCar.Color);
        Assert.Equal(4, parkedCar.NumberOfWheels);
        Assert.Equal("Electric", parkedCar.FuelType);
        Assert.Contains("CAR-1 har parkerats.", output);
    }

    [Fact]
    public void Run_FindVehicle_PrintsTheMatchingVehicle()
    {
        FakeGarageService garageService = new FakeGarageService();
        garageService.SetGarage(new Garage(2));
        garageService.TryPark(new Car("CAR-1", "Blue", 4, "Electric"), out _);

        string output = RunWithInput(garageService, "5", "CAR-1", "0");

        Assert.Contains("Car: reg.nr CAR-1", output);
    }

    [Fact]
    public void Run_SearchVehicles_PassesEnteredFiltersToService()
    {
        FakeGarageService garageService = new FakeGarageService();
        garageService.SetGarage(new Garage(2));

        string output = RunWithInput(garageService, "6", "Car", "Blue", "4", "0");

        Assert.Equal("Car", garageService.SearchType);
        Assert.Equal("Blue", garageService.SearchColor);
        Assert.Equal(4, garageService.SearchWheels);
        Assert.Contains("Inga fordon matchade sökningen.", output);
    }

    [Fact]
    public void Run_WhenGarageIsFull_DoesNotPromptToCreateAnotherVehicle()
    {
        FakeGarageService garageService = new FakeGarageService();
        garageService.SetGarage(new Garage(1));
        garageService.TryPark(new Car("CAR-1", "Blue", 4, "Electric"), out _);

        string output = RunWithInput(garageService, "3", "0");

        Assert.Contains("Det gick inte att parkera: garaget är fullt.", output);
        Assert.Equal(1, garageService.Count);
    }

    private static string RunWithInput(FakeGarageService garageService, params string[] inputLines)
    {
        TextReader originalInput = Console.In;
        TextWriter originalOutput = Console.Out;

        using StringReader input = new StringReader(string.Join(Environment.NewLine, inputLines));
        using StringWriter output = new StringWriter();

        try
        {
            Console.SetIn(input);
            Console.SetOut(output);

            AppManager appManager = new AppManager(
                garageService,
                NullLogger<AppManager>.Instance);

            appManager.Run();
            return output.ToString();
        }
        finally
        {
            Console.SetIn(originalInput);
            Console.SetOut(originalOutput);
        }
    }

    private sealed class FakeGarageService : IGarageService
    {
        private Garage? _garage;

        public bool HasGarage => _garage is not null;
        public int Capacity => _garage?.Capacity ?? 0;
        public int Count => _garage?.Count ?? 0;

        public string? SearchType { get; private set; }
        public string? SearchColor { get; private set; }
        public int? SearchWheels { get; private set; }

        public void SetGarage(Garage garage)
        {
            _garage = garage;
        }

        public IReadOnlyList<Vehicle> GetAllVehicles() =>
            _garage?.Vehicles ?? Array.Empty<Vehicle>();

        public IReadOnlyDictionary<string, int> GetCountsByType()
        {
            Dictionary<string, int> counts = new Dictionary<string, int>();

            foreach (Vehicle vehicle in GetAllVehicles())
            {
                counts[vehicle.TypeName] = counts.GetValueOrDefault(vehicle.TypeName) + 1;
            }

            return counts;
        }

        public bool TryPark(Vehicle vehicle, out string message)
        {
            if (_garage is null)
            {
                message = "Skapa ett garage först.";
                return false;
            }

            return _garage.TryPark(vehicle, out message);
        }

        public Vehicle? FindByRegistrationNumber(string registrationNumber) =>
            _garage?.Find(registrationNumber);

        public Vehicle? RemoveByRegistrationNumber(string registrationNumber) =>
            _garage?.Remove(registrationNumber);

        public IReadOnlyList<Vehicle> Search(string? type, string? color, int? numberOfWheels)
        {
            SearchType = type;
            SearchColor = color;
            SearchWheels = numberOfWheels;
            return Array.Empty<Vehicle>();
        }
    }
}
