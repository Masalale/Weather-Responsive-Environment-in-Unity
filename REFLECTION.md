# Reflection

Weather-Responsive Environment in Unity using UnityWebRequest via API

## Deserialization Structure

- **Data Models**: Five `[Serializable]` classes (`WeatherResponse`, `WeatherInfo`, `MainInfo`, `WindInfo`, `SysInfo`) handle OpenWeather's nested JSON through `JsonUtility`.
- **Null Safety**: OpenWeather can return empty arrays on bad queries, so `weather == null || weather.Length == 0` is checked upfront to prevent null reference crashes. Long error URLs from the API are also trimmed to fit the UI.
- **Data Abstraction**: The raw JSON is mapped into a `WeatherSnapshot` container. UI and scene scripts only ever touch this snapshot, so any API changes only require editing the parser.
- **Day and Night**: Calculated by comparing the observation timestamp (`dt`) against `sys.sunrise` and `sys.sunset`, falling back to the icon suffix ('d'/'n'). This keeps lighting accurate to the target city rather than the local computer clock.
- **Condition Mapping**: Grouped condition codes into 4 visual states: 2xx-5xx for Rain, 6xx for Snow, 800 for Clear, and 801-804 plus 7xx atmospheric codes (mist/fog) into Cloudy.

## UI Presentation

- **Layout**: Pinned panels to Canvas corners (controls on the top-left, readouts on the top-right) so the interface scales across different WebGL resolutions without hardcoded coordinates overlapping.
- **Input Validation**: Seven preset cities in a dropdown, plus custom lat/long inputs. Inputs are parsed with `CultureInfo.InvariantCulture` so systems using comma decimals do not reject valid coordinates.
- **Readout Metrics**: TextMeshPro labels showing city name, country, weather description, temperature in °C, humidity %, wind speed in m/s, and daytime/nighttime.
- **Feedback States**: Color-coded status line (green for success, amber for test mode, red for errors) with a loading spinner. Action buttons disable during active requests to stop duplicate queries.
- **Event Decoupling**: `WeatherController` dispatches `WeatherUpdated`, `RequestFailed`, and `LoadingChanged` events. The UI updates only when an event fires rather than polling in `Update()`.

## Extra Features

- **Provider Abstraction**: An `IWeatherProvider` interface lets `WeatherController` swap between `OpenWeatherService` (live web requests) and `SampleWeatherProvider` (offline test JSONs) without changing controller logic.
- **Smooth Transitions**: A coroutine lerps sky color, sun angle, ambient light, and fog density over 1.5 seconds. Any active blend is stopped before a new one starts to keep transitions from fighting over the main directional light.
- **Ambient Audio**: 2D AudioSource loops crossfade volume alongside visual transitions so audio matches the active weather state.
- **Environment Presets**: Visual profiles are serialized as `EnvironmentPreset` structs with `[Range]` sliders, making lighting and fog easy to tune in the Inspector without editing scripts.
- **Test Mode & Build Tool**: Nine bundled sample JSONs test all weather conditions, day/night states, and 401 error handling offline, plus an editor script (`BuildWebGL.cs`) for one-click WebGL exports.
