using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private float timeElapsed;
    public TextMeshProUGUI timerText;
    // Start is called before the first frame update
    void Start()
    {
        timeElapsed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;
        timerText.text = ((float)Math.Round(timeElapsed * 100f) / 100f).ToString();
    }
}
