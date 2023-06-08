using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameManager : MonoBehaviour
{
    [SerializeField] private RectTransform endGamePanel;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private RectTransform playAgainButton;
    private Vector2 _crossHairPos;
    private bool _buttonState;

    private void TimeFinished()
    {
        endGamePanel.DOAnchorPos(Vector2.zero, 0.6f).SetEase(Ease.OutBounce);
        var scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreText.text = $"Score: {scoreManager.Score}";
        }
        UniTask.Action(async () =>
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2.5f));
            while (true)
            {
                if (_buttonState )
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    break;
                }

                await UniTask.Yield();
            }
        }).Invoke();
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

    private void OnCrossHair(CrossHairData data)
    {
        _crossHairPos = data.position - new Vector2(Screen.width / 2f, -Screen.height / 2f);
        _buttonState = data.isButtonPressed;
    }

    private void OnEnable()
    {
        EventSystem.TimeFinished += TimeFinished;
        EventSystem.OnCrossHair += OnCrossHair;
    }

    private void OnDisable()
    {
        EventSystem.TimeFinished -= TimeFinished;
        EventSystem.OnCrossHair -= OnCrossHair;
    }
}