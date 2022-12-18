using OxyPlot.Legends;
using OxyPlot;
using SparseMatrixAnalysis.Plots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace SparseMatrixAnalysis.Tests
{
    static class Results
    {
        public static PerfomanceComparisonBar barplot;
        public static PerfomanceComparisonBarView perfomanceComparisonBarView;
        public static int runs = 0;
        public static bool isShown = false;
        public static BarSeries s1;
        public static BarSeries s2;
        public static CategoryAxis categoryAxis;

        public static void Initialize()
        {
            barplot = new PerfomanceComparisonBar();
            perfomanceComparisonBarView = new PerfomanceComparisonBarView();

            barplot.DataContext = perfomanceComparisonBarView;

            s1 = new BarSeries { Title = "Мое решение", StrokeColor = OxyColors.Black, StrokeThickness = 1 };
            s2 = new BarSeries { Title = "Math.NET Numerics", StrokeColor = OxyColors.Black, StrokeThickness = 1 };

            s1.LabelFormatString = "{0:0.00} сек";
            s2.LabelFormatString = "{0:0.00} сек";

            perfomanceComparisonBarView.PerfomanceComparisonModel.Series.Add(s1);
            perfomanceComparisonBarView.PerfomanceComparisonModel.Series.Add(s2);

            var valueAxis = new LinearAxis { Position = AxisPosition.Bottom, MinimumPadding = 0, MaximumPadding = 0.06, AbsoluteMinimum = 0 };
            perfomanceComparisonBarView.PerfomanceComparisonModel.Axes.Add(valueAxis);

            categoryAxis = new CategoryAxis { Position = AxisPosition.Left };
            perfomanceComparisonBarView.PerfomanceComparisonModel.Axes.Add(categoryAxis);

            perfomanceComparisonBarView.PerfomanceComparisonModel.Legends.Add(new Legend
            {
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.TopRight,
            });
        }
    }
}
