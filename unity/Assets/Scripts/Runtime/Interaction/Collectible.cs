using System;
using UnityEngine;

namespace BelowTheBlue.Interaction
{
    [DisallowMultipleComponent]
    public sealed class Collectible : MonoBehaviour
    {
        public static event Action<Collectible> Collected;

        [SerializeField] private string itemId = "sample";

        public string ItemId => itemId;

        public void Collect()
        {
            Collected?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
