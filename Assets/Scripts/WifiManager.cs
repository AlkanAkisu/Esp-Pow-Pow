using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Cysharp.Threading.Tasks;
using Unity.Collections;

public class WifiManager : MonoBehaviour
{
    [SerializeField] private string ipAddress = "192.168.20.199";

    [SerializeField] private bool stopDataFlow;
    [SerializeField] private EspData espData;


    private void Start()
    {
        var uri = "http://" + ipAddress;
        UniTask.Action(async () =>
        {
            while (!stopDataFlow)
            {
                using var webRequest = UnityWebRequest.Get(uri);
                await webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(webRequest.error);
                }
                else
                {
                    var data = webRequest.downloadHandler.text;
                    ProcessData(data.Split("\n")[1]);
                }
            }
        }).Invoke();
    }
    
    void ProcessData(string data)
    {
        var splitData = data.Split(',');
        if (splitData.Length == 3)
        {
            espData = new EspData()
            {
                angleZ = float.Parse(splitData[0]),
                angleY = float.Parse(splitData[1]),
                buttonState = int.Parse(splitData[2]) > 0
            };
            EventSystem.OnEspData?.Invoke(espData);
        }
    }

    private void OnDisable()
    {
        stopDataFlow = true;
    }
}

[Serializable]
public struct EspData
{
    public float angleZ;
    public float angleY;
    public bool buttonState;
}