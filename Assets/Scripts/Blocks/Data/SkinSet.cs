using System;
using UnityEngine;

namespace Blocks.Data
{
    /// <summary>
    /// A single themed skin set identified by <see cref="SkinId"/> and a collection of slot sprites.
    /// The integer slot index is a generic "variant" selector (e.g., default, state A/B/C, etc.).
    /// </summary>
    [CreateAssetMenu(fileName = "SkinSet", menuName = "Blocks/Skin Set")]
    public sealed class SkinSet : ScriptableObject
    {
        public int SkinId;

        public Sprite[] Slots;
        
        public Sprite Get(int slot)
        {
            if (Slots == null || Slots.Length == 0)
            {
                return null;
            }

            if (slot >= Slots.Length)
            {
                slot = Math.Clamp(slot, 0, Slots.Length - 1);
            }
            
            return Slots[slot];
        }
    }
}