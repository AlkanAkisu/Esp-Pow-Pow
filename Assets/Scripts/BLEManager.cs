using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BLEManager : MonoBehaviour
{
    public string DeviceName = "ESP32test";
    public string ServiceUUID = "2220";
    public string SubscribeCharacteristic = "2221";
    public string WriteCharacteristic = "2222";

    enum States
    {
        None,
        Scan,
        ScanRSSI,
        Connect,
        Subscribe,
        Unsubscribe,
        Disconnect,
    }

    private bool _connected = false;
    private float _timeout = 0f;
    private States _state = States.None;
    private string _deviceAddress;
    private bool _foundSubscribeID = false;
    private bool _foundWriteID = false;


    void Reset()
    {
        _connected = false;
        _timeout = 0f;
        _state = States.None;
        _deviceAddress = null;
        _foundSubscribeID = false;
        _foundWriteID = false;
    }

    void SetState(States newState, float timeout)
    {
        _state = newState;
        _timeout = timeout;
    }

    void StartProcess()
    {
        Reset();
        BluetoothLEHardwareInterface.Initialize(true, false, () =>
        {
            Debug.Log("Initialize: ");
            SetState(States.Scan, 0.1f);
        }, (error) => { Debug.Log("Error during initialize: " + error); });
    }

    // Use this for initialization
    void Start()
    {
        Debug.Log($"Start");
        StartProcess();
    }

    void EvaluateBLEStates()
    {
        switch (_state)
        {
            case States.None:
                break;

            case States.Scan:
                BluetoothLEHardwareInterface.ScanForPeripheralsWithServices(null, (address, name) =>
                {
                    Debug.Log ($"Scaning({name}): ");
                    if (!name.Contains(DeviceName)) return;
                    BluetoothLEHardwareInterface.StopScan();
                    Debug.Log ($"Found {name}");
                    _deviceAddress = address;
                    SetState(States.Connect, 0.5f);
                }, (address, name, rssi, bytes) =>
                {
                    // use this one if the device responses with manufacturer specific data and the rssi

                    if (name.Contains(DeviceName))
                    {
                        BluetoothLEHardwareInterface.StopScan();

                        // found a device with the name we want
                        // this example does not deal with finding more than one
                        _deviceAddress = address;
                        SetState(States.Connect, 0.5f);
                    }
                });

                break;

            case States.ScanRSSI:
                break;

            case States.Connect:
                // set these flags
                _foundSubscribeID = false;
                _foundWriteID = false;

                // note that the first parameter is the address, not the name. I have not fixed this because
                // of backwards compatiblity.
                // also note that I am note using the first 2 callbacks. If you are not looking for specific characteristics you can use one of
                // the first 2, but keep in mind that the device will enumerate everything and so you will want to have a timeout
                // large enough that it will be finished enumerating before you try to subscribe or do any other operations.
                BluetoothLEHardwareInterface.ConnectToPeripheral(_deviceAddress, null, null,
                    (address, serviceUUID, characteristicUUID) =>
                    {
                        Debug.Log($"Trying to connect");
                        if (!IsEqual(serviceUUID, ServiceUUID)) 
                            return;
                        _foundSubscribeID =
                            _foundSubscribeID || IsEqual(characteristicUUID, SubscribeCharacteristic);
                        _foundWriteID = _foundWriteID || IsEqual(characteristicUUID, WriteCharacteristic);

                        if (_foundSubscribeID) // && _foundWriteID)
                        {
                            _connected = true;
                            SetState(States.Subscribe, 2f);
                        }
                    });
                break;

            case States.Subscribe:
                BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress(_deviceAddress, ServiceUUID,
                    SubscribeCharacteristic, null, (address, characteristicUUID, bytes) =>
                    {
                        _state = States.None;

                        // we received some data from the device
                        var data = System.BitConverter.ToString(bytes);
                        Debug.Log($"{data}");
                    });
                break;

            case States.Unsubscribe:
                BluetoothLEHardwareInterface.UnSubscribeCharacteristic(_deviceAddress, ServiceUUID,
                    SubscribeCharacteristic, null);
                SetState(States.Disconnect, 4f);
                break;

            case States.Disconnect:
                if (_connected)
                {
                    BluetoothLEHardwareInterface.DisconnectPeripheral(_deviceAddress, (address) =>
                    {
                        BluetoothLEHardwareInterface.DeInitialize(() =>
                        {
                            _connected = false;
                            _state = States.None;
                        });
                    });
                }
                else
                {
                    BluetoothLEHardwareInterface.DeInitialize(() => { _state = States.None; });
                }

                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_timeout > 0f)
        {
            _timeout -= Time.deltaTime;
            if (_timeout <= 0f)
            {
                _timeout = 0f;

                EvaluateBLEStates();
            }
        }
    }

    private bool ledON = false;

    bool IsEqual(string uuid1, string uuid2)
    {
        if (uuid1.Length == 4)
            uuid1 = FullUUID(uuid1);
        if (uuid2.Length == 4)
            uuid2 = FullUUID(uuid2);

        return (uuid1.ToUpper().CompareTo(uuid2.ToUpper()) == 0);
    }

    string FullUUID(string uuid)
    {
        return "0000" + uuid + "-0000-1000-8000-00805f9b34fb";
    }
}