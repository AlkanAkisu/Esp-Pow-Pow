using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject scorePopupPrefab;
    [SerializeField] private RectTransform scoresParent;
    [SerializeField] private ShotData debugShotData;

    private float _score;

    public float Score => _score;

    private void Update()
    {
        scoreText.text = $"Score: {Score}";
    }

    private void OnShot(ShotData shotData)
    {
        var shotTargets =
            shotData.shotTargets.Count((target => target.Spawnable.spawnableType == SpawnableType.CircleTarget));
        var scoreToAdd = shotTargets * shotTargets;
        var scorePopup = Instantiate(scorePopupPrefab, scoresParent);
        var pos = shotData.shotPosition;
        pos.x -= Screen.width / 2f;
        pos.y += Screen.height / 2f;
        scorePopup.GetComponent<RectTransform>().anchoredPosition = pos;
        scorePopup.GetComponent<TMP_Text>().text = $"+{scoreToAdd}";
        UniTask.Action(async () =>
        {
            var startTime = Time.time;
            var rectTransform = scorePopup.GetComponent<RectTransform>();
            var canvasGroup = scorePopup.GetComponent<CanvasGroup>();
            var startPos = rectTransform.anchoredPosition;
            var endPos = new Vector2(802.8f, 434.5194f);

            while (startTime + 0.3f > Time.time)
            {
                rectTransform.anchoredPosition += Vector2.up * 50 * Time.deltaTime;
                rectTransform.localScale += Vector3.one * Time.deltaTime;
                await UniTask.Yield();
            }

            UniTask.Delay(TimeSpan.FromSeconds(0.2f));
            startTime = Time.time;
            const float lifeTime = 0.5f;
            
          
            while (startTime + lifeTime > Time.time)
            {
                var t = (Time.time - startTime) / lifeTime;
                t *= 0.7f;

                rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);

                canvasGroup.alpha = Mathf.Lerp(1, 0.3f, t);
                await UniTask.Yield();
            }

            AddScore(scoreToAdd);
            Destroy(scorePopup);
        }).Invoke();
    }

    private void AddScore(int scoreToAdd)
    {
        _score = Score + scoreToAdd;
    }
    private void OnStartGame(GameData gameData)
    {
        scoreText.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        EventSystem.OnShot += OnShot;
        EventSystem.StartGame += OnStartGame;
    }

    private void OnDisable()
    {
        EventSystem.OnShot -= OnShot;
        EventSystem.StartGame -= OnStartGame;

    }
}