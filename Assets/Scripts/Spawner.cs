using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [SerializeField] List<MultiSpawnWithDifficulty> multiSpawn;
    [SerializeField] private RectTransform targetCircles;
    [SerializeField] private GameObject targetCircle;


    public void MultipleSpawn(MultipleSpawn multipleSpawn)
    {
        Debug.Log($"Multiple Spawn");
        UniTask.Action(async () =>
        {
            var i = 0;
            var startTime = Time.time;
            Func<int, bool> condition =
                multipleSpawn.spawnFinishMode == SpawnFinishMode.NumOfSpawn
                    ? t => t < multipleSpawn.numOfSpawns
                    : time => time - startTime < multipleSpawn.seconds;
            while (condition(multipleSpawn.spawnFinishMode == SpawnFinishMode.NumOfSpawn
                       ? i
                       : (int) Time.time))
            {
                var startRadiusRatio = GetRandom(multipleSpawn.startRadiusRatioScreen);
                var startRadius = Screen.height * startRadiusRatio;
                var radiusSpeed = GetRandom(multipleSpawn.radiusChangeInSecond) * startRadius;
                var pos = new Vector2(Random.Range(0f, Screen.width), Random.Range(0f, -Screen.height));
                if (pos.x > Screen.width - startRadius)
                    pos.x -= startRadius;
                else if (pos.x < 0 + startRadius)
                    pos.x += startRadius;
                if (pos.y < -Screen.height + startRadius)
                    pos.y += startRadius;
                else if (pos.y > 0 - startRadius)
                    pos.y -= startRadius;
                Debug.Log($"Spawn position: {pos}");
                var spawnAble = new Spawnable
                {
                    lifeTime = GetRandom(multipleSpawn.lifeTime),
                    startRadius = startRadius,
                    radiusChangeSpeed = radiusSpeed,
                    spawnableType = multipleSpawn.spawnableType
                };

                Spawn(spawnAble, pos);
                i++;
                await UniTask.Delay(TimeSpan.FromSeconds(GetRandom(multipleSpawn.delayBetweenSpawn)));
            }
        }).Invoke();
    }

    public void Spawn(Spawnable spawnable, Vector2 pos)
    {
        var circle = Instantiate(targetCircle, targetCircles).GetComponent<RectTransform>();
        var target = new Target
        {
            RectTransform = circle,
            Spawnable = spawnable,
            Position = pos
        };
        var startTime = Time.time;
        pos.x -= Screen.width / 2f;
        pos.y += Screen.height / 2f;
        circle.anchoredPosition = pos;
        Debug.Log($"Spawn at: {pos}");

        DataBase.Targets.Add(target);
        UniTask.Action(async () =>
        {
            circle.localScale = spawnable.startRadius / 100f * Vector2.one;
            var rotationSpeed = Random.Range(10f, 40f);
            while (Time.time - startTime < spawnable.lifeTime)
            {
                if (circle == null || target.isShot)
                    break;
                var newScale = circle.localScale -
                               (Vector3.one * spawnable.radiusChangeSpeed / 100f * Time.deltaTime).ChangeVector(z: 0);
                circle.localScale = newScale;
                var newRotation = circle.rotation.eulerAngles.z + rotationSpeed * Time.deltaTime;
                circle.rotation = Quaternion.Euler(0f, 0f, newRotation);
                await UniTask.Yield();
            }

            if (circle != null && !target.isShot)
            {
                Destroy(circle.gameObject);
                DataBase.Targets.Remove(target);
            }
        }).Invoke();
    }

    public float GetRandom(Vector2 vect) => Random.Range(vect.x, vect.y);

    private void StartGame(GameData gameData)
    {
        MultipleSpawn(multiSpawn.First(e => e.DifficultyMode == gameData.DifficultyMode).MultipleSpawn);
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


[Serializable]
public class MultiSpawnWithDifficulty
{
    public DifficultyMode DifficultyMode;
    public MultipleSpawn MultipleSpawn;
}

[Serializable]
public class MultipleSpawn
{
    public SpawnFinishMode spawnFinishMode;
    [HideIf(nameof(IsTimedSpawn))] public int numOfSpawns;
    [ShowIf(nameof(IsTimedSpawn))] public float seconds;
    public SpawnableType spawnableType;
    [MinMaxSlider(0, 0.5f)] public Vector2 startRadiusRatioScreen;
    [MinMaxSlider(0, 1)] public Vector2 radiusChangeInSecond;
    [MinMaxSlider(0, 10)] public Vector2 lifeTime;
    [MinMaxSlider(0, 5)] public Vector2 delayBetweenSpawn;

    private bool IsTimedSpawn() => spawnFinishMode == SpawnFinishMode.Time;
}

public enum SpawnFinishMode
{
    NumOfSpawn,
    Time
}

[Serializable]
public class Spawnable
{
    public SpawnableType spawnableType;
    public float startRadius;
    public float radiusChangeSpeed;
    public float lifeTime;
}

[Serializable]
public class Target
{
    public Spawnable Spawnable;
    public RectTransform RectTransform;
    public Vector2 Position;
    public bool isShot;
}

public enum SpawnableType
{
    CircleTarget,
    DoublePoint
}