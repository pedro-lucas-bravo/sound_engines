using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CsoundAgent : AudioAgent
{   
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
