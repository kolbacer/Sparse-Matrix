using System;
using System.IO;
using Sparse_Matrix.ISparseMatrix;
using Sparse_Matrix.SparseMatrix_CSR;

namespace Sparse_Matrix
{
    static class Globals
    {
        public static string logfilepath = @"C:\Users\pc\source\repos\NIR\test\log.txt";
        public static StreamWriter sw;

        public static string logLfilepath = @"C:\Users\pc\source\repos\NIR\test\logL.txt";
        public static StreamWriter swL;

        public static string logUfilepath = @"C:\Users\pc\source\repos\NIR\test\logU.txt";
        public static StreamWriter swU;
    }

    class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            Globals.sw = new StreamWriter(Globals.logfilepath, false);
            StreamWriter sw = Globals.sw;

            Globals.swL = new StreamWriter(Globals.logLfilepath, false);
            StreamWriter swL = Globals.swL;

            Globals.swU = new StreamWriter(Globals.logUfilepath, false);
            StreamWriter swU = Globals.swU;

            SparseMatrixCSR matrix = new SparseMatrixCSR(3, 10);

            matrix.AssignElement(1, 3, 1);
            matrix.AssignElement(1, 4, 3);
            matrix.AssignElement(1, 8, 5);
            matrix.AssignElement(3, 6, 7);
            matrix.AssignElement(3, 8, 1);
            matrix.Print();

            Console.WriteLine();
            matrix.PrintStorage();

            //

            string filename = @"C:\Users\pc\source\repos\NIR\test\text.txt";
            Console.WriteLine();
            SparseMatrix matrix1 = SparseMatrixCSR.ReadFromFile(filename);
            matrix1.Print();
            Console.WriteLine();
            matrix1.PrintStorage();

            //

            Console.WriteLine();
            matrix1.AddRows(2, 1);
            matrix1.AddRows(2, 3);
            matrix1.AddRows(1, 1, 2);
            matrix1.AddRows(3, 3, 2);
            matrix1.Print();
            Console.WriteLine();
            matrix1.PrintStorage();
            Console.WriteLine();

            matrix1.PrintTransposed();
            Console.WriteLine();
            matrix1.PrintStorageTransposed();

            SparseMatrixCSR.SwapRows(matrix1, 1, 3);
            matrix1.Print();
            Console.WriteLine();
            SparseMatrixCSR.SwapRows(matrix1, 2, 3);
            matrix1.Print();

            Console.WriteLine();
            SparseMatrix matrix2 = matrix1.Copy();
            matrix2.AddRows(1, 2);
            matrix2.Print();
            Console.WriteLine();
            matrix1.Print();

            //

            /*Console.WriteLine();
            string filename2 = @"C:\Users\pc\source\repos\NIR\test\text2.txt";
            SparseMatrix matrix3 = SparseMatrix.ReadFromFile(filename2);
            matrix3.Print();
            Console.WriteLine();
            Console.WriteLine("LUP: ");
            Console.WriteLine();
            Console.WriteLine("L:");
            SparseMatrix.LUP LUP = matrix3.LUPdecompose();
            LUP.L.Print();
            Console.WriteLine();
            Console.WriteLine("U:");
            LUP.U.Print();
            Console.WriteLine();
            Console.WriteLine("P:");
            for (int i = 1; i <= LUP.P.Length; ++i)
            {
                Console.Write(LUP.P[i - 1] + " ");
            }
            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("Ly=b");
            SparseMatrix.SparseVector b = new SparseMatrix.SparseVector(new vtype[] { -46, 53, -62 });
            Console.Write("b = ");
            b.Print();

            Console.WriteLine();
            Console.WriteLine("x = ");
            SparseMatrix.SparseVector x = SparseMatrix.SolveSLAE(LUP, b);
            x.Print();*/

            //

            //sw.WriteLine();
            string superfile = @"C:\Users\pc\source\repos\NIR\test\matrix_2.txt";
            SparseMatrix supermatrix = SparseMatrixCSR.ReadFromFile(superfile);
            //supermatrix.PrintToLog();
            //sw.WriteLine();
            //sw.WriteLine("LUP: ");
            //sw.WriteLine();
            //sw.WriteLine("L:");

            var startTime = System.Diagnostics.Stopwatch.StartNew();
            LUP LUP1 = supermatrix.LUPdecompose();
            startTime.Stop();
            var resultTime = startTime.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
                resultTime.Hours,
                resultTime.Minutes,
                resultTime.Seconds,
                resultTime.Milliseconds);
            Console.WriteLine("#######");
            Console.WriteLine("LUPdecompose time: " + resultTime);
            Console.WriteLine("LUPdecompose time: " + elapsedTime);
            Console.WriteLine("#######");

            //LUP1.L.PrintToLog();
            //sw.WriteLine();
            //sw.WriteLine("U:");
            //LUP1.U.PrintToLog();
            //sw.WriteLine();
            //sw.WriteLine("P:");
            /*for (int i = 1; i <= LUP1.P.Length; ++i)
            {
                sw.Write(LUP1.P[i - 1] + " ");
            }
            sw.WriteLine();*/


            sw.Close();

            swL.Close();
            swU.Close();

            Console.WriteLine("END");
        }
    }
}