using Oculus.Interaction;
using Oculus.Platform;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RingController : MonoBehaviour
{
    public AudioAgent audioAgentPrefab;
    public int agentsSize = 5;
    public TMP_Text sizeLabel;
    public PokeInteractable pokeInteractableAdd;
    public PokeInteractable pokeInteractableRemove;

    private AudioAgent[] _agents;
    private string[] _labels;
    private int maxAgents = 2000;
    private int _agentCount = 0;

    private void Awake() {
        //To avoid garbage collection
        _labels = new string[maxAgents];
        for (int i = 0; i < maxAgents; i++) {
            _labels[i] = "N: " + i;
        }
        _agents = new AudioAgent[maxAgents];
    }

    // Start is called before the first frame update
    private void Start() {
        for (int i = 0; i < agentsSize; i++) {
            var agent = Instantiate(audioAgentPrefab);
            _agents[i] = agent;
        }
        _agentCount = agentsSize;
        sizeLabel.text = _labels[_agentCount];

        pokeInteractableAdd.WhenStateChanged += (state) => {
            if (state.NewState == InteractableState.Normal) {
                AddAgent();
            }
        };

        pokeInteractableRemove.WhenStateChanged += (state) => {
            if (state.NewState == InteractableState.Normal) {
                RemoveAgent();
            }
        };
    }

    // Update is called once per frame
    void Update()
    {
        //OVRInput.Update();
        if (Input.GetKeyDown(KeyCode.A) ||
            OVRInput.GetDown(OVRInput.Button.One)) {
            AddAgent();
        }

        if((Input.GetKeyDown(KeyCode.D) ||
            OVRInput.GetDown(OVRInput.Button.Two)) && _agentCount > 0) {
            RemoveAgent();
        }

    }

    void AddAgent() {
        var agent = Instantiate(audioAgentPrefab);
        _agents[_agentCount] = agent;
        _agentCount++;
        sizeLabel.text = _labels[_agentCount];
    }

    void RemoveAgent() {
        if (_agentCount > 0) {
            _agentCount--;
            var agent = _agents[_agentCount];
            agent.Stop();            
            Destroy(agent.gameObject);
            sizeLabel.text = _labels[_agentCount];
        }
    }
}
