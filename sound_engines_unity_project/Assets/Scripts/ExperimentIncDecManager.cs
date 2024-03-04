using UnityEngine;

//Experiment for incrementing and decrementing agents
public class ExperimentIncDecManager : MonoBehaviour
{
    public enum State { None, Playing, Resting}

    public RingController controller;
    public RuntimeProfiler profiler;
    public int numberOfSources = 3;
    public int numberOfTrialsPerSource = 30;
    public int numberOfUpDownRounds = 2;
    public float restPeriodSec = 1f;

    private State _currentState = State.None;
    private float _restTimer = 0f;

    void Start() {
        //Wait the resting time to start the experiment
        _currentState = State.Resting;
    }

    public void SaveAndFinishRound() {
        profiler.StopRecording(false);
        profiler.SaveToFile();        
    }

    public void GoToNextRound() {
        profiler.NextRound();
        _currentState = State.Resting;
    }

    public void StopExperiment() {
        _currentState = State.None;
        Debug.LogWarning("Experiment finished");
        //Close the application on editor or android
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void  AddSource() {
        controller.AddAgent();
        profiler.AddCounterTrial();
    }

    private void RemoveSource() {
        controller.RemoveAgent();
        profiler.RemoveCounterTrial();
    }

    private bool RoundIsUp => profiler.Round % 2 == 0;
    private int _agentOffset = 0;

    private void Update() {
        switch(_currentState) {
            case State.None:
                break;
            case State.Playing:
                if ((RoundIsUp && profiler.CurrentDataCount >= (controller.AgentCount + _agentOffset + 1) * numberOfTrialsPerSource)
                    ||
                    (!RoundIsUp && profiler.CurrentDataCount >= ((numberOfSources - controller.AgentCount) + _agentOffset + 1) * numberOfTrialsPerSource)) {
                    if (RoundIsUp && controller.AgentCount < numberOfSources) {
                        profiler.StopRecording(false);
                        AddSource();
                        _currentState = State.Resting;
                    } else {
                        if (!RoundIsUp && controller.AgentCount > 0) {
                            profiler.StopRecording(false);
                            RemoveSource();
                            _currentState = State.Resting;
                        } else {
                            SaveAndFinishRound();
                            if (profiler.Round < numberOfUpDownRounds - 1) {
                                GoToNextRound();
                                if (RoundIsUp)
                                    AddSource();
                                else
                                    RemoveSource();
                                _agentOffset = -1;
                            } else {
                                StopExperiment();
                            }
                        }
                    } 
                }
                break;
            case State.Resting:
                _restTimer += Time.deltaTime;
                if (_restTimer >= restPeriodSec) {
                    profiler.StartRecording();
                    _restTimer = 0f;
                    _currentState = State.Playing;
                }
                break;
        }
    }


}
