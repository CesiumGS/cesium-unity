#include "UnityLoggerSink.h"

#include <Oxidize/System/Object.h>
#include <Oxidize/System/String.h>
#include <Oxidize/UnityEngine/Debug.h>

using namespace CesiumForUnity;
using namespace Oxidize;

void UnityLoggerSink::sink_it_(const spdlog::details::log_msg& msg) {
  spdlog::memory_buf_t formatted;
  this->formatter_->format(msg, formatted);
  UnityEngine::Debug::Log(System::String(fmt::to_string(formatted)));
}

void UnityLoggerSink::flush_() {}
