using System;
using Sparse_Matrix.ISparseMatrix;

namespace Sparse_Matrix.SparseMatrix_CSR
{
    using stype = Int32;

    partial class SparseMatrixCSR
    {
        public class LUP_CSR : LUP
        {
            public SparseMatrix L { private set; get; }
            public SparseMatrix U { private set; get; }
            public stype[] P { private set; get; }

            public LUP_CSR(SparseMatrixCSR L, SparseMatrixCSR U, stype[] P)
            {
                this.L = L;
                this.U = U;
                this.P = P;
            }
        }
    }
}