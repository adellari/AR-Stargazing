using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine;
using AOT;
using Newtonsoft.Json;

/// <summary>
/// C-API exposed by the Host, i.e., Unity -> Host API.
/// </summary>
public class HostNativeAPI {
    public delegate void TestDelegate(IntPtr data, int length);

    [DllImport("__Internal")]
    public static extern void sendUnityStateUpdate(string state);

    [DllImport("__Internal")]
    public static extern void setTestDelegate(TestDelegate cb);
}

/// <summary>
/// C-API exposed by Unity, i.e., Host -> Unity API.
/// </summary>
public class UnityNativeAPI {

    [MonoPInvokeCallback(typeof(HostNativeAPI.TestDelegate))]
    //passing raw bytes array for ble communication readback
    public static void Test(IntPtr dataPtr, int length)
    {
        byte[] data = new byte[length];
        Marshal.Copy(dataPtr, data, 0, length);
        
        if (length == 241)
            getPositionsList(data);
        else if (length == 321)
            getRotationsList(data);
        //Debug.Log(data.Length);
    }

    public static void getPositionsList(byte[] data)
    {
        //float[] positions = new float[20];
        int idByte = data[0];

        List<float> floats = new List<float>();
        for (int i = 1; i < data.Length; i += 4)
        {
            byte[] fourBytes = new byte[4];
            Array.Copy(data, i, fourBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(fourBytes);
            }
            float f = BitConverter.ToSingle(fourBytes, 0);
            floats.Add(f);
        }
        // positions = floats.ToArray();
        // Debug.Log("first float in list: " + floats[0]);
        NativeHandler.Instance.handleParseDataP(floats.ToArray());
    }

    public static void getRotationsList(byte[] data)
    {
        //float[] positions = new float[20];
        int idByte = data[0];

        List<float> floats = new List<float>();
        for (int i = 1; i < data.Length; i += 4)
        {
            byte[] fourBytes = new byte[4];
            Array.Copy(data, i, fourBytes, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(fourBytes);
            }
            float f = BitConverter.ToSingle(fourBytes, 0);
            floats.Add(f);
        }
        // positions = floats.ToArray();
         //Debug.Log("first rotation in list: " + floats[0]);
        NativeHandler.Instance.handleParseDataR(floats.ToArray());
    }

}


/// <summary>
/// This structure holds the type of an incoming message.
/// Based on the type, we will parse the extra provided data.
/// </summary>
public struct Message
{
    public string type;
}

/// <summary>
/// This structure holds the type of an incoming message, as well
/// as some data.
/// </summary>
public struct MessageWithData<T>
{
    [JsonProperty(Required = Newtonsoft.Json.Required.AllowNull)]
    public string type;

    [JsonProperty(Required = Newtonsoft.Json.Required.AllowNull)]
    public T data;
}

public class NativeHandler : MonoBehaviour
{
    public GameObject cube;

    public static NativeHandler Instance { get; private set; } 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        
    }


    void Start()
    {
        //armCustom = armCustom.GetComponent<ArmCustomizer>();
        #if UNITY_IOS
        if (Application.platform == RuntimePlatform.IPhonePlayer) {
            HostNativeAPI.setTestDelegate(UnityNativeAPI.Test);
            HostNativeAPI.sendUnityStateUpdate("ready");
        }
        #endif
        
    }

    public void handleParseDataP(float[] floats)
    {
        List<Vector3> positions = new List<Vector3>(20);

        for(int a=0; a<60; a += 3)
        {
            Vector3 pos = new Vector3(floats[a], floats[a + 1], floats[a + 2]);
            positions.Add(pos);
        }
        //remoteHand.setJointPositions(positions.ToArray());
    }

    public void handleParseDataR(float[] floats)
    {
        List<Vector4> rotations = new List<Vector4>(20);

        for (int a = 0; a < 80; a += 4)
        {
            Vector4 rot = new Vector4(floats[a], floats[a + 1], floats[a + 2], floats[a + 3]);
            rotations.Add(rot);
        }
        //remoteHand.setJointRotations(rotations.ToArray());
    }

    void ReceiveNative(string serializedMessage)
    {
        var header = JsonConvert.DeserializeObject<Message>(serializedMessage);
        switch (header.type) {
            default:
                Debug.LogError("Unrecognized message '" + header.type + "'");
                break;
        }
        //armCustom.receivedMessage(header.type);
    }

    private void _UpdateCubeColor(string serialized)
    {
        var msg = JsonConvert.DeserializeObject<MessageWithData<float[]>>(serialized);
        if (msg.data != null && msg.data.Length >= 3)
        {
            var color = new Color(msg.data[0], msg.data[1], msg.data[2]);
            Debug.Log("Setting Color = " + color);
            var material = cube.GetComponent<MeshRenderer>()?.sharedMaterial;
            material?.SetColor("_Color", color);
        }
    }
}
