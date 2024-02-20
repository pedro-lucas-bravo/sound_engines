using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChuckAgent : AudioAgent
{
    public bool useIndependentMain = false;


    private string _freqId;
    private string _runId;
    private ChuckSubInstance _chuckSubInstance;

    // Start is called before the first frame update
    void Start() {
        var freq_pref = "freq_";
        var freq = Random.Range(minFrequency, maxFrequency);
        if (!useIndependentMain) {
            _chuckSubInstance = GetComponent<ChuckSubInstance>();
            _freqId = _chuckSubInstance.GetUniqueVariableName(freq_pref);
            _runId = _chuckSubInstance.GetUniqueVariableName("run_");
            _chuckSubInstance.RunCode(code(_runId, _freqId));
            _chuckSubInstance.SetFloat(_freqId, freq);
        } else { 
            var _chuckMainInstance = GetComponent<ChuckMainInstance>();
            _freqId = _chuckMainInstance.GetUniqueVariableName(freq_pref);
            _runId = _chuckSubInstance.GetUniqueVariableName("run_");
            _chuckMainInstance.RunCode(code(_runId, _freqId));
            _chuckMainInstance.SetFloat(_freqId, freq);
        }
    }

    string code(string runId, string freqId) {
        return $@"
                1 => global int {runId};
        	    global float {freqId};

                SinOsc s => dac;
                {freqId} => s.freq;
                while( {runId} ) 
                {{
                    {freqId} => s.freq;
                    1::samp => now;
                }}
        ";
    }

    public override void Stop() {
        if(_chuckSubInstance != null)
            _chuckSubInstance.SetInt(_runId, (System.IntPtr)0);
    }

}
