using System;
using System.Collections.Generic;

namespace Sparse_Matrix
{
    class Program
    {
        static void Main(string[] args)
        {
            SparsedMatrix matrix = new SparsedMatrix(3, 10);

            matrix.AssignElement(0, 2, 1);
            matrix.AssignElement(0, 3, 3);
            matrix.AssignElement(0, 7, 5);
            matrix.AssignElement(2, 5, 7);
            matrix.AssignElement(2, 7, 1);
            matrix.Print();
        }
    }

    class SparsedMatrix
    {
        LinkedList<int> IA = new LinkedList<int>(new int[1]);     // IA[i] - индекс для JA и AN первого элемента в i-й строке (IA[n] - первая свободная позиция в JA и AN)
                                                                  // не изменяет размер => нужно сделать массивом
                                                                  // (?) массив пар указателей

        LinkedList<int?> JA = new LinkedList<int?>(new int?[] { null });            // столбцовые индексы
        LinkedList<double?> AN = new LinkedList<double?>(new double?[] { null });   // значения ненулевых элементов матрицы

        int rows = 0;
        int columns = 0;

        public SparsedMatrix(int rows, int columns)
        {
            this.rows = rows;
            this.columns = columns;

            IA = new LinkedList<int>(new int[rows + 1]);
            JA = new LinkedList<int?>(new int?[] { null });
            AN = new LinkedList<double?>(new double?[] { null });
        }

        public double? GetElement(int i, int j)  // O(rows+columns)
        {
            if ((i>=rows) || (j>=columns) || (i<0) || (j<0)) throw new Exception("Выход за пределы матрицы");

            LinkedListNode<int> IANode = IA.First;
            for (int k = 0; k < i; ++k)  // O(rows)
            {
                IANode = IANode.Next;
            }
            int first = IANode.Value;
            int last = IANode.Next.Value - 1;

            if (first > last) return 0; // в строке нет ненулевых элементов

            int amount = last - first + 1; // кол-во ненулевых элементов в строке
            LinkedListNode<int?> JANode = JA.First;
            LinkedListNode<double?> ANNode = AN.First;
            for (int k = 0; k < first; ++k)       //
            {                                     //
                JANode = JANode.Next;             //
                ANNode = ANNode.Next;             //
            }                                     //
            for (int k = first; k <= last; ++k)   //
            {                                     //  O(columns)
                if (JANode.Value == j) {          //  
                    return ANNode.Value;          //
                }                                 //
                JANode = JANode.Next;             //
                ANNode = ANNode.Next;             //
            }                                     //

            return 0;
        }

        public void AssignElement(int i, int j, double value) // O(rows+columns)
        {
            if ((i >= rows) || (j >= columns) || (i < 0) || (j < 0)) throw new Exception("Выход за пределы матрицы");

            LinkedListNode<int> IANode = IA.First;
            for (int k = 0; k < i; ++k)  // O(rows)
            {
                IANode = IANode.Next;
            }

            int first = IANode.Value;
            int last = IANode.Next.Value - 1;
            int amount = last - first + 1;

            LinkedListNode<int?> JANode = JA.First;
            LinkedListNode<double?> ANNode = AN.First;
            for (int k = 0; k < first; ++k)        //
            {                                      //
                JANode = JANode.Next;              //
                ANNode = ANNode.Next;              //  O(rows+columns)
            }                                      //
            for (int k = first; k <= last+1; ++k)  //
            {
                if ((k <= last) && (JANode.Value == j))
                {
                    if (value != 0)  // reassign element
                    {
                        ANNode.Value = value;
                    } else           // delete element
                    {
                        JA.Remove(JANode);
                        AN.Remove(ANNode);

                        for (IANode = IANode.Next; IANode != null; IANode = IANode.Next) // O(rows)
                        {
                            --IANode.Value;
                        }
                    }
                    break;
                }
                if ((k == last+1) || (JANode.Value > j))  // add element
                {
                    JA.AddBefore(JANode, j);
                    AN.AddBefore(ANNode, value);

                    for (IANode = IANode.Next; IANode != null; IANode = IANode.Next)  // O(rows)
                    {
                        ++IANode.Value;
                    }
                    break;
                }

                JANode = JANode.Next;
                ANNode = ANNode.Next;
            }
        }

        public void Print()  // slow
        {
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < columns; ++j)
                {
                    Console.Write(GetElement(i, j) + " ");
                }
                Console.WriteLine();
            }
        }
    }
}
