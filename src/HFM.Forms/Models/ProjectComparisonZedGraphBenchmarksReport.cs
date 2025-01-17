﻿using System.Globalization;

using HFM.Core.Data;
using HFM.Core.WorkUnits;
using HFM.Forms.Internal;
using HFM.Preferences;

using ZedGraph;

namespace HFM.Forms.Models;

public class ProjectComparisonZedGraphBenchmarksReport : ZedGraphBenchmarksReport
{
    public const string KeyName = "Project Comparison";

    public IPreferences Preferences { get; }

    public ProjectComparisonZedGraphBenchmarksReport(IPreferences preferences, IProteinService proteinService, IProteinBenchmarkRepository benchmarks)
        : base(KeyName, proteinService, benchmarks)
    {
        Preferences = preferences ?? new InMemoryPreferencesProvider();
    }

    public override async Task Generate(IBenchmarksReportSource source)
    {
        var slotIdentifier = source.SlotIdentifier;
        var projects = source.Projects;
        var colors = source.Colors;

        if (slotIdentifier is null || projects.Count == 0)
        {
            Result = null;
            return;
        }

        var benchmarks = (await Benchmarks.GetBenchmarksAsync(slotIdentifier.Value, projects).ConfigureAwait(true))
            .OrderBy(x => x.SlotIdentifier.Name)
            .ThenBy(x => x.BenchmarkIdentifier.Threads)
            .ToList();

        if (benchmarks.Count == 0)
        {
            Result = null;
            return;
        }

        var projectToXAxisOrdinal = BuildProjectToXAxisOrdinal(benchmarks);

        var zg = CreateZedGraphControl();
        try
        {
            GraphPane pane = zg.GraphPane;

            int i = 0;
            var ppd = new List<double>();

            var slotPoints = benchmarks
                .GroupBy(x => (x.SlotIdentifier, x.BenchmarkIdentifier.Processor, x.BenchmarkIdentifier.Threads))
                .Select(g => BuildSlotPoints(g.OrderBy(x => x.BenchmarkIdentifier.ProjectID), projectToXAxisOrdinal))
                .OrderByDescending(x => x.Points.Max(y => y.Y))
                .Take(colors.Count);

            foreach (var x in slotPoints)
            {
                (PointPairList points, string label) = x;

                if (points.Count > 0)
                {
                    // collection PPD values from all points
                    ppd.AddRange(points.Select(p => p.Y));

                    Color color = GetNextColor(i++, colors);
                    AddSlotCurve(pane, label, points, color);
                }
            }

            if (ppd.Count > 0)
            {
                var averagePPD = ppd.Average();
                var averagePoints = BuildAveragePoints(averagePPD, projectToXAxisOrdinal);
                AddAverageCurve(pane, averagePoints);
            }

            ConfigureXAxis(pane.XAxis, projectToXAxisOrdinal);
            ConfigureYAxis(pane.YAxis, ppd.MaxOrDefault());

            FillGraphPane(pane);
        }
        finally
        {
            zg.AxisChange();
        }

        Result = zg;
    }

    private static Dictionary<int, double> BuildProjectToXAxisOrdinal(IEnumerable<ProteinBenchmark> benchmarks)
    {
        double ordinal = 1.0;
        var projectToXAxisOrdinal = new Dictionary<int, double>();
        foreach (int projectID in benchmarks.Select(x => x.BenchmarkIdentifier.ProjectID).OrderBy(x => x).Distinct())
        {
            projectToXAxisOrdinal.Add(projectID, ordinal++);
        }
        return projectToXAxisOrdinal;
    }

    private (PointPairList Points, string Label) BuildSlotPoints(IEnumerable<ProteinBenchmark> benchmarks, Dictionary<int, double> projectToXAxisOrdinal)
    {
        bool calculateBonus = Preferences.Get<BonusCalculation>(Preference.BonusCalculation) != BonusCalculation.None;

        var points = new PointPairList();
        string label = null;

        foreach (var b in benchmarks)
        {
            var protein = ProteinService.Get(b.BenchmarkIdentifier.ProjectID);
            if (protein is null)
            {
                continue;
            }

            if (label is null)
            {
                label = GetSlotNameAndProcessor(b, protein);
            }

            double y = GetPPD(protein, b.AverageFrameTime, calculateBonus);
            points.Add(projectToXAxisOrdinal[b.BenchmarkIdentifier.ProjectID], y);
        }

        return (points, label);
    }

    private static void AddSlotCurve(GraphPane pane, string label, PointPairList points, Color color)
    {
        var lineItem = pane.AddCurve(label, points, color, SymbolType.Circle);
        lineItem.Symbol.Fill = new Fill(color);
        lineItem.IsOverrideOrdinal = true;
    }

    private static PointPairList BuildAveragePoints(double averagePPD, Dictionary<int, double> projectToXAxisOrdinal)
    {
        var points = new PointPairList();
        foreach (var x in projectToXAxisOrdinal.Values)
        {
            points.Add(x, averagePPD);
        }
        return points;
    }

    private static void AddAverageCurve(GraphPane pane, PointPairList points)
    {
        var averageLineItem = pane.AddCurve("Average PPD", points, Color.Black, SymbolType.Circle);
        averageLineItem.Symbol.Fill = new Fill(Color.Black);
        averageLineItem.IsOverrideOrdinal = true;
    }

    private static void ConfigureXAxis(XAxis xAxis, Dictionary<int, double> projectToXAxisOrdinal)
    {
        xAxis.Title.Text = "Project Number";
        xAxis.Scale.TextLabels = projectToXAxisOrdinal.Keys.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray();
        xAxis.Type = AxisType.Text;
    }

    private static void ConfigureYAxis(YAxis yAxis, double yMaximum)
    {
        yAxis.Title.Text = "PPD";

        // Don't show YAxis.Scale as 10^3         
        yAxis.Scale.MagAuto = false;
        SetYAxisScale(yAxis, yMaximum);
    }
}
