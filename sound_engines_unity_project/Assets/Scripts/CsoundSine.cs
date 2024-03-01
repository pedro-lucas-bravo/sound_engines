using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CsoundSine : AudioAgent
{
    [Header("Audio Params")]
    public float minFrequency = 220.0f;
    public float maxFrequency = 1760.0f;

    private CsoundUnity _csoundObject;

    protected override void Awake() {
        base.Awake();
        _csoundObject = GetComponent<CsoundUnity>();
        var frequency = Random.Range(minFrequency, maxFrequency);
        SetFrequency(frequency).Forget();
    }

    private async UniTask SetFrequency(float frequency) {
        while (!_csoundObject.IsInitialized)
            await UniTask.Yield();
        _csoundObject.SetChannel("freq", frequency);
    }

}
