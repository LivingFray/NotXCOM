using System;
using System.Collections.Generic;

//You'd think a language with a set of built in collections implemented
//PriorityQueues, but apparently not

class PriorityQueue<T> {
    SortedList<Pair<float>, T> _list;
    int num;

    public int Count { get; private set; }

    public PriorityQueue() {
        _list = new SortedList<Pair<float>, T>(new PairComparer<float>());
    }

    public void Enqueue(T item, float priority) {
        _list.Add(new Pair<float>(priority, num), item);
        num++;
        Count++;
    }

    public T Dequeue() {
        T item = _list[_list.Keys[0]];
        _list.RemoveAt(0);
        Count--;
        return item;
    }

    public bool Contains(T value) {
        return _list.ContainsValue(value);
    }
}

class Pair<T> {
    public T First { get; private set; }
    public T Second { get; private set; }

    public Pair(T first, T second) {
        First = first;
        Second = second;
    }

    public override int GetHashCode() {
        return First.GetHashCode() ^ Second.GetHashCode();
    }

    public override bool Equals(object other) {
        Pair<T> pair = other as Pair<T>;
        if (pair == null) {
            return false;
        }
        return (this.First.Equals(pair.First) && this.Second.Equals(pair.Second));
    }
}

class PairComparer<T> : IComparer<Pair<T>> where T : IComparable {
    public int Compare(Pair<T> x, Pair<T> y) {
        if (x.First.CompareTo(y.First) < 0) {
            return -1;
        } else if (x.First.CompareTo(y.First) > 0) {
            return 1;
        } else {
            return x.Second.CompareTo(y.Second);
        }
    }
}