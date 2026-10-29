using TMPro;
using UnityEngine;

namespace WeatherApp
{
    public class WeatherUIController : MonoBehaviour
    {
        [SerializeField] private WeatherController controller;

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
            controller.WeatherUpdated += OnWeatherUpdated;
            controller.RequestFailed += OnRequestFailed;
        }

        private void OnDestroy()
        {
            controller.WeatherUpdated -= OnWeatherUpdated;
            controller.RequestFailed -= OnRequestFailed;
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