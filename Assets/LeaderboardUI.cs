using UnityEngine;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform contentContainer;

    void OnEnable()
    {
        LoadLeaderboardData();
    }

    public void LoadLeaderboardData()
    {
        // ลบรายการเก่าออกก่อน
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        // Mockup ข้อมูลตัวอย่าง
        CreateItem("#1", "Player 1", "4,000 pts");
        CreateItem("#2", "Player 2", "4,000 pts");
        CreateItem("#3", "You", "1,200 pts");
        CreateItem("#4", "Player 4", "50 pts");
    }

    void CreateItem(string rank, string name, string score)
    {
        GameObject newItem = Instantiate(itemPrefab, contentContainer);
        TMP_Text[] texts = newItem.GetComponentsInChildren<TMP_Text>();
        
        if (texts.Length >= 3)
        {
            texts[0].text = rank;
            texts[1].text = name;
            texts[2].text = score;
        }
    }
}