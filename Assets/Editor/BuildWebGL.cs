using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WeatherApp.EditorTools
{
    // Builds the WebGL player to Builds/WebGL
    public static class BuildWebGL
    {
        private const string ScenePath = "Assets/Scenes/Development.unity";
        private const string OutputPath = "Builds/WebGL";

        [MenuItem("Weather App/Build WebGL")]
        public static void Build()
        {
            PlayerSettings.companyName = "Weather App Assignment";
            PlayerSettings.productName = "Weather Responsive Environment";

            // Compression is left off so the build can be served by any plain static file
            // server (python -m http.server, Live Server, etc.) without extra headers.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.WebGL, ScriptingImplementation.IL2CPP);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = OutputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log(string.Format("WebGL build succeeded: {0} ({1:0.0} MB, {2:0}s)",
                    OutputPath,
                    summary.totalSize / (1024f * 1024f),
                    summary.totalTime.TotalSeconds));
            }
            else
            {
                Debug.LogError("WebGL build failed: " + summary.result + " with " + summary.totalErrors + " error(s).");
                if (Application.isBatchMode) EditorApplication.Exit(1);
            }
        }
    }
}