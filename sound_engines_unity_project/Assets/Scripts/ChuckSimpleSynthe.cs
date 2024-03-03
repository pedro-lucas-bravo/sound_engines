using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChuckSimpleSynthe : AudioAgent {
    [Header("Audio Params")]
    public float changePeriod = 0.25f;
    public float minFrequency = 220.0f;
    public float maxFrequency = 1760.0f;
    public float minCuttoff = 400.0f;
    public float maxCuttoff = 1000.0f;
    public float minResonance = 10.0f;
    public float maxResonance = 400.0f;
    public float minReverb = 0.5f;
    public float maxReverb = 1.0f;
    public float gain = 0.1f;

    private ChuckSubInstance _chuckSubInstance;
    private float _changeTimer;

    private string _runId;
    private string _freqId;
    private string _cutOffId;
    private string _resonanceId;
    private string _reverbId;
    private string _gainId;

    protected override void Awake() {
        base.Awake();

        _chuckSubInstance = GetComponent<ChuckSubInstance>();
        _runId = _chuckSubInstance.GetUniqueVariableName("run");
        _freqId = _chuckSubInstance.GetUniqueVariableName("freq");
        _cutOffId = _chuckSubInstance.GetUniqueVariableName("cutOff");
        _resonanceId = _chuckSubInstance.GetUniqueVariableName("resonance");
        _reverbId = _chuckSubInstance.GetUniqueVariableName("revmix");
        _gainId = _chuckSubInstance.GetUniqueVariableName("gain");
        _chuckSubInstance.RunCode(code());

        _changeTimer = changePeriod;
    }

    public override void Stop() {
        if (_chuckSubInstance != null)
#if UNITY_ANDROID
            _chuckSubInstance.SetInt(_runId, (System.IntPtr)0);
#else
            _chuckSubInstance.SetInt(_runId, 0);
#endif
    }

    string code() {
        return $@"
                global Event noteOn;
                global Event noteOff;

                1 => global int {_runId};
        	    global float {_freqId};
                global float { _cutOffId };
                global float { _resonanceId };
                global float { _reverbId };
                global float { _gainId };

                SawOsc s => LPF filter => JCRev reverb => ADSR e => Gain amp => dac;                

                e.set( 100::ms, 200::ms, 0.7, 500::ms );

                while( {_runId} ) 
                {{
                    noteOn => now;
                    {_freqId} => s.freq;
                    {_cutOffId} => filter.freq;
                    {_resonanceId} => filter.Q;
                    {_reverbId} => reverb.mix;
                    {_gainId} => amp.gain;
                    1::samp => now;
                    e.keyOn();
                    noteOff => now;
                    e.keyOff();
                    e.releaseTime() => now;
                }}
        ";
    }

    bool _playRound = true;
    private void Update() {
        _changeTimer += Time.deltaTime;
        if (_changeTimer >= changePeriod) {
            _changeTimer = 0;
            _playRound = !_playRound;
            if (_playRound) {
                _chuckSubInstance.BroadcastEvent("noteOff");
                return;
            }

            //Frequency freq
            var frequency = Random.Range(minFrequency, maxFrequency);
            _chuckSubInstance.SetFloat(_freqId, frequency);
            //Filter cutoff
            var cuttoff = Random.Range(minCuttoff, maxCuttoff);
            _chuckSubInstance.SetFloat(_cutOffId, cuttoff);
            //Filter resonance
            var resonance = Random.Range(minResonance, maxResonance);
            _chuckSubInstance.SetFloat(_resonanceId, resonance);
            //Reverb revtime
            var reverb = Random.Range(minReverb, maxReverb);
            _chuckSubInstance.SetFloat(_reverbId, reverb);
            //gain
            _chuckSubInstance.SetFloat(_gainId, gain);

            _chuckSubInstance.BroadcastEvent("noteOn");

            //Debug.Log($"freq: {frequency}, fc: {cuttoff}, Q: {resonance}, revtime: {reverb}");
        }
    }
}
