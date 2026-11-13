using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance;

    public string selectedMode = "";

    private string serverURL = "http://172.20.10.2:5000/mode";

    private void Awake()
    {
        Instance = this;
    }

    public void SelectPOT()
    {
        selectedMode = "POT";
        Debug.Log("Selected Mode: " + selectedMode);

        StartCoroutine(SendModeToPi());
    }

    public void SelectLDR()
    {
        selectedMode = "LDR";
        Debug.Log("Selected Mode: " + selectedMode);

        StartCoroutine(SendModeToPi());
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

                UIManager.Instance.StartGame();
            }
            else
            {
                Debug.LogError("Failed to send mode: " + request.error);
            }
        }
    }
}