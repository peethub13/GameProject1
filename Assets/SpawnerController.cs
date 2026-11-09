using UnityEngine;

public class SpawnerController : MonoBehaviour
{
    [Header("การขยับ ขึ้น-ลง (Up & Down Movement)")]
    public float moveSpeed = 3f;      // ความเร็วในการเคลื่อนที่
    public float topY = 5f;           // จุดสูงสุด (พิกัด Y ด้านบน)
    public float bottomY = -5f;       // จุดต่ำสุด (พิกัด Y ด้านล่าง)
    private int direction = 1;        // 1 = กำลังขยับขึ้น, -1 = กำลังขยับลง

    void Update()
    {
        // --- เลื่อนขึ้น/ลง ตามทิศทางปัจจุบัน ---
        transform.Translate(Vector3.up * moveSpeed * direction * Time.deltaTime);

        // ถ้าขยับขึ้นจนถึงขอบบนสุด (topY) ให้เปลี่ยนทิศเป็น "ลง" (-1)
        if (transform.position.y >= topY)
        {
            direction = -1;
        }
        // ถ้าขยับลงจนถึงขอบล่างสุด (bottomY) ให้เปลี่ยนทิศเป็น "ขึ้น" (1)
        else if (transform.position.y <= bottomY)
        {
            direction = 1;
        }
    }
}