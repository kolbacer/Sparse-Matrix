using System;
using System.Collections.Generic;
using System.Text;

namespace Sparse_Matrix.ISparseMatrix
{
    using stype = Int32;
    using vtype = Double;

    interface SparseVector
    {
        public stype Length { get; }
        public stype NumberOfNonzero { get; }
        public bool isColumn { get; }

        public void AddLast(vtype x);
        public vtype[] ToFilled();
        public void Print();
    }
}
