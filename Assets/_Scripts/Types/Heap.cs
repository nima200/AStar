using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Heap<T> where T : IHeapItem<T>
{
    public T[] Items { get; private set; }

    public int Count { get; private set; }

    public Heap(int maxHeapSize)
    {
        Items = new T[maxHeapSize];
    }

    public void Add(T item)
    {
        item.HeapIndex = Count;
        Items[Count] = item;
        SortUp(item);
        Count++;
    }

    public T RemoveFirst()
    {
        var firstItem = Items[0];
        Count--;
        Items[0] = Items[Count];
        Items[0].HeapIndex = 0;
        SortDown(Items[0]);
        return firstItem;
    }

    public void UpdateItem(T item)
    {
        SortUp(item);
    }

    public bool Contains(T item)
    {
        return Equals(Items[item.HeapIndex], item);
    }

    private void SortDown(T item)
    {
        while (true)
        {
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            if (childIndexLeft < Count)
            {
                int swapIndex = childIndexLeft;
                if (childIndexRight < Count)
                {
                    if (Items[childIndexLeft].CompareTo(Items[childIndexRight]) < 0)
                    {
                        swapIndex = childIndexRight;
                    }
                }
                if (item.CompareTo(Items[swapIndex]) < 0)
                {
                    Swap(item, Items[swapIndex]);
                }
                else { return; }
            }
            else { return; }
        }
    }

    private void SortUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;
        while (true)
        {
            var parentItem = Items[parentIndex];
            if (item.CompareTo(parentItem) > 0)
            {
                Swap(item, parentItem);
            }
            else
            {
                break;
            }
            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    private void Swap(T item1, T item2)
    {
        Items[item1.HeapIndex] = item2;
        Items[item2.HeapIndex] = item1;
        int item1Index = item1.HeapIndex;
        item1.HeapIndex = item2.HeapIndex;
        item2.HeapIndex = item1Index;
    }
}