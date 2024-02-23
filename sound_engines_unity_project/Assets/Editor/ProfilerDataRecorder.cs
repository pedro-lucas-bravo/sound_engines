using Codice.Client.BaseCommands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

//https://docs.unity3d.com/2020.3/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.html

public class ProfilerDataRecorder : EditorWindow {

    //private bool isRecording = false;
    private float samplePeriod = 1f;
   private double nextSampleTime = 0;

    [MenuItem("Tools/Profiler Data Recorder")]
    public static void ShowWindow() {
        GetWindow(typeof(ProfilerDataRecorder), false, "Profiler Data Recorder");
    }

    //private void OnGUI() {
    //    GUILayout.Label("Profiler Data Recorder Settings", EditorStyles.boldLabel);

    //    samplePeriod = EditorGUILayout.FloatField("Sample Period (seconds):", samplePeriod);
    //    samplePeriod = Mathf.Max(0.1f, samplePeriod); // Enforce a minimum sample period


    //    if (GUILayout.Button(isRecording ? "Stop Recording" : "Start Recording")) {
    //        isRecording = !isRecording;
    //        if (isRecording) {
    //            StartRecording();
    //        } else {
    //            StopRecording();
    //        }
    //    }
    //}

    //private void Update() {
    //    if (isRecording && EditorApplication.timeSinceStartup >= nextSampleTime) {
    //        nextSampleTime = EditorApplication.timeSinceStartup + samplePeriod;
    //        RecordSample();
    //    }
    //}

    //private void StartRecording() {
    //    nextSampleTime = EditorApplication.timeSinceStartup + samplePeriod;
    //}

    //private void StopRecording() {
    //    isRecording = false;
    //}

    //private void RecordSample() {
    //    StringBuilder sb = new StringBuilder();
    //    Profiler.fr
    //    // Define the data you are interested in
    //    float cpuUsage = Profiler.GetTotalReservedMemoryLong() / (float)(1024 * 1024);
    //    float memoryUsage = Profiler.GetTotalAllocatedMemoryLong() / (float)(1024 * 1024);
    //    Debug.Log("CPU Usage: " + cpuUsage + " MB: " + memoryUsage);
    //}

    string statsText;
    ProfilerRecorder systemMemoryRecorder;
    ProfilerRecorder gcMemoryRecorder;
    ProfilerRecorder mainThreadTimeRecorder;
    ProfilerRecorder audioTimeRecorder;
    ProfilerRecorder audioMemoryRecorder;

    static double GetRecorderFrameAverage(ProfilerRecorder recorder) {
        var samplesCount = recorder.Capacity;
        if (samplesCount == 0)
            return 0;

        double r = 0;
        //unsafe {
            var samples = new List<ProfilerRecorderSample>();
            recorder.CopyTo(samples);
        if (samples.Count < samplesCount) return 0;
            for (var i = 0; i < samplesCount; ++i)
                r += samples[i].Value;
            r /= samplesCount;
        //}

        return r;
    }

    void OnEnable() {
        systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Used Memory");
        mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
        audioTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Audio, "CsoundUnity");
        audioMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Audio, "Audio Used Memory");
    }

    void OnDisable() {
        systemMemoryRecorder.Dispose();
        gcMemoryRecorder.Dispose();
        mainThreadTimeRecorder.Dispose();
        audioTimeRecorder.Dispose();
        audioMemoryRecorder.Dispose();
    }

    void Update() {
        var sb = new StringBuilder(500);
        var mainThreadTime = GetRecorderFrameAverage(mainThreadTimeRecorder) * (1e-6f);
        sb.AppendLine($"Frame Time: {mainThreadTime:F3} ms");
        sb.AppendLine($"GC Memory: {gcMemoryRecorder.LastValue / (float)(1024 * 1024):F3} MB");
        sb.AppendLine($"System Memory: {systemMemoryRecorder.LastValue / (float)(1024 * 1024):F3} MB");
        var audioTime = audioTimeRecorder.LastValue * (1e-6f);//GetRecorderFrameAverage(audioTimeRecorder) * (1e-6f);
        sb.AppendLine($"Audio Frame Time: {audioTime:F3} ms CPU: {(audioTime / mainThreadTime) * 100f:F3}%");
        sb.AppendLine($"Audio Memory: {audioMemoryRecorder.LastValue / (float)(1024 * 1024):F3} MB");
        statsText = sb.ToString();
            
        if (audioTime > 0 && EditorApplication.timeSinceStartup >= nextSampleTime) {
            nextSampleTime = EditorApplication.timeSinceStartup + samplePeriod;
           this.Repaint();
        }
        
    }

    void OnGUI() {
        GUI.TextArea(new Rect(10, 30, 250, 300), statsText);
    }


    struct StatInfo {
        public ProfilerCategory Cat;
        public string Name;
        public ProfilerMarkerDataUnit Unit;
    }

    //https://docs.unity3d.com/ScriptReference/Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderDescription.html
    static void EnumerateProfilerStats() {
        var availableStatHandles = new List<ProfilerRecorderHandle>();
        ProfilerRecorderHandle.GetAvailable(availableStatHandles);

        var availableStats = new List<StatInfo>(availableStatHandles.Count);
        foreach (var h in availableStatHandles) {
            var statDesc = ProfilerRecorderHandle.GetDescription(h);
            var statInfo = new StatInfo() {
                Cat = statDesc.Category,
                Name = statDesc.Name,
                Unit = statDesc.UnitType
            };
            availableStats.Add(statInfo);
        }
        availableStats.Sort((a, b) => {
            var result = string.Compare(a.Cat.ToString(), b.Cat.ToString());
            if (result != 0)
                return result;

            return string.Compare(a.Name, b.Name);
        });

        var sb = new StringBuilder("Available stats:\n");
        foreach (var s in availableStats) {
            sb.AppendLine($"{(int)s.Cat}\t\t - {s.Name}\t\t - {s.Unit}");
        }

        Debug.Log(sb.ToString());
    }
}
