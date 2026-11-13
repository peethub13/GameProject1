using UnityEngine;

public class StretcherController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 12f;
    public float minX = -6f; // ขอบซ้ายสุด
    public float maxX = 6f;  // ขอบขวาสุด

    [Header("Invert Control")]
    public bool invertDirection = true; // กลับทิศทางควบคุม

    void Update()
{
    if (GameManager.Instance != null &&
        GameManager.Instance.isGameOver)
        return;

    float moveInput = 0f;

    // ใช้ Hardware เฉพาะเมื่อมีการส่งข้อมูลควบคุมจริง
    bool useHardware =
        NetworkInputManager.Instance != null &&
        NetworkInputManager.Instance.IsHardwareConnected &&
        NetworkInputManager.Instance.isGameActive;

    if (useHardware)
    {
        moveInput = NetworkInputManager.Instance.horizontalInput;
    }
    else
    {
        // Keyboard: รองรับลูกศรและ A/D
        moveInput = Input.GetAxisRaw("Horizontal");

        if (invertDirection)
            moveInput *= -1f;
    }

    Vector3 pos = transform.position;
    pos.x += moveInput * moveSpeed * Time.deltaTime;
    pos.x = Mathf.Clamp(pos.x, minX, maxX);
    transform.position = pos;
}
    // ----------------------------------------------------
    // 🧟‍♂️ ระบบตรวจจับการชนกับ ซอมบี้ (Phantom Jumpscare)
    // ----------------------------------------------------

    // สำหรับ Collider2D ที่ติ๊ก Is Trigger = true
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleCollision(other.gameObject);
    }

    // สำหรับ Collider2D แบบชนแข็งปกติ (ไม่ได้ติ๊ก Is Trigger)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject obj)
    {
        // เช็กว่าวัตถุที่ตกมาโดนเปลคือ ซอมบี้ หรือไม่
        if (obj.CompareTag("Zombie"))
        {
            // 1. เรียกทำ Phantom Jumpscare (ภาพพุ่งใส่หน้า + สั่นรัวๆ + เสียง)
            if (UIManager.Instance != null)
            {
                //UIManager.Instance.TriggerPhantomJumpscare();
            }

            // 2. หักคะแนน
            if (GameManager.Instance != null)
            {
                // ✅ เปลี่ยนเป็นเข้าถึงตัวแปร score โดยตรง
GameManager.Instance.score -= 20;
            }

            // 3. ทำลายซอมบี้ทิ้งทันที
            Destroy(obj);

            Debug.Log("😱 เปลชนซอมบี้! เรียก Phantom Jumpscare");
        }
    }
}