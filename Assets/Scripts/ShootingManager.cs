using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ShootingManager : MonoBehaviour
{
    [SerializeField] private float shootingCooldown;
    private float _lastShootingTime = -1f;
    [SerializeField] private GameObject spawnOnShoot;
    [SerializeField] RectTransform bulletHolesParent;
    private bool _isShootingEnabled;
    [SerializeField] private Color shotColor;
    private bool _wasPressed;


    private void OnCrossHair(CrossHairData crossHair)
    {
        if (!_isShootingEnabled)
            return;
        if (!crossHair.isButtonPressed && !_wasPressed)
            return;
        Shoot(crossHair);
    }

    private void Shoot(CrossHairData crossHair)
    {
        _wasPressed = true;
        if (_lastShootingTime + shootingCooldown > Time.time)
            return;
        CheckForTarget(crossHair);
        MakeBulletHole(crossHair);
        _lastShootingTime = Time.time;
        _wasPressed = false;
    }

    private void CheckForTarget(CrossHairData crossHairData)
    {
        var n = DataBase.Targets.Count;
        var targets = DataBase.Targets;
        var shotTargets = new List<Target>();
        var crossHairPos = crossHairData.position;
        for (var i = n - 1; i >= 0; i--)
        {
            var target = targets[i];
            if ((target.Position - crossHairData.position).magnitude < target.RectTransform.localScale.x * 100f)
            {
                TargetShot(target);
                shotTargets.Add(target);
            }
        }

        if (shotTargets.Count == 0)
            return;
        EventSystem.OnShot.Invoke(new ShotData
        {
            shotPosition = crossHairPos,
            shotTargets = shotTargets
        });
    }

    private void TargetShot(Target target)
    {
        if (target.RectTransform == null)
            return;
        DataBase.Targets.Remove(target);
        UniTask.Action(async () =>
        {
            target.isShot = true;
            var image = target.RectTransform.GetComponent<Image>();
            await image.DOColor(shotColor, 0.3f).AsyncWaitForCompletion();
            await image.DOFade(0.3f, 0.3f).AsyncWaitForCompletion();
            Destroy(target.RectTransform.gameObject);
        }).Invoke();
    }

    private void MakeBulletHole(CrossHairData crossHair)
    {
        var bulletHole = Instantiate(spawnOnShoot, bulletHolesParent).GetComponent<RectTransform>();
        bulletHole.anchoredPosition = crossHair.position;
        bulletHole.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        UniTask.Action(async () =>
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            if (bulletHole != null)
            {
                await bulletHole.GetComponent<Image>().DOFade(0.3f, 0.5f).AsyncWaitForCompletion();
                Destroy(bulletHole.gameObject);
            }
        }).Invoke();
    }

    private void OnEnable()
    {
        EventSystem.OnCrossHair += OnCrossHair;
        EventSystem.ToggleShooting += ToggleShooting;
    }

    private void OnDisable()
    {
        EventSystem.OnCrossHair -= OnCrossHair;
        EventSystem.ToggleShooting -= ToggleShooting;
    }

    private void ToggleShooting(bool isEnable)
    {
        _isShootingEnabled = isEnable;
    }
}

[Serializable]
public class ShotData
{
    public List<Target> shotTargets;
    public Vector2 shotPosition;
}