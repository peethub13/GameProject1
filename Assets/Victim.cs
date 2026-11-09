using UnityEngine;

public enum VictimType { Human, Dog, Cat, Zombie }

public class Victim : MonoBehaviour
{
    public VictimType type;
    private Rigidbody2D rb;

    [Header("Constant Movement Settings")]
    public float targetBounceHeight = 5.0f; 
    public float horizontalSpeed = 3.0f;     
    public float initialUpwardSpeed = 3.0f; // ประกาศตัวแปรความเร็วพุ่งขึ้นต้นเกม

    [Header("Lighting")]
    public GameObject spotlightObject;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        ApplyStatsByType();
        if (rb != null)
        {
            // กำหนดความเร็วเริ่มต้นตอนสปอว์นออกจากหน้าต่าง
            rb.linearVelocity = new Vector2(horizontalSpeed, initialUpwardSpeed);
        }
    }

    void Update()
    {
        // ระบบไฟฉาย
        if (spotlightObject != null && EnvironmentManager.Instance != null)
        {
            if (type == VictimType.Zombie)
            {
                spotlightObject.SetActive(false); // ซอมบี้ปิดไฟฉายเสมอ (เห็นแค่ตาสีแดง)
            }
            else
            {
                bool isNight = (EnvironmentManager.Instance.currentTime == EnvironmentTime.Night);
                spotlightObject.SetActive(isNight);
            }
        }

        // กันวัตถุหลุดออกนอกฉาก
        if (transform.position.y < -10f)
        {
            if (type != VictimType.Zombie && GameManager.Instance != null)
            {
                GameManager.Instance.VictimMissed();
            }
            Destroy(gameObject);
        }
    }

    void ApplyStatsByType()
    {
        if (rb == null) return;

        bool isNight = (EnvironmentManager.Instance != null && EnvironmentManager.Instance.currentTime == EnvironmentTime.Night);

        switch (type)
        {
            case VictimType.Human:
                rb.gravityScale = 1.0f;
                horizontalSpeed = 2.5f;
                targetBounceHeight = 5.0f;
                initialUpwardSpeed = 2.5f;
                break;

            case VictimType.Dog:
                rb.gravityScale = 1.6f;
                horizontalSpeed = 3.2f;
                targetBounceHeight = 5.5f;
                initialUpwardSpeed = 3.5f;
                break;

            case VictimType.Cat:
    rb.gravityScale = 2.4f;              // ลดแรงโน้มถ่วงลง (จากเดิม 2.4 เหลือ 1.2) ให้แมวตัวเบา ลอยนวล
    horizontalSpeed = 3.2f; 
    targetBounceHeight = 6.5f;           // เพิ่มความสูงตอนเด้งเปล (จากเดิม 6.0 เป็น 8.5)
    initialUpwardSpeed = 10.5f;          // เพิ่มแรงพุ่งออกจากหน้าต่าง (จากเดิม 5.0 เป็น 10.0)
    break;

            case VictimType.Zombie:
                rb.gravityScale = 1.8f;
                targetBounceHeight = 4.5f;

                // สุ่มสไตล์การพุ่งของซอมบี้ 3 รูปแบบ
                int zombiePattern = Random.Range(0, 3);
                if (zombiePattern == 0) // 1. แบบต่ำ (Low)
                {
                    horizontalSpeed = 3.5f;
                    initialUpwardSpeed = 2.0f;
                }
                else if (zombiePattern == 1) // 2. แบบไกล (Far)
                {
                    horizontalSpeed = 5.5f;
                    initialUpwardSpeed = 2.5f;
                }
                else // 3. แบบสูง (High)
                {
                    horizontalSpeed = 2.8f;
                    initialUpwardSpeed = 6.0f;
                }
                break;
        }
    }

    public void ApplyConstantBounce()
    {
        if (rb == null) return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        float g = Mathf.Abs(Physics2D.gravity.y) * rb.gravityScale;
        float calculatedVelocityY = Mathf.Sqrt(2f * g * targetBounceHeight);

        rb.linearVelocity = new Vector2(horizontalSpeed, calculatedVelocityY);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. ชนเปล
        if (collision.CompareTag("Stretcher"))
        {
            if (type == VictimType.Zombie)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ResetCombo(); // ชนเปล -> ล้าง Combo เหลือ 0
                }
                Destroy(gameObject);
            }
            else
            {
                ApplyConstantBounce();
            }
        }
        // 2. เข้ารถพยาบาล
        else if (collision.CompareTag("Ambulance"))
        {
            if (type == VictimType.Zombie)
            {
                Destroy(gameObject);
            }
            else
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.VictimSaved(type);
                }
                Destroy(gameObject);
            }
        }
        // 3. ตกพื้น
        else if (collision.CompareTag("Ground"))
        {
            if (type == VictimType.Zombie)
            {
                Destroy(gameObject); // ซอมบี้ตกพื้นไม่ Game Over
            }
            else
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.VictimMissed();
                }
                Destroy(gameObject);
            }
        }
    }
}