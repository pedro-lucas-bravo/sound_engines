using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

//Experiment for detecting hanging processes, if
//memory and CPU still works over audio eventhough there is not sources after adding and removing
public class ExperimentHangProcManager : MonoBehaviour
{
    public enum State { None, Sampling, WaitingToAdd, WaitingToRemove, WaitingToSample }

    public RingController controller;
    public RuntimeProfiler profiler;

    public int trials = 30;
    //public int agentsToAddRemove = 1;
    public float addRemovePeriod = 1f;
    public int samplesPerTrial = 30;

    private State _currentState = State.None;
    private float _waitTimer = 0f;
    private bool _isFirst = true;
    private int _trialCounter = 0;

    // Start is called before the first frame update
    void Start()
    {
        //Wait the resting time to start the experiment
        _currentState = State.WaitingToAdd;
    }

    private void StartTrial(bool isFirst) {
        if (!isFirst) {
            controller.AddAgent();
            profiler.AddCounterTrial();
        }
        _trialCounter++;
    }

    private void RemoveSource(bool isFirst) {
        if (!isFirst)
            controller.RemoveAgent();
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

    // Update is called once per frame
    void Update()
    {
        switch (_currentState) {
            case State.None:
                break;
            case State.Sampling:
                if (profiler.CurrentDataCount >= samplesPerTrial * _trialCounter)
                {                    
                    profiler.StopRecording(false);
                    if (_trialCounter >= trials + 1) {                        
                        profiler.SaveToFile("hang_");
                        StopExperiment();
                    } else {
                        _currentState = State.WaitingToAdd;
                    }
                }
                break;
            case State.WaitingToAdd:
                _waitTimer += Time.deltaTime;
                if (_waitTimer >= addRemovePeriod) {
                    StartTrial(_isFirst);                    
                    _waitTimer = 0f;
                    _currentState = State.WaitingToRemove;
                }
                break;
            case State.WaitingToRemove:
                _waitTimer += Time.deltaTime;
                if (_waitTimer >= addRemovePeriod) {
                    RemoveSource(_isFirst);
                    _isFirst = false;
                    _waitTimer = 0f;
                    _currentState = State.WaitingToSample;
                }
                break;
            case State.WaitingToSample:
                _waitTimer += Time.deltaTime;
                if (_waitTimer >= addRemovePeriod) {
                    profiler.StartRecording();
                    _waitTimer = 0f;
                    _currentState = State.Sampling;
                }
                break;
            
        }
    }
}
