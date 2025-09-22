using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blocks.Data
{
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

        public Sprite GetSkin(int setId, int slotIndex)
        {
            if (m_ById != null && m_ById.TryGetValue(setId, out var set))
            {
                return set.Get(slotIndex);
            }
            
            return null;
        }
    }
}