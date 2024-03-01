using UnityEngine;

public class CsoundSimpleSynthe : AudioAgent
{
    [Header("Audio Params")]
    public float changePeriod = 0.25f;
    public float minFrequency = 220.0f;
    public float maxFrequency = 1760.0f;
    public float minCuttoff = 400.0f;
    public float maxCuttoff = 1000.0f;
    public float minResonance = 10.0f;
    public float maxResonance = 400.0f;
    public float minReverb = 1.0f;
    public float maxReverb = 4.0f;
    public float gain = 0.1f;

    private CsoundUnity _csoundObject;
    private float _changeTimer;

    protected override void Awake() {
        base.Awake();
        _csoundObject = GetComponent<CsoundUnity>();
        _changeTimer = changePeriod;
    }

    private void Update() {
        if(!_csoundObject.IsInitialized) return;

        _changeTimer += Time.deltaTime;
        if(_changeTimer >= changePeriod) {
            _changeTimer = 0;
            //Frequency freq
            var frequency = Random.Range(minFrequency, maxFrequency);
            _csoundObject.SetChannel("freq", frequency);
            //Filter cutoff
            var cuttoff = Random.Range(minCuttoff, maxCuttoff);
            _csoundObject.SetChannel("fc", cuttoff);
            //Filter resonance
            var resonance = Random.Range(minResonance, maxResonance);
            _csoundObject.SetChannel("Q", resonance);
            //Reverb revtime
            var reverb = Random.Range(minReverb, maxReverb);
            _csoundObject.SetChannel("revtime", reverb);
            //gain
            _csoundObject.SetChannel("gain", gain);

            //Debug.Log($"freq: {frequency}, fc: {cuttoff}, Q: {resonance}, revtime: {reverb}");
        }
    }
}
