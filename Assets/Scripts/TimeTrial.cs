using System.Collections;
using TMPro;
using UnityEngine;

public class TimeTrial : MonoBehaviour
{
    [SerializeField] TMP_Text timerText;
    [SerializeField] CanvasGroup timerAlpha;
    [SerializeField] float flashInterval = 0.5f;
    
    float time;
    bool timerActive = false;

    void Start()
    {
        if (!timerActive)
        {
            StartCoroutine(TimerFlash());
        }
    }

    void Update()
    {
        RunTimer();
    }

    void RunTimer()
    {
        if (timerActive)
        {
            time += Time.deltaTime;            
        }

        int seconds = Mathf.FloorToInt(time % 60);
        int minutes = Mathf.FloorToInt((time / 60) % 60);
        //int hours = Mathf.FloorToInt((time / 3600) % 24);

        timerText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
    }

    void StartTimer()
    {
        timerActive = true;
    }

    public void FirstPointReached()
    {
        StartTimer();
        StopAllCoroutines();
        timerAlpha.alpha = 1f;
    }

    public void ResetTimer()
    {
        timerActive = false;
        time = 0f;

        if (!timerActive)
        {
            StartCoroutine(TimerFlash());
        }
    }

    public void StopTimer()
    {
        timerActive = false;
        StartCoroutine(TimerFlash());
    }
    
    IEnumerator TimerFlash()
    {
        while (true)
        {
            if (timerAlpha != null)
            {
                // Flips value (ternary)
                timerAlpha.alpha = (timerAlpha.alpha == 0f) ? 1f : 0f;
            }
            yield return new WaitForSeconds(flashInterval);
        }        
    }
}


// Todo: 

// Save current time at each checkpoint so that it can be reverted after death