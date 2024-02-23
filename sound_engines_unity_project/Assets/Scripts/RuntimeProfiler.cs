using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

public class RuntimeProfiler : MonoBehaviour {

    public enum AudioTimeStat { 
        Csound,
        Chuck
    }

    [Header("use 'CsoundUnity' ")]
    public AudioTimeStat audioTimeStat = AudioTimeStat.Csound; 
    public float measureInterval = 0.2f;
    public int numberOfMesurementsToShow = 30;
    public bool showInUI = true;
    public TMP_Text statsLabel;

    public int CurrentDataCount {
        get {
            return allData.Count;
        }
    }

    public int Round {
        get {
            return _round;
        }
    }

    private bool isRecording = false;

    ProfilerRecorder systemMemoryRecorder;
    ProfilerRecorder gcMemoryRecorder;
    ProfilerRecorder mainThreadTimeRecorder;
    ProfilerRecorder audioTimeRecorder;
    ProfilerRecorder audioMemoryRecorder;

    private string _audioTimeStat;
    private int N_dataFields = 7;
    private float[] currentMeasurements;
    private int currentMeasurementCount = 0;
    public List<float[]> allData = new List<float[]>();
    private int bufferSize;
    private int sampleRate; 
    private int _currentPlayingSources = 0;
    private int _round = 0;

    private string _fileDirectoryPath;
    private string _date;

    void Start() {
        _audioTimeStat = audioTimeStat == AudioTimeStat.Csound ? "CsoundUnity" : "ChuckMainInstance";
        AudioSettings.GetDSPBufferSize(out var bufferLength, out var numBuffers);
        Debug.Log("DSP Buffer Size: " + bufferLength + " Num: " + numBuffers);
        Debug.Log("Sample Rate: " + AudioSettings.outputSampleRate);
        bufferSize = bufferLength;
        sampleRate = AudioSettings.outputSampleRate;
        _date = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        if(!showInUI)
            statsLabel.gameObject.SetActive(false);

#if UNITY_ANDROID && !UNITY_EDITOR
        _fileDirectoryPath = Application.persistentDataPath;
#else
        _fileDirectoryPath = Application.dataPath + "/../../data_analysis/editor_data";
#endif
        currentMeasurements = new float[N_dataFields];

        ResetMeasurements();
        
    }

    public void StartRecording() {
        systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
        mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
        audioTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Audio, _audioTimeStat);
        audioMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Audio, "Audio Used Memory");
        isRecording = true;
        ResetMeasurements();
    }

    public void StopRecording(bool clearAllData = true) {
        systemMemoryRecorder.Dispose();
        gcMemoryRecorder.Dispose();
        mainThreadTimeRecorder.Dispose();
        audioTimeRecorder.Dispose();
        audioMemoryRecorder.Dispose();
        isRecording = false;
        if (clearAllData) {
            allData.Clear();
        }
        System.GC.Collect();
    }

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

    string StringfyDataRow(float[] dataRow) {
        var sb = new StringBuilder(500);        
        sb.AppendLine($"Frame Time: {dataRow[0]:F3} ms");
        sb.AppendLine($"GC Memory: {dataRow[1]:F3} MB");
        sb.AppendLine($"System Memory: {dataRow[2]:F3} MB");
        sb.AppendLine($"Audio Frame Time: {dataRow[3]:F3} ms CPU: {(dataRow[3] / dataRow[0]) * 100f:F3}%");
        sb.AppendLine($"Audio Memory: {dataRow[4]:F3} MB");
        sb.AppendLine($"Playing Audio Sources: {dataRow[5]}");
        return sb.ToString();
    }

    void ResetMeasurements() {
        for (int i = 0; i < currentMeasurements.Length; i++) {
            currentMeasurements[i] = 0;
        }
        currentMeasurementCount = 0;  
    }

    void CollectMeasurement() {
        var audioTime = audioTimeRecorder.LastValue * (1e-6f);
        if (_currentPlayingSources == 0 || audioTime > 0.000001) {
            var dataRow = new float[N_dataFields];
            var index = 0;
            dataRow[index++] = Time.timeSinceLevelLoad;
            dataRow[index++] = mainThreadTimeRecorder.LastValue * (1e-6f);
            dataRow[index++] = gcMemoryRecorder.LastValue / (float)(1024 * 1024);
            dataRow[index++] = systemMemoryRecorder.LastValue / (float)(1024 * 1024);
            dataRow[index++] = audioTime;
            dataRow[index++] = audioMemoryRecorder.LastValue / (float)(1024 * 1024);
            dataRow[index++] = _currentPlayingSources;
            allData.Add(dataRow);
            for (int i = 0; i < currentMeasurements.Length; i++) {
                currentMeasurements[i] += dataRow[i];
            }
            currentMeasurementCount++;
        }
    }

    void AverageMeasurements() {
        if (currentMeasurementCount > 0) {
            for (int i = 0; i < currentMeasurements.Length; i++) {
                currentMeasurements[i] /= currentMeasurementCount;
            }
        }
    }


    private void Update() {
        if (isRecording) {
            if (Time.time % measureInterval < Time.deltaTime) {
                CollectMeasurement();
            }
            if (currentMeasurementCount >= numberOfMesurementsToShow) {
                AverageMeasurements();
                //Show in UI
                if (showInUI)
                    statsLabel.text = StringfyDataRow(currentMeasurements);
                ResetMeasurements();
            }
        } 

        if(Input.GetKeyDown(KeyCode.R)) {
            if (isRecording) {
                StopRecording();
            }
            else {
                StartRecording();
            }
        }
    }

    public void AddPlayingAudioSource() {
        _currentPlayingSources++;
    }
    public void RemovePlayingAudioSource() {
        _currentPlayingSources--;
    }

    public void NextRound() {
        _round++;
    }

    //Save all data rows to a csv file
    public void SaveToFile(bool clearAllData = true) {
        bool trySaving = true;
        string exception = "";
        var fileName = _audioTimeStat + "_" +  _date + "_buff_" + bufferSize + "_sr_" + sampleRate + "_round_" + _round + ".csv";
        while (trySaving) {
            try {
                string header = "time, frame_t_ms, gc_mem_mb, system_mem_mb, audio_frame_t_ms, audio_memory_mb, sources\n";
                string data = header;
                for (int i = 0; i < allData.Count; i++) {
                    data += string.Join(",", allData[i]) + "\n";
                }
                System.IO.File.WriteAllText(_fileDirectoryPath + "/" + fileName, data);
                trySaving = false;
            } catch (System.Exception e) {
                //Writing the message in a log file
                exception += e.Message + "\n";
                trySaving = true;
                System.IO.File.WriteAllText(_fileDirectoryPath + "/" + fileName + "_EXC.logerror", exception);
            }
        }
        if (clearAllData) {
            allData.Clear();
        }
        System.GC.Collect();
    }
}
