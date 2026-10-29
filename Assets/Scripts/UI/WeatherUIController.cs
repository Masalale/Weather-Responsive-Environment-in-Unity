using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace WeatherApp
{
    public class WeatherUIController : MonoBehaviour
    {
        [SerializeField] private WeatherController controller;

        [Header("Locations")] [SerializeField] private List<LocationPreset> locations = new();

        [Header("Controls")] [SerializeField] private TMP_Dropdown locationDropdown;

        [Header("Readout")] [SerializeField] private TMP_Text locationText;

        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text temperatureText;
        [SerializeField] private TMP_Text humidityText;
        [SerializeField] private TMP_Text windText;
        [SerializeField] private TMP_Text timeOfDayText;
        [SerializeField] private TMP_Text statusText;

        [Header("Status Colours")] [SerializeField]
        private Color successColour = new(0.6f, 0.9f, 0.65f);

        [SerializeField] private Color errorColour = new(1f, 0.5f, 0.45f);

        private void Start()
        {
            foreach (var location in locations)
                locationDropdown.options.Add(new TMP_Dropdown.OptionData(location.Name));
            locationDropdown.RefreshShownValue();

            controller.WeatherUpdated += OnWeatherUpdated;
            controller.RequestFailed += OnRequestFailed;

            if (locations.Count > 0)
            {
                locationDropdown.value = 0;
                controller.Request(locations[0].Latitude, locations[0].Longitude);
            }
        }

        private void OnDestroy()
        {
            controller.WeatherUpdated -= OnWeatherUpdated;
            controller.RequestFailed -= OnRequestFailed;
        }

        public void OnLocationSelected(int index)
        {
            if (index < 0 || index >= locations.Count) return;

            controller.Request(locations[index].Latitude, locations[index].Longitude);
        }

        private void OnWeatherUpdated(WeatherSnapshot snapshot)
        {
            locationText.text = snapshot.DisplayLocation;
            descriptionText.text = snapshot.Description;
            temperatureText.text = $"{snapshot.TemperatureC:0.0} °C";
            humidityText.text = $"{snapshot.Humidity} %";
            windText.text = $"{snapshot.WindSpeedMs:0.0} m/s";
            timeOfDayText.text = snapshot.TimeOfDay;

            SetStatus("Updated from " + controller.SourceLabel + ".", successColour);
        }

        private void OnRequestFailed(string message)
        {
            SetStatus(message, errorColour);
        }

        private void SetStatus(string message, Color colour)
        {
            statusText.text = message;
            statusText.color = colour;
        }
    }
}