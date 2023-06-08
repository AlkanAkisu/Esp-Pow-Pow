using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class EventSystem
{
    public static Action<EspData> OnEspData = delegate { };
    public static Action<List<CalibrationResult>> OnCalibrationFinished = delegate { };
    public static Action<CrossHairData> OnCrossHair = delegate { };
    public static Action<bool> ToggleShooting = delegate { };
    public static Action<bool> ToggleMenuPanel = delegate { };
    public static Action<int> OnCalibrationStep = delegate { };
    public static Action StartCalibration = delegate { };
    public static Action<GameData> StartGame = delegate { };
    public static Action<ShotData> OnShot = delegate { };
    public static Action TimeFinished = delegate { };
}

