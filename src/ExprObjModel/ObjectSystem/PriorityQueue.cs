using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExprObjModel.ObjectSystem
{
    class PriorityQueue<T>
    {
        private List<Tuple<long, T>> items;
        private Comparison<T> comparer;
        private long nextStamp;

        public PriorityQueue(Comparison<T> comparer)
        {
            this.items = new List<Tuple<long, T>>();
            this.comparer = comparer;
            this.nextStamp = 0L;
        }

        private static int IndexLeftChild(int index)
        {
            return (((index + 1) << 1) - 1);
        }

        private static int IndexRightChild(int index)
        {
            return ((index + 1) << 1);
        }

        private static int IndexParent(int index)
        {
            return (((index + 1) >> 1) - 1);
        }

        private int Compare(Tuple<long, T> a, Tuple<long, T> b)
        {
            int r = comparer(a.Item2, b.Item2);
            if (r != 0) return r;

            if (a.Item1 > b.Item1) return -1;
            if (a.Item1 < b.Item1) return 1;

            return 0;
        }

        private void Swap(int a, int b)
        {
            var x = items[a]; items[a] = items[b]; items[b] = x;
        }

        private void UpHeap(int index)
        {
            while (true)
            {
                if (index == 0) break;

                int parent = IndexParent(index);
                if (Compare(items[index], items[parent]) > 0)
                {
                    Swap(index, parent);
                    index = parent;
                }
                else
                {
                    break;
                }
            }
        }

        private void DownHeap(int index)
        {
            while (true)
            {
                int leftChild = IndexLeftChild(index);
                int rightChild = IndexRightChild(index);

                if (leftChild >= items.Count) break;

                int greaterChild = (rightChild >= items.Count) ? leftChild : (Compare(items[leftChild], items[rightChild]) < 0) ? rightChild : leftChild;
                if (Compare(items[index], items[greaterChild]) < 0)
                {
                    Swap(index, greaterChild);
                    index = greaterChild;
                }
                else break;
            }
        }

        public void Push(T item)
        {
            items.Add(new Tuple<long, T>(nextStamp, item));
            ++nextStamp;
            UpHeap(items.Count - 1);
        }

        public T Top
        {
            get
            {
                if (items.Count == 0) throw new InvalidOperationException("The top of an empty priority queue is not defined");
                return items[0].Item2;
            }
        }

        public T Pop()
        {
            T result = Top;
            items[0] = items[items.Count - 1];
            items.RemoveAt(items.Count - 1);
            DownHeap(0);
            return result;
        }

        public int Count { get { return items.Count; } }
    }

    public static partial class Utils
    {
        public static Comparison<T> CompareBy<T, U>(Func<T, U> selector)
        {
            IComparer<U> comparer = Comparer<U>.Default;

            Comparison<T> c = delegate(T a, T b)
            {
                return comparer.Compare(selector(a), selector(b));
            };

            return c;
        }

        public static Comparison<T> CompareLessThan<T>(Func<T, T, bool> lessThan)
        {
            Comparison<T> c = delegate(T a, T b)
            {
                if (lessThan(a, b)) return -1;
                if (lessThan(b, a)) return 1;
                return 0;
            };

            return c;
        }
    }
}
