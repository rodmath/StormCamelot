using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chronos;

public class TimeControl : MonoBehaviour
{
    Timeline[] timelines;
    Clock clock;

    private void Start()
    {
        timelines = FindObjectsOfType<Timeline>();
        clock = Timekeeper.instance.Clock("Root");
    }

    void Update()
    {
        if (false && clock.localTimeScale < 0f)
        {
            bool stopRewind = false;
            foreach (Timeline timeline in timelines)
            {

                if (timeline.isActiveAndEnabled)
                    if (timeline.rewindable)
                        if (timeline.availableRewindDuration <= clock.deltaTime)
                            stopRewind = true;
            }

            if (stopRewind)
            {
                Debug.Log("Stopping rewinding");
                clock.localTimeScale = 0f;
            }
        }

        // Get the Enemies global clock


        //// Change its time scale on key press
        if (Input.GetKeyDown(KeyCode.Q))
        {
            clock.localTimeScale = -1; // Rewind
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            clock.localTimeScale = 0; // Pause
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            clock.localTimeScale = 0.5f; // Slow
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            clock.localTimeScale = 1; // Normal
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            clock.localTimeScale = 2; // Accelerate
        }
    }
}