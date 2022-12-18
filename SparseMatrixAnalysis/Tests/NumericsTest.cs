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

namespace SparseMatrixAnalysis.Tests
{
    static class NumericsTest
    {
        public static void Run(string filepath)
        {
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
            Console.WriteLine("#######");
            Console.WriteLine("LUPdecompose time: " + elapsedTime);
            Console.WriteLine("#######");

            object locker = new object();
            lock (locker)
            {
                Matrix<double> matrix1 = DelimitedReader.Read<double>(filepath, false, " ", true);

                Console.WriteLine("[Numerics] LUP decomposing in lock...");
                var startTime1 = System.Diagnostics.Stopwatch.StartNew();
                var LU1 = matrix1.LU();
                startTime1.Stop();
                var resultTime1 = startTime1.Elapsed;
                string elapsedTime1 = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                    resultTime1.Hours,
                    resultTime1.Minutes,
                    resultTime1.Seconds,
                    resultTime1.Milliseconds);
                Console.WriteLine("#######");
                Console.WriteLine("LUPdecompose time: " + elapsedTime1);
                Console.WriteLine("#######");
            }
        }
    }
}
