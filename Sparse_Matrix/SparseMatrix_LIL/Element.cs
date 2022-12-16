﻿using System;

namespace Sparse_Matrix.SparseMatrix_LIL
{
    using stype = Int32;
    using vtype = Double;

    partial class SparseMatrixLIL
    {
        // Вспомогательные классы

        public class Element  // пара индекс_столбца - значение
        {
            public stype Index { get; set; }  // JA[i]
            public vtype ElemValue { get; set; }    // AN[i]

            public Element(stype Index, vtype ElemValue)
            {
                this.Index = Index;
                this.ElemValue = ElemValue;
            }
        }
    }
}