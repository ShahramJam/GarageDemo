# GarageDemo

En svenskspråkig konsolapplikation för hantering av ett garage. Projektet använder .NET 10.

## Funktioner

- Skapa ett garage och ange dess kapacitet.
- Parkera och hämta ut bilar, bussar, motorcyklar, flygplan och båtar.
- Lista fordon, visa antal per fordonstyp och söka efter registreringsnummer eller fordonsegenskaper.
- Spara garagets uppgifter som JSON och läsa in dem när programmet startar.
- Validera registreringsnummer: tre bokstäver följda av tre siffror, till exempel `ABc123`. Registreringsnummer måste vara unika inom garaget. Jämförelser tar inte hänsyn till skillnader mellan stora och små bokstäver.

## Köra programmet

Installera .NET 10 SDK och kör följande kommando från lösningens rotmapp:

```powershell
dotnet run --project Garage1App/Garage1App.csproj
```

I Windows sparas garageuppgifterna i `%LOCALAPPDATA%\Garage1App\garage-state.json`.
