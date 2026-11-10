using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("--- Movement Settings ---")]
    public float moveSpeed = 5f; // ความเร็วในการขยับ

    void Update()
    {
        // 1. ตรวจสอบว่า NetworkInputManager ทำงานอยู่หรือไม่
        if (NetworkInputManager.Instance == null) return;

        // 2. ดึงค่าทิศทาง (-1 = ซ้าย, 0 = อยู่เฉยๆ, 1 = ขวา) ที่มาจาก ESP32
        float moveX = NetworkInputManager.Instance.horizontalInput;

        // 3. สั่งขยับตำแหน่ง GameObject ไปทางซ้ายหรือขวา
        transform.Translate(Vector3.right * moveX * moveSpeed * Time.deltaTime);
    }
}