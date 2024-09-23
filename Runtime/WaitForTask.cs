using System;
using UnityEngine;

namespace CesiumForUnity
{
    /// <summary>
    /// A YieldInstruction that can be yielded from a coroutine in order to wait
    /// until a given task completes.
    /// </summary>
    public class WaitForTask : CustomYieldInstruction
    {
        private IAsyncResult _task;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="task">The task to wait for.</param>
        public WaitForTask(IAsyncResult task)
        {
            this._task = task;
        }

        public override bool keepWaiting => !this._task.IsCompleted;
    }
}
