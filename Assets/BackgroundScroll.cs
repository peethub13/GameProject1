using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    [Header("ความเร็วการเลื่อนฉาก")]
    public float speed = 0.2f;

    private Material mat;

    void Start()
    {
        // ดึง Material ของ SpriteRenderer มาใช้งาน
        mat = GetComponent<SpriteRenderer>().material;
    }

    void Update()
    {
        // เลื่อนภาพข้ามแกน X ไปเรื่อยๆ แบบวนลูปอัตโนมัติ
        mat.mainTextureOffset += new Vector2(speed * Time.deltaTime, 0);
    }
}