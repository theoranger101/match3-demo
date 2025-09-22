using System;
using UnityEngine;

namespace Blocks.Data
{
    [CreateAssetMenu(fileName = "SkinSet", menuName = "Blocks/Skin Set")]
    public class SkinSet : ScriptableObject
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