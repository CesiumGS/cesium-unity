#include "UnityTaskProcessor.h"

#ifndef __EMSCRIPTEN__
#include <DotNet/System/Action.h>
#include <DotNet/System/Threading/Tasks/Task.h>
#endif // __EMSCRIPTEN__

#include <condition_variable>
#include <mutex>
#include <queue>
#include <thread>
#include <vector>

namespace CesiumForUnityNative {

#ifdef __EMSCRIPTEN__
// Unity Wasm can't use C# System/Threads, so we need to implement our own thread pool.

// How many threads to use in the pool. This is somewhat arbitrary; we
// just don't want to create a new thread for every task, to keep the number
// of threads under the Emscripten limit.
static const int UNITY_THREAD_POOL_SIZE = 4;

class UnityThreadPool {
public:
    UnityThreadPool(size_t threads);

    ~UnityThreadPool();

    void enqueue(std::function<void()> f);

private:
    std::vector<std::thread> workers;
    std::queue<std::function<void()>> tasks;
    std::mutex queue_mutex;
    std::condition_variable condition;
    bool stop;
};

UnityThreadPool::UnityThreadPool(size_t threads)
    : stop(false) {
    for(size_t i = 0; i < threads; ++i) {
        workers.emplace_back([this] {
            while (true) {
                std::function<void()> task;

                {
                    std::unique_lock<std::mutex> lock(this->queue_mutex);
                    
                    this->condition.wait(lock, [this] { 
                        return this->stop || !this->tasks.empty(); 
                    });

                    if (this->stop && this->tasks.empty()) {
                        return;
                    }
                    
                    task = std::move(this->tasks.front());
                    this->tasks.pop();
                }

                task();
            }
        });
    }
}

void UnityThreadPool::enqueue(std::function<void()> task) {
    {
        std::unique_lock<std::mutex> lock(queue_mutex);
        if (stop) {
          return;
        }
        tasks.emplace(task);
    }   
    condition.notify_one();
}

UnityThreadPool::~UnityThreadPool() {
    {
        std::unique_lock<std::mutex> lock(queue_mutex);
        stop = true;
    }
    
    condition.notify_all();
    
    for (std::thread &worker: workers) {
        worker.join();
    }
}

UnityTaskProcessor::UnityTaskProcessor()
  : _threadPool(new UnityThreadPool(UNITY_THREAD_POOL_SIZE)) {
}

UnityTaskProcessor::~UnityTaskProcessor() {
  delete _threadPool;
}
#else
UnityTaskProcessor::UnityTaskProcessor() {}

UnityTaskProcessor::~UnityTaskProcessor() {}
#endif // __EMSCRIPTEN__

void UnityTaskProcessor::startTask(std::function<void()> f) {
#ifdef __EMSCRIPTEN__
  _threadPool->enqueue(f);
#else // __EMSCRIPTEN__
  DotNet::System::Threading::Tasks::Task::Run(f);
#endif
}

} // namespace CesiumForUnityNative
