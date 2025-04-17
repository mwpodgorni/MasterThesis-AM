using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class ConnectionLines : VisualElement
{
    private List<(Vector2, Vector2)> connections = new List<(Vector2, Vector2)>();

    public ConnectionLines()
    {
        generateVisualContent += ctx =>
        {
            var painter = ctx.painter2D;
            painter.strokeColor = new Color32(156, 70, 59, 200);
            painter.lineWidth = 2;

            foreach (var (start, end) in connections)
            {
                DrawCurvedLine(painter, start, end);
            }
        };
    }
    private void DrawCurvedLine(Painter2D painter, Vector2 start, Vector2 end)
    {
        Vector2 mid = (start + end) / 2;
        Vector2 dir = (end - start).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x);

        Vector2 controlPoint1 = mid + normal * 50f;
        Vector2 controlPoint2 = mid - normal * 50f;

        int segments = 20;
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector2 point = CalculateCubicBezierPoint(t, start, controlPoint1, controlPoint2, end);

            if (i == 0)
                painter.BeginPath();
            else
                painter.LineTo(point);

            if (i == segments)
                painter.Stroke();
        }
    }

    private Vector2 CalculateCubicBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector2 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;

        return p;
    }
    public void ClearLines()
    {
        connections.Clear();
        MarkDirtyRepaint();
    }

    public void AddConnection(Vector2 start, Vector2 end)
    {
        connections.Add((start, end));
    }

    public new class UxmlFactory : UxmlFactory<ConnectionLines, UxmlTraits> { }
}