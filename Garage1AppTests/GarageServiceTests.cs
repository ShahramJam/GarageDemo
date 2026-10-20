using Garage1App;
using Microsoft.Extensions.Logging.Abstractions;

namespace Garage1AppTests;

public sealed class GarageServiceTests
{
    [Fact]
    public void Constructor_WhenRepositoryHasNoGarage_ExposesEmptyState()
    {
        GarageService service = CreateService(new StubRepository());

        Assert.False(service.HasGarage);
        Assert.Equal(0, service.Capacity);
        Assert.Equal(0, service.Count);
        Assert.Empty(service.GetAllVehicles());
        Assert.Empty(service.GetCountsByType());
    }

    [Fact]
    public void Constructor_WhenRepositoryHasGarage_LoadsItsState()
    {
        Garage garage = new Garage(3);
        garage.TryPark(new Car("CAR-1", "Blue", 4, "Electric"), out _);
        GarageService service = CreateService(new StubRepository { GarageToLoad = garage });

        Assert.True(service.HasGarage);
        Assert.Equal(3, service.Capacity);
        Assert.Equal(1, service.Count);
        Assert.Same(garage.Vehicles[0], Assert.Single(service.GetAllVehicles()));
    }

    [Fact]
    public void Create_ReturnsGarageWithRequestedCapacity()
    {
        Garage garage = GarageService.Create(4);

        Assert.Equal(4, garage.Capacity);
        Assert.Equal(0, garage.Count);
    }

    [Fact]
    public void Create_RejectsNonPositiveCapacity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GarageService.Create(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => GarageService.Create(-1));
    }

    [Fact]
    public void TryPark_WhenGarageExists_SavesVehicleAndUpdatesCounts()
    {
        StubRepository repository = new() { GarageToLoad = new Garage(2) };
        GarageService service = CreateService(repository);
        Car car = new("CAR-1", "Blue", 4, "Electric");

        bool parked = service.TryPark(car, out string message);

        Assert.True(parked);
        Assert.Equal("CAR-1 har parkerats.", message);
        Assert.Same(car, service.FindByRegistrationNumber("car-1"));
        Assert.Equal(1, service.Count);
        Assert.Single(repository.SavedGarages);
    }

    [Fact]
    public void TryPark_WhenGarageDoesNotExist_ReturnsFailure()
    {
        GarageService service = CreateService(new StubRepository());

        bool parked = service.TryPark(new Car("CAR-1", "Blue", 4, "Electric"), out string message);

        Assert.False(parked);
        Assert.Equal("Skapa ett garage först.", message);
    }

    [Fact]
    public void TryPark_WhenVehicleIsDuplicateOrGarageIsFull_DoesNotSave()
    {
        StubRepository repository = new() { GarageToLoad = new Garage(1) };
        GarageService service = CreateService(repository);
        service.TryPark(new Car("CAR-1", "Blue", 4, "Electric"), out _);

        bool duplicateParked = service.TryPark(
            new Car("car-1", "Red", 4, "Gas"),
            out string duplicateMessage);
        bool fullParked = service.TryPark(
            new Bus("BUS-1", "Red", 6, 40),
            out string fullMessage);

        Assert.False(duplicateParked);
        Assert.Equal("Registreringsnumret finns redan i garaget.", duplicateMessage);
        Assert.False(fullParked);
        Assert.Equal("Garaget är fullt.", fullMessage);
        Assert.Equal(1, repository.SavedGarages?.Count);
        Assert.Equal(1, service.Count);
    }

    [Fact]
    public void TryPark_WhenSaveFails_RollsBackVehicleAndReturnsFailure()
    {
        StubRepository repository = new()
        {
            GarageToLoad = new Garage(2),
            SaveException = new InvalidOperationException("Save failed.")
        };
        GarageService service = CreateService(repository);

        bool parked = service.TryPark(new Car("CAR-1", "Blue", 4, "Electric"), out string message);

        Assert.False(parked);
        Assert.Equal("Ett oväntat fel uppstod när fordonet skulle parkeras.", message);
        Assert.Equal(0, service.Count);
        Assert.Null(service.FindByRegistrationNumber("CAR-1"));
    }

    [Fact]
    public void GetCountsByType_CountsVehiclesByType()
    {
        Garage garage = new Garage(3);
        garage.TryPark(new Car("CAR-1", "Blue", 4, "Electric"), out _);
        garage.TryPark(new Car("CAR-2", "Red", 4, "Gas"), out _);
        garage.TryPark(new Bus("BUS-1", "White", 6, 40), out _);
        GarageService service = CreateService(new StubRepository { GarageToLoad = garage });

        IReadOnlyDictionary<string, int> counts = service.GetCountsByType();

        Assert.Equal(2, counts["Car"]);
        Assert.Equal(1, counts["Bus"]);
    }

    [Fact]
    public void Search_FiltersByTypeColorAndWheelCount_CaseInsensitively()
    {
        Garage garage = new Garage(3);
        Car blueCar = new("CAR-1", "Blue", 4, "Electric");
        garage.TryPark(blueCar, out _);
        garage.TryPark(new Car("CAR-2", "Red", 4, "Gas"), out _);
        garage.TryPark(new Bus("BUS-1", "Blue", 6, 40), out _);
        GarageService service = CreateService(new StubRepository { GarageToLoad = garage });

        Vehicle result = Assert.Single(service.Search(" car ", " blue ", 4));

        Assert.Same(blueCar, result);
    }

    [Fact]
    public void Search_WhenFiltersAreBlank_ReturnsAllVehicles()
    {
        Garage garage = new Garage(2);
        garage.TryPark(new Car("CAR-1", "Blue", 4, "Electric"), out _);
        garage.TryPark(new Bus("BUS-1", "Red", 6, 40), out _);
        GarageService service = CreateService(new StubRepository { GarageToLoad = garage });

        Assert.Equal(2, service.Search(" ", null, null).Count);
    }

    [Fact]
    public void RemoveByRegistrationNumber_RemovesVehicleAndSavesGarage()
    {
        StubRepository repository = new() { GarageToLoad = new Garage(2) };
        GarageService service = CreateService(repository);
        Car car = new("CAR-1", "Blue", 4, "Electric");
        service.TryPark(car, out _);
        repository.SavedGarages.Clear();

        Vehicle? removed = service.RemoveByRegistrationNumber("car-1");

        Assert.Same(car, removed);
        Assert.Null(service.FindByRegistrationNumber("CAR-1"));
        Assert.Equal(0, service.Count);
        Assert.Single(repository.SavedGarages);
    }

    [Fact]
    public void RemoveByRegistrationNumber_WhenSaveFails_RestoresVehicleAndRethrows()
    {
        StubRepository repository = new() { GarageToLoad = new Garage(2) };
        GarageService service = CreateService(repository);
        Car car = new("CAR-1", "Blue", 4, "Electric");
        service.TryPark(car, out _);
        repository.SaveException = new InvalidOperationException("Save failed.");

        Assert.Throws<InvalidOperationException>(() => service.RemoveByRegistrationNumber("CAR-1"));

        Assert.Same(car, service.FindByRegistrationNumber("CAR-1"));
        Assert.Equal(1, service.Count);
    }

    [Fact]
    public void SetGarage_SavesAndReplacesCurrentGarage()
    {
        StubRepository repository = new() { GarageToLoad = new Garage(1) };
        GarageService service = CreateService(repository);
        Garage replacement = new Garage(3);

        service.SetGarage(replacement);

        Assert.Same(replacement, repository.SavedGarages[^1]);
        Assert.Equal(3, service.Capacity);
        Assert.True(service.HasGarage);
    }



    private static GarageService CreateService(StubRepository repository) =>
        new(NullLogger<GarageService>.Instance, repository);

    private sealed class StubRepository : IRepository
    {
        public Garage? GarageToLoad { get; init; }

        public List<Garage> SavedGarages { get; } = new();

        public Exception? SaveException { get; set; }

        public Garage? Load() => GarageToLoad;

        public void Save(Garage garage)
        {
            SavedGarages.Add(garage);

            if (SaveException is not null)
            {
                throw SaveException;
            }
        }
    }
}