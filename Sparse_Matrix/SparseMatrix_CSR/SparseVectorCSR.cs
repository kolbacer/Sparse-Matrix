using System;
using System.Collections.Generic;
using Sparse_Matrix.ISparseMatrix;

namespace Sparse_Matrix.SparseMatrix_CSR
{
    using stype = Int32;
    using vtype = Double;

    partial class SparseMatrixCSR
    {
        public class SparseVectorCSR : SparseVector
        {
            public LinkedList<Element> Elements = new LinkedList<Element>(new Element[] { null });
            public stype Length { set; get; } = 0;
            public stype NumberOfNonzero { get { return Elements.Count - 1; } }
            public bool isColumn { set; get; } = false;

            public SparseVectorCSR() { }

            public SparseVectorCSR(IEnumerable<vtype> collection)
            {
                IEnumerator<vtype> it = collection.GetEnumerator();

                if (!it.MoveNext()) return;
                it.Reset();
                it.MoveNext();

                bool isNotEnd = true;
                for (stype i = 1; isNotEnd; ++i)
                {
                    vtype Value = it.Current;
                    if (Value != 0) Elements.AddBefore(Elements.Last, new Element(i, Value));
                    ++Length;
                    isNotEnd = it.MoveNext();
                }
            }

            public void AddLast(vtype x)
            {
                if (x != 0)
                    Elements.AddBefore(Elements.Last, new Element(Length + 1, x));
                ++Length;
            }

            public vtype[] ToFilled()
            {
                vtype[] array = new vtype[Length];

                LinkedListNode<Element> first = Elements.First;
                LinkedListNode<Element> current = first;

                for (int i = 1; i <= Length; ++i)
                {
                    if ((current == Elements.Last) || (i < current.Value.Index))
                        array[i + offset] = 0;
                    else if (i == current.Value.Index)
                    {
                        array[i + offset] = current.Value.ElemValue;
                        current = current.Next;
                    }
                }

                return array;
            }

            public void Print()
            {
                LinkedListNode<Element> first = Elements.First;
                LinkedListNode<Element> current = first;

                for (int i = 1; i <= Length; ++i)
                {
                    if ((current == Elements.Last) || (i < current.Value.Index))
                        Console.Write(0 + " ");
                    else if (i == current.Value.Index)
                    {
                        Console.Write(current.Value.ElemValue + " ");
                        current = current.Next;
                    }
                    if (isColumn) Console.WriteLine();
                }
                Console.WriteLine();
            }
        }
    }
}