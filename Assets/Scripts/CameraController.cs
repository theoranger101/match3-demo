using LevelManagement;
using LevelManagement.Data;
using UnityEngine;
using Utilities.DI;
using Utilities.Events;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public Camera Camera;

    [Inject] private GridGeometryConfig m_GeometryConfig;
    
    private void OnEnable()
    {
        GEM.Subscribe<LevelEvent>(HandleLevelStart, channel:(int)LevelEventType.StartLevel);
    }

    private void OnDisable()
    {
        GEM.Unsubscribe<LevelEvent>(HandleLevelStart, channel:(int)LevelEventType.StartLevel);
    }

    private void HandleLevelStart(LevelEvent evt)
    {
        FitCameraToLevelDefinition(evt.LevelDefinition);
    }
    
    private void FitCameraToLevelDefinition(LevelDefinition lvlDef)
    {
        var cols = lvlDef.LevelRules.Columns;
        var rows = lvlDef.LevelRules.Rows;

        var cellSizeX = m_GeometryConfig.CellSize.x;
        var cellSizeY = m_GeometryConfig.CellSize.y;
        
        CameraFitter.Fit(Camera, cols, rows, cellSizeX, cellSizeY);
        var center = new Vector3((cols - 1) * 0.5f * cellSizeX, (rows - 1) * 0.5f * cellSizeY, -10f);
        Camera.transform.position = center;
    }
}