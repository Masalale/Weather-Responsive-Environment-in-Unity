# Weather-Responsive Environment

Unity 3D environment that retrieves live weather data via OpenWeather API and updates scene lighting, skybox, particles, and audio.

## Features

- **Live Weather API**: Fetches current weather for 7 preset cities or custom coordinates (-90 to 90 lat, -180 to 180 lon) using `UnityWebRequest`.
- **4 Weather States**: Clear, Cloudy, Rain, and Snow, each with distinct Day and Night profiles (8 visual states total).
- **Day/Night Calculation**: Evaluates local sunrise and sunset timestamps from the API, independent of computer clock.
- **Test Mode**: Offline toggle supporting 9 bundled JSON samples for testing all conditions, day/night, and API errors.
- **Transitions & Audio**: Smooth 1.5s visual interpolation and normalized ambient audio crossfading.
- **Architecture**: Decoupled `IWeatherProvider` interface and C# events (`WeatherUpdated`) separating API, UI, and world simulation.

## Quick Start

1. Clone the repository:
   ```bash
   git clone https://github.com/Masalale/Weather-Responsive-Environment-in-Unity.git
   ```
2. Copy `Assets/Resources/secrets.json.example` to `Assets/Resources/secrets.json` and add your OpenWeather API key.
3. Open in Unity and press **Play** on `Assets/Scenes/Development.unity`.

## Structure

- `Assets/Scripts/Api/OpenWeatherService.cs` : API requests & sample data
- `Assets/Scripts/Core/WeatherController.cs` : app state & events
- `Assets/Scripts/Data/WeatherData.cs` : data models & JSON parsing
- `Assets/Scripts/UI/WeatherUIController.cs` : UI HUD & coordinate input
- `Assets/Scripts/World/EnvironmentController.cs` : lighting, sky, particles & audio
- `Assets/Editor/BuildWebGL.cs` : WebGL export tool

## Credits

- Weather data: [OpenWeather Current Weather API](https://openweathermap.org/current)
- 3D models: [RPG Poly Pack - Lite](https://assetstore.unity.com/) by Gigel3d
- Audio: CC0 ambient nature recordings
