using System;
using System.Collections.Generic;

namespace Sparse_Matrix
{
    class Program
    {
        static void Main(string[] args)
        {
            SparseMatrix matrix = new SparseMatrix(3, 10);

            matrix.AssignElement(1, 3, 1);
            matrix.AssignElement(1, 4, 3);
            matrix.AssignElement(1, 8, 5);
            matrix.AssignElement(3, 6, 7);
            matrix.AssignElement(3, 8, 1);
            matrix.Print();

            Console.WriteLine();
            matrix.PrintStorage();
        }
    }

    class SparseMatrix
    {
        LinkedList<Element<int, double>> Elements = new LinkedList<Element<int, double>>(new Element<int, double>[] { null });
        LinkedListNode<Element<int, double>>[] IA = { null };    // хранит ссылки на элементы списка Elements
        private const int offset = -1;                           // смещение для итерации по массиву
        //int[] IAIndexes = new int[] { 1 };  // на всякий случай

        public int Rows { get; } = 0;
        public int Columns { get; } = 0;

        public SparseMatrix(int Rows, int Columns)
        {
            this.Rows = Rows;
            this.Columns = Columns;

            Elements = new LinkedList<Element<int, double>>(new Element<int, double>[] { null });
            IA = new LinkedListNode<Element<int, double>>[Rows + 1];
            //IAIndexes = new int[Rows + 1];
            for (int i = 1; i <= Rows + 1; ++i)
            {
                //IAIndexes[i + offset] = 1;
                IA[i + offset] = Elements.First;
            }
        }

        public double GetElement(int i, int j)  // O(columns)
        {
            if ((i > Rows) || (j > Columns) || (i <= 0) || (j <= 0)) throw new Exception("Выход за пределы матрицы");

            LinkedListNode<Element<int, double>> first = IA[i + offset];
            LinkedListNode<Element<int, double>> last = IA[i+ 1 + offset].Previous;

            if ((last == null) || (first.Previous == last)) return 0; // в строке нет ненулевых элементов

            //int? amount = last.Value.ColumnIndex - first.Value.ColumnIndex + 1; // кол-во ненулевых элементов в строке

            for (LinkedListNode<Element<int, double>> current = first; current.Previous != last; current = current.Next) // O(columns)
            {
                if (current.Value.ColumnIndex == j)
                {
                    return current.Value.ElemValue;
                } 
                else if (current.Value.ColumnIndex > j)
                {
                    return 0;
                }
            }

            return 0;
        }

        public void AssignElement(int i, int j, double value) // O(rows+columns)
        {
            if ((i > Rows) || (j > Columns) || (i <= 0) || (j <= 0)) throw new Exception("Выход за пределы матрицы");

            LinkedListNode<Element<int, double>> first = IA[i + offset];
            LinkedListNode<Element<int, double>> last = IA[i + 1 + offset].Previous;

            //int? amount = last.Value.ColumnIndex - first.Value.ColumnIndex + 1; // кол-во ненулевых элементов в строке

            for (LinkedListNode<Element<int, double>> current = first; (last == null) || (current.Previous != last.Next); current = current.Next) // O(columns)
            {
                if ((current.Previous != last) && (current.Value.ColumnIndex == j))
                {
                    if (value != 0)  // reassign element
                    {
                        current.Value.ElemValue = value;
                    }
                    else             // delete element
                    {
                        for (int k = i + 1; (k <= Rows + 1) && (IA[k + offset] == current); ++k)  //
                        {                                                                         //
                            IA[k + offset] = IA[k + offset].Next;                                 //
                        }                                                                         // смещаем все ссылки, которые указывают на удаляемый элемент
                        for (int k = i; (k > 0) && (IA[k + offset] == current); --k)              // O(rows)
                        {                                                                         //
                            IA[k + offset] = IA[k + offset].Next;                                 //
                        }                                                                         //

                        Elements.Remove(current);

                        /*for (int k = i + 1; k <= Rows + 1; ++k)  // индексы
                        {
                            --IAIndexes[k + offset];
                        }*/
                    }
                    break;
                }
                if ((last == null) || (current.Previous == last) || (current.Value.ColumnIndex > j))  // add element
                {
                    Elements.AddBefore(current, new Element<int, double>(j, value));

                    for (int k = i; (k >= 1) && (IA[k + offset] == current); --k)  // если в i-й строке новый первый элемент => смещаем ссылки
                    {                                                              // O(rows)
                        IA[k + offset] = IA[k + offset].Previous;
                    }

                    /*for (int k = i + 1; k <= Rows + 1; ++k)   // индексы
                    {
                        ++IAIndexes[k + offset];
                    }*/
                    break;
                }
            }

        }

        public void Print()  // slow
        {
            for (int i = 1; i <= Rows; ++i)
            {
                for (int j = 1; j <= Columns; ++j)
                {
                    Console.Write(GetElement(i, j) + " ");
                }
                Console.WriteLine();
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
            for (int i = 1; i <= Rows + 1; ++i)
            {
                LinkedListNode<Element<int, double>> node = IA[i + offset];
                int count = 1;
                for (LinkedListNode<Element<int, double>> current = Elements.First; current != node; current = current.Next)
                {
                    ++count;
                }
                Console.Write(count + " ");
            }
            Console.WriteLine();
            Console.Write("JA: ");
            for (LinkedListNode<Element<int, double>> current = Elements.First; current != null; current = current.Next)
            {
                if (current.Value != null)
                {
                    Console.Write(current.Value.ColumnIndex + " ");
                } else
                {
                    Console.Write("N ");
                }
            }
            Console.WriteLine();
            Console.Write("AN: ");
            for (LinkedListNode<Element<int, double>> current = Elements.First; current != null; current = current.Next)
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

        // Вспомогательные классы

        public class Element<stype, vtype>  // пара индекс_столбца - значение
        {
            public stype ColumnIndex { get; set; }  // JA[i]
            public vtype ElemValue { get; set; }    // AN[i]

            public Element(stype ColumnIndex, vtype ElemValue)
            {
                this.ColumnIndex = ColumnIndex;
                this.ElemValue = ElemValue;
            }
        }
    }
}
