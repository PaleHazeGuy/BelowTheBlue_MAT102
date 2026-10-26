
using UnityEngine;

// ตรวจจับเมื่อเรือชนหิน
public class RockHazard : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        int hits = RunState.RegisterRockHit();

        // หลังชนครั้งที่ 3 RunState จะจบเกมทันที
        if (hits >= 3) return;

        // ป้องกันชนหินก้อนเดิมซ้ำติดกัน
        // ในเกมจริงควรมีระบบพักการชนหรือผลักเรือออก
    }
}
