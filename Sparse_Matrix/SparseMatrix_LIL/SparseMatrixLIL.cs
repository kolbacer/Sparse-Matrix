using Sparse_Matrix.ISparseMatrix;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sparse_Matrix.SparseMatrix_LIL
{
    using stype = Int32;
    using vtype = Double;

    partial class SparseMatrixLIL// : SparseMatrix
    {
        LinkedList<Element>[] RowList;

        private const stype offset = -1;                           // смещение для итерации по массиву
        private const double eps = Globals.eps;                    // что считаем нулем

        public stype Rows { get; } = 0;
        public stype Columns { get; private set; } = 0;
        public stype NumberOfNonzeroElements { get; } = 0;

        public SparseMatrixLIL(stype Rows, stype Columns)
        {
            this.Rows = Rows;
            this.Columns = Columns;

            this.RowList = new LinkedList<Element>[Rows];
            for (int i = 1; i <= Rows; i++)
            {
                Console.WriteLine(this.RowList[i + offset]);
            }
        }
    }
}
