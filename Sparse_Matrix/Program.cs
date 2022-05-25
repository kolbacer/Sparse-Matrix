using System;
using System.IO;
using System.Collections.Generic;

namespace Sparse_Matrix
{
    using stype = Int32;
    using vtype = Double;

    static class Globals
    {
        public static string logfilepath = @"C:\Users\pc\source\repos\NIR\test\log.txt";
        public static StreamWriter sw;
    }

    class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

            Globals.sw = new StreamWriter(Globals.logfilepath, false);
            StreamWriter sw = Globals.sw;

            SparseMatrix matrix = new SparseMatrix(3, 10);

            matrix.AssignElement(1, 3, 1);
            matrix.AssignElement(1, 4, 3);
            matrix.AssignElement(1, 8, 5);
            matrix.AssignElement(3, 6, 7);
            matrix.AssignElement(3, 8, 1);
            matrix.Print();

            Console.WriteLine();
            matrix.PrintStorage();

            //

            string filename = @"C:\Users\pc\source\repos\NIR\test\text.txt";
            Console.WriteLine();
            SparseMatrix matrix1 = SparseMatrix.ReadFromFile(filename);
            matrix1.Print();
            Console.WriteLine();
            matrix1.PrintStorage();

            //

            Console.WriteLine();
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

            SparseMatrix.SwapRows(matrix1, 1, 3);
            matrix1.Print();
            Console.WriteLine();
            SparseMatrix.SwapRows(matrix1, 2, 3);
            matrix1.Print();

            Console.WriteLine();
            SparseMatrix matrix2 = matrix1.Copy();
            matrix2.AddRows(1, 2);
            matrix2.Print();
            Console.WriteLine();
            matrix1.Print();

            //

            Console.WriteLine();
            string filename2 = @"C:\Users\pc\source\repos\NIR\test\text2.txt";
            SparseMatrix matrix3 = SparseMatrix.ReadFromFile(filename2);
            matrix3.Print();
            Console.WriteLine();
            Console.WriteLine("LUP: ");
            Console.WriteLine();
            Console.WriteLine("L:");
            SparseMatrix.LUP LUP = matrix3.LUPdecompose();
            LUP.L.Print();
            Console.WriteLine();
            Console.WriteLine("U:");
            LUP.U.Print();
            Console.WriteLine();
            Console.WriteLine("P:");
            for (int i = 1; i <= LUP.P.Length; ++i)
            {
                Console.Write(LUP.P[i - 1] + " ");
            }
            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("Ly=b");
            SparseMatrix.SparseVector b = new SparseMatrix.SparseVector(new vtype[] { -46, 53, -62 });
            Console.Write("b = ");
            b.Print();

            Console.WriteLine();
            Console.WriteLine("x = ");
            SparseMatrix.SparseVector x = SparseMatrix.SolveSLAE(LUP, b);
            x.Print();

            //

            sw.WriteLine();
            string superfile = @"C:\Users\pc\source\repos\NIR\test\matrix1.txt";
            SparseMatrix supermatrix = SparseMatrix.ReadFromFile(superfile);
            supermatrix.PrintToLog();
            sw.WriteLine();
            sw.WriteLine("LUP: ");
            sw.WriteLine();
            sw.WriteLine("L:");
            SparseMatrix.LUP LUP1 = supermatrix.LUPdecompose();
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


            sw.Close();
        }
    }

    class SparseMatrix
    {
        LinkedList<Element> Elements = new LinkedList<Element>(new Element[] { null });
        LinkedListNode<Element>[] IA = { null };                   // хранит ссылки на элементы списка Elements
        private const stype offset = -1;                           // смещение для итерации по массиву
        private const double eps = 1e-10;                          // что считаем нулем
        //int[] IAIndexes = new int[] { 1 };                       // на всякий случай

        // для транспонированной \ хранение по столбцам
        LinkedList<Element> ElementsT = new LinkedList<Element>(new Element[] { null });
        LinkedListNode<Element>[] IAT = { null };
        private bool transposedIsRelevant = false;   // при различных операциях (перестановка строк и т.д.)

        // транспонирование может стать неактуальным
        public stype Rows { get; } = 0;
        public stype Columns { get; private set; } = 0;
        public stype NumberOfNonzeroElements { get { return Elements.Count - 1; } }

        public SparseMatrix(stype Rows, stype Columns)
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

            stype[] P = new stype[Rows];
            for (stype i = 1; i <= Rows; ++i)
                P[i + offset] = i;

            SparseMatrix L = new SparseMatrix(Rows, Columns);              // vtype должен быть с плавающей точкой
            L.Elements = new LinkedList<Element>(new Element[Rows + 1]);
            L.IA = new LinkedListNode<Element>[Rows + 1];
            LinkedListNode<Element> cur = L.Elements.First;
            for (stype i = 1; i <= Rows + 1; ++i)
            {
                L.IA[i + offset] = cur;
                cur = cur.Next;
            }

            SparseMatrix U = this.Copy();

            for (stype k = 1; k < Columns; ++k)
            {
                sw.WriteLine("Iteration " + k);

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

                    sw.WriteLine("SwapRows(" + k + ", " + maxRow + ")");
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

                    sw.WriteLine("AddRows(" + p + ", " + k + ", " + -coef + ")");

                    //U.CreateTransposed();    // вроде не нужно
                }

                sw.WriteLine("NonZero elements: " + U.NumberOfNonzeroElements);
                sw.WriteLine();
            }

            for (stype i = Rows; i >= 1; --i)
            {
                L.Elements.AddBefore(L.IA[i + 1 + offset], new Element(i, 1));
                L.IA[i + offset] = L.IA[i + offset].Next;
                L.Elements.Remove(L.IA[i + offset].Previous);
            }

            sw.WriteLine();

            return new LUP(L, U, P);
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

            StreamWriter sw = Globals.sw;
            sw.WriteLine("CreateTransposed()");

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
            SparseMatrix newMatrix = new SparseMatrix(Rows, Columns);

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

        public static void SwapRows(SparseMatrix matrix, stype row1, stype row2)             // работает за O(кол-во ненулевых элементов)
        {                                                                                    // ПОТОМУ ЧТО LinkedList'ы НЕЛЬЗЯ ОБЪЕДИНЯТЬ!!!!!!
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

        public static SparseVector MultiplyMatrixByVector(SparseMatrix matrix, SparseVector vector)  // *where vector is column
        {                                                                                            // O(rows*[кол-во ненулевых вектора] + кол-во ненулевых матрицы)
            if (matrix.Columns != vector.Length) throw new Exception("Кол-во столбцов матрицы != длина вектора");

            SparseVector result = new SparseVector();
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

        public static SparseVector SolveLy_b(SparseMatrix L, SparseVector _b, stype[] P = null)
        {
            if (L.Columns != _b.Length) throw new Exception("Кол-во столбцов матрицы != длина вектора");

            vtype[] tmp = _b.ToFilled();
            vtype[] b = new vtype[tmp.Length];
            SparseVector y = new SparseVector();
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

        public static SparseVector SolveUx_y(SparseMatrix U, SparseVector _y)
        {
            if (U.Columns != _y.Length) throw new Exception("Кол-во столбцов матрицы != длина вектора");

            vtype[] y = _y.ToFilled();
            SparseVector x = new SparseVector();
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

        public static SparseMatrix ReadFromFile(string filepath) // O(rows*columns)
        {                                                        // экономно по памяти
            FileStream fs = File.OpenRead(filepath);             // Bug: нужен символ после последнего числа

            stype rows = CountRows(fs);  // O(rows*columns)

            SparseMatrix matrix = new SparseMatrix(rows, 0);

            for (stype i = 1; (i <= rows) && (fs.Position != fs.Length); ++i) // O(rows*columns)
            {
                matrix.IA[i + 1 + offset] = matrix.IA[i + offset];

                int j = 1;
                bool firstAdded = false;
                while (fs.Position != fs.Length)  // O(columns)
                {
                    vtype? x = ReadNextValue(fs);
                    if (x == null) break;   // переход на следующую строку
                    else
                    {
                        if ((i == 1) && (j > matrix.Columns)) matrix.Columns = j;
                        if (x != 0)                     // добавление элемента
                        {
                            if (firstAdded)
                            {
                                matrix.Elements.AddBefore(matrix.Elements.Last, new Element(j, x.GetValueOrDefault(0)));
                            }
                            else
                            {
                                matrix.Elements.AddLast(new Element(j, x.GetValueOrDefault(0)));
                                SwapNodeElements(matrix.Elements.Last, matrix.Elements.Last.Previous);  // чтобы не перекидывать ссылки
                                matrix.IA[i + 1 + offset] = matrix.Elements.Last;

                                firstAdded = true;
                            }
                        }
                        ++j;
                    }
                }
            }

            fs.Dispose();

            return matrix;
        }

        private static vtype? ReadNextValue(FileStream fs)  // устанавливает курсор на конец следующего слова в строке,
        {                                                   // либо на начало следующей строки, если очередное слово не найдено
            byte[] buffer = new byte[256];
            int i = 0;
            char? c = (char)fs.ReadByte();
            for (; (c == ' ' || c == '\t'); c = (char)fs.ReadByte()) ; // пропускаем пробелы и табы

            if (c == '\r') c = (char)fs.ReadByte();
            if ((c == '\n') || (fs.Position == fs.Length))
            {
                return null;
            }

            for (; (c != '\n') && (c != ' ') && (c != '\t') && (fs.Position != fs.Length); c = (char)fs.ReadByte())
            {
                buffer[i] = (byte)c;
                ++i;
            }
            buffer[i] = (byte)'\n';
            string word = System.Text.Encoding.Default.GetString(buffer);

            if ((c == '\n') || (c == ' ') || (c == '\t'))
            {
                fs.Seek(-1, SeekOrigin.Current);
            }

            return vtype.Parse(word);
        }

        private static stype CountRows(FileStream fs)    // подсчитывает строки в матрице и возвращается к началу файла
        {
            int bufsize = 4;                             // если в первых [bufsize] байтах строки нет символов, кроме пробелов,
            byte[] buffer = new byte[bufsize];           // то строка считается пустой

            stype rows = 0;
            while (fs.Position != fs.Length)
            {
                fs.Read(buffer, 0, bufsize);
                string str = System.Text.Encoding.Default.GetString(buffer);
                if (!string.IsNullOrWhiteSpace(str)) ++rows;

                for (char? c = null; (c != '\n') && (fs.Position != fs.Length); c = (char)fs.ReadByte()) ;
            }

            fs.Seek(0, SeekOrigin.Begin);

            return rows;
        }

        private static void SwapNodeElements(LinkedListNode<Element> x, LinkedListNode<Element> y)
        {
            Element temp = x.Value;
            x.Value = y.Value;
            y.Value = temp;
        }

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

        public class LUP
        {
            public SparseMatrix L { private set; get; }
            public SparseMatrix U { private set; get; }
            public stype[] P { private set; get; }

            public LUP(SparseMatrix L, SparseMatrix U, stype[] P)
            {
                this.L = L;
                this.U = U;
                this.P = P;
            }
        }

        public class SparseVector
        {
            public LinkedList<Element> Elements = new LinkedList<Element>(new Element[] { null });
            public stype Length { set; get; } = 0;
            public stype NumberOfNonzero { get { return Elements.Count - 1; } }
            public bool isColumn = false;

            public SparseVector() { }

            public SparseVector(IEnumerable<vtype> collection)
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