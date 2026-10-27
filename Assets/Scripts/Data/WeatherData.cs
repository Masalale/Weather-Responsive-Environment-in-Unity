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
}