
using UnityEngine;

// เปลี่ยนภาพเรือตามจำนวนครั้งที่ชน
public class SubmarineDamageVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer boatRenderer;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite damagedSprite1;
    [SerializeField] private Sprite damagedSprite2;

    private int previousHitCount = 0;

    private void Update()
    {
        if (RunState.HitCount == previousHitCount) return;

        previousHitCount = RunState.HitCount;

        if (boatRenderer == null) return;

        if (previousHitCount == 1 && damagedSprite1 != null)
        {
            boatRenderer.sprite = damagedSprite1;
        }
        else if (previousHitCount == 2 && damagedSprite2 != null)
        {
            boatRenderer.sprite = damagedSprite2;
        }
    }
}
