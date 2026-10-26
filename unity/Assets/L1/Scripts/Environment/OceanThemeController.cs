
using UnityEngine;

// เปลี่ยนสีบรรยากาศตามระดับความสูง
public class OceanThemeController : MonoBehaviour
{
    [SerializeField] private Transform boat;
    [SerializeField] private Camera gameCamera;

    [SerializeField] private float theme2Height = 80f;
    [SerializeField] private float theme3Height = 160f;

    [SerializeField]
    private Color deepColor =
        new Color(0.02f, 0.06f, 0.12f);

    [SerializeField]
    private Color coralColor =
        new Color(0.03f, 0.18f, 0.24f);

    [SerializeField]
    private Color upperColor =
        new Color(0.10f, 0.35f, 0.48f);

    [SerializeField] private float blendSpeed = 1f;

    private void Update()
    {
        if (boat == null || gameCamera == null) return;

        float y = boat.position.y;
        Color targetColor;

        if (y < theme2Height)
        {
            targetColor = deepColor;
        }
        else if (y < theme3Height)
        {
            targetColor = coralColor;
        }
        else
        {
            targetColor = upperColor;
        }

        gameCamera.backgroundColor = Color.Lerp(
            gameCamera.backgroundColor,
            targetColor,
            blendSpeed * Time.deltaTime
        );
    }
}
