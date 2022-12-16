using System;
using System.IO;
using Sparse_Matrix.ISparseMatrix;
using Sparse_Matrix.SparseMatrix_CSR;
using Sparse_Matrix.SparseMatrix_CSR.Tests;
using IOutils;

namespace Sparse_Matrix
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            TestCSR.Run();
        }
    }
}