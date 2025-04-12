using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LineChart : VisualElement
{
    public new class UxmlFactory : UxmlFactory<LineChart, UxmlTraits> { }

    public List<float> data = new();
    public Color lineColor = Color.green;
    public float lineWidth = 2f;

    private VisualElement labelContainer;

    public LineChart()
    {
        generateVisualContent += OnGenerateVisualContent;
        style.flexGrow = 1;
        Initialize();
    }

    private void Initialize()
    {
        labelContainer = new VisualElement();
        labelContainer.pickingMode = PickingMode.Ignore;
        hierarchy.Add(labelContainer);
        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        if (data == null || data.Count < 2) return;

        var painter = ctx.painter2D;
        var rect = contentRect;
        float width = rect.width;
        float height = rect.height;
        float step = width / (data.Count - 1);
        float min = Mathf.Min(data.ToArray());
        float max = Mathf.Max(data.ToArray());
        float range = Mathf.Max(max - min, 0.0001f);

        // Background
        painter.fillColor = new Color(0.1f, 0.1f, 0.1f, 1);
        painter.BeginPath();
        painter.MoveTo(new Vector2(rect.x, rect.y));
        painter.LineTo(new Vector2(rect.xMax, rect.y));
        painter.LineTo(new Vector2(rect.xMax, rect.yMax));
        painter.LineTo(new Vector2(rect.x, rect.yMax));
        painter.ClosePath();
        painter.Fill();

        // Axes
        painter.strokeColor = Color.white;
        painter.lineWidth = 1;

        // X-axis
        painter.BeginPath();
        painter.MoveTo(new Vector2(rect.x, rect.yMax));
        painter.LineTo(new Vector2(rect.xMax, rect.yMax));
        painter.Stroke();

        // Y-axis
        painter.BeginPath();
        painter.MoveTo(new Vector2(rect.x, rect.y));
        painter.LineTo(new Vector2(rect.x, rect.yMax));
        painter.Stroke();

        // Y-axis scale lines (4 steps)
        int yTicks = 4;
        for (int i = 0; i <= yTicks; i++)
        {
            float t = (float)i / yTicks;
            float y = rect.yMax - t * height;
            float val = min + t * range;

            // Draw horizontal grid lines
            painter.strokeColor = new Color(1, 1, 1, 0.2f);
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.x, y));
            painter.LineTo(new Vector2(rect.xMax, y));
            painter.Stroke();
        }

        // Line chart
        painter.strokeColor = lineColor;
        painter.lineWidth = lineWidth;

        for (int i = 0; i < data.Count - 1; i++)
        {
            float x1 = rect.x + i * step;
            float y1 = rect.yMax - ((data[i] - min) / range * height);
            float x2 = rect.x + (i + 1) * step;
            float y2 = rect.yMax - ((data[i + 1] - min) / range * height);

            painter.BeginPath();
            painter.MoveTo(new Vector2(x1, y1));
            painter.LineTo(new Vector2(x2, y2));
            painter.Stroke();
        }
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        // Add the axis labels after geometry changes
        AddAxisLabels();
    }

    private void AddAxisLabels()
    {
        // Calculate positions and add labels only once the layout is ready
        var rect = contentRect;
        float width = rect.width;
        float height = rect.height;
        float step = width / (data.Count - 1);
        float min = Mathf.Min(data.ToArray());
        float range = Mathf.Max(Mathf.Max(data.ToArray()) - min, 0.0001f);

        // Y-axis labels (outside the chart area to the left)
        int yTicks = 4;
        for (int i = 0; i <= yTicks; i++)
        {
            float t = (float)i / yTicks;
            float y = rect.yMax - t * height;
            float val = min + t * range;

            var label = new Label(val.ToString("0.0"));
            label.style.position = Position.Absolute;
            label.style.left = rect.x - 40;
            label.style.top = y - 12;
            label.style.fontSize = 14;
            label.style.color = Color.white;
            labelContainer.Add(label);
        }

        // X-axis labels (outside the chart area below)
        for (int i = 0; i < data.Count; i += Mathf.Max(1, data.Count / 5))
        {
            float x = rect.x + i * step;

            var label = new Label(i.ToString());
            label.style.position = Position.Absolute;
            label.style.left = x - 12;
            label.style.top = rect.yMax + 8;
            label.style.fontSize = 14;
            label.style.color = Color.white;
            labelContainer.Add(label);
        }
    }
}
