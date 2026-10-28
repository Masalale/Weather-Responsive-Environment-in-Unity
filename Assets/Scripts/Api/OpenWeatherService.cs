using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace WeatherApp
{
    // Shared interface for live API and sample data
    public interface IWeatherProvider
    {
        string Label { get; }
        IEnumerator Fetch(float latitude, float longitude, Action<WeatherSnapshot, string> onDone);
    }

    public class OpenWeatherService : IWeatherProvider
    {
        private const string Endpoint = "https://api.openweathermap.org/data/2.5/weather";
        private readonly string apiKey;

        public OpenWeatherService()
        {
            // Load API key from secrets.json
            var secret = Resources.Load<TextAsset>("secrets");
            if (secret == null)
            {
                Debug.LogWarning("No secrets.json in Assets/Resources.");
                return;
            }

            try
            {
                apiKey = JsonUtility.FromJson<Secrets>(secret.text).apiKey.Trim();
            }
            catch (Exception)
            {
                Debug.LogError("secrets.json is not valid JSON.");
            }
        }

        public string Label => "Live API";

        public IEnumerator Fetch(float latitude, float longitude, Action<WeatherSnapshot, string> onDone)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                onDone(null, "No API key. Add secrets.json to Assets/Resources.");
                yield break;
            }

            var url = FormattableString.Invariant(
                $"{Endpoint}?lat={latitude:F2}&lon={longitude:F2}&appid={apiKey}&units=metric");

            using (var request = UnityWebRequest.Get(url))
            {
                request.timeout = 10;
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onDone(null, Explain(request));
                    yield break;
                }

                var snapshot =
                    WeatherJsonParser.Parse(request.downloadHandler.text, latitude, longitude, out var error);
                if (snapshot == null)
                {
                    onDone(null, error);
                    yield break;
                }

                onDone(snapshot, null);
            }
        }

        private static string Explain(UnityWebRequest request)
        {
            switch (request.responseCode)
            {
                case 401: return "Invalid API key (401).";
                case 404: return "Location not found (404).";
                case 429: return "Too many requests (429). Try again shortly.";
                case 500:
                case 502:
                case 503: return "The weather service is down. Try again later.";
            }

            if (request.result == UnityWebRequest.Result.ConnectionError)
                return string.IsNullOrEmpty(request.error)
                    ? "No internet connection."
                    : "No internet connection. (" + request.error + ")";

            return "Request failed (" + request.responseCode + ").";
        }

        [Serializable]
        private class Secrets
        {
            public string apiKey;
        }
    }
}