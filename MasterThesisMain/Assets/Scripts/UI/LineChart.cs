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

    public List<(List<float> values, Color color, string label)> datasets = new();

    private VisualElement labelContainer;
    private VisualElement legendContainer;
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

        legendContainer = new VisualElement();
        legendContainer.style.flexDirection = FlexDirection.Row;
        legendContainer.style.flexWrap = Wrap.Wrap;
        legendContainer.style.marginTop = 8;
        hierarchy.Add(legendContainer);



        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }
    public void Refresh()
    {
        labelContainer.Clear();
        MarkDirtyRepaint();
        AddAxisLabels();
        RefreshLegend();
    }
    void OnGenerateVisualContent(MeshGenerationContext ctx)
    {
        // pick series (fall back to single data if none in datasets)
        var seriesList = datasets.Count > 0
            ? datasets
            : new List<(List<float>, Color, string)> { (data, lineColor, "Default") };

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
        float paddedMin = globalMin - padding;
        float paddedMax = globalMax + padding;
        float paddedRange = Mathf.Max(paddedMax - paddedMin, 0.0001f);

        // --- background, axes & grid (same as before) ---
        painter.fillColor = new Color(0.1f, 0.1f, 0.1f, 1);
        painter.BeginPath();
        painter.MoveTo(rect.min);
        painter.LineTo(new Vector2(rect.xMax, rect.yMin));
        painter.LineTo(rect.max);
        painter.LineTo(new Vector2(rect.xMin, rect.yMax));
        painter.ClosePath();
        painter.Fill();

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

        //  plot each dataset with smoother curves & thicker lines 
        int smoothSteps = 8;                   // subdivisions per segment
        painter.lineWidth = lineWidth * 1.5f; // 50% thicker

        foreach (var (vals, col, _) in seriesList)
        {
            painter.strokeColor = col;

            // build list of points
            var pts = new List<Vector2>(pointCount);
            for (int i = 0; i < pointCount; i++)
            {
                float x = rect.xMin + i * step;
                float y = rect.yMax - ((vals[i] - paddedMin) / paddedRange) * h;
                pts.Add(new Vector2(x, y));
            }

            // spline‐interpolate each span
            for (int i = 0; i < pointCount - 1; i++)
            {
                // neighbors p0 & p3 for Catmull–Rom
                var p1 = pts[i];
                var p2 = pts[i + 1];
                var p0 = i > 0 ? pts[i - 1] : p1 + (p1 - p2);
                var p3 = i < pointCount - 2 ? pts[i + 2] : p2 + (p2 - p1);

                painter.BeginPath();
                painter.MoveTo(p1);

                for (int s = 1; s <= smoothSteps; s++)
                {
                    float t = s / (float)smoothSteps;
                    Vector2 q = CatmullRom(p0, p1, p2, p3, t);
                    painter.LineTo(q);
                }

                painter.Stroke();
            }
        }
    }

    // fixed‑tension Catmull–Rom 
    private Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        return 0.5f * ((2f * p1) +
                       (-p0 + p2) * t +
                       (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                       (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
    }
    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        AddAxisLabels();
    }

    private void AddAxisLabels()
    {
        labelContainer.Clear();

        // 1 same seriesList as used in drawing
        var seriesList = datasets.Count > 0
            ? datasets
            : new List<(List<float>, Color, string)> { (data, lineColor, "Default") };

        var values = seriesList.SelectMany(s => s.Item1).ToList();
        if (values.Count < 2)
            return;

        int pointCount = seriesList[0].Item1.Count;
        var rect = contentRect;
        float w = rect.width;
        float h = rect.height;
        float stepX = w / (pointCount - 1);

        // 2 global min/max
        float globalMin = values.Min();
        float globalMax = values.Max();
        float padding = (globalMax - globalMin) * 0.1f;
        float paddedMin = globalMin - padding;
        float paddedMax = globalMax + padding;
        float range = Mathf.Max(paddedMax - paddedMin, 0.0001f);

        // 3 Y‑axis labels
        int yTicks = 4;
        for (int i = 0; i <= yTicks; i++)
        {
            // Debug.Log($"Adding Y-axis label {i}");
            float t = i / (float)yTicks;
            float y = Mathf.Lerp(rect.yMax, rect.yMin, t);
            float val = Mathf.Lerp(paddedMin, paddedMax, t);

            var lbl = new Label(val.ToString("0.0"));
            lbl.style.position = Position.Absolute;
            lbl.style.left = rect.xMin - 40;
            lbl.style.top = y - 10;
            lbl.style.fontSize = 12;
            lbl.style.color = Color.white;
            labelContainer.Add(lbl);
        }

        // 4 X‑axis labels (5 ticks max)
        int maxXTicks = 5;
        int xStepCount = Mathf.Max(1, (pointCount - 1) / (maxXTicks - 1));
        for (int i = 0; i < pointCount; i += xStepCount)
        {
            // Debug.Log($"Adding X-axis label {i}");
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
    private void RefreshLegend()
    {
        legendContainer.Clear();

        var seriesList = datasets.Count > 0
            ? datasets
            : new List<(List<float>, Color, string)> { (data, lineColor, "Default") };

        foreach (var (_, color, label) in seriesList)
        {
            var item = new VisualElement();
            item.style.flexDirection = FlexDirection.Row;
            item.style.alignItems = Align.Center;
            item.style.marginRight = 8;
            item.style.marginLeft = 8;
            item.style.marginBottom = 24;

            var colorBox = new VisualElement();
            colorBox.style.width = 12;
            colorBox.style.height = 12;
            colorBox.style.marginRight = 4;
            colorBox.style.backgroundColor = color;

            var text = new Label(label);
            text.style.color = Color.white;
            text.style.fontSize = 12;

            item.Add(colorBox);
            item.Add(text);
            legendContainer.Add(item);
        }
    }
}
