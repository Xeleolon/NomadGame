using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public Slider mouseSlider;
    float playerCameraSensitivity;

    public GameObject videoPlayerCanvas;
    [Tooltip("time before video will start playing on it own")]

    public float inActiveMax = 5;
    private float inActiveClock;

    [Tooltip("delay between activating of video before inactive will be dectected")]
    public float inActiveDelayReset = 1;
    private float inActiveDelayClock;
    private bool disableVideo;
    void Start()
    {
        mouseSlider.value = playerCameraSensitivity;
    }

    void Update()
    {
        if (!videoPlayerCanvas.activeSelf)
        {
            if (inActiveClock > inActiveMax)
            {
                PlayVideo(true);
            }
            else
            {
                inActiveClock += 1 * Time.deltaTime;
            }
        }
        else if (videoPlayerCanvas.activeSelf)
        {
            if (inActiveDelayClock > inActiveDelayReset)
            {
                disableVideo = true;
            }
            else
            {
                inActiveDelayClock += 1 * Time.deltaTime;
            }
        }
    }

    public void PlayVideo(bool play)
    {
        inActiveClock = 0;
        if (play)
        {
            if (!videoPlayerCanvas.activeSelf)
            {
                videoPlayerCanvas.SetActive(true);
            }
        }
        else
        {
            if (videoPlayerCanvas.activeSelf)
            {
                videoPlayerCanvas.SetActive(false);
            }
        }
    }

    public void OpenScene(string sceneName)
    {
        if (sceneName != null)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
    public void WindowMode(bool windowed)
    {
        Screen.fullScreen = !windowed;
    }

    public void SetMouseSensitivity()
    {
        playerCameraSensitivity = mouseSlider.value;
    }
}
