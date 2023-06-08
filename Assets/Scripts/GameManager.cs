using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UniTask.Action(async () =>
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
            EventSystem.StartCalibration.Invoke();
        }).Invoke();
    }

    private void OnCalibrationFinished(List<CalibrationResult> obj)
    {
        EventSystem.ToggleMenuPanel.Invoke(true);
    }

    private void OnEnable()
    {
        EventSystem.OnCalibrationFinished += OnCalibrationFinished;
    }

    private void OnDisable()
    {
        EventSystem.OnCalibrationFinished -= OnCalibrationFinished;
    }
}