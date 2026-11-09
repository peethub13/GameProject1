using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    [Header("Victim Prefabs")]
    public GameObject humanPrefab;
    public GameObject dogPrefab;
    public GameObject catPrefab;
    public GameObject zombiePrefab;

    [Header("Spawn Settings")]
    public Transform[] spawnPoints;

    [Header("Normal Victims Interval (Human, Dog, Cat)")]
    public float normalSpawnInterval = 3.7f; // ความถี่คงที่ของ คน หมา แมว (อ้างอิงจากช่วงกลางวัน)

    [Header("Zombie Intervals by Time")]
    public float dayZombieInterval = 8.0f;     // กลางวัน: ซอมบี้ออกช้าสุด
    public float sunsetZombieInterval = 4.0f;  // ตอนเย็น: ซอมบี้ออกปานกลาง
    public float nightZombieInterval = 1.5f;   // กลางคืน: ซอมบี้ออกถี่สุด

    private float normalTimer;
    private float zombieTimer;

    void Update()
    {
        if (GameManager.Instance != null)
        {
            if (!GameManager.Instance.isGameStarted || GameManager.Instance.isGameOver) return;
        }

        // --- 1. เสก คน หมา แมว (ความถี่เท่ากันทุกช่วงเวลา) ---
        normalTimer += Time.deltaTime;
        if (normalTimer >= normalSpawnInterval)
        {
            normalTimer = 0f;
            SpawnNormalVictim(); // สุ่มเสกแค่ คน หมา แมว
        }

        // --- 2. เสก ซอมบี้ (ความถี่เปลี่ยนตามช่วงเวลา) ---
        float currentZombieInterval = dayZombieInterval; // ค่าเริ่มต้น (กลางวัน)

        if (EnvironmentManager.Instance != null)
        {
            switch (EnvironmentManager.Instance.currentTime)
            {
                case EnvironmentTime.Day:
                    currentZombieInterval = dayZombieInterval;
                    break;
                case EnvironmentTime.Sunset:
                    currentZombieInterval = sunsetZombieInterval;
                    break;
                case EnvironmentTime.Night:
                    currentZombieInterval = nightZombieInterval; // กลางคืนถี่สุด
                    break;
            }
        }

        zombieTimer += Time.deltaTime;
        if (zombieTimer >= currentZombieInterval)
        {
            zombieTimer = 0f;
            SpawnSpecificPrefab(zombiePrefab); // สั่งเสกเฉพาะซอมบี้
        }
    }

    // สุ่มเสกเฉพาะ คน, หมา, แมว (โอกาสอย่างละ 33.3%)
    void SpawnNormalVictim()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        float rand = Random.value;
        GameObject prefabToSpawn = humanPrefab;

        if (rand < 0.33f)         prefabToSpawn = humanPrefab;
        else if (rand < 0.66f)    prefabToSpawn = dogPrefab;
        else                      prefabToSpawn = catPrefab;

        SpawnAtPoint(prefabToSpawn, spawnPoint);
    }

    // เสก Prefab ที่กำหนดลงบนจุดสุ่ม
    void SpawnSpecificPrefab(GameObject prefab)
    {
        if (spawnPoints == null || spawnPoints.Length == 0 || prefab == null) return;
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        SpawnAtPoint(prefab, spawnPoint);
    }

    // ฟังก์ชันสร้าง Object พร้อมใส่แรงดัน
    void SpawnAtPoint(GameObject prefabToSpawn, Transform spawnPoint)
    {
        if (prefabToSpawn == null) return;

        GameObject spawnedObj = Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);

        Rigidbody2D rb = spawnedObj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (prefabToSpawn == dogPrefab || prefabToSpawn == catPrefab)
            {
                rb.AddForce(Vector2.up * 8f, ForceMode2D.Impulse);
            }
        }
    }
}