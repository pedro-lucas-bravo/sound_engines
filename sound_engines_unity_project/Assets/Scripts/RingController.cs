using Oculus.Interaction;
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

    private List<AudioAgent> _agents = new List<AudioAgent>();

    // Start is called before the first frame update
    private void Start() {
        for (int i = 0; i < agentsSize; i++) {
            var agent = Instantiate(audioAgentPrefab);
            _agents.Add(agent);
        }
        sizeLabel.text = "N: " + _agents.Count;

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
        if (Input.GetKeyDown(KeyCode.A)) {
            AddAgent();
        }

        if(Input.GetKeyDown(KeyCode.D) && _agents.Count > 0) {
            RemoveAgent();
        }

    }

    void AddAgent() {
        var agent = Instantiate(audioAgentPrefab);
        _agents.Add(agent);
        sizeLabel.text = "N: " + _agents.Count;
    }

    void RemoveAgent() {
        if (_agents.Count > 0) {
            var agent = _agents[0];
            agent.Stop();
            _agents.Remove(agent);
            Destroy(agent.gameObject);
            sizeLabel.text = "N: " + _agents.Count;
        }
    }
}
