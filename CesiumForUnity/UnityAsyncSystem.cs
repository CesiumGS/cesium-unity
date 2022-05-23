using CesiumAsync;
using System;
using System.Threading.Tasks;

namespace CesiumForUnity
{

    internal class UnityAsyncSystem : AsyncSystem
    {
        protected override void StartTask(Action task)
        {
            Task.Run(task);
        }
    }

}
