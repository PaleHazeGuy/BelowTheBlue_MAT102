
using UnityEngine;

// หากเรือเข้าสู่น้ำวน ให้แพ้ทันที
public class WhirlpoolHazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        RunState.Lose();
    }
}
