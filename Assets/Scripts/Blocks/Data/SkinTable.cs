using System.Collections.Generic;
using UnityEngine;

namespace Blocks.Data
{
    /// <summary>
    /// A table of <see cref="SkinSet"/>s, indexed either by list position (0..N-1)
    /// or by explicit <see cref="SkinSet.SkinId"/> via a lookup dictionary.
    /// </summary>
    [CreateAssetMenu(fileName = "SkinTable", menuName = "Blocks/Skin Table")]
    public sealed class SkinTable : ScriptableObject
    {
        public int TableId;
        
        public List<SkinSet> SkinSets = new();

        private Dictionary<int, SkinSet> m_ById;

        private void OnEnable()
        {
            m_ById = new Dictionary<int, SkinSet>();
            for (var i = 0; i < SkinSets.Count; i++)
            {
                var set = SkinSets[i];
                if (set != null)
                {
                    m_ById.Add(set.SkinId, set);
                }
            }
        }

        public Sprite GetSkinById(int setId, int slotIndex)
        {
            if (m_ById != null && m_ById.TryGetValue(setId, out var set))
            {
                return set.Get(slotIndex);
            }
            
            return null;
        }

        public Sprite GetSkin(int setId, int slotIndex)
        {
            if (setId < 0 || setId >= SkinSets.Count)
            {
                return null;
            }
            
            return SkinSets[setId].Get(slotIndex);
        }
    }
}