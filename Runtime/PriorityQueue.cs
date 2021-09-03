using System;
using System.Collections.Generic;

namespace Lonfee.AStar
{
    public class PriorityQueue<T>
    {
        private IComparer<T> comparer;
        private T[] heap;

        public int Count { get; private set; }

        public PriorityQueue() : this(null) { }
        public PriorityQueue(int capacity) : this(capacity, null) { }
        public PriorityQueue(IComparer<T> comparer) : this(16, comparer) { }

        public PriorityQueue(int capacity, IComparer<T> comparer)
        {
            this.comparer = (comparer == null) ? Comparer<T>.Default : comparer;
            this.heap = new T[capacity];
        }

        public void Push(T value)
        {
            if (Count >= heap.Length)
                Array.Resize(ref heap, Count << 1);

            heap[Count] = value;
            SiftUp(Count++);
        }

        public T Pop()
        {
            T value = Top();
            heap[0] = heap[--Count];

            if (Count > 0)
                SiftDown(0);

            return value;
        }

        public T Top()
        {
            if (Count > 0)
                return heap[0];

            throw new InvalidOperationException("PriorityQueue is null.");
        }

        public void Clear()
        {
            Count = 0;
            for (int i = 0; i < heap.Length; i++)
            {
                heap[i] = default(T);
            }
        }

        private void SiftUp(int childIndex)
        {
            T value = heap[childIndex];

            for (int parentIndex = childIndex >> 1;
                childIndex > 0 && comparer.Compare(value, heap[parentIndex]) < 0;
                childIndex = parentIndex, parentIndex = parentIndex >> 1)
            {
                heap[childIndex] = heap[parentIndex];
            }

            heap[childIndex] = value;
        }

        private void SiftDown(int n)
        {
            T value = heap[n];

            for (int n2 = n << 1; n2 < Count; n = n2, n2 = n2 << 1)
            {
                if (n2 + 1 < Count && comparer.Compare(heap[n2 + 1], heap[n2]) < 0)
                    n2++;

                if (comparer.Compare(value, heap[n2]) <= 0)
                    break;

                heap[n] = heap[n2];
            }

            heap[n] = value;
        }
    }
}