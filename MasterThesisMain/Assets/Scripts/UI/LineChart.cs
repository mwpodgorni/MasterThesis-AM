using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
public class LineChart : VisualElement
{
    public new class UxmlFactory : UxmlFactory<LineChart, UxmlTraits> { }

    public List<float> data = new();
    public Color lineColor = Color.green;
    public float lineWidth = 2f;

    public List<Tuple<List<float>, Color>> datasets = new();

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
        // pick series (fall back to single data if none in datasets)
        var seriesList = datasets.Count > 0
            ? datasets
            : new List<Tuple<List<float>, Color>> { Tuple.Create(data, lineColor) };

        int pointCount = seriesList[0].Item1.Count;
        if (pointCount < 2)
            return;

        var painter = ctx.painter2D;
        var rect = contentRect;
        float w = rect.width;
        float h = rect.height;
        float step = w / (pointCount - 1);

        // compute global min/max
        float globalMin = seriesList.SelectMany(s => s.Item1).Min();
        float globalMax = seriesList.SelectMany(s => s.Item1).Max();
        float range = Mathf.Max(globalMax - globalMin, 0.0001f);

        // --- background ---
        painter.fillColor = new Color(0.1f, 0.1f, 0.1f, 1);
        painter.BeginPath();
        painter.MoveTo(rect.min);
        painter.LineTo(new Vector2(rect.xMax, rect.yMin));
        painter.LineTo(rect.max);
        painter.LineTo(new Vector2(rect.xMin, rect.yMax));
        painter.ClosePath();
        painter.Fill();

        // --- axes & grid ---
        painter.strokeColor = Color.white;
        painter.lineWidth = 1;

        // X
        painter.BeginPath();
        painter.MoveTo(new Vector2(rect.xMin, rect.yMax));
        painter.LineTo(new Vector2(rect.xMax, rect.yMax));
        painter.Stroke();

        // Y
        painter.BeginPath();
        painter.MoveTo(new Vector2(rect.xMin, rect.yMin));
        painter.LineTo(new Vector2(rect.xMin, rect.yMax));
        painter.Stroke();

        // horizontal grid
        painter.strokeColor = new Color(1, 1, 1, 0.2f);
        for (int i = 0; i <= 4; i++)
        {
            float t = i / 4f;
            float y = Mathf.Lerp(rect.yMax, rect.yMin, t);
            painter.BeginPath();
            painter.MoveTo(new Vector2(rect.xMin, y));
            painter.LineTo(new Vector2(rect.xMax, y));
            painter.Stroke();
        }

        // --- plot each dataset ---
        foreach (var (vals, col) in seriesList)
        {
            painter.strokeColor = col;
            painter.lineWidth = lineWidth;

            for (int i = 0; i < pointCount - 1; i++)
            {
                float x1 = rect.xMin + i * step;
                float y1 = rect.yMax - ((vals[i] - globalMin) / range) * h;
                float x2 = rect.xMin + (i + 1) * step;
                float y2 = rect.yMax - ((vals[i + 1] - globalMin) / range) * h;

                painter.BeginPath();
                painter.MoveTo(new Vector2(x1, y1));
                painter.LineTo(new Vector2(x2, y2));
                painter.Stroke();
            }
        }
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
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
