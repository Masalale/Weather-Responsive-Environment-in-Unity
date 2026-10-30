using System;
using System.Collections;
using UnityEngine;

namespace WeatherApp
{
    public class WeatherController : MonoBehaviour
    {
        [Tooltip("Bundled JSON files used by test mode. Order sets the dropdown order.")] [SerializeField]
        private TextAsset[] samplePayloads;

        private bool hasQuery;
        private float lastLatitude;
        private float lastLongitude;

        private OpenWeatherService live;
        private IWeatherProvider provider;
        private Coroutine running;
        private SampleWeatherProvider sample;

        public bool UsingSamples { get; private set; }
        public string SourceLabel => provider.Label;
        public string[] SampleNames { get; private set; }

        private void Awake()
        {
            if (samplePayloads == null) samplePayloads = new TextAsset[0];

            live = new OpenWeatherService();
            sample = new SampleWeatherProvider(samplePayloads);
            provider = live;

            SampleNames = new string[samplePayloads.Length];
            for (var i = 0; i < samplePayloads.Length; i++)
                SampleNames[i] = samplePayloads[i] != null ? samplePayloads[i].name : "(missing)";
        }

        // Fire events so UI and 3D environment update together
        public event Action<WeatherSnapshot> WeatherUpdated;
        public event Action<string> RequestFailed;
        public event Action<bool> LoadingChanged;

        // Switch between live API and offline test mode
        public void UseSamples(bool useSamples)
        {
            UsingSamples = useSamples;
            provider = useSamples ? sample : live;

            if (hasQuery) Request(lastLatitude, lastLongitude);
        }

        public void SelectSample(int index)
        {
            sample.Index = index;

            if (UsingSamples && hasQuery) Request(lastLatitude, lastLongitude);
        }

        public void Request(float latitude, float longitude)
        {
            lastLatitude = latitude;
            lastLongitude = longitude;
            hasQuery = true;

            if (running != null) StopCoroutine(running);
            running = StartCoroutine(Run(latitude, longitude));
        }

        public void Refresh()
        {
            if (hasQuery) Request(lastLatitude, lastLongitude);
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