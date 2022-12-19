using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Data.Text;
using System.Reflection.PortableExecutable;
using OxyPlot.Series;
using OxyPlot;
using SparseMatrixAnalysis.Plots;

namespace SparseMatrixAnalysis.Tests
{
    static class NumericsTest
    {
        public static void Run(string filepath)
        {

            //object locker = new object();
            //lock (locker)
            //{
            Matrix<double> matrix = DelimitedReader.Read<double>(filepath, false, " ", true);

            Console.WriteLine("[Numerics] LUP decomposing...");
            var startTime = System.Diagnostics.Stopwatch.StartNew();
            var LU = matrix.LU();
            startTime.Stop();
            var resultTime = startTime.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                resultTime.Hours,
                resultTime.Minutes,
                resultTime.Seconds,
                resultTime.Milliseconds);
            Console.WriteLine("[Numerics] LUPdecompose time: " + elapsedTime);

            Results.s2.Items.Add(new BarItem { Value = resultTime.TotalMilliseconds / 1000 });
            //}

        }
    }
}
