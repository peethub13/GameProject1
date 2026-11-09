using UnityEngine;
using TMPro;

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance;

    [Header("--- 💬 UI Display ---")]
    public TMP_Text comboText; // ลาก Text (TMP) ที่จะแสดงคำชมมาใส่ช่องนี้

    private int currentCombo = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Call ฟังก์ชันนี้ทุกครั้งที่นำ Object เข้ารถพยาบาลสำเร็จ 1 ตัว
    public void AddCombo()
    {
        currentCombo++;
        UpdateComboUI();
    }

    // Call ฟังก์ชันนี้เมื่อหลุด Combo หรือจบเกม/เริ่มเกมใหม่
    public void ResetCombo()
    {
        currentCombo = 0;
        if (comboText != null) 
            comboText.text = "";
    }

    // ฟังก์ชันคำนวณและแสดงคำชมตามจำนวน Combo
    private void UpdateComboUI()
    {
        if (comboText == null) return;

        if (currentCombo >= 15)
        {
            comboText.text = "เยี่ยมมากเยี่ยมมาย 🔥";
        }
        else if (currentCombo >= 10)
        {
            comboText.text = "เยี่ยมมาก 👍";
        }
        else if (currentCombo >= 5)
        {
            comboText.text = "หวานเจี๊ยบ ✨";
        }
        else
        {
            // ถ้ายังไม่ถึง 5 ตัว สามารถเลือกได้ว่าให้อนข้างว่างไว้ หรือโชว์จำนวนนับแบบปกติก่อน
            comboText.text = $"x{currentCombo}"; 
        }
    }
}