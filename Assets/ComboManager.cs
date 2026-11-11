using UnityEngine;
using TMPro;
using System.Collections;

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance;

    [Header("--- 💬 UI Display ---")]
    public TMP_Text comboText;          // แสดงตัวเลข Combo (เช่น Combo: 5)
    public TMP_Text txtComboFeedback;   // 🟢 แสดงข้อความ GOOD! / GREAT! / EXCELLENT!

    [Header("--- ⏱️ Settings ---")]
    [Tooltip("ระยะเวลาที่ให้ข้อความแสดงก่อนจะจางหายไป (วินาที)")]
    public float displayDuration = 1.0f;

    private int currentCombo = 0;
    private Coroutine feedbackRoutine;
    private Vector3 originalScale = Vector3.one;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (txtComboFeedback != null)
        {
            originalScale = txtComboFeedback.transform.localScale;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameStarted)
        {
            if (currentCombo != GameManager.Instance.combo)
            {
                int newCombo = GameManager.Instance.combo;
                
                if (newCombo > currentCombo)
                {
                    CheckComboMilestone(newCombo);
                }
                
                currentCombo = newCombo;
                UpdateComboTextOnly();
            }
        }
    }

    public void AddCombo()
    {
        currentCombo++;
        CheckComboMilestone(currentCombo);
        UpdateComboTextOnly();
    }

    public void ResetCombo()
    {
        currentCombo = 0;
        
        if (comboText != null) 
            comboText.text = "";

        if (txtComboFeedback != null)
            txtComboFeedback.gameObject.SetActive(false);
    }

    private void UpdateComboTextOnly()
    {
        if (comboText != null)
        {
            comboText.text = currentCombo > 0 ? "Combo: " + currentCombo : "";
        }
    }

    private void CheckComboMilestone(int comboVal)
    {
        if (txtComboFeedback == null) return;

        if (comboVal == 5)
        {
            ShowFeedback("GOOD!", Color.green);
        }
        else if (comboVal == 10)
        {
            ShowFeedback("GREAT!", Color.yellow);
        }
        else if (comboVal >= 15 && comboVal % 5 == 0)
        {
            ShowFeedback("EXCELLENT!", Color.magenta);
        }
    }

    private void ShowFeedback(string message, Color color)
    {
        txtComboFeedback.text = message;
        txtComboFeedback.color = color;
        txtComboFeedback.gameObject.SetActive(true);

        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }
        feedbackRoutine = StartCoroutine(JuicyPopRoutine());
    }

    // 💥 Coroutine เด้งสปริง + สุ่มมุมหมุน + Fade Out จางหาย
    private IEnumerator JuicyPopRoutine()
    {
        Transform tf = txtComboFeedback.transform;

        // 1. สุ่มมุมหมุนเอียงเล็กน้อย (-6 ถึง +6 องศา) เพิ่มความดุดัน
        float randomAngle = Random.Range(-6f, 6f);
        tf.localRotation = Quaternion.Euler(0, 0, randomAngle);

        // 2. เด้งขยายใหญ่ขึ้นสไตล์ Elastic Pop (ใหญ่สุด 1.5 เท่า)
        Vector3 peakScale = originalScale * 1.5f;
        Vector3 dipScale = originalScale * 0.9f;
        
        float time = 0f;
        // ช่วงที่ 1: พุ่งใหญ่ขึ้นไวมาก
        while (time < 0.1f)
        {
            time += Time.deltaTime;
            tf.localScale = Vector3.Lerp(originalScale, peakScale, time / 0.1f);
            yield return null;
        }

        time = 0f;
        // ช่วงที่ 2: ย่อลงต่ำกว่าขนาดปกติเล็กน้อย (สปริง)
        while (time < 0.08f)
        {
            time += Time.deltaTime;
            tf.localScale = Vector3.Lerp(peakScale, dipScale, time / 0.08f);
            yield return null;
        }

        // ช่วงที่ 3: เด้งกลับมาขนาดปกติ
        tf.localScale = originalScale;

        // 3. ค้างไว้ตามเวลา displayDuration
        yield return new WaitForSeconds(displayDuration);

        // 4. Fade Out (ค่อยๆ โปร่งแสงจางหายไปใน 0.3 วินาที)
        time = 0f;
        Color startColor = txtComboFeedback.color;
        
        while (time < 0.3f)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, time / 0.3f);
            txtComboFeedback.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // คืนค่า Alpha และปิดการแสดงผล
        txtComboFeedback.color = startColor;
        tf.localRotation = Quaternion.identity;
        txtComboFeedback.gameObject.SetActive(false);
    }
}