using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreboardManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentContainer; // ลาก Object 'Content' ใน ScrollView มาใส่
    public GameObject itemPrefab;      // ลาก Prefab แถวรายการมาใส่

    [Header("My Rank Pinned UI")]
    public GameObject myRankRow;       // 📌 ลาก Object 'MyRankRow' ที่ตรึงอยู่ด้านล่างมาใส่ช่องนี้

    void OnEnable()
    {
        LoadScoreboard();
    }

    public void LoadScoreboard()
    {
        if (NetworkManager.Instance != null)
        {
            Debug.Log("⏳ กำลังโหลดตารางคะแนน...");
            NetworkManager.Instance.GetScoreboard(DisplayScoreboard);
        }
    }

    private void DisplayScoreboard(List<ScoreboardPlayer> players)
    {
        // 1. ลบรายการเก่าใน ScrollView
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        ScoreboardPlayer myData = null;
        string myUsername = NetworkManager.Instance != null ? NetworkManager.Instance.currentUsername : "";

        // 2. วาดรายการคะแนนลงใน ScrollView พร้อมวนหาอันดับตัวเอง
        foreach (ScoreboardPlayer player in players)
        {
            GameObject item = Instantiate(itemPrefab, contentContainer);
            SetRowData(item, player.rank.ToString(), player.username, player.bestScore);

            // ตรวจสอบว่าเป็นชื่อผู้เล่นที่กำลังล็อกอินอยู่หรือไม่
            if (!string.IsNullOrEmpty(myUsername) && player.username == myUsername)
            {
                myData = player;
            }
        }

        // 3. แสดงผลข้อมูลของตัวเองที่แถบ MyRankRow ด้านล่าง
        if (myRankRow != null)
        {
            if (myData != null)
            {
                // ถ้าเจอชื่อตัวเองในตารางคะแนน
                SetRowData(myRankRow, myData.rank.ToString(), myData.username, myData.bestScore);
            }
            else
            {
                // ถ้ายังไม่เจอ (เช่น ยังไม่เคยเล่น หรือยังไม่มีคะแนนติด Top)
                SetRowData(myRankRow, "-", string.IsNullOrEmpty(myUsername) ? "GUEST" : myUsername, 0);
            }
        }

        Debug.Log("✅ แสดงผลตารางคะแนนและอันดับส่วนตัวเรียบร้อย!");
    }

    // 🛠️ Helper สำหรับใส่ข้อมูลลงใน Text Components
    private void SetRowData(GameObject rowObject, string rankText, string usernameText, int score)
    {
        TextMeshProUGUI[] textComponents = rowObject.GetComponentsInChildren<TextMeshProUGUI>();

        if (textComponents.Length >= 3)
        {
            textComponents[0].text = rankText.StartsWith("#") || rankText == "-" ? rankText : "#" + rankText;
            textComponents[1].text = usernameText;
            textComponents[2].text = score.ToString("N0");
        }
    }
}