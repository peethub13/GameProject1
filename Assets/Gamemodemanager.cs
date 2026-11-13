using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    public string selectedMode = "";

    [Header("Control Mode")]
    public bool useHardware = false;

    private string serverURL = "http://172.20.10.2:5000/mode";

    private void Awake()
    {
        Instance = this;
    }

    public void SelectPOT()
    {
        selectedMode = "POT";
        StartSelectedMode();
    }

    public void SelectLDR()
    {
        selectedMode = "LDR";
        StartSelectedMode();
    }

    private void StartSelectedMode()
    {
        Debug.Log("Selected Mode: " + selectedMode);

        if (useHardware)
        {
            StartCoroutine(SendModeToPi());
        }
        else
        {
            Debug.Log("Keyboard Mode: Starting without Raspberry Pi");

            if (UIManager.Instance != null)
            {
                UIManager.Instance.StartGame();
            }
        }
    }

    IEnumerator SendModeToPi()
    {
        string json = "{\"mode\":\"" + selectedMode + "\"}";
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(serverURL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("Sending mode to Raspberry Pi: " + json);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Raspberry Pi Response: " + request.downloadHandler.text);

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.StartGame();
                }
            }
            else
            {
                Debug.LogError("Failed to send mode: " + request.error);
            }
        }
    }
}