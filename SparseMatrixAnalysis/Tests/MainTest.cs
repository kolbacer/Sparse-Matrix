using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Sparse_Matrix.ISparseMatrix;
using Sparse_Matrix.SparseMatrix_CSR;
using Sparse_Matrix;
using IOutils;
using SparseMatrixAnalysis.Plots;
using OxyPlot.Series;
using OxyPlot.Legends;
using OxyPlot.Axes;
using OxyPlot;

namespace SparseMatrixAnalysis.Tests
{
    static class MainTest
    {
        public static void Run(string filepath)
        {
            string directory = Path.GetDirectoryName(filepath);
            string logfilepath = directory + @"\log.txt";
            string logLfilepath = directory + @"\logL.txt";
            string logUfilepath = directory + @"\logU.txt";

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            Globals.sw = new StreamWriter(logfilepath, false);
            StreamWriter sw = Globals.sw;

            Globals.swL = new StreamWriter(logLfilepath, false);
            StreamWriter swL = Globals.swL;

            Globals.swU = new StreamWriter(logUfilepath, false);
            StreamWriter swU = Globals.swU;

            sw.WriteLine();
            Console.WriteLine("Reading from file...");
            SparseMatrix supermatrix = SparseMatrixCSR.ReadDenseFromFile(filepath);
            Console.WriteLine("Matrix was read!");
            supermatrix.PrintToLog();
            sw.WriteLine();
            sw.WriteLine("LUP: ");
            sw.WriteLine();
            sw.WriteLine("L:");

            Console.WriteLine("LUP decomposing...");
            var startTime = System.Diagnostics.Stopwatch.StartNew();
            LUP LUP1 = supermatrix.LUPdecompose();
            startTime.Stop();
            var resultTime = startTime.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                resultTime.Hours,
                resultTime.Minutes,
                resultTime.Seconds,
                resultTime.Milliseconds);
            Console.WriteLine("LUPdecompose time: " + elapsedTime);

            LUP1.L.PrintToLog();
            sw.WriteLine();
            sw.WriteLine("U:");
            LUP1.U.PrintToLog();
            sw.WriteLine();
            sw.WriteLine("P:");
            for (int i = 1; i <= LUP1.P.Length; ++i)
            {
                sw.Write(LUP1.P[i - 1] + " ");
            }
            sw.WriteLine();

            //

            sw.Close();
            swL.Close();
            swU.Close();


            FileStream Lfile = File.OpenRead(logLfilepath);
            Scanner LScanner = new Scanner(Lfile);

            FileStream Ufile = File.OpenRead(logUfilepath);
            Scanner UScanner = new Scanner(Ufile);

            LinkedList<int> Lnonzeros = new LinkedList<int>();
            LinkedList<int> Unonzeros = new LinkedList<int>();

            int? numL = LScanner.NextInt();
            int? numU = UScanner.NextInt();

            while ((numL != null) && (numU != null))
            {
                Lnonzeros.AddLast(numL.Value);
                Unonzeros.AddLast(numU.Value);

                numL = LScanner.NextInt();
                numU = UScanner.NextInt();
            }

            Lfile.Dispose();
            Ufile.Dispose();

            LocalFillingPlot plot = new LocalFillingPlot();
            LocalFillingPlotView localFillingPlotView = new LocalFillingPlotView();
            plot.DataContext = localFillingPlotView;

            int pointCount = Lnonzeros.Count;
            int[] xs = new int[pointCount];
            int[] ys1 = new int[pointCount];
            int[] ys2 = new int[pointCount];

            LinkedListNode<int> Lnode = Lnonzeros.First;
            LinkedListNode<int> Unode = Unonzeros.First;
            for (int i = 0; i < pointCount; i++)
            {
                xs[i] = i + 1;
                ys1[i] = Lnode.Value;
                ys2[i] = Unode.Value;

                Lnode = Lnode.Next;
                Unode = Unode.Next;
            }

            // create lines and fill them with data points
            var line1 = new LineSeries()
            {
                Title = $"Локальное заполнение в L",
                Color = OxyPlot.OxyColors.Blue,
                StrokeThickness = 1
            };

            var line2 = new LineSeries()
            {
                Title = $"Локальное заполнение в U",
                Color = OxyPlot.OxyColors.Red,
                StrokeThickness = 1
            };

            for (int i = 0; i < pointCount; i++)
            {
                line1.Points.Add(new OxyPlot.DataPoint(xs[i], ys1[i]));
                line2.Points.Add(new OxyPlot.DataPoint(xs[i], ys2[i]));
            }

            // create the model and add the lines to it
            var model = new OxyPlot.PlotModel
            {
                Title = $"Рост локального заполнения в матрицах L и U ({pointCount:N0} итераций)"
            };
            model.Series.Add(line1);
            model.Series.Add(line2);

            model.Legends.Add(new Legend
            {
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.TopRight,
            });

            model.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                Title = "Итерация"
            });
            model.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Left,
                Title = "Кол-во ненулевых элементов"
            });

            // load the model into the user control
            localFillingPlotView.LocalFillingModel = model;

            plot.Show();

            // !!!!!!!

            Results.s1.Items.Add(new BarItem { Value = resultTime.TotalSeconds + resultTime.TotalMilliseconds / 1000 });

            Console.WriteLine();
        }
    }
}
