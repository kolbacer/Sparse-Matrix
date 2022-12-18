using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OxyPlot;
using OxyPlot.Series;

namespace SparseMatrixAnalysis.Plots
{
    public class PerfomanceComparisonBarView
    {
        public PerfomanceComparisonBarView()
        {
            this.PerfomanceComparisonModel = new PlotModel { Title = "Сравнение производительности" };
        }

        public PlotModel PerfomanceComparisonModel { get; set; }
    }
}
