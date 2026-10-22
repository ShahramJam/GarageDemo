using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
namespace Garage1App;

public sealed class AppManager : IAppManager
{
    private readonly IGarageService _garageService;
    private readonly ILogger<AppManager> _logger;

    public AppManager(IGarageService garageService, ILogger<AppManager> logger)
    {
        _garageService = garageService;
        _logger = logger;
    }

    public void Run()
    {
        try
        {
            bool running = true;

            while (running)
            {
                Console.WriteLine("\n=== Garage 1.0 ===");

                if (!_garageService.HasGarage)
                {
                    Console.WriteLine("1. Skapa garage");
                    Console.WriteLine("0. Avsluta");

                    switch (ReadInt("Välj: ", 0, 1))
                    {
                        case 1:
                            CreateGarage();
                            break;
                        case 0:
                            running = false;
                            break;
                    }

                    continue;
                }

                Console.WriteLine($"Platser: {_garageService.Count}/{_garageService.Capacity}");
                Console.WriteLine("1. Lista parkerade fordon");
                Console.WriteLine("2. Visa antal per fordonstyp");
                Console.WriteLine("3. Parkera fordon");
                Console.WriteLine("4. Hämta ut fordon");
                Console.WriteLine("5. Sök registreringsnummer");
                Console.WriteLine("6. Sök med egenskaper");
                Console.WriteLine("7. Lägg till flera fordon");
                Console.WriteLine("8. Skapa nytt garage");
                Console.WriteLine("0. Avsluta");

                switch (ReadInt("Välj: ", 0, 8))
                {
                    case 1:
                        ListVehicles();
                        break;
                    case 2:
                        ShowVehicleCounts();
                        break;
                    case 3:
                        ParkVehicle();
                        break;
                    case 4:
                        RemoveVehicle();
                        break;
                    case 5:
                        FindVehicle();
                        break;
                    case 6:
                        SearchVehicles();
                        break;
                    case 7:
                        PopulateGarage();
                        break;
                    case 8:
                        CreateGarage();
                        break;
                    case 0:
                        running = false;
                        break;
                }
            }

            Console.WriteLine("Programmet avslutas.");
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception, "Programmet avslutas efter ett oväntat fel.");
            Console.WriteLine("Ett oväntat fel uppstod. Programmet avslutas.");
        }
    }

    private void CreateGarage()
    {
        int capacity = ReadInt("Ange garagets kapacitet: ", 1, int.MaxValue);
        _garageService.SetGarage(GarageService.Create(capacity));
        Console.WriteLine($"Garage med {capacity} platser har skapats.");

        if (ReadYesNo("Vill du lägga till fordon nu? (j/n): "))
        {
            PopulateGarage();
        }
    }

    private void PopulateGarage()
    {
        int available = _garageService.Capacity - _garageService.Count;
        if (available <= 0)
        {
            Console.WriteLine("Garaget är fullt.");
            return;
        }

        int number = ReadInt($"Hur många fordon vill du lägga till? (1–{available}): ", 1, available);

        for (int i = 0; i < number; i++)
        {
            Console.WriteLine($"\nFordon {i + 1} av {number}:");
            ParkVehicle();
        }
    }

    private void ParkVehicle()
    {
        if (_garageService.Count == _garageService.Capacity)
        {
            Console.WriteLine("Det gick inte att parkera: garaget är fullt.");
            return;
        }

        Vehicle vehicle = CreateVehicle();
        bool success = _garageService.TryPark(vehicle, out string message);
        Console.WriteLine(success ? message : $"Det gick inte att parkera: {message}");
    }

    private static Vehicle CreateVehicle()
    {
        while (true)
        {
            try
            {
                Console.WriteLine("Välj fordonstyp:");
                Console.WriteLine("1. Motorcykel");
                Console.WriteLine("2. Flygplan");
                Console.WriteLine("3. Bil");
                Console.WriteLine("4. Buss");
                Console.WriteLine("5. Båt");

                int choice = ReadInt("Typ: ", 1, 5);
                string registrationNumber = ReadRequiredString("Registreringsnummer: ");
                string color = ReadRequiredString("Färg: ");
                int wheels = ReadInt("Antal hjul: ", 0, int.MaxValue);

                return choice switch
                {
                    1 => new Motorcycle(registrationNumber, color, wheels,
                        ReadInt("Cylindervolym (cc): ", 1, int.MaxValue)),
                    2 => new Airplane(registrationNumber, color, wheels,
                        ReadInt("Antal motorer: ", 1, int.MaxValue)),
                    3 => new Car(registrationNumber, color, wheels,
                        ReadRequiredString("Bränsletyp: ")),
                    4 => new Bus(registrationNumber, color, wheels,
                        ReadInt("Antal sittplatser: ", 1, int.MaxValue)),
                    5 => new Boat(registrationNumber, color, wheels,
                        ReadDouble("Längd i meter: ", 0.1, double.MaxValue)),
                    _ => throw new InvalidOperationException("Ogiltig fordonstyp.")
                };
            }
            catch (ArgumentException exception)
            {
                Console.WriteLine($"Kunde inte skapa fordonet: {exception.Message}");
                Console.WriteLine("Försök igen.");
            }
        }
    }

    private void ListVehicles()
    {
        IReadOnlyList<Vehicle> vehicles = _garageService.GetAllVehicles();
        if (vehicles.Count == 0)
        {
            Console.WriteLine("Garaget är tomt.");
            return;
        }

        foreach (Vehicle vehicle in vehicles)
        {
            Console.WriteLine(vehicle);
        }
    }

    private void ShowVehicleCounts()
    {
        IReadOnlyDictionary<string, int> counts = _garageService.GetCountsByType();
        if (counts.Count == 0)
        {
            Console.WriteLine("Garaget är tomt.");
            return;
        }

        foreach (KeyValuePair<string, int> item in counts)
        {
            Console.WriteLine($"{item.Key}: {item.Value}");
        }
    }

    private void RemoveVehicle()
    {
        string registrationNumber = ReadRequiredString("Registreringsnummer att hämta ut: ");
        Vehicle? vehicle = _garageService.RemoveByRegistrationNumber(registrationNumber);

        Console.WriteLine(vehicle is null
            ? "Inget fordon med det registreringsnumret hittades."
            : $"Fordonet har hämtats ut: {vehicle}");
    }

    private void FindVehicle()
    {
        string registrationNumber = ReadRequiredString("Registreringsnummer att söka efter: ");
        Vehicle? vehicle = _garageService.FindByRegistrationNumber(registrationNumber);

        Console.WriteLine(vehicle is null
            ? "Inget fordon med det registreringsnumret hittades."
            : vehicle.ToString());
    }

    private void SearchVehicles()
    {
        Console.WriteLine("Tillgängliga typer: Motorcycle, Airplane, Car, Bus, Boat");
        string? type = ReadOptionalString("Fordonstyp (lämna tomt för alla): ");
        string? color = ReadOptionalString("Färg (lämna tomt för alla): ");
        int? wheels = ReadOptionalInt("Antal hjul (lämna tomt för alla): ");

        IReadOnlyList<Vehicle> results = _garageService.Search(type, color, wheels);
        if (results.Count == 0)
        {
            Console.WriteLine("Inga fordon matchade sökningen.");
            return;
        }

        foreach (Vehicle vehicle in results)
        {
            Console.WriteLine(vehicle);
        }
    }

    private static string ReadRequiredString(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? value = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            Console.WriteLine("Värdet får inte vara tomt.");
        }
    }

    private static string? ReadOptionalString(string prompt)
    {
        Console.Write(prompt);
        string? value = Console.ReadLine()?.Trim();
        return string.IsNullOrEmpty(value) ? null : value;
    }

    private static int ReadInt(string prompt, int minimum, int maximum)
    {
        while (true)
        {
            Console.Write(prompt);

            if (int.TryParse(Console.ReadLine(), out int value) &&
                value >= minimum && value <= maximum)
            {
                return value;
            }

            Console.WriteLine($"Ange ett heltal mellan {minimum} och {maximum}.");
        }
    }

    private static int? ReadOptionalInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            if (int.TryParse(input, out int value) && value >= 0)
            {
                return value;
            }

            Console.WriteLine("Ange ett giltigt, icke-negativt heltal eller lämna tomt.");
        }
    }

    private static double ReadDouble(string prompt, double minimum, double maximum)
    {
        while (true)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine()?.Trim().Replace(',', '.');

            if (double.TryParse(
                    input,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double value) &&
                value >= minimum && value <= maximum)
            {
                return value;
            }

            Console.WriteLine($"Ange ett tal mellan {minimum} och {maximum}.");
        }
    }

    private static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? answer = Console.ReadLine()?.Trim();

            if (string.Equals(answer, "j", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(answer, "n", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            Console.WriteLine("Svara j eller n.");
        }
    }
}
