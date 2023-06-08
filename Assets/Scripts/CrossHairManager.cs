using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CrossHairManager : MonoBehaviour
{
    private Vector2 _angleData = Vector2.zero;
    private bool crossHairEnabled = false;

    [SerializeField] private RectTransform crossHairObject;
    private ScreenGyroCalculations _screenGyroCalc;
    [SerializeField] private float maxSpeed;
    private bool _isButtonPressed;

    private void OnEspData(EspData data)
    {
        _angleData.x = data.angleZ;
        _angleData.y = data.angleY;
        _isButtonPressed = data.buttonState;
    }

    private void Start()
    {
        crossHairObject.gameObject.SetActive(crossHairEnabled);
        Debug.Log($"{Screen.width},{Screen.height}");
    }

    private void Update()
    {
        if (!crossHairEnabled)
            return;
        crossHairObject.gameObject.SetActive(crossHairEnabled);

        var target = CalculateCrossHairPos(_screenGyroCalc, _angleData);
        var current = Vector2.MoveTowards(crossHairObject.anchoredPosition, target, maxSpeed / Time.deltaTime);
        EventSystem.OnCrossHair.Invoke(new CrossHairData
        {
            position = current,
            isButtonPressed = _isButtonPressed
        });
        crossHairObject.anchoredPosition = current;
    }

    private Vector2 CalculateCrossHairPos(ScreenGyroCalculations screenGyroCalc, Vector2 angleData)
    {
        Vector2 crossHairPos = Vector2.zero;
        var (left, right, down, up) =
            (screenGyroCalc.left, screenGyroCalc.right, screenGyroCalc.down, screenGyroCalc.up);
        if (angleData.x > left)
        {
            crossHairPos.x = Screen.width;
            screenGyroCalc.left = angleData.x;
            screenGyroCalc.right = angleData.x - screenGyroCalc.widthDiff;
        }
        else if (angleData.x < right)
        {
            crossHairPos.x = 0;
            screenGyroCalc.right = angleData.x;
            screenGyroCalc.left = angleData.x + screenGyroCalc.widthDiff;
        }
        else
        {
            crossHairPos.x = Screen.width * (1 - Mathf.InverseLerp(right, left, angleData.x));
        }

        if (angleData.y > down)
            screenGyroCalc.down = angleData.y;
        else if (angleData.y < up)
            screenGyroCalc.up = angleData.y;
        else
            crossHairPos.y = -Screen.height * Mathf.InverseLerp(up, down, angleData.y);


        return crossHairPos;
    }

    private void OnCalibrationFinished(List<CalibrationResult> calibration)
    {
        var upLeft = calibration[0];
        var upRight = calibration[1];
        var downRight = calibration[2];
        var downLeft = calibration[3];
        _screenGyroCalc = new ScreenGyroCalculations()
        {
            left = Mathf.Max(upLeft.AngleZ, downLeft.AngleZ),
            right = Mathf.Min(upRight.AngleZ, downRight.AngleZ),
            up = Mathf.Min(upRight.AngleY, upLeft.AngleY),
            down = Mathf.Max(downRight.AngleY, downLeft.AngleY),
        };
        _screenGyroCalc.heightDiff = Mathf.Abs(_screenGyroCalc.up - _screenGyroCalc.down);
        _screenGyroCalc.widthDiff = Mathf.Abs(_screenGyroCalc.left - _screenGyroCalc.right);
        crossHairEnabled = true;
        UniTask.Action(async () =>
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
            EventSystem.ToggleShooting.Invoke(transform);
        }).Invoke();
    }

    // x sağ negatif sol > sa*
    // y yukarı negatif

    private void OnEnable()
    {
        EventSystem.OnEspData += OnEspData;
        EventSystem.OnCalibrationFinished += OnCalibrationFinished;
    }

    private void OnDisable()
    {
        EventSystem.OnEspData -= OnEspData;
        EventSystem.OnCalibrationFinished -= OnCalibrationFinished;
    }
}

[Serializable]
public class ScreenGyroCalculations
{
    public float left;
    public float right;
    public float up;
    public float down;
    public float heightDiff;
    public float widthDiff;

    public float GetWidthRatio(float gyroVal)
    {
        return Mathf.InverseLerp(left, right, gyroVal);
    }

    public float GetHeightRatio(float gyroVal)
    {
        return Mathf.InverseLerp(up, down, gyroVal);
    }
}

public class CrossHairData
{
    public Vector2 position;
    public bool isButtonPressed;
}