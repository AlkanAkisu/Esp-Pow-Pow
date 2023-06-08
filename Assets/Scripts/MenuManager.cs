using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private Vector2 _crossHairPosition = Vector2.negativeInfinity;
    [SerializeField] private RectTransform menuPanel;
    [SerializeField] private RectTransform[] buttons;
    [SerializeField] private Color activeButtonColor;
    [SerializeField] private TMP_Text selectDifficulty;

    private RectTransform _selectedButton;
    private bool _isEnabled = false;
    private bool _buttonState;
    private DifficultyMode _selectedDifficulty = DifficultyMode.None;


    void Update()
    {
        if (!_isEnabled)
            return;
        _selectedButton = null;
        foreach (var button in buttons)
        {
            var isActive = IsOnButton(button, _crossHairPosition);
            ToggleButtonColor(button, isActive);
            if (isActive)
            {
                _selectedButton = button;
            }
        }

        selectDifficulty.text = _selectedDifficulty switch
        {
            DifficultyMode.None => "Select Difficulty",
            DifficultyMode.Easy => $"Selected: Easy",
            DifficultyMode.Medium => $"Selected: Medium",
            DifficultyMode.Hard => $"Selected: Hard",
            DifficultyMode.Chaos => $"Selected: Chaos",
        };
    }

    private void ToggleMenuPanel(bool isActive)
    {
        menuPanel.gameObject.SetActive(isActive);
        _isEnabled = isActive;
    }

    private bool IsOnButton(RectTransform button, Vector2 crossHairPos)
    {
        var buttonSize = button.rect.size;

        var buttonPos = button.anchoredPosition;

        var halfButtonSize = buttonSize / 2;

        var leftBottomCorner = buttonPos - halfButtonSize;
        var rightTopCorner = buttonPos + halfButtonSize;

        return crossHairPos.x > leftBottomCorner.x && crossHairPos.x < rightTopCorner.x &&
               crossHairPos.y > leftBottomCorner.y && crossHairPos.y < rightTopCorner.y;
    }

    private void ToggleButtonColor(RectTransform button, bool isActive)
    {
        if (button.TryGetComponent<Image>(out var image))
        {
            image.color = isActive ? activeButtonColor : Color.white;
        }
        else if (button.TryGetComponent<TMP_Text>(out var txt))
        {
            txt.color = isActive ? activeButtonColor : Color.white;
        }
    }

    private void OnCrossHair(CrossHairData crossHair)
    {
        if (!_isEnabled)
            return;
        _crossHairPosition = crossHair.position - new Vector2(Screen.width / 2f, -Screen.height / 2f);
        if (!_buttonState && crossHair.isButtonPressed)
        {
            ButtonPressed(_selectedButton);
        }

        _buttonState = crossHair.isButtonPressed;
    }

    private void ButtonPressed(RectTransform selectedButton)
    {
        if (selectedButton == null)
            return;
        if (selectedButton.name.ToLower().Contains("easy"))
            _selectedDifficulty = DifficultyMode.Easy;
        else if (selectedButton.name.ToLower().Contains("medium"))
            _selectedDifficulty = DifficultyMode.Medium;
        else if (selectedButton.name.ToLower().Contains("hard"))
            _selectedDifficulty = DifficultyMode.Hard;
        else if (selectedButton.name.ToLower().Contains("chaos"))
            _selectedDifficulty = DifficultyMode.Chaos;
        else if (selectedButton.name.ToLower().Contains("play"))
            PlayGame();
    }


    private void PlayGame()
    {
        ToggleMenuPanel(false);
        _isEnabled = false;
        EventSystem.StartGame.Invoke(new GameData
        {
            DifficultyMode = _selectedDifficulty,
            gameTimeSeconds = 60f
        });
    }

    private void OnEnable()
    {
        EventSystem.OnCrossHair += OnCrossHair;
        EventSystem.ToggleMenuPanel += ToggleMenuPanel;
    }

    private void OnDisable()
    {
        EventSystem.OnCrossHair -= OnCrossHair;
        EventSystem.ToggleMenuPanel -= ToggleMenuPanel;
    }
}

public enum DifficultyMode
{
    None,
    Easy,
    Medium,
    Hard,
    Chaos
}

[Serializable]
public class GameData
{
    public DifficultyMode DifficultyMode;
    public float gameTimeSeconds;
}