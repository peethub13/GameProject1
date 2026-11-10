using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement; // ใช้สำหรับการจัดการ Scene

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
    public GameObject gameOverPanel;  // ช่องสำหรับลาก GameOverPanel ใน Hierarchy มาใส่
    public GameObject panelMainGuest; // ตัวแปรอ้างอิง Panel Main Guest
    public GameObject panelMainUser;  // ตัวแปรอ้างอิง Panel Main User
    public GameObject panelModeSelect; // ตัวแปรอ้างอิง Panel Mode Select

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

    // ฟังก์ชันสำหรับบวก/หัก คะแนน
    public void AddScore(int value)
    {
        score += value;
        if (score < 0) score = 0; // ป้องกันคะแนนติดลบ
    }

    // ฟังก์ชันสำหรับเรียกใช้งานเมื่อกดปุ่ม Start/Play จาก UIManager
    public void StartGame()
    {
        Time.timeScale = 1f; // มั่นใจว่าเวลากลับมาเดินปกติ
        score = 0;
        combo = 0;
        level = 1;
        survivalTime = 0f;
        isGameOver = false;
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false); // ซ่อน UI Game Over
        if (panelModeSelect != null) panelModeSelect.SetActive(false); // ซ่อนหน้าเลือกโหมดเมื่อเริ่มเล่น
        
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

        // ส่งคะแนนไปบันทึกลง Firebase ผ่าน NetworkManager
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.SubmitScore(score);
        }

        // แจ้ง ESP32 ให้เปลี่ยนสถานะจอยสติ๊กกลับสู่โหมด UI
        NotifyGameOver();

        // 1. สั่งเปิด UI Game Over Panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // 2. หยุดเวลาในเกม (ฟิสิกส์และการเคลื่อนที่ทั้งหมดจะถูกแช่แข็ง)
        Time.timeScale = 0f;
    }

    // 🟢 3. ฟังก์ชันสำหรับผูกกับปุ่ม Restart / Play Again (กลับไปหน้าเลือกโหมด)
    public void RestartGame()
    {
        Time.timeScale = 1f; // คืนค่าความเร็วเกมเป็นปกติ

        // 1. ปิดหน้า Game Over
        if (gameOverPanel != null) 
            gameOverPanel.SetActive(false);

        // 2. ปิดหน้า Main Menu ต่างๆ (กันขึ้นซ้อน)
        if (panelMainGuest != null) panelMainGuest.SetActive(false);
        if (panelMainUser != null) panelMainUser.SetActive(false);

        // 3. บังคับเปิดหน้าเลือกโหมด (Panel_ModeSelect)
        if (panelModeSelect != null) 
        {
            panelModeSelect.SetActive(true);
        }

        // 4. สั่งแจ้ง ESP32 ให้กลับมาสภาวะใช้ปุ่มกดเพื่อเลือกโหมดอีกครั้ง
        NotifyGameOver();
    }

    // 🟢 ฟังก์ชันส่งสัญญาณแจ้ง ESP32/Server เมื่อจบเกมหรือต้องการคืนค่าเมนู
    public void NotifyGameOver()
    {
        if (NetworkInputManager.Instance != null)
        {
            NetworkInputManager.Instance.NotifyGameOver();
        }
    }

    // 🟢 4. ฟังก์ชันสำหรับปุ่ม Back / Go to Main Menu (เช็กสถานะ Login)
    // 🟢 บังคับเปิด Panel_MainUser เสมอเมื่อกด Home / GoToMainMenu
public void GoToMainMenu()
{
    Time.timeScale = 1f; // คืนเวลาปกติ

    // 1. ปิดหน้า UI ที่ไม่เกี่ยวข้อง
    if (gameOverPanel != null) gameOverPanel.SetActive(false);
    if (panelModeSelect != null) panelModeSelect.SetActive(false);
    if (panelMainGuest != null) panelMainGuest.SetActive(false); // ปิดหน้า Guest

    // 2. บังคับเปิดหน้า Main User ทันที
    if (panelMainUser != null) 
    {
        panelMainUser.SetActive(true);
    }

    // 3. แจ้ง ESP32 ให้เปลี่ยนสถานะจอยกลับสู่โหมด UI
    NotifyGameOver();
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