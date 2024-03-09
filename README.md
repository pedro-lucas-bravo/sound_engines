# Csound vs ChucK: Sound Generation for XR Multi-Agent Audio Systems in the Meta Quest 3 using the Unity Game Engine

## Important:

When open the project, you will find compilation errors due to a missing class called **AudioTimeMeasurer**. 
This is because this class was added to the **CsoundUnity.cs** file to perform the measurements and should be there since **CsoundUnity** is a Unity package and therefore is not tracked by Git. 
If your intention is not dealing with the performance measurements, then you can remove any reference to this class in the project; otherwise, you need to follow the next steps:

1. Find the **CsoundUnity.cs** class in the local installation of your package and add the **AudioTimeMeasurer** to the very end of this file after the main class. This is the class:

```
public class AudioTimeMeasurer {

    public static List<AudioTimeMeasurer> AllData = new List<AudioTimeMeasurer>();

    private object _lock = new object();

    public double LastFrameTime {
        get {
            lock (_lock) {
                return _lastFrameTime;
            }
        }
    }

    private double _lastFrameTime;

    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch() ;

    public AudioTimeMeasurer() {
        AllData.Add(this);
    }

    public void OnDestroy() {
        AllData.Remove(this);
    }

    public void TakeStartTime() {
        stopwatch.Reset();
        stopwatch.Start();
        //_startTime = System;
    }

    public void TakeEndTime() {
        stopwatch.Stop();
        lock (_lock) {
            _lastFrameTime = stopwatch.Elapsed.TotalMilliseconds;
        }
    }
}
```

2. In the **CsoundUnity** class of this same file, add a new private variable ``` private AudioTimeMeasurer _audioTimeMeasurer; ```.
3. Look for the **Awake()** function and add ``` _audioTimeMeasurer = new AudioTimeMeasurer(); ``` at the end of it.
4. Look for the *OnAudioFilterRead* function in the same file and place the functions for measuring the DSP time. The function should look like this after that change:

```

void OnAudioFilterRead(float[] data, int channels)
{
    
    if (csound != null)
    {
        _audioTimeMeasurer?.TakeStartTime();
        ProcessBlock(data, channels);
        _audioTimeMeasurer?.TakeEndTime();
    }
    
}

```

That's it. Now you shouldn't get any errors.

