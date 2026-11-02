using System;
using System.Collections;
using BelowTheBlue.Core;
using UnityEngine;

namespace BelowTheBlue.Interaction
{
    [DisallowMultipleComponent]
    public sealed class CollectorArm : MonoBehaviour
    {
        [Header("Parts (scene objects, art only)")]
        [Tooltip("Positioned and rotated by this script. The segment and claw are its children, drawn pointing right.")]
        [SerializeField] private Transform armRoot;
        [Tooltip("Stretched along the arm. Needs a sprite whose pivot is its left-centre.")]
        [SerializeField] private SpriteRenderer segment;
        [Tooltip("Sits at the tip. Needs a sprite whose pivot is its left-centre (the wrist).")]
        [SerializeField] private SpriteRenderer claw;
        [SerializeField] private Sprite clawOpen;
        [SerializeField] private Sprite clawClosed;
        [SerializeField] private SpriteRenderer facingRenderer;
        [SerializeField] private Health health;

        [Header("Mount (world units from the sub's centre)")]
        [SerializeField] private float mountForward = 1.4f;
        [SerializeField] private float mountVertical = -0.7f;

        [Header("Motion")]
        [SerializeField, Min(0.05f)] private float extendSeconds = 0.3f;
        [SerializeField, Min(0f)] private float grabPauseSeconds = 0.12f;
        [SerializeField, Min(0.05f)] private float retractSeconds = 0.35f;
        [Tooltip("How far past the wrist the claw holds the item.")]
        [SerializeField, Min(0f)] private float gripOffset = 0.45f;
        [SerializeField, Min(0.01f)] private float segmentThickness = 0.3f;

        [Header("Draw order")]
        [SerializeField] private int segmentOrder = 4;
        [SerializeField] private int clawOrder = 5;
        [SerializeField] private int carriedItemOrder = 3;

        public bool IsBusy { get; private set; }

        public event Action Grabbed;

        private Coroutine routine;

        private void Awake()
        {
            facingRenderer ??= GetComponent<SpriteRenderer>();
            health ??= GetComponent<Health>();

            Transform parent = armRoot != null ? armRoot.parent : null;
            if (parent != null)
            {
                Vector3 parentScale = parent.lossyScale;
                if (Mathf.Abs(parentScale.x) > 0.0001f && Mathf.Abs(parentScale.y) > 0.0001f)
                {
                    armRoot.localScale = new Vector3(1f / parentScale.x, 1f / parentScale.y, 1f);
                }
            }

            if (segment != null)
            {
                segment.drawMode = SpriteDrawMode.Sliced;
                segment.sortingOrder = segmentOrder;
            }

            if (claw != null)
            {
                claw.sortingOrder = clawOrder;
            }

            SetVisible(false);
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= OnDied;
            }
        }

        public bool TryGrab(Collectible item)
        {
            if (IsBusy || item == null || armRoot == null || segment == null || claw == null)
            {
                return false;
            }

            if (!item.BeginCapture())
            {
                return false;
            }

            routine = StartCoroutine(GrabRoutine(item));
            return true;
        }

        private IEnumerator GrabRoutine(Collectible item)
        {
            IsBusy = true;
            Vector2 grabPoint = item.transform.position;
            Vector3 startScale = item.transform.localScale;
            SpriteRenderer itemRenderer = item.Renderer;
            if (itemRenderer != null)
            {
                itemRenderer.sortingOrder = carriedItemOrder;
            }

            claw.sprite = clawOpen;
            SetVisible(true);

            for (float t = 0f; t < extendSeconds; t += Time.deltaTime)
            {
                Aim(grabPoint, Smooth(t / extendSeconds), out _, out _);
                yield return null;
            }

            Aim(grabPoint, 1f, out _, out _);

            claw.sprite = clawClosed;
            Grabbed?.Invoke();
            if (grabPauseSeconds > 0f)
            {
                float pause = 0f;
                while (pause < grabPauseSeconds)
                {
                    pause += Time.deltaTime;
                    Aim(grabPoint, 1f, out _, out _);
                    yield return null;
                }
            }

            for (float t = 0f; t < retractSeconds; t += Time.deltaTime)
            {
                float k = 1f - Smooth(t / retractSeconds);
                Aim(grabPoint, k, out Vector2 tip, out Vector2 direction);
                if (item != null)
                {
                    item.transform.position = new Vector3(
                        tip.x + direction.x * gripOffset,
                        tip.y + direction.y * gripOffset,
                        item.transform.position.z);
                    item.transform.localScale = startScale * Smooth(Mathf.Clamp01(k / 0.4f));
                }

                yield return null;
            }

            SetVisible(false);
            IsBusy = false;
            routine = null;
            if (item != null)
            {
                item.CompleteCapture();
            }
        }

        private void Aim(Vector2 grabPoint, float extension, out Vector2 tip, out Vector2 direction)
        {
            Vector2 mount = GetMount();
            Vector2 toTarget = grabPoint - mount;
            float distance = toTarget.magnitude;
            direction = distance > 0.0001f ? toTarget / distance : Vector2.left;

            float reach = Mathf.Max(0f, distance - gripOffset);
            float length = reach * extension;
            tip = mount + direction * length;

            armRoot.position = new Vector3(mount.x, mount.y, armRoot.position.z);
            armRoot.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            bool pointingLeft = direction.x < 0f;
            segment.flipY = pointingLeft;
            claw.flipY = pointingLeft;
            segment.size = new Vector2(length, segmentThickness);
            claw.transform.localPosition = new Vector3(length, 0f, 0f);
        }

        private Vector2 GetMount()
        {
            float side = facingRenderer != null && facingRenderer.flipX ? 1f : -1f;
            return (Vector2)transform.position
                + (Vector2)transform.right * (side * mountForward)
                + (Vector2)transform.up * mountVertical;
        }

        private void OnDied()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            IsBusy = false;
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (armRoot != null)
            {
                armRoot.gameObject.SetActive(visible);
            }
        }

        private static float Smooth(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }
    }
}
