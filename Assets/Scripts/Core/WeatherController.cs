using System;
using System.Collections;
using UnityEngine;

namespace WeatherApp
{
    public class WeatherController : MonoBehaviour
    {
        private OpenWeatherService live;
        private IWeatherProvider provider;
        private Coroutine running;

        public string SourceLabel => provider.Label;

        private void Awake()
        {
            live = new OpenWeatherService();
            provider = live;
        }

        // Fire an event so the UI and the 3D environment update together
        public event Action<WeatherSnapshot> WeatherUpdated;

        public void Request(float latitude, float longitude)
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Run(latitude, longitude));
        }

        private IEnumerator Run(float latitude, float longitude)
        {
            WeatherSnapshot snapshot = null;
            string error = null;
            yield return provider.Fetch(latitude, longitude, (s, err) =>
            {
                snapshot = s;
                error = err;
            });

            running = null;

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogWarning("Weather request failed: " + error);
                yield break;
            }

            if (snapshot == null) yield break;

            WeatherUpdated?.Invoke(snapshot);
        }
    }
}