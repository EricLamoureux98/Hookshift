using System.Collections;
using TMPro;
using UnityEngine;

public class TimeTrial : MonoBehaviour
{
    [SerializeField] TMP_Text timerText;
    [SerializeField] CanvasGroup timerAlpha;
    [SerializeField] float flashDuration = 999f;
    [SerializeField] float flashInterval = 0.05f;
    
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

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (timerActive) return;

            StartTimer();
            StopAllCoroutines();
            timerAlpha.alpha = 1f;
            //Debug.Log("Player started the timer");
        }
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

    IEnumerator TimerFlash()
    {
        float timer = 0f;
        while (timer < flashDuration)
        {
            if (timerAlpha != null)
            {
                timerAlpha.alpha = (timerAlpha.alpha == 1f) ? 0f : 1f;
            }
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }        

        // Ensures alpha is enabled at the end 
        if (timerAlpha != null)
        {
            timerAlpha.alpha = 1f;
        }
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
}
