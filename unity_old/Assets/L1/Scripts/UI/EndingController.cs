
using UnityEngine;

// เลือกฉากจบตามจำนวน Extra ที่เก็บได้
public class EndingController : MonoBehaviour
{
    [SerializeField] private GameObject[] endingPanels;

    private void Start()
    {
        // ปิดทุกฉากจบก่อน
        foreach (GameObject panel in endingPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        // จำกัด Index ให้อยู่ระหว่าง 0 ถึง 3
        int index = Mathf.Clamp(RunState.ExtraCount, 0, 3);

        // เปิดเฉพาะฉากจบที่ตรงกับจำนวน Extra
        if (endingPanels.Length > index &&
            endingPanels[index] != null)
        {
            endingPanels[index].SetActive(true);
        }
    }
}
