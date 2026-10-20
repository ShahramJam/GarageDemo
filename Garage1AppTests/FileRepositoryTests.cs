using Garage1App;

namespace Garage1AppTests;

public sealed class FileRepositoryTests
{
    [Fact]
    public void SaveAndLoad_RestoresGarageAndAllVehicleTypes()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string filePath = Path.Combine(directory, "garage-state.json");

        try
        {
            FileRepository repository = new FileRepository(filePath);
            Garage garage = new Garage(5);
            garage.TryPark(new Car("CAR-1", "Blue", 4, "Electric"), out _);
            garage.TryPark(new Bus("BUS-1", "Red", 6, 40), out _);
            garage.TryPark(new Motorcycle("MOTO-1", "Black", 2, 650), out _);
            garage.TryPark(new Airplane("AIR-1", "White", 3, 2), out _);
            garage.TryPark(new Boat("BOAT-1", "Green", 0, 8.5), out _);

            repository.Save(garage);
            Garage? loaded = repository.Load();

            Assert.NotNull(loaded);
            Assert.Equal(5, loaded.Capacity);
            Assert.Equal(5, loaded.Count);

            Assert.Equal("Electric", Assert.IsType<Car>(loaded.Vehicles[0]).FuelType);
            Assert.Equal(40, Assert.IsType<Bus>(loaded.Vehicles[1]).NumberOfSeats);
            Assert.Equal(650, Assert.IsType<Motorcycle>(loaded.Vehicles[2]).CylinderVolume);
            Assert.Equal(2, Assert.IsType<Airplane>(loaded.Vehicles[3]).NumberOfEngines);
            Assert.Equal(8.5, Assert.IsType<Boat>(loaded.Vehicles[4]).Length);
        }
        finally
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }

    [Fact]
    public void Load_ReturnsNullWhenStateFileDoesNotExist()
    {
        string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        string filePath = Path.Combine(directory, "missing.json");

        FileRepository repository = new FileRepository(filePath);

        Assert.Null(repository.Load());
    }
}
