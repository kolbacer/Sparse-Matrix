using System;
using System.Collections.Generic;
using System.Text;

namespace Sparse_Matrix.ISparseMatrix
{
    using stype = Int32;
    using vtype = Double;

    public interface SparseMatrix
    {
        public stype Rows { get; }
        public stype Columns { get; }
        public stype NumberOfNonzeroElements { get; }
        public vtype GetElement(stype i, stype j);
        public void AssignElement(stype i, stype j, vtype value);
        public LUP LUPdecompose();
        public void AddRows(stype augend, stype addend, vtype coef = 1);
        public void SwapRows(stype row1, stype row2);
        public SparseMatrix Copy();
        public static SparseVector MultiplyMatrixByVector(SparseMatrix _matrix, SparseVector _vector) => throw new NotImplementedException();
        public static SparseVector SolveSLAE(LUP LUP, SparseVector b) => throw new NotImplementedException();
        public static SparseVector SolveLy_b(SparseMatrix _L, SparseVector _b, stype[] P = null) => throw new NotImplementedException();
        public static SparseVector SolveUx_y(SparseMatrix _U, SparseVector _y) => throw new NotImplementedException();
        public void Print();
        public void PrintTransposed();
        public void PrintToLog();
        public void PrintStorage();
        public void PrintStorageTransposed();
        public static SparseMatrix ReadDenseFromFile(string filepath) => throw new NotImplementedException();
    }
}
