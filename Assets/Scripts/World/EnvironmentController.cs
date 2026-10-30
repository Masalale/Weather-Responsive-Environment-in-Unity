using System;
using System.Collections;
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

        public bool Rain;
        public bool Snow;
        public bool Clouds;
    }

    // Picks one of the eight looks and smoothly transitions skybox, sun, fog, particles and audio
    public class EnvironmentController : MonoBehaviour
    {
        [SerializeField] private WeatherController controller;
        [SerializeField] private Light sunLight;
        [SerializeField] private Material skyboxSource;
        [SerializeField] private ParticleSystem rainSystem;
        [SerializeField] private ParticleSystem snowSystem;
        [SerializeField] private ParticleSystem cloudSystem;

        [Header("Transition")] [SerializeField] [Range(0.5f, 4f)]
        private float transitionDuration = 1.5f;

        [Header("Audio")] [SerializeField] private AudioSource audioSource;

        [SerializeField] private AudioClip clearAudio;
        [SerializeField] private AudioClip cloudyAudio;
        [SerializeField] private AudioClip rainAudio;
        [SerializeField] private AudioClip snowAudio;
        [SerializeField] [Range(0f, 1f)] private float maxVolume = 0.4f;

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
        private Coroutine transitionRoutine;

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

            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
                transitionRoutine = null;
            }
        }

        private void OnDestroy()
        {
            if (skybox != null) Destroy(skybox);
        }

        private void Apply(WeatherSnapshot snapshot)
        {
            var target = Pick(snapshot);

            if (transitionRoutine != null) StopCoroutine(transitionRoutine);
            transitionRoutine = StartCoroutine(TransitionTo(target, snapshot.Category));
        }

        // Smoothly fade the sky, lights, fog, and sound over 1.5s
        private IEnumerator TransitionTo(EnvironmentState target, WeatherCategory category)
        {
            // Toggle particle systems
            SetParticles(rainSystem, target.Rain);
            SetParticles(snowSystem, target.Snow);
            SetParticles(cloudSystem, target.Clouds);

            // Audio crossfading setup
            var targetClip = PickAudio(category);
            var clipChanging = audioSource != null && audioSource.clip != targetClip;
            var startVolume = audioSource != null ? audioSource.volume : 0f;

            // Capture current values for smooth interpolation
            var fromSkyTop = skybox != null ? skybox.GetColor("_TopColor") : target.SkyTop;
            var fromSkyHorizon = skybox != null ? skybox.GetColor("_HorizonColor") : target.SkyHorizon;
            var fromSkyGround = skybox != null ? skybox.GetColor("_GroundColor") : target.SkyGround;

            var fromLight = sunLight != null ? sunLight.color : target.LightColour;
            var fromIntensity = sunLight != null ? sunLight.intensity : target.LightIntensity;
            var fromRot = sunLight != null ? sunLight.transform.eulerAngles : target.LightRotation;

            var fromAmbient = RenderSettings.ambientLight;
            var fromFog = RenderSettings.fogColor;
            var fromDensity = RenderSettings.fogDensity;

            var elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / transitionDuration);

                // Fade skybox
                if (skybox != null)
                {
                    skybox.SetColor("_TopColor", Color.Lerp(fromSkyTop, target.SkyTop, t));
                    skybox.SetColor("_HorizonColor", Color.Lerp(fromSkyHorizon, target.SkyHorizon, t));
                    skybox.SetColor("_GroundColor", Color.Lerp(fromSkyGround, target.SkyGround, t));
                }

                // Fade directional light
                if (sunLight != null)
                {
                    sunLight.color = Color.Lerp(fromLight, target.LightColour, t);
                    sunLight.intensity = Mathf.Lerp(fromIntensity, target.LightIntensity, t);
                    sunLight.transform.rotation = Quaternion.Euler(
                        Mathf.LerpAngle(fromRot.x, target.LightRotation.x, t),
                        Mathf.LerpAngle(fromRot.y, target.LightRotation.y, t),
                        0f
                    );
                }

                // Fade ambient & fog
                RenderSettings.ambientLight = Color.Lerp(fromAmbient, target.AmbientColour, t);
                RenderSettings.fogColor = Color.Lerp(fromFog, target.FogColour, t);
                RenderSettings.fogDensity = Mathf.Lerp(fromDensity, target.FogDensity, t);

                // Crossfade ambient audio
                if (audioSource != null)
                {
                    if (clipChanging)
                    {
                        if (t < 0.5f)
                        {
                            audioSource.volume = Mathf.Lerp(startVolume, 0f, t * 2f);
                        }
                        else
                        {
                            if (audioSource.clip != targetClip)
                            {
                                audioSource.clip = targetClip;
                                if (targetClip != null) audioSource.Play();
                                else audioSource.Stop();
                            }

                            audioSource.volume = Mathf.Lerp(0f, maxVolume, (t - 0.5f) * 2f);
                        }
                    }
                    else if (!audioSource.isPlaying && targetClip != null)
                    {
                        audioSource.clip = targetClip;
                        audioSource.volume = Mathf.Lerp(0f, maxVolume, t);
                        audioSource.Play();
                    }
                }

                yield return null;
            }

            if (audioSource != null && audioSource.clip == targetClip && targetClip != null)
                audioSource.volume = maxVolume;

            transitionRoutine = null;
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

        private AudioClip PickAudio(WeatherCategory category)
        {
            switch (category)
            {
                case WeatherCategory.Clear: return clearAudio;
                case WeatherCategory.Rain: return rainAudio;
                case WeatherCategory.Snow: return snowAudio;
                default: return cloudyAudio;
            }
        }

        private static void SetParticles(ParticleSystem system, bool active)
        {
            if (system == null) return;

            if (active && !system.isPlaying) system.Play();
            else if (!active && system.isPlaying) system.Stop();
        }
    }
}