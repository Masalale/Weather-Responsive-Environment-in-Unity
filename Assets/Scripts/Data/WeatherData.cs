using System;
using UnityEngine;

namespace WeatherApp
{
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