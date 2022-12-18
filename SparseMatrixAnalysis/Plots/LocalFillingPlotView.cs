using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OxyPlot;
using OxyPlot.Series;

namespace SparseMatrixAnalysis.Plots
{
    public class LocalFillingPlotView
    {
        public LocalFillingPlotView()
        {
            this.LocalFillingModel = new PlotModel { Title = "Рост локального заполнения" };
        }

        public PlotModel LocalFillingModel { get; set; }
    }
}
