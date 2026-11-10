using UnityEngine;
using UnityEngine.UI;

public class MenuNavigation : MonoBehaviour
{
    [Header("--- 🔘 ลากปุ่ม UI มาใส่ช่องนี้ ---")]
    public Button[] menuButtons; // ลากปุ่มในหน้าจอมาใส่เรียงตามลำดับ 0, 1, 2...
    private int currentIndex = 0;

    private void OnEnable()
    {
        // ผูก Event การกดปุ่มจาก NetworkInputManager
        if (NetworkInputManager.Instance != null)
        {
            NetworkInputManager.Instance.OnMenuLeftPressed += HandleMenuLeft;
            NetworkInputManager.Instance.OnMenuRightPressed += HandleMenuRight;
            NetworkInputManager.Instance.OnMenuSelectPressed += HandleMenuSelect;
        }
        HighlightButton(currentIndex);
    }

    private void OnDisable()
    {
        // ยกเลิกการผูก Event เมื่อปิดหน้าเมนู
        if (NetworkInputManager.Instance != null)
        {
            NetworkInputManager.Instance.OnMenuLeftPressed -= HandleMenuLeft;
            NetworkInputManager.Instance.OnMenuRightPressed -= HandleMenuRight;
            NetworkInputManager.Instance.OnMenuSelectPressed -= HandleMenuSelect;
        }
    }

    private void HandleMenuLeft()
    {
        if (menuButtons == null || menuButtons.Length == 0) return;
        currentIndex = (currentIndex - 1 + menuButtons.Length) % menuButtons.Length;
        HighlightButton(currentIndex);
    }

    private void HandleMenuRight()
    {
        if (menuButtons == null || menuButtons.Length == 0) return;
        currentIndex = (currentIndex + 1) % menuButtons.Length;
        HighlightButton(currentIndex);
    }

    private void HandleMenuSelect()
    {
        if (menuButtons != null && menuButtons.Length > currentIndex && menuButtons[currentIndex] != null)
        {
            // จำลองการกดคลิกปุ่ม UI ที่กำลังไฮไลท์อยู่
            menuButtons[currentIndex].onClick.Invoke();
        }
    }

    private void HighlightButton(int index)
    {
        if (menuButtons == null) return;
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null)
            {
                // ขยายขนาดปุ่มที่กำลังเลือกให้ใหญ่ขึ้น 1.15 เท่า เพื่อให้ผู้เล่นรู้ว่าเลือกปุ่มไหนอยู่
                menuButtons[i].transform.localScale = (i == index) ? new Vector3(1.15f, 1.15f, 1f) : Vector3.one;
            }
        }
    }
}