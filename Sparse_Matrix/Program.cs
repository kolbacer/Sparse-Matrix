using System;
using System.IO;
using System.Collections.Generic;

namespace Sparse_Matrix
{
    using stype = Int32;
    using vtype = Double;

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
            matrix1.AddRows(2, 3, -6);
            matrix1.AddRows(1, 1, 2);
            matrix1.Print();
            Console.WriteLine();
            matrix1.PrintStorage();
        }
    }

    class SparseMatrix
    {
        LinkedList<Element> Elements = new LinkedList<Element>(new Element[] { null });
        LinkedListNode<Element>[] IA = { null };                   // хранит ссылки на элементы списка Elements
        private const stype offset = -1;                           // смещение для итерации по массиву
        //int[] IAIndexes = new int[] { 1 };  // на всякий случай

        public stype Rows { get; } = 0;
        public stype Columns { get; private set; } = 0;

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

            //int? amount = last.Value.ColumnIndex - first.Value.ColumnIndex + 1; // кол-во ненулевых элементов в строке

            for (LinkedListNode<Element> current = first; current.Previous != last; current = current.Next) // O(columns)
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

        public void AssignElement(stype i, stype j, vtype value) // O(rows+columns)
        {
            if ((i > Rows) || (j > Columns) || (i <= 0) || (j <= 0)) throw new Exception("Выход за пределы матрицы");

            LinkedListNode<Element> first = IA[i + offset];
            LinkedListNode<Element> last = IA[i + 1 + offset].Previous;

            //int? amount = last.Value.ColumnIndex - first.Value.ColumnIndex + 1; // кол-во ненулевых элементов в строке

            for (LinkedListNode<Element> current = first; (last == null) || (current.Previous != last.Next); current = current.Next) // O(columns)
            {
                if ((current.Previous != last) && (current.Value.ColumnIndex == j))
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
                if ((last == null) || (current.Previous == last) || (current.Value.ColumnIndex > j))  // add element
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

        public void AddRows(stype augend, stype addend, vtype coef = 1)   // augend - тот, к которому прибавляем, addend - тот, кого прибавляем, coef - коэффициент домножения
        {                                                                 // проверки на null, потенциально, можно убрать (кроме end != null)
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
                Elements.AddBefore(begin1, new Element(begin2.Value.ColumnIndex, begin2.Value.ElemValue * coef));
                for (stype k = augend; (k >= 1) && (IA[k + offset] == begin1); --k)
                {                                                             
                    IA[k + offset] = IA[k + offset].Previous;
                }
                for (LinkedListNode<Element> current = begin2.Next; (current != null) && (current != end2.Next); current = current.Next)
                {
                    Elements.AddBefore(IA[augend + 1 + offset], new Element(current.Value.ColumnIndex, current.Value.ElemValue * coef));
                }
                return;
            }

            LinkedListNode<Element> current1 = begin1;
            LinkedListNode<Element> current2 = begin2;

            if (begin2.Value.ColumnIndex < begin1.Value.ColumnIndex)   // новый 1ый элемент
            {
                Elements.AddBefore(begin1, new Element(begin2.Value.ColumnIndex, begin2.Value.ElemValue * coef));
                for (stype k = augend; (k >= 1) && (IA[k + offset] == begin1); --k)
                {
                    IA[k + offset] = IA[k + offset].Previous;
                }
                current2 = current2.Next;
            }

            while (((current1 != null) && (end1 != null) && (current1 != end1.Next)) || ((current2 != null) && (current2 != end2.Next)))
            {
                while ((current1 != null) && (current1 != end1.Next) && ((current2 == null) || (current2 == end2.Next) || (current1.Value.ColumnIndex < current2.Value.ColumnIndex)))
                {
                    current1 = current1.Next;
                }
                while ((current2 != null) && (end1 != null) && (current2 != end2.Next) && ((current1 == null) || (current1 == end1.Next) || (current2.Value.ColumnIndex <= current1.Value.ColumnIndex)))
                {
                    if ((current1 == null) || (current1 == end1.Next) || (current2.Value.ColumnIndex != current1.Value.ColumnIndex))
                    {
                        Elements.AddBefore(current1, new Element(current2.Value.ColumnIndex, current2.Value.ElemValue * coef));
                    }
                    else
                    {
                        if (current1.Value.ElemValue + (current2.Value.ElemValue * coef) == 0)       // удаление элемента
                        {
                            for (stype k = augend + 1; (k <= Rows + 1) && (IA[k + offset] == current1); ++k)
                            {
                                IA[k + offset] = IA[k + offset].Next;
                            }
                            for (stype k = augend; (k > 0) && (IA[k + offset] == current1); --k)
                            {
                                IA[k + offset] = IA[k + offset].Next;
                            }

                            LinkedListNode<Element> toDelete = current1;
                            current1 = current1.Next;
                            if (toDelete == end1) end1 = null;  // почему-то если удалять объект, на который ссылается end, он не становится null
                            Elements.Remove(toDelete);
                        }
                        else
                        {
                            current1.Value.ElemValue += (current2.Value.ElemValue * coef);
                        }
                    }

                    current2 = current2.Next;
                }
            }

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
                    Console.Write(current.Value.ColumnIndex + " ");
                } else
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

        public static SparseMatrix ReadFromFile(string filepath) // O(rows*columns)
        {                                                        // экономно по памяти
            FileStream fs = File.OpenRead(filepath);

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
