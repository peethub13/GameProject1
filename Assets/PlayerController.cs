using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;        // ความเร็วในการเลื่อนเปล
    public float minX = -7f;            // ขอบซ้ายสุดของจอ
    public float maxX = 7f;             // ขอบขวาสุดของจอ

    void Update()
    {
        // 1. รับค่าการกดคีย์บอร์ด (A/D หรือ ปุ่มลูกศรซ้าย/ขวา)
        float moveInput = Input.GetAxis("Horizontal");

        // 2. คำนวณตำแหน่งใหม่
        Vector3 newPosition = transform.position + new Vector3(moveInput * moveSpeed * Time.deltaTime, 0, 0);

        // 3. จำกัดไม่ให้เปลเลื่อนหลุดขอบหน้าจอ
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);

        // 4. อัปเดตตำแหน่งของเปล
        transform.position = newPosition;
    }
}