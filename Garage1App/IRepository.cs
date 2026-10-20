namespace Garage1App;

public interface IRepository
{
    Garage? Load();

    void Save(Garage garage);
}
