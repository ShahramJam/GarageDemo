using Garage1App;

namespace Garage1AppTests;

public sealed class GarageTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_RejectsNonPositiveCapacity(int capacity)
    {
        ArgumentOutOfRangeException exception =
            Assert.Throws<ArgumentOutOfRangeException>(() => new Garage(capacity));

        Assert.Equal("capacity", exception.ParamName);
    }

    [Fact]
    public void Constructor_SetsCapacityAndStartsEmpty()
    {
        Garage garage = new(2);

        Assert.Equal(2, garage.Capacity);
        Assert.Equal(0, garage.Count);
        Assert.Empty(garage.Vehicles);
    }

    [Fact]
    public void TryPark_AddsVehicleAndReturnsConfirmation()
    {
        Garage garage = new(1);
        Car car = new("CAR-1", "Blue", 4, "Electric");

        bool parked = garage.TryPark(car, out string message);

        Assert.True(parked);
        Assert.Equal("CAR-1 har parkerats.", message);
        Assert.Equal(1, garage.Count);
        Assert.Same(car, Assert.Single(garage.Vehicles));
    }

    [Fact]
    public void TryPark_RejectsDuplicateRegistrationIgnoringCase()
    {
        Garage garage = new(2);
        garage.TryPark(new Car("CAR-1", "Blue", 4, "Electric"), out _);

        bool parked = garage.TryPark(
            new Car("car-1", "Red", 4, "Gas"),
            out string message);

        Assert.False(parked);
        Assert.Equal("Registreringsnumret finns redan i garaget.", message);
        Assert.Equal(1, garage.Count);
    }

    [Fact]
    public void TryPark_RejectsVehicleWhenGarageIsFull()
    {
        Garage garage = new(1);
        garage.TryPark(new Car("CAR-1", "Blue", 4, "Electric"), out _);

        bool parked = garage.TryPark(
            new Bus("BUS-1", "White", 6, 40),
            out string message);

        Assert.False(parked);
        Assert.Equal("Garaget är fullt.", message);
        Assert.Equal(1, garage.Count);
    }

    [Fact]
    public void Find_ReturnsMatchingVehicleIgnoringCase()
    {
        Garage garage = new(1);
        Car car = new("CAR-1", "Blue", 4, "Electric");
        garage.TryPark(car, out _);

        Vehicle? found = garage.Find("car-1");

        Assert.Same(car, found);
    }

    [Fact]
    public void Find_ReturnsNullWhenRegistrationIsNotFound()
    {
        Garage garage = new(1);

        Assert.Null(garage.Find("CAR-1"));
    }

    [Fact]
    public void Remove_ReturnsVehicleAndDecreasesCount()
    {
        Garage garage = new(2);
        Car car = new("CAR-1", "Blue", 4, "Electric");
        garage.TryPark(car, out _);

        Vehicle? removed = garage.Remove("car-1");

        Assert.Same(car, removed);
        Assert.Equal(0, garage.Count);
        Assert.Null(garage.Find("CAR-1"));
    }

    [Fact]
    public void Remove_ReturnsNullWhenRegistrationIsNotFound()
    {
        Garage garage = new(1);

        Assert.Null(garage.Remove("CAR-1"));
        Assert.Equal(0, garage.Count);
    }

    [Fact]
    public void TryPark_CanUseSpaceFreedByRemovingVehicle()
    {
        Garage garage = new(1);
        garage.TryPark(new Car("CAR-1", "Blue", 4, "Electric"), out _);
        garage.Remove("CAR-1");
        Car replacement = new("CAR-2", "Red", 4, "Gas");

        bool parked = garage.TryPark(replacement, out _);

        Assert.True(parked);
        Assert.Equal(1, garage.Count);
        Assert.Same(replacement, Assert.Single(garage.Vehicles));
    }

    [Fact]
    public void Vehicles_ReturnsSnapshotNotAffectedByLaterChanges()
    {
        Garage garage = new(2);
        Car first = new("CAR-1", "Blue", 4, "Electric");
        garage.TryPark(first, out _);

        IReadOnlyList<Vehicle> snapshot = garage.Vehicles;
        garage.TryPark(new Car("CAR-2", "Red", 4, "Gas"), out _);

        Assert.Single(snapshot);
        Assert.Same(first, snapshot[0]);
        Assert.Equal(2, garage.Vehicles.Count);
    }
}