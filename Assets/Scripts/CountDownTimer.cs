using System;
using TMPro;
using UnityEngine;

public class CountDownTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text countdown;
    private bool _enable;
    private float _startTime;
    private float _totalTime;

    private void Update()
    {
        if (!_enable)
            return;
        var elapsedTime = Time.time - _startTime;
        var remainingTime = _totalTime - elapsedTime;
        countdown.text = $"Time: {remainingTime:F}";
        if (remainingTime < 0)
        {
            EventSystem.TimeFinished.Invoke();
            _enable = false;
        }        

    }

    private void StartGame(GameData gameData)
    {
        _enable = true;
        _startTime = Time.time;
        _totalTime = gameData.gameTimeSeconds;
        countdown.gameObject.SetActive(true);
    }
    
    private void OnEnable()
    {
        EventSystem.StartGame += StartGame;        
    }

    private void OnDisable()
    {
        EventSystem.StartGame -= StartGame;        
    }
}
