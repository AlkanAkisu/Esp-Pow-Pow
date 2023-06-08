using UnityEngine;

public class CalibrationPanelManager : MonoBehaviour
{
    [SerializeField] private Transform calibrationPanel;

    private void TogglePanel(bool isOpen)
    {
        calibrationPanel.gameObject.SetActive(isOpen);
    }

    private void SetArrows(int index)
    {
        for (var i = 0; i < 4; i++)
        {
            calibrationPanel.GetChild(i).gameObject.SetActive(false);
        }

        calibrationPanel.GetChild(index).gameObject.SetActive(true);
    }

    private void OnCalibrationStep(int index)
    {
        if (index == 0)
        {
            TogglePanel(true);
        }
        else if (index > 3)
        {
            TogglePanel(false);
            return;
        }
        SetArrows(index);
    }

    private void OnEnable()
    {
        EventSystem.OnCalibrationStep += OnCalibrationStep;
    }

    private void OnDisable()
    {
        EventSystem.OnCalibrationStep -= OnCalibrationStep;
    }
}