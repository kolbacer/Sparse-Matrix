using System;
using System.IO;
using System.Collections.Generic;
using Sparse_Matrix.ISparseMatrix;
using IOutils;

namespace Sparse_Matrix.SparseMatrix_CSR
{
    using stype = Int32;
    using vtype = Double;

    public partial class SparseMatrixCSR : SparseMatrix
    {
        LinkedList<Element> Elements = new LinkedList<Element>(new Element[] { null });
        LinkedListNode<Element>[] IA = { null };                   // хранит ссылки на элементы списка Elements
        private const stype offset = -1;                           // смещение для итерации по массиву
        private const double eps = Globals.eps;                    // что считаем нулем
        //int[] IAIndexes = new int[] { 1 };                       // на всякий случай

        // для транспонированной \ хранение по столбцам
        LinkedList<Element> ElementsT = new LinkedList<Element>(new Element[] { null });
        LinkedListNode<Element>[] IAT = { null };
        private bool transposedIsRelevant = false;   // при различных операциях (перестановка строк и т.д.)
                                                     // транспонирование может стать неактуальным

        public stype Rows { get; } = 0;
        public stype Columns { get; private set; } = 0;
        public stype NumberOfNonzeroElements { get { return Elements.Count - 1; } }

        public SparseMatrixCSR(stype Rows, stype Columns)
        {
            this.Rows = Rows;
            this.Columns = Columns;

            Elements = new LinkedList<Element>(new Element[] { null });
            IA = new LinkedListNode<Element>[Rows + 1];
            //IAIndexes = new int[Rows + 1];
            for (stype i = 1; i <= Rows + 1; ++i)
            {
                //IAIndexes[i + offset] = 1;
                IA[i + offset] = Elements.First;
            }
        }

        public vtype GetElement(stype i, stype j)  // O(columns)
        {
            if ((i > Rows) || (j > Columns) || (i <= 0) || (j <= 0)) throw new Exception("Выход за пределы матрицы");

            LinkedListNode<Element> first = IA[i + offset];
            LinkedListNode<Element> last = IA[i + 1 + offset].Previous;

            if ((last == null) || (first.Previous == last)) return 0; // в строке нет ненулевых элементов

            //int? amount = last.Value.Index - first.Value.Index + 1; // кол-во ненулевых элементов в строке

            for (LinkedListNode<Element> current = first; current.Previous != last; current = current.Next) // O(columns)
            {
                if (current.Value.Index == j)
                {
                    return current.Value.ElemValue;
                }
                else if (current.Value.Index > j)
                {
                    return 0;
                }
            }

            return 0;
        }

        public vtype GetElementTransposed(stype i, stype j)  // O(columns)
        {
            if ((i > Columns) || (j > Rows) || (i <= 0) || (j <= 0)) throw new Exception("Выход за пределы матрицы");

            LinkedListNode<Element> first = IAT[i + offset];
            LinkedListNode<Element> last = IAT[i + 1 + offset].Previous;

            if ((last == null) || (first.Previous == last)) return 0; // в строке нет ненулевых элементов

            //int? amount = last.Value.Index - first.Value.Index + 1; // кол-во ненулевых элементов в строке

            for (LinkedListNode<Element> current = first; current.Previous != last; current = current.Next) // O(columns)
            {
                if (current.Value.Index == j)
                {
                    return current.Value.ElemValue;
                }
                else if (current.Value.Index > j)
                {
                    return 0;
                }
            }

            return 0;
        }

        public void AssignElement(stype i, stype j, vtype value) // O(rows+columns)
        {
            if ((i > Rows) || (j > Columns) || (i <= 0) || (j <= 0)) throw new Exception("Выход за пределы матрицы");

            LinkedListNode<Element> first = IA[i + offset];
            LinkedListNode<Element> last = IA[i + 1 + offset].Previous;

            //int? amount = last.Value.Index - first.Value.Index + 1; // кол-во ненулевых элементов в строке

            for (LinkedListNode<Element> current = first; (last == null) || (current.Previous != last.Next); current = current.Next) // O(columns)
            {
                if ((current.Previous != last) && (current.Value.Index == j))
                {
                    if (value != 0)  // reassign element
                    {
                        current.Value.ElemValue = value;
                    }
                    else             // delete element
                    {
                        for (stype k = i + 1; (k <= Rows + 1) && (IA[k + offset] == current); ++k)  //
                        {                                                                           //
                            IA[k + offset] = IA[k + offset].Next;                                   //
                        }                                                                           // смещаем все ссылки, которые указывают на удаляемый элемент
                        for (stype k = i; (k > 0) && (IA[k + offset] == current); --k)              // O(rows)
                        {                                                                           //
                            IA[k + offset] = IA[k + offset].Next;                                   //
                        }                                                                           //

                        Elements.Remove(current);

                        /*for (int k = i + 1; k <= Rows + 1; ++k)  // индексы
                        {
                            --IAIndexes[k + offset];
                        }*/
                    }
                    break;
                }
                if ((last == null) || (current.Previous == last) || (current.Value.Index > j))  // add element
                {
                    Elements.AddBefore(current, new Element(j, value));

                    for (stype k = i; (k >= 1) && (IA[k + offset] == current); --k)  // если в i-й строке новый первый элемент => смещаем ссылки
                    {                                                                // O(rows)
                        IA[k + offset] = IA[k + offset].Previous;
                    }

                    /*for (int k = i; k >= 1; --k)   // индексы
                    {
                        ++IAIndexes[k + offset];
                    }*/
                    break;
                }
            }
        }

        public LUP LUPdecompose()
        {
            if (Rows != Columns) throw new Exception("Матрица не квадратная");

            StreamWriter sw = Globals.sw;

            StreamWriter swL = Globals.swL;
            StreamWriter swU = Globals.swU;

            stype[] P = new stype[Rows];
            for (stype i = 1; i <= Rows; ++i)
                P[i + offset] = i;

            SparseMatrixCSR L = new SparseMatrixCSR(Rows, Columns);              // vtype должен быть с плавающей точкой
            L.Elements = new LinkedList<Element>(new Element[Rows + 1]);
            L.IA = new LinkedListNode<Element>[Rows + 1];
            LinkedListNode<Element> cur = L.Elements.First;
            for (stype i = 1; i <= Rows + 1; ++i)
            {
                L.IA[i + offset] = cur;
                cur = cur.Next;
            }

            SparseMatrixCSR U = (SparseMatrixCSR)this.Copy();

            for (stype k = 1; k < Columns; ++k)
            {
                //sw.WriteLine("Iteration " + k);
                sw.WriteLine(k + " " + (L.NumberOfNonzeroElements+Columns) + " " + U.NumberOfNonzeroElements);

                Element maxElement = U.FindMaxInColumn(k, k);
                if (maxElement.ElemValue == 0) throw new Exception("Матрица вырожденная");
                stype maxRow = maxElement.Index;
                if (maxRow != k)
                {
                    stype temp = P[k + offset];
                    P[k + offset] = P[maxRow + offset];
                    P[maxRow + offset] = temp;

                    SwapRows(U, k, maxRow);
                    SwapRows(L, k, maxRow);

                    //sw.WriteLine("SwapRows(" + k + ", " + maxRow + ")");
                }

                U.CreateTransposed();  // O(кол-во ненулевых элементов)
                LinkedListNode<Element> first = U.IAT[k + offset];
                LinkedListNode<Element> last = U.IAT[k + 1 + offset].Previous;
                while ((first.Value.Index <= k) && (first != last.Next))
                    first = first.Next;
                if (first == last.Next) continue;

                vtype valueToDivide = U.IA[k + offset].Value.ElemValue;    // НЕНАДЕЖНО!! (потому что в k-ой строке могут вместо нулей в начале оказаться маленькие числа)
                for (LinkedListNode<Element> current = first; current != last.Next;)
                {
                    vtype coef = current.Value.ElemValue / valueToDivide;
                    L.Elements.AddBefore(L.IA[current.Value.Index + 1 + offset], new Element(k, coef));
                    stype p = current.Value.Index;
                    current = current.Next;
                    U.AddRows(p, k, -coef);

                    //sw.WriteLine("AddRows(" + p + ", " + k + ", " + -coef + ")");

                    //U.CreateTransposed();    // вроде не нужно
                }

                //sw.WriteLine("NonZero elements: " + U.NumberOfNonzeroElements);
                //sw.WriteLine();
                if (L.NumberOfNonzeroElements != null)
                {
                    swL.WriteLine(L.NumberOfNonzeroElements + Columns);
                }
                else
                {
                    swL.WriteLine(0);
                }

                if (U.NumberOfNonzeroElements != null)
                {
                    swU.WriteLine(U.NumberOfNonzeroElements);
                }
                else
                {
                    swU.WriteLine(0);
                }
            }

            for (stype i = Rows; i >= 1; --i)
            {
                L.Elements.AddBefore(L.IA[i + 1 + offset], new Element(i, 1));
                L.IA[i + offset] = L.IA[i + offset].Next;
                L.Elements.Remove(L.IA[i + offset].Previous);
            }

            //sw.WriteLine();

            return new LUP_CSR(L, U, P);
        }

        public void AddRows(stype augend, stype addend, vtype coef = 1)   // augend - тот, к которому прибавляем, addend - тот, кого прибавляем, coef - коэффициент домножения
        {
            if ((augend < 1) || (augend > Rows) || (addend < 1) || (addend > Rows))
            {
                throw new Exception("Выход за пределы матрицы");
            }

            if (augend == addend)      // строчка прибавляется сама к себе
            {
                LinkedListNode<Element> begin = IA[augend + offset];
                LinkedListNode<Element> end = IA[augend + 1 + offset].Previous;

                if ((end == null) || (end == begin.Previous)) return;

                if (coef == -1)
                {
                    for (stype k = augend + 1; (k <= Rows + 1) && (IA[k + offset] == begin); ++k)
                    {
                        IA[k + offset] = end.Next;
                    }
                    for (stype k = augend; (k > 0) && (IA[k + offset] == begin); --k)
                    {
                        IA[k + offset] = end.Next;
                    }

                    LinkedListNode<Element> current = begin;
                    while ((current != null) && (end != null) && (current != end.Next))
                    {

                        LinkedListNode<Element> toDelete = current;
                        current = current.Next;
                        if (toDelete == end) end = null;  // почему-то если удалять объект, на который ссылается end, он не становится null
                        Elements.Remove(toDelete);
                    }
                }
                else
                {
                    for (LinkedListNode<Element> current = begin; (current != null) && (current != end.Next); current = current.Next)
                    {
                        current.Value.ElemValue += (current.Value.ElemValue * coef);
                    }
                }

                return;
            }

            LinkedListNode<Element> begin1 = IA[augend + offset];
            LinkedListNode<Element> end1 = IA[augend + 1 + offset].Previous;
            LinkedListNode<Element> begin2 = IA[addend + offset];
            LinkedListNode<Element> end2 = IA[addend + 1 + offset].Previous;

            if ((end2 == null) || (end2 == begin2.Previous) || (coef == 0)) return;

            if ((end1 == null) || (end1 == begin1.Previous))
            {
                Elements.AddBefore(begin1, new Element(begin2.Value.Index, begin2.Value.ElemValue * coef));
                for (stype k = augend; (k >= 1) && (IA[k + offset] == begin1); --k)
                {
                    IA[k + offset] = IA[k + offset].Previous;
                }
                for (LinkedListNode<Element> current = begin2.Next; (current != null) && (current != end2.Next); current = current.Next)
                {
                    Elements.AddBefore(IA[augend + 1 + offset], new Element(current.Value.Index, current.Value.ElemValue * coef));
                }
                return;
            }

            LinkedListNode<Element> current1 = begin1;
            LinkedListNode<Element> current2 = begin2;

            if (begin2.Value.Index < begin1.Value.Index)   // новый 1ый элемент
            {
                Elements.AddBefore(begin1, new Element(begin2.Value.Index, begin2.Value.ElemValue * coef));
                for (stype k = augend; (k >= 1) && (IA[k + offset] == begin1); --k)
                {
                    IA[k + offset] = IA[k + offset].Previous;
                }
                current2 = current2.Next;
            }

            LinkedList<LinkedListNode<Element>> toRemove = new LinkedList<LinkedListNode<Element>>();

            while ((current1 != end1.Next) || (current2 != end2.Next))
            {
                if ((current2 == end2.Next) || ((current1 != end1.Next) && (current1.Value.Index < current2.Value.Index)))
                    current1 = current1.Next;
                else if ((current1 == end1.Next) || ((current2 != end2.Next) && (current2.Value.Index < current1.Value.Index)))
                {
                    if (current1 == end1.Next)
                    {
                        Elements.AddBefore(current1, new Element(current2.Value.Index, current2.Value.ElemValue * coef));
                        end1 = current1.Previous;
                    }
                    else
                        Elements.AddBefore(current1, new Element(current2.Value.Index, current2.Value.ElemValue * coef));

                    if (IA[augend + offset] == current1)
                        IA[augend + offset] = current1.Previous;

                    current2 = current2.Next;
                }
                else // if (current1.Vaule.Index == current2.Value.Index)
                {
                    if (Math.Abs(current1.Value.ElemValue + (current2.Value.ElemValue * coef)) < eps)      // !!!!!!!!!
                    {
                        for (stype k = augend + 1; (k <= Rows + 1) && (IA[k + offset] == current1); ++k)
                        {
                            IA[k + offset] = IA[k + offset].Next;
                        }
                        for (stype k = augend; (k > 0) && (IA[k + offset] == current1); --k)
                        {
                            IA[k + offset] = IA[k + offset].Next;
                        }

                        toRemove.AddLast(current1);

                        current1 = current1.Next;
                    }
                    else
                    {
                        current1.Value.ElemValue += (current2.Value.ElemValue * coef);
                        current1 = current1.Next;
                    }
                    current2 = current2.Next;
                }
            }

            for (LinkedListNode<LinkedListNode<Element>> cur = toRemove.First; cur != null; cur = cur.Next)
            {
                Elements.Remove(cur.Value);
            }
        }

        private void CreateTransposed()         // O(кол-во ненулевых элемментов)
        {                                       // создает столбцовое представление
            if (transposedIsRelevant) return;

            //StreamWriter sw = Globals.sw;
            //sw.WriteLine("CreateTransposed()");

            ElementsT = new LinkedList<Element>(new Element[Columns + 1]);
            IAT = new LinkedListNode<Element>[Columns + 1];
            LinkedListNode<Element> current = ElementsT.First;
            for (stype i = 1; i <= Columns + 1; ++i)     // C# LinkedList не поддерживает конкатенацию, поэтому все будем делать в одном списке
            {
                IAT[i + offset] = current;
                current = current.Next;
            }

            for (stype i = 1; i <= Rows; ++i)
            {
                LinkedListNode<Element> first = IA[i + offset];
                LinkedListNode<Element> last = IA[i + 1 + offset].Previous;

                for (current = first; current != last.Next; current = current.Next)
                {
                    ElementsT.AddBefore(IAT[current.Value.Index + 1 + offset], new Element(i, current.Value.ElemValue));
                }
            }

            for (stype i = Columns; i >= 1; --i)
            {
                IAT[i + offset] = IAT[i + offset].Next;
                ElementsT.Remove(IAT[i + offset].Previous);
            }

            //transposedIsRelevant = true;
        }

        public SparseMatrix Copy()        // O(кол-во элементов)
        {
            SparseMatrixCSR newMatrix = new SparseMatrixCSR(Rows, Columns);

            stype i = 1;
            LinkedListNode<Element> first = IA[1 + offset];
            LinkedListNode<Element> last = IA[Rows + 1 + offset].Previous;

            for (LinkedListNode<Element> current = first; current != last.Next; current = current.Next)
            {
                newMatrix.Elements.AddBefore(newMatrix.Elements.Last, new Element(current.Value.Index, current.Value.ElemValue));
                if (IA[i + offset] == current)
                {
                    newMatrix.IA[i + offset] = newMatrix.Elements.Last.Previous;
                    ++i;
                }
            }

            return newMatrix;
        }

        private Element FindMaxInColumn(stype column, stype startRow = 0, stype endRow = 0)  // max по модулю
        {
            if (startRow == 0) startRow = 1;
            if (endRow == 0) endRow = Rows;
            CreateTransposed();

            LinkedListNode<Element> first = IAT[column + offset];
            LinkedListNode<Element> last = IAT[column + 1 + offset].Previous;
            stype curRow = first.Value.Index;
            LinkedListNode<Element> curNode = first;

            Element maxElement = null;
            while ((curRow <= endRow) && (curNode != last.Next))
            {
                if ((curRow >= startRow) && ((maxElement == null) || (Math.Abs(maxElement.ElemValue) < Math.Abs(curNode.Value.ElemValue))))
                    maxElement = curNode.Value;

                curNode = curNode.Next;
                if (curNode != last.Next)
                    curRow = curNode.Value.Index;
            }

            return maxElement;
        }

        public static void SwapRows(SparseMatrix _matrix, stype row1, stype row2)             // работает за O(кол-во ненулевых элементов)
        {                                                                                    // ПОТОМУ ЧТО LinkedList'ы НЕЛЬЗЯ ОБЪЕДИНЯТЬ!!!!!!
            SparseMatrixCSR matrix = (SparseMatrixCSR)_matrix;
            if ((row1 < 1) || (row1 > matrix.Rows) || (row2 < 1) || (row2 > matrix.Rows))
                throw new Exception("Выход за пределы матрицы");

            if (row1 == row2) return;

            if (row1 > row2)
            {
                stype temp = row1;
                row1 = row2;
                row2 = temp;
            }

            LinkedList<Element> newElements = new LinkedList<Element>(new Element[] { null });
            LinkedListNode<Element>[] newIA = new LinkedListNode<Element>[matrix.Rows + 1];
            newIA[matrix.Rows + 1 + offset] = newElements.Last;

            LinkedListNode<Element> first, last, current;

            for (stype i = 1; i <= matrix.Rows; ++i)
            {
                if (i == row1)
                {
                    first = matrix.IA[row2 + offset];
                    last = matrix.IA[row2 + 1 + offset].Previous;
                }
                else if (i == row2)
                {
                    first = matrix.IA[row1 + offset];
                    last = matrix.IA[row1 + 1 + offset].Previous;
                }
                else
                {
                    first = matrix.IA[i + offset];
                    last = matrix.IA[i + 1 + offset].Previous;
                }

                newIA[i + offset] = newElements.Last;
                for (current = first; current != last.Next; current = current.Next)
                {
                    if (current.Value == null)
                    {
                        newElements.AddBefore(newElements.Last, new Element(1, 1));  // bad
                        newElements.Last.Previous.Value = null;
                    }
                    else
                        newElements.AddBefore(newElements.Last, new Element(current.Value.Index, current.Value.ElemValue));
                    if (current == first)
                        newIA[i + offset] = newElements.Last.Previous;
                }
            }

            matrix.IA = newIA;
            matrix.Elements = newElements;
            matrix.transposedIsRelevant = false;
        }

        public static SparseVector MultiplyMatrixByVector(SparseMatrix _matrix, SparseVector _vector)  // *where vector is column
        {                                                                                              // O(rows*[кол-во ненулевых вектора] + кол-во ненулевых матрицы)
            SparseMatrixCSR matrix = (SparseMatrixCSR)_matrix;
            SparseVectorCSR vector = (SparseVectorCSR)_vector;
            if (matrix.Columns != vector.Length) throw new Exception("Кол-во столбцов матрицы != длина вектора");

            SparseVectorCSR result = new SparseVectorCSR();
            result.isColumn = true;

            LinkedListNode<Element> first2 = vector.Elements.First;
            LinkedListNode<Element> last2 = vector.Elements.Last.Previous;
            if (last2 == null)
            {
                result.Length = vector.Length;
                return result;
            }

            for (stype i = 1; i <= matrix.Rows; ++i)
            {
                LinkedListNode<Element> first1 = matrix.IA[i + offset];
                LinkedListNode<Element> last1 = matrix.IA[i + 1 + offset].Previous;

                if ((last1 == null) || (first1 == last1.Next))  // нет ненулевых
                {
                    result.AddLast(0);
                    continue;
                }

                LinkedListNode<Element> current1 = first1;
                LinkedListNode<Element> current2 = first2;
                vtype sum = 0;

                while ((current1 != last1.Next) || (current2 != last2.Next))
                {
                    if ((current2 == last2.Next) || ((current1 != last1.Next) && (current1.Value.Index < current2.Value.Index)))
                        current1 = current1.Next;
                    else if ((current1 == last1.Next) || ((current2 != last2.Next) && (current2.Value.Index < current1.Value.Index)))
                        current2 = current2.Next;
                    else
                    {
                        sum += (current1.Value.ElemValue * current2.Value.ElemValue);
                        current1 = current1.Next;
                    }
                }

                result.AddLast(sum);
            }

            return result;
        }

        public static SparseVector SolveSLAE(LUP LUP, SparseVector b)
        {
            SparseVector y = SolveLy_b(LUP.L, b, LUP.P);
            SparseVector x = SolveUx_y(LUP.U, y);

            return x;
        }

        public static SparseVector SolveLy_b(SparseMatrix _L, SparseVector _b, stype[] P = null)
        {
            SparseMatrixCSR L = (SparseMatrixCSR)_L;
            if (L.Columns != _b.Length) throw new Exception("Кол-во столбцов матрицы != длина вектора");

            vtype[] tmp = _b.ToFilled();
            vtype[] b = new vtype[tmp.Length];
            SparseVectorCSR y = new SparseVectorCSR();
            y.isColumn = true;

            if (P != null)
            {
                for (stype i = 1; i <= b.Length; ++i)
                    b[i + offset] = tmp[P[i + offset] + offset];
            }
            else
            {
                b = tmp;
            }
            tmp = null;

            for (stype i = 1; i <= L.Rows; ++i)
            {
                LinkedListNode<Element> first = L.IA[i + offset];
                LinkedListNode<Element> last = L.IA[i + 1 + offset].Previous.Previous;
                vtype sum = 0;

                if (last == null)
                {
                    y.AddLast(b[i + offset]);
                    continue;
                }

                for (LinkedListNode<Element> current = first; current != last.Next; current = current.Next)
                    sum += current.Value.ElemValue * b[current.Value.Index + offset];

                b[i + offset] -= sum;
                y.AddLast(b[i + offset]);
            }

            return y;
        }

        public static SparseVector SolveUx_y(SparseMatrix _U, SparseVector _y)
        {
            SparseMatrixCSR U = (SparseMatrixCSR)_U;
            if (U.Columns != _y.Length) throw new Exception("Кол-во столбцов матрицы != длина вектора");

            vtype[] y = _y.ToFilled();
            SparseVectorCSR x = new SparseVectorCSR();
            x.isColumn = true;

            for (stype i = y.Length; i >= 1; --i)
            {
                LinkedListNode<Element> first = U.IA[i + offset];
                LinkedListNode<Element> last = U.IA[i + 1 + offset].Previous;
                vtype sum = 0;

                if (first == last.Next) throw new Exception("Матрица вырожденная");

                for (LinkedListNode<Element> current = first.Next; current != last.Next; current = current.Next)
                    sum += current.Value.ElemValue * y[current.Value.Index + offset];

                y[i + offset] = (y[i + offset] - sum) / first.Value.ElemValue;
                x.Elements.AddBefore(x.Elements.First, new Element(i, y[i + offset]));
                x.Length++;
            }

            return x;
        }

        public void Print()  // slow
        {
            for (stype i = 1; i <= Rows; ++i)
            {
                for (stype j = 1; j <= Columns; ++j)
                {
                    Console.Write(GetElement(i, j) + " ");
                }
                Console.WriteLine();
            }
        }

        public void PrintTransposed()  // slow
        {
            CreateTransposed();

            for (stype i = 1; i <= Columns; ++i)
            {
                for (stype j = 1; j <= Rows; ++j)
                {
                    Console.Write(GetElementTransposed(i, j) + " ");
                }
                Console.WriteLine();
            }
        }

        public void PrintToLog()  // slow
        {
            StreamWriter sw = Globals.sw;

            for (stype i = 1; i <= Rows; ++i)
            {
                for (stype j = 1; j <= Columns; ++j)
                {
                    sw.Write(GetElement(i, j) + " ");
                }
                sw.WriteLine();
            }
        }

        public void PrintStorage()
        {
            Console.WriteLine("Storage: ");
            /*Console.Write("IAIndexes: ");
            for (int i = 1; i <= Rows + 1; ++i)
            {
                Console.Write(IAIndexes[i + offset] + " ");
            }
            Console.WriteLine();*/
            Console.Write("IA: ");
            for (stype i = 1; i <= Rows + 1; ++i)
            {
                LinkedListNode<Element> node = IA[i + offset];
                int count = 1;
                for (LinkedListNode<Element> current = Elements.First; current != node; current = current.Next)
                {
                    ++count;
                }
                Console.Write(count + " ");
            }
            Console.WriteLine();
            Console.Write("JA: ");
            for (LinkedListNode<Element> current = Elements.First; current != null; current = current.Next)
            {
                if (current.Value != null)
                {
                    Console.Write(current.Value.Index + " ");
                }
                else
                {
                    Console.Write("N ");
                }
            }
            Console.WriteLine();
            Console.Write("AN: ");
            for (LinkedListNode<Element> current = Elements.First; current != null; current = current.Next)
            {
                if (current.Value != null)
                {
                    Console.Write(current.Value.ElemValue + " ");
                }
                else
                {
                    Console.Write("N ");
                }
            }
            Console.WriteLine();
        }

        public void PrintStorageTransposed()
        {
            Console.WriteLine("Storage Transposed: ");
            /*Console.Write("IAIndexes: ");
            for (int i = 1; i <= Rows + 1; ++i)
            {
                Console.Write(IAIndexes[i + offset] + " ");
            }
            Console.WriteLine();*/
            Console.Write("IAT: ");
            for (stype i = 1; i <= Columns + 1; ++i)
            {
                LinkedListNode<Element> node = IAT[i + offset];
                int count = 1;
                for (LinkedListNode<Element> current = ElementsT.First; current != node; current = current.Next)
                {
                    ++count;
                }
                Console.Write(count + " ");
            }
            Console.WriteLine();
            Console.Write("JAT: ");
            for (LinkedListNode<Element> current = ElementsT.First; current != null; current = current.Next)
            {
                if (current.Value != null)
                {
                    Console.Write(current.Value.Index + " ");
                }
                else
                {
                    Console.Write("N ");
                }
            }
            Console.WriteLine();
            Console.Write("ANT: ");
            for (LinkedListNode<Element> current = ElementsT.First; current != null; current = current.Next)
            {
                if (current.Value != null)
                {
                    Console.Write(current.Value.ElemValue + " ");
                }
                else
                {
                    Console.Write("N ");
                }
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Читает матрицу в обычном виде из файла.
        /// Первые 2 числа - количество строк и столбцов.
        /// </summary>
        public static SparseMatrix ReadDenseFromFile(string filepath)  // O(rows*columns)
        {                                                              // экономно по памяти
            FileStream fs = File.OpenRead(filepath);

            Scanner fileScanner = new Scanner(fs);

            stype rows = stype.Parse(fileScanner.NextToken());
            stype cols = stype.Parse(fileScanner.NextToken());

            SparseMatrixCSR matrix = new SparseMatrixCSR(rows, cols);

            for (stype i = 1; i <= rows; ++i)
            {
                matrix.IA[i + 1 + offset] = matrix.IA[i + offset];

                bool firstAdded = false;
                for (stype j = 1; j <= cols; ++j)
                {
                    vtype x = vtype.Parse(fileScanner.NextToken());
                    if (x != 0)                     // добавление элемента
                    {
                        if (firstAdded)
                        {
                            matrix.Elements.AddBefore(matrix.Elements.Last, new Element(j, x));
                        }
                        else
                        {
                            matrix.Elements.AddLast(new Element(j, x));
                            SwapNodeElements(matrix.Elements.Last, matrix.Elements.Last.Previous);  // чтобы не перекидывать ссылки
                            matrix.IA[i + 1 + offset] = matrix.Elements.Last;

                            firstAdded = true;
                        }
                    }
                }
            }
            fs.Dispose();

            return matrix;
        }

        private static void SwapNodeElements(LinkedListNode<Element> x, LinkedListNode<Element> y)
        {
            Element temp = x.Value;
            x.Value = y.Value;
            y.Value = temp;
        }
    }
}