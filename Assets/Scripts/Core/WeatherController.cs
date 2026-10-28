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

        // Fire events so UI and 3D environment update together
        public event Action<WeatherSnapshot> WeatherUpdated;
        public event Action<string> RequestFailed;
        public event Action<bool> LoadingChanged;

        public void Request(float latitude, float longitude)
        {
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Run(latitude, longitude));
        }

        private IEnumerator Run(float latitude, float longitude)
        {
            LoadingChanged?.Invoke(true);

            WeatherSnapshot snapshot = null;
            string error = null;
            yield return provider.Fetch(latitude, longitude, (s, err) =>
            {
                snapshot = s;
                error = err;
            });

            running = null;
            LoadingChanged?.Invoke(false);

            if (!string.IsNullOrEmpty(error))
            {
                RequestFailed?.Invoke(error);
                yield break;
            }

            if (snapshot == null)
            {
                RequestFailed?.Invoke("The request was cancelled.");
                yield break;
            }

            WeatherUpdated?.Invoke(snapshot);
        }
    }
}