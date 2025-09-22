using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks.Data
{
    public enum IconTier : byte
    {
        Default = 0,
        A = 1,
        B = 2,
        C = 3,
    }

    /*
    [Serializable]
    public struct SkinSet
    {
        public string Id; // editor
        public Sprite IconDefault;
        public Sprite IconA;
        public Sprite IconB;
        public Sprite IconC;

        public Sprite GetSprite(IconTier tier)
        {
            return tier switch
            {
                IconTier.A => IconA,
                IconTier.B => IconB,
                IconTier.C => IconC,
                _ => IconDefault
            };
        }
    }


    [CreateAssetMenu(fileName = "SkinSetCollection", menuName = "Levels/Skin Set Collection")]
    public sealed class SkinSetCollection : ScriptableObject
    {
        public List<SkinSet> Skins = new();

        private Dictionary<string, SkinSet> m_ById;

        private void OnEnable()
        {
            m_ById = new Dictionary<string, SkinSet>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < Skins.Count; i++)
            {
                var entry = Skins[i];
                if (!string.IsNullOrEmpty(entry.Id))
                {
                    m_ById.Add(entry.Id, entry);
                }
            }
        }

        public SkinSet GetSkinSet(int index)
        {
            if (index < 0 || index >= Skins.Count)
                throw new IndexOutOfRangeException("Group Id for SkinSet is out of range.");

            return Skins[index];
        }

        public Sprite Get(string id, IconTier tier)
        {
            if (!m_ById.TryGetValue(id, out var entry)) return null;
            return tier switch
            {
                IconTier.A => entry.IconA,
                IconTier.B => entry.IconB,
                IconTier.C => entry.IconC,
                _ => entry.IconDefault
            };
        }

        public Sprite Get(int index, IconTier tier)
        {
            if (index < 0 || index >= Skins.Count) return null;

            var entry = Skins[index];
            return tier switch
            {
                IconTier.A => entry.IconA,
                IconTier.B => entry.IconB,
                IconTier.C => entry.IconC,
                _ => entry.IconDefault
            };
        }
    }

        */
}