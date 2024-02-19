using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleColorChanger : MonoBehaviour {
    public Renderer graphics;
    public Color color;

    // Start is called before the first frame update
    void Start() {
        graphics.material.color = color;
    }
}
