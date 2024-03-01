using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioAgent : MonoBehaviour
{    
    public float radius = 1.0f;
    public float speed = 2.0f;

    private Renderer _renderer;
    private Transform _center;
    private Transform _trans;

    protected virtual void Awake() {
        _trans = transform;
        _center = GameObject.FindGameObjectWithTag("Center").transform;
        _renderer = GetComponent<Renderer>();
        _renderer.material.color = Color.HSVToRGB(Random.Range(0, 1f), 1f, 1f);
        _currentAngle = Random.value * (2f * Mathf.PI);
        UpdatePosition();
        
    }

    private void FixedUpdate() {
        _currentAngle += Time.deltaTime * speed;
        UpdatePosition();
    }

    private void UpdatePosition() {
        _trans.position = _center.position + new Vector3(Mathf.Cos(_currentAngle), 0f, Mathf.Sin(_currentAngle)) * radius;
    }

    private float _currentAngle = 0f;

    public virtual void Stop() {
    }

}
