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

        // compute global min/max and add 10% headroom above max
        float globalMin = seriesList.SelectMany(s => s.Item1).Min();
        float globalMax = seriesList.SelectMany(s => s.Item1).Max();
        float padding = (globalMax - globalMin) * 0.1f;
        float paddedMax = globalMax + padding;
        float paddedRange = Mathf.Max(paddedMax - globalMin, 0.0001f);

        // --- background ---
        painter.strokeColor = Color.white;
        painter.lineWidth = 1;
        painter.BeginPath();
        painter.MoveTo(new Vector2(rect.xMin, rect.yMin));         // top‑left
        painter.LineTo(new Vector2(rect.xMax, rect.yMin));         // top‑right
        painter.LineTo(new Vector2(rect.xMax, rect.yMax));         // bottom‑right
        painter.LineTo(new Vector2(rect.xMin, rect.yMax));         // bottom‑left
        painter.ClosePath();
        painter.Stroke();

        // --- axes & grid ---
        painter.strokeColor = Color.white;
        painter.lineWidth = 1;

        // X axis
        painter.BeginPath();
        painter.MoveTo(new Vector2(rect.xMin, rect.yMax));
        painter.LineTo(new Vector2(rect.xMax, rect.yMax));
        painter.Stroke();

        // Y axis
        painter.BeginPath();
        painter.MoveTo(new Vector2(rect.xMin, rect.yMin));
        painter.LineTo(new Vector2(rect.xMin, rect.yMax));
        painter.Stroke();

        // horizontal grid lines
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

        // --- plot each dataset with padded range ---
        foreach (var (vals, col) in seriesList)
        {
            painter.strokeColor = col;
            painter.lineWidth = lineWidth;

            for (int i = 0; i < pointCount - 1; i++)
            {
                float x1 = rect.xMin + i * step;
                float y1 = rect.yMax - ((vals[i] - globalMin) / paddedRange) * h;
                float x2 = rect.xMin + (i + 1) * step;
                float y2 = rect.yMax - ((vals[i + 1] - globalMin) / paddedRange) * h;

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
        labelContainer.Clear();

        // 1) same seriesList as used in drawing
        var seriesList = datasets.Count > 0
            ? datasets
            : new List<Tuple<List<float>, Color>> { Tuple.Create(data, lineColor) };

        var values = seriesList.SelectMany(s => s.Item1).ToList();
        if (values.Count < 2)
            return;

        int pointCount = seriesList[0].Item1.Count;
        var rect = contentRect;
        float w = rect.width;
        float h = rect.height;
        float stepX = w / (pointCount - 1);

        // 2) global min/max
        float globalMin = values.Min();
        float globalMax = values.Max();
        float range = Mathf.Max(globalMax - globalMin, 0.0001f);

        // 3) Y‑axis labels
        int yTicks = 4;
        for (int i = 0; i <= yTicks; i++)
        {
            float t = i / (float)yTicks;
            float y = Mathf.Lerp(rect.yMax, rect.yMin, t);
            float val = Mathf.Lerp(globalMin, globalMax, t);

            var lbl = new Label(val.ToString("0.0"));
            lbl.style.position = Position.Absolute;
            lbl.style.left = rect.xMin - 40;
            lbl.style.top = y - 10;
            lbl.style.fontSize = 12;
            lbl.style.color = Color.white;
            labelContainer.Add(lbl);
        }

        // 4) X‑axis labels (5 ticks max)
        int maxXTicks = 5;
        int xStepCount = Mathf.Max(1, (pointCount - 1) / (maxXTicks - 1));
        for (int i = 0; i < pointCount; i += xStepCount)
        {
            float x = rect.xMin + i * stepX;

            var lbl = new Label(i.ToString());
            lbl.style.position = Position.Absolute;
            lbl.style.left = x - 8;
            lbl.style.top = rect.yMax + 4;
            lbl.style.fontSize = 12;
            lbl.style.color = Color.white;
            labelContainer.Add(lbl);
        }
    }
}
