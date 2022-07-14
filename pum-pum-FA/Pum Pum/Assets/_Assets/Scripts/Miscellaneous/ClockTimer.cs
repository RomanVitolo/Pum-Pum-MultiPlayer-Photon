using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ClockTimer : MonoBehaviour
{
    [Header("---- Clock Parameters ----")]
    [SerializeField] private TextMeshProUGUI _clockText = null;
    [SerializeField] private float _targetTime = 10;

    [Header("---- Clock Direction -- Complete just if you need countdown ----")]
    [SerializeField] private bool _countdown = true;
    [SerializeField] private float _startCountdownTime = 10;

    public event Action OnClockRunOutOfTime;

    private float _clockTime = 0;
    private float _miliseconds = 0;
    private float _seconds = 0;
    private float _minutes = 0;
    private int _timerDirection = 1;

    public float ClockTime { get => _clockTime; set => _clockTime = value; }

    private void Start()
    {
        if (_countdown)
        {
            _timerDirection *= -1;
            _clockTime = _startCountdownTime;
        }
        else _timerDirection = 1;

        ExecuteClock();
    }

    private void ExecuteClock()
    {
        if (_countdown)
        {
            StartCoroutine(CountdownTimer());
        }
        else
        {
            StartCoroutine(StandardTimer());
        }
    }

    IEnumerator CountdownTimer() //Whiles diferentes para poder capear el reloj cuando llega al target time y que no siga sumando o restando tiempo
    {
        while (_clockTime > 0)
        {
            ExecuteTimer();
            yield return null;
        }

        OnClockRunOutOfTime?.Invoke();
    }

    IEnumerator StandardTimer()
    {
        while (_clockTime - _targetTime < 0)
        {
            ExecuteTimer();
            yield return null;
        }
    }

    private void ExecuteTimer()
    {
        _clockTime += Time.deltaTime * _timerDirection;
        //_miliseconds = (int)((_time - (int)_time) * 100);
        _seconds = (int)(_clockTime % 60);
        _minutes = (int)(_clockTime / 60 % 60);

        if(_clockText != null) _clockText.text = string.Format("{0:00}:{1:00}", _minutes, _seconds);
    }

    public void SetTimerCountdownParameters(float startCountdownTime, float targetTime)
    {
        _countdown = true;
        _startCountdownTime = startCountdownTime;
        _targetTime = targetTime;
    }
}
