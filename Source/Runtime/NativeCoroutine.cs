using System;
using System.Collections;

namespace CesiumForUnity
{
    internal partial class NativeCoroutine : IEnumerable
    {
        private Func<object, object> _callback;

        public NativeCoroutine(Func<object, object> callback)
        {
            _callback = callback;
        }

        public IEnumerator GetEnumerator()
        {
            // We're using the callback instance itself as a
            // an opaque sentinel that indicates "end the coroutine".
            object sentinel = this._callback;
            object next = this._callback(sentinel);
            while (next != sentinel)
            {
                yield return next;
                next = this._callback(sentinel);
            }
        }
    }
}
