using System;
using UnityEngine;

namespace WeatherApp
{
    public enum WeatherCategory
    {
        Clear,
        Cloudy,
        Rain,
        Snow
    }

    // The values the UI and the environment both read from
    public class WeatherSnapshot
    {
        public WeatherCategory Category;
        public string Country;
        public string Description;
        public int Humidity;
        public bool IsDaytime;
        public string LocationName;
        public float TemperatureC;
        public float WindSpeedMs;

        public string DisplayLocation =>
            string.IsNullOrEmpty(Country) ? LocationName : LocationName + ", " + Country;

        public string TimeOfDay => IsDaytime ? "Daytime" : "Nighttime";
    }

    public static class WeatherCategoryMapper
    {
        // Groups OpenWeather condition codes into the four visual states
        public static WeatherCategory FromConditionId(int id)
        {
            if (id >= 200 && id < 600) return WeatherCategory.Rain;
            if (id >= 600 && id < 700) return WeatherCategory.Snow;
            if (id == 800) return WeatherCategory.Clear;
            return WeatherCategory.Cloudy;
        }
    }

    // JSON shape of the Current Weather response. Only the fields this app uses are kept.
    [Serializable]
    public class WeatherResponse
    {
        public WeatherInfo[] weather;
        public MainInfo main;
        public WindInfo wind;
        public SysInfo sys;
        public long dt;
        public string name;
        public string message;
    }

    [Serializable]
    public class WeatherInfo
    {
        public int id;
        public string description;
        public string icon;
    }

    [Serializable]
    public class MainInfo
    {
        public float temp;
        public int humidity;
    }

    [Serializable]
    public class WindInfo
    {
        public float speed;
    }

    [Serializable]
    public class SysInfo
    {
        public string country;
        public long sunrise;
        public long sunset;
    }

    public static class WeatherJsonParser
    {
        public static WeatherSnapshot Parse(string json, float requestedLat, float requestedLon, out string error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                error = "The weather service returned nothing.";
                return null;
            }

            WeatherResponse response;
            try
            {
                response = JsonUtility.FromJson<WeatherResponse>(json);
            }
            catch (Exception)
            {
                error = "The weather response could not be read.";
                return null;
            }

            if (response == null || response.weather == null || response.weather.Length == 0 || response.main == null)
            {
                error = string.IsNullOrEmpty(response?.message)
                    ? "No weather data for this location."
                    : Shorten(response.message);
                return null;
            }

            var condition = response.weather[0];

            return new WeatherSnapshot
            {
                LocationName = string.IsNullOrEmpty(response.name)
                    ? FormattableString.Invariant($"{requestedLat:F2}, {requestedLon:F2}")
                    : response.name,
                Country = response.sys != null ? response.sys.country : "",
                Description = Capitalise(condition.description),
                Category = WeatherCategoryMapper.FromConditionId(condition.id),
                TemperatureC = response.main.temp,
                Humidity = response.main.humidity,
                WindSpeedMs = response.wind != null ? response.wind.speed : 0f,
                IsDaytime = IsDaytime(response, condition)
            };
        }

        // The API appends a long FAQ link to some errors, which overflows the status line
        private static string Shorten(string message)
        {
            var link = message.IndexOf("http", StringComparison.Ordinal);
            if (link <= 0) return message;

            var head = message.Substring(0, link).Trim();

            // Drop any dangling lead-in such as "Please see"
            var stop = head.LastIndexOf('.');
            return stop > 0 ? head.Substring(0, stop + 1) : head;
        }

        // Check if it's day or night from sunrise and sunset times
        private static bool IsDaytime(WeatherResponse response, WeatherInfo condition)
        {
            if (response.sys != null && response.sys.sunrise > 0 && response.sys.sunset > 0 && response.dt > 0)
                return response.dt >= response.sys.sunrise && response.dt < response.sys.sunset;

            return (condition.icon ?? "").EndsWith("d");
        }

        private static string Capitalise(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return char.ToUpper(text[0]) + text.Substring(1);
        }
    }
}