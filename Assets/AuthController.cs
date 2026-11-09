using UnityEngine;
using TMPro;

public class AuthController : MonoBehaviour
{
    [Header("--- 📱 UI Panels ---")]
    public GameObject panelMainGuest;
    public GameObject panelMainUser;
    public GameObject panelLogin;
    public GameObject panelRegister;

    [Header("--- 📝 Register Inputs ---")]
    public TMP_InputField regUsernameInput;
    public TMP_InputField regPasswordInput;

    [Header("--- 🔑 Login Inputs ---")]
    public TMP_InputField loginUsernameInput;
    public TMP_InputField loginPasswordInput;

    // ----------------------------------------------------
    // 🔄 1. ฟังก์ชันสลับหน้า Panel UI
    // ----------------------------------------------------
    public void OpenPanelRegister()
    {
        CloseAllPanels();
        if (panelRegister != null) panelRegister.SetActive(true);
    }

    public void OpenPanelLogin()
    {
        CloseAllPanels();
        if (panelLogin != null) panelLogin.SetActive(true);
    }

    public void OpenPanelMainGuest()
    {
        CloseAllPanels();
        if (panelMainGuest != null) panelMainGuest.SetActive(true);
    }

    public void OpenPanelMainUser()
    {
        CloseAllPanels();
        if (panelMainUser != null) panelMainUser.SetActive(true);
    }

    private void CloseAllPanels()
    {
        if (panelMainGuest != null) panelMainGuest.SetActive(false);
        if (panelMainUser != null) panelMainUser.SetActive(false);
        if (panelLogin != null) panelLogin.SetActive(false);
        if (panelRegister != null) panelRegister.SetActive(false);
    }

    // ----------------------------------------------------
    // 📝 2. ฟังก์ชันกดปุ่ม Register
    // ----------------------------------------------------
    public void OnClickRegister()
    {
        Debug.Log("🔥 ปุ่ม Register โดนกดแล้ว!");
        string user = regUsernameInput.text.Trim();
        string pass = regPasswordInput.text.Trim();

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            Debug.LogWarning("⚠️ กรุณากรอกข้อมูลให้ครบถ้วน");
            return;
        }

        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.Register(user, pass);
            // สมัครเสร็จแล้วเปิดหน้า Login ให้ทันที
            OpenPanelLogin();
        }
    }

    // ----------------------------------------------------
    // 🔑 3. ฟังก์ชันกดปุ่ม Login
    // ----------------------------------------------------
    public void OnClickLogin()
    {
        Debug.Log("🔥 ปุ่ม Login โดนกดแล้ว!");
        string user = loginUsernameInput.text.Trim();
        string pass = loginPasswordInput.text.Trim();

        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            Debug.LogWarning("⚠️ กรุณากรอกข้อมูลให้ครบถ้วน");
            return;
        }

        if (NetworkManager.Instance != null)
        {
            // 1. บันทึกชื่อผู้เล่นปัจจุบันลง NetworkManager
            NetworkManager.Instance.currentUsername = user;

            // 2. เรียกยิง Login API (ส่ง 2 Arguments)
            NetworkManager.Instance.Login(user, pass);

            // 3. สลับหน้าเป็น MainUser และดึงตารางคะแนน
            OnLoginSuccess();
        }
    }

    // ----------------------------------------------------
    // 🎉 4. ฟังก์ชันทำงานเมื่อ Login สำเร็จ
    // ----------------------------------------------------
    public void OnLoginSuccess()
    {
        Debug.Log("✅ Login สำเร็จ! กำลังเปิดหน้า Panel_MainUser...");

        // 1. สลับหน้าเป็น MainUser
        OpenPanelMainUser();

        // 2. สั่งให้ ScoreboardManager ในหน้า MainUser อัปเดตข้อมูลตารางคะแนนและอันดับทันที
        if (panelMainUser != null)
        {
            ScoreboardManager scoreboard = panelMainUser.GetComponentInChildren<ScoreboardManager>(true);
            if (scoreboard != null)
            {
                scoreboard.LoadScoreboard();
            }
        }
    }
}