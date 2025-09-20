using System;
using System.Collections.Generic;
using Blocks;
using LevelManagement.Data;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace LevelManagement.Editor
{
    [CustomEditor(typeof(LevelDefinition))]
    public class LevelDefinitionEditor : UnityEditor.Editor
    {
        private enum Brush
        {
            None,
            Match,
            Obstacle,
            PowerUp,
            Erase,
        }

        private static readonly Color[] s_PaletteColors =
        {
            Color.blue,
            Color.green,
            Color.red,
            Color.yellow,
        };

        private static readonly Color[] s_ObstacleColors =
        {
            new Color(0.9f, 0.2f, 0.2f),
            new Color(0.7f, 0.4f, 0.1f),
        };

        private static readonly Color[] s_PowerUpColors =
        {
            new Color(0.1f, 0.8f, 0.9f),
            new Color(0.9f, 0f, 0.9f),
            new Color(0.9f, 0.9f, 0.9f),
        };

        private static readonly Color s_EmptyColor = new Color(0.2f, 0.2f, 0.2f, 0.2f);

        private LevelDefinition m_Level;
        private SerializedProperty m_GridSizeProp, m_CellsProp;
        private SerializedProperty m_LevelRulesProp, m_GameThemeProp;
        private SerializedProperty m_MoveCountProp;

        private Brush m_BrushState = Brush.None;

        // Temporary Cell Datas
        private int m_MatchGroupId = -1;
        private bool m_AutoIncrementGroupId = false;
        private ObstacleType m_ObstacleType;
        private PowerUpType m_PowerUpType;

        private const int CellPx = 26;
        private const int Pad = 2;

        private void OnEnable()
        {
            m_Level = (LevelDefinition)target;
            m_GridSizeProp = serializedObject.FindProperty("GridSize");
            m_CellsProp = serializedObject.FindProperty("Cells");
            
            m_LevelRulesProp = serializedObject.FindProperty("LevelRules");
            m_GameThemeProp = serializedObject.FindProperty("GameTheme");
            
            m_MoveCountProp = serializedObject.FindProperty("MoveCount");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header
            EditorGUILayout.LabelField("Level", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(m_GridSizeProp);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(m_LevelRulesProp);
            EditorGUILayout.PropertyField(m_GameThemeProp);

            EditorGUILayout.Space(4);

            // Painter toolbar
            EditorGUILayout.LabelField("Paint", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            m_BrushState = (Brush)GUILayout.Toolbar((int)m_BrushState, Enum.GetNames(typeof(Brush)));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            switch (m_BrushState)
            {
                case Brush.None:
                case Brush.Erase:
                    break;
                case Brush.Match:
                    m_MatchGroupId = EditorGUILayout.IntField("Group ID", m_MatchGroupId);
                    m_AutoIncrementGroupId =
                        EditorGUILayout.ToggleLeft("Auto-increment group on paint", m_AutoIncrementGroupId);
                    break;
                case Brush.Obstacle:
                    m_ObstacleType = (ObstacleType)EditorGUILayout.EnumPopup("Obstacle Type", m_ObstacleType);
                    break;
                case Brush.PowerUp:
                    m_PowerUpType = (PowerUpType)EditorGUILayout.EnumPopup("Power Up Type", m_PowerUpType);
                    break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);

            DrawGrid();

            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear All Cells"))
            {
                if (EditorUtility.DisplayDialog("Clear Cells?", "Remove all placed cells?", "Yes", "No"))
                {
                    m_CellsProp.ClearArray();
                }
            }

            if (GUILayout.Button("Validate"))
            {
                ValidateAndLog();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);
            
            EditorGUILayout.PropertyField(m_MoveCountProp);
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGrid()
        {
            var width = m_GridSizeProp.vector2IntValue.x;
            var height = m_GridSizeProp.vector2IntValue.y;

            var middleLine = GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true));

            var rect = GUILayoutUtility.GetRect(width * (CellPx + Pad), height * (CellPx + Pad),
                GUILayout.ExpandWidth(false));
            rect.x = middleLine.x + (middleLine.width - rect.width) * 0.5f;
            var topLeft = new Vector2(rect.x + Pad, rect.y + Pad);

            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            GUILayout.FlexibleSpace();

            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, rect.height),
                new Color(0.12f, 0.12f, 0.12f, 1f));

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            var placed = new Dictionary<Vector2Int, CellDefinition>();
            for (var i = 0; i < m_CellsProp.arraySize; i++)
            {
                var prop = m_CellsProp.GetArrayElementAtIndex(i);
                var cellDef = ReadCell(prop);

                placed.Add(cellDef.Position, cellDef);
            }

            for (var y = height - 1; y >= 0; y--)
            {
                for (var x = 0; x < width; x++)
                {
                    var cellRect = new Rect(topLeft.x + x * (CellPx + Pad),
                        topLeft.y + (height - y - 1) * (CellPx + Pad),
                        CellPx, CellPx);

                    var gridPosition = new Vector2Int(x, y);
                    
                    var inDict = placed.TryGetValue(gridPosition, out var cellDef);
                    var label = "";

                    var color = s_EmptyColor;

                    if (inDict)
                    {
                        switch (cellDef.CellType)
                        {
                            case CellType.MatchBlock:
                                var groupId = cellDef.MatchGroupId;
                                color = s_PaletteColors[groupId % s_PaletteColors.Length];
                                label = $"M{groupId}";
                                break;
                            case CellType.ObstacleBlock:
                                var obstacleType = (int)cellDef.ObstacleType;
                                color = s_ObstacleColors[obstacleType % s_ObstacleColors.Length];
                                label = obstacleType switch
                                {
                                    (int)ObstacleType.Balloon => $"O-1",
                                    (int)ObstacleType.WoodenBox => $"O-2",
                                    _ => throw new ArgumentOutOfRangeException()
                                };
                                break;
                            case CellType.PowerUpBlock:
                                var powerUpType = (int)cellDef.PowerUpType;
                                color = s_PowerUpColors[powerUpType % s_PowerUpColors.Length];
                                label = powerUpType switch
                                {
                                    (int)PowerUpType.Rocket => $"P-1",
                                    (int)PowerUpType.Bomb => $"P-2",
                                    (int)PowerUpType.DiscoBall => $"P-3",
                                    _ => throw new ArgumentOutOfRangeException()
                                };
                                break;
                        }
                    }

                    EditorGUI.DrawRect(cellRect, color);

                    var lbl = new GUIStyle(EditorStyles.miniBoldLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal = { textColor = color },
                    };
                    EditorGUI.LabelField(cellRect, label, lbl);

                    if (Event.current.type == EventType.MouseDown && cellRect.Contains(Event.current.mousePosition))
                    {
                        OnPaintCell(gridPosition, cellDef);
                        Event.current.Use();
                    }

                    Handles.color = new Color(0f, 0f, 0f, 0.6f);
                    Handles.DrawAAPolyLine(1.5f,
                        new Vector3(rect.xMin, rect.yMin),
                        new Vector3(rect.xMax, rect.yMin),
                        new Vector3(rect.xMax, rect.yMax),
                        new Vector3(rect.xMin, rect.yMax),
                        new Vector3(rect.xMin, rect.yMin));
                }
            }
        }

        private void OnPaintCell(Vector2Int position, CellDefinition existingCell)
        {
            CellDefinition cd;
            switch (m_BrushState)
            {
                case Brush.None:
                    break;
                case Brush.Match:
                    if (m_MatchGroupId < 0)
                    {
                        EditorUtility.DisplayDialog("Match Group ID is not valid!",
                            "Match Group ID is not valid, please fix and try again!", "OK");
                        return;
                    }

                    cd = new CellDefinition()
                    {
                        CellType = CellType.MatchBlock,
                        MatchGroupId = m_MatchGroupId,
                        Position = position,
                    };
                    UpsertCell(in cd);

                    if (m_AutoIncrementGroupId)
                    {
                        m_MatchGroupId++;
                    }

                    MarkDirty();
                    break;
                case Brush.Obstacle:
                    cd = new CellDefinition()
                    {
                        CellType = CellType.ObstacleBlock,
                        ObstacleType = m_ObstacleType,
                        Position = position,
                    };
                    UpsertCell(cd);

                    MarkDirty();
                    break;
                case Brush.PowerUp:
                    cd = new CellDefinition()
                    {
                        CellType = CellType.PowerUpBlock,
                        PowerUpType = m_PowerUpType,
                        Position = position,
                    };
                    UpsertCell(cd);

                    MarkDirty();
                    break;
                case Brush.Erase:
                    RemoveCellAt(existingCell.Position);
                    MarkDirty();
                    break;
            }
        }

        #region Cell List Utilities

        private void UpsertCell(in CellDefinition cellDef)
        {
            var index = FindCellIndex(cellDef.Position);

            if (index >= 0) // edit existing cell
            {
                var el = m_CellsProp.GetArrayElementAtIndex(index);
                WriteCell(el, cellDef);
            }
            else // add a new element
            {
                m_CellsProp.InsertArrayElementAtIndex(m_CellsProp.arraySize);
                WriteCell(m_CellsProp.GetArrayElementAtIndex(m_CellsProp.arraySize - 1), cellDef);
            }
        }

        private void RemoveCellAt(Vector2Int position)
        {
            var index = FindCellIndex(position);
            if (index < 0)
            {
                return;
            }

            m_CellsProp.DeleteArrayElementAtIndex(index);
        }

        private int FindCellIndex(Vector2Int position)
        {
            for (var i = 0; i < m_CellsProp.arraySize; i++)
            {
                var prop = m_CellsProp.GetArrayElementAtIndex(i);
                var propPos = prop.FindPropertyRelative("Position").vector2IntValue;
                if (propPos == position)
                {
                    return i;
                }
            }

            return -1;
        }

        private static CellDefinition ReadCell(SerializedProperty prop) => new CellDefinition()
        {
            Position = prop.FindPropertyRelative("Position").vector2IntValue,
            CellType = (CellType)prop.FindPropertyRelative("CellType").enumValueIndex,
            
            MatchGroupId = prop.FindPropertyRelative("MatchGroupId").intValue,
            PowerUpType = (PowerUpType)prop.FindPropertyRelative("PowerUpType").enumValueIndex,
            ObstacleType = (ObstacleType)prop.FindPropertyRelative("ObstacleType").enumValueIndex,
        };

        private static void WriteCell(SerializedProperty prop, in CellDefinition cellDef)
        {
            prop.FindPropertyRelative("Position").vector2IntValue = cellDef.Position;
            prop.FindPropertyRelative("CellType").enumValueIndex = (int)cellDef.CellType;

            prop.FindPropertyRelative("MatchGroupId").intValue = cellDef.MatchGroupId;
            prop.FindPropertyRelative("PowerUpType").enumValueIndex = (int)cellDef.PowerUpType;
            prop.FindPropertyRelative("ObstacleType").enumValueIndex = (int)cellDef.ObstacleType;
        }

        #endregion

        private void MarkDirty()
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(m_Level);
        }

        private void ValidateAndLog()
        {
            var gridSize = m_GridSizeProp.vector2IntValue;
            var seen = new HashSet<Vector2Int>();

            for (var i = 0; i < m_CellsProp.arraySize; i++)
            {
                var cell = m_CellsProp.GetArrayElementAtIndex(i);
                var cellDef = ReadCell(cell);

                var cellPos = cellDef.Position;
                if (cellPos.x < 0 || cellPos.y < 0 || cellPos.x >= gridSize.x || cellPos.y >= gridSize.y)
                {
                    Debug.LogWarning($"Cell position out of range: {cellPos.x}, {cellPos.y} at index {i}");
                }
                if (!seen.Add(cellPos))
                {
                    Debug.LogWarning($"Duplicate cell position: {cellPos.x}, {cellPos.y} at index {i}");
                }
            }
        }
    }
}

#endif