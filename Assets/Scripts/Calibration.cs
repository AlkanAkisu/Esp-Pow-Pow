using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Calibration : MonoBehaviour
{
    [SerializeField] private List<CalibrationResult> _calibrationResults;
    private float _angleY;
    private float _angleZ;
    private bool buttonState;

    private CalibrationCorner[] _calibrationCorners =
        {CalibrationCorner.UpLeft, CalibrationCorner.UpRight, CalibrationCorner.DownRight, CalibrationCorner.DownLeft};

    private bool _buttonClicked;
    private int calibrationIndex;

    private void OnEspData(EspData espData)
    {
        _angleY = espData.angleY;
        _angleZ = espData.angleZ;
        _buttonClicked = false;
        if (!buttonState && espData.buttonState)
        {
            _buttonClicked = true;
        }

        buttonState = espData.buttonState;
    }

    
    public void StartCalibration()
    {
        calibrationIndex = -1;
        EventSystem.OnCalibrationStep.Invoke(calibrationIndex+1);
        UniTask.Action(async () =>
        {
            while (calibrationIndex < (int) CalibrationCorner.count - 1)
            {
                while (!_buttonClicked)
                    await UniTask.Yield();
                _buttonClicked = false;
                calibrationIndex++;
                _calibrationResults.Add(new CalibrationResult()
                {
                    AngleY = _angleY,
                    AngleZ = _angleZ,
                    CalibrationCorner = (CalibrationCorner) calibrationIndex
                });
                EventSystem.OnCalibrationStep.Invoke(calibrationIndex+1);
                await UniTask.DelayFrame(2);
            }

            EventSystem.OnCalibrationFinished?.Invoke(_calibrationResults);
        }).Invoke();
    }

    private void OnEnable()
    {
        EventSystem.OnEspData += OnEspData;
        EventSystem.StartCalibration += StartCalibration;
        
    }

    private void OnDisable()
    {
        EventSystem.OnEspData -= OnEspData;
        EventSystem.StartCalibration -= StartCalibration;
    }
}

[Serializable]
public class CalibrationResult
{
    public CalibrationCorner CalibrationCorner;
    public float AngleY;
    public float AngleZ;
}

public enum CalibrationCorner
{
    UpLeft,
    UpRight,
    DownRight,
    DownLeft,
    count
}