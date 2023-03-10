//#define COLLECTION_CHECK
using System;
using System.Collections.Concurrent;

namespace CesiumForUnity
{
    public class ObjectPool<T>
    {
        // Why not use Unity's built in ObjectPool?
        // It is not thread safe and it does not allow you to pre-allocate.
        public int Count => objects.Count;
        private ConcurrentBag<T> objects;
        private Func<T> objectConstructor;
        private Action<T> onRent;
        private Action<T> onReturn;
        private Action<T> objectDeconstructor;
        private int capacity;

        public ObjectPool(
            int capacity,
            int preallocationCount,
            Func<T> objectConstructor,
            Action<T> objectDeconstructor = null,
            Action<T> onRent = null,
            Action<T> onReturn = null)
        {
            objects = new ConcurrentBag<T>();
            this.objectConstructor = objectConstructor;
            this.objectDeconstructor = objectDeconstructor;
            this.onRent = onRent;
            this.onReturn = onReturn;
            this.capacity = capacity;

            for (int i = 0; i < preallocationCount; i++)
            {
                T item = objectConstructor();
                objects.Add(item);
            }
        }

        public T Rent()
        {
            if (!objects.TryTake(out T item))
                item = objectConstructor.Invoke();

            onRent?.Invoke(item);
            return item;
        }

        public void Release(T item)
        {

#if COLLECTION_CHECK
            if (objects.Count > 0)
            {
                foreach (var obj in objects)
                {
                    if (obj.Equals(item))
                        throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
                }
            }
#endif
            onReturn?.Invoke(item);
            if (objects.Count >= capacity)
            {
                objectDeconstructor?.Invoke(item);
                return;
            }
            objects.Add(item);
        }
    }
}