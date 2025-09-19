using UnityEngine;

public static class CameraFitter
{
    public static void Fit(Camera cam, int cols, int rows, float cellSize, float margin = 0.5f)
    {
        float worldW = cols * cellSize + margin * 2f;
        float worldH = rows * cellSize + margin * 2f;

        float aspect = (float)Screen.width / Screen.height;
        float sizeByH = worldH * 0.5f; // vertical constraint
        float sizeByW = (worldW * 0.5f) / aspect; // horizontal constraint

        cam.orthographicSize = Mathf.Max(sizeByH, sizeByW);
    }
}
