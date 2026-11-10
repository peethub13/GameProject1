using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class NetworkInputManager : MonoBehaviour
{
    public static NetworkInputManager Instance;

    [Header("--- 🌐 Server Config ---")]
    public string serverIP = "172.20.10.2";
    public int serverPort = 5000;
    public float pollInterval = 0.05f; // ดึงข้อมูลทุก 0.05 วินาที (20Hz)

    [Header("--- 📊 Output Values (Read Only) ---")]
    public string currentMode = "POT";
    public string currentDirection = "CENTER";
    public float horizontalInput = 0f; // -1 (ซ้าย), 0 (ตรงกลาง), 1 (ขวา)
    public bool isGameActive = false;

    // Events สำหรับสั่งงาน UI Menu แบบจังหวะกด
    public event Action OnMenuLeftPressed;
    public event Action OnMenuRightPressed;
    public event Action OnMenuSelectPressed;

    //private string lastDirection = "CENTER";
    private string getInputUrl;
    private string gameOverUrl;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ให้คงอยู่ข้าม Scene ได้
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
{
    Debug.Log("TEST START WORKING");

    getInputUrl = $"http://{serverIP}:{serverPort}/unity_input";
    gameOverUrl = $"http://{serverIP}:{serverPort}/game/finish";

    Debug.Log("กำลังเชื่อมต่อ Raspberry Pi: " + getInputUrl);
    StartCoroutine(FetchInputLoop());
}

    // วนลูปดึงข้อมูลจาก Raspberry Pi Server
    private IEnumerator FetchInputLoop()
    {
        while (true)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(getInputUrl))
            {
                www.timeout = 2;
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
{
    Debug.Log("ได้รับข้อมูลจาก Raspberry Pi: " + www.downloadHandler.text);
    ParseServerData(www.downloadHandler.text);
}
else
{
    Debug.LogError("เชื่อมต่อ Raspberry Pi ไม่สำเร็จ: " 
        + www.error + " | URL: " + getInputUrl);
}
            }
            yield return new WaitForSeconds(pollInterval);
        }
    }

    private void ParseServerData(string jsonText)
{   
    Debug.Log("ได้รับข้อมูลจาก Raspberry Pi: " + jsonText);
    try
    {
        ServerInputData data =
            JsonUtility.FromJson<ServerInputData>(jsonText);

        currentMode = data.mode;
        currentDirection = data.direction;
        isGameActive = data.game_active;

        // รับค่าทิศทางจาก POT หรือ LDR
        if (currentDirection == "LEFT")
            horizontalInput = -1f;
        else if (currentDirection == "RIGHT")
            horizontalInput = 1f;
        else
            horizontalInput = 0f;

        // รับปุ่มเมนูแยกจากทิศทางเซนเซอร์
        if (!isGameActive)
        {
            if (data.button == "LEFT")
                OnMenuLeftPressed?.Invoke();

            else if (data.button == "RIGHT")
                OnMenuRightPressed?.Invoke();

            else if (data.button == "OK")
                OnMenuSelectPressed?.Invoke();
        }
    }
    catch (Exception e)
    {
        Debug.LogWarning("Error parsing server data: " + e.Message);
    }
}

    // สั่งแจ้ง Server ว่าจบเกม เพื่อให้ ESP32 ตัดกลับหน้า Menu
    public void NotifyGameOver()
    {
        StartCoroutine(SendGameOverSignal());
    }

    private IEnumerator SendGameOverSignal()
    {
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(gameOverUrl, ""))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("🏁 แจ้ง Game Over ไปยัง ESP32 เรียบร้อยแล้ว");
                isGameActive = false;
            }
        }
    }
}

// โครงสร้างข้อมูล JSON ที่รับมาจาก Raspberry Pi
[System.Serializable]
public class ServerInputData
{
    public bool game_active;
    public string mode;
    public string direction;
    public string button;
    public int leftValue;
    public int rightValue;
}



