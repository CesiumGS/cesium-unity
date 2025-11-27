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

// Unity Wasm can't use C# System/Threads, so we need to implement our own
// thread pool.

// How many threads to use in the pool. This is somewhat arbitrary; we
// just don't want to create a new thread for every task, to keep the number
// of threads under the Emscripten limit.
namespace {
const int UNITY_THREAD_POOL_SIZE = 4;
}

class UnityThreadPool {
public:
  UnityThreadPool(size_t threads);

  ~UnityThreadPool();

  void enqueue(std::function<void()> f);

private:
  std::vector<std::thread> _workers;
  std::queue<std::function<void()>> _tasks;
  std::mutex _queueMutex;
  std::condition_variable _condition;
  bool _stop;
};

UnityThreadPool::UnityThreadPool(size_t threads) : _stop(false) {
  for (size_t i = 0; i < threads; ++i) {
    this->_workers.emplace_back([this] {
      while (true) {
        std::function<void()> task;

        {
          std::unique_lock<std::mutex> lock(this->_queueMutex);

          this->_condition.wait(lock, [this] {
            return this->_stop || !this->_tasks.empty();
          });

          if (this->_stop && this->_tasks.empty()) {
            return;
          }

          task = std::move(this->_tasks.front());
          this->_tasks.pop();
        }

        task();
      }
    });
  }
}

void UnityThreadPool::enqueue(std::function<void()> task) {
  {
    std::unique_lock<std::mutex> lock(this->_queueMutex);
    if (this->_stop) {
      return;
    }
    this->_tasks.emplace(std::move(task));
    this->_condition.notify_one();
  }
}

UnityThreadPool::~UnityThreadPool() {
  {
    std::unique_lock<std::mutex> lock(this->_queueMutex);
    this->_stop = true;
  }

  this->_condition.notify_all();
  for (std::thread& worker : this->_workers) {
    worker.join();
  }
}

UnityTaskProcessor::UnityTaskProcessor()
    : _pThreadPool(std::make_unique<UnityThreadPool>(UNITY_THREAD_POOL_SIZE)) {}
#else
UnityTaskProcessor::UnityTaskProcessor() = default;
#endif // __EMSCRIPTEN__

UnityTaskProcessor::~UnityTaskProcessor() = default;

void UnityTaskProcessor::startTask(std::function<void()> f) {
#ifdef __EMSCRIPTEN__
  this->_pThreadPool->enqueue(f);
#else // __EMSCRIPTEN__
  DotNet::System::Threading::Tasks::Task::Run(f);
#endif
}

} // namespace CesiumForUnityNative
