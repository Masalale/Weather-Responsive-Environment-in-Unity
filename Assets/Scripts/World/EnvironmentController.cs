using System;
using UnityEngine;

namespace WeatherApp
{
    // One row of the environment look. Everything here is edited in the Inspector.
    [Serializable]
    public class EnvironmentState
    {
        public Color SkyTop;
        public Color SkyHorizon;
        public Color SkyGround;
        public Color LightColour;

        [Range(0f, 3f)] [Tooltip("Sun brightness for this look.")]
        public float LightIntensity;

        public Vector3 LightRotation;
        public Color AmbientColour;
        public Color FogColour;

        [Range(0f, 0.05f)] [Tooltip("Higher values close the scene in faster.")]
        public float FogDensity;
    }

    // Picks one of the eight looks and applies the sky, sun and fog
    public class EnvironmentController : MonoBehaviour
    {
        [SerializeField] private WeatherController controller;
        [SerializeField] private Light sunLight;
        [SerializeField] private Material skyboxSource;

        // Presets for all 8 weather looks (day and night)
        [Header("Clear")] [SerializeField] private EnvironmentState clearDay = new();

        [SerializeField] private EnvironmentState clearNight = new();

        [Header("Cloudy")] [SerializeField] private EnvironmentState cloudyDay = new();

        [SerializeField] private EnvironmentState cloudyNight = new();

        [Header("Rain")] [SerializeField] private EnvironmentState rainDay = new();

        [SerializeField] private EnvironmentState rainNight = new();

        [Header("Snow")] [SerializeField] private EnvironmentState snowDay = new();

        [SerializeField] private EnvironmentState snowNight = new();

        private Material skybox;

        private void Awake()
        {
            if (skyboxSource != null)
            {
                skybox = new Material(skyboxSource);
                RenderSettings.skybox = skybox;
            }
        }

        private void OnEnable()
        {
            if (controller != null) controller.WeatherUpdated += Apply;
        }

        private void OnDisable()
        {
            if (controller != null) controller.WeatherUpdated -= Apply;
        }

        private void OnDestroy()
        {
            if (skybox != null) Destroy(skybox);
        }

        private void Apply(WeatherSnapshot snapshot)
        {
            var target = Pick(snapshot);

            if (skybox != null)
            {
                skybox.SetColor("_TopColor", target.SkyTop);
                skybox.SetColor("_HorizonColor", target.SkyHorizon);
                skybox.SetColor("_GroundColor", target.SkyGround);
            }

            if (sunLight != null)
            {
                sunLight.color = target.LightColour;
                sunLight.intensity = target.LightIntensity;
                sunLight.transform.rotation = Quaternion.Euler(target.LightRotation);
            }

            RenderSettings.ambientLight = target.AmbientColour;
            RenderSettings.fogColor = target.FogColour;
            RenderSettings.fogDensity = target.FogDensity;
        }

        private EnvironmentState Pick(WeatherSnapshot snapshot)
        {
            switch (snapshot.Category)
            {
                case WeatherCategory.Clear: return snapshot.IsDaytime ? clearDay : clearNight;
                case WeatherCategory.Rain: return snapshot.IsDaytime ? rainDay : rainNight;
                case WeatherCategory.Snow: return snapshot.IsDaytime ? snowDay : snowNight;
                default: return snapshot.IsDaytime ? cloudyDay : cloudyNight;
            }
        }
    }
}