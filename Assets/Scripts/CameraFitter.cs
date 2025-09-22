using UnityEngine;

public static class CameraFitter
{
    public static void Fit(Camera cam, int cols, int rows, float cellSizeX, float cellSizeY, float margin = 0.5f)
    {
        var worldW = cols * cellSizeX + margin * 2f;
        var worldH = rows * cellSizeY + margin * 2f;

        var aspect = (float)Screen.width / Screen.height;
        var sizeByH = worldH * 0.5f; // vertical constraint
        var sizeByW = (worldW * 0.5f) / aspect; // horizontal constraint

        cam.orthographicSize = Mathf.Max(sizeByH, sizeByW);
    }
}
