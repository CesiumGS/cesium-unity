using System;
using System.Collections.Generic;

namespace CesiumForUnity
{
    internal class CesiumObjectPool<T> : IDisposable where T : class
    {
        private List<T> _pool;
        private int _maximumSize;
        private Func<T> _createCallback;
        private Action<T> _releaseCallback;
        private Action<T> _destroyCallback;

        public CesiumObjectPool(Func<T> createCallback, Action<T> releaseCallback, Action<T> destroyCallback, int maximumSize = 1000)
        {
            this._pool = new List<T>(maximumSize);
            this._maximumSize = maximumSize;
            this._createCallback = createCallback;
            this._releaseCallback = releaseCallback;
            this._destroyCallback = destroyCallback;
        }

        public void Dispose()
        {
            this.Clear();

            // A null pool indicates released objects should be freed,
            // rather than added back into the pool.
            this._pool = null;
        }

        public int CountInactive => this._pool.Count;

        public void Clear()
        {
            if (this._pool == null)
                return;

            foreach (T o in this._pool)
            {
                this._destroyCallback(o);
            }
        }

        public T Get()
        {
            if (this._pool != null && this._pool.Count > 0)
            {
                int pos = this._pool.Count - 1;
                T result = this._pool[pos];
                this._pool.RemoveAt(pos);
                return result;
            }
            else
            {
                return this._createCallback();
            }
        }

        public void Release(T element)
        {
            this._releaseCallback(element);

            if (this._pool != null && this._pool.Count < this._maximumSize)
            {
                this._pool.Add(element);
            }
            else
            {
                this._destroyCallback(element);
            }
        }
    }
}
