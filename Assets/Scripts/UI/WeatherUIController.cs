using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WeatherApp
{
    public class WeatherUIController : MonoBehaviour
    {
        [SerializeField] private WeatherController controller;

        [Header("Locations")] [SerializeField] private List<LocationPreset> locations = new();

        [Header("Controls")] [SerializeField] private TMP_Dropdown locationDropdown;

        [SerializeField] private TMP_InputField latitudeInput;
        [SerializeField] private TMP_InputField longitudeInput;
        [SerializeField] private Button getWeatherButton;
        [SerializeField] private Button refreshButton;

        [Header("Test Mode")] [SerializeField] private Toggle testModeToggle;

        [SerializeField] private TMP_Dropdown sampleDropdown;
        [SerializeField] private GameObject testModeBanner;

        [Header("Readout")] [SerializeField] private TMP_Text locationText;

        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text temperatureText;
        [SerializeField] private TMP_Text humidityText;
        [SerializeField] private TMP_Text windText;
        [SerializeField] private TMP_Text timeOfDayText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private GameObject loadingIndicator;

        [Header("Status Colours")] [SerializeField]
        private Color successColour = new(0.6f, 0.9f, 0.65f);

        [SerializeField] private Color errorColour = new(1f, 0.5f, 0.45f);
        [SerializeField] private Color infoColour = new(1f, 0.85f, 0.45f);

        private void Start()
        {
            foreach (var location in locations)
                locationDropdown.options.Add(new TMP_Dropdown.OptionData(location.Name));
            locationDropdown.RefreshShownValue();

            foreach (var name in controller.SampleNames) sampleDropdown.options.Add(new TMP_Dropdown.OptionData(name));
            sampleDropdown.RefreshShownValue();

            controller.WeatherUpdated += OnWeatherUpdated;
            controller.RequestFailed += OnRequestFailed;
            controller.LoadingChanged += OnLoadingChanged;

            sampleDropdown.interactable = false;
            testModeBanner.SetActive(false);

            if (locations.Count > 0)
            {
                locationDropdown.value = 0;
                SetInputs(locations[0]);
                controller.Request(locations[0].Latitude, locations[0].Longitude);
            }
        }

        private void OnDestroy()
        {
            controller.WeatherUpdated -= OnWeatherUpdated;
            controller.RequestFailed -= OnRequestFailed;
            controller.LoadingChanged -= OnLoadingChanged;
        }

        public void OnLocationSelected(int index)
        {
            if (index < 0 || index >= locations.Count) return;

            SetInputs(locations[index]);
            controller.Request(locations[index].Latitude, locations[index].Longitude);
        }

        // Hooked up to the buttons in the Inspector
        public void OnGetWeather()
        {
            if (!TryRead(latitudeInput.text, -90f, 90f, "Latitude", out var latitude)) return;
            if (!TryRead(longitudeInput.text, -180f, 180f, "Longitude", out var longitude)) return;

            controller.Request(latitude, longitude);
        }

        // Check that lat is -90 to 90 and lon is -180 to 180 before sending
        private bool TryRead(string text, float min, float max, string label, out float value)
        {
            value = 0f;

            if (string.IsNullOrWhiteSpace(text))
            {
                SetStatus(label + " is required.", errorColour);
                return false;
            }

            if (!float.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            {
                SetStatus(label + " must be a number.", errorColour);
                return false;
            }

            if (value < min || value > max)
            {
                SetStatus($"{label} must be between {min} and {max}.", errorColour);
                return false;
            }

            return true;
        }

        public void OnTestModeChanged(bool useSamples)
        {
            sampleDropdown.interactable = useSamples;
            locationDropdown.interactable = !useSamples;
            latitudeInput.interactable = !useSamples;
            longitudeInput.interactable = !useSamples;
            getWeatherButton.interactable = !useSamples;
            testModeBanner.SetActive(useSamples);

            SetStatus(useSamples ? "Test mode: bundled sample data." : "Live weather.", infoColour);

            controller.UseSamples(useSamples);
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

        private void OnLoadingChanged(bool isLoading)
        {
            loadingIndicator.SetActive(isLoading);
            getWeatherButton.interactable = !isLoading && !testModeToggle.isOn;
            refreshButton.interactable = !isLoading;
        }

        private void SetInputs(LocationPreset location)
        {
            latitudeInput.text = location.Latitude.ToString("F2", CultureInfo.InvariantCulture);
            longitudeInput.text = location.Longitude.ToString("F2", CultureInfo.InvariantCulture);
        }

        private void SetStatus(string message, Color colour)
        {
            statusText.text = message;
            statusText.color = colour;
        }
    }
}