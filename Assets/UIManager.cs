using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Panels")]
    public GameObject panelMainGuest;
    public GameObject panelLogin;
    public GameObject panelRegister;
    public GameObject panelMainUser;
    public GameObject panelModeSelect;
    public GameObject panelDeviceConnect;
    public GameObject panelGameOver;
    public GameObject panelIngame; // 🟢 เพิ่ม: Panel_Ingame สำหรับแสดงผลตอนเล่นเกม

    [Header("Ingame Displays")]
    public TMP_Text scoreText;  // 🟢 เพิ่ม: Text แสดง Score
    public TMP_Text comboText;  // 🟢 เพิ่ม: Text แสดง Combo
    public TMP_Text levelText;  // 🟢 เพิ่ม: Text แสดง Level

    [Header("Login Inputs")]
    public TMP_InputField inputLoginUsername;
    public TMP_InputField inputLoginPassword;
    public Toggle toggleRememberMe;

    [Header("Register Inputs")]
    public TMP_InputField inputRegUsername;
    public TMP_InputField inputRegPassword;

    [Header("Device Connect State")]
    public TextMeshProUGUI textDeviceStatus;
    public GameObject buttonStartGame;

    [Header("Game Controllers")]
    [Tooltip("ลาก GameObject ของ Spawner (หรือ SpawnerManager) มาวางตรงนี้")]
    public GameObject spawnerObject; // ตัวควบคุม Spawner

    private bool isUserLoggedIn = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // เริ่มต้นเกม: ปิด Spawner ไว้ และสั่งเปิดหน้า Main
        SetSpawnerActive(false);
        ShowMainPage();
    }

    void Update()
    {
        // 🟢 อัปเดตตัวเลข Score / Combo / Level บนหน้าจอขณะเล่นเกม
        if (GameManager.Instance != null && GameManager.Instance.isGameStarted && !GameManager.Instance.isGameOver)
        {
            if (scoreText != null) scoreText.text = "Score: " + GameManager.Instance.score;
            if (comboText != null) comboText.text = "Combo: " + GameManager.Instance.combo;
            if (levelText != null) levelText.text = "Level: " + GameManager.Instance.level;
        }

        // ----------------------------------------------------
        // ⌨️ ปุ่มลัดสำหรับทดสอบเกม (Debug Shortcuts)
        // ----------------------------------------------------

        // กด F1: ข้ามไปหน้าเริ่มเล่นเกม + เปิด Spawner + เดินเวลาเกมทันที
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("⚡ กด F1: ข้ามไปทดสอบเกมทันที");
            BypassToGame();
        }

        // กด F2: สลับ เปิด/ปิด Spawner ระหว่างทดสอบ
        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (spawnerObject != null)
            {
                bool currentState = spawnerObject.activeSelf;
                SetSpawnerActive(!currentState);
                Debug.Log("⚡ กด F2: สลับการทำงาน Spawner -> " + !currentState);
            }
        }
    }

    // ฟังก์ชันสั่ง เปิด/ปิด Spawner
    public void SetSpawnerActive(bool isActive)
    {
        if (spawnerObject != null)
        {
            // ถ้าสั่งเปิด ให้ Reset เผื่อ Script ใน Spawner ค้างอยู่
            if (isActive)
            {
                spawnerObject.SetActive(false);
                spawnerObject.SetActive(true);
            }
            else
            {
                spawnerObject.SetActive(false);
            }
        }
    }

    // ฟังก์ชันเริ่มเล่นเกมจริง
    public void StartGame()
    {
        HideAllPanels();

        // 🟢 เปิด Panel_Ingame เพื่อโชว์ Score/Combo/Level
        if (panelIngame != null) panelIngame.SetActive(true);

        // 1. ปล่อยเดินเวลาในเกมตามปกติ
        Time.timeScale = 1f;

        // 2. รีเซ็ตสถานะ Game Over (สำคัญมาก! ถ้าไม่รีเซ็ต Spawner จะไม่ยอมเสกของ)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame(); // สั่งให้ GameManager รีเซ็ตค่าและเปลี่ยนสถานะ isGameStarted = true
        }
        if (EnvironmentManager.Instance != null)
        {
            EnvironmentManager.Instance.ResetEnvironment();
        }

        // 3. สั่งเปิด Spawner ให้เริ่มเสกของจริง
        SetSpawnerActive(true);

        Debug.Log("🚀 เริ่มเล่นเกมเรียบร้อย! Spawner พร้อมทำงาน");
    }

    // ปุ่มลัดสำหรับข้ามไปทดสอบ
    public void BypassToGame()
    {
        isUserLoggedIn = true;
        StartGame();
    }

    public void HideAllPanels()
    {
        if (panelMainGuest != null) panelMainGuest.SetActive(false);
        if (panelLogin != null) panelLogin.SetActive(false);
        if (panelRegister != null) panelRegister.SetActive(false);
        if (panelMainUser != null) panelMainUser.SetActive(false);
        if (panelModeSelect != null) panelModeSelect.SetActive(false);
        if (panelDeviceConnect != null) panelDeviceConnect.SetActive(false);
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelIngame != null) panelIngame.SetActive(false); // 🟢 ซ่อน Ingame Panel เมื่อสลับหน้า
    }

    public void ShowMainPage()
    {
        HideAllPanels();
        SetSpawnerActive(false);

        // ⏸️ สั่งหยุดเวลาในเกมชั่วคราวขณะอยู่หน้าเมนู
        Time.timeScale = 0f;

        if (isUserLoggedIn)
        {
            if (panelMainUser != null) panelMainUser.SetActive(true);
        }
        else
        {
            if (panelMainGuest != null) panelMainGuest.SetActive(true);
        }
    }

    public void OpenLoginPanel()
    {
        HideAllPanels();
        if (panelLogin != null) panelLogin.SetActive(true);
    }

    public void OpenRegisterPanel()
    {
        HideAllPanels();
        if (panelRegister != null) panelRegister.SetActive(true);
    }

    public void SubmitLogin()
    {
        if (inputLoginUsername != null && !string.IsNullOrEmpty(inputLoginUsername.text))
        {
            isUserLoggedIn = true;
            ShowMainPage();
        }
    }

    public void SubmitRegister()
    {
        if (inputRegUsername != null && !string.IsNullOrEmpty(inputRegUsername.text))
        {
            OpenLoginPanel();
        }
    }

    public void Logout()
    {
        isUserLoggedIn = false;
        SetSpawnerActive(false);
        ShowMainPage();
    }

    public void OpenModeSelect()
    {
        HideAllPanels();
        if (panelModeSelect != null) panelModeSelect.SetActive(true);
    }

    public void OpenDeviceConnect()
    {
        HideAllPanels();
        if (panelDeviceConnect != null) panelDeviceConnect.SetActive(true);
    }

    // ฟังก์ชันจบเกม (Game Over)
    public void ShowGameOver()
    {
        HideAllPanels();
        SetSpawnerActive(false);
        if (panelGameOver != null) panelGameOver.SetActive(true);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.isGameOver = true;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }


    [Header("👻 FNAF Phantom Jumpscare (No Sound)")]
public RectTransform phantomImageTransform; // ลากวัตถุ PhantomJumpscare มาวางตรงนี้
public float jumpscareDuration = 0.5f;     // ระยะเวลาพุ่งใส่หน้า (0.5 วินาที)

private Coroutine jumpscareCoroutine;

// ฟังก์ชันสั่งทำ Phantom Jumpscare
public void TriggerPhantomJumpscare()
{
    if (jumpscareCoroutine != null)
    {
        StopCoroutine(jumpscareCoroutine);
    }
    jumpscareCoroutine = StartCoroutine(PhantomJumpscareRoutine());
}

private System.Collections.IEnumerator PhantomJumpscareRoutine()
{
    if (phantomImageTransform == null) yield break;

    // 1. เปิดภาพ UI
    phantomImageTransform.gameObject.SetActive(true);

    float elapsed = 0f;
    Vector3 startScale = new Vector3(0.3f, 0.3f, 1f); // สเกลเริ่มต้น
    Vector3 endScale = new Vector3(1.8f, 1.8f, 1f);   // สเกลปลายทาง
    Vector2 originalPos = phantomImageTransform.anchoredPosition;

    // 2. ลูปขยายใหญ่ + สั่นรัวๆ (FNAF Style)
    while (elapsed < jumpscareDuration)
    {
        elapsed += Time.deltaTime;
        float progress = elapsed / jumpscareDuration;

        // ขยายขนาดภาพพุ่งเข้าหาจอ
        phantomImageTransform.localScale = Vector3.Lerp(startScale, endScale, progress);

        // สั่นตำแหน่งภาพรัวๆ (Random Jitter)
        float shakeX = Random.Range(-30f, 30f);
        float shakeY = Random.Range(-30f, 30f);
        phantomImageTransform.anchoredPosition = originalPos + new Vector2(shakeX, shakeY);

        yield return null;
    }

    // 3. รีเซ็ตตำแหน่ง คืนค่า และปิดภาพ
    phantomImageTransform.anchoredPosition = originalPos;
    phantomImageTransform.gameObject.SetActive(false);
}
}