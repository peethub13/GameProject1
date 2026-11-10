using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement; // เพิ่มเพื่อใช้ในการ Reload Scene หรือกลับหน้า Menu

[System.Serializable]
public class SaveData
{
    public string username = "Player1";
    public int highScore = 0;
    public float maxSurvivalTime = 0f;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public bool isGameStarted = false; // เช็กสถานะว่ากดเริ่มเกมหรือยัง
    public bool isGameOver = false;

    [Header("Game Stats")]
    public int score = 0;
    public int combo = 0; // นับจำนวน Object ที่ลงรถพยาบาลต่อเนื่อง
    public int level = 1;
    public float survivalTime = 0f;

    [Header("UI Panels")]
    public GameObject gameOverPanel; // 🟢 ช่องสำหรับลาก GameOverPanel ใน Hierarchy มาใส่

    [Header("Save Data")]
    public SaveData currentSave = new SaveData();
    private string savePath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        savePath = Application.persistentDataPath + "/savegame.json";
        LoadGame();
    }

    void Update()
    {
        // ถ้ายังไม่กดเริ่มเกม หรือ Game Over แล้ว จะไม่คำนวณเวลาและเลเวล
        if (!isGameStarted || isGameOver) return;

        survivalTime += Time.deltaTime;
        level = Mathf.FloorToInt(survivalTime / 20f) + 1; 
    }

    // 🟢 ฟังก์ชันสำหรับบวก/หัก คะแนน (ป้องกัน Error CS1061)
    public void AddScore(int value)
    {
        score += value;
        if (score < 0) score = 0; // ป้องกันคะแนนติดลบ
    }

    // เพิ่มฟังก์ชันสำหรับเรียกใช้งานเมื่อกดปุ่ม Start/Play จาก UIManager
    public void StartGame()
    {
        Time.timeScale = 1f; // มั่นใจว่าเวลากลับมาเดินปกติ
        score = 0;
        combo = 0;
        level = 1;
        survivalTime = 0f;
        isGameOver = false;
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false); // ซ่อน UI Game Over
        
        isGameStarted = true; // สั่งเริ่มเกม
    }

    // เมื่อช่วยตัวละครลงรถพยาบาลสำเร็จ
    public void VictimSaved(VictimType type)
    {
        combo++;

        int baseScore = 10; // Human
        if (type == VictimType.Dog) baseScore = 20;
        else if (type == VictimType.Cat) baseScore = 30;

        AddScore(baseScore * combo);
    }

    // เมื่อตัวละครตกพื้น
    public void VictimMissed()
    {
        combo = 0; // รีเซ็ต Combo เป็น 0
        GameOver();
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        isGameStarted = false; // หยุดระบบเมื่อ Game Over

        // บันทึก High Score ลงเครื่อง
        if (score > currentSave.highScore) currentSave.highScore = score;
        if (survivalTime > currentSave.maxSurvivalTime) currentSave.maxSurvivalTime = survivalTime;

        SaveGame();

        // 🌐 🏆 ส่งคะแนนไปบันทึกลง Firebase ผ่าน NetworkManager ( Render Backend )
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.SubmitScore(score);
        }

        // 🌐 🎮 (เพิ่มใหม่) แจ้ง Server และ ESP32 ว่าจบเกม เพื่อตัดเข้าสู่โหมดปุ่มกดเลือกเมนู UI
        if (NetworkInputManager.Instance != null)
        {
            NetworkInputManager.Instance.NotifyGameOver();
        }

        // 🟢 1. สั่งเปิด UI Game Over Panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 🟢 2. หยุดเวลาในเกม (ฟิสิกส์และการเคลื่อนที่ทั้งหมดจะถูกแช่แข็ง)
        Time.timeScale = 0f;
    }

    // 🟢 3. ฟังก์ชันสำหรับผูกกับปุ่ม Restart (เล่นใหม่ทันที)
    public void RestartGame()
    {
        Time.timeScale = 1f; // คืนค่าเวลาปกติ

        // เรียก StartGame() รีเซ็ตค่าคะแนน/เวลา/สถานะ แล้วเริ่มเล่นทันที
        StartGame(); 

        // โหลด Scene ใหม่เพื่อเคลียร์วัตถุเดิมบนหน้าจอ
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 🟢 4. ฟังก์ชันสำหรับผูกกับปุ่ม Main Menu
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene"); // เปลี่ยนชื่อ Scene ตามหน้าเมนูของคุณ
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(currentSave, true);
        File.WriteAllText(savePath, json);
    }

    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            currentSave = JsonUtility.FromJson<SaveData>(json);
        }
    }

    public void ResetCombo()
    {
        combo = 0;
    }
}