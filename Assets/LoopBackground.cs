using UnityEngine;

public class LoopBackground : MonoBehaviour
{
    public float speed = 2.0f; // ความเร็วในการขยับ
    public float resetPositionX = -10f; // จุดที่ให้เด้งกลับไปเริ่มใหม่
    public float startPositionX = 10f;  // จุดเริ่มต้นใหม่

    void Update()
    {
        // เลื่อนตำแหน่งไปทางซ้ายเรื่อยๆ
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        // ถ้าเลื่อนไปจนพ้นจอ ให้ย้ายกลับไปเริ่มที่จุดเริ่มต้น
        if (transform.position.x <= resetPositionX)
        {
            transform.position = new Vector3(startPositionX, transform.position.y, transform.position.z);
        }
    }
}