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
            painter.strokeColor = Color.yellow;
            painter.lineWidth = 2;

            foreach (var (start, end) in connections)
            {
                DrawCurvedLine(painter, start, end);
            }
        };
    }
    private void DrawCurvedLine(Painter2D painter, Vector2 start, Vector2 end)
    {
        // Control point for the curve, adjust the Y offset for more/less curvature
        Vector2 controlPoint = (start + end) / 2 + new Vector2(0, 50);

        // Use a simple quadratic Bezier curve, which requires drawing several points
        int segments = 20;  // Number of segments for the curve approximation
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;  // Parametric value from 0 to 1
            Vector2 point = CalculateBezierPoint(t, start, controlPoint, end);

            if (i == 0)
                painter.BeginPath();
            else
                painter.LineTo(point);

            if (i == segments)
                painter.Stroke();
        }
    }

    // Calculate a point on the quadratic Bezier curve at t using the formula:
    // P(t) = (1 - t)^2 * start + 2 * (1 - t) * t * controlPoint + t^2 * end
    private Vector2 CalculateBezierPoint(float t, Vector2 start, Vector2 controlPoint, Vector2 end)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector2 p = uu * start; // (1 - t)^2 * start
        p += 2 * u * t * controlPoint; // 2 * (1 - t) * t * controlPoint
        p += tt * end; // t^2 * end

        return p;
    }

    public void ClearLines()
    {
        connections.Clear();
    }

    public void AddConnection(Vector2 start, Vector2 end)
    {
        connections.Add((start, end));
    }

    public new class UxmlFactory : UxmlFactory<ConnectionLines, UxmlTraits> { }
}