using UnityEngine;

public enum EnvironmentTime { Day, Sunset, Night }

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance;

    [Header("1. สภาพแวดล้อมปัจจุบัน")]
    public EnvironmentTime currentTime = EnvironmentTime.Day;

    [Header("2. ก้อน Parent ของฉากแต่ละช่วงเวลา")]
    public GameObject bgDay;
    public GameObject bgSunset;
    public GameObject bgNight;

    [Header("3. ระยะเวลาเปลี่ยนฉาก (วินาที)")]
    public float timePerPhase = 30f; // เช่น ฉากละ 30 วินาที

    private float gameTimer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ResetEnvironment();
    }

    void Update()
    {
        // ถ้ายังไม่เริ่มเกม หรือ Game Over แล้ว ให้หยุดจับเวลา
        if (GameManager.Instance != null && (!GameManager.Instance.isGameStarted || GameManager.Instance.isGameOver)) 
            return;

        gameTimer += Time.deltaTime;

        // เปลี่ยนฉากตามเวลาที่ผ่านไป
        if (currentTime == EnvironmentTime.Day && gameTimer >= timePerPhase)
        {
            SetEnvironment(EnvironmentTime.Sunset);
        }
        else if (currentTime == EnvironmentTime.Sunset && gameTimer >= timePerPhase * 2f)
        {
            SetEnvironment(EnvironmentTime.Night);
        }
    }

    // ฟังก์ชันสำคัญ: สั่งเปิดเฉพาะฉากปัจจุบัน และปิด (false) ฉากที่ไม่ได้ใช้ทั้งหมด
    public void SetEnvironment(EnvironmentTime newTime)
    {
        currentTime = newTime;

        if (bgDay != null) bgDay.SetActive(currentTime == EnvironmentTime.Day);
        if (bgSunset != null) bgSunset.SetActive(currentTime == EnvironmentTime.Sunset);
        if (bgNight != null) bgNight.SetActive(currentTime == EnvironmentTime.Night);

        Debug.Log("🌅 เปลี่ยนฉากเป็น: " + currentTime);
    }

    // รีเซ็ตเวลากลับเป็นกลางวันตอนเริ่มเกมใหม่
    public void ResetEnvironment()
    {
        gameTimer = 0f;
        SetEnvironment(EnvironmentTime.Day);
    }
}