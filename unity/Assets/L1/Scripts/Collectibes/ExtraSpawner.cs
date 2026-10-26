
using UnityEngine;

// สุ่มตำแหน่งเกิด Extra จากจุดที่เตรียมไว้
public class ExtraSpawner : MonoBehaviour
{
    [SerializeField] private GameObject extraPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private int extraToSpawn = 3;

    private void Start()
    {
        if (extraPrefab == null || spawnPoints == null) return;
        if (spawnPoints.Length == 0) return;

        // สุ่มจำนวนที่จะสร้างระหว่าง 0 ถึงค่าที่กำหนด
        int count = Random.Range(0, extraToSpawn + 1);

        count = Mathf.Min(count, spawnPoints.Length);

        // สุ่มจุดเกิดโดยไม่ใช้จุดเดิมซ้ำ
        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(i, spawnPoints.Length);

            Transform temp = spawnPoints[i];
            spawnPoints[i] = spawnPoints[index];
            spawnPoints[index] = temp;

            Instantiate(
                extraPrefab,
                spawnPoints[i].position,
                Quaternion.identity
            );
        }
    }
}
