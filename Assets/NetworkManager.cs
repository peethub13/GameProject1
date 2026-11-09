using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// 📦 โมเดลข้อมูลสำหรับส่ง JSON ไปเซิร์ฟเวอร์
[System.Serializable]
public class AuthData
{
    public string username;
    public string password;
}

[System.Serializable]
public class ScoreData
{
    public string username;
    public int score;
}

// 📦 โมเดลสำหรับรับข้อมูล Response (ที่มี Token) กลับมาจากเซิร์ฟเวอร์
[System.Serializable]
public class AuthResponse
{
    public string message;
    public string token;
    public string access_token;
    public string idToken;
}

// 🏆 โมเดลสำหรับรับข้อมูลตารางคะแนน (Scoreboard)
[System.Serializable]
public class ScoreboardPlayer
{
    public int rank;
    public string uid;
    public string username;
    public int bestScore;
    public string bestScoreAchievedAt;
}

[System.Serializable]
public class ScoreboardResponse
{
    public List<ScoreboardPlayer> players;
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    [Header("Render Server URL")]
    [Tooltip("ใส่ URL ของ Render เช่น https://your-app.onrender.com")]
    public string baseUrl = "https://your-app-name.onrender.com";

    [Header("User Session")]
    public string currentUsername = "";
    public string authToken = ""; // 🔑 ตัวแปรเก็บ Token ยืนยันตัวตน

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ให้ทำงานข้าม Scene ได้
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ----------------------------------------------------
    // 🔑 1. ระบบ เข้าสู่ระบบ (Login)
    // ----------------------------------------------------
    public void Login(string username, string password)
    {
        StartCoroutine(LoginRoutine(username, password));
    }

    private IEnumerator LoginRoutine(string username, string password)
    {
        string url = baseUrl + "/login";
        AuthData data = new AuthData { username = username, password = password };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest www = CreatePostRequest(url, json))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                currentUsername = username;
                string jsonResponse = www.downloadHandler.text;
                
                // 🔑 ดึง Token จาก response เก็บไว้ใช้ยืนยันตัวตนส่งคะแนน
                try
                {
                    AuthResponse res = JsonUtility.FromJson<AuthResponse>(jsonResponse);
                    if (!string.IsNullOrEmpty(res.token)) authToken = res.token;
                    else if (!string.IsNullOrEmpty(res.access_token)) authToken = res.access_token;
                    else if (!string.IsNullOrEmpty(res.idToken)) authToken = res.idToken;
                }
                catch
                {
                    Debug.LogWarning("⚠️ ไม่พบ Token ในรูปแบบ JSON มาตรฐาน");
                }

                Debug.Log($"✅ เข้าสู่ระบบสำเร็จ! ผู้ใช้: {currentUsername} | Token: {authToken}");
            }
            else
            {
                Debug.LogError("❌ เข้าสู่ระบบไม่สำเร็จ: " + www.error + " | " + www.downloadHandler.text);
            }
        }
    }

    // ----------------------------------------------------
    // 📝 2. ระบบ สมัครสมาชิก (Register)
    // ----------------------------------------------------
    public void Register(string username, string password)
    {
        StartCoroutine(RegisterRoutine(username, password));
    }

    private IEnumerator RegisterRoutine(string username, string password)
    {
        string url = baseUrl + "/register";
        AuthData data = new AuthData { username = username, password = password };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest www = CreatePostRequest(url, json))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ สมัครสมาชิกสำเร็จ! กรุณาเข้าสู่ระบบ");
            }
            else
            {
                Debug.LogError("❌ สมัครสมาชิกไม่สำเร็จ: " + www.error + " | " + www.downloadHandler.text);
            }
        }
    }

    // ----------------------------------------------------
    // 🏆 3. ระบบ บันทึกคะแนน (Submit Score)
    // ----------------------------------------------------
    public void SubmitScore(int score)
    {
        if (string.IsNullOrEmpty(currentUsername))
        {
            Debug.LogWarning("⚠️ ยังไม่ได้ล็อกอิน ไม่สามารถบันทึกคะแนนได้");
            return;
        }

        StartCoroutine(SubmitScoreRoutine(score));
    }

    private IEnumerator SubmitScoreRoutine(int score)
    {
        string url = baseUrl + "/score";
        ScoreData data = new ScoreData { username = currentUsername, score = score };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest www = CreatePostRequest(url, json))
        {
            // 🔑 แนบ Header ส่ง Token ให้ครอบคลุมทุกรูปแบบที่ Backend อาจเรียกใช้
            if (!string.IsNullOrEmpty(authToken))
            {
                www.SetRequestHeader("Authorization", "Bearer " + authToken); // Standard Bearer
                www.SetRequestHeader("x-access-token", authToken);            // Flask Standard
                www.SetRequestHeader("token", authToken);                     // Custom Header
            }

            Debug.Log($"⏳ กำลังบันทึกคะแนน {score} ไปที่เซิร์ฟเวอร์...");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("🏆 บันทึกคะแนนลง Firebase สำเร็จ: " + score);
            }
            else
            {
                Debug.LogError("❌ บันทึกคะแนนไม่สำเร็จ: " + www.error + " | " + www.downloadHandler.text);
            }
        }
    }

    // ----------------------------------------------------
    // 📊 4. ระบบ ดึงตารางคะแนน (Get Scoreboard)
    // ----------------------------------------------------
    public void GetScoreboard(System.Action<List<ScoreboardPlayer>> onSuccess = null)
    {
        StartCoroutine(GetScoreboardRoutine(onSuccess));
    }

    private IEnumerator GetScoreboardRoutine(System.Action<List<ScoreboardPlayer>> onSuccess)
    {
        string url = baseUrl + "/scoreboard";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            if (!string.IsNullOrEmpty(authToken))
            {
                www.SetRequestHeader("Authorization", "Bearer " + authToken);
                www.SetRequestHeader("x-access-token", authToken);
                www.SetRequestHeader("token", authToken);
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResult = www.downloadHandler.text;
                Debug.Log("📊 ดึงตารางคะแนนสำเร็จ: " + jsonResult);

                try
                {
                    ScoreboardResponse res = JsonUtility.FromJson<ScoreboardResponse>(jsonResult);
                    if (res != null && res.players != null)
                    {
                        onSuccess?.Invoke(res.players);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("❌ แปลงข้อมูล Scoreboard JSON ไม่สำเร็จ: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("❌ ดึงตารางคะแนนไม่สำเร็จ: " + www.error);
            }
        }
    }

    // 🛠️ Helper สรุปการสร้าง POST Request + JSON
    private UnityWebRequest CreatePostRequest(string url, string json)
    {
        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        return www;
    }
}