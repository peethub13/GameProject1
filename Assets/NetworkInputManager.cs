using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class NetworkInputManager : MonoBehaviour
{
    public static NetworkInputManager Instance;

    [Header("--- Server Config ---")]
    public string serverIP = "192.168.50.2";
    public int serverPort = 5000;
    public float pollInterval = 0.05f;

    [Header("--- Output Values ---")]
    public string currentMode = "POT";
    public string currentDirection = "CENTER";
    public float horizontalInput = 0f;
    public bool isGameActive = false;

    public event Action OnMenuLeftPressed;
    public event Action OnMenuRightPressed;
    public event Action OnMenuSelectPressed;

    private string getInputUrl;
    private string gameOverUrl;

    private bool hasReceivedServerData = false;
    private bool pendingStartGame = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("[Network] NetworkInputManager Started!");

        getInputUrl = $"http://{serverIP}:{serverPort}/unity_input";
        gameOverUrl = $"http://{serverIP}:{serverPort}/game/finish";

        Debug.Log("[Network] Raspberry Pi URL: " + getInputUrl);

        StartCoroutine(FetchInputLoop());
    }

    private void Update()
    {
        // รอ UIManager พร้อมแล้วจึงเริ่มเกม
        if (pendingStartGame)
        {
            TryStartGame();
        }
    }

    private IEnumerator FetchInputLoop()
    {
        while (true)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(getInputUrl))
            {
                www.timeout = 5;

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    ParseServerData(www.downloadHandler.text);
                }
                else
                {
                    Debug.LogError(
                        "[Network] Raspberry Pi Connection Failed: "
                        + www.error
                    );
                }
            }

            yield return new WaitForSecondsRealtime(pollInterval);
        }
    }

    private void ParseServerData(string jsonText)
    {
        try
        {
            ServerInputData data =
                JsonUtility.FromJson<ServerInputData>(jsonText);

            if (data == null)
            {
                Debug.LogWarning("[Network] JSON Data is NULL!");
                return;
            }

            bool wasGameActive = isGameActive;

            currentMode = data.mode;
            currentDirection = data.direction;
            isGameActive = data.game_active;

            // แสดงข้อมูลครั้งแรกที่ได้รับ
            if (!hasReceivedServerData)
            {
                Debug.Log(
                    "[Network] First Server Data: " + jsonText
                );

                hasReceivedServerData = true;
            }

            // แสดง Log เมื่อสถานะเกมเปลี่ยน
            if (wasGameActive != isGameActive)
            {
                Debug.Log(
                    $"[Network] Game State Changed: " +
                    $"{wasGameActive} -> {isGameActive}, " +
                    $"Mode: {currentMode}"
                );
            }

            // รับค่าทิศทางจาก POT หรือ LDR
            if (currentDirection == "LEFT")
            {
                horizontalInput = -1f;
            }
            else if (currentDirection == "RIGHT")
            {
                horizontalInput = 1f;
            }
            else
            {
                horizontalInput = 0f;
            }

            // ตรวจจับคำสั่งเริ่มเกม
            if (!wasGameActive && isGameActive)
            {
                Debug.Log(
                    "[Network] Raspberry Pi START GAME!"
                );

                pendingStartGame = true;
            }

            // ลองเริ่มเกมเมื่อมีคำสั่งรออยู่
            if (pendingStartGame)
            {
                TryStartGame();
            }

            // รับปุ่มเมนูเมื่อเกมยังไม่เริ่ม
            if (!isGameActive)
            {
                if (data.button == "LEFT")
                {
                    OnMenuLeftPressed?.Invoke();
                }
                else if (data.button == "RIGHT")
                {
                    OnMenuRightPressed?.Invoke();
                }
                else if (data.button == "OK")
                {
                    OnMenuSelectPressed?.Invoke();
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(
                "[Network] JSON Parsing Error: " + e.Message
            );
        }
    }

    private void TryStartGame()
    {
        if (UIManager.Instance == null)
        {
            return;
        }

        Debug.Log("[Network] UIManager Found!");

        // ป้องกันการเรียกซ้ำ
        pendingStartGame = false;

        UIManager.Instance.StartGame();

        Debug.Log(
            "[Network] StartGame() Called Successfully!"
        );
    }

    public void NotifyGameOver()
    {
        StartCoroutine(SendGameOverSignal());
    }

    private IEnumerator SendGameOverSignal()
    {
        using (UnityWebRequest www =
            UnityWebRequest.PostWwwForm(gameOverUrl, ""))
        {
            www.timeout = 5;

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(
                    "[Network] Game Over Signal Sent!"
                );

                isGameActive = false;
                pendingStartGame = false;
            }
            else
            {
                Debug.LogError(
                    "[Network] Game Over Failed: " + www.error
                );
            }
        }
    }
}

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