using System.Collections.Generic;
using BelowTheBlue.Interaction;
using BelowTheBlue.World;
using UnityEngine;
using UnityEngine.UI;

namespace BelowTheBlue.UI
{
    [DisallowMultipleComponent]
    public sealed class RadarMinimap : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private RectTransform blipRoot;
        [SerializeField] private RectTransform sweep;
        [SerializeField] private Image blipTemplate;
        [SerializeField, Min(1f)] private float rangeWorld = 28f;
        [SerializeField, Min(0.5f)] private float sweepSeconds = 3f;
        [SerializeField] private Color sampleColor = new Color(0.78f, 0.95f, 0.31f, 1f);
        [SerializeField] private Color whirlpoolColor = new Color(1f, 0.3f, 0.2f, 1f);
        [SerializeField, Min(0.1f)] private float refreshSeconds = 0.5f;

        private readonly List<Collectible> samples = new List<Collectible>();
        private readonly List<ContactDamage> whirlpools = new List<ContactDamage>();
        private readonly List<Image> blips = new List<Image>();
        private float refreshTimer;
        private float sweepAngle;

        private void Awake()
        {
            if (blipTemplate != null)
            {
                blipTemplate.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void Update()
        {
            if (player == null || blipRoot == null)
            {
                return;
            }

            refreshTimer -= Time.unscaledDeltaTime;
            if (refreshTimer <= 0f)
            {
                Refresh();
            }

            sweepAngle = Mathf.Repeat(sweepAngle + 360f / sweepSeconds * Time.unscaledDeltaTime, 360f);
            if (sweep != null)
            {
                sweep.localRotation = Quaternion.Euler(0f, 0f, -sweepAngle);
            }

            float radius = Mathf.Min(blipRoot.rect.width, blipRoot.rect.height) * 0.5f;
            int used = 0;
            Vector2 origin = player.position;

            for (int i = 0; i < samples.Count; i++)
            {
                if (samples[i] == null)
                {
                    continue;
                }

                Place(ref used, (Vector2)samples[i].transform.position - origin, radius, sampleColor, true, 14f);
            }

            for (int i = 0; i < whirlpools.Count; i++)
            {
                if (whirlpools[i] == null)
                {
                    continue;
                }

                Place(ref used, (Vector2)whirlpools[i].transform.position - origin, radius, whirlpoolColor, false, 10f);
            }

            for (int i = used; i < blips.Count; i++)
            {
                blips[i].gameObject.SetActive(false);
            }
        }

        private void Place(ref int used, Vector2 offset, float radius, Color color, bool clampToRim, float size)
        {
            float distance = offset.magnitude;
            if (!clampToRim && distance > rangeWorld)
            {
                return;
            }

            float t = Mathf.Min(distance / rangeWorld, 1f);
            Vector2 direction = distance > 0.0001f ? offset / distance : Vector2.zero;
            Image blip = GetBlip(used++);
            blip.rectTransform.anchoredPosition = direction * t * radius * 0.94f;
            blip.rectTransform.sizeDelta = new Vector2(size, size);

            float blipAngle = Mathf.Repeat(90f - Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, 360f);
            float behind = Mathf.Repeat(sweepAngle - blipAngle, 360f) / 360f;
            float brightness = Mathf.Lerp(1f, 0.35f, behind);
            if (distance > rangeWorld)
            {
                brightness *= 0.6f;
            }

            color.a = brightness;
            blip.color = color;
        }

        private Image GetBlip(int index)
        {
            while (blips.Count <= index)
            {
                Image blip = Instantiate(blipTemplate, blipRoot);
                blip.gameObject.SetActive(true);
                blips.Add(blip);
            }

            Image result = blips[index];
            result.gameObject.SetActive(true);
            return result;
        }

        private void Refresh()
        {
            refreshTimer = refreshSeconds;
            samples.Clear();
            samples.AddRange(FindObjectsByType<Collectible>(FindObjectsSortMode.None));
            whirlpools.Clear();
            foreach (ContactDamage damage in FindObjectsByType<ContactDamage>(FindObjectsSortMode.None))
            {
                if (damage.Mode == ObstacleDamageMode.InstantKill)
                {
                    whirlpools.Add(damage);
                }
            }
        }
    }
}
