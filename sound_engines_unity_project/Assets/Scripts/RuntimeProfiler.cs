using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;

public class RuntimeProfiler : MonoBehaviour {

    public enum AudioTimeStat { 
        Csound,
        Chuck
    }
 
    public AudioTimeStat audioTimeStat = AudioTimeStat.Csound; 
    public float measureInterval = 0.2f;
    public int numberOfMesurementsToShow = 30;
    public bool showInUI = true;
    public TMP_Text statsLabel;
    public bool saveToFile = true;

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
    //ProfilerRecorder audioTimeRecorder;
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
//#if UNITY_EDITOR
//        audioTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Audio, _audioTimeStat);
//#else
//        audioTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Audio, "Audio.Thread");
//#endif
        audioMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Audio, "Audio Used Memory");
        isRecording = true;
        ResetMeasurements();
        //EnumerateProfilerStats();
    }

    public void StopRecording(bool clearAllData = true) {
        systemMemoryRecorder.Dispose();
        gcMemoryRecorder.Dispose();
        mainThreadTimeRecorder.Dispose();
        //audioTimeRecorder.Dispose();
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
        var index = 0;
        sb.AppendLine($"Time: {dataRow[index++]:F3} s");
        sb.AppendLine($"Frame Time: {dataRow[index++]:F3} ms");
        sb.AppendLine($"GC Memory: {dataRow[index++]:F3} MB");
        sb.AppendLine($"System Memory: {dataRow[index++]:F3} MB");
        sb.AppendLine($"Audio Frame Time: {dataRow[index++]:F3} ms");
        sb.AppendLine($"Audio Memory: {dataRow[index++]:F3} MB");
        sb.AppendLine($"Playing Audio Sources: {dataRow[index++]}");
        return sb.ToString();
    }

    void ResetMeasurements() {
        for (int i = 0; i < currentMeasurements.Length; i++) {
            currentMeasurements[i] = 0;
        }
        currentMeasurementCount = 0;  
    }

    void CollectMeasurement() {
        var totalAudioFrameTime = 0.0;
        var totalMeters = AudioTimeMeasurer.AllData.Count;
        for (int i = 0; i < totalMeters; i++) {
            var sample = AudioTimeMeasurer.AllData[i];
            totalAudioFrameTime += sample.LastFrameTime;
        }

        //var audioTime = audioTimeRecorder.LastValue * (1e-6f);
        //if (_currentPlayingSources == 0 || audioTime > 0.000001) {
        var dataRow = new float[N_dataFields];
        var index = 0;
        dataRow[index++] = Time.timeSinceLevelLoad;
        dataRow[index++] = mainThreadTimeRecorder.LastValue * (1e-6f);
        dataRow[index++] = gcMemoryRecorder.LastValue / (float)(1024 * 1024);
        dataRow[index++] = systemMemoryRecorder.LastValue / (float)(1024 * 1024);
        dataRow[index++] = (float)totalAudioFrameTime;//audioTime;
        dataRow[index++] = audioMemoryRecorder.LastValue / (float)(1024 * 1024);
        dataRow[index++] = _currentPlayingSources;
        allData.Add(dataRow);
        for (int i = 0; i < currentMeasurements.Length; i++) {
            currentMeasurements[i] += dataRow[i];
        }
        currentMeasurements[index - 1] = _currentPlayingSources;
        currentMeasurementCount++;
            //Debug.Log("Data collected: " + StringfyDataRow(dataRow));
            //Debug.LogWarning("Inst: " + totalMeters + " AudioFrameTime: " + totalAudioFrameTime + " ms");
        //}
    }

    void AverageMeasurements() {
        if (currentMeasurementCount > 0) {
            for (int i = 0; i < currentMeasurements.Length - 1; i++) {
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

    public void AddCounterTrial() {
        _currentPlayingSources++;
    }
    public void RemoveCounterTrial() {
        _currentPlayingSources--;
    }

    public void NextRound() {
        _round++;
    }

    //Save all data rows to a csv file
    public void SaveToFile(string prefixFileName = "" , bool clearAllData = true) {
        if (saveToFile) {
            bool trySaving = true;
            string exception = "";
            var fileName = prefixFileName + _audioTimeStat + "_" + _date + "_buff_" + bufferSize + "_sr_" + sampleRate + "_round_" + _round + ".csv";
            while (trySaving) {
                try {
                    //Note: Last column is usually teh number of sources, but for the experiment for hanging processes, it is the trial number
                    string header = "time,frame_t_ms,gc_mem_mb,system_mem_mb,audio_frame_t_ms,audio_memory_mb,sources\n";
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
        }
        if (clearAllData) {
            allData.Clear();
        }
        System.GC.Collect();
    }

    //https://docs.unity3d.com/ScriptReference/Unity.Profiling.LowLevel.Unsafe.ProfilerRecorderDescription.html
    struct StatInfo {
        public ProfilerCategory Cat;
        public string Name;
        public ProfilerMarkerDataUnit Unit;
    }

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

//public class AudioTimeMeasurer {
//    public static List<AudioTimeMeasurer> AllData = new List<AudioTimeMeasurer>();
//    public double LastFrameTime { 
//        get { 
//            lock (lockObject) {
//                return _lastFrameTime;
//            }
//        } 
//    }

//    private double _lastFrameTime;

//    private object lockObject = new object();

//    private System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

//    public AudioTimeMeasurer() {
//        AllData.Add(this);
//    }

//    public void TakeStartTime() {
//        stopwatch.Reset();
//        stopwatch.Start();
//    }

//    public void TakeEndTime() {
//        stopwatch.Stop();
//        lock (lockObject) {
//            _lastFrameTime = stopwatch.Elapsed.TotalMilliseconds;
//        }
//    }

//    public void OnDestroy() {
//        AllData.Remove(this);
//    }
//}
