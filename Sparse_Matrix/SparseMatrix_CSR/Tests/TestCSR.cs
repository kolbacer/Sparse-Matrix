using System;
using System.IO;
using Sparse_Matrix.ISparseMatrix;

namespace Sparse_Matrix.SparseMatrix_CSR.Tests
{
    static class TestCSR
    {
        public static string logfilepath = @"..\..\..\..\test\log.txt";
        public static string logLfilepath = @"..\..\..\..\test\logL.txt";
        public static string logUfilepath = @"..\..\..\..\test\logU.txt";

        public static void Run()
        {
            Globals.sw = new StreamWriter(logfilepath, false);
            StreamWriter sw = Globals.sw;

            Globals.swL = new StreamWriter(logLfilepath, false);
            StreamWriter swL = Globals.swL;

            Globals.swU = new StreamWriter(logUfilepath, false);
            StreamWriter swU = Globals.swU;

            Console.WriteLine("START");

            //

            Console.WriteLine("\n########## Test 1 ##########\n");

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

            Console.WriteLine("\n########## Test 2 ##########\n");

            string filename = @"..\..\..\..\test\test_dense.txt";
            SparseMatrix matrix1 = SparseMatrixCSR.ReadDenseFromFile(filename);
            matrix1.Print();
            Console.WriteLine();
            matrix1.PrintStorage();

            //

            Console.WriteLine("\n########## Test 3 ##########\n");

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

            Console.WriteLine("\n########## Test 4 ##########\n");

            sw.WriteLine();
            string superfile = @"..\..\..\..\test\matrix_2_dense.txt";
            SparseMatrix supermatrix = SparseMatrixCSR.ReadDenseFromFile(superfile);
            supermatrix.PrintToLog();
            sw.WriteLine();
            sw.WriteLine("LUP: ");
            sw.WriteLine();
            sw.WriteLine("L:");

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
            Console.WriteLine("LUPdecompose time: " + elapsedTime);
            Console.WriteLine("#######");

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

            Console.WriteLine("\nEND");
        }
    }
}