#pragma once

#if defined(_WIN32)
#ifdef CESIUMFORUNITYNATIVERUNTIME_BUILDING
#define CESIUMFORUNITYNATIVERUNTIME_API __declspec(dllexport)
#else
#define CESIUMFORUNITYNATIVERUNTIME_API __declspec(dllimport)
#endif
#else
#define CESIUMFORUNITYNATIVERUNTIME_API
#endif
