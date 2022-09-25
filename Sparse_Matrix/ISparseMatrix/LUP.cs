using System;
using System.Collections.Generic;
using System.Text;

namespace Sparse_Matrix.ISparseMatrix
{
    using stype = Int32;
    using vtype = Double;

    interface LUP
    {
        public SparseMatrix L { get; }
        public SparseMatrix U { get; }
        public stype[] P { get; }
    }
}
