/// <summary>
/// Internals of the bindings between native and .NET code.
/// Game code shouldn't go here.
/// </summary>
/// <author>
/// Jackson Dunstan, 2017, http://JacksonDunstan.com
/// </author>
/// <license>
/// MIT
/// </license>

#include "Cesium3DTileset.h"
#include "NativeDownloadHandler.h"

// Type definitions
#include "Bindings.h"

// For assert()
#include <assert.h>

// For memset(), etc.
#include <string.h>

// Macro to put before functions that need to be exposed to C#
#ifdef _WIN32
	#define DLLEXPORT extern "C" __declspec(dllexport)
#else
	#define DLLEXPORT extern "C"
#endif

////////////////////////////////////////////////////////////////
// C# functions for C++ to call
////////////////////////////////////////////////////////////////

namespace Plugin
{
	void (*ReleaseObject)(int32_t handle);
	int32_t (*StringNew)(const char* chars);
	void (*SetException)(int32_t handle);
	int32_t (*ArrayGetLength)(int32_t handle);
	int32_t (*EnumerableGetEnumerator)(int32_t handle);

	/*BEGIN FUNCTION POINTERS*/
	void (*SystemIDisposableMethodDispose)(int32_t thisHandle);
	void (*ReleaseSystemDecimal)(int32_t handle);
	int32_t (*SystemDecimalConstructorSystemDouble)(double value);
	int32_t (*SystemDecimalConstructorSystemUInt64)(uint64_t value);
	int32_t (*BoxDecimal)(int32_t valHandle);
	int32_t (*UnboxDecimal)(int32_t valHandle);
	UnityEngine::Vector3 (*UnityEngineVector3ConstructorSystemSingle_SystemSingle_SystemSingle)(float x, float y, float z);
	int32_t (*BoxVector3)(UnityEngine::Vector3& val);
	UnityEngine::Vector3 (*UnboxVector3)(int32_t valHandle);
	UnityEngine::Vector4 (*UnityEngineVector4ConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingle)(float x, float y, float z, float w);
	int32_t (*BoxVector4)(UnityEngine::Vector4& val);
	UnityEngine::Vector4 (*UnboxVector4)(int32_t valHandle);
	UnityEngine::Quaternion (*UnityEngineQuaternionConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingle)(float x, float y, float z, float w);
	int32_t (*BoxQuaternion)(UnityEngine::Quaternion& val);
	UnityEngine::Quaternion (*UnboxQuaternion)(int32_t valHandle);
	UnityEngine::Matrix4x4 (*UnityEngineMatrix4x4ConstructorUnityEngineVector4_UnityEngineVector4_UnityEngineVector4_UnityEngineVector4)(UnityEngine::Vector4& column0, UnityEngine::Vector4& column1, UnityEngine::Vector4& column2, UnityEngine::Vector4& column3);
	UnityEngine::Quaternion (*UnityEngineMatrix4x4PropertyGetRotation)(UnityEngine::Matrix4x4* thiz);
	UnityEngine::Vector3 (*UnityEngineMatrix4x4PropertyGetLossyScale)(UnityEngine::Matrix4x4* thiz);
	UnityEngine::Vector3 (*UnityEngineMatrix4x4MethodGetPosition)(UnityEngine::Matrix4x4* thiz);
	int32_t (*BoxMatrix4x4)(UnityEngine::Matrix4x4& val);
	UnityEngine::Matrix4x4 (*UnboxMatrix4x4)(int32_t valHandle);
	int32_t (*UnityEngineObjectPropertyGetName)(int32_t thisHandle);
	void (*UnityEngineObjectPropertySetName)(int32_t thisHandle, int32_t valueHandle);
	int32_t (*UnityEngineComponentPropertyGetTransform)(int32_t thisHandle);
	UnityEngine::Vector3 (*UnityEngineTransformPropertyGetPosition)(int32_t thisHandle);
	void (*UnityEngineTransformPropertySetPosition)(int32_t thisHandle, UnityEngine::Vector3& value);
	UnityEngine::Quaternion (*UnityEngineTransformPropertyGetRotation)(int32_t thisHandle);
	void (*UnityEngineTransformPropertySetRotation)(int32_t thisHandle, UnityEngine::Quaternion& value);
	UnityEngine::Vector3 (*UnityEngineTransformPropertyGetLocalScale)(int32_t thisHandle);
	void (*UnityEngineTransformPropertySetLocalScale)(int32_t thisHandle, UnityEngine::Vector3& value);
	UnityEngine::Matrix4x4 (*UnityEngineTransformPropertyGetLocalToWorldMatrix)(int32_t thisHandle);
	int32_t (*UnityEngineTransformPropertyGetParent)(int32_t thisHandle);
	void (*UnityEngineTransformPropertySetParent)(int32_t thisHandle, int32_t valueHandle);
	UnityEngine::Vector3 (*UnityEngineTransformPropertyGetForward)(int32_t thisHandle);
	void (*UnityEngineTransformPropertySetForward)(int32_t thisHandle, UnityEngine::Vector3& value);
	UnityEngine::Vector3 (*UnityEngineTransformPropertyGetUp)(int32_t thisHandle);
	void (*UnityEngineTransformPropertySetUp)(int32_t thisHandle, UnityEngine::Vector3& value);
	int32_t (*SystemCollectionsIEnumeratorPropertyGetCurrent)(int32_t thisHandle);
	int32_t (*SystemCollectionsIEnumeratorMethodMoveNext)(int32_t thisHandle);
	int32_t (*UnityEngineGameObjectConstructorSystemString)(int32_t nameHandle);
	int32_t (*UnityEngineGameObjectPropertyGetTransform)(int32_t thisHandle);
	int32_t (*UnityEngineGameObjectMethodAddComponentCesiumForUnityBaseCesium3DTileset)(int32_t thisHandle);
	int32_t (*UnityEngineGameObjectMethodAddComponentUnityEngineMeshFilter)(int32_t thisHandle);
	int32_t (*UnityEngineGameObjectMethodAddComponentUnityEngineMeshRenderer)(int32_t thisHandle);
	int32_t (*UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveType)(UnityEngine::PrimitiveType type);
	void (*UnityEngineGameObjectMethodSetActiveSystemBoolean)(int32_t thisHandle, uint32_t value);
	void (*UnityEngineDebugMethodLogSystemObject)(int32_t messageHandle);
	int32_t (*UnityEngineMonoBehaviourPropertyGetTransform)(int32_t thisHandle);
	int32_t (*UnityEngineMonoBehaviourPropertyGetGameObject)(int32_t thisHandle);
	UnityEngine::Vector3 (*SystemCollectionsGenericIEnumeratorUnityEngineVector3PropertyGetCurrent)(int32_t thisHandle);
	int32_t (*SystemCollectionsGenericIEnumeratorSystemInt32PropertyGetCurrent)(int32_t thisHandle);
	int32_t (*SystemCollectionsGenericIEnumerableUnityEngineVector3MethodGetEnumerator)(int32_t thisHandle);
	int32_t (*SystemCollectionsGenericIEnumerableSystemInt32MethodGetEnumerator)(int32_t thisHandle);
	int32_t (*UnityEngineMeshConstructor)();
	void (*UnityEngineMeshMethodSetVerticesUnityEngineVector3Array1)(int32_t thisHandle, int32_t inVerticesHandle);
	void (*UnityEngineMeshMethodSetNormalsUnityEngineVector3Array1)(int32_t thisHandle, int32_t inNormalsHandle);
	void (*UnityEngineMeshMethodSetTrianglesSystemInt32Array1_SystemInt32_SystemBoolean_SystemInt32)(int32_t thisHandle, int32_t trianglesHandle, int32_t submesh, uint32_t calculateBounds, int32_t baseVertex);
	int32_t (*UnityEngineMeshFilterPropertyGetMesh)(int32_t thisHandle);
	void (*UnityEngineMeshFilterPropertySetMesh)(int32_t thisHandle, int32_t valueHandle);
	int32_t (*UnityEngineMeshRendererPropertyGetMaterial)(int32_t thisHandle);
	void (*UnityEngineMeshRendererPropertySetMaterial)(int32_t thisHandle, int32_t valueHandle);
	int32_t (*SystemExceptionConstructorSystemString)(int32_t messageHandle);
	int32_t (*BoxPrimitiveType)(UnityEngine::PrimitiveType val);
	UnityEngine::PrimitiveType (*UnboxPrimitiveType)(int32_t valHandle);
	float (*UnityEngineTimePropertyGetDeltaTime)();
	int32_t (*UnityEngineCameraPropertyGetMain)();
	float (*UnityEngineCameraPropertyGetFieldOfView)(int32_t thisHandle);
	void (*UnityEngineCameraPropertySetFieldOfView)(int32_t thisHandle, float value);
	float (*UnityEngineCameraPropertyGetAspect)(int32_t thisHandle);
	void (*UnityEngineCameraPropertySetAspect)(int32_t thisHandle, float value);
	int32_t (*UnityEngineCameraPropertyGetPixelWidth)(int32_t thisHandle);
	int32_t (*UnityEngineCameraPropertyGetPixelHeight)(int32_t thisHandle);
	int32_t (*BoxRawDownloadedData)(CesiumForUnity::RawDownloadedData& val);
	CesiumForUnity::RawDownloadedData (*UnboxRawDownloadedData)(int32_t valHandle);
	void (*ReleaseBaseNativeDownloadHandler)(int32_t handle);
	void (*BaseNativeDownloadHandlerConstructor)(int32_t cppHandle, int32_t* handle);
	int32_t (*UnityEngineNetworkingUnityWebRequestPropertyGetError)(int32_t thisHandle);
	int32_t (*UnityEngineNetworkingUnityWebRequestPropertyGetIsDone)(int32_t thisHandle);
	int64_t (*UnityEngineNetworkingUnityWebRequestPropertyGetResponseCode)(int32_t thisHandle);
	int32_t (*UnityEngineNetworkingUnityWebRequestPropertyGetUrl)(int32_t thisHandle);
	void (*UnityEngineNetworkingUnityWebRequestPropertySetUrl)(int32_t thisHandle, int32_t valueHandle);
	int32_t (*UnityEngineNetworkingUnityWebRequestPropertyGetMethod)(int32_t thisHandle);
	void (*UnityEngineNetworkingUnityWebRequestPropertySetMethod)(int32_t thisHandle, int32_t valueHandle);
	int32_t (*UnityEngineNetworkingUnityWebRequestPropertyGetDownloadHandler)(int32_t thisHandle);
	void (*UnityEngineNetworkingUnityWebRequestPropertySetDownloadHandler)(int32_t thisHandle, int32_t valueHandle);
	int32_t (*UnityEngineNetworkingUnityWebRequestMethodGetSystemString)(int32_t uriHandle);
	void (*UnityEngineNetworkingUnityWebRequestMethodSetRequestHeaderSystemString_SystemString)(int32_t thisHandle, int32_t nameHandle, int32_t valueHandle);
	int32_t (*UnityEngineNetworkingUnityWebRequestMethodSendWebRequest)(int32_t thisHandle);
	int32_t (*UnityEngineNetworkingUnityWebRequestMethodGetResponseHeaderSystemString)(int32_t thisHandle, int32_t nameHandle);
	void (*UnityEngineNetworkingUnityWebRequestAsyncOperationAddEventCompleted)(int32_t thisHandle, int32_t delHandle);
	void (*UnityEngineNetworkingUnityWebRequestAsyncOperationRemoveEventCompleted)(int32_t thisHandle, int32_t delHandle);
	void* (*SystemRuntimeInteropServicesMarshalMethodStringToCoTaskMemUTF8SystemString)(int32_t sHandle);
	void (*SystemRuntimeInteropServicesMarshalMethodFreeCoTaskMemSystemIntPtr)(void* ptr);
	void (*ReleaseBaseCesium3DTileset)(int32_t handle);
	void (*BaseCesium3DTilesetConstructor)(int32_t cppHandle, int32_t* handle);
	int32_t (*SystemThreadingTasksTaskMethodRunSystemAction)(int32_t actionHandle);
	int32_t (*UnityEngineResourcesMethodLoadUnityEngineMaterialSystemString)(int32_t pathHandle);
	int32_t (*UnityEditorSceneViewPropertyGetLastActiveSceneView)();
	int32_t (*UnityEditorSceneViewPropertyGetCamera)(int32_t thisHandle);
	int32_t (*BoxBoolean)(uint32_t val);
	int32_t (*UnboxBoolean)(int32_t valHandle);
	int32_t (*BoxSByte)(int8_t val);
	int8_t (*UnboxSByte)(int32_t valHandle);
	int32_t (*BoxByte)(uint8_t val);
	uint8_t (*UnboxByte)(int32_t valHandle);
	int32_t (*BoxInt16)(int16_t val);
	int16_t (*UnboxInt16)(int32_t valHandle);
	int32_t (*BoxUInt16)(uint16_t val);
	uint16_t (*UnboxUInt16)(int32_t valHandle);
	int32_t (*BoxInt32)(int32_t val);
	int32_t (*UnboxInt32)(int32_t valHandle);
	int32_t (*BoxUInt32)(uint32_t val);
	uint32_t (*UnboxUInt32)(int32_t valHandle);
	int32_t (*BoxInt64)(int64_t val);
	int64_t (*UnboxInt64)(int32_t valHandle);
	int32_t (*BoxUInt64)(uint64_t val);
	uint64_t (*UnboxUInt64)(int32_t valHandle);
	int32_t (*BoxChar)(uint16_t val);
	int16_t (*UnboxChar)(int32_t valHandle);
	int32_t (*BoxSingle)(float val);
	float (*UnboxSingle)(int32_t valHandle);
	int32_t (*BoxDouble)(double val);
	double (*UnboxDouble)(int32_t valHandle);
	int32_t (*UnityEngineUnityEngineVector3Array1Constructor1)(int32_t length0);
	UnityEngine::Vector3 (*UnityEngineVector3Array1GetItem1)(int32_t thisHandle, int32_t index0);
	int32_t (*UnityEngineVector3Array1SetItem1)(int32_t thisHandle, int32_t index0, UnityEngine::Vector3& item);
	int32_t (*SystemSystemInt32Array1Constructor1)(int32_t length0);
	int32_t (*SystemInt32Array1GetItem1)(int32_t thisHandle, int32_t index0);
	int32_t (*SystemInt32Array1SetItem1)(int32_t thisHandle, int32_t index0, int32_t item);
	void (*ReleaseSystemAction)(int32_t handle, int32_t classHandle);
	void (*SystemActionConstructor)(int32_t cppHandle, int32_t* handle, int32_t* classHandle);
	void (*SystemActionAdd)(int32_t thisHandle, int32_t delHandle);
	void (*SystemActionRemove)(int32_t thisHandle, int32_t delHandle);
	void (*SystemActionInvoke)(int32_t thisHandle);
	void (*ReleaseSystemActionUnityEngineAsyncOperation)(int32_t handle, int32_t classHandle);
	void (*SystemActionUnityEngineAsyncOperationConstructor)(int32_t cppHandle, int32_t* handle, int32_t* classHandle);
	void (*SystemActionUnityEngineAsyncOperationAdd)(int32_t thisHandle, int32_t delHandle);
	void (*SystemActionUnityEngineAsyncOperationRemove)(int32_t thisHandle, int32_t delHandle);
	void (*SystemActionUnityEngineAsyncOperationInvoke)(int32_t thisHandle, int32_t objHandle);
	/*END FUNCTION POINTERS*/
}

////////////////////////////////////////////////////////////////
// Global variables
////////////////////////////////////////////////////////////////

namespace Plugin
{
	System::String NullString(nullptr);
}

////////////////////////////////////////////////////////////////
// Plugin Types
////////////////////////////////////////////////////////////////

namespace Plugin
{
	ManagedType::ManagedType()
		: Handle(0)
	{
	}

	ManagedType::ManagedType(decltype(nullptr))
		: Handle(0)
	{
	}

	ManagedType::ManagedType(Plugin::InternalUse iu, int32_t handle)
		: Handle(handle)
	{
	}
}

////////////////////////////////////////////////////////////////
// C# Primitive Types
////////////////////////////////////////////////////////////////

namespace System
{
	Boolean::Boolean()
		: Value(0)
	{
	}

	Boolean::Boolean(bool value)
		: Value((int32_t)value)
	{
	}

	Boolean::Boolean(int32_t value)
		: Value(value)
	{
	}

	Boolean::Boolean(uint32_t value)
		: Value(value)
	{
	}

	Boolean::operator bool() const
	{
		return (bool)Value;
	}

	Boolean::operator int32_t() const
	{
		return Value;
	}

	Boolean::operator uint32_t() const
	{
		return Value;
	}

	Boolean::operator Object() const
	{
		return Object(Plugin::InternalUse::Only, Plugin::BoxBoolean(Value));
	}

	Boolean::operator ValueType() const
	{
		return ValueType(Plugin::InternalUse::Only, Plugin::BoxBoolean(Value));
	}

	Boolean::operator IComparable() const
	{
		return IComparable(Plugin::InternalUse::Only, Plugin::BoxBoolean(Value));
	}

	Boolean::operator IFormattable() const
	{
		return IFormattable(Plugin::InternalUse::Only, Plugin::BoxBoolean(Value));
	}

	Boolean::operator IConvertible() const
	{
		return IConvertible(Plugin::InternalUse::Only, Plugin::BoxBoolean(Value));
	}

	Boolean::operator IComparable_1<Boolean>() const
	{
		return IComparable_1<Boolean>(Plugin::InternalUse::Only, Plugin::BoxBoolean(Value));
	}

	Boolean::operator IEquatable_1<Boolean>() const
	{
		return IEquatable_1<Boolean>(Plugin::InternalUse::Only, Plugin::BoxBoolean(Value));
	}

	Char::Char()
		: Value(0)
	{
	}

	Char::Char(char value)
		: Value(value)
	{
	}

	Char::Char(int16_t value)
		: Value(value)
	{
	}

	Char::operator int16_t() const
	{
		return Value;
	}

	Char::operator Object() const
	{
		return Object(Plugin::InternalUse::Only, Plugin::BoxChar(Value));
	}

	Char::operator ValueType() const
	{
		return ValueType(Plugin::InternalUse::Only, Plugin::BoxChar(Value));
	}

	Char::operator IComparable() const
	{
		return IComparable(Plugin::InternalUse::Only, Plugin::BoxChar(Value));
	}

	Char::operator IFormattable() const
	{
		return IFormattable(Plugin::InternalUse::Only, Plugin::BoxChar(Value));
	}

	Char::operator IConvertible() const
	{
		return IConvertible(Plugin::InternalUse::Only, Plugin::BoxChar(Value));
	}

	Char::operator IComparable_1<Char>() const
	{
		return IComparable_1<Char>(Plugin::InternalUse::Only, Plugin::BoxChar(Value));
	}

	Char::operator IEquatable_1<Char>() const
	{
		return IEquatable_1<Char>(Plugin::InternalUse::Only, Plugin::BoxChar(Value));
	}

	SByte::SByte()
		: Value(0)
	{
	}

	SByte::SByte(int8_t val)
		: Value(val)
	{
	}

	SByte::operator int8_t() const
	{
		return Value;
	}

	SByte::operator Object() const
	{
		return Object(Plugin::InternalUse::Only, Plugin::BoxSByte(Value));
	}

	SByte::operator ValueType() const
	{
		return ValueType(Plugin::InternalUse::Only, Plugin::BoxSByte(Value));
	}

	SByte::operator IComparable() const
	{
		return IComparable(Plugin::InternalUse::Only, Plugin::BoxSByte(Value));
	}

	SByte::operator IFormattable() const
	{
		return IFormattable(Plugin::InternalUse::Only, Plugin::BoxSByte(Value));
	}

	SByte::operator IConvertible() const
	{
		return IConvertible(Plugin::InternalUse::Only, Plugin::BoxSByte(Value));
	}

	SByte::operator IComparable_1<SByte>() const
	{
		return IComparable_1<SByte>(Plugin::InternalUse::Only, Plugin::BoxSByte(Value));
	}

	SByte::operator IEquatable_1<SByte>() const
	{
		return IEquatable_1<SByte>(Plugin::InternalUse::Only, Plugin::BoxSByte(Value));
	}

	Byte::Byte()
		: Value(0)
	{
	}

	Byte::Byte(uint8_t value)
		: Value(value)
	{
	}

	Byte::operator uint8_t() const
	{
		return Value;
	}

	Byte::operator Object() const
	{
		return Object(Plugin::InternalUse::Only, Plugin::BoxByte(Value));
	}

	Byte::operator ValueType() const
	{
		return ValueType(Plugin::InternalUse::Only, Plugin::BoxByte(Value));
	}

	Byte::operator IComparable() const
	{
		return IComparable(Plugin::InternalUse::Only, Plugin::BoxByte(Value));
	}

	Byte::operator IFormattable() const
	{
		return IFormattable(Plugin::InternalUse::Only, Plugin::BoxByte(Value));
	}

	Byte::operator IConvertible() const
	{
		return IConvertible(Plugin::InternalUse::Only, Plugin::BoxByte(Value));
	}

	Byte::operator IComparable_1<Byte>() const
	{
		return IComparable_1<Byte>(Plugin::InternalUse::Only, Plugin::BoxByte(Value));
	}

	Byte::operator IEquatable_1<Byte>() const
	{
		return IEquatable_1<Byte>(Plugin::InternalUse::Only, Plugin::BoxByte(Value));
	}

	Int16::Int16()
		: Value(0)
	{
	}

	Int16::Int16(int16_t value)
		: Value(value)
	{
	}

	Int16::operator int16_t() const
	{
		return Value;
	}

	Int16::operator Object() const
	{
		return Object(Plugin::InternalUse::Only, Plugin::BoxInt16(Value));
	}

	Int16::operator ValueType() const
	{
		return ValueType(Plugin::InternalUse::Only, Plugin::BoxInt16(Value));
	}

	Int16::operator IComparable() const
	{
		return IComparable(Plugin::InternalUse::Only, Plugin::BoxInt16(Value));
	}

	Int16::operator IFormattable() const
	{
		return IFormattable(Plugin::InternalUse::Only, Plugin::BoxInt16(Value));
	}

	Int16::operator IConvertible() const
	{
		return IConvertible(Plugin::InternalUse::Only, Plugin::BoxInt16(Value));
	}

	Int16::operator IComparable_1<Int16>() const
	{
		return IComparable_1<Int16>(Plugin::InternalUse::Only, Plugin::BoxInt16(Value));
	}

	Int16::operator IEquatable_1<Int16>() const
	{
		return IEquatable_1<Int16>(Plugin::InternalUse::Only, Plugin::BoxInt16(Value));
	}

	UInt16::UInt16()
		: Value(0)
	{
	}

	UInt16::UInt16(uint16_t value)
		: Value(value)
	{
	}

	UInt16::operator uint16_t() const
	{
		return Value;
	}

	UInt16::operator Object() const
	{
		return Object(Plugin::InternalUse::Only, Plugin::BoxUInt16(Value));
	}

	UInt16::operator ValueType() const
	{
		return ValueType(Plugin::InternalUse::Only, Plugin::BoxUInt16(Value));
	}

	UInt16::operator IComparable() const
	{
		return IComparable(Plugin::InternalUse::Only, Plugin::BoxUInt16(Value));
	}

	UInt16::operator IFormattable() const
	{
		return IFormattable(Plugin::InternalUse::Only, Plugin::BoxUInt16(Value));
	}

	UInt16::operator IConvertible() const
	{
		return IConvertible(Plugin::InternalUse::Only, Plugin::BoxUInt16(Value));
	}

	UInt16::operator IComparable_1<UInt16>() const
	{
		return IComparable_1<UInt16>(Plugin::InternalUse::Only, Plugin::BoxUInt16(Value));
	}

	UInt16::operator IEquatable_1<UInt16>() const
	{
		return IEquatable_1<UInt16>(Plugin::InternalUse::Only, Plugin::BoxUInt16(Value));
	}

	Int32::Int32()
		: Value(0)
	{
	}

	Int32::Int32(int32_t value)
		: Value(value)
	{
	}

	Int32::operator int32_t() const
	{
		return Value;
	}

	Int32::operator Object() const
	{
		return Object(Plugin::InternalUse::Only, Plugin::BoxInt32(Value));
	}

	Int32::operator ValueType() const
	{
		return ValueType(Plugin::InternalUse::Only, Plugin::BoxInt32(Value));
	}

	Int32::operator IComparable() const
	{
		return IComparable(Plugin::InternalUse::Only, Plugin::BoxInt32(Value));
	}

	Int32::operator IFormattable() const
	{
		return IFormattable(Plugin::InternalUse::Only, Plugin::BoxInt32(Value));
	}

	Int32::operator IConvertible() const
	{
		return IConvertible(Plugin::InternalUse::Only, Plugin::BoxInt32(Value));
	}

	Int32::operator IComparable_1<Int32>() const
	{
		return IComparable_1<Int32>(Plugin::InternalUse::Only, Plugin::BoxInt32(Value));
	}

	Int32::operator IEquatable_1<Int32>() const
	{
		return IEquatable_1<Int32>(Plugin::InternalUse::Only, Plugin::BoxInt32(Value));
	}

	UInt32::UInt32()
		: Value(0)
	{
	}

	UInt32::UInt32(uint32_t value)
		: Value(value)
	{
	}

	UInt32::operator uint32_t() const
	{
		return Value;
	}

	UInt32::operator Object() const
	{
		return Object(Plugin::InternalUse::Only, Plugin::BoxUInt32(Value));
	}

	UInt32::operator ValueType() const
	{
		return ValueType(Plugin::InternalUse::Only, Plugin::BoxUInt32(Value));
	}

	UInt32::operator IComparable() const
	{
		return IComparable(Plugin::InternalUse::Only, Plugin::BoxUInt32(Value));
	}

	UInt32::operator IFormattable() const
	{
		return IFormattable(Plugin::InternalUse::Only, Plugin::BoxUInt32(Value));
	}

	UInt32::operator IConvertible() const
	{
		return IConvertible(Plugin::InternalUse::Only, Plugin::BoxUInt32(Value));
	}

	UInt32::operator IComparable_1<UInt32>() const
	{
		return IComparable_1<UInt32>(Plugin::InternalUse::Only, Plugin::BoxUInt32(Value));
	}

	UInt32::operator IEquatable_1<UInt32>() const
	{
		return IEquatable_1<UInt32>(Plugin::InternalUse::Only, Plugin::BoxUInt32(Value));
	}

	Int64::Int64()
		: Value(0)
	{
	}

	Int64::Int64(int64_t value)
		: Value(value)
	{
	}

	Int64::operator int64_t() const
	{
		return Value;
	}

	Int64::operator Object() const
	{
		return Object(Plugin::InternalUse::Only, Plugin::BoxInt64(Value));
	}

	Int64::operator ValueType() const
	{
		return ValueType(Plugin::InternalUse::Only, Plugin::BoxInt64(Value));
	}

	Int64::operator IComparable() const
	{
		return IComparable(Plugin::InternalUse::Only, Plugin::BoxInt64(Value));
	}

	Int64::operator IFormattable() const
	{
		return IFormattable(Plugin::InternalUse::Only, Plugin::BoxInt64(Value));
	}

	Int64::operator IConvertible() const
	{
		return IConvertible(Plugin::InternalUse::Only, Plugin::BoxInt64(Value));
	}

	Int64::operator IComparable_1<Int64>() const
	{
		return IComparable_1<Int64>(Plugin::InternalUse::Only, Plugin::BoxInt64(Value));
	}

	Int64::operator IEquatable_1<Int64>() const
	{
		return IEquatable_1<Int64>(Plugin::InternalUse::Only, Plugin::BoxInt64(Value));
	}

	UInt64::UInt64()
		: Value(0)
	{
	}

	UInt64::UInt64(uint64_t value)
		: Value(value)
	{
	}

	UInt64::operator uint64_t() const
	{
		return Value;
	}

	UInt64::operator Object() const
	{
		return Object(Plugin::InternalUse::Only, Plugin::BoxUInt64(Value));
	}

	UInt64::operator ValueType() const
	{
		return ValueType(Plugin::InternalUse::Only, Plugin::BoxUInt64(Value));
	}

	UInt64::operator IComparable() const
	{
		return IComparable(Plugin::InternalUse::Only, Plugin::BoxUInt64(Value));
	}

	UInt64::operator IFormattable() const
	{
		return IFormattable(Plugin::InternalUse::Only, Plugin::BoxUInt64(Value));
	}

	UInt64::operator IConvertible() const
	{
		return IConvertible(Plugin::InternalUse::Only, Plugin::BoxUInt64(Value));
	}

	UInt64::operator IComparable_1<UInt64>() const
	{
		return IComparable_1<UInt64>(Plugin::InternalUse::Only, Plugin::BoxUInt64(Value));
	}

	UInt64::operator IEquatable_1<UInt64>() const
	{
		return IEquatable_1<UInt64>(Plugin::InternalUse::Only, Plugin::BoxUInt64(Value));
	}

	Single::Single()
		: Value(0.0f)
	{
	}

	Single::Single(float value)
		: Value(value)
	{
	}

	Single::operator float() const
	{
		return Value;
	}

	Single::operator Object() const
	{
		return Object(Plugin::InternalUse::Only, Plugin::BoxSingle(Value));
	}

	Single::operator ValueType() const
	{
		return ValueType(Plugin::InternalUse::Only, Plugin::BoxSingle(Value));
	}

	Single::operator IComparable() const
	{
		return IComparable(Plugin::InternalUse::Only, Plugin::BoxSingle(Value));
	}

	Single::operator IFormattable() const
	{
		return IFormattable(Plugin::InternalUse::Only, Plugin::BoxSingle(Value));
	}

	Single::operator IConvertible() const
	{
		return IConvertible(Plugin::InternalUse::Only, Plugin::BoxSingle(Value));
	}

	Single::operator IComparable_1<Single>() const
	{
		return IComparable_1<Single>(Plugin::InternalUse::Only, Plugin::BoxSingle(Value));
	}

	Single::operator IEquatable_1<Single>() const
	{
		return IEquatable_1<Single>(Plugin::InternalUse::Only, Plugin::BoxSingle(Value));
	}

	Double::Double()
		: Value(0.0)
	{
	}

	Double::Double(double value)
		: Value(value)
	{
	}

	Double::operator double() const
	{
		return Value;
	}

	Double::operator Object() const
	{
		return Object(Plugin::InternalUse::Only, Plugin::BoxDouble(Value));
	}

	Double::operator ValueType() const
	{
		return ValueType(Plugin::InternalUse::Only, Plugin::BoxDouble(Value));
	}

	Double::operator IComparable() const
	{
		return IComparable(Plugin::InternalUse::Only, Plugin::BoxDouble(Value));
	}

	Double::operator IFormattable() const
	{
		return IFormattable(Plugin::InternalUse::Only, Plugin::BoxDouble(Value));
	}

	Double::operator IConvertible() const
	{
		return IConvertible(Plugin::InternalUse::Only, Plugin::BoxDouble(Value));
	}

	Double::operator IComparable_1<Double>() const
	{
		return IComparable_1<Double>(Plugin::InternalUse::Only, Plugin::BoxDouble(Value));
	}

	Double::operator IEquatable_1<Double>() const
	{
		return IEquatable_1<Double>(Plugin::InternalUse::Only, Plugin::BoxDouble(Value));
	}
}

////////////////////////////////////////////////////////////////
// Support for using IEnumerable with range for loops
////////////////////////////////////////////////////////////////

namespace Plugin
{
	// End iterators are dummies full of null
	EnumerableIterator::EnumerableIterator(decltype(nullptr))
		: enumerator(nullptr)
		, hasMore(false)
	{
	}

	// Begin iterators keep track of an IEnumerator
	EnumerableIterator::EnumerableIterator(
		System::Collections::IEnumerable& enumerable)
		: enumerator(enumerable.GetEnumerator())
	{
		hasMore = enumerator.MoveNext();
	}

	EnumerableIterator& EnumerableIterator::operator++()
	{
		hasMore = enumerator.MoveNext();
		return *this;
	}

	bool EnumerableIterator::operator!=(const EnumerableIterator& other)
	{
		return hasMore;
	}

	System::Object EnumerableIterator::operator*()
	{
		return enumerator.GetCurrent();
	}
}

////////////////////////////////////////////////////////////////
// User-defined literals for creating decimals (System.Decimal)
////////////////////////////////////////////////////////////////

System::Decimal operator"" _m(long double x)
{
	return System::Decimal((System::Double)x);
}

System::Decimal operator"" _m(unsigned long long x)
{
	return System::Decimal((System::UInt64)x);
}

////////////////////////////////////////////////////////////////
// Reference counting of managed objects
////////////////////////////////////////////////////////////////

namespace Plugin
{
	int32_t RefCountsLenClass;
	int32_t* RefCountsClass;

	void ReferenceManagedClass(int32_t handle)
	{
		assert(handle >= 0 && handle < RefCountsLenClass);
		if (handle != 0)
		{
			RefCountsClass[handle]++;
		}
	}

	void DereferenceManagedClass(int32_t handle)
	{
		assert(handle >= 0 && handle < RefCountsLenClass);
		if (handle != 0)
		{
			int32_t numRemain = --RefCountsClass[handle];
			if (numRemain == 0)
			{
				ReleaseObject(handle);
			}
		}
	}

	bool DereferenceManagedClassNoRelease(int32_t handle)
	{
		assert(handle >= 0 && handle < RefCountsLenClass);
		if (handle != 0)
		{
			int32_t numRemain = --RefCountsClass[handle];
			if (numRemain == 0)
			{
				return true;
			}
		}
		return false;
	}

	/*BEGIN GLOBAL STATE AND FUNCTIONS*/
	int32_t RefCountsLenSystemDecimal;
	int32_t* RefCountsSystemDecimal;
	
	void ReferenceManagedSystemDecimal(int32_t handle)
	{
		assert(handle >= 0 && handle < RefCountsLenSystemDecimal);
		if (handle != 0)
		{
			RefCountsSystemDecimal[handle]++;
		}
	}
	
	void DereferenceManagedSystemDecimal(int32_t handle)
	{
		assert(handle >= 0 && handle < RefCountsLenSystemDecimal);
		if (handle != 0)
		{
			int32_t numRemain = --RefCountsSystemDecimal[handle];
			if (numRemain == 0)
			{
				ReleaseSystemDecimal(handle);
			}
		}
	}
	
	// Free list for CesiumForUnity::BaseNativeDownloadHandler pointers
	
	int32_t BaseNativeDownloadHandlerFreeListSize;
	CesiumForUnity::BaseNativeDownloadHandler** BaseNativeDownloadHandlerFreeList;
	CesiumForUnity::BaseNativeDownloadHandler** NextFreeBaseNativeDownloadHandler;
	
	int32_t StoreBaseNativeDownloadHandler(CesiumForUnity::BaseNativeDownloadHandler* del)
	{
		assert(NextFreeBaseNativeDownloadHandler != nullptr);
		CesiumForUnity::BaseNativeDownloadHandler** pNext = NextFreeBaseNativeDownloadHandler;
		NextFreeBaseNativeDownloadHandler = (CesiumForUnity::BaseNativeDownloadHandler**)*pNext;
		*pNext = del;
		return (int32_t)(pNext - BaseNativeDownloadHandlerFreeList);
	}
	
	CesiumForUnity::BaseNativeDownloadHandler* GetBaseNativeDownloadHandler(int32_t handle)
	{
		assert(handle >= 0 && handle < BaseNativeDownloadHandlerFreeListSize);
		return BaseNativeDownloadHandlerFreeList[handle];
	}
	
	void RemoveBaseNativeDownloadHandler(int32_t handle)
	{
		CesiumForUnity::BaseNativeDownloadHandler** pRelease = BaseNativeDownloadHandlerFreeList + handle;
		*pRelease = (CesiumForUnity::BaseNativeDownloadHandler*)NextFreeBaseNativeDownloadHandler;
		NextFreeBaseNativeDownloadHandler = pRelease;
	}
	
	// Free list for whole CesiumForUnity::BaseNativeDownloadHandler objects
	
	union BaseNativeDownloadHandlerFreeWholeListEntry
	{
		BaseNativeDownloadHandlerFreeWholeListEntry* Next;
		CesiumForUnity::BaseNativeDownloadHandler Value;
	};
	int32_t BaseNativeDownloadHandlerFreeWholeListSize;
	BaseNativeDownloadHandlerFreeWholeListEntry* BaseNativeDownloadHandlerFreeWholeList;
	BaseNativeDownloadHandlerFreeWholeListEntry* NextFreeWholeBaseNativeDownloadHandler;
	
	CesiumForUnity::BaseNativeDownloadHandler* StoreWholeBaseNativeDownloadHandler()
	{
		assert(NextFreeWholeBaseNativeDownloadHandler != nullptr);
		BaseNativeDownloadHandlerFreeWholeListEntry* pNext = NextFreeWholeBaseNativeDownloadHandler;
		NextFreeWholeBaseNativeDownloadHandler = pNext->Next;
		return &pNext->Value;
	}
	
	void RemoveWholeBaseNativeDownloadHandler(CesiumForUnity::BaseNativeDownloadHandler* instance)
	{
		BaseNativeDownloadHandlerFreeWholeListEntry* pRelease = (BaseNativeDownloadHandlerFreeWholeListEntry*)instance;
		if (pRelease >= BaseNativeDownloadHandlerFreeWholeList && pRelease < BaseNativeDownloadHandlerFreeWholeList + (BaseNativeDownloadHandlerFreeWholeListSize - 1))
		{
			pRelease->Next = NextFreeWholeBaseNativeDownloadHandler;
			NextFreeWholeBaseNativeDownloadHandler = pRelease->Next;
		}
	}
	
	// Free list for CesiumForUnity::BaseCesium3DTileset pointers
	
	int32_t BaseCesium3DTilesetFreeListSize;
	CesiumForUnity::BaseCesium3DTileset** BaseCesium3DTilesetFreeList;
	CesiumForUnity::BaseCesium3DTileset** NextFreeBaseCesium3DTileset;
	
	int32_t StoreBaseCesium3DTileset(CesiumForUnity::BaseCesium3DTileset* del)
	{
		assert(NextFreeBaseCesium3DTileset != nullptr);
		CesiumForUnity::BaseCesium3DTileset** pNext = NextFreeBaseCesium3DTileset;
		NextFreeBaseCesium3DTileset = (CesiumForUnity::BaseCesium3DTileset**)*pNext;
		*pNext = del;
		return (int32_t)(pNext - BaseCesium3DTilesetFreeList);
	}
	
	CesiumForUnity::BaseCesium3DTileset* GetBaseCesium3DTileset(int32_t handle)
	{
		assert(handle >= 0 && handle < BaseCesium3DTilesetFreeListSize);
		return BaseCesium3DTilesetFreeList[handle];
	}
	
	void RemoveBaseCesium3DTileset(int32_t handle)
	{
		CesiumForUnity::BaseCesium3DTileset** pRelease = BaseCesium3DTilesetFreeList + handle;
		*pRelease = (CesiumForUnity::BaseCesium3DTileset*)NextFreeBaseCesium3DTileset;
		NextFreeBaseCesium3DTileset = pRelease;
	}
	
	// Free list for whole CesiumForUnity::BaseCesium3DTileset objects
	
	union BaseCesium3DTilesetFreeWholeListEntry
	{
		BaseCesium3DTilesetFreeWholeListEntry* Next;
		CesiumForUnity::BaseCesium3DTileset Value;
	};
	int32_t BaseCesium3DTilesetFreeWholeListSize;
	BaseCesium3DTilesetFreeWholeListEntry* BaseCesium3DTilesetFreeWholeList;
	BaseCesium3DTilesetFreeWholeListEntry* NextFreeWholeBaseCesium3DTileset;
	
	CesiumForUnity::BaseCesium3DTileset* StoreWholeBaseCesium3DTileset()
	{
		assert(NextFreeWholeBaseCesium3DTileset != nullptr);
		BaseCesium3DTilesetFreeWholeListEntry* pNext = NextFreeWholeBaseCesium3DTileset;
		NextFreeWholeBaseCesium3DTileset = pNext->Next;
		return &pNext->Value;
	}
	
	void RemoveWholeBaseCesium3DTileset(CesiumForUnity::BaseCesium3DTileset* instance)
	{
		BaseCesium3DTilesetFreeWholeListEntry* pRelease = (BaseCesium3DTilesetFreeWholeListEntry*)instance;
		if (pRelease >= BaseCesium3DTilesetFreeWholeList && pRelease < BaseCesium3DTilesetFreeWholeList + (BaseCesium3DTilesetFreeWholeListSize - 1))
		{
			pRelease->Next = NextFreeWholeBaseCesium3DTileset;
			NextFreeWholeBaseCesium3DTileset = pRelease->Next;
		}
	}
	
	// Free list for System::Action pointers
	
	int32_t SystemActionFreeListSize;
	System::Action** SystemActionFreeList;
	System::Action** NextFreeSystemAction;
	
	int32_t StoreSystemAction(System::Action* del)
	{
		assert(NextFreeSystemAction != nullptr);
		System::Action** pNext = NextFreeSystemAction;
		NextFreeSystemAction = (System::Action**)*pNext;
		*pNext = del;
		return (int32_t)(pNext - SystemActionFreeList);
	}
	
	System::Action* GetSystemAction(int32_t handle)
	{
		assert(handle >= 0 && handle < SystemActionFreeListSize);
		return SystemActionFreeList[handle];
	}
	
	void RemoveSystemAction(int32_t handle)
	{
		System::Action** pRelease = SystemActionFreeList + handle;
		*pRelease = (System::Action*)NextFreeSystemAction;
		NextFreeSystemAction = pRelease;
	}
	
	// Free list for System::Action_1<UnityEngine::AsyncOperation> pointers
	
	int32_t SystemActionUnityEngineAsyncOperationFreeListSize;
	System::Action_1<UnityEngine::AsyncOperation>** SystemActionUnityEngineAsyncOperationFreeList;
	System::Action_1<UnityEngine::AsyncOperation>** NextFreeSystemActionUnityEngineAsyncOperation;
	
	int32_t StoreSystemActionUnityEngineAsyncOperation(System::Action_1<UnityEngine::AsyncOperation>* del)
	{
		assert(NextFreeSystemActionUnityEngineAsyncOperation != nullptr);
		System::Action_1<UnityEngine::AsyncOperation>** pNext = NextFreeSystemActionUnityEngineAsyncOperation;
		NextFreeSystemActionUnityEngineAsyncOperation = (System::Action_1<UnityEngine::AsyncOperation>**)*pNext;
		*pNext = del;
		return (int32_t)(pNext - SystemActionUnityEngineAsyncOperationFreeList);
	}
	
	System::Action_1<UnityEngine::AsyncOperation>* GetSystemActionUnityEngineAsyncOperation(int32_t handle)
	{
		assert(handle >= 0 && handle < SystemActionUnityEngineAsyncOperationFreeListSize);
		return SystemActionUnityEngineAsyncOperationFreeList[handle];
	}
	
	void RemoveSystemActionUnityEngineAsyncOperation(int32_t handle)
	{
		System::Action_1<UnityEngine::AsyncOperation>** pRelease = SystemActionUnityEngineAsyncOperationFreeList + handle;
		*pRelease = (System::Action_1<UnityEngine::AsyncOperation>*)NextFreeSystemActionUnityEngineAsyncOperation;
		NextFreeSystemActionUnityEngineAsyncOperation = pRelease;
	}
	/*END GLOBAL STATE AND FUNCTIONS*/
}

namespace Plugin
{
	// An unhandled exception caused by C++ calling into C#
	System::Exception* unhandledCsharpException = nullptr;
}

////////////////////////////////////////////////////////////////
// Mirrors of C# types. These wrap the C# functions to present
// a similiar API as in C#.
////////////////////////////////////////////////////////////////

namespace System
{
	Object::Object()
		: Plugin::ManagedType(nullptr)
	{
	}

	Object::Object(Plugin::InternalUse iu, int32_t handle)
		: ManagedType(Plugin::InternalUse::Only, handle)
	{
	}

	Object::Object(decltype(nullptr))
		: ManagedType(nullptr)
	{
	}

	Object::~Object()
	{
	}

	bool Object::operator==(decltype(nullptr)) const
	{
		return Handle == 0;
	}

	bool Object::operator!=(decltype(nullptr)) const
	{
		return Handle != 0;
	}

	void Object::ThrowReferenceToThis()
	{
		throw *this;
	}

	ValueType::ValueType(Plugin::InternalUse iu, int32_t handle)
		: Object(iu, handle)
	{
	}

	ValueType::ValueType(decltype(nullptr))
		: Object(nullptr)
	{
	}

	Enum::Enum(Plugin::InternalUse iu, int32_t handle)
		: ValueType(iu, handle)
	{
	}

	Enum::Enum(decltype(nullptr))
		: ValueType(nullptr)
	{
	}

	String::String(decltype(nullptr))
		: Object(Plugin::InternalUse::Only, 0)
	{
	}

	String::String(Plugin::InternalUse iu, int32_t handle)
		: Object(iu, handle)
	{
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}

	String::String(const String& other)
		: Object(Plugin::InternalUse::Only, other.Handle)
	{
		if (Handle)
		{
			Plugin::ReferenceManagedClass(Handle);
		}
	}

	String::String(String&& other)
		: Object(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}

	String::~String()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}

	String& String::operator=(const String& other)
	{
		if (Handle != other.Handle)
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
			}
			Handle = other.Handle;
			if (Handle)
			{
				Plugin::ReferenceManagedClass(Handle);
			}
		}
		return *this;
	}

	String& String::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}

	String& String::operator=(String&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}

	String::String(const char* chars)
		: Object(Plugin::InternalUse::Only, Plugin::StringNew(chars))
	{
		Plugin::ReferenceManagedClass(Handle);
	}

	ICloneable::ICloneable(Plugin::InternalUse iu, int32_t handle)
		: Object(iu, handle)
	{
	}

	ICloneable::ICloneable(decltype(nullptr))
		: Object(nullptr)
	{
	}

	namespace Collections
	{
		IEnumerable::IEnumerable(Plugin::InternalUse iu, int32_t handle)
			: Object(iu, handle)
		{
		}

		IEnumerable::IEnumerable(decltype(nullptr))
			: Object(nullptr)
		{
		}

		IEnumerator IEnumerable::GetEnumerator()
		{
			return IEnumerator(
				Plugin::InternalUse::Only,
				Plugin::EnumerableGetEnumerator(Handle));
		}

		Plugin::EnumerableIterator begin(
			System::Collections::IEnumerable& enumerable)
		{
			return Plugin::EnumerableIterator(enumerable);
		}

		Plugin::EnumerableIterator end(
			System::Collections::IEnumerable& enumerable)
		{
			return Plugin::EnumerableIterator(nullptr);
		}

		ICollection::ICollection(Plugin::InternalUse iu, int32_t handle)
			: Object(iu, handle)
			, IEnumerable(nullptr)
		{
		}

		ICollection::ICollection(decltype(nullptr))
			: Object(nullptr)
			, IEnumerable(nullptr)
		{
		}

		IList::IList(Plugin::InternalUse iu, int32_t handle)
			: Object(iu, handle)
			, IEnumerable(nullptr)
			, ICollection(nullptr)
		{
		}

		IList::IList(decltype(nullptr))
			: Object(nullptr)
			, IEnumerable(nullptr)
			, ICollection(nullptr)
		{
		}
	}

	Array::Array(Plugin::InternalUse iu, int32_t handle)
		: Object(iu, handle)
		, ICloneable(nullptr)
		, Collections::IEnumerable(nullptr)
		, Collections::ICollection(nullptr)
		, Collections::IList(nullptr)
	{
	}

	Array::Array(decltype(nullptr))
		: Object(nullptr)
		, ICloneable(nullptr)
		, Collections::IEnumerable(nullptr)
		, Collections::ICollection(nullptr)
		, Collections::IList(nullptr)
	{
	}

	int32_t Array::GetLength()
	{
		return Plugin::ArrayGetLength(Handle);
	}

	int32_t Array::GetRank()
	{
		return 0;
	}
}

/*BEGIN METHOD DEFINITIONS*/
namespace System
{
	IFormattable::IFormattable(decltype(nullptr))
	{
	}
	
	IFormattable::IFormattable(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IFormattable::IFormattable(const IFormattable& other)
		: IFormattable(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IFormattable::IFormattable(IFormattable&& other)
		: IFormattable(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IFormattable::~IFormattable()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IFormattable& IFormattable::operator=(const IFormattable& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IFormattable& IFormattable::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IFormattable& IFormattable::operator=(IFormattable&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IFormattable::operator==(const IFormattable& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IFormattable::operator!=(const IFormattable& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IConvertible::IConvertible(decltype(nullptr))
	{
	}
	
	IConvertible::IConvertible(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IConvertible::IConvertible(const IConvertible& other)
		: IConvertible(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IConvertible::IConvertible(IConvertible&& other)
		: IConvertible(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IConvertible::~IConvertible()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IConvertible& IConvertible::operator=(const IConvertible& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IConvertible& IConvertible::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IConvertible& IConvertible::operator=(IConvertible&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IConvertible::operator==(const IConvertible& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IConvertible::operator!=(const IConvertible& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IComparable::IComparable(decltype(nullptr))
	{
	}
	
	IComparable::IComparable(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IComparable::IComparable(const IComparable& other)
		: IComparable(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IComparable::IComparable(IComparable&& other)
		: IComparable(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IComparable::~IComparable()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IComparable& IComparable::operator=(const IComparable& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IComparable& IComparable::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IComparable& IComparable::operator=(IComparable&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IComparable::operator==(const IComparable& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IComparable::operator!=(const IComparable& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	ISpanFormattable::ISpanFormattable(decltype(nullptr))
	{
	}
	
	ISpanFormattable::ISpanFormattable(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	ISpanFormattable::ISpanFormattable(const ISpanFormattable& other)
		: ISpanFormattable(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	ISpanFormattable::ISpanFormattable(ISpanFormattable&& other)
		: ISpanFormattable(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	ISpanFormattable::~ISpanFormattable()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	ISpanFormattable& ISpanFormattable::operator=(const ISpanFormattable& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	ISpanFormattable& ISpanFormattable::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	ISpanFormattable& ISpanFormattable::operator=(ISpanFormattable&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool ISpanFormattable::operator==(const ISpanFormattable& other) const
	{
		return Handle == other.Handle;
	}
	
	bool ISpanFormattable::operator!=(const ISpanFormattable& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IDisposable::IDisposable(decltype(nullptr))
	{
	}
	
	IDisposable::IDisposable(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IDisposable::IDisposable(const IDisposable& other)
		: IDisposable(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IDisposable::IDisposable(IDisposable&& other)
		: IDisposable(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IDisposable::~IDisposable()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IDisposable& IDisposable::operator=(const IDisposable& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IDisposable& IDisposable::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IDisposable& IDisposable::operator=(IDisposable&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IDisposable::operator==(const IDisposable& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IDisposable::operator!=(const IDisposable& other) const
	{
		return Handle != other.Handle;
	}
	
	void System::IDisposable::Dispose()
	{
		Plugin::SystemIDisposableMethodDispose(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
}

namespace System
{
	namespace Collections
	{
		IStructuralEquatable::IStructuralEquatable(decltype(nullptr))
		{
		}
		
		IStructuralEquatable::IStructuralEquatable(Plugin::InternalUse, int32_t handle)
		{
			Handle = handle;
			if (handle)
			{
				Plugin::ReferenceManagedClass(handle);
			}
		}
		
		IStructuralEquatable::IStructuralEquatable(const IStructuralEquatable& other)
			: IStructuralEquatable(Plugin::InternalUse::Only, other.Handle)
		{
		}
		
		IStructuralEquatable::IStructuralEquatable(IStructuralEquatable&& other)
			: IStructuralEquatable(Plugin::InternalUse::Only, other.Handle)
		{
			other.Handle = 0;
		}
		
		IStructuralEquatable::~IStructuralEquatable()
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
		}
		
		IStructuralEquatable& IStructuralEquatable::operator=(const IStructuralEquatable& other)
		{
			if (this->Handle)
			{
				Plugin::DereferenceManagedClass(this->Handle);
			}
			this->Handle = other.Handle;
			if (this->Handle)
			{
				Plugin::ReferenceManagedClass(this->Handle);
			}
			return *this;
		}
		
		IStructuralEquatable& IStructuralEquatable::operator=(decltype(nullptr))
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
			return *this;
		}
		
		IStructuralEquatable& IStructuralEquatable::operator=(IStructuralEquatable&& other)
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
			}
			Handle = other.Handle;
			other.Handle = 0;
			return *this;
		}
		
		bool IStructuralEquatable::operator==(const IStructuralEquatable& other) const
		{
			return Handle == other.Handle;
		}
		
		bool IStructuralEquatable::operator!=(const IStructuralEquatable& other) const
		{
			return Handle != other.Handle;
		}
	}
}

namespace System
{
	namespace Collections
	{
		IStructuralComparable::IStructuralComparable(decltype(nullptr))
		{
		}
		
		IStructuralComparable::IStructuralComparable(Plugin::InternalUse, int32_t handle)
		{
			Handle = handle;
			if (handle)
			{
				Plugin::ReferenceManagedClass(handle);
			}
		}
		
		IStructuralComparable::IStructuralComparable(const IStructuralComparable& other)
			: IStructuralComparable(Plugin::InternalUse::Only, other.Handle)
		{
		}
		
		IStructuralComparable::IStructuralComparable(IStructuralComparable&& other)
			: IStructuralComparable(Plugin::InternalUse::Only, other.Handle)
		{
			other.Handle = 0;
		}
		
		IStructuralComparable::~IStructuralComparable()
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
		}
		
		IStructuralComparable& IStructuralComparable::operator=(const IStructuralComparable& other)
		{
			if (this->Handle)
			{
				Plugin::DereferenceManagedClass(this->Handle);
			}
			this->Handle = other.Handle;
			if (this->Handle)
			{
				Plugin::ReferenceManagedClass(this->Handle);
			}
			return *this;
		}
		
		IStructuralComparable& IStructuralComparable::operator=(decltype(nullptr))
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
			return *this;
		}
		
		IStructuralComparable& IStructuralComparable::operator=(IStructuralComparable&& other)
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
			}
			Handle = other.Handle;
			other.Handle = 0;
			return *this;
		}
		
		bool IStructuralComparable::operator==(const IStructuralComparable& other) const
		{
			return Handle == other.Handle;
		}
		
		bool IStructuralComparable::operator!=(const IStructuralComparable& other) const
		{
			return Handle != other.Handle;
		}
	}
}

namespace System
{
	IEquatable_1<System::Boolean>::IEquatable_1(decltype(nullptr))
	{
	}
	
	IEquatable_1<System::Boolean>::IEquatable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IEquatable_1<System::Boolean>::IEquatable_1(const IEquatable_1<System::Boolean>& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IEquatable_1<System::Boolean>::IEquatable_1(IEquatable_1<System::Boolean>&& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IEquatable_1<System::Boolean>::~IEquatable_1<System::Boolean>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IEquatable_1<System::Boolean>& IEquatable_1<System::Boolean>::operator=(const IEquatable_1<System::Boolean>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IEquatable_1<System::Boolean>& IEquatable_1<System::Boolean>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IEquatable_1<System::Boolean>& IEquatable_1<System::Boolean>::operator=(IEquatable_1<System::Boolean>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IEquatable_1<System::Boolean>::operator==(const IEquatable_1<System::Boolean>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IEquatable_1<System::Boolean>::operator!=(const IEquatable_1<System::Boolean>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IEquatable_1<System::Char>::IEquatable_1(decltype(nullptr))
	{
	}
	
	IEquatable_1<System::Char>::IEquatable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IEquatable_1<System::Char>::IEquatable_1(const IEquatable_1<System::Char>& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IEquatable_1<System::Char>::IEquatable_1(IEquatable_1<System::Char>&& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IEquatable_1<System::Char>::~IEquatable_1<System::Char>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IEquatable_1<System::Char>& IEquatable_1<System::Char>::operator=(const IEquatable_1<System::Char>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IEquatable_1<System::Char>& IEquatable_1<System::Char>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IEquatable_1<System::Char>& IEquatable_1<System::Char>::operator=(IEquatable_1<System::Char>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IEquatable_1<System::Char>::operator==(const IEquatable_1<System::Char>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IEquatable_1<System::Char>::operator!=(const IEquatable_1<System::Char>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IEquatable_1<System::SByte>::IEquatable_1(decltype(nullptr))
	{
	}
	
	IEquatable_1<System::SByte>::IEquatable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IEquatable_1<System::SByte>::IEquatable_1(const IEquatable_1<System::SByte>& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IEquatable_1<System::SByte>::IEquatable_1(IEquatable_1<System::SByte>&& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IEquatable_1<System::SByte>::~IEquatable_1<System::SByte>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IEquatable_1<System::SByte>& IEquatable_1<System::SByte>::operator=(const IEquatable_1<System::SByte>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IEquatable_1<System::SByte>& IEquatable_1<System::SByte>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IEquatable_1<System::SByte>& IEquatable_1<System::SByte>::operator=(IEquatable_1<System::SByte>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IEquatable_1<System::SByte>::operator==(const IEquatable_1<System::SByte>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IEquatable_1<System::SByte>::operator!=(const IEquatable_1<System::SByte>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IEquatable_1<System::Byte>::IEquatable_1(decltype(nullptr))
	{
	}
	
	IEquatable_1<System::Byte>::IEquatable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IEquatable_1<System::Byte>::IEquatable_1(const IEquatable_1<System::Byte>& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IEquatable_1<System::Byte>::IEquatable_1(IEquatable_1<System::Byte>&& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IEquatable_1<System::Byte>::~IEquatable_1<System::Byte>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IEquatable_1<System::Byte>& IEquatable_1<System::Byte>::operator=(const IEquatable_1<System::Byte>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IEquatable_1<System::Byte>& IEquatable_1<System::Byte>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IEquatable_1<System::Byte>& IEquatable_1<System::Byte>::operator=(IEquatable_1<System::Byte>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IEquatable_1<System::Byte>::operator==(const IEquatable_1<System::Byte>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IEquatable_1<System::Byte>::operator!=(const IEquatable_1<System::Byte>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IEquatable_1<System::Int16>::IEquatable_1(decltype(nullptr))
	{
	}
	
	IEquatable_1<System::Int16>::IEquatable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IEquatable_1<System::Int16>::IEquatable_1(const IEquatable_1<System::Int16>& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IEquatable_1<System::Int16>::IEquatable_1(IEquatable_1<System::Int16>&& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IEquatable_1<System::Int16>::~IEquatable_1<System::Int16>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IEquatable_1<System::Int16>& IEquatable_1<System::Int16>::operator=(const IEquatable_1<System::Int16>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IEquatable_1<System::Int16>& IEquatable_1<System::Int16>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IEquatable_1<System::Int16>& IEquatable_1<System::Int16>::operator=(IEquatable_1<System::Int16>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IEquatable_1<System::Int16>::operator==(const IEquatable_1<System::Int16>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IEquatable_1<System::Int16>::operator!=(const IEquatable_1<System::Int16>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IEquatable_1<System::UInt16>::IEquatable_1(decltype(nullptr))
	{
	}
	
	IEquatable_1<System::UInt16>::IEquatable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IEquatable_1<System::UInt16>::IEquatable_1(const IEquatable_1<System::UInt16>& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IEquatable_1<System::UInt16>::IEquatable_1(IEquatable_1<System::UInt16>&& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IEquatable_1<System::UInt16>::~IEquatable_1<System::UInt16>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IEquatable_1<System::UInt16>& IEquatable_1<System::UInt16>::operator=(const IEquatable_1<System::UInt16>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IEquatable_1<System::UInt16>& IEquatable_1<System::UInt16>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IEquatable_1<System::UInt16>& IEquatable_1<System::UInt16>::operator=(IEquatable_1<System::UInt16>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IEquatable_1<System::UInt16>::operator==(const IEquatable_1<System::UInt16>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IEquatable_1<System::UInt16>::operator!=(const IEquatable_1<System::UInt16>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IEquatable_1<System::Int32>::IEquatable_1(decltype(nullptr))
	{
	}
	
	IEquatable_1<System::Int32>::IEquatable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IEquatable_1<System::Int32>::IEquatable_1(const IEquatable_1<System::Int32>& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IEquatable_1<System::Int32>::IEquatable_1(IEquatable_1<System::Int32>&& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IEquatable_1<System::Int32>::~IEquatable_1<System::Int32>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IEquatable_1<System::Int32>& IEquatable_1<System::Int32>::operator=(const IEquatable_1<System::Int32>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IEquatable_1<System::Int32>& IEquatable_1<System::Int32>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IEquatable_1<System::Int32>& IEquatable_1<System::Int32>::operator=(IEquatable_1<System::Int32>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IEquatable_1<System::Int32>::operator==(const IEquatable_1<System::Int32>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IEquatable_1<System::Int32>::operator!=(const IEquatable_1<System::Int32>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IEquatable_1<System::UInt32>::IEquatable_1(decltype(nullptr))
	{
	}
	
	IEquatable_1<System::UInt32>::IEquatable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IEquatable_1<System::UInt32>::IEquatable_1(const IEquatable_1<System::UInt32>& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IEquatable_1<System::UInt32>::IEquatable_1(IEquatable_1<System::UInt32>&& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IEquatable_1<System::UInt32>::~IEquatable_1<System::UInt32>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IEquatable_1<System::UInt32>& IEquatable_1<System::UInt32>::operator=(const IEquatable_1<System::UInt32>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IEquatable_1<System::UInt32>& IEquatable_1<System::UInt32>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IEquatable_1<System::UInt32>& IEquatable_1<System::UInt32>::operator=(IEquatable_1<System::UInt32>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IEquatable_1<System::UInt32>::operator==(const IEquatable_1<System::UInt32>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IEquatable_1<System::UInt32>::operator!=(const IEquatable_1<System::UInt32>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IEquatable_1<System::Int64>::IEquatable_1(decltype(nullptr))
	{
	}
	
	IEquatable_1<System::Int64>::IEquatable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IEquatable_1<System::Int64>::IEquatable_1(const IEquatable_1<System::Int64>& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IEquatable_1<System::Int64>::IEquatable_1(IEquatable_1<System::Int64>&& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IEquatable_1<System::Int64>::~IEquatable_1<System::Int64>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IEquatable_1<System::Int64>& IEquatable_1<System::Int64>::operator=(const IEquatable_1<System::Int64>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IEquatable_1<System::Int64>& IEquatable_1<System::Int64>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IEquatable_1<System::Int64>& IEquatable_1<System::Int64>::operator=(IEquatable_1<System::Int64>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IEquatable_1<System::Int64>::operator==(const IEquatable_1<System::Int64>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IEquatable_1<System::Int64>::operator!=(const IEquatable_1<System::Int64>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IEquatable_1<System::UInt64>::IEquatable_1(decltype(nullptr))
	{
	}
	
	IEquatable_1<System::UInt64>::IEquatable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IEquatable_1<System::UInt64>::IEquatable_1(const IEquatable_1<System::UInt64>& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IEquatable_1<System::UInt64>::IEquatable_1(IEquatable_1<System::UInt64>&& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IEquatable_1<System::UInt64>::~IEquatable_1<System::UInt64>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IEquatable_1<System::UInt64>& IEquatable_1<System::UInt64>::operator=(const IEquatable_1<System::UInt64>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IEquatable_1<System::UInt64>& IEquatable_1<System::UInt64>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IEquatable_1<System::UInt64>& IEquatable_1<System::UInt64>::operator=(IEquatable_1<System::UInt64>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IEquatable_1<System::UInt64>::operator==(const IEquatable_1<System::UInt64>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IEquatable_1<System::UInt64>::operator!=(const IEquatable_1<System::UInt64>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IEquatable_1<System::Single>::IEquatable_1(decltype(nullptr))
	{
	}
	
	IEquatable_1<System::Single>::IEquatable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IEquatable_1<System::Single>::IEquatable_1(const IEquatable_1<System::Single>& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IEquatable_1<System::Single>::IEquatable_1(IEquatable_1<System::Single>&& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IEquatable_1<System::Single>::~IEquatable_1<System::Single>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IEquatable_1<System::Single>& IEquatable_1<System::Single>::operator=(const IEquatable_1<System::Single>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IEquatable_1<System::Single>& IEquatable_1<System::Single>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IEquatable_1<System::Single>& IEquatable_1<System::Single>::operator=(IEquatable_1<System::Single>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IEquatable_1<System::Single>::operator==(const IEquatable_1<System::Single>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IEquatable_1<System::Single>::operator!=(const IEquatable_1<System::Single>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IEquatable_1<System::Double>::IEquatable_1(decltype(nullptr))
	{
	}
	
	IEquatable_1<System::Double>::IEquatable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IEquatable_1<System::Double>::IEquatable_1(const IEquatable_1<System::Double>& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IEquatable_1<System::Double>::IEquatable_1(IEquatable_1<System::Double>&& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IEquatable_1<System::Double>::~IEquatable_1<System::Double>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IEquatable_1<System::Double>& IEquatable_1<System::Double>::operator=(const IEquatable_1<System::Double>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IEquatable_1<System::Double>& IEquatable_1<System::Double>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IEquatable_1<System::Double>& IEquatable_1<System::Double>::operator=(IEquatable_1<System::Double>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IEquatable_1<System::Double>::operator==(const IEquatable_1<System::Double>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IEquatable_1<System::Double>::operator!=(const IEquatable_1<System::Double>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IEquatable_1<System::Decimal>::IEquatable_1(decltype(nullptr))
	{
	}
	
	IEquatable_1<System::Decimal>::IEquatable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IEquatable_1<System::Decimal>::IEquatable_1(const IEquatable_1<System::Decimal>& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IEquatable_1<System::Decimal>::IEquatable_1(IEquatable_1<System::Decimal>&& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IEquatable_1<System::Decimal>::~IEquatable_1<System::Decimal>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IEquatable_1<System::Decimal>& IEquatable_1<System::Decimal>::operator=(const IEquatable_1<System::Decimal>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IEquatable_1<System::Decimal>& IEquatable_1<System::Decimal>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IEquatable_1<System::Decimal>& IEquatable_1<System::Decimal>::operator=(IEquatable_1<System::Decimal>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IEquatable_1<System::Decimal>::operator==(const IEquatable_1<System::Decimal>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IEquatable_1<System::Decimal>::operator!=(const IEquatable_1<System::Decimal>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IEquatable_1<UnityEngine::Vector3>::IEquatable_1(decltype(nullptr))
	{
	}
	
	IEquatable_1<UnityEngine::Vector3>::IEquatable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IEquatable_1<UnityEngine::Vector3>::IEquatable_1(const IEquatable_1<UnityEngine::Vector3>& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IEquatable_1<UnityEngine::Vector3>::IEquatable_1(IEquatable_1<UnityEngine::Vector3>&& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IEquatable_1<UnityEngine::Vector3>::~IEquatable_1<UnityEngine::Vector3>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IEquatable_1<UnityEngine::Vector3>& IEquatable_1<UnityEngine::Vector3>::operator=(const IEquatable_1<UnityEngine::Vector3>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IEquatable_1<UnityEngine::Vector3>& IEquatable_1<UnityEngine::Vector3>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IEquatable_1<UnityEngine::Vector3>& IEquatable_1<UnityEngine::Vector3>::operator=(IEquatable_1<UnityEngine::Vector3>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IEquatable_1<UnityEngine::Vector3>::operator==(const IEquatable_1<UnityEngine::Vector3>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IEquatable_1<UnityEngine::Vector3>::operator!=(const IEquatable_1<UnityEngine::Vector3>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IEquatable_1<UnityEngine::Vector4>::IEquatable_1(decltype(nullptr))
	{
	}
	
	IEquatable_1<UnityEngine::Vector4>::IEquatable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IEquatable_1<UnityEngine::Vector4>::IEquatable_1(const IEquatable_1<UnityEngine::Vector4>& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IEquatable_1<UnityEngine::Vector4>::IEquatable_1(IEquatable_1<UnityEngine::Vector4>&& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IEquatable_1<UnityEngine::Vector4>::~IEquatable_1<UnityEngine::Vector4>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IEquatable_1<UnityEngine::Vector4>& IEquatable_1<UnityEngine::Vector4>::operator=(const IEquatable_1<UnityEngine::Vector4>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IEquatable_1<UnityEngine::Vector4>& IEquatable_1<UnityEngine::Vector4>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IEquatable_1<UnityEngine::Vector4>& IEquatable_1<UnityEngine::Vector4>::operator=(IEquatable_1<UnityEngine::Vector4>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IEquatable_1<UnityEngine::Vector4>::operator==(const IEquatable_1<UnityEngine::Vector4>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IEquatable_1<UnityEngine::Vector4>::operator!=(const IEquatable_1<UnityEngine::Vector4>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IEquatable_1<UnityEngine::Quaternion>::IEquatable_1(decltype(nullptr))
	{
	}
	
	IEquatable_1<UnityEngine::Quaternion>::IEquatable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IEquatable_1<UnityEngine::Quaternion>::IEquatable_1(const IEquatable_1<UnityEngine::Quaternion>& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IEquatable_1<UnityEngine::Quaternion>::IEquatable_1(IEquatable_1<UnityEngine::Quaternion>&& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IEquatable_1<UnityEngine::Quaternion>::~IEquatable_1<UnityEngine::Quaternion>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IEquatable_1<UnityEngine::Quaternion>& IEquatable_1<UnityEngine::Quaternion>::operator=(const IEquatable_1<UnityEngine::Quaternion>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IEquatable_1<UnityEngine::Quaternion>& IEquatable_1<UnityEngine::Quaternion>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IEquatable_1<UnityEngine::Quaternion>& IEquatable_1<UnityEngine::Quaternion>::operator=(IEquatable_1<UnityEngine::Quaternion>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IEquatable_1<UnityEngine::Quaternion>::operator==(const IEquatable_1<UnityEngine::Quaternion>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IEquatable_1<UnityEngine::Quaternion>::operator!=(const IEquatable_1<UnityEngine::Quaternion>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IEquatable_1<UnityEngine::Matrix4x4>::IEquatable_1(decltype(nullptr))
	{
	}
	
	IEquatable_1<UnityEngine::Matrix4x4>::IEquatable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IEquatable_1<UnityEngine::Matrix4x4>::IEquatable_1(const IEquatable_1<UnityEngine::Matrix4x4>& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IEquatable_1<UnityEngine::Matrix4x4>::IEquatable_1(IEquatable_1<UnityEngine::Matrix4x4>&& other)
		: IEquatable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IEquatable_1<UnityEngine::Matrix4x4>::~IEquatable_1<UnityEngine::Matrix4x4>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IEquatable_1<UnityEngine::Matrix4x4>& IEquatable_1<UnityEngine::Matrix4x4>::operator=(const IEquatable_1<UnityEngine::Matrix4x4>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IEquatable_1<UnityEngine::Matrix4x4>& IEquatable_1<UnityEngine::Matrix4x4>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IEquatable_1<UnityEngine::Matrix4x4>& IEquatable_1<UnityEngine::Matrix4x4>::operator=(IEquatable_1<UnityEngine::Matrix4x4>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IEquatable_1<UnityEngine::Matrix4x4>::operator==(const IEquatable_1<UnityEngine::Matrix4x4>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IEquatable_1<UnityEngine::Matrix4x4>::operator!=(const IEquatable_1<UnityEngine::Matrix4x4>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IComparable_1<System::Boolean>::IComparable_1(decltype(nullptr))
	{
	}
	
	IComparable_1<System::Boolean>::IComparable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IComparable_1<System::Boolean>::IComparable_1(const IComparable_1<System::Boolean>& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IComparable_1<System::Boolean>::IComparable_1(IComparable_1<System::Boolean>&& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IComparable_1<System::Boolean>::~IComparable_1<System::Boolean>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IComparable_1<System::Boolean>& IComparable_1<System::Boolean>::operator=(const IComparable_1<System::Boolean>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IComparable_1<System::Boolean>& IComparable_1<System::Boolean>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IComparable_1<System::Boolean>& IComparable_1<System::Boolean>::operator=(IComparable_1<System::Boolean>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IComparable_1<System::Boolean>::operator==(const IComparable_1<System::Boolean>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IComparable_1<System::Boolean>::operator!=(const IComparable_1<System::Boolean>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IComparable_1<System::Char>::IComparable_1(decltype(nullptr))
	{
	}
	
	IComparable_1<System::Char>::IComparable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IComparable_1<System::Char>::IComparable_1(const IComparable_1<System::Char>& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IComparable_1<System::Char>::IComparable_1(IComparable_1<System::Char>&& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IComparable_1<System::Char>::~IComparable_1<System::Char>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IComparable_1<System::Char>& IComparable_1<System::Char>::operator=(const IComparable_1<System::Char>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IComparable_1<System::Char>& IComparable_1<System::Char>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IComparable_1<System::Char>& IComparable_1<System::Char>::operator=(IComparable_1<System::Char>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IComparable_1<System::Char>::operator==(const IComparable_1<System::Char>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IComparable_1<System::Char>::operator!=(const IComparable_1<System::Char>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IComparable_1<System::SByte>::IComparable_1(decltype(nullptr))
	{
	}
	
	IComparable_1<System::SByte>::IComparable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IComparable_1<System::SByte>::IComparable_1(const IComparable_1<System::SByte>& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IComparable_1<System::SByte>::IComparable_1(IComparable_1<System::SByte>&& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IComparable_1<System::SByte>::~IComparable_1<System::SByte>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IComparable_1<System::SByte>& IComparable_1<System::SByte>::operator=(const IComparable_1<System::SByte>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IComparable_1<System::SByte>& IComparable_1<System::SByte>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IComparable_1<System::SByte>& IComparable_1<System::SByte>::operator=(IComparable_1<System::SByte>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IComparable_1<System::SByte>::operator==(const IComparable_1<System::SByte>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IComparable_1<System::SByte>::operator!=(const IComparable_1<System::SByte>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IComparable_1<System::Byte>::IComparable_1(decltype(nullptr))
	{
	}
	
	IComparable_1<System::Byte>::IComparable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IComparable_1<System::Byte>::IComparable_1(const IComparable_1<System::Byte>& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IComparable_1<System::Byte>::IComparable_1(IComparable_1<System::Byte>&& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IComparable_1<System::Byte>::~IComparable_1<System::Byte>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IComparable_1<System::Byte>& IComparable_1<System::Byte>::operator=(const IComparable_1<System::Byte>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IComparable_1<System::Byte>& IComparable_1<System::Byte>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IComparable_1<System::Byte>& IComparable_1<System::Byte>::operator=(IComparable_1<System::Byte>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IComparable_1<System::Byte>::operator==(const IComparable_1<System::Byte>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IComparable_1<System::Byte>::operator!=(const IComparable_1<System::Byte>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IComparable_1<System::Int16>::IComparable_1(decltype(nullptr))
	{
	}
	
	IComparable_1<System::Int16>::IComparable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IComparable_1<System::Int16>::IComparable_1(const IComparable_1<System::Int16>& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IComparable_1<System::Int16>::IComparable_1(IComparable_1<System::Int16>&& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IComparable_1<System::Int16>::~IComparable_1<System::Int16>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IComparable_1<System::Int16>& IComparable_1<System::Int16>::operator=(const IComparable_1<System::Int16>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IComparable_1<System::Int16>& IComparable_1<System::Int16>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IComparable_1<System::Int16>& IComparable_1<System::Int16>::operator=(IComparable_1<System::Int16>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IComparable_1<System::Int16>::operator==(const IComparable_1<System::Int16>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IComparable_1<System::Int16>::operator!=(const IComparable_1<System::Int16>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IComparable_1<System::UInt16>::IComparable_1(decltype(nullptr))
	{
	}
	
	IComparable_1<System::UInt16>::IComparable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IComparable_1<System::UInt16>::IComparable_1(const IComparable_1<System::UInt16>& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IComparable_1<System::UInt16>::IComparable_1(IComparable_1<System::UInt16>&& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IComparable_1<System::UInt16>::~IComparable_1<System::UInt16>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IComparable_1<System::UInt16>& IComparable_1<System::UInt16>::operator=(const IComparable_1<System::UInt16>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IComparable_1<System::UInt16>& IComparable_1<System::UInt16>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IComparable_1<System::UInt16>& IComparable_1<System::UInt16>::operator=(IComparable_1<System::UInt16>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IComparable_1<System::UInt16>::operator==(const IComparable_1<System::UInt16>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IComparable_1<System::UInt16>::operator!=(const IComparable_1<System::UInt16>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IComparable_1<System::Int32>::IComparable_1(decltype(nullptr))
	{
	}
	
	IComparable_1<System::Int32>::IComparable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IComparable_1<System::Int32>::IComparable_1(const IComparable_1<System::Int32>& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IComparable_1<System::Int32>::IComparable_1(IComparable_1<System::Int32>&& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IComparable_1<System::Int32>::~IComparable_1<System::Int32>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IComparable_1<System::Int32>& IComparable_1<System::Int32>::operator=(const IComparable_1<System::Int32>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IComparable_1<System::Int32>& IComparable_1<System::Int32>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IComparable_1<System::Int32>& IComparable_1<System::Int32>::operator=(IComparable_1<System::Int32>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IComparable_1<System::Int32>::operator==(const IComparable_1<System::Int32>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IComparable_1<System::Int32>::operator!=(const IComparable_1<System::Int32>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IComparable_1<System::UInt32>::IComparable_1(decltype(nullptr))
	{
	}
	
	IComparable_1<System::UInt32>::IComparable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IComparable_1<System::UInt32>::IComparable_1(const IComparable_1<System::UInt32>& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IComparable_1<System::UInt32>::IComparable_1(IComparable_1<System::UInt32>&& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IComparable_1<System::UInt32>::~IComparable_1<System::UInt32>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IComparable_1<System::UInt32>& IComparable_1<System::UInt32>::operator=(const IComparable_1<System::UInt32>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IComparable_1<System::UInt32>& IComparable_1<System::UInt32>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IComparable_1<System::UInt32>& IComparable_1<System::UInt32>::operator=(IComparable_1<System::UInt32>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IComparable_1<System::UInt32>::operator==(const IComparable_1<System::UInt32>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IComparable_1<System::UInt32>::operator!=(const IComparable_1<System::UInt32>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IComparable_1<System::Int64>::IComparable_1(decltype(nullptr))
	{
	}
	
	IComparable_1<System::Int64>::IComparable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IComparable_1<System::Int64>::IComparable_1(const IComparable_1<System::Int64>& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IComparable_1<System::Int64>::IComparable_1(IComparable_1<System::Int64>&& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IComparable_1<System::Int64>::~IComparable_1<System::Int64>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IComparable_1<System::Int64>& IComparable_1<System::Int64>::operator=(const IComparable_1<System::Int64>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IComparable_1<System::Int64>& IComparable_1<System::Int64>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IComparable_1<System::Int64>& IComparable_1<System::Int64>::operator=(IComparable_1<System::Int64>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IComparable_1<System::Int64>::operator==(const IComparable_1<System::Int64>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IComparable_1<System::Int64>::operator!=(const IComparable_1<System::Int64>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IComparable_1<System::UInt64>::IComparable_1(decltype(nullptr))
	{
	}
	
	IComparable_1<System::UInt64>::IComparable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IComparable_1<System::UInt64>::IComparable_1(const IComparable_1<System::UInt64>& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IComparable_1<System::UInt64>::IComparable_1(IComparable_1<System::UInt64>&& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IComparable_1<System::UInt64>::~IComparable_1<System::UInt64>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IComparable_1<System::UInt64>& IComparable_1<System::UInt64>::operator=(const IComparable_1<System::UInt64>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IComparable_1<System::UInt64>& IComparable_1<System::UInt64>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IComparable_1<System::UInt64>& IComparable_1<System::UInt64>::operator=(IComparable_1<System::UInt64>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IComparable_1<System::UInt64>::operator==(const IComparable_1<System::UInt64>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IComparable_1<System::UInt64>::operator!=(const IComparable_1<System::UInt64>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IComparable_1<System::Single>::IComparable_1(decltype(nullptr))
	{
	}
	
	IComparable_1<System::Single>::IComparable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IComparable_1<System::Single>::IComparable_1(const IComparable_1<System::Single>& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IComparable_1<System::Single>::IComparable_1(IComparable_1<System::Single>&& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IComparable_1<System::Single>::~IComparable_1<System::Single>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IComparable_1<System::Single>& IComparable_1<System::Single>::operator=(const IComparable_1<System::Single>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IComparable_1<System::Single>& IComparable_1<System::Single>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IComparable_1<System::Single>& IComparable_1<System::Single>::operator=(IComparable_1<System::Single>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IComparable_1<System::Single>::operator==(const IComparable_1<System::Single>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IComparable_1<System::Single>::operator!=(const IComparable_1<System::Single>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IComparable_1<System::Double>::IComparable_1(decltype(nullptr))
	{
	}
	
	IComparable_1<System::Double>::IComparable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IComparable_1<System::Double>::IComparable_1(const IComparable_1<System::Double>& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IComparable_1<System::Double>::IComparable_1(IComparable_1<System::Double>&& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IComparable_1<System::Double>::~IComparable_1<System::Double>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IComparable_1<System::Double>& IComparable_1<System::Double>::operator=(const IComparable_1<System::Double>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IComparable_1<System::Double>& IComparable_1<System::Double>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IComparable_1<System::Double>& IComparable_1<System::Double>::operator=(IComparable_1<System::Double>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IComparable_1<System::Double>::operator==(const IComparable_1<System::Double>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IComparable_1<System::Double>::operator!=(const IComparable_1<System::Double>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	IComparable_1<System::Decimal>::IComparable_1(decltype(nullptr))
	{
	}
	
	IComparable_1<System::Decimal>::IComparable_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IComparable_1<System::Decimal>::IComparable_1(const IComparable_1<System::Decimal>& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IComparable_1<System::Decimal>::IComparable_1(IComparable_1<System::Decimal>&& other)
		: IComparable_1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IComparable_1<System::Decimal>::~IComparable_1<System::Decimal>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IComparable_1<System::Decimal>& IComparable_1<System::Decimal>::operator=(const IComparable_1<System::Decimal>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IComparable_1<System::Decimal>& IComparable_1<System::Decimal>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IComparable_1<System::Decimal>& IComparable_1<System::Decimal>::operator=(IComparable_1<System::Decimal>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IComparable_1<System::Decimal>::operator==(const IComparable_1<System::Decimal>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IComparable_1<System::Decimal>::operator!=(const IComparable_1<System::Decimal>& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	namespace Runtime
	{
		namespace Serialization
		{
			IDeserializationCallback::IDeserializationCallback(decltype(nullptr))
			{
			}
			
			IDeserializationCallback::IDeserializationCallback(Plugin::InternalUse, int32_t handle)
			{
				Handle = handle;
				if (handle)
				{
					Plugin::ReferenceManagedClass(handle);
				}
			}
			
			IDeserializationCallback::IDeserializationCallback(const IDeserializationCallback& other)
				: IDeserializationCallback(Plugin::InternalUse::Only, other.Handle)
			{
			}
			
			IDeserializationCallback::IDeserializationCallback(IDeserializationCallback&& other)
				: IDeserializationCallback(Plugin::InternalUse::Only, other.Handle)
			{
				other.Handle = 0;
			}
			
			IDeserializationCallback::~IDeserializationCallback()
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
			}
			
			IDeserializationCallback& IDeserializationCallback::operator=(const IDeserializationCallback& other)
			{
				if (this->Handle)
				{
					Plugin::DereferenceManagedClass(this->Handle);
				}
				this->Handle = other.Handle;
				if (this->Handle)
				{
					Plugin::ReferenceManagedClass(this->Handle);
				}
				return *this;
			}
			
			IDeserializationCallback& IDeserializationCallback::operator=(decltype(nullptr))
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
				return *this;
			}
			
			IDeserializationCallback& IDeserializationCallback::operator=(IDeserializationCallback&& other)
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
				}
				Handle = other.Handle;
				other.Handle = 0;
				return *this;
			}
			
			bool IDeserializationCallback::operator==(const IDeserializationCallback& other) const
			{
				return Handle == other.Handle;
			}
			
			bool IDeserializationCallback::operator!=(const IDeserializationCallback& other) const
			{
				return Handle != other.Handle;
			}
		}
	}
}

namespace System
{
	Decimal::Decimal(decltype(nullptr))
	{
	}
	
	Decimal::Decimal(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedSystemDecimal(Handle);
		}
	}
	
	Decimal::Decimal(const Decimal& other)
		: Decimal(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	Decimal::Decimal(Decimal&& other)
		: Decimal(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	Decimal::~Decimal()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedSystemDecimal(Handle);
			Handle = 0;
		}
	}
	
	Decimal& Decimal::operator=(const Decimal& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedSystemDecimal(Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedSystemDecimal(Handle);
		}
		return *this;
	}
	
	Decimal& Decimal::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedSystemDecimal(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	Decimal& Decimal::operator=(Decimal&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedSystemDecimal(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool Decimal::operator==(const Decimal& other) const
	{
		return Handle == other.Handle;
	}
	
	bool Decimal::operator!=(const Decimal& other) const
	{
		return Handle != other.Handle;
	}
	
	System::Decimal::Decimal(System::Double value)
	{
		auto returnValue = Plugin::SystemDecimalConstructorSystemDouble(value);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		Handle = returnValue;
		if (returnValue)
		{
			Plugin::ReferenceManagedSystemDecimal(Handle);
		}
	}
	
	System::Decimal::Decimal(System::UInt64 value)
	{
		auto returnValue = Plugin::SystemDecimalConstructorSystemUInt64(value);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		Handle = returnValue;
		if (returnValue)
		{
			Plugin::ReferenceManagedSystemDecimal(Handle);
		}
	}
	
	System::Decimal::operator System::ValueType()
	{
		int32_t handle = Plugin::BoxDecimal(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::ValueType(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	System::Decimal::operator System::Object()
	{
		int32_t handle = Plugin::BoxDecimal(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::Object(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	System::Decimal::operator System::Runtime::Serialization::IDeserializationCallback()
	{
		int32_t handle = Plugin::BoxDecimal(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::Runtime::Serialization::IDeserializationCallback(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	System::Decimal::operator System::IFormattable()
	{
		int32_t handle = Plugin::BoxDecimal(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::IFormattable(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	System::Decimal::operator System::ISpanFormattable()
	{
		int32_t handle = Plugin::BoxDecimal(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::ISpanFormattable(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	System::Decimal::operator System::IComparable()
	{
		int32_t handle = Plugin::BoxDecimal(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::IComparable(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	System::Decimal::operator System::IComparable_1<System::Decimal>()
	{
		int32_t handle = Plugin::BoxDecimal(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::IComparable_1<System::Decimal>(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	System::Decimal::operator System::IConvertible()
	{
		int32_t handle = Plugin::BoxDecimal(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::IConvertible(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	System::Decimal::operator System::IEquatable_1<System::Decimal>()
	{
		int32_t handle = Plugin::BoxDecimal(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::IEquatable_1<System::Decimal>(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
}

namespace System
{
	System::Object::operator System::Decimal()
	{
		System::Decimal returnVal(Plugin::InternalUse::Only, Plugin::UnboxDecimal(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace UnityEngine
{
	Vector3::Vector3()
	{
	}
	
	UnityEngine::Vector3::Vector3(System::Single x, System::Single y, System::Single z)
	{
		auto returnValue = Plugin::UnityEngineVector3ConstructorSystemSingle_SystemSingle_SystemSingle(x, y, z);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		*this = returnValue;
	}
	
	UnityEngine::Vector3::operator System::ValueType()
	{
		int32_t handle = Plugin::BoxVector3(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::ValueType(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	UnityEngine::Vector3::operator System::Object()
	{
		int32_t handle = Plugin::BoxVector3(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::Object(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	UnityEngine::Vector3::operator System::IFormattable()
	{
		int32_t handle = Plugin::BoxVector3(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::IFormattable(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	UnityEngine::Vector3::operator System::IEquatable_1<UnityEngine::Vector3>()
	{
		int32_t handle = Plugin::BoxVector3(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::IEquatable_1<UnityEngine::Vector3>(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
}

namespace System
{
	System::Object::operator UnityEngine::Vector3()
	{
		UnityEngine::Vector3 returnVal(Plugin::UnboxVector3(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace UnityEngine
{
	Vector4::Vector4()
	{
	}
	
	UnityEngine::Vector4::Vector4(System::Single x, System::Single y, System::Single z, System::Single w)
	{
		auto returnValue = Plugin::UnityEngineVector4ConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingle(x, y, z, w);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		*this = returnValue;
	}
	
	UnityEngine::Vector4::operator System::ValueType()
	{
		int32_t handle = Plugin::BoxVector4(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::ValueType(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	UnityEngine::Vector4::operator System::Object()
	{
		int32_t handle = Plugin::BoxVector4(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::Object(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	UnityEngine::Vector4::operator System::IFormattable()
	{
		int32_t handle = Plugin::BoxVector4(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::IFormattable(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	UnityEngine::Vector4::operator System::IEquatable_1<UnityEngine::Vector4>()
	{
		int32_t handle = Plugin::BoxVector4(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::IEquatable_1<UnityEngine::Vector4>(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
}

namespace System
{
	System::Object::operator UnityEngine::Vector4()
	{
		UnityEngine::Vector4 returnVal(Plugin::UnboxVector4(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace UnityEngine
{
	Quaternion::Quaternion()
	{
	}
	
	UnityEngine::Quaternion::Quaternion(System::Single x, System::Single y, System::Single z, System::Single w)
	{
		auto returnValue = Plugin::UnityEngineQuaternionConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingle(x, y, z, w);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		*this = returnValue;
	}
	
	UnityEngine::Quaternion::operator System::ValueType()
	{
		int32_t handle = Plugin::BoxQuaternion(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::ValueType(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	UnityEngine::Quaternion::operator System::Object()
	{
		int32_t handle = Plugin::BoxQuaternion(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::Object(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	UnityEngine::Quaternion::operator System::IFormattable()
	{
		int32_t handle = Plugin::BoxQuaternion(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::IFormattable(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	UnityEngine::Quaternion::operator System::IEquatable_1<UnityEngine::Quaternion>()
	{
		int32_t handle = Plugin::BoxQuaternion(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::IEquatable_1<UnityEngine::Quaternion>(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
}

namespace System
{
	System::Object::operator UnityEngine::Quaternion()
	{
		UnityEngine::Quaternion returnVal(Plugin::UnboxQuaternion(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace UnityEngine
{
	Matrix4x4::Matrix4x4()
	{
	}
	
	UnityEngine::Matrix4x4::Matrix4x4(UnityEngine::Vector4& column0, UnityEngine::Vector4& column1, UnityEngine::Vector4& column2, UnityEngine::Vector4& column3)
	{
		auto returnValue = Plugin::UnityEngineMatrix4x4ConstructorUnityEngineVector4_UnityEngineVector4_UnityEngineVector4_UnityEngineVector4(column0, column1, column2, column3);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		*this = returnValue;
	}
	
	UnityEngine::Quaternion UnityEngine::Matrix4x4::GetRotation()
	{
		auto returnValue = Plugin::UnityEngineMatrix4x4PropertyGetRotation(this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnValue;
	}
	
	UnityEngine::Vector3 UnityEngine::Matrix4x4::GetLossyScale()
	{
		auto returnValue = Plugin::UnityEngineMatrix4x4PropertyGetLossyScale(this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnValue;
	}
	
	UnityEngine::Vector3 UnityEngine::Matrix4x4::GetPosition()
	{
		auto returnValue = Plugin::UnityEngineMatrix4x4MethodGetPosition(this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnValue;
	}
	
	UnityEngine::Matrix4x4::operator System::ValueType()
	{
		int32_t handle = Plugin::BoxMatrix4x4(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::ValueType(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	UnityEngine::Matrix4x4::operator System::Object()
	{
		int32_t handle = Plugin::BoxMatrix4x4(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::Object(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	UnityEngine::Matrix4x4::operator System::IFormattable()
	{
		int32_t handle = Plugin::BoxMatrix4x4(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::IFormattable(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	UnityEngine::Matrix4x4::operator System::IEquatable_1<UnityEngine::Matrix4x4>()
	{
		int32_t handle = Plugin::BoxMatrix4x4(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::IEquatable_1<UnityEngine::Matrix4x4>(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
}

namespace System
{
	System::Object::operator UnityEngine::Matrix4x4()
	{
		UnityEngine::Matrix4x4 returnVal(Plugin::UnboxMatrix4x4(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace UnityEngine
{
	Object::Object(decltype(nullptr))
	{
	}
	
	Object::Object(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	Object::Object(const Object& other)
		: Object(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	Object::Object(Object&& other)
		: Object(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	Object::~Object()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	Object& Object::operator=(const Object& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	Object& Object::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	Object& Object::operator=(Object&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool Object::operator==(const Object& other) const
	{
		return Handle == other.Handle;
	}
	
	bool Object::operator!=(const Object& other) const
	{
		return Handle != other.Handle;
	}
	
	System::String UnityEngine::Object::GetName()
	{
		auto returnValue = Plugin::UnityEngineObjectPropertyGetName(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return System::String(Plugin::InternalUse::Only, returnValue);
	}
	
	void UnityEngine::Object::SetName(System::String& value)
	{
		Plugin::UnityEngineObjectPropertySetName(Handle, value.Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
}

namespace UnityEngine
{
	Component::Component(decltype(nullptr))
		: UnityEngine::Object(nullptr)
	{
	}
	
	Component::Component(Plugin::InternalUse, int32_t handle)
		: UnityEngine::Object(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	Component::Component(const Component& other)
		: Component(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	Component::Component(Component&& other)
		: Component(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	Component::~Component()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	Component& Component::operator=(const Component& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	Component& Component::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	Component& Component::operator=(Component&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool Component::operator==(const Component& other) const
	{
		return Handle == other.Handle;
	}
	
	bool Component::operator!=(const Component& other) const
	{
		return Handle != other.Handle;
	}
	
	UnityEngine::Transform UnityEngine::Component::GetTransform()
	{
		auto returnValue = Plugin::UnityEngineComponentPropertyGetTransform(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return UnityEngine::Transform(Plugin::InternalUse::Only, returnValue);
	}
}

namespace UnityEngine
{
	Transform::Transform(decltype(nullptr))
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
		, System::Collections::IEnumerable(nullptr)
	{
	}
	
	Transform::Transform(Plugin::InternalUse, int32_t handle)
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
		, System::Collections::IEnumerable(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	Transform::Transform(const Transform& other)
		: Transform(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	Transform::Transform(Transform&& other)
		: Transform(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	Transform::~Transform()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	Transform& Transform::operator=(const Transform& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	Transform& Transform::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	Transform& Transform::operator=(Transform&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool Transform::operator==(const Transform& other) const
	{
		return Handle == other.Handle;
	}
	
	bool Transform::operator!=(const Transform& other) const
	{
		return Handle != other.Handle;
	}
	
	UnityEngine::Vector3 UnityEngine::Transform::GetPosition()
	{
		auto returnValue = Plugin::UnityEngineTransformPropertyGetPosition(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnValue;
	}
	
	void UnityEngine::Transform::SetPosition(UnityEngine::Vector3& value)
	{
		Plugin::UnityEngineTransformPropertySetPosition(Handle, value);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	UnityEngine::Quaternion UnityEngine::Transform::GetRotation()
	{
		auto returnValue = Plugin::UnityEngineTransformPropertyGetRotation(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnValue;
	}
	
	void UnityEngine::Transform::SetRotation(UnityEngine::Quaternion& value)
	{
		Plugin::UnityEngineTransformPropertySetRotation(Handle, value);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	UnityEngine::Vector3 UnityEngine::Transform::GetLocalScale()
	{
		auto returnValue = Plugin::UnityEngineTransformPropertyGetLocalScale(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnValue;
	}
	
	void UnityEngine::Transform::SetLocalScale(UnityEngine::Vector3& value)
	{
		Plugin::UnityEngineTransformPropertySetLocalScale(Handle, value);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	UnityEngine::Matrix4x4 UnityEngine::Transform::GetLocalToWorldMatrix()
	{
		auto returnValue = Plugin::UnityEngineTransformPropertyGetLocalToWorldMatrix(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnValue;
	}
	
	UnityEngine::Transform UnityEngine::Transform::GetParent()
	{
		auto returnValue = Plugin::UnityEngineTransformPropertyGetParent(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return UnityEngine::Transform(Plugin::InternalUse::Only, returnValue);
	}
	
	void UnityEngine::Transform::SetParent(UnityEngine::Transform& value)
	{
		Plugin::UnityEngineTransformPropertySetParent(Handle, value.Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	UnityEngine::Vector3 UnityEngine::Transform::GetForward()
	{
		auto returnValue = Plugin::UnityEngineTransformPropertyGetForward(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnValue;
	}
	
	void UnityEngine::Transform::SetForward(UnityEngine::Vector3& value)
	{
		Plugin::UnityEngineTransformPropertySetForward(Handle, value);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	UnityEngine::Vector3 UnityEngine::Transform::GetUp()
	{
		auto returnValue = Plugin::UnityEngineTransformPropertyGetUp(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnValue;
	}
	
	void UnityEngine::Transform::SetUp(UnityEngine::Vector3& value)
	{
		Plugin::UnityEngineTransformPropertySetUp(Handle, value);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
}

namespace System
{
	namespace Collections
	{
		IEnumerator::IEnumerator(decltype(nullptr))
		{
		}
		
		IEnumerator::IEnumerator(Plugin::InternalUse, int32_t handle)
		{
			Handle = handle;
			if (handle)
			{
				Plugin::ReferenceManagedClass(handle);
			}
		}
		
		IEnumerator::IEnumerator(const IEnumerator& other)
			: IEnumerator(Plugin::InternalUse::Only, other.Handle)
		{
		}
		
		IEnumerator::IEnumerator(IEnumerator&& other)
			: IEnumerator(Plugin::InternalUse::Only, other.Handle)
		{
			other.Handle = 0;
		}
		
		IEnumerator::~IEnumerator()
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
		}
		
		IEnumerator& IEnumerator::operator=(const IEnumerator& other)
		{
			if (this->Handle)
			{
				Plugin::DereferenceManagedClass(this->Handle);
			}
			this->Handle = other.Handle;
			if (this->Handle)
			{
				Plugin::ReferenceManagedClass(this->Handle);
			}
			return *this;
		}
		
		IEnumerator& IEnumerator::operator=(decltype(nullptr))
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
			return *this;
		}
		
		IEnumerator& IEnumerator::operator=(IEnumerator&& other)
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
			}
			Handle = other.Handle;
			other.Handle = 0;
			return *this;
		}
		
		bool IEnumerator::operator==(const IEnumerator& other) const
		{
			return Handle == other.Handle;
		}
		
		bool IEnumerator::operator!=(const IEnumerator& other) const
		{
			return Handle != other.Handle;
		}
		
		System::Object System::Collections::IEnumerator::GetCurrent()
		{
			auto returnValue = Plugin::SystemCollectionsIEnumeratorPropertyGetCurrent(Handle);
			if (Plugin::unhandledCsharpException)
			{
				System::Exception* ex = Plugin::unhandledCsharpException;
				Plugin::unhandledCsharpException = nullptr;
				ex->ThrowReferenceToThis();
				delete ex;
			}
			return System::Object(Plugin::InternalUse::Only, returnValue);
		}
		
		System::Boolean System::Collections::IEnumerator::MoveNext()
		{
			auto returnValue = Plugin::SystemCollectionsIEnumeratorMethodMoveNext(Handle);
			if (Plugin::unhandledCsharpException)
			{
				System::Exception* ex = Plugin::unhandledCsharpException;
				Plugin::unhandledCsharpException = nullptr;
				ex->ThrowReferenceToThis();
				delete ex;
			}
			return returnValue;
		}
	}
}

namespace System
{
	namespace Runtime
	{
		namespace Serialization
		{
			ISerializable::ISerializable(decltype(nullptr))
			{
			}
			
			ISerializable::ISerializable(Plugin::InternalUse, int32_t handle)
			{
				Handle = handle;
				if (handle)
				{
					Plugin::ReferenceManagedClass(handle);
				}
			}
			
			ISerializable::ISerializable(const ISerializable& other)
				: ISerializable(Plugin::InternalUse::Only, other.Handle)
			{
			}
			
			ISerializable::ISerializable(ISerializable&& other)
				: ISerializable(Plugin::InternalUse::Only, other.Handle)
			{
				other.Handle = 0;
			}
			
			ISerializable::~ISerializable()
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
			}
			
			ISerializable& ISerializable::operator=(const ISerializable& other)
			{
				if (this->Handle)
				{
					Plugin::DereferenceManagedClass(this->Handle);
				}
				this->Handle = other.Handle;
				if (this->Handle)
				{
					Plugin::ReferenceManagedClass(this->Handle);
				}
				return *this;
			}
			
			ISerializable& ISerializable::operator=(decltype(nullptr))
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
				return *this;
			}
			
			ISerializable& ISerializable::operator=(ISerializable&& other)
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
				}
				Handle = other.Handle;
				other.Handle = 0;
				return *this;
			}
			
			bool ISerializable::operator==(const ISerializable& other) const
			{
				return Handle == other.Handle;
			}
			
			bool ISerializable::operator!=(const ISerializable& other) const
			{
				return Handle != other.Handle;
			}
		}
	}
}

namespace System
{
	namespace Runtime
	{
		namespace InteropServices
		{
			_Exception::_Exception(decltype(nullptr))
			{
			}
			
			_Exception::_Exception(Plugin::InternalUse, int32_t handle)
			{
				Handle = handle;
				if (handle)
				{
					Plugin::ReferenceManagedClass(handle);
				}
			}
			
			_Exception::_Exception(const _Exception& other)
				: _Exception(Plugin::InternalUse::Only, other.Handle)
			{
			}
			
			_Exception::_Exception(_Exception&& other)
				: _Exception(Plugin::InternalUse::Only, other.Handle)
			{
				other.Handle = 0;
			}
			
			_Exception::~_Exception()
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
			}
			
			_Exception& _Exception::operator=(const _Exception& other)
			{
				if (this->Handle)
				{
					Plugin::DereferenceManagedClass(this->Handle);
				}
				this->Handle = other.Handle;
				if (this->Handle)
				{
					Plugin::ReferenceManagedClass(this->Handle);
				}
				return *this;
			}
			
			_Exception& _Exception::operator=(decltype(nullptr))
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
				return *this;
			}
			
			_Exception& _Exception::operator=(_Exception&& other)
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
				}
				Handle = other.Handle;
				other.Handle = 0;
				return *this;
			}
			
			bool _Exception::operator==(const _Exception& other) const
			{
				return Handle == other.Handle;
			}
			
			bool _Exception::operator!=(const _Exception& other) const
			{
				return Handle != other.Handle;
			}
		}
	}
}

namespace UnityEngine
{
	GameObject::GameObject(decltype(nullptr))
		: UnityEngine::Object(nullptr)
	{
	}
	
	GameObject::GameObject(Plugin::InternalUse, int32_t handle)
		: UnityEngine::Object(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	GameObject::GameObject(const GameObject& other)
		: GameObject(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	GameObject::GameObject(GameObject&& other)
		: GameObject(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	GameObject::~GameObject()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	GameObject& GameObject::operator=(const GameObject& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	GameObject& GameObject::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	GameObject& GameObject::operator=(GameObject&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool GameObject::operator==(const GameObject& other) const
	{
		return Handle == other.Handle;
	}
	
	bool GameObject::operator!=(const GameObject& other) const
	{
		return Handle != other.Handle;
	}
	
	UnityEngine::GameObject::GameObject(System::String& name)
		: UnityEngine::Object(nullptr)
	{
		auto returnValue = Plugin::UnityEngineGameObjectConstructorSystemString(name.Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		Handle = returnValue;
		if (returnValue)
		{
			Plugin::ReferenceManagedClass(returnValue);
		}
	}
	
	UnityEngine::Transform UnityEngine::GameObject::GetTransform()
	{
		auto returnValue = Plugin::UnityEngineGameObjectPropertyGetTransform(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return UnityEngine::Transform(Plugin::InternalUse::Only, returnValue);
	}
	
	template<> CesiumForUnity::BaseCesium3DTileset UnityEngine::GameObject::AddComponent<CesiumForUnity::BaseCesium3DTileset>()
	{
		auto returnValue = Plugin::UnityEngineGameObjectMethodAddComponentCesiumForUnityBaseCesium3DTileset(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return CesiumForUnity::BaseCesium3DTileset(Plugin::InternalUse::Only, returnValue);
	}
	
	template<> UnityEngine::MeshFilter UnityEngine::GameObject::AddComponent<UnityEngine::MeshFilter>()
	{
		auto returnValue = Plugin::UnityEngineGameObjectMethodAddComponentUnityEngineMeshFilter(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return UnityEngine::MeshFilter(Plugin::InternalUse::Only, returnValue);
	}
	
	template<> UnityEngine::MeshRenderer UnityEngine::GameObject::AddComponent<UnityEngine::MeshRenderer>()
	{
		auto returnValue = Plugin::UnityEngineGameObjectMethodAddComponentUnityEngineMeshRenderer(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return UnityEngine::MeshRenderer(Plugin::InternalUse::Only, returnValue);
	}
	
	UnityEngine::GameObject UnityEngine::GameObject::CreatePrimitive(UnityEngine::PrimitiveType type)
	{
		auto returnValue = Plugin::UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveType(type);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return UnityEngine::GameObject(Plugin::InternalUse::Only, returnValue);
	}
	
	void UnityEngine::GameObject::SetActive(System::Boolean value)
	{
		Plugin::UnityEngineGameObjectMethodSetActiveSystemBoolean(Handle, value);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
}

namespace UnityEngine
{
	Debug::Debug(decltype(nullptr))
	{
	}
	
	Debug::Debug(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	Debug::Debug(const Debug& other)
		: Debug(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	Debug::Debug(Debug&& other)
		: Debug(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	Debug::~Debug()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	Debug& Debug::operator=(const Debug& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	Debug& Debug::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	Debug& Debug::operator=(Debug&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool Debug::operator==(const Debug& other) const
	{
		return Handle == other.Handle;
	}
	
	bool Debug::operator!=(const Debug& other) const
	{
		return Handle != other.Handle;
	}
	
	void UnityEngine::Debug::Log(System::Object& message)
	{
		Plugin::UnityEngineDebugMethodLogSystemObject(message.Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
}

namespace UnityEngine
{
	Behaviour::Behaviour(decltype(nullptr))
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
	{
	}
	
	Behaviour::Behaviour(Plugin::InternalUse, int32_t handle)
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	Behaviour::Behaviour(const Behaviour& other)
		: Behaviour(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	Behaviour::Behaviour(Behaviour&& other)
		: Behaviour(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	Behaviour::~Behaviour()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	Behaviour& Behaviour::operator=(const Behaviour& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	Behaviour& Behaviour::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	Behaviour& Behaviour::operator=(Behaviour&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool Behaviour::operator==(const Behaviour& other) const
	{
		return Handle == other.Handle;
	}
	
	bool Behaviour::operator!=(const Behaviour& other) const
	{
		return Handle != other.Handle;
	}
}

namespace UnityEngine
{
	MonoBehaviour::MonoBehaviour(decltype(nullptr))
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
		, UnityEngine::Behaviour(nullptr)
	{
	}
	
	MonoBehaviour::MonoBehaviour(Plugin::InternalUse, int32_t handle)
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
		, UnityEngine::Behaviour(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	MonoBehaviour::MonoBehaviour(const MonoBehaviour& other)
		: MonoBehaviour(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	MonoBehaviour::MonoBehaviour(MonoBehaviour&& other)
		: MonoBehaviour(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	MonoBehaviour::~MonoBehaviour()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	MonoBehaviour& MonoBehaviour::operator=(const MonoBehaviour& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	MonoBehaviour& MonoBehaviour::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	MonoBehaviour& MonoBehaviour::operator=(MonoBehaviour&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool MonoBehaviour::operator==(const MonoBehaviour& other) const
	{
		return Handle == other.Handle;
	}
	
	bool MonoBehaviour::operator!=(const MonoBehaviour& other) const
	{
		return Handle != other.Handle;
	}
	
	UnityEngine::Transform UnityEngine::MonoBehaviour::GetTransform()
	{
		auto returnValue = Plugin::UnityEngineMonoBehaviourPropertyGetTransform(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return UnityEngine::Transform(Plugin::InternalUse::Only, returnValue);
	}
	
	UnityEngine::GameObject UnityEngine::MonoBehaviour::GetGameObject()
	{
		auto returnValue = Plugin::UnityEngineMonoBehaviourPropertyGetGameObject(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return UnityEngine::GameObject(Plugin::InternalUse::Only, returnValue);
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			IEnumerator_1<UnityEngine::Vector3>::IEnumerator_1(decltype(nullptr))
				: System::IDisposable(nullptr)
				, System::Collections::IEnumerator(nullptr)
			{
			}
			
			IEnumerator_1<UnityEngine::Vector3>::IEnumerator_1(Plugin::InternalUse, int32_t handle)
				: System::IDisposable(nullptr)
				, System::Collections::IEnumerator(nullptr)
			{
				Handle = handle;
				if (handle)
				{
					Plugin::ReferenceManagedClass(handle);
				}
			}
			
			IEnumerator_1<UnityEngine::Vector3>::IEnumerator_1(const IEnumerator_1<UnityEngine::Vector3>& other)
				: IEnumerator_1(Plugin::InternalUse::Only, other.Handle)
			{
			}
			
			IEnumerator_1<UnityEngine::Vector3>::IEnumerator_1(IEnumerator_1<UnityEngine::Vector3>&& other)
				: IEnumerator_1(Plugin::InternalUse::Only, other.Handle)
			{
				other.Handle = 0;
			}
			
			IEnumerator_1<UnityEngine::Vector3>::~IEnumerator_1<UnityEngine::Vector3>()
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
			}
			
			IEnumerator_1<UnityEngine::Vector3>& IEnumerator_1<UnityEngine::Vector3>::operator=(const IEnumerator_1<UnityEngine::Vector3>& other)
			{
				if (this->Handle)
				{
					Plugin::DereferenceManagedClass(this->Handle);
				}
				this->Handle = other.Handle;
				if (this->Handle)
				{
					Plugin::ReferenceManagedClass(this->Handle);
				}
				return *this;
			}
			
			IEnumerator_1<UnityEngine::Vector3>& IEnumerator_1<UnityEngine::Vector3>::operator=(decltype(nullptr))
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
				return *this;
			}
			
			IEnumerator_1<UnityEngine::Vector3>& IEnumerator_1<UnityEngine::Vector3>::operator=(IEnumerator_1<UnityEngine::Vector3>&& other)
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
				}
				Handle = other.Handle;
				other.Handle = 0;
				return *this;
			}
			
			bool IEnumerator_1<UnityEngine::Vector3>::operator==(const IEnumerator_1<UnityEngine::Vector3>& other) const
			{
				return Handle == other.Handle;
			}
			
			bool IEnumerator_1<UnityEngine::Vector3>::operator!=(const IEnumerator_1<UnityEngine::Vector3>& other) const
			{
				return Handle != other.Handle;
			}
			
			UnityEngine::Vector3 System::Collections::Generic::IEnumerator_1<UnityEngine::Vector3>::GetCurrent()
			{
				auto returnValue = Plugin::SystemCollectionsGenericIEnumeratorUnityEngineVector3PropertyGetCurrent(Handle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
				return returnValue;
			}
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			IEnumerator_1<System::Int32>::IEnumerator_1(decltype(nullptr))
				: System::IDisposable(nullptr)
				, System::Collections::IEnumerator(nullptr)
			{
			}
			
			IEnumerator_1<System::Int32>::IEnumerator_1(Plugin::InternalUse, int32_t handle)
				: System::IDisposable(nullptr)
				, System::Collections::IEnumerator(nullptr)
			{
				Handle = handle;
				if (handle)
				{
					Plugin::ReferenceManagedClass(handle);
				}
			}
			
			IEnumerator_1<System::Int32>::IEnumerator_1(const IEnumerator_1<System::Int32>& other)
				: IEnumerator_1(Plugin::InternalUse::Only, other.Handle)
			{
			}
			
			IEnumerator_1<System::Int32>::IEnumerator_1(IEnumerator_1<System::Int32>&& other)
				: IEnumerator_1(Plugin::InternalUse::Only, other.Handle)
			{
				other.Handle = 0;
			}
			
			IEnumerator_1<System::Int32>::~IEnumerator_1<System::Int32>()
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
			}
			
			IEnumerator_1<System::Int32>& IEnumerator_1<System::Int32>::operator=(const IEnumerator_1<System::Int32>& other)
			{
				if (this->Handle)
				{
					Plugin::DereferenceManagedClass(this->Handle);
				}
				this->Handle = other.Handle;
				if (this->Handle)
				{
					Plugin::ReferenceManagedClass(this->Handle);
				}
				return *this;
			}
			
			IEnumerator_1<System::Int32>& IEnumerator_1<System::Int32>::operator=(decltype(nullptr))
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
				return *this;
			}
			
			IEnumerator_1<System::Int32>& IEnumerator_1<System::Int32>::operator=(IEnumerator_1<System::Int32>&& other)
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
				}
				Handle = other.Handle;
				other.Handle = 0;
				return *this;
			}
			
			bool IEnumerator_1<System::Int32>::operator==(const IEnumerator_1<System::Int32>& other) const
			{
				return Handle == other.Handle;
			}
			
			bool IEnumerator_1<System::Int32>::operator!=(const IEnumerator_1<System::Int32>& other) const
			{
				return Handle != other.Handle;
			}
			
			System::Int32 System::Collections::Generic::IEnumerator_1<System::Int32>::GetCurrent()
			{
				auto returnValue = Plugin::SystemCollectionsGenericIEnumeratorSystemInt32PropertyGetCurrent(Handle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
				return returnValue;
			}
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			IEnumerable_1<UnityEngine::Vector3>::IEnumerable_1(decltype(nullptr))
				: System::Collections::IEnumerable(nullptr)
			{
			}
			
			IEnumerable_1<UnityEngine::Vector3>::IEnumerable_1(Plugin::InternalUse, int32_t handle)
				: System::Collections::IEnumerable(nullptr)
			{
				Handle = handle;
				if (handle)
				{
					Plugin::ReferenceManagedClass(handle);
				}
			}
			
			IEnumerable_1<UnityEngine::Vector3>::IEnumerable_1(const IEnumerable_1<UnityEngine::Vector3>& other)
				: IEnumerable_1(Plugin::InternalUse::Only, other.Handle)
			{
			}
			
			IEnumerable_1<UnityEngine::Vector3>::IEnumerable_1(IEnumerable_1<UnityEngine::Vector3>&& other)
				: IEnumerable_1(Plugin::InternalUse::Only, other.Handle)
			{
				other.Handle = 0;
			}
			
			IEnumerable_1<UnityEngine::Vector3>::~IEnumerable_1<UnityEngine::Vector3>()
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
			}
			
			IEnumerable_1<UnityEngine::Vector3>& IEnumerable_1<UnityEngine::Vector3>::operator=(const IEnumerable_1<UnityEngine::Vector3>& other)
			{
				if (this->Handle)
				{
					Plugin::DereferenceManagedClass(this->Handle);
				}
				this->Handle = other.Handle;
				if (this->Handle)
				{
					Plugin::ReferenceManagedClass(this->Handle);
				}
				return *this;
			}
			
			IEnumerable_1<UnityEngine::Vector3>& IEnumerable_1<UnityEngine::Vector3>::operator=(decltype(nullptr))
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
				return *this;
			}
			
			IEnumerable_1<UnityEngine::Vector3>& IEnumerable_1<UnityEngine::Vector3>::operator=(IEnumerable_1<UnityEngine::Vector3>&& other)
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
				}
				Handle = other.Handle;
				other.Handle = 0;
				return *this;
			}
			
			bool IEnumerable_1<UnityEngine::Vector3>::operator==(const IEnumerable_1<UnityEngine::Vector3>& other) const
			{
				return Handle == other.Handle;
			}
			
			bool IEnumerable_1<UnityEngine::Vector3>::operator!=(const IEnumerable_1<UnityEngine::Vector3>& other) const
			{
				return Handle != other.Handle;
			}
			
			System::Collections::Generic::IEnumerator_1<UnityEngine::Vector3> System::Collections::Generic::IEnumerable_1<UnityEngine::Vector3>::GetEnumerator()
			{
				auto returnValue = Plugin::SystemCollectionsGenericIEnumerableUnityEngineVector3MethodGetEnumerator(Handle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
				return System::Collections::Generic::IEnumerator_1<UnityEngine::Vector3>(Plugin::InternalUse::Only, returnValue);
			}
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			IEnumerable_1<System::Int32>::IEnumerable_1(decltype(nullptr))
				: System::Collections::IEnumerable(nullptr)
			{
			}
			
			IEnumerable_1<System::Int32>::IEnumerable_1(Plugin::InternalUse, int32_t handle)
				: System::Collections::IEnumerable(nullptr)
			{
				Handle = handle;
				if (handle)
				{
					Plugin::ReferenceManagedClass(handle);
				}
			}
			
			IEnumerable_1<System::Int32>::IEnumerable_1(const IEnumerable_1<System::Int32>& other)
				: IEnumerable_1(Plugin::InternalUse::Only, other.Handle)
			{
			}
			
			IEnumerable_1<System::Int32>::IEnumerable_1(IEnumerable_1<System::Int32>&& other)
				: IEnumerable_1(Plugin::InternalUse::Only, other.Handle)
			{
				other.Handle = 0;
			}
			
			IEnumerable_1<System::Int32>::~IEnumerable_1<System::Int32>()
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
			}
			
			IEnumerable_1<System::Int32>& IEnumerable_1<System::Int32>::operator=(const IEnumerable_1<System::Int32>& other)
			{
				if (this->Handle)
				{
					Plugin::DereferenceManagedClass(this->Handle);
				}
				this->Handle = other.Handle;
				if (this->Handle)
				{
					Plugin::ReferenceManagedClass(this->Handle);
				}
				return *this;
			}
			
			IEnumerable_1<System::Int32>& IEnumerable_1<System::Int32>::operator=(decltype(nullptr))
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
				return *this;
			}
			
			IEnumerable_1<System::Int32>& IEnumerable_1<System::Int32>::operator=(IEnumerable_1<System::Int32>&& other)
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
				}
				Handle = other.Handle;
				other.Handle = 0;
				return *this;
			}
			
			bool IEnumerable_1<System::Int32>::operator==(const IEnumerable_1<System::Int32>& other) const
			{
				return Handle == other.Handle;
			}
			
			bool IEnumerable_1<System::Int32>::operator!=(const IEnumerable_1<System::Int32>& other) const
			{
				return Handle != other.Handle;
			}
			
			System::Collections::Generic::IEnumerator_1<System::Int32> System::Collections::Generic::IEnumerable_1<System::Int32>::GetEnumerator()
			{
				auto returnValue = Plugin::SystemCollectionsGenericIEnumerableSystemInt32MethodGetEnumerator(Handle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
				return System::Collections::Generic::IEnumerator_1<System::Int32>(Plugin::InternalUse::Only, returnValue);
			}
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			ICollection_1<UnityEngine::Vector3>::ICollection_1(decltype(nullptr))
				: System::Collections::IEnumerable(nullptr)
				, System::Collections::Generic::IEnumerable_1<UnityEngine::Vector3>(nullptr)
			{
			}
			
			ICollection_1<UnityEngine::Vector3>::ICollection_1(Plugin::InternalUse, int32_t handle)
				: System::Collections::IEnumerable(nullptr)
				, System::Collections::Generic::IEnumerable_1<UnityEngine::Vector3>(nullptr)
			{
				Handle = handle;
				if (handle)
				{
					Plugin::ReferenceManagedClass(handle);
				}
			}
			
			ICollection_1<UnityEngine::Vector3>::ICollection_1(const ICollection_1<UnityEngine::Vector3>& other)
				: ICollection_1(Plugin::InternalUse::Only, other.Handle)
			{
			}
			
			ICollection_1<UnityEngine::Vector3>::ICollection_1(ICollection_1<UnityEngine::Vector3>&& other)
				: ICollection_1(Plugin::InternalUse::Only, other.Handle)
			{
				other.Handle = 0;
			}
			
			ICollection_1<UnityEngine::Vector3>::~ICollection_1<UnityEngine::Vector3>()
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
			}
			
			ICollection_1<UnityEngine::Vector3>& ICollection_1<UnityEngine::Vector3>::operator=(const ICollection_1<UnityEngine::Vector3>& other)
			{
				if (this->Handle)
				{
					Plugin::DereferenceManagedClass(this->Handle);
				}
				this->Handle = other.Handle;
				if (this->Handle)
				{
					Plugin::ReferenceManagedClass(this->Handle);
				}
				return *this;
			}
			
			ICollection_1<UnityEngine::Vector3>& ICollection_1<UnityEngine::Vector3>::operator=(decltype(nullptr))
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
				return *this;
			}
			
			ICollection_1<UnityEngine::Vector3>& ICollection_1<UnityEngine::Vector3>::operator=(ICollection_1<UnityEngine::Vector3>&& other)
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
				}
				Handle = other.Handle;
				other.Handle = 0;
				return *this;
			}
			
			bool ICollection_1<UnityEngine::Vector3>::operator==(const ICollection_1<UnityEngine::Vector3>& other) const
			{
				return Handle == other.Handle;
			}
			
			bool ICollection_1<UnityEngine::Vector3>::operator!=(const ICollection_1<UnityEngine::Vector3>& other) const
			{
				return Handle != other.Handle;
			}
		}
	}
}

namespace Plugin
{
	SystemCollectionsGenericICollectionUnityEngineVector3Iterator::SystemCollectionsGenericICollectionUnityEngineVector3Iterator(decltype(nullptr))
		: enumerator(nullptr)
		, hasMore(false)
	{
	}
	
	SystemCollectionsGenericICollectionUnityEngineVector3Iterator::SystemCollectionsGenericICollectionUnityEngineVector3Iterator(System::Collections::Generic::ICollection_1<UnityEngine::Vector3>& enumerable)
		: enumerator(enumerable.GetEnumerator())
	{
		hasMore = enumerator.MoveNext();
	}
	
	SystemCollectionsGenericICollectionUnityEngineVector3Iterator::~SystemCollectionsGenericICollectionUnityEngineVector3Iterator()
	{
		if (enumerator != nullptr)
		{
			enumerator.Dispose();
		}
	}
	
	SystemCollectionsGenericICollectionUnityEngineVector3Iterator& SystemCollectionsGenericICollectionUnityEngineVector3Iterator::operator++()
	{
		hasMore = enumerator.MoveNext();
		return *this;
	}
	
	bool SystemCollectionsGenericICollectionUnityEngineVector3Iterator::operator!=(const SystemCollectionsGenericICollectionUnityEngineVector3Iterator& other)
	{
		return hasMore;
	}
	
	UnityEngine::Vector3 SystemCollectionsGenericICollectionUnityEngineVector3Iterator::operator*()
	{
		return enumerator.GetCurrent();
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericICollectionUnityEngineVector3Iterator begin(System::Collections::Generic::ICollection_1<UnityEngine::Vector3>& enumerable)
			{
				return Plugin::SystemCollectionsGenericICollectionUnityEngineVector3Iterator(enumerable);
			}
			
			Plugin::SystemCollectionsGenericICollectionUnityEngineVector3Iterator end(System::Collections::Generic::ICollection_1<UnityEngine::Vector3>& enumerable)
			{
				return Plugin::SystemCollectionsGenericICollectionUnityEngineVector3Iterator(nullptr);
			}
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			ICollection_1<System::Int32>::ICollection_1(decltype(nullptr))
				: System::Collections::IEnumerable(nullptr)
				, System::Collections::Generic::IEnumerable_1<System::Int32>(nullptr)
			{
			}
			
			ICollection_1<System::Int32>::ICollection_1(Plugin::InternalUse, int32_t handle)
				: System::Collections::IEnumerable(nullptr)
				, System::Collections::Generic::IEnumerable_1<System::Int32>(nullptr)
			{
				Handle = handle;
				if (handle)
				{
					Plugin::ReferenceManagedClass(handle);
				}
			}
			
			ICollection_1<System::Int32>::ICollection_1(const ICollection_1<System::Int32>& other)
				: ICollection_1(Plugin::InternalUse::Only, other.Handle)
			{
			}
			
			ICollection_1<System::Int32>::ICollection_1(ICollection_1<System::Int32>&& other)
				: ICollection_1(Plugin::InternalUse::Only, other.Handle)
			{
				other.Handle = 0;
			}
			
			ICollection_1<System::Int32>::~ICollection_1<System::Int32>()
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
			}
			
			ICollection_1<System::Int32>& ICollection_1<System::Int32>::operator=(const ICollection_1<System::Int32>& other)
			{
				if (this->Handle)
				{
					Plugin::DereferenceManagedClass(this->Handle);
				}
				this->Handle = other.Handle;
				if (this->Handle)
				{
					Plugin::ReferenceManagedClass(this->Handle);
				}
				return *this;
			}
			
			ICollection_1<System::Int32>& ICollection_1<System::Int32>::operator=(decltype(nullptr))
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
				return *this;
			}
			
			ICollection_1<System::Int32>& ICollection_1<System::Int32>::operator=(ICollection_1<System::Int32>&& other)
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
				}
				Handle = other.Handle;
				other.Handle = 0;
				return *this;
			}
			
			bool ICollection_1<System::Int32>::operator==(const ICollection_1<System::Int32>& other) const
			{
				return Handle == other.Handle;
			}
			
			bool ICollection_1<System::Int32>::operator!=(const ICollection_1<System::Int32>& other) const
			{
				return Handle != other.Handle;
			}
		}
	}
}

namespace Plugin
{
	SystemCollectionsGenericICollectionSystemInt32Iterator::SystemCollectionsGenericICollectionSystemInt32Iterator(decltype(nullptr))
		: enumerator(nullptr)
		, hasMore(false)
	{
	}
	
	SystemCollectionsGenericICollectionSystemInt32Iterator::SystemCollectionsGenericICollectionSystemInt32Iterator(System::Collections::Generic::ICollection_1<System::Int32>& enumerable)
		: enumerator(enumerable.GetEnumerator())
	{
		hasMore = enumerator.MoveNext();
	}
	
	SystemCollectionsGenericICollectionSystemInt32Iterator::~SystemCollectionsGenericICollectionSystemInt32Iterator()
	{
		if (enumerator != nullptr)
		{
			enumerator.Dispose();
		}
	}
	
	SystemCollectionsGenericICollectionSystemInt32Iterator& SystemCollectionsGenericICollectionSystemInt32Iterator::operator++()
	{
		hasMore = enumerator.MoveNext();
		return *this;
	}
	
	bool SystemCollectionsGenericICollectionSystemInt32Iterator::operator!=(const SystemCollectionsGenericICollectionSystemInt32Iterator& other)
	{
		return hasMore;
	}
	
	System::Int32 SystemCollectionsGenericICollectionSystemInt32Iterator::operator*()
	{
		return enumerator.GetCurrent();
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericICollectionSystemInt32Iterator begin(System::Collections::Generic::ICollection_1<System::Int32>& enumerable)
			{
				return Plugin::SystemCollectionsGenericICollectionSystemInt32Iterator(enumerable);
			}
			
			Plugin::SystemCollectionsGenericICollectionSystemInt32Iterator end(System::Collections::Generic::ICollection_1<System::Int32>& enumerable)
			{
				return Plugin::SystemCollectionsGenericICollectionSystemInt32Iterator(nullptr);
			}
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			IReadOnlyCollection_1<UnityEngine::Vector3>::IReadOnlyCollection_1(decltype(nullptr))
				: System::Collections::IEnumerable(nullptr)
				, System::Collections::Generic::IEnumerable_1<UnityEngine::Vector3>(nullptr)
			{
			}
			
			IReadOnlyCollection_1<UnityEngine::Vector3>::IReadOnlyCollection_1(Plugin::InternalUse, int32_t handle)
				: System::Collections::IEnumerable(nullptr)
				, System::Collections::Generic::IEnumerable_1<UnityEngine::Vector3>(nullptr)
			{
				Handle = handle;
				if (handle)
				{
					Plugin::ReferenceManagedClass(handle);
				}
			}
			
			IReadOnlyCollection_1<UnityEngine::Vector3>::IReadOnlyCollection_1(const IReadOnlyCollection_1<UnityEngine::Vector3>& other)
				: IReadOnlyCollection_1(Plugin::InternalUse::Only, other.Handle)
			{
			}
			
			IReadOnlyCollection_1<UnityEngine::Vector3>::IReadOnlyCollection_1(IReadOnlyCollection_1<UnityEngine::Vector3>&& other)
				: IReadOnlyCollection_1(Plugin::InternalUse::Only, other.Handle)
			{
				other.Handle = 0;
			}
			
			IReadOnlyCollection_1<UnityEngine::Vector3>::~IReadOnlyCollection_1<UnityEngine::Vector3>()
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
			}
			
			IReadOnlyCollection_1<UnityEngine::Vector3>& IReadOnlyCollection_1<UnityEngine::Vector3>::operator=(const IReadOnlyCollection_1<UnityEngine::Vector3>& other)
			{
				if (this->Handle)
				{
					Plugin::DereferenceManagedClass(this->Handle);
				}
				this->Handle = other.Handle;
				if (this->Handle)
				{
					Plugin::ReferenceManagedClass(this->Handle);
				}
				return *this;
			}
			
			IReadOnlyCollection_1<UnityEngine::Vector3>& IReadOnlyCollection_1<UnityEngine::Vector3>::operator=(decltype(nullptr))
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
				return *this;
			}
			
			IReadOnlyCollection_1<UnityEngine::Vector3>& IReadOnlyCollection_1<UnityEngine::Vector3>::operator=(IReadOnlyCollection_1<UnityEngine::Vector3>&& other)
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
				}
				Handle = other.Handle;
				other.Handle = 0;
				return *this;
			}
			
			bool IReadOnlyCollection_1<UnityEngine::Vector3>::operator==(const IReadOnlyCollection_1<UnityEngine::Vector3>& other) const
			{
				return Handle == other.Handle;
			}
			
			bool IReadOnlyCollection_1<UnityEngine::Vector3>::operator!=(const IReadOnlyCollection_1<UnityEngine::Vector3>& other) const
			{
				return Handle != other.Handle;
			}
		}
	}
}

namespace Plugin
{
	SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator::SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator(decltype(nullptr))
		: enumerator(nullptr)
		, hasMore(false)
	{
	}
	
	SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator::SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator(System::Collections::Generic::IReadOnlyCollection_1<UnityEngine::Vector3>& enumerable)
		: enumerator(enumerable.GetEnumerator())
	{
		hasMore = enumerator.MoveNext();
	}
	
	SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator::~SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator()
	{
		if (enumerator != nullptr)
		{
			enumerator.Dispose();
		}
	}
	
	SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator& SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator::operator++()
	{
		hasMore = enumerator.MoveNext();
		return *this;
	}
	
	bool SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator::operator!=(const SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator& other)
	{
		return hasMore;
	}
	
	UnityEngine::Vector3 SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator::operator*()
	{
		return enumerator.GetCurrent();
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator begin(System::Collections::Generic::IReadOnlyCollection_1<UnityEngine::Vector3>& enumerable)
			{
				return Plugin::SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator(enumerable);
			}
			
			Plugin::SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator end(System::Collections::Generic::IReadOnlyCollection_1<UnityEngine::Vector3>& enumerable)
			{
				return Plugin::SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator(nullptr);
			}
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			IReadOnlyCollection_1<System::Int32>::IReadOnlyCollection_1(decltype(nullptr))
				: System::Collections::IEnumerable(nullptr)
				, System::Collections::Generic::IEnumerable_1<System::Int32>(nullptr)
			{
			}
			
			IReadOnlyCollection_1<System::Int32>::IReadOnlyCollection_1(Plugin::InternalUse, int32_t handle)
				: System::Collections::IEnumerable(nullptr)
				, System::Collections::Generic::IEnumerable_1<System::Int32>(nullptr)
			{
				Handle = handle;
				if (handle)
				{
					Plugin::ReferenceManagedClass(handle);
				}
			}
			
			IReadOnlyCollection_1<System::Int32>::IReadOnlyCollection_1(const IReadOnlyCollection_1<System::Int32>& other)
				: IReadOnlyCollection_1(Plugin::InternalUse::Only, other.Handle)
			{
			}
			
			IReadOnlyCollection_1<System::Int32>::IReadOnlyCollection_1(IReadOnlyCollection_1<System::Int32>&& other)
				: IReadOnlyCollection_1(Plugin::InternalUse::Only, other.Handle)
			{
				other.Handle = 0;
			}
			
			IReadOnlyCollection_1<System::Int32>::~IReadOnlyCollection_1<System::Int32>()
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
			}
			
			IReadOnlyCollection_1<System::Int32>& IReadOnlyCollection_1<System::Int32>::operator=(const IReadOnlyCollection_1<System::Int32>& other)
			{
				if (this->Handle)
				{
					Plugin::DereferenceManagedClass(this->Handle);
				}
				this->Handle = other.Handle;
				if (this->Handle)
				{
					Plugin::ReferenceManagedClass(this->Handle);
				}
				return *this;
			}
			
			IReadOnlyCollection_1<System::Int32>& IReadOnlyCollection_1<System::Int32>::operator=(decltype(nullptr))
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
				return *this;
			}
			
			IReadOnlyCollection_1<System::Int32>& IReadOnlyCollection_1<System::Int32>::operator=(IReadOnlyCollection_1<System::Int32>&& other)
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
				}
				Handle = other.Handle;
				other.Handle = 0;
				return *this;
			}
			
			bool IReadOnlyCollection_1<System::Int32>::operator==(const IReadOnlyCollection_1<System::Int32>& other) const
			{
				return Handle == other.Handle;
			}
			
			bool IReadOnlyCollection_1<System::Int32>::operator!=(const IReadOnlyCollection_1<System::Int32>& other) const
			{
				return Handle != other.Handle;
			}
		}
	}
}

namespace Plugin
{
	SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator::SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator(decltype(nullptr))
		: enumerator(nullptr)
		, hasMore(false)
	{
	}
	
	SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator::SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator(System::Collections::Generic::IReadOnlyCollection_1<System::Int32>& enumerable)
		: enumerator(enumerable.GetEnumerator())
	{
		hasMore = enumerator.MoveNext();
	}
	
	SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator::~SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator()
	{
		if (enumerator != nullptr)
		{
			enumerator.Dispose();
		}
	}
	
	SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator& SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator::operator++()
	{
		hasMore = enumerator.MoveNext();
		return *this;
	}
	
	bool SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator::operator!=(const SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator& other)
	{
		return hasMore;
	}
	
	System::Int32 SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator::operator*()
	{
		return enumerator.GetCurrent();
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator begin(System::Collections::Generic::IReadOnlyCollection_1<System::Int32>& enumerable)
			{
				return Plugin::SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator(enumerable);
			}
			
			Plugin::SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator end(System::Collections::Generic::IReadOnlyCollection_1<System::Int32>& enumerable)
			{
				return Plugin::SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator(nullptr);
			}
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			IList_1<UnityEngine::Vector3>::IList_1(decltype(nullptr))
				: System::Collections::IEnumerable(nullptr)
				, System::Collections::Generic::IEnumerable_1<UnityEngine::Vector3>(nullptr)
				, System::Collections::Generic::ICollection_1<UnityEngine::Vector3>(nullptr)
			{
			}
			
			IList_1<UnityEngine::Vector3>::IList_1(Plugin::InternalUse, int32_t handle)
				: System::Collections::IEnumerable(nullptr)
				, System::Collections::Generic::IEnumerable_1<UnityEngine::Vector3>(nullptr)
				, System::Collections::Generic::ICollection_1<UnityEngine::Vector3>(nullptr)
			{
				Handle = handle;
				if (handle)
				{
					Plugin::ReferenceManagedClass(handle);
				}
			}
			
			IList_1<UnityEngine::Vector3>::IList_1(const IList_1<UnityEngine::Vector3>& other)
				: IList_1(Plugin::InternalUse::Only, other.Handle)
			{
			}
			
			IList_1<UnityEngine::Vector3>::IList_1(IList_1<UnityEngine::Vector3>&& other)
				: IList_1(Plugin::InternalUse::Only, other.Handle)
			{
				other.Handle = 0;
			}
			
			IList_1<UnityEngine::Vector3>::~IList_1<UnityEngine::Vector3>()
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
			}
			
			IList_1<UnityEngine::Vector3>& IList_1<UnityEngine::Vector3>::operator=(const IList_1<UnityEngine::Vector3>& other)
			{
				if (this->Handle)
				{
					Plugin::DereferenceManagedClass(this->Handle);
				}
				this->Handle = other.Handle;
				if (this->Handle)
				{
					Plugin::ReferenceManagedClass(this->Handle);
				}
				return *this;
			}
			
			IList_1<UnityEngine::Vector3>& IList_1<UnityEngine::Vector3>::operator=(decltype(nullptr))
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
				return *this;
			}
			
			IList_1<UnityEngine::Vector3>& IList_1<UnityEngine::Vector3>::operator=(IList_1<UnityEngine::Vector3>&& other)
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
				}
				Handle = other.Handle;
				other.Handle = 0;
				return *this;
			}
			
			bool IList_1<UnityEngine::Vector3>::operator==(const IList_1<UnityEngine::Vector3>& other) const
			{
				return Handle == other.Handle;
			}
			
			bool IList_1<UnityEngine::Vector3>::operator!=(const IList_1<UnityEngine::Vector3>& other) const
			{
				return Handle != other.Handle;
			}
		}
	}
}

namespace Plugin
{
	SystemCollectionsGenericIListUnityEngineVector3Iterator::SystemCollectionsGenericIListUnityEngineVector3Iterator(decltype(nullptr))
		: enumerator(nullptr)
		, hasMore(false)
	{
	}
	
	SystemCollectionsGenericIListUnityEngineVector3Iterator::SystemCollectionsGenericIListUnityEngineVector3Iterator(System::Collections::Generic::IList_1<UnityEngine::Vector3>& enumerable)
		: enumerator(enumerable.GetEnumerator())
	{
		hasMore = enumerator.MoveNext();
	}
	
	SystemCollectionsGenericIListUnityEngineVector3Iterator::~SystemCollectionsGenericIListUnityEngineVector3Iterator()
	{
		if (enumerator != nullptr)
		{
			enumerator.Dispose();
		}
	}
	
	SystemCollectionsGenericIListUnityEngineVector3Iterator& SystemCollectionsGenericIListUnityEngineVector3Iterator::operator++()
	{
		hasMore = enumerator.MoveNext();
		return *this;
	}
	
	bool SystemCollectionsGenericIListUnityEngineVector3Iterator::operator!=(const SystemCollectionsGenericIListUnityEngineVector3Iterator& other)
	{
		return hasMore;
	}
	
	UnityEngine::Vector3 SystemCollectionsGenericIListUnityEngineVector3Iterator::operator*()
	{
		return enumerator.GetCurrent();
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIListUnityEngineVector3Iterator begin(System::Collections::Generic::IList_1<UnityEngine::Vector3>& enumerable)
			{
				return Plugin::SystemCollectionsGenericIListUnityEngineVector3Iterator(enumerable);
			}
			
			Plugin::SystemCollectionsGenericIListUnityEngineVector3Iterator end(System::Collections::Generic::IList_1<UnityEngine::Vector3>& enumerable)
			{
				return Plugin::SystemCollectionsGenericIListUnityEngineVector3Iterator(nullptr);
			}
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			IList_1<System::Int32>::IList_1(decltype(nullptr))
				: System::Collections::IEnumerable(nullptr)
				, System::Collections::Generic::IEnumerable_1<System::Int32>(nullptr)
				, System::Collections::Generic::ICollection_1<System::Int32>(nullptr)
			{
			}
			
			IList_1<System::Int32>::IList_1(Plugin::InternalUse, int32_t handle)
				: System::Collections::IEnumerable(nullptr)
				, System::Collections::Generic::IEnumerable_1<System::Int32>(nullptr)
				, System::Collections::Generic::ICollection_1<System::Int32>(nullptr)
			{
				Handle = handle;
				if (handle)
				{
					Plugin::ReferenceManagedClass(handle);
				}
			}
			
			IList_1<System::Int32>::IList_1(const IList_1<System::Int32>& other)
				: IList_1(Plugin::InternalUse::Only, other.Handle)
			{
			}
			
			IList_1<System::Int32>::IList_1(IList_1<System::Int32>&& other)
				: IList_1(Plugin::InternalUse::Only, other.Handle)
			{
				other.Handle = 0;
			}
			
			IList_1<System::Int32>::~IList_1<System::Int32>()
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
			}
			
			IList_1<System::Int32>& IList_1<System::Int32>::operator=(const IList_1<System::Int32>& other)
			{
				if (this->Handle)
				{
					Plugin::DereferenceManagedClass(this->Handle);
				}
				this->Handle = other.Handle;
				if (this->Handle)
				{
					Plugin::ReferenceManagedClass(this->Handle);
				}
				return *this;
			}
			
			IList_1<System::Int32>& IList_1<System::Int32>::operator=(decltype(nullptr))
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
				return *this;
			}
			
			IList_1<System::Int32>& IList_1<System::Int32>::operator=(IList_1<System::Int32>&& other)
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
				}
				Handle = other.Handle;
				other.Handle = 0;
				return *this;
			}
			
			bool IList_1<System::Int32>::operator==(const IList_1<System::Int32>& other) const
			{
				return Handle == other.Handle;
			}
			
			bool IList_1<System::Int32>::operator!=(const IList_1<System::Int32>& other) const
			{
				return Handle != other.Handle;
			}
		}
	}
}

namespace Plugin
{
	SystemCollectionsGenericIListSystemInt32Iterator::SystemCollectionsGenericIListSystemInt32Iterator(decltype(nullptr))
		: enumerator(nullptr)
		, hasMore(false)
	{
	}
	
	SystemCollectionsGenericIListSystemInt32Iterator::SystemCollectionsGenericIListSystemInt32Iterator(System::Collections::Generic::IList_1<System::Int32>& enumerable)
		: enumerator(enumerable.GetEnumerator())
	{
		hasMore = enumerator.MoveNext();
	}
	
	SystemCollectionsGenericIListSystemInt32Iterator::~SystemCollectionsGenericIListSystemInt32Iterator()
	{
		if (enumerator != nullptr)
		{
			enumerator.Dispose();
		}
	}
	
	SystemCollectionsGenericIListSystemInt32Iterator& SystemCollectionsGenericIListSystemInt32Iterator::operator++()
	{
		hasMore = enumerator.MoveNext();
		return *this;
	}
	
	bool SystemCollectionsGenericIListSystemInt32Iterator::operator!=(const SystemCollectionsGenericIListSystemInt32Iterator& other)
	{
		return hasMore;
	}
	
	System::Int32 SystemCollectionsGenericIListSystemInt32Iterator::operator*()
	{
		return enumerator.GetCurrent();
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIListSystemInt32Iterator begin(System::Collections::Generic::IList_1<System::Int32>& enumerable)
			{
				return Plugin::SystemCollectionsGenericIListSystemInt32Iterator(enumerable);
			}
			
			Plugin::SystemCollectionsGenericIListSystemInt32Iterator end(System::Collections::Generic::IList_1<System::Int32>& enumerable)
			{
				return Plugin::SystemCollectionsGenericIListSystemInt32Iterator(nullptr);
			}
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			IReadOnlyList_1<UnityEngine::Vector3>::IReadOnlyList_1(decltype(nullptr))
				: System::Collections::IEnumerable(nullptr)
				, System::Collections::Generic::IEnumerable_1<UnityEngine::Vector3>(nullptr)
				, System::Collections::Generic::IReadOnlyCollection_1<UnityEngine::Vector3>(nullptr)
			{
			}
			
			IReadOnlyList_1<UnityEngine::Vector3>::IReadOnlyList_1(Plugin::InternalUse, int32_t handle)
				: System::Collections::IEnumerable(nullptr)
				, System::Collections::Generic::IEnumerable_1<UnityEngine::Vector3>(nullptr)
				, System::Collections::Generic::IReadOnlyCollection_1<UnityEngine::Vector3>(nullptr)
			{
				Handle = handle;
				if (handle)
				{
					Plugin::ReferenceManagedClass(handle);
				}
			}
			
			IReadOnlyList_1<UnityEngine::Vector3>::IReadOnlyList_1(const IReadOnlyList_1<UnityEngine::Vector3>& other)
				: IReadOnlyList_1(Plugin::InternalUse::Only, other.Handle)
			{
			}
			
			IReadOnlyList_1<UnityEngine::Vector3>::IReadOnlyList_1(IReadOnlyList_1<UnityEngine::Vector3>&& other)
				: IReadOnlyList_1(Plugin::InternalUse::Only, other.Handle)
			{
				other.Handle = 0;
			}
			
			IReadOnlyList_1<UnityEngine::Vector3>::~IReadOnlyList_1<UnityEngine::Vector3>()
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
			}
			
			IReadOnlyList_1<UnityEngine::Vector3>& IReadOnlyList_1<UnityEngine::Vector3>::operator=(const IReadOnlyList_1<UnityEngine::Vector3>& other)
			{
				if (this->Handle)
				{
					Plugin::DereferenceManagedClass(this->Handle);
				}
				this->Handle = other.Handle;
				if (this->Handle)
				{
					Plugin::ReferenceManagedClass(this->Handle);
				}
				return *this;
			}
			
			IReadOnlyList_1<UnityEngine::Vector3>& IReadOnlyList_1<UnityEngine::Vector3>::operator=(decltype(nullptr))
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
				return *this;
			}
			
			IReadOnlyList_1<UnityEngine::Vector3>& IReadOnlyList_1<UnityEngine::Vector3>::operator=(IReadOnlyList_1<UnityEngine::Vector3>&& other)
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
				}
				Handle = other.Handle;
				other.Handle = 0;
				return *this;
			}
			
			bool IReadOnlyList_1<UnityEngine::Vector3>::operator==(const IReadOnlyList_1<UnityEngine::Vector3>& other) const
			{
				return Handle == other.Handle;
			}
			
			bool IReadOnlyList_1<UnityEngine::Vector3>::operator!=(const IReadOnlyList_1<UnityEngine::Vector3>& other) const
			{
				return Handle != other.Handle;
			}
		}
	}
}

namespace Plugin
{
	SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator::SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator(decltype(nullptr))
		: enumerator(nullptr)
		, hasMore(false)
	{
	}
	
	SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator::SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator(System::Collections::Generic::IReadOnlyList_1<UnityEngine::Vector3>& enumerable)
		: enumerator(enumerable.GetEnumerator())
	{
		hasMore = enumerator.MoveNext();
	}
	
	SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator::~SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator()
	{
		if (enumerator != nullptr)
		{
			enumerator.Dispose();
		}
	}
	
	SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator& SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator::operator++()
	{
		hasMore = enumerator.MoveNext();
		return *this;
	}
	
	bool SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator::operator!=(const SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator& other)
	{
		return hasMore;
	}
	
	UnityEngine::Vector3 SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator::operator*()
	{
		return enumerator.GetCurrent();
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator begin(System::Collections::Generic::IReadOnlyList_1<UnityEngine::Vector3>& enumerable)
			{
				return Plugin::SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator(enumerable);
			}
			
			Plugin::SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator end(System::Collections::Generic::IReadOnlyList_1<UnityEngine::Vector3>& enumerable)
			{
				return Plugin::SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator(nullptr);
			}
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			IReadOnlyList_1<System::Int32>::IReadOnlyList_1(decltype(nullptr))
				: System::Collections::IEnumerable(nullptr)
				, System::Collections::Generic::IEnumerable_1<System::Int32>(nullptr)
				, System::Collections::Generic::IReadOnlyCollection_1<System::Int32>(nullptr)
			{
			}
			
			IReadOnlyList_1<System::Int32>::IReadOnlyList_1(Plugin::InternalUse, int32_t handle)
				: System::Collections::IEnumerable(nullptr)
				, System::Collections::Generic::IEnumerable_1<System::Int32>(nullptr)
				, System::Collections::Generic::IReadOnlyCollection_1<System::Int32>(nullptr)
			{
				Handle = handle;
				if (handle)
				{
					Plugin::ReferenceManagedClass(handle);
				}
			}
			
			IReadOnlyList_1<System::Int32>::IReadOnlyList_1(const IReadOnlyList_1<System::Int32>& other)
				: IReadOnlyList_1(Plugin::InternalUse::Only, other.Handle)
			{
			}
			
			IReadOnlyList_1<System::Int32>::IReadOnlyList_1(IReadOnlyList_1<System::Int32>&& other)
				: IReadOnlyList_1(Plugin::InternalUse::Only, other.Handle)
			{
				other.Handle = 0;
			}
			
			IReadOnlyList_1<System::Int32>::~IReadOnlyList_1<System::Int32>()
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
			}
			
			IReadOnlyList_1<System::Int32>& IReadOnlyList_1<System::Int32>::operator=(const IReadOnlyList_1<System::Int32>& other)
			{
				if (this->Handle)
				{
					Plugin::DereferenceManagedClass(this->Handle);
				}
				this->Handle = other.Handle;
				if (this->Handle)
				{
					Plugin::ReferenceManagedClass(this->Handle);
				}
				return *this;
			}
			
			IReadOnlyList_1<System::Int32>& IReadOnlyList_1<System::Int32>::operator=(decltype(nullptr))
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
				return *this;
			}
			
			IReadOnlyList_1<System::Int32>& IReadOnlyList_1<System::Int32>::operator=(IReadOnlyList_1<System::Int32>&& other)
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
				}
				Handle = other.Handle;
				other.Handle = 0;
				return *this;
			}
			
			bool IReadOnlyList_1<System::Int32>::operator==(const IReadOnlyList_1<System::Int32>& other) const
			{
				return Handle == other.Handle;
			}
			
			bool IReadOnlyList_1<System::Int32>::operator!=(const IReadOnlyList_1<System::Int32>& other) const
			{
				return Handle != other.Handle;
			}
		}
	}
}

namespace Plugin
{
	SystemCollectionsGenericIReadOnlyListSystemInt32Iterator::SystemCollectionsGenericIReadOnlyListSystemInt32Iterator(decltype(nullptr))
		: enumerator(nullptr)
		, hasMore(false)
	{
	}
	
	SystemCollectionsGenericIReadOnlyListSystemInt32Iterator::SystemCollectionsGenericIReadOnlyListSystemInt32Iterator(System::Collections::Generic::IReadOnlyList_1<System::Int32>& enumerable)
		: enumerator(enumerable.GetEnumerator())
	{
		hasMore = enumerator.MoveNext();
	}
	
	SystemCollectionsGenericIReadOnlyListSystemInt32Iterator::~SystemCollectionsGenericIReadOnlyListSystemInt32Iterator()
	{
		if (enumerator != nullptr)
		{
			enumerator.Dispose();
		}
	}
	
	SystemCollectionsGenericIReadOnlyListSystemInt32Iterator& SystemCollectionsGenericIReadOnlyListSystemInt32Iterator::operator++()
	{
		hasMore = enumerator.MoveNext();
		return *this;
	}
	
	bool SystemCollectionsGenericIReadOnlyListSystemInt32Iterator::operator!=(const SystemCollectionsGenericIReadOnlyListSystemInt32Iterator& other)
	{
		return hasMore;
	}
	
	System::Int32 SystemCollectionsGenericIReadOnlyListSystemInt32Iterator::operator*()
	{
		return enumerator.GetCurrent();
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIReadOnlyListSystemInt32Iterator begin(System::Collections::Generic::IReadOnlyList_1<System::Int32>& enumerable)
			{
				return Plugin::SystemCollectionsGenericIReadOnlyListSystemInt32Iterator(enumerable);
			}
			
			Plugin::SystemCollectionsGenericIReadOnlyListSystemInt32Iterator end(System::Collections::Generic::IReadOnlyList_1<System::Int32>& enumerable)
			{
				return Plugin::SystemCollectionsGenericIReadOnlyListSystemInt32Iterator(nullptr);
			}
		}
	}
}

namespace UnityEngine
{
	Mesh::Mesh(decltype(nullptr))
		: UnityEngine::Object(nullptr)
	{
	}
	
	Mesh::Mesh(Plugin::InternalUse, int32_t handle)
		: UnityEngine::Object(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	Mesh::Mesh(const Mesh& other)
		: Mesh(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	Mesh::Mesh(Mesh&& other)
		: Mesh(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	Mesh::~Mesh()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	Mesh& Mesh::operator=(const Mesh& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	Mesh& Mesh::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	Mesh& Mesh::operator=(Mesh&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool Mesh::operator==(const Mesh& other) const
	{
		return Handle == other.Handle;
	}
	
	bool Mesh::operator!=(const Mesh& other) const
	{
		return Handle != other.Handle;
	}
	
	UnityEngine::Mesh::Mesh()
		: UnityEngine::Object(nullptr)
	{
		auto returnValue = Plugin::UnityEngineMeshConstructor();
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		Handle = returnValue;
		if (returnValue)
		{
			Plugin::ReferenceManagedClass(returnValue);
		}
	}
	
	void UnityEngine::Mesh::SetVertices(System::Array1<UnityEngine::Vector3>& inVertices)
	{
		Plugin::UnityEngineMeshMethodSetVerticesUnityEngineVector3Array1(Handle, inVertices.Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	void UnityEngine::Mesh::SetNormals(System::Array1<UnityEngine::Vector3>& inNormals)
	{
		Plugin::UnityEngineMeshMethodSetNormalsUnityEngineVector3Array1(Handle, inNormals.Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	void UnityEngine::Mesh::SetTriangles(System::Array1<System::Int32>& triangles, System::Int32 submesh, System::Boolean calculateBounds, System::Int32 baseVertex)
	{
		Plugin::UnityEngineMeshMethodSetTrianglesSystemInt32Array1_SystemInt32_SystemBoolean_SystemInt32(Handle, triangles.Handle, submesh, calculateBounds, baseVertex);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
}

namespace UnityEngine
{
	MeshFilter::MeshFilter(decltype(nullptr))
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
	{
	}
	
	MeshFilter::MeshFilter(Plugin::InternalUse, int32_t handle)
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	MeshFilter::MeshFilter(const MeshFilter& other)
		: MeshFilter(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	MeshFilter::MeshFilter(MeshFilter&& other)
		: MeshFilter(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	MeshFilter::~MeshFilter()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	MeshFilter& MeshFilter::operator=(const MeshFilter& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	MeshFilter& MeshFilter::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	MeshFilter& MeshFilter::operator=(MeshFilter&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool MeshFilter::operator==(const MeshFilter& other) const
	{
		return Handle == other.Handle;
	}
	
	bool MeshFilter::operator!=(const MeshFilter& other) const
	{
		return Handle != other.Handle;
	}
	
	UnityEngine::Mesh UnityEngine::MeshFilter::GetMesh()
	{
		auto returnValue = Plugin::UnityEngineMeshFilterPropertyGetMesh(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return UnityEngine::Mesh(Plugin::InternalUse::Only, returnValue);
	}
	
	void UnityEngine::MeshFilter::SetMesh(UnityEngine::Mesh& value)
	{
		Plugin::UnityEngineMeshFilterPropertySetMesh(Handle, value.Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
}

namespace UnityEngine
{
	Renderer::Renderer(decltype(nullptr))
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
	{
	}
	
	Renderer::Renderer(Plugin::InternalUse, int32_t handle)
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	Renderer::Renderer(const Renderer& other)
		: Renderer(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	Renderer::Renderer(Renderer&& other)
		: Renderer(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	Renderer::~Renderer()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	Renderer& Renderer::operator=(const Renderer& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	Renderer& Renderer::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	Renderer& Renderer::operator=(Renderer&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool Renderer::operator==(const Renderer& other) const
	{
		return Handle == other.Handle;
	}
	
	bool Renderer::operator!=(const Renderer& other) const
	{
		return Handle != other.Handle;
	}
}

namespace UnityEngine
{
	MeshRenderer::MeshRenderer(decltype(nullptr))
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
		, UnityEngine::Renderer(nullptr)
	{
	}
	
	MeshRenderer::MeshRenderer(Plugin::InternalUse, int32_t handle)
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
		, UnityEngine::Renderer(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	MeshRenderer::MeshRenderer(const MeshRenderer& other)
		: MeshRenderer(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	MeshRenderer::MeshRenderer(MeshRenderer&& other)
		: MeshRenderer(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	MeshRenderer::~MeshRenderer()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	MeshRenderer& MeshRenderer::operator=(const MeshRenderer& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	MeshRenderer& MeshRenderer::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	MeshRenderer& MeshRenderer::operator=(MeshRenderer&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool MeshRenderer::operator==(const MeshRenderer& other) const
	{
		return Handle == other.Handle;
	}
	
	bool MeshRenderer::operator!=(const MeshRenderer& other) const
	{
		return Handle != other.Handle;
	}
	
	UnityEngine::Material UnityEngine::MeshRenderer::GetMaterial()
	{
		auto returnValue = Plugin::UnityEngineMeshRendererPropertyGetMaterial(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return UnityEngine::Material(Plugin::InternalUse::Only, returnValue);
	}
	
	void UnityEngine::MeshRenderer::SetMaterial(UnityEngine::Material& value)
	{
		Plugin::UnityEngineMeshRendererPropertySetMaterial(Handle, value.Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
}

namespace System
{
	Exception::Exception(decltype(nullptr))
		: System::Runtime::InteropServices::_Exception(nullptr)
		, System::Runtime::Serialization::ISerializable(nullptr)
	{
	}
	
	Exception::Exception(Plugin::InternalUse, int32_t handle)
		: System::Runtime::InteropServices::_Exception(nullptr)
		, System::Runtime::Serialization::ISerializable(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	Exception::Exception(const Exception& other)
		: Exception(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	Exception::Exception(Exception&& other)
		: Exception(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	Exception::~Exception()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	Exception& Exception::operator=(const Exception& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	Exception& Exception::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	Exception& Exception::operator=(Exception&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool Exception::operator==(const Exception& other) const
	{
		return Handle == other.Handle;
	}
	
	bool Exception::operator!=(const Exception& other) const
	{
		return Handle != other.Handle;
	}
	
	System::Exception::Exception(System::String& message)
		: System::Runtime::InteropServices::_Exception(nullptr)
		, System::Runtime::Serialization::ISerializable(nullptr)
	{
		auto returnValue = Plugin::SystemExceptionConstructorSystemString(message.Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		Handle = returnValue;
		if (returnValue)
		{
			Plugin::ReferenceManagedClass(returnValue);
		}
	}
}

namespace System
{
	SystemException::SystemException(decltype(nullptr))
		: System::Runtime::InteropServices::_Exception(nullptr)
		, System::Runtime::Serialization::ISerializable(nullptr)
		, System::Exception(nullptr)
	{
	}
	
	SystemException::SystemException(Plugin::InternalUse, int32_t handle)
		: System::Runtime::InteropServices::_Exception(nullptr)
		, System::Runtime::Serialization::ISerializable(nullptr)
		, System::Exception(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	SystemException::SystemException(const SystemException& other)
		: SystemException(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	SystemException::SystemException(SystemException&& other)
		: SystemException(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	SystemException::~SystemException()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	SystemException& SystemException::operator=(const SystemException& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	SystemException& SystemException::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	SystemException& SystemException::operator=(SystemException&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool SystemException::operator==(const SystemException& other) const
	{
		return Handle == other.Handle;
	}
	
	bool SystemException::operator!=(const SystemException& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	NullReferenceException::NullReferenceException(decltype(nullptr))
		: System::Runtime::InteropServices::_Exception(nullptr)
		, System::Runtime::Serialization::ISerializable(nullptr)
		, System::Exception(nullptr)
		, System::SystemException(nullptr)
	{
	}
	
	NullReferenceException::NullReferenceException(Plugin::InternalUse, int32_t handle)
		: System::Runtime::InteropServices::_Exception(nullptr)
		, System::Runtime::Serialization::ISerializable(nullptr)
		, System::Exception(nullptr)
		, System::SystemException(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	NullReferenceException::NullReferenceException(const NullReferenceException& other)
		: NullReferenceException(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	NullReferenceException::NullReferenceException(NullReferenceException&& other)
		: NullReferenceException(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	NullReferenceException::~NullReferenceException()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	NullReferenceException& NullReferenceException::operator=(const NullReferenceException& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	NullReferenceException& NullReferenceException::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	NullReferenceException& NullReferenceException::operator=(NullReferenceException&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool NullReferenceException::operator==(const NullReferenceException& other) const
	{
		return Handle == other.Handle;
	}
	
	bool NullReferenceException::operator!=(const NullReferenceException& other) const
	{
		return Handle != other.Handle;
	}
}

namespace UnityEngine
{
	PrimitiveType::PrimitiveType(int32_t value)
		: Value(value)
	{
	}
	
	UnityEngine::PrimitiveType::operator int32_t() const
	{
		return Value;
	}
	
	bool UnityEngine::PrimitiveType::operator==(PrimitiveType other)
	{
		return Value == other.Value;
	}
	
	bool UnityEngine::PrimitiveType::operator!=(PrimitiveType other)
	{
		return Value != other.Value;
	}
	
	UnityEngine::PrimitiveType::operator System::Enum()
	{
		int32_t handle = Plugin::BoxPrimitiveType(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::Enum(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	UnityEngine::PrimitiveType::operator System::ValueType()
	{
		int32_t handle = Plugin::BoxPrimitiveType(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::ValueType(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	UnityEngine::PrimitiveType::operator System::Object()
	{
		int32_t handle = Plugin::BoxPrimitiveType(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::Object(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	UnityEngine::PrimitiveType::operator System::IFormattable()
	{
		int32_t handle = Plugin::BoxPrimitiveType(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::IFormattable(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	UnityEngine::PrimitiveType::operator System::IComparable()
	{
		int32_t handle = Plugin::BoxPrimitiveType(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::IComparable(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	UnityEngine::PrimitiveType::operator System::IConvertible()
	{
		int32_t handle = Plugin::BoxPrimitiveType(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::IConvertible(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
}
const UnityEngine::PrimitiveType UnityEngine::PrimitiveType::Sphere(0);
const UnityEngine::PrimitiveType UnityEngine::PrimitiveType::Capsule(1);
const UnityEngine::PrimitiveType UnityEngine::PrimitiveType::Cylinder(2);
const UnityEngine::PrimitiveType UnityEngine::PrimitiveType::Cube(3);
const UnityEngine::PrimitiveType UnityEngine::PrimitiveType::Plane(4);
const UnityEngine::PrimitiveType UnityEngine::PrimitiveType::Quad(5);

namespace System
{
	System::Object::operator UnityEngine::PrimitiveType()
	{
		UnityEngine::PrimitiveType returnVal(Plugin::UnboxPrimitiveType(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace UnityEngine
{
	Time::Time(decltype(nullptr))
	{
	}
	
	Time::Time(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	Time::Time(const Time& other)
		: Time(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	Time::Time(Time&& other)
		: Time(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	Time::~Time()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	Time& Time::operator=(const Time& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	Time& Time::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	Time& Time::operator=(Time&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool Time::operator==(const Time& other) const
	{
		return Handle == other.Handle;
	}
	
	bool Time::operator!=(const Time& other) const
	{
		return Handle != other.Handle;
	}
	
	System::Single UnityEngine::Time::GetDeltaTime()
	{
		auto returnValue = Plugin::UnityEngineTimePropertyGetDeltaTime();
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnValue;
	}
}

namespace UnityEngine
{
	Camera::Camera(decltype(nullptr))
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
		, UnityEngine::Behaviour(nullptr)
	{
	}
	
	Camera::Camera(Plugin::InternalUse, int32_t handle)
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
		, UnityEngine::Behaviour(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	Camera::Camera(const Camera& other)
		: Camera(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	Camera::Camera(Camera&& other)
		: Camera(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	Camera::~Camera()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	Camera& Camera::operator=(const Camera& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	Camera& Camera::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	Camera& Camera::operator=(Camera&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool Camera::operator==(const Camera& other) const
	{
		return Handle == other.Handle;
	}
	
	bool Camera::operator!=(const Camera& other) const
	{
		return Handle != other.Handle;
	}
	
	UnityEngine::Camera UnityEngine::Camera::GetMain()
	{
		auto returnValue = Plugin::UnityEngineCameraPropertyGetMain();
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return UnityEngine::Camera(Plugin::InternalUse::Only, returnValue);
	}
	
	System::Single UnityEngine::Camera::GetFieldOfView()
	{
		auto returnValue = Plugin::UnityEngineCameraPropertyGetFieldOfView(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnValue;
	}
	
	void UnityEngine::Camera::SetFieldOfView(System::Single value)
	{
		Plugin::UnityEngineCameraPropertySetFieldOfView(Handle, value);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	System::Single UnityEngine::Camera::GetAspect()
	{
		auto returnValue = Plugin::UnityEngineCameraPropertyGetAspect(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnValue;
	}
	
	void UnityEngine::Camera::SetAspect(System::Single value)
	{
		Plugin::UnityEngineCameraPropertySetAspect(Handle, value);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	System::Int32 UnityEngine::Camera::GetPixelWidth()
	{
		auto returnValue = Plugin::UnityEngineCameraPropertyGetPixelWidth(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnValue;
	}
	
	System::Int32 UnityEngine::Camera::GetPixelHeight()
	{
		auto returnValue = Plugin::UnityEngineCameraPropertyGetPixelHeight(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnValue;
	}
}

namespace UnityEngine
{
	YieldInstruction::YieldInstruction(decltype(nullptr))
	{
	}
	
	YieldInstruction::YieldInstruction(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	YieldInstruction::YieldInstruction(const YieldInstruction& other)
		: YieldInstruction(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	YieldInstruction::YieldInstruction(YieldInstruction&& other)
		: YieldInstruction(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	YieldInstruction::~YieldInstruction()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	YieldInstruction& YieldInstruction::operator=(const YieldInstruction& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	YieldInstruction& YieldInstruction::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	YieldInstruction& YieldInstruction::operator=(YieldInstruction&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool YieldInstruction::operator==(const YieldInstruction& other) const
	{
		return Handle == other.Handle;
	}
	
	bool YieldInstruction::operator!=(const YieldInstruction& other) const
	{
		return Handle != other.Handle;
	}
}

namespace UnityEngine
{
	AsyncOperation::AsyncOperation(decltype(nullptr))
		: UnityEngine::YieldInstruction(nullptr)
	{
	}
	
	AsyncOperation::AsyncOperation(Plugin::InternalUse, int32_t handle)
		: UnityEngine::YieldInstruction(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	AsyncOperation::AsyncOperation(const AsyncOperation& other)
		: AsyncOperation(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	AsyncOperation::AsyncOperation(AsyncOperation&& other)
		: AsyncOperation(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	AsyncOperation::~AsyncOperation()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	AsyncOperation& AsyncOperation::operator=(const AsyncOperation& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	AsyncOperation& AsyncOperation::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	AsyncOperation& AsyncOperation::operator=(AsyncOperation&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool AsyncOperation::operator==(const AsyncOperation& other) const
	{
		return Handle == other.Handle;
	}
	
	bool AsyncOperation::operator!=(const AsyncOperation& other) const
	{
		return Handle != other.Handle;
	}
}

namespace UnityEngine
{
	namespace Networking
	{
		DownloadHandler::DownloadHandler(decltype(nullptr))
			: System::IDisposable(nullptr)
		{
		}
		
		DownloadHandler::DownloadHandler(Plugin::InternalUse, int32_t handle)
			: System::IDisposable(nullptr)
		{
			Handle = handle;
			if (handle)
			{
				Plugin::ReferenceManagedClass(handle);
			}
		}
		
		DownloadHandler::DownloadHandler(const DownloadHandler& other)
			: DownloadHandler(Plugin::InternalUse::Only, other.Handle)
		{
		}
		
		DownloadHandler::DownloadHandler(DownloadHandler&& other)
			: DownloadHandler(Plugin::InternalUse::Only, other.Handle)
		{
			other.Handle = 0;
		}
		
		DownloadHandler::~DownloadHandler()
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
		}
		
		DownloadHandler& DownloadHandler::operator=(const DownloadHandler& other)
		{
			if (this->Handle)
			{
				Plugin::DereferenceManagedClass(this->Handle);
			}
			this->Handle = other.Handle;
			if (this->Handle)
			{
				Plugin::ReferenceManagedClass(this->Handle);
			}
			return *this;
		}
		
		DownloadHandler& DownloadHandler::operator=(decltype(nullptr))
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
			return *this;
		}
		
		DownloadHandler& DownloadHandler::operator=(DownloadHandler&& other)
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
			}
			Handle = other.Handle;
			other.Handle = 0;
			return *this;
		}
		
		bool DownloadHandler::operator==(const DownloadHandler& other) const
		{
			return Handle == other.Handle;
		}
		
		bool DownloadHandler::operator!=(const DownloadHandler& other) const
		{
			return Handle != other.Handle;
		}
	}
}

namespace UnityEngine
{
	namespace Networking
	{
		DownloadHandlerScript::DownloadHandlerScript(decltype(nullptr))
			: System::IDisposable(nullptr)
			, UnityEngine::Networking::DownloadHandler(nullptr)
		{
		}
		
		DownloadHandlerScript::DownloadHandlerScript(Plugin::InternalUse, int32_t handle)
			: System::IDisposable(nullptr)
			, UnityEngine::Networking::DownloadHandler(nullptr)
		{
			Handle = handle;
			if (handle)
			{
				Plugin::ReferenceManagedClass(handle);
			}
		}
		
		DownloadHandlerScript::DownloadHandlerScript(const DownloadHandlerScript& other)
			: DownloadHandlerScript(Plugin::InternalUse::Only, other.Handle)
		{
		}
		
		DownloadHandlerScript::DownloadHandlerScript(DownloadHandlerScript&& other)
			: DownloadHandlerScript(Plugin::InternalUse::Only, other.Handle)
		{
			other.Handle = 0;
		}
		
		DownloadHandlerScript::~DownloadHandlerScript()
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
		}
		
		DownloadHandlerScript& DownloadHandlerScript::operator=(const DownloadHandlerScript& other)
		{
			if (this->Handle)
			{
				Plugin::DereferenceManagedClass(this->Handle);
			}
			this->Handle = other.Handle;
			if (this->Handle)
			{
				Plugin::ReferenceManagedClass(this->Handle);
			}
			return *this;
		}
		
		DownloadHandlerScript& DownloadHandlerScript::operator=(decltype(nullptr))
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
			return *this;
		}
		
		DownloadHandlerScript& DownloadHandlerScript::operator=(DownloadHandlerScript&& other)
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
			}
			Handle = other.Handle;
			other.Handle = 0;
			return *this;
		}
		
		bool DownloadHandlerScript::operator==(const DownloadHandlerScript& other) const
		{
			return Handle == other.Handle;
		}
		
		bool DownloadHandlerScript::operator!=(const DownloadHandlerScript& other) const
		{
			return Handle != other.Handle;
		}
	}
}

namespace CesiumForUnity
{
	RawDownloadedData::RawDownloadedData()
	{
	}
	
	CesiumForUnity::RawDownloadedData::operator System::ValueType()
	{
		int32_t handle = Plugin::BoxRawDownloadedData(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::ValueType(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
	
	CesiumForUnity::RawDownloadedData::operator System::Object()
	{
		int32_t handle = Plugin::BoxRawDownloadedData(*this);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
			return System::Object(Plugin::InternalUse::Only, handle);
		}
		return nullptr;
	}
}

namespace System
{
	System::Object::operator CesiumForUnity::RawDownloadedData()
	{
		CesiumForUnity::RawDownloadedData returnVal(Plugin::UnboxRawDownloadedData(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace CesiumForUnity
{
	AbstractBaseNativeDownloadHandler::AbstractBaseNativeDownloadHandler(decltype(nullptr))
		: System::IDisposable(nullptr)
		, UnityEngine::Networking::DownloadHandler(nullptr)
		, UnityEngine::Networking::DownloadHandlerScript(nullptr)
	{
	}
	
	AbstractBaseNativeDownloadHandler::AbstractBaseNativeDownloadHandler(Plugin::InternalUse, int32_t handle)
		: System::IDisposable(nullptr)
		, UnityEngine::Networking::DownloadHandler(nullptr)
		, UnityEngine::Networking::DownloadHandlerScript(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	AbstractBaseNativeDownloadHandler::AbstractBaseNativeDownloadHandler(const AbstractBaseNativeDownloadHandler& other)
		: AbstractBaseNativeDownloadHandler(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	AbstractBaseNativeDownloadHandler::AbstractBaseNativeDownloadHandler(AbstractBaseNativeDownloadHandler&& other)
		: AbstractBaseNativeDownloadHandler(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	AbstractBaseNativeDownloadHandler::~AbstractBaseNativeDownloadHandler()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	AbstractBaseNativeDownloadHandler& AbstractBaseNativeDownloadHandler::operator=(const AbstractBaseNativeDownloadHandler& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	AbstractBaseNativeDownloadHandler& AbstractBaseNativeDownloadHandler::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	AbstractBaseNativeDownloadHandler& AbstractBaseNativeDownloadHandler::operator=(AbstractBaseNativeDownloadHandler&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool AbstractBaseNativeDownloadHandler::operator==(const AbstractBaseNativeDownloadHandler& other) const
	{
		return Handle == other.Handle;
	}
	
	bool AbstractBaseNativeDownloadHandler::operator!=(const AbstractBaseNativeDownloadHandler& other) const
	{
		return Handle != other.Handle;
	}
}

namespace CesiumForUnity
{
	CesiumForUnity::BaseNativeDownloadHandler::BaseNativeDownloadHandler()
		: System::IDisposable(nullptr)
		, UnityEngine::Networking::DownloadHandler(nullptr)
		, UnityEngine::Networking::DownloadHandlerScript(nullptr)
		, CesiumForUnity::AbstractBaseNativeDownloadHandler(nullptr)
	{
		CppHandle = Plugin::StoreBaseNativeDownloadHandler(this);
		System::Int32* handle = (System::Int32*)&Handle;
		int32_t cppHandle = CppHandle;
		Plugin::BaseNativeDownloadHandlerConstructor(cppHandle, &handle->Value);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (Handle)
		{
			Plugin::ReferenceManagedClass(Handle);
		}
		else
		{
			Plugin::RemoveBaseNativeDownloadHandler(CppHandle);
			CppHandle = 0;
		}
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	BaseNativeDownloadHandler::BaseNativeDownloadHandler(decltype(nullptr))
		: System::IDisposable(nullptr)
		, UnityEngine::Networking::DownloadHandler(nullptr)
		, UnityEngine::Networking::DownloadHandlerScript(nullptr)
		, CesiumForUnity::AbstractBaseNativeDownloadHandler(nullptr)
	{
		CppHandle = Plugin::StoreBaseNativeDownloadHandler(this);
	}
	
	CesiumForUnity::BaseNativeDownloadHandler::BaseNativeDownloadHandler(const CesiumForUnity::BaseNativeDownloadHandler& other)
		: System::IDisposable(nullptr)
		, UnityEngine::Networking::DownloadHandler(nullptr)
		, UnityEngine::Networking::DownloadHandlerScript(nullptr)
		, CesiumForUnity::AbstractBaseNativeDownloadHandler(nullptr)
	{
		Handle = other.Handle;
		CppHandle = Plugin::StoreBaseNativeDownloadHandler(this);
		if (Handle)
		{
			Plugin::ReferenceManagedClass(Handle);
		}
	}
	
	CesiumForUnity::BaseNativeDownloadHandler::BaseNativeDownloadHandler(CesiumForUnity::BaseNativeDownloadHandler&& other)
		: System::IDisposable(nullptr)
		, UnityEngine::Networking::DownloadHandler(nullptr)
		, UnityEngine::Networking::DownloadHandlerScript(nullptr)
		, CesiumForUnity::AbstractBaseNativeDownloadHandler(nullptr)
	{
		Handle = other.Handle;
		CppHandle = other.CppHandle;
		other.Handle = 0;
		other.CppHandle = 0;
	}
	
	CesiumForUnity::BaseNativeDownloadHandler::BaseNativeDownloadHandler(Plugin::InternalUse, int32_t handle)
		: System::IDisposable(nullptr)
		, UnityEngine::Networking::DownloadHandler(nullptr)
		, UnityEngine::Networking::DownloadHandlerScript(nullptr)
		, CesiumForUnity::AbstractBaseNativeDownloadHandler(nullptr)
	{
		Handle = handle;
		CppHandle = Plugin::StoreBaseNativeDownloadHandler(this);
		if (Handle)
		{
			Plugin::ReferenceManagedClass(Handle);
		}
	}
	
	CesiumForUnity::BaseNativeDownloadHandler::~BaseNativeDownloadHandler()
	{
		Plugin::RemoveWholeBaseNativeDownloadHandler(this);
		Plugin::RemoveBaseNativeDownloadHandler(CppHandle);
		CppHandle = 0;
		if (Handle)
		{
			int32_t handle = Handle;
			Handle = 0;
			if (Plugin::DereferenceManagedClassNoRelease(handle))
			{
				Plugin::ReleaseBaseNativeDownloadHandler(handle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
			}
		}
	}
	
	CesiumForUnity::BaseNativeDownloadHandler& CesiumForUnity::BaseNativeDownloadHandler::operator=(const CesiumForUnity::BaseNativeDownloadHandler& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	CesiumForUnity::BaseNativeDownloadHandler& CesiumForUnity::BaseNativeDownloadHandler::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			int32_t handle = Handle;
			Handle = 0;
			if (Plugin::DereferenceManagedClassNoRelease(handle))
			{
				Plugin::ReleaseBaseNativeDownloadHandler(handle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
			}
		}
		Handle = 0;
		return *this;
	}
	
	CesiumForUnity::BaseNativeDownloadHandler& CesiumForUnity::BaseNativeDownloadHandler::operator=(CesiumForUnity::BaseNativeDownloadHandler&& other)
	{
		Plugin::RemoveBaseNativeDownloadHandler(CppHandle);
		CppHandle = 0;
		if (Handle)
		{
			int32_t handle = Handle;
			Handle = 0;
			if (Plugin::DereferenceManagedClassNoRelease(handle))
			{
				Plugin::ReleaseBaseNativeDownloadHandler(handle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
			}
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool CesiumForUnity::BaseNativeDownloadHandler::operator==(const CesiumForUnity::BaseNativeDownloadHandler& other) const
	{
		return Handle == other.Handle;
	}
	
	bool CesiumForUnity::BaseNativeDownloadHandler::operator!=(const CesiumForUnity::BaseNativeDownloadHandler& other) const
	{
		return Handle != other.Handle;
	}
	
	DLLEXPORT int32_t NewBaseNativeDownloadHandler(int32_t handle)
	{
		CesiumForUnity::BaseNativeDownloadHandler* memory = Plugin::StoreWholeBaseNativeDownloadHandler();
		CesiumForUnity::NativeDownloadHandler* thiz = new (memory) CesiumForUnity::NativeDownloadHandler(Plugin::InternalUse::Only, handle);
		return thiz->CppHandle;
	}

	DLLEXPORT void DestroyBaseNativeDownloadHandler(int32_t cppHandle)
	{
		CesiumForUnity::BaseNativeDownloadHandler* instance = Plugin::GetBaseNativeDownloadHandler(cppHandle);
		instance->~BaseNativeDownloadHandler();
	}

	System::Boolean CesiumForUnity::BaseNativeDownloadHandler::ReceiveDataNative(void* data, System::Int32 dataLength)
	{
		return {};
	}
	
	DLLEXPORT int32_t CesiumForUnityAbstractBaseNativeDownloadHandlerReceiveDataNative(int32_t cppHandle, void* data, int32_t dataLength)
	{
		try
		{
			return Plugin::GetBaseNativeDownloadHandler(cppHandle)->ReceiveDataNative(data, dataLength);
		}
		catch (System::Exception ex)
		{
			Plugin::SetException(ex.Handle);
			return {};
		}
		catch (...)
		{
			System::String msg = "Unhandled exception invoking CesiumForUnity::AbstractBaseNativeDownloadHandler";
			System::Exception ex(msg);
			Plugin::SetException(ex.Handle);
			return {};
		}
	}
}

namespace UnityEngine
{
	namespace Networking
	{
		UnityWebRequest::UnityWebRequest(decltype(nullptr))
			: System::IDisposable(nullptr)
		{
		}
		
		UnityWebRequest::UnityWebRequest(Plugin::InternalUse, int32_t handle)
			: System::IDisposable(nullptr)
		{
			Handle = handle;
			if (handle)
			{
				Plugin::ReferenceManagedClass(handle);
			}
		}
		
		UnityWebRequest::UnityWebRequest(const UnityWebRequest& other)
			: UnityWebRequest(Plugin::InternalUse::Only, other.Handle)
		{
		}
		
		UnityWebRequest::UnityWebRequest(UnityWebRequest&& other)
			: UnityWebRequest(Plugin::InternalUse::Only, other.Handle)
		{
			other.Handle = 0;
		}
		
		UnityWebRequest::~UnityWebRequest()
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
		}
		
		UnityWebRequest& UnityWebRequest::operator=(const UnityWebRequest& other)
		{
			if (this->Handle)
			{
				Plugin::DereferenceManagedClass(this->Handle);
			}
			this->Handle = other.Handle;
			if (this->Handle)
			{
				Plugin::ReferenceManagedClass(this->Handle);
			}
			return *this;
		}
		
		UnityWebRequest& UnityWebRequest::operator=(decltype(nullptr))
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
			return *this;
		}
		
		UnityWebRequest& UnityWebRequest::operator=(UnityWebRequest&& other)
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
			}
			Handle = other.Handle;
			other.Handle = 0;
			return *this;
		}
		
		bool UnityWebRequest::operator==(const UnityWebRequest& other) const
		{
			return Handle == other.Handle;
		}
		
		bool UnityWebRequest::operator!=(const UnityWebRequest& other) const
		{
			return Handle != other.Handle;
		}
		
		System::String UnityEngine::Networking::UnityWebRequest::GetError()
		{
			auto returnValue = Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetError(Handle);
			if (Plugin::unhandledCsharpException)
			{
				System::Exception* ex = Plugin::unhandledCsharpException;
				Plugin::unhandledCsharpException = nullptr;
				ex->ThrowReferenceToThis();
				delete ex;
			}
			return System::String(Plugin::InternalUse::Only, returnValue);
		}
		
		System::Boolean UnityEngine::Networking::UnityWebRequest::GetIsDone()
		{
			auto returnValue = Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetIsDone(Handle);
			if (Plugin::unhandledCsharpException)
			{
				System::Exception* ex = Plugin::unhandledCsharpException;
				Plugin::unhandledCsharpException = nullptr;
				ex->ThrowReferenceToThis();
				delete ex;
			}
			return returnValue;
		}
		
		System::Int64 UnityEngine::Networking::UnityWebRequest::GetResponseCode()
		{
			auto returnValue = Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetResponseCode(Handle);
			if (Plugin::unhandledCsharpException)
			{
				System::Exception* ex = Plugin::unhandledCsharpException;
				Plugin::unhandledCsharpException = nullptr;
				ex->ThrowReferenceToThis();
				delete ex;
			}
			return returnValue;
		}
		
		System::String UnityEngine::Networking::UnityWebRequest::GetUrl()
		{
			auto returnValue = Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetUrl(Handle);
			if (Plugin::unhandledCsharpException)
			{
				System::Exception* ex = Plugin::unhandledCsharpException;
				Plugin::unhandledCsharpException = nullptr;
				ex->ThrowReferenceToThis();
				delete ex;
			}
			return System::String(Plugin::InternalUse::Only, returnValue);
		}
		
		void UnityEngine::Networking::UnityWebRequest::SetUrl(System::String& value)
		{
			Plugin::UnityEngineNetworkingUnityWebRequestPropertySetUrl(Handle, value.Handle);
			if (Plugin::unhandledCsharpException)
			{
				System::Exception* ex = Plugin::unhandledCsharpException;
				Plugin::unhandledCsharpException = nullptr;
				ex->ThrowReferenceToThis();
				delete ex;
			}
		}
		
		System::String UnityEngine::Networking::UnityWebRequest::GetMethod()
		{
			auto returnValue = Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetMethod(Handle);
			if (Plugin::unhandledCsharpException)
			{
				System::Exception* ex = Plugin::unhandledCsharpException;
				Plugin::unhandledCsharpException = nullptr;
				ex->ThrowReferenceToThis();
				delete ex;
			}
			return System::String(Plugin::InternalUse::Only, returnValue);
		}
		
		void UnityEngine::Networking::UnityWebRequest::SetMethod(System::String& value)
		{
			Plugin::UnityEngineNetworkingUnityWebRequestPropertySetMethod(Handle, value.Handle);
			if (Plugin::unhandledCsharpException)
			{
				System::Exception* ex = Plugin::unhandledCsharpException;
				Plugin::unhandledCsharpException = nullptr;
				ex->ThrowReferenceToThis();
				delete ex;
			}
		}
		
		UnityEngine::Networking::DownloadHandler UnityEngine::Networking::UnityWebRequest::GetDownloadHandler()
		{
			auto returnValue = Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetDownloadHandler(Handle);
			if (Plugin::unhandledCsharpException)
			{
				System::Exception* ex = Plugin::unhandledCsharpException;
				Plugin::unhandledCsharpException = nullptr;
				ex->ThrowReferenceToThis();
				delete ex;
			}
			return UnityEngine::Networking::DownloadHandler(Plugin::InternalUse::Only, returnValue);
		}
		
		void UnityEngine::Networking::UnityWebRequest::SetDownloadHandler(UnityEngine::Networking::DownloadHandler& value)
		{
			Plugin::UnityEngineNetworkingUnityWebRequestPropertySetDownloadHandler(Handle, value.Handle);
			if (Plugin::unhandledCsharpException)
			{
				System::Exception* ex = Plugin::unhandledCsharpException;
				Plugin::unhandledCsharpException = nullptr;
				ex->ThrowReferenceToThis();
				delete ex;
			}
		}
		
		UnityEngine::Networking::UnityWebRequest UnityEngine::Networking::UnityWebRequest::Get(System::String& uri)
		{
			auto returnValue = Plugin::UnityEngineNetworkingUnityWebRequestMethodGetSystemString(uri.Handle);
			if (Plugin::unhandledCsharpException)
			{
				System::Exception* ex = Plugin::unhandledCsharpException;
				Plugin::unhandledCsharpException = nullptr;
				ex->ThrowReferenceToThis();
				delete ex;
			}
			return UnityEngine::Networking::UnityWebRequest(Plugin::InternalUse::Only, returnValue);
		}
	
		void UnityEngine::Networking::UnityWebRequest::SetRequestHeader(System::String& name, System::String& value)
		{
			Plugin::UnityEngineNetworkingUnityWebRequestMethodSetRequestHeaderSystemString_SystemString(Handle, name.Handle, value.Handle);
			if (Plugin::unhandledCsharpException)
			{
				System::Exception* ex = Plugin::unhandledCsharpException;
				Plugin::unhandledCsharpException = nullptr;
				ex->ThrowReferenceToThis();
				delete ex;
			}
		}
	
		UnityEngine::Networking::UnityWebRequestAsyncOperation UnityEngine::Networking::UnityWebRequest::SendWebRequest()
		{
			auto returnValue = Plugin::UnityEngineNetworkingUnityWebRequestMethodSendWebRequest(Handle);
			if (Plugin::unhandledCsharpException)
			{
				System::Exception* ex = Plugin::unhandledCsharpException;
				Plugin::unhandledCsharpException = nullptr;
				ex->ThrowReferenceToThis();
				delete ex;
			}
			return UnityEngine::Networking::UnityWebRequestAsyncOperation(Plugin::InternalUse::Only, returnValue);
		}
	
		System::String UnityEngine::Networking::UnityWebRequest::GetResponseHeader(System::String& name)
		{
			auto returnValue = Plugin::UnityEngineNetworkingUnityWebRequestMethodGetResponseHeaderSystemString(Handle, name.Handle);
			if (Plugin::unhandledCsharpException)
			{
				System::Exception* ex = Plugin::unhandledCsharpException;
				Plugin::unhandledCsharpException = nullptr;
				ex->ThrowReferenceToThis();
				delete ex;
			}
			return System::String(Plugin::InternalUse::Only, returnValue);
		}
	}
}

namespace UnityEngine
{
	namespace Networking
	{
		UnityWebRequestAsyncOperation::UnityWebRequestAsyncOperation(decltype(nullptr))
			: UnityEngine::YieldInstruction(nullptr)
			, UnityEngine::AsyncOperation(nullptr)
		{
		}
		
		UnityWebRequestAsyncOperation::UnityWebRequestAsyncOperation(Plugin::InternalUse, int32_t handle)
			: UnityEngine::YieldInstruction(nullptr)
			, UnityEngine::AsyncOperation(nullptr)
		{
			Handle = handle;
			if (handle)
			{
				Plugin::ReferenceManagedClass(handle);
			}
		}
		
		UnityWebRequestAsyncOperation::UnityWebRequestAsyncOperation(const UnityWebRequestAsyncOperation& other)
			: UnityWebRequestAsyncOperation(Plugin::InternalUse::Only, other.Handle)
		{
		}
		
		UnityWebRequestAsyncOperation::UnityWebRequestAsyncOperation(UnityWebRequestAsyncOperation&& other)
			: UnityWebRequestAsyncOperation(Plugin::InternalUse::Only, other.Handle)
		{
			other.Handle = 0;
		}
		
		UnityWebRequestAsyncOperation::~UnityWebRequestAsyncOperation()
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
		}
		
		UnityWebRequestAsyncOperation& UnityWebRequestAsyncOperation::operator=(const UnityWebRequestAsyncOperation& other)
		{
			if (this->Handle)
			{
				Plugin::DereferenceManagedClass(this->Handle);
			}
			this->Handle = other.Handle;
			if (this->Handle)
			{
				Plugin::ReferenceManagedClass(this->Handle);
			}
			return *this;
		}
		
		UnityWebRequestAsyncOperation& UnityWebRequestAsyncOperation::operator=(decltype(nullptr))
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
			return *this;
		}
		
		UnityWebRequestAsyncOperation& UnityWebRequestAsyncOperation::operator=(UnityWebRequestAsyncOperation&& other)
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
			}
			Handle = other.Handle;
			other.Handle = 0;
			return *this;
		}
		
		bool UnityWebRequestAsyncOperation::operator==(const UnityWebRequestAsyncOperation& other) const
		{
			return Handle == other.Handle;
		}
		
		bool UnityWebRequestAsyncOperation::operator!=(const UnityWebRequestAsyncOperation& other) const
		{
			return Handle != other.Handle;
		}
		
		void UnityEngine::Networking::UnityWebRequestAsyncOperation::AddCompleted(System::Action_1<UnityEngine::AsyncOperation>& del)
		{
			Plugin::UnityEngineNetworkingUnityWebRequestAsyncOperationAddEventCompleted(Handle, del.Handle);
			if (Plugin::unhandledCsharpException)
			{
				System::Exception* ex = Plugin::unhandledCsharpException;
				Plugin::unhandledCsharpException = nullptr;
				ex->ThrowReferenceToThis();
				delete ex;
			}
		}
	
		void UnityEngine::Networking::UnityWebRequestAsyncOperation::RemoveCompleted(System::Action_1<UnityEngine::AsyncOperation>& del)
		{
			Plugin::UnityEngineNetworkingUnityWebRequestAsyncOperationRemoveEventCompleted(Handle, del.Handle);
			if (Plugin::unhandledCsharpException)
			{
				System::Exception* ex = Plugin::unhandledCsharpException;
				Plugin::unhandledCsharpException = nullptr;
				ex->ThrowReferenceToThis();
				delete ex;
			}
		}
	}
}

namespace System
{
	namespace Runtime
	{
		namespace InteropServices
		{
			void* System::Runtime::InteropServices::Marshal::StringToCoTaskMemUTF8(System::String& s)
			{
				auto returnValue = Plugin::SystemRuntimeInteropServicesMarshalMethodStringToCoTaskMemUTF8SystemString(s.Handle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
				return returnValue;
			}
	
			void System::Runtime::InteropServices::Marshal::FreeCoTaskMem(void* ptr)
			{
				Plugin::SystemRuntimeInteropServicesMarshalMethodFreeCoTaskMemSystemIntPtr(ptr);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
			}
		}
	}
}

namespace CesiumForUnity
{
	AbstractBaseCesium3DTileset::AbstractBaseCesium3DTileset(decltype(nullptr))
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
		, UnityEngine::Behaviour(nullptr)
		, UnityEngine::MonoBehaviour(nullptr)
	{
	}
	
	AbstractBaseCesium3DTileset::AbstractBaseCesium3DTileset(Plugin::InternalUse, int32_t handle)
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
		, UnityEngine::Behaviour(nullptr)
		, UnityEngine::MonoBehaviour(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	AbstractBaseCesium3DTileset::AbstractBaseCesium3DTileset(const AbstractBaseCesium3DTileset& other)
		: AbstractBaseCesium3DTileset(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	AbstractBaseCesium3DTileset::AbstractBaseCesium3DTileset(AbstractBaseCesium3DTileset&& other)
		: AbstractBaseCesium3DTileset(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	AbstractBaseCesium3DTileset::~AbstractBaseCesium3DTileset()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	AbstractBaseCesium3DTileset& AbstractBaseCesium3DTileset::operator=(const AbstractBaseCesium3DTileset& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	AbstractBaseCesium3DTileset& AbstractBaseCesium3DTileset::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	AbstractBaseCesium3DTileset& AbstractBaseCesium3DTileset::operator=(AbstractBaseCesium3DTileset&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool AbstractBaseCesium3DTileset::operator==(const AbstractBaseCesium3DTileset& other) const
	{
		return Handle == other.Handle;
	}
	
	bool AbstractBaseCesium3DTileset::operator!=(const AbstractBaseCesium3DTileset& other) const
	{
		return Handle != other.Handle;
	}
}

namespace CesiumForUnity
{
	CesiumForUnity::BaseCesium3DTileset::BaseCesium3DTileset()
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
		, UnityEngine::Behaviour(nullptr)
		, UnityEngine::MonoBehaviour(nullptr)
		, CesiumForUnity::AbstractBaseCesium3DTileset(nullptr)
	{
		CppHandle = Plugin::StoreBaseCesium3DTileset(this);
		System::Int32* handle = (System::Int32*)&Handle;
		int32_t cppHandle = CppHandle;
		Plugin::BaseCesium3DTilesetConstructor(cppHandle, &handle->Value);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (Handle)
		{
			Plugin::ReferenceManagedClass(Handle);
		}
		else
		{
			Plugin::RemoveBaseCesium3DTileset(CppHandle);
			CppHandle = 0;
		}
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	BaseCesium3DTileset::BaseCesium3DTileset(decltype(nullptr))
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
		, UnityEngine::Behaviour(nullptr)
		, UnityEngine::MonoBehaviour(nullptr)
		, CesiumForUnity::AbstractBaseCesium3DTileset(nullptr)
	{
		CppHandle = Plugin::StoreBaseCesium3DTileset(this);
	}
	
	CesiumForUnity::BaseCesium3DTileset::BaseCesium3DTileset(const CesiumForUnity::BaseCesium3DTileset& other)
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
		, UnityEngine::Behaviour(nullptr)
		, UnityEngine::MonoBehaviour(nullptr)
		, CesiumForUnity::AbstractBaseCesium3DTileset(nullptr)
	{
		Handle = other.Handle;
		CppHandle = Plugin::StoreBaseCesium3DTileset(this);
		if (Handle)
		{
			Plugin::ReferenceManagedClass(Handle);
		}
	}
	
	CesiumForUnity::BaseCesium3DTileset::BaseCesium3DTileset(CesiumForUnity::BaseCesium3DTileset&& other)
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
		, UnityEngine::Behaviour(nullptr)
		, UnityEngine::MonoBehaviour(nullptr)
		, CesiumForUnity::AbstractBaseCesium3DTileset(nullptr)
	{
		Handle = other.Handle;
		CppHandle = other.CppHandle;
		other.Handle = 0;
		other.CppHandle = 0;
	}
	
	CesiumForUnity::BaseCesium3DTileset::BaseCesium3DTileset(Plugin::InternalUse, int32_t handle)
		: UnityEngine::Object(nullptr)
		, UnityEngine::Component(nullptr)
		, UnityEngine::Behaviour(nullptr)
		, UnityEngine::MonoBehaviour(nullptr)
		, CesiumForUnity::AbstractBaseCesium3DTileset(nullptr)
	{
		Handle = handle;
		CppHandle = Plugin::StoreBaseCesium3DTileset(this);
		if (Handle)
		{
			Plugin::ReferenceManagedClass(Handle);
		}
	}
	
	CesiumForUnity::BaseCesium3DTileset::~BaseCesium3DTileset()
	{
		Plugin::RemoveWholeBaseCesium3DTileset(this);
		Plugin::RemoveBaseCesium3DTileset(CppHandle);
		CppHandle = 0;
		if (Handle)
		{
			int32_t handle = Handle;
			Handle = 0;
			if (Plugin::DereferenceManagedClassNoRelease(handle))
			{
				Plugin::ReleaseBaseCesium3DTileset(handle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
			}
		}
	}
	
	CesiumForUnity::BaseCesium3DTileset& CesiumForUnity::BaseCesium3DTileset::operator=(const CesiumForUnity::BaseCesium3DTileset& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	CesiumForUnity::BaseCesium3DTileset& CesiumForUnity::BaseCesium3DTileset::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			int32_t handle = Handle;
			Handle = 0;
			if (Plugin::DereferenceManagedClassNoRelease(handle))
			{
				Plugin::ReleaseBaseCesium3DTileset(handle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
			}
		}
		Handle = 0;
		return *this;
	}
	
	CesiumForUnity::BaseCesium3DTileset& CesiumForUnity::BaseCesium3DTileset::operator=(CesiumForUnity::BaseCesium3DTileset&& other)
	{
		Plugin::RemoveBaseCesium3DTileset(CppHandle);
		CppHandle = 0;
		if (Handle)
		{
			int32_t handle = Handle;
			Handle = 0;
			if (Plugin::DereferenceManagedClassNoRelease(handle))
			{
				Plugin::ReleaseBaseCesium3DTileset(handle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
			}
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool CesiumForUnity::BaseCesium3DTileset::operator==(const CesiumForUnity::BaseCesium3DTileset& other) const
	{
		return Handle == other.Handle;
	}
	
	bool CesiumForUnity::BaseCesium3DTileset::operator!=(const CesiumForUnity::BaseCesium3DTileset& other) const
	{
		return Handle != other.Handle;
	}
	
	DLLEXPORT int32_t NewBaseCesium3DTileset(int32_t handle)
	{
		CesiumForUnity::BaseCesium3DTileset* memory = Plugin::StoreWholeBaseCesium3DTileset();
		CesiumForUnity::Cesium3DTileset* thiz = new (memory) CesiumForUnity::Cesium3DTileset(Plugin::InternalUse::Only, handle);
		return thiz->CppHandle;
	}

	DLLEXPORT void DestroyBaseCesium3DTileset(int32_t cppHandle)
	{
		CesiumForUnity::BaseCesium3DTileset* instance = Plugin::GetBaseCesium3DTileset(cppHandle);
		instance->~BaseCesium3DTileset();
	}

	void CesiumForUnity::BaseCesium3DTileset::Start()
	{
	}
	
	DLLEXPORT void CesiumForUnityAbstractBaseCesium3DTilesetStart(int32_t cppHandle)
	{
		try
		{
			Plugin::GetBaseCesium3DTileset(cppHandle)->Start();
		}
		catch (System::Exception ex)
		{
			Plugin::SetException(ex.Handle);
		}
		catch (...)
		{
			System::String msg = "Unhandled exception invoking CesiumForUnity::AbstractBaseCesium3DTileset";
			System::Exception ex(msg);
			Plugin::SetException(ex.Handle);
		}
	}
	
	void CesiumForUnity::BaseCesium3DTileset::Update()
	{
	}
	
	DLLEXPORT void CesiumForUnityAbstractBaseCesium3DTilesetUpdate(int32_t cppHandle)
	{
		try
		{
			Plugin::GetBaseCesium3DTileset(cppHandle)->Update();
		}
		catch (System::Exception ex)
		{
			Plugin::SetException(ex.Handle);
		}
		catch (...)
		{
			System::String msg = "Unhandled exception invoking CesiumForUnity::AbstractBaseCesium3DTileset";
			System::Exception ex(msg);
			Plugin::SetException(ex.Handle);
		}
	}
}

namespace System
{
	IAsyncResult::IAsyncResult(decltype(nullptr))
	{
	}
	
	IAsyncResult::IAsyncResult(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IAsyncResult::IAsyncResult(const IAsyncResult& other)
		: IAsyncResult(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IAsyncResult::IAsyncResult(IAsyncResult&& other)
		: IAsyncResult(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IAsyncResult::~IAsyncResult()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IAsyncResult& IAsyncResult::operator=(const IAsyncResult& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IAsyncResult& IAsyncResult::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IAsyncResult& IAsyncResult::operator=(IAsyncResult&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IAsyncResult::operator==(const IAsyncResult& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IAsyncResult::operator!=(const IAsyncResult& other) const
	{
		return Handle != other.Handle;
	}
}

namespace System
{
	namespace Threading
	{
		IThreadPoolWorkItem::IThreadPoolWorkItem(decltype(nullptr))
		{
		}
		
		IThreadPoolWorkItem::IThreadPoolWorkItem(Plugin::InternalUse, int32_t handle)
		{
			Handle = handle;
			if (handle)
			{
				Plugin::ReferenceManagedClass(handle);
			}
		}
		
		IThreadPoolWorkItem::IThreadPoolWorkItem(const IThreadPoolWorkItem& other)
			: IThreadPoolWorkItem(Plugin::InternalUse::Only, other.Handle)
		{
		}
		
		IThreadPoolWorkItem::IThreadPoolWorkItem(IThreadPoolWorkItem&& other)
			: IThreadPoolWorkItem(Plugin::InternalUse::Only, other.Handle)
		{
			other.Handle = 0;
		}
		
		IThreadPoolWorkItem::~IThreadPoolWorkItem()
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
		}
		
		IThreadPoolWorkItem& IThreadPoolWorkItem::operator=(const IThreadPoolWorkItem& other)
		{
			if (this->Handle)
			{
				Plugin::DereferenceManagedClass(this->Handle);
			}
			this->Handle = other.Handle;
			if (this->Handle)
			{
				Plugin::ReferenceManagedClass(this->Handle);
			}
			return *this;
		}
		
		IThreadPoolWorkItem& IThreadPoolWorkItem::operator=(decltype(nullptr))
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
			return *this;
		}
		
		IThreadPoolWorkItem& IThreadPoolWorkItem::operator=(IThreadPoolWorkItem&& other)
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
			}
			Handle = other.Handle;
			other.Handle = 0;
			return *this;
		}
		
		bool IThreadPoolWorkItem::operator==(const IThreadPoolWorkItem& other) const
		{
			return Handle == other.Handle;
		}
		
		bool IThreadPoolWorkItem::operator!=(const IThreadPoolWorkItem& other) const
		{
			return Handle != other.Handle;
		}
	}
}

namespace System
{
	namespace Threading
	{
		namespace Tasks
		{
			Task::Task(decltype(nullptr))
				: System::IAsyncResult(nullptr)
				, System::IDisposable(nullptr)
				, System::Threading::IThreadPoolWorkItem(nullptr)
			{
			}
			
			Task::Task(Plugin::InternalUse, int32_t handle)
				: System::IAsyncResult(nullptr)
				, System::IDisposable(nullptr)
				, System::Threading::IThreadPoolWorkItem(nullptr)
			{
				Handle = handle;
				if (handle)
				{
					Plugin::ReferenceManagedClass(handle);
				}
			}
			
			Task::Task(const Task& other)
				: Task(Plugin::InternalUse::Only, other.Handle)
			{
			}
			
			Task::Task(Task&& other)
				: Task(Plugin::InternalUse::Only, other.Handle)
			{
				other.Handle = 0;
			}
			
			Task::~Task()
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
			}
			
			Task& Task::operator=(const Task& other)
			{
				if (this->Handle)
				{
					Plugin::DereferenceManagedClass(this->Handle);
				}
				this->Handle = other.Handle;
				if (this->Handle)
				{
					Plugin::ReferenceManagedClass(this->Handle);
				}
				return *this;
			}
			
			Task& Task::operator=(decltype(nullptr))
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
					Handle = 0;
				}
				return *this;
			}
			
			Task& Task::operator=(Task&& other)
			{
				if (Handle)
				{
					Plugin::DereferenceManagedClass(Handle);
				}
				Handle = other.Handle;
				other.Handle = 0;
				return *this;
			}
			
			bool Task::operator==(const Task& other) const
			{
				return Handle == other.Handle;
			}
			
			bool Task::operator!=(const Task& other) const
			{
				return Handle != other.Handle;
			}
			
			System::Threading::Tasks::Task System::Threading::Tasks::Task::Run(System::Action& action)
			{
				auto returnValue = Plugin::SystemThreadingTasksTaskMethodRunSystemAction(action.Handle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
				return System::Threading::Tasks::Task(Plugin::InternalUse::Only, returnValue);
			}
		}
	}
}

namespace UnityEngine
{
	Material::Material(decltype(nullptr))
		: UnityEngine::Object(nullptr)
	{
	}
	
	Material::Material(Plugin::InternalUse, int32_t handle)
		: UnityEngine::Object(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	Material::Material(const Material& other)
		: Material(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	Material::Material(Material&& other)
		: Material(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	Material::~Material()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	Material& Material::operator=(const Material& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	Material& Material::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	Material& Material::operator=(Material&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool Material::operator==(const Material& other) const
	{
		return Handle == other.Handle;
	}
	
	bool Material::operator!=(const Material& other) const
	{
		return Handle != other.Handle;
	}
}

namespace UnityEngine
{
	Resources::Resources(decltype(nullptr))
	{
	}
	
	Resources::Resources(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	Resources::Resources(const Resources& other)
		: Resources(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	Resources::Resources(Resources&& other)
		: Resources(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	Resources::~Resources()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	Resources& Resources::operator=(const Resources& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	Resources& Resources::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	Resources& Resources::operator=(Resources&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool Resources::operator==(const Resources& other) const
	{
		return Handle == other.Handle;
	}
	
	bool Resources::operator!=(const Resources& other) const
	{
		return Handle != other.Handle;
	}
	
	template<> UnityEngine::Material UnityEngine::Resources::Load<UnityEngine::Material>(System::String& path)
	{
		auto returnValue = Plugin::UnityEngineResourcesMethodLoadUnityEngineMaterialSystemString(path.Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return UnityEngine::Material(Plugin::InternalUse::Only, returnValue);
	}
}

namespace UnityEngine
{
	ScriptableObject::ScriptableObject(decltype(nullptr))
		: UnityEngine::Object(nullptr)
	{
	}
	
	ScriptableObject::ScriptableObject(Plugin::InternalUse, int32_t handle)
		: UnityEngine::Object(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	ScriptableObject::ScriptableObject(const ScriptableObject& other)
		: ScriptableObject(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	ScriptableObject::ScriptableObject(ScriptableObject&& other)
		: ScriptableObject(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	ScriptableObject::~ScriptableObject()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	ScriptableObject& ScriptableObject::operator=(const ScriptableObject& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	ScriptableObject& ScriptableObject::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	ScriptableObject& ScriptableObject::operator=(ScriptableObject&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool ScriptableObject::operator==(const ScriptableObject& other) const
	{
		return Handle == other.Handle;
	}
	
	bool ScriptableObject::operator!=(const ScriptableObject& other) const
	{
		return Handle != other.Handle;
	}
}

namespace UnityEditor
{
	EditorWindow::EditorWindow(decltype(nullptr))
		: UnityEngine::Object(nullptr)
		, UnityEngine::ScriptableObject(nullptr)
	{
	}
	
	EditorWindow::EditorWindow(Plugin::InternalUse, int32_t handle)
		: UnityEngine::Object(nullptr)
		, UnityEngine::ScriptableObject(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	EditorWindow::EditorWindow(const EditorWindow& other)
		: EditorWindow(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	EditorWindow::EditorWindow(EditorWindow&& other)
		: EditorWindow(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	EditorWindow::~EditorWindow()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	EditorWindow& EditorWindow::operator=(const EditorWindow& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	EditorWindow& EditorWindow::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	EditorWindow& EditorWindow::operator=(EditorWindow&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool EditorWindow::operator==(const EditorWindow& other) const
	{
		return Handle == other.Handle;
	}
	
	bool EditorWindow::operator!=(const EditorWindow& other) const
	{
		return Handle != other.Handle;
	}
}

namespace UnityEditor
{
	SearchableEditorWindow::SearchableEditorWindow(decltype(nullptr))
		: UnityEngine::Object(nullptr)
		, UnityEngine::ScriptableObject(nullptr)
		, UnityEditor::EditorWindow(nullptr)
	{
	}
	
	SearchableEditorWindow::SearchableEditorWindow(Plugin::InternalUse, int32_t handle)
		: UnityEngine::Object(nullptr)
		, UnityEngine::ScriptableObject(nullptr)
		, UnityEditor::EditorWindow(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	SearchableEditorWindow::SearchableEditorWindow(const SearchableEditorWindow& other)
		: SearchableEditorWindow(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	SearchableEditorWindow::SearchableEditorWindow(SearchableEditorWindow&& other)
		: SearchableEditorWindow(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	SearchableEditorWindow::~SearchableEditorWindow()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	SearchableEditorWindow& SearchableEditorWindow::operator=(const SearchableEditorWindow& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	SearchableEditorWindow& SearchableEditorWindow::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	SearchableEditorWindow& SearchableEditorWindow::operator=(SearchableEditorWindow&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool SearchableEditorWindow::operator==(const SearchableEditorWindow& other) const
	{
		return Handle == other.Handle;
	}
	
	bool SearchableEditorWindow::operator!=(const SearchableEditorWindow& other) const
	{
		return Handle != other.Handle;
	}
}

namespace UnityEditor
{
	IHasCustomMenu::IHasCustomMenu(decltype(nullptr))
	{
	}
	
	IHasCustomMenu::IHasCustomMenu(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	IHasCustomMenu::IHasCustomMenu(const IHasCustomMenu& other)
		: IHasCustomMenu(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	IHasCustomMenu::IHasCustomMenu(IHasCustomMenu&& other)
		: IHasCustomMenu(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	IHasCustomMenu::~IHasCustomMenu()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	IHasCustomMenu& IHasCustomMenu::operator=(const IHasCustomMenu& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	IHasCustomMenu& IHasCustomMenu::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	IHasCustomMenu& IHasCustomMenu::operator=(IHasCustomMenu&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool IHasCustomMenu::operator==(const IHasCustomMenu& other) const
	{
		return Handle == other.Handle;
	}
	
	bool IHasCustomMenu::operator!=(const IHasCustomMenu& other) const
	{
		return Handle != other.Handle;
	}
}

namespace UnityEditor
{
	namespace Overlays
	{
		ISupportsOverlays::ISupportsOverlays(decltype(nullptr))
		{
		}
		
		ISupportsOverlays::ISupportsOverlays(Plugin::InternalUse, int32_t handle)
		{
			Handle = handle;
			if (handle)
			{
				Plugin::ReferenceManagedClass(handle);
			}
		}
		
		ISupportsOverlays::ISupportsOverlays(const ISupportsOverlays& other)
			: ISupportsOverlays(Plugin::InternalUse::Only, other.Handle)
		{
		}
		
		ISupportsOverlays::ISupportsOverlays(ISupportsOverlays&& other)
			: ISupportsOverlays(Plugin::InternalUse::Only, other.Handle)
		{
			other.Handle = 0;
		}
		
		ISupportsOverlays::~ISupportsOverlays()
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
		}
		
		ISupportsOverlays& ISupportsOverlays::operator=(const ISupportsOverlays& other)
		{
			if (this->Handle)
			{
				Plugin::DereferenceManagedClass(this->Handle);
			}
			this->Handle = other.Handle;
			if (this->Handle)
			{
				Plugin::ReferenceManagedClass(this->Handle);
			}
			return *this;
		}
		
		ISupportsOverlays& ISupportsOverlays::operator=(decltype(nullptr))
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
				Handle = 0;
			}
			return *this;
		}
		
		ISupportsOverlays& ISupportsOverlays::operator=(ISupportsOverlays&& other)
		{
			if (Handle)
			{
				Plugin::DereferenceManagedClass(Handle);
			}
			Handle = other.Handle;
			other.Handle = 0;
			return *this;
		}
		
		bool ISupportsOverlays::operator==(const ISupportsOverlays& other) const
		{
			return Handle == other.Handle;
		}
		
		bool ISupportsOverlays::operator!=(const ISupportsOverlays& other) const
		{
			return Handle != other.Handle;
		}
	}
}

namespace UnityEditor
{
	SceneView::SceneView(decltype(nullptr))
		: UnityEngine::Object(nullptr)
		, UnityEngine::ScriptableObject(nullptr)
		, UnityEditor::EditorWindow(nullptr)
		, UnityEditor::SearchableEditorWindow(nullptr)
		, UnityEditor::IHasCustomMenu(nullptr)
		, UnityEditor::Overlays::ISupportsOverlays(nullptr)
	{
	}
	
	SceneView::SceneView(Plugin::InternalUse, int32_t handle)
		: UnityEngine::Object(nullptr)
		, UnityEngine::ScriptableObject(nullptr)
		, UnityEditor::EditorWindow(nullptr)
		, UnityEditor::SearchableEditorWindow(nullptr)
		, UnityEditor::IHasCustomMenu(nullptr)
		, UnityEditor::Overlays::ISupportsOverlays(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
	}
	
	SceneView::SceneView(const SceneView& other)
		: SceneView(Plugin::InternalUse::Only, other.Handle)
	{
	}
	
	SceneView::SceneView(SceneView&& other)
		: SceneView(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
	}
	
	SceneView::~SceneView()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	SceneView& SceneView::operator=(const SceneView& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		return *this;
	}
	
	SceneView& SceneView::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	SceneView& SceneView::operator=(SceneView&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool SceneView::operator==(const SceneView& other) const
	{
		return Handle == other.Handle;
	}
	
	bool SceneView::operator!=(const SceneView& other) const
	{
		return Handle != other.Handle;
	}
	
	UnityEditor::SceneView UnityEditor::SceneView::GetLastActiveSceneView()
	{
		auto returnValue = Plugin::UnityEditorSceneViewPropertyGetLastActiveSceneView();
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return UnityEditor::SceneView(Plugin::InternalUse::Only, returnValue);
	}
	
	UnityEngine::Camera UnityEditor::SceneView::GetCamera()
	{
		auto returnValue = Plugin::UnityEditorSceneViewPropertyGetCamera(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return UnityEngine::Camera(Plugin::InternalUse::Only, returnValue);
	}
}

namespace System
{
	System::Object::operator System::Boolean()
	{
		System::Boolean returnVal(Plugin::UnboxBoolean(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace System
{
	System::Object::operator System::SByte()
	{
		System::SByte returnVal(Plugin::UnboxSByte(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace System
{
	System::Object::operator System::Byte()
	{
		System::Byte returnVal(Plugin::UnboxByte(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace System
{
	System::Object::operator System::Int16()
	{
		System::Int16 returnVal(Plugin::UnboxInt16(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace System
{
	System::Object::operator System::UInt16()
	{
		System::UInt16 returnVal(Plugin::UnboxUInt16(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace System
{
	System::Object::operator System::Int32()
	{
		System::Int32 returnVal(Plugin::UnboxInt32(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace System
{
	System::Object::operator System::UInt32()
	{
		System::UInt32 returnVal(Plugin::UnboxUInt32(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace System
{
	System::Object::operator System::Int64()
	{
		System::Int64 returnVal(Plugin::UnboxInt64(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace System
{
	System::Object::operator System::UInt64()
	{
		System::UInt64 returnVal(Plugin::UnboxUInt64(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace System
{
	System::Object::operator System::Char()
	{
		System::Char returnVal(Plugin::UnboxChar(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace System
{
	System::Object::operator System::Single()
	{
		System::Single returnVal(Plugin::UnboxSingle(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace System
{
	System::Object::operator System::Double()
	{
		System::Double returnVal(Plugin::UnboxDouble(Handle));
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnVal;
	}
}

namespace Plugin
{
	ArrayElementProxy1_1<UnityEngine::Vector3>::ArrayElementProxy1_1(Plugin::InternalUse, int32_t handle, int32_t index0)
	{
		Handle = handle;
		Index0 = index0;
	}
	
	void ArrayElementProxy1_1<UnityEngine::Vector3>::operator=(UnityEngine::Vector3 item)
	{
		Plugin::UnityEngineVector3Array1SetItem1(Handle, Index0, item);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	ArrayElementProxy1_1<UnityEngine::Vector3>::operator UnityEngine::Vector3()
	{
		auto returnValue = Plugin::UnityEngineVector3Array1GetItem1(Handle, Index0);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnValue;
	}
}

namespace System
{
	Array1<UnityEngine::Vector3>::Array1(decltype(nullptr))
		: System::ICloneable(nullptr)
		, System::Collections::IEnumerable(nullptr)
		, System::Collections::ICollection(nullptr)
		, System::Collections::IList(nullptr)
		, System::Collections::IStructuralComparable(nullptr)
		, System::Collections::IStructuralEquatable(nullptr)
		, System::Array(nullptr)
		, System::Collections::Generic::IEnumerable_1<UnityEngine::Vector3>(nullptr)
		, System::Collections::Generic::ICollection_1<UnityEngine::Vector3>(nullptr)
		, System::Collections::Generic::IList_1<UnityEngine::Vector3>(nullptr)
		, System::Collections::Generic::IReadOnlyCollection_1<UnityEngine::Vector3>(nullptr)
		, System::Collections::Generic::IReadOnlyList_1<UnityEngine::Vector3>(nullptr)
	{
		this->InternalLength = 0;
	}
	
	Array1<UnityEngine::Vector3>::Array1(Plugin::InternalUse, int32_t handle)
		: System::ICloneable(nullptr)
		, System::Collections::IEnumerable(nullptr)
		, System::Collections::ICollection(nullptr)
		, System::Collections::IList(nullptr)
		, System::Collections::IStructuralComparable(nullptr)
		, System::Collections::IStructuralEquatable(nullptr)
		, System::Array(nullptr)
		, System::Collections::Generic::IEnumerable_1<UnityEngine::Vector3>(nullptr)
		, System::Collections::Generic::ICollection_1<UnityEngine::Vector3>(nullptr)
		, System::Collections::Generic::IList_1<UnityEngine::Vector3>(nullptr)
		, System::Collections::Generic::IReadOnlyCollection_1<UnityEngine::Vector3>(nullptr)
		, System::Collections::Generic::IReadOnlyList_1<UnityEngine::Vector3>(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
		this->InternalLength = 0;
	}
	
	Array1<UnityEngine::Vector3>::Array1(const Array1<UnityEngine::Vector3>& other)
		: Array1(Plugin::InternalUse::Only, other.Handle)
	{
		InternalLength = other.InternalLength;
	}
	
	Array1<UnityEngine::Vector3>::Array1(Array1<UnityEngine::Vector3>&& other)
		: Array1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
		InternalLength = other.InternalLength;
		other.InternalLength = 0;
	}
	
	Array1<UnityEngine::Vector3>::~Array1<UnityEngine::Vector3>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	Array1<UnityEngine::Vector3>& Array1<UnityEngine::Vector3>::operator=(const Array1<UnityEngine::Vector3>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		InternalLength = other.InternalLength;
		return *this;
	}
	
	Array1<UnityEngine::Vector3>& Array1<UnityEngine::Vector3>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	Array1<UnityEngine::Vector3>& Array1<UnityEngine::Vector3>::operator=(Array1<UnityEngine::Vector3>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		InternalLength = other.InternalLength;
		other.Handle = 0;
		other.InternalLength = 0;
		return *this;
	}
	
	bool Array1<UnityEngine::Vector3>::operator==(const Array1<UnityEngine::Vector3>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool Array1<UnityEngine::Vector3>::operator!=(const Array1<UnityEngine::Vector3>& other) const
	{
		return Handle != other.Handle;
	}
	
	System::Array1<UnityEngine::Vector3>::Array1(System::Int32 length0)
		: System::ICloneable(nullptr)
		, System::Collections::IEnumerable(nullptr)
		, System::Collections::ICollection(nullptr)
		, System::Collections::IList(nullptr)
		, System::Collections::IStructuralComparable(nullptr)
		, System::Collections::IStructuralEquatable(nullptr)
		, System::Array(nullptr)
		, System::Collections::Generic::IEnumerable_1<UnityEngine::Vector3>(nullptr)
		, System::Collections::Generic::ICollection_1<UnityEngine::Vector3>(nullptr)
		, System::Collections::Generic::IList_1<UnityEngine::Vector3>(nullptr)
		, System::Collections::Generic::IReadOnlyCollection_1<UnityEngine::Vector3>(nullptr)
		, System::Collections::Generic::IReadOnlyList_1<UnityEngine::Vector3>(nullptr)
	{
		auto returnValue = Plugin::UnityEngineUnityEngineVector3Array1Constructor1(length0);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		Handle = returnValue;
		if (returnValue)
		{
			Plugin::ReferenceManagedClass(returnValue);
			InternalLength = length0;
		}
	}
	
	System::Int32 System::Array1<UnityEngine::Vector3>::GetLength()
	{
		int32_t returnVal = InternalLength;
		if (returnVal == 0)
		{
			returnVal = Array::GetLength();
			InternalLength = returnVal;
		};
		return returnVal;
	}
	
	System::Int32 System::Array1<UnityEngine::Vector3>::GetRank()
	{
		return 1;
	}
	
	Plugin::ArrayElementProxy1_1<UnityEngine::Vector3> System::Array1<UnityEngine::Vector3>::operator[](int32_t index)
	{
		return Plugin::ArrayElementProxy1_1<UnityEngine::Vector3>(Plugin::InternalUse::Only, Handle, index);
	}
}

namespace Plugin
{
	UnityEngineVector3Array1Iterator::UnityEngineVector3Array1Iterator(System::Array1<UnityEngine::Vector3>& array, int32_t index)
		: array(array)
		, index(index)
	{
	}
	
	UnityEngineVector3Array1Iterator& UnityEngineVector3Array1Iterator::operator++()
	{
		index++;
		return *this;
	}
	
	bool UnityEngineVector3Array1Iterator::operator!=(const UnityEngineVector3Array1Iterator& other)
	{
		return index != other.index;
	}
	
	UnityEngine::Vector3 UnityEngineVector3Array1Iterator::operator*()
	{
		return array[index];
	}
}

namespace System
{
	Plugin::UnityEngineVector3Array1Iterator begin(System::Array1<UnityEngine::Vector3>& array)
	{
		return Plugin::UnityEngineVector3Array1Iterator(array, 0);
	}
	
	Plugin::UnityEngineVector3Array1Iterator end(System::Array1<UnityEngine::Vector3>& array)
	{
		return Plugin::UnityEngineVector3Array1Iterator(array, array.GetLength() - 1);
	}
}

namespace Plugin
{
	ArrayElementProxy1_1<System::Int32>::ArrayElementProxy1_1(Plugin::InternalUse, int32_t handle, int32_t index0)
	{
		Handle = handle;
		Index0 = index0;
	}
	
	void ArrayElementProxy1_1<System::Int32>::operator=(System::Int32 item)
	{
		Plugin::SystemInt32Array1SetItem1(Handle, Index0, item);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	ArrayElementProxy1_1<System::Int32>::operator System::Int32()
	{
		auto returnValue = Plugin::SystemInt32Array1GetItem1(Handle, Index0);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		return returnValue;
	}
}

namespace System
{
	Array1<System::Int32>::Array1(decltype(nullptr))
		: System::ICloneable(nullptr)
		, System::Collections::IEnumerable(nullptr)
		, System::Collections::ICollection(nullptr)
		, System::Collections::IList(nullptr)
		, System::Collections::IStructuralComparable(nullptr)
		, System::Collections::IStructuralEquatable(nullptr)
		, System::Array(nullptr)
		, System::Collections::Generic::IEnumerable_1<System::Int32>(nullptr)
		, System::Collections::Generic::ICollection_1<System::Int32>(nullptr)
		, System::Collections::Generic::IList_1<System::Int32>(nullptr)
		, System::Collections::Generic::IReadOnlyCollection_1<System::Int32>(nullptr)
		, System::Collections::Generic::IReadOnlyList_1<System::Int32>(nullptr)
	{
		this->InternalLength = 0;
	}
	
	Array1<System::Int32>::Array1(Plugin::InternalUse, int32_t handle)
		: System::ICloneable(nullptr)
		, System::Collections::IEnumerable(nullptr)
		, System::Collections::ICollection(nullptr)
		, System::Collections::IList(nullptr)
		, System::Collections::IStructuralComparable(nullptr)
		, System::Collections::IStructuralEquatable(nullptr)
		, System::Array(nullptr)
		, System::Collections::Generic::IEnumerable_1<System::Int32>(nullptr)
		, System::Collections::Generic::ICollection_1<System::Int32>(nullptr)
		, System::Collections::Generic::IList_1<System::Int32>(nullptr)
		, System::Collections::Generic::IReadOnlyCollection_1<System::Int32>(nullptr)
		, System::Collections::Generic::IReadOnlyList_1<System::Int32>(nullptr)
	{
		Handle = handle;
		if (handle)
		{
			Plugin::ReferenceManagedClass(handle);
		}
		this->InternalLength = 0;
	}
	
	Array1<System::Int32>::Array1(const Array1<System::Int32>& other)
		: Array1(Plugin::InternalUse::Only, other.Handle)
	{
		InternalLength = other.InternalLength;
	}
	
	Array1<System::Int32>::Array1(Array1<System::Int32>&& other)
		: Array1(Plugin::InternalUse::Only, other.Handle)
	{
		other.Handle = 0;
		InternalLength = other.InternalLength;
		other.InternalLength = 0;
	}
	
	Array1<System::Int32>::~Array1<System::Int32>()
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
	}
	
	Array1<System::Int32>& Array1<System::Int32>::operator=(const Array1<System::Int32>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		InternalLength = other.InternalLength;
		return *this;
	}
	
	Array1<System::Int32>& Array1<System::Int32>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
			Handle = 0;
		}
		return *this;
	}
	
	Array1<System::Int32>& Array1<System::Int32>::operator=(Array1<System::Int32>&& other)
	{
		if (Handle)
		{
			Plugin::DereferenceManagedClass(Handle);
		}
		Handle = other.Handle;
		InternalLength = other.InternalLength;
		other.Handle = 0;
		other.InternalLength = 0;
		return *this;
	}
	
	bool Array1<System::Int32>::operator==(const Array1<System::Int32>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool Array1<System::Int32>::operator!=(const Array1<System::Int32>& other) const
	{
		return Handle != other.Handle;
	}
	
	System::Array1<System::Int32>::Array1(System::Int32 length0)
		: System::ICloneable(nullptr)
		, System::Collections::IEnumerable(nullptr)
		, System::Collections::ICollection(nullptr)
		, System::Collections::IList(nullptr)
		, System::Collections::IStructuralComparable(nullptr)
		, System::Collections::IStructuralEquatable(nullptr)
		, System::Array(nullptr)
		, System::Collections::Generic::IEnumerable_1<System::Int32>(nullptr)
		, System::Collections::Generic::ICollection_1<System::Int32>(nullptr)
		, System::Collections::Generic::IList_1<System::Int32>(nullptr)
		, System::Collections::Generic::IReadOnlyCollection_1<System::Int32>(nullptr)
		, System::Collections::Generic::IReadOnlyList_1<System::Int32>(nullptr)
	{
		auto returnValue = Plugin::SystemSystemInt32Array1Constructor1(length0);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		Handle = returnValue;
		if (returnValue)
		{
			Plugin::ReferenceManagedClass(returnValue);
			InternalLength = length0;
		}
	}
	
	System::Int32 System::Array1<System::Int32>::GetLength()
	{
		int32_t returnVal = InternalLength;
		if (returnVal == 0)
		{
			returnVal = Array::GetLength();
			InternalLength = returnVal;
		};
		return returnVal;
	}
	
	System::Int32 System::Array1<System::Int32>::GetRank()
	{
		return 1;
	}
	
	Plugin::ArrayElementProxy1_1<System::Int32> System::Array1<System::Int32>::operator[](int32_t index)
	{
		return Plugin::ArrayElementProxy1_1<System::Int32>(Plugin::InternalUse::Only, Handle, index);
	}
}

namespace Plugin
{
	SystemInt32Array1Iterator::SystemInt32Array1Iterator(System::Array1<System::Int32>& array, int32_t index)
		: array(array)
		, index(index)
	{
	}
	
	SystemInt32Array1Iterator& SystemInt32Array1Iterator::operator++()
	{
		index++;
		return *this;
	}
	
	bool SystemInt32Array1Iterator::operator!=(const SystemInt32Array1Iterator& other)
	{
		return index != other.index;
	}
	
	System::Int32 SystemInt32Array1Iterator::operator*()
	{
		return array[index];
	}
}

namespace System
{
	Plugin::SystemInt32Array1Iterator begin(System::Array1<System::Int32>& array)
	{
		return Plugin::SystemInt32Array1Iterator(array, 0);
	}
	
	Plugin::SystemInt32Array1Iterator end(System::Array1<System::Int32>& array)
	{
		return Plugin::SystemInt32Array1Iterator(array, array.GetLength() - 1);
	}
}

namespace System
{
	System::Action::Action()
	{
		CppHandle = Plugin::StoreSystemAction(this);
		System::Int32* handle = (System::Int32*)&Handle;
		int32_t cppHandle = CppHandle;
		System::Int32* classHandle = (System::Int32*)&ClassHandle;
		Plugin::SystemActionConstructor(cppHandle, &handle->Value, &classHandle->Value);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (Handle)
		{
			Plugin::ReferenceManagedClass(Handle);
		}
		else
		{
			Plugin::RemoveSystemAction(CppHandle);
			ClassHandle = 0;
			CppHandle = 0;
		}
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	Action::Action(decltype(nullptr))
	{
		CppHandle = Plugin::StoreSystemAction(this);
		ClassHandle = 0;
	}
	
	System::Action::Action(const System::Action& other)
	{
		Handle = other.Handle;
		CppHandle = Plugin::StoreSystemAction(this);
		if (Handle)
		{
			Plugin::ReferenceManagedClass(Handle);
		}
		ClassHandle = other.ClassHandle;
	}
	
	System::Action::Action(System::Action&& other)
	{
		Handle = other.Handle;
		CppHandle = other.CppHandle;
		ClassHandle = other.ClassHandle;
		other.Handle = 0;
		other.CppHandle = 0;
		other.ClassHandle = 0;
	}
	
	System::Action::Action(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		CppHandle = Plugin::StoreSystemAction(this);
		if (Handle)
		{
			Plugin::ReferenceManagedClass(Handle);
		}
		ClassHandle = 0;
	}
	
	System::Action::~Action()
	{
		Plugin::RemoveSystemAction(CppHandle);
		CppHandle = 0;
		if (Handle)
		{
			int32_t handle = Handle;
			int32_t classHandle = ClassHandle;
			Handle = 0;
			ClassHandle = 0;
			if (Plugin::DereferenceManagedClassNoRelease(handle))
			{
				Plugin::ReleaseSystemAction(handle, classHandle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
			}
		}
	}
	
	System::Action& System::Action::operator=(const System::Action& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		ClassHandle = other.ClassHandle;
		return *this;
	}
	
	System::Action& System::Action::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			int32_t handle = Handle;
			int32_t classHandle = ClassHandle;
			Handle = 0;
			ClassHandle = 0;
			if (Plugin::DereferenceManagedClassNoRelease(handle))
			{
				Plugin::ReleaseSystemAction(handle, classHandle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
			}
		}
		ClassHandle = 0;
		Handle = 0;
		return *this;
	}
	
	System::Action& System::Action::operator=(System::Action&& other)
	{
		Plugin::RemoveSystemAction(CppHandle);
		CppHandle = 0;
		if (Handle)
		{
			int32_t handle = Handle;
			int32_t classHandle = ClassHandle;
			Handle = 0;
			ClassHandle = 0;
			if (Plugin::DereferenceManagedClassNoRelease(handle))
			{
				Plugin::ReleaseSystemAction(handle, classHandle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
			}
		}
		ClassHandle = other.ClassHandle;
		other.ClassHandle = 0;
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool System::Action::operator==(const System::Action& other) const
	{
		return Handle == other.Handle;
	}
	
	bool System::Action::operator!=(const System::Action& other) const
	{
		return Handle != other.Handle;
	}
	
	void System::Action::operator+=(System::Action& del)
	{
		Plugin::SystemActionAdd(Handle, del.Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	void System::Action::operator-=(System::Action& del)
	{
		Plugin::SystemActionRemove(Handle, del.Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	void System::Action::operator()()
	{
	}
	
	DLLEXPORT void SystemActionNativeInvoke(int32_t cppHandle)
	{
		try
		{
			Plugin::GetSystemAction(cppHandle)->operator()();
		}
		catch (System::Exception ex)
		{
			Plugin::SetException(ex.Handle);
		}
		catch (...)
		{
			System::String msg = "Unhandled exception invoking System::Action";
			System::Exception ex(msg);
			Plugin::SetException(ex.Handle);
		}
	}
	
	void System::Action::Invoke()
	{
		Plugin::SystemActionInvoke(Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
}

namespace System
{
	System::Action_1<UnityEngine::AsyncOperation>::Action_1()
	{
		CppHandle = Plugin::StoreSystemActionUnityEngineAsyncOperation(this);
		System::Int32* handle = (System::Int32*)&Handle;
		int32_t cppHandle = CppHandle;
		System::Int32* classHandle = (System::Int32*)&ClassHandle;
		Plugin::SystemActionUnityEngineAsyncOperationConstructor(cppHandle, &handle->Value, &classHandle->Value);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
		if (Handle)
		{
			Plugin::ReferenceManagedClass(Handle);
		}
		else
		{
			Plugin::RemoveSystemActionUnityEngineAsyncOperation(CppHandle);
			ClassHandle = 0;
			CppHandle = 0;
		}
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	Action_1<UnityEngine::AsyncOperation>::Action_1(decltype(nullptr))
	{
		CppHandle = Plugin::StoreSystemActionUnityEngineAsyncOperation(this);
		ClassHandle = 0;
	}
	
	System::Action_1<UnityEngine::AsyncOperation>::Action_1(const System::Action_1<UnityEngine::AsyncOperation>& other)
	{
		Handle = other.Handle;
		CppHandle = Plugin::StoreSystemActionUnityEngineAsyncOperation(this);
		if (Handle)
		{
			Plugin::ReferenceManagedClass(Handle);
		}
		ClassHandle = other.ClassHandle;
	}
	
	System::Action_1<UnityEngine::AsyncOperation>::Action_1(System::Action_1<UnityEngine::AsyncOperation>&& other)
	{
		Handle = other.Handle;
		CppHandle = other.CppHandle;
		ClassHandle = other.ClassHandle;
		other.Handle = 0;
		other.CppHandle = 0;
		other.ClassHandle = 0;
	}
	
	System::Action_1<UnityEngine::AsyncOperation>::Action_1(Plugin::InternalUse, int32_t handle)
	{
		Handle = handle;
		CppHandle = Plugin::StoreSystemActionUnityEngineAsyncOperation(this);
		if (Handle)
		{
			Plugin::ReferenceManagedClass(Handle);
		}
		ClassHandle = 0;
	}
	
	System::Action_1<UnityEngine::AsyncOperation>::~Action_1<UnityEngine::AsyncOperation>()
	{
		Plugin::RemoveSystemActionUnityEngineAsyncOperation(CppHandle);
		CppHandle = 0;
		if (Handle)
		{
			int32_t handle = Handle;
			int32_t classHandle = ClassHandle;
			Handle = 0;
			ClassHandle = 0;
			if (Plugin::DereferenceManagedClassNoRelease(handle))
			{
				Plugin::ReleaseSystemActionUnityEngineAsyncOperation(handle, classHandle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
			}
		}
	}
	
	System::Action_1<UnityEngine::AsyncOperation>& System::Action_1<UnityEngine::AsyncOperation>::operator=(const System::Action_1<UnityEngine::AsyncOperation>& other)
	{
		if (this->Handle)
		{
			Plugin::DereferenceManagedClass(this->Handle);
		}
		this->Handle = other.Handle;
		if (this->Handle)
		{
			Plugin::ReferenceManagedClass(this->Handle);
		}
		ClassHandle = other.ClassHandle;
		return *this;
	}
	
	System::Action_1<UnityEngine::AsyncOperation>& System::Action_1<UnityEngine::AsyncOperation>::operator=(decltype(nullptr))
	{
		if (Handle)
		{
			int32_t handle = Handle;
			int32_t classHandle = ClassHandle;
			Handle = 0;
			ClassHandle = 0;
			if (Plugin::DereferenceManagedClassNoRelease(handle))
			{
				Plugin::ReleaseSystemActionUnityEngineAsyncOperation(handle, classHandle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
			}
		}
		ClassHandle = 0;
		Handle = 0;
		return *this;
	}
	
	System::Action_1<UnityEngine::AsyncOperation>& System::Action_1<UnityEngine::AsyncOperation>::operator=(System::Action_1<UnityEngine::AsyncOperation>&& other)
	{
		Plugin::RemoveSystemActionUnityEngineAsyncOperation(CppHandle);
		CppHandle = 0;
		if (Handle)
		{
			int32_t handle = Handle;
			int32_t classHandle = ClassHandle;
			Handle = 0;
			ClassHandle = 0;
			if (Plugin::DereferenceManagedClassNoRelease(handle))
			{
				Plugin::ReleaseSystemActionUnityEngineAsyncOperation(handle, classHandle);
				if (Plugin::unhandledCsharpException)
				{
					System::Exception* ex = Plugin::unhandledCsharpException;
					Plugin::unhandledCsharpException = nullptr;
					ex->ThrowReferenceToThis();
					delete ex;
				}
			}
		}
		ClassHandle = other.ClassHandle;
		other.ClassHandle = 0;
		Handle = other.Handle;
		other.Handle = 0;
		return *this;
	}
	
	bool System::Action_1<UnityEngine::AsyncOperation>::operator==(const System::Action_1<UnityEngine::AsyncOperation>& other) const
	{
		return Handle == other.Handle;
	}
	
	bool System::Action_1<UnityEngine::AsyncOperation>::operator!=(const System::Action_1<UnityEngine::AsyncOperation>& other) const
	{
		return Handle != other.Handle;
	}
	
	void System::Action_1<UnityEngine::AsyncOperation>::operator+=(System::Action_1<UnityEngine::AsyncOperation>& del)
	{
		Plugin::SystemActionUnityEngineAsyncOperationAdd(Handle, del.Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	void System::Action_1<UnityEngine::AsyncOperation>::operator-=(System::Action_1<UnityEngine::AsyncOperation>& del)
	{
		Plugin::SystemActionUnityEngineAsyncOperationRemove(Handle, del.Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
	
	void System::Action_1<UnityEngine::AsyncOperation>::operator()(UnityEngine::AsyncOperation& obj)
	{
	}
	
	DLLEXPORT void SystemActionUnityEngineAsyncOperationNativeInvoke(int32_t cppHandle, int32_t objHandle)
	{
		try
		{
			auto obj = UnityEngine::AsyncOperation(Plugin::InternalUse::Only, objHandle);
			Plugin::GetSystemActionUnityEngineAsyncOperation(cppHandle)->operator()(obj);
		}
		catch (System::Exception ex)
		{
			Plugin::SetException(ex.Handle);
		}
		catch (...)
		{
			System::String msg = "Unhandled exception invoking System::Action_1<UnityEngine::AsyncOperation>";
			System::Exception ex(msg);
			Plugin::SetException(ex.Handle);
		}
	}
	
	void System::Action_1<UnityEngine::AsyncOperation>::Invoke(UnityEngine::AsyncOperation& obj)
	{
		Plugin::SystemActionUnityEngineAsyncOperationInvoke(Handle, obj.Handle);
		if (Plugin::unhandledCsharpException)
		{
			System::Exception* ex = Plugin::unhandledCsharpException;
			Plugin::unhandledCsharpException = nullptr;
			ex->ThrowReferenceToThis();
			delete ex;
		}
	}
}

namespace System
{
	struct NullReferenceExceptionThrower : System::NullReferenceException
	{
		NullReferenceExceptionThrower(int32_t handle)
			: System::Runtime::InteropServices::_Exception(nullptr)
			, System::Runtime::Serialization::ISerializable(nullptr)
			, System::Exception(nullptr)
			, System::SystemException(nullptr)
			, System::NullReferenceException(Plugin::InternalUse::Only, handle)
		{
		}
	
		virtual void ThrowReferenceToThis()
		{
			throw *this;
		}
	};
}

DLLEXPORT void SetCsharpExceptionSystemNullReferenceException(int32_t handle)
{
	delete Plugin::unhandledCsharpException;
	Plugin::unhandledCsharpException = new System::NullReferenceExceptionThrower(handle);
}
/*END METHOD DEFINITIONS*/

////////////////////////////////////////////////////////////////
// App-specific functions for this file to call
////////////////////////////////////////////////////////////////

// Called when the plugin is initialized
extern void PluginMain(
	void* memory,
	int32_t memorySize,
	bool isFirstBoot);

////////////////////////////////////////////////////////////////
// C++ functions for C# to call
////////////////////////////////////////////////////////////////

enum class InitMode : uint8_t
{
	FirstBoot,
	Reload
};

// Init the plugin
DLLEXPORT void Init(
	uint8_t* memory,
	int32_t memorySize,
	InitMode initMode)
{
	uint8_t* curMemory = memory;

	// Read fixed parameters
	Plugin::ReleaseObject = *(void (**)(int32_t handle))curMemory;
	curMemory += sizeof(Plugin::ReleaseObject);
	Plugin::StringNew = *(int32_t (**)(const char*))curMemory;
	curMemory += sizeof(Plugin::StringNew);
	Plugin::SetException = *(void (**)(int32_t))curMemory;
	curMemory += sizeof(Plugin::SetException);
	Plugin::ArrayGetLength = *(int32_t (**)(int32_t))curMemory;
	curMemory += sizeof(Plugin::ArrayGetLength);
	Plugin::EnumerableGetEnumerator = *(int32_t (**)(int32_t))curMemory;
	curMemory += sizeof(Plugin::EnumerableGetEnumerator);

	// Read generated parameters
	int32_t maxManagedObjects = *(int32_t*)curMemory;
	curMemory += sizeof(int32_t);
	/*BEGIN INIT BODY PARAMETER READS*/
	Plugin::SystemIDisposableMethodDispose = *(void (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::SystemIDisposableMethodDispose);
	Plugin::ReleaseSystemDecimal = *(void (**)(int32_t handle))curMemory;
	curMemory += sizeof(Plugin::ReleaseSystemDecimal);
	Plugin::SystemDecimalConstructorSystemDouble = *(int32_t (**)(double value))curMemory;
	curMemory += sizeof(Plugin::SystemDecimalConstructorSystemDouble);
	Plugin::SystemDecimalConstructorSystemUInt64 = *(int32_t (**)(uint64_t value))curMemory;
	curMemory += sizeof(Plugin::SystemDecimalConstructorSystemUInt64);
	Plugin::BoxDecimal = *(int32_t (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::BoxDecimal);
	Plugin::UnboxDecimal = *(int32_t (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxDecimal);
	Plugin::UnityEngineVector3ConstructorSystemSingle_SystemSingle_SystemSingle = *(UnityEngine::Vector3 (**)(float x, float y, float z))curMemory;
	curMemory += sizeof(Plugin::UnityEngineVector3ConstructorSystemSingle_SystemSingle_SystemSingle);
	Plugin::BoxVector3 = *(int32_t (**)(UnityEngine::Vector3& val))curMemory;
	curMemory += sizeof(Plugin::BoxVector3);
	Plugin::UnboxVector3 = *(UnityEngine::Vector3 (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxVector3);
	Plugin::UnityEngineVector4ConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingle = *(UnityEngine::Vector4 (**)(float x, float y, float z, float w))curMemory;
	curMemory += sizeof(Plugin::UnityEngineVector4ConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingle);
	Plugin::BoxVector4 = *(int32_t (**)(UnityEngine::Vector4& val))curMemory;
	curMemory += sizeof(Plugin::BoxVector4);
	Plugin::UnboxVector4 = *(UnityEngine::Vector4 (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxVector4);
	Plugin::UnityEngineQuaternionConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingle = *(UnityEngine::Quaternion (**)(float x, float y, float z, float w))curMemory;
	curMemory += sizeof(Plugin::UnityEngineQuaternionConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingle);
	Plugin::BoxQuaternion = *(int32_t (**)(UnityEngine::Quaternion& val))curMemory;
	curMemory += sizeof(Plugin::BoxQuaternion);
	Plugin::UnboxQuaternion = *(UnityEngine::Quaternion (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxQuaternion);
	Plugin::UnityEngineMatrix4x4ConstructorUnityEngineVector4_UnityEngineVector4_UnityEngineVector4_UnityEngineVector4 = *(UnityEngine::Matrix4x4 (**)(UnityEngine::Vector4& column0, UnityEngine::Vector4& column1, UnityEngine::Vector4& column2, UnityEngine::Vector4& column3))curMemory;
	curMemory += sizeof(Plugin::UnityEngineMatrix4x4ConstructorUnityEngineVector4_UnityEngineVector4_UnityEngineVector4_UnityEngineVector4);
	Plugin::UnityEngineMatrix4x4PropertyGetRotation = *(UnityEngine::Quaternion (**)(UnityEngine::Matrix4x4* thiz))curMemory;
	curMemory += sizeof(Plugin::UnityEngineMatrix4x4PropertyGetRotation);
	Plugin::UnityEngineMatrix4x4PropertyGetLossyScale = *(UnityEngine::Vector3 (**)(UnityEngine::Matrix4x4* thiz))curMemory;
	curMemory += sizeof(Plugin::UnityEngineMatrix4x4PropertyGetLossyScale);
	Plugin::UnityEngineMatrix4x4MethodGetPosition = *(UnityEngine::Vector3 (**)(UnityEngine::Matrix4x4* thiz))curMemory;
	curMemory += sizeof(Plugin::UnityEngineMatrix4x4MethodGetPosition);
	Plugin::BoxMatrix4x4 = *(int32_t (**)(UnityEngine::Matrix4x4& val))curMemory;
	curMemory += sizeof(Plugin::BoxMatrix4x4);
	Plugin::UnboxMatrix4x4 = *(UnityEngine::Matrix4x4 (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxMatrix4x4);
	Plugin::UnityEngineObjectPropertyGetName = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineObjectPropertyGetName);
	Plugin::UnityEngineObjectPropertySetName = *(void (**)(int32_t thisHandle, int32_t valueHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineObjectPropertySetName);
	Plugin::UnityEngineComponentPropertyGetTransform = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineComponentPropertyGetTransform);
	Plugin::UnityEngineTransformPropertyGetPosition = *(UnityEngine::Vector3 (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineTransformPropertyGetPosition);
	Plugin::UnityEngineTransformPropertySetPosition = *(void (**)(int32_t thisHandle, UnityEngine::Vector3& value))curMemory;
	curMemory += sizeof(Plugin::UnityEngineTransformPropertySetPosition);
	Plugin::UnityEngineTransformPropertyGetRotation = *(UnityEngine::Quaternion (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineTransformPropertyGetRotation);
	Plugin::UnityEngineTransformPropertySetRotation = *(void (**)(int32_t thisHandle, UnityEngine::Quaternion& value))curMemory;
	curMemory += sizeof(Plugin::UnityEngineTransformPropertySetRotation);
	Plugin::UnityEngineTransformPropertyGetLocalScale = *(UnityEngine::Vector3 (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineTransformPropertyGetLocalScale);
	Plugin::UnityEngineTransformPropertySetLocalScale = *(void (**)(int32_t thisHandle, UnityEngine::Vector3& value))curMemory;
	curMemory += sizeof(Plugin::UnityEngineTransformPropertySetLocalScale);
	Plugin::UnityEngineTransformPropertyGetLocalToWorldMatrix = *(UnityEngine::Matrix4x4 (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineTransformPropertyGetLocalToWorldMatrix);
	Plugin::UnityEngineTransformPropertyGetParent = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineTransformPropertyGetParent);
	Plugin::UnityEngineTransformPropertySetParent = *(void (**)(int32_t thisHandle, int32_t valueHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineTransformPropertySetParent);
	Plugin::UnityEngineTransformPropertyGetForward = *(UnityEngine::Vector3 (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineTransformPropertyGetForward);
	Plugin::UnityEngineTransformPropertySetForward = *(void (**)(int32_t thisHandle, UnityEngine::Vector3& value))curMemory;
	curMemory += sizeof(Plugin::UnityEngineTransformPropertySetForward);
	Plugin::UnityEngineTransformPropertyGetUp = *(UnityEngine::Vector3 (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineTransformPropertyGetUp);
	Plugin::UnityEngineTransformPropertySetUp = *(void (**)(int32_t thisHandle, UnityEngine::Vector3& value))curMemory;
	curMemory += sizeof(Plugin::UnityEngineTransformPropertySetUp);
	Plugin::SystemCollectionsIEnumeratorPropertyGetCurrent = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::SystemCollectionsIEnumeratorPropertyGetCurrent);
	Plugin::SystemCollectionsIEnumeratorMethodMoveNext = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::SystemCollectionsIEnumeratorMethodMoveNext);
	Plugin::UnityEngineGameObjectConstructorSystemString = *(int32_t (**)(int32_t nameHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineGameObjectConstructorSystemString);
	Plugin::UnityEngineGameObjectPropertyGetTransform = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineGameObjectPropertyGetTransform);
	Plugin::UnityEngineGameObjectMethodAddComponentCesiumForUnityBaseCesium3DTileset = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineGameObjectMethodAddComponentCesiumForUnityBaseCesium3DTileset);
	Plugin::UnityEngineGameObjectMethodAddComponentUnityEngineMeshFilter = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineGameObjectMethodAddComponentUnityEngineMeshFilter);
	Plugin::UnityEngineGameObjectMethodAddComponentUnityEngineMeshRenderer = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineGameObjectMethodAddComponentUnityEngineMeshRenderer);
	Plugin::UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveType = *(int32_t (**)(UnityEngine::PrimitiveType type))curMemory;
	curMemory += sizeof(Plugin::UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveType);
	Plugin::UnityEngineGameObjectMethodSetActiveSystemBoolean = *(void (**)(int32_t thisHandle, uint32_t value))curMemory;
	curMemory += sizeof(Plugin::UnityEngineGameObjectMethodSetActiveSystemBoolean);
	Plugin::UnityEngineDebugMethodLogSystemObject = *(void (**)(int32_t messageHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineDebugMethodLogSystemObject);
	Plugin::UnityEngineMonoBehaviourPropertyGetTransform = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineMonoBehaviourPropertyGetTransform);
	Plugin::UnityEngineMonoBehaviourPropertyGetGameObject = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineMonoBehaviourPropertyGetGameObject);
	Plugin::SystemCollectionsGenericIEnumeratorUnityEngineVector3PropertyGetCurrent = *(UnityEngine::Vector3 (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::SystemCollectionsGenericIEnumeratorUnityEngineVector3PropertyGetCurrent);
	Plugin::SystemCollectionsGenericIEnumeratorSystemInt32PropertyGetCurrent = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::SystemCollectionsGenericIEnumeratorSystemInt32PropertyGetCurrent);
	Plugin::SystemCollectionsGenericIEnumerableUnityEngineVector3MethodGetEnumerator = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::SystemCollectionsGenericIEnumerableUnityEngineVector3MethodGetEnumerator);
	Plugin::SystemCollectionsGenericIEnumerableSystemInt32MethodGetEnumerator = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::SystemCollectionsGenericIEnumerableSystemInt32MethodGetEnumerator);
	Plugin::UnityEngineMeshConstructor = *(int32_t (**)())curMemory;
	curMemory += sizeof(Plugin::UnityEngineMeshConstructor);
	Plugin::UnityEngineMeshMethodSetVerticesUnityEngineVector3Array1 = *(void (**)(int32_t thisHandle, int32_t inVerticesHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineMeshMethodSetVerticesUnityEngineVector3Array1);
	Plugin::UnityEngineMeshMethodSetNormalsUnityEngineVector3Array1 = *(void (**)(int32_t thisHandle, int32_t inNormalsHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineMeshMethodSetNormalsUnityEngineVector3Array1);
	Plugin::UnityEngineMeshMethodSetTrianglesSystemInt32Array1_SystemInt32_SystemBoolean_SystemInt32 = *(void (**)(int32_t thisHandle, int32_t trianglesHandle, int32_t submesh, uint32_t calculateBounds, int32_t baseVertex))curMemory;
	curMemory += sizeof(Plugin::UnityEngineMeshMethodSetTrianglesSystemInt32Array1_SystemInt32_SystemBoolean_SystemInt32);
	Plugin::UnityEngineMeshFilterPropertyGetMesh = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineMeshFilterPropertyGetMesh);
	Plugin::UnityEngineMeshFilterPropertySetMesh = *(void (**)(int32_t thisHandle, int32_t valueHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineMeshFilterPropertySetMesh);
	Plugin::UnityEngineMeshRendererPropertyGetMaterial = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineMeshRendererPropertyGetMaterial);
	Plugin::UnityEngineMeshRendererPropertySetMaterial = *(void (**)(int32_t thisHandle, int32_t valueHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineMeshRendererPropertySetMaterial);
	Plugin::SystemExceptionConstructorSystemString = *(int32_t (**)(int32_t messageHandle))curMemory;
	curMemory += sizeof(Plugin::SystemExceptionConstructorSystemString);
	Plugin::BoxPrimitiveType = *(int32_t (**)(UnityEngine::PrimitiveType val))curMemory;
	curMemory += sizeof(Plugin::BoxPrimitiveType);
	Plugin::UnboxPrimitiveType = *(UnityEngine::PrimitiveType (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxPrimitiveType);
	Plugin::UnityEngineTimePropertyGetDeltaTime = *(float (**)())curMemory;
	curMemory += sizeof(Plugin::UnityEngineTimePropertyGetDeltaTime);
	Plugin::UnityEngineCameraPropertyGetMain = *(int32_t (**)())curMemory;
	curMemory += sizeof(Plugin::UnityEngineCameraPropertyGetMain);
	Plugin::UnityEngineCameraPropertyGetFieldOfView = *(float (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineCameraPropertyGetFieldOfView);
	Plugin::UnityEngineCameraPropertySetFieldOfView = *(void (**)(int32_t thisHandle, float value))curMemory;
	curMemory += sizeof(Plugin::UnityEngineCameraPropertySetFieldOfView);
	Plugin::UnityEngineCameraPropertyGetAspect = *(float (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineCameraPropertyGetAspect);
	Plugin::UnityEngineCameraPropertySetAspect = *(void (**)(int32_t thisHandle, float value))curMemory;
	curMemory += sizeof(Plugin::UnityEngineCameraPropertySetAspect);
	Plugin::UnityEngineCameraPropertyGetPixelWidth = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineCameraPropertyGetPixelWidth);
	Plugin::UnityEngineCameraPropertyGetPixelHeight = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineCameraPropertyGetPixelHeight);
	Plugin::BoxRawDownloadedData = *(int32_t (**)(CesiumForUnity::RawDownloadedData& val))curMemory;
	curMemory += sizeof(Plugin::BoxRawDownloadedData);
	Plugin::UnboxRawDownloadedData = *(CesiumForUnity::RawDownloadedData (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxRawDownloadedData);
	Plugin::ReleaseBaseNativeDownloadHandler = *(void (**)(int32_t handle))curMemory;
	curMemory += sizeof(Plugin::ReleaseBaseNativeDownloadHandler);
	Plugin::BaseNativeDownloadHandlerConstructor = *(void (**)(int32_t cppHandle, int32_t* handle))curMemory;
	curMemory += sizeof(Plugin::BaseNativeDownloadHandlerConstructor);
	Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetError = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetError);
	Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetIsDone = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetIsDone);
	Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetResponseCode = *(int64_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetResponseCode);
	Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetUrl = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetUrl);
	Plugin::UnityEngineNetworkingUnityWebRequestPropertySetUrl = *(void (**)(int32_t thisHandle, int32_t valueHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineNetworkingUnityWebRequestPropertySetUrl);
	Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetMethod = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetMethod);
	Plugin::UnityEngineNetworkingUnityWebRequestPropertySetMethod = *(void (**)(int32_t thisHandle, int32_t valueHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineNetworkingUnityWebRequestPropertySetMethod);
	Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetDownloadHandler = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineNetworkingUnityWebRequestPropertyGetDownloadHandler);
	Plugin::UnityEngineNetworkingUnityWebRequestPropertySetDownloadHandler = *(void (**)(int32_t thisHandle, int32_t valueHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineNetworkingUnityWebRequestPropertySetDownloadHandler);
	Plugin::UnityEngineNetworkingUnityWebRequestMethodGetSystemString = *(int32_t (**)(int32_t uriHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineNetworkingUnityWebRequestMethodGetSystemString);
	Plugin::UnityEngineNetworkingUnityWebRequestMethodSetRequestHeaderSystemString_SystemString = *(void (**)(int32_t thisHandle, int32_t nameHandle, int32_t valueHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineNetworkingUnityWebRequestMethodSetRequestHeaderSystemString_SystemString);
	Plugin::UnityEngineNetworkingUnityWebRequestMethodSendWebRequest = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineNetworkingUnityWebRequestMethodSendWebRequest);
	Plugin::UnityEngineNetworkingUnityWebRequestMethodGetResponseHeaderSystemString = *(int32_t (**)(int32_t thisHandle, int32_t nameHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineNetworkingUnityWebRequestMethodGetResponseHeaderSystemString);
	Plugin::UnityEngineNetworkingUnityWebRequestAsyncOperationAddEventCompleted = *(void (**)(int32_t thisHandle, int32_t delHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineNetworkingUnityWebRequestAsyncOperationAddEventCompleted);
	Plugin::UnityEngineNetworkingUnityWebRequestAsyncOperationRemoveEventCompleted = *(void (**)(int32_t thisHandle, int32_t delHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineNetworkingUnityWebRequestAsyncOperationRemoveEventCompleted);
	Plugin::SystemRuntimeInteropServicesMarshalMethodStringToCoTaskMemUTF8SystemString = *(void* (**)(int32_t sHandle))curMemory;
	curMemory += sizeof(Plugin::SystemRuntimeInteropServicesMarshalMethodStringToCoTaskMemUTF8SystemString);
	Plugin::SystemRuntimeInteropServicesMarshalMethodFreeCoTaskMemSystemIntPtr = *(void (**)(void* ptr))curMemory;
	curMemory += sizeof(Plugin::SystemRuntimeInteropServicesMarshalMethodFreeCoTaskMemSystemIntPtr);
	Plugin::ReleaseBaseCesium3DTileset = *(void (**)(int32_t handle))curMemory;
	curMemory += sizeof(Plugin::ReleaseBaseCesium3DTileset);
	Plugin::BaseCesium3DTilesetConstructor = *(void (**)(int32_t cppHandle, int32_t* handle))curMemory;
	curMemory += sizeof(Plugin::BaseCesium3DTilesetConstructor);
	Plugin::SystemThreadingTasksTaskMethodRunSystemAction = *(int32_t (**)(int32_t actionHandle))curMemory;
	curMemory += sizeof(Plugin::SystemThreadingTasksTaskMethodRunSystemAction);
	Plugin::UnityEngineResourcesMethodLoadUnityEngineMaterialSystemString = *(int32_t (**)(int32_t pathHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEngineResourcesMethodLoadUnityEngineMaterialSystemString);
	Plugin::UnityEditorSceneViewPropertyGetLastActiveSceneView = *(int32_t (**)())curMemory;
	curMemory += sizeof(Plugin::UnityEditorSceneViewPropertyGetLastActiveSceneView);
	Plugin::UnityEditorSceneViewPropertyGetCamera = *(int32_t (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::UnityEditorSceneViewPropertyGetCamera);
	Plugin::BoxBoolean = *(int32_t (**)(uint32_t val))curMemory;
	curMemory += sizeof(Plugin::BoxBoolean);
	Plugin::UnboxBoolean = *(int32_t (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxBoolean);
	Plugin::BoxSByte = *(int32_t (**)(int8_t val))curMemory;
	curMemory += sizeof(Plugin::BoxSByte);
	Plugin::UnboxSByte = *(int8_t (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxSByte);
	Plugin::BoxByte = *(int32_t (**)(uint8_t val))curMemory;
	curMemory += sizeof(Plugin::BoxByte);
	Plugin::UnboxByte = *(uint8_t (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxByte);
	Plugin::BoxInt16 = *(int32_t (**)(int16_t val))curMemory;
	curMemory += sizeof(Plugin::BoxInt16);
	Plugin::UnboxInt16 = *(int16_t (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxInt16);
	Plugin::BoxUInt16 = *(int32_t (**)(uint16_t val))curMemory;
	curMemory += sizeof(Plugin::BoxUInt16);
	Plugin::UnboxUInt16 = *(uint16_t (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxUInt16);
	Plugin::BoxInt32 = *(int32_t (**)(int32_t val))curMemory;
	curMemory += sizeof(Plugin::BoxInt32);
	Plugin::UnboxInt32 = *(int32_t (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxInt32);
	Plugin::BoxUInt32 = *(int32_t (**)(uint32_t val))curMemory;
	curMemory += sizeof(Plugin::BoxUInt32);
	Plugin::UnboxUInt32 = *(uint32_t (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxUInt32);
	Plugin::BoxInt64 = *(int32_t (**)(int64_t val))curMemory;
	curMemory += sizeof(Plugin::BoxInt64);
	Plugin::UnboxInt64 = *(int64_t (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxInt64);
	Plugin::BoxUInt64 = *(int32_t (**)(uint64_t val))curMemory;
	curMemory += sizeof(Plugin::BoxUInt64);
	Plugin::UnboxUInt64 = *(uint64_t (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxUInt64);
	Plugin::BoxChar = *(int32_t (**)(uint16_t val))curMemory;
	curMemory += sizeof(Plugin::BoxChar);
	Plugin::UnboxChar = *(int16_t (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxChar);
	Plugin::BoxSingle = *(int32_t (**)(float val))curMemory;
	curMemory += sizeof(Plugin::BoxSingle);
	Plugin::UnboxSingle = *(float (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxSingle);
	Plugin::BoxDouble = *(int32_t (**)(double val))curMemory;
	curMemory += sizeof(Plugin::BoxDouble);
	Plugin::UnboxDouble = *(double (**)(int32_t valHandle))curMemory;
	curMemory += sizeof(Plugin::UnboxDouble);
	Plugin::UnityEngineUnityEngineVector3Array1Constructor1 = *(int32_t (**)(int32_t length0))curMemory;
	curMemory += sizeof(Plugin::UnityEngineUnityEngineVector3Array1Constructor1);
	Plugin::UnityEngineVector3Array1GetItem1 = *(UnityEngine::Vector3 (**)(int32_t thisHandle, int32_t index0))curMemory;
	curMemory += sizeof(Plugin::UnityEngineVector3Array1GetItem1);
	Plugin::UnityEngineVector3Array1SetItem1 = *(int32_t (**)(int32_t thisHandle, int32_t index0, UnityEngine::Vector3& item))curMemory;
	curMemory += sizeof(Plugin::UnityEngineVector3Array1SetItem1);
	Plugin::SystemSystemInt32Array1Constructor1 = *(int32_t (**)(int32_t length0))curMemory;
	curMemory += sizeof(Plugin::SystemSystemInt32Array1Constructor1);
	Plugin::SystemInt32Array1GetItem1 = *(int32_t (**)(int32_t thisHandle, int32_t index0))curMemory;
	curMemory += sizeof(Plugin::SystemInt32Array1GetItem1);
	Plugin::SystemInt32Array1SetItem1 = *(int32_t (**)(int32_t thisHandle, int32_t index0, int32_t item))curMemory;
	curMemory += sizeof(Plugin::SystemInt32Array1SetItem1);
	Plugin::ReleaseSystemAction = *(void (**)(int32_t handle, int32_t classHandle))curMemory;
	curMemory += sizeof(Plugin::ReleaseSystemAction);
	Plugin::SystemActionConstructor = *(void (**)(int32_t cppHandle, int32_t* handle, int32_t* classHandle))curMemory;
	curMemory += sizeof(Plugin::SystemActionConstructor);
	Plugin::SystemActionAdd = *(void (**)(int32_t thisHandle, int32_t delHandle))curMemory;
	curMemory += sizeof(Plugin::SystemActionAdd);
	Plugin::SystemActionRemove = *(void (**)(int32_t thisHandle, int32_t delHandle))curMemory;
	curMemory += sizeof(Plugin::SystemActionRemove);
	Plugin::SystemActionInvoke = *(void (**)(int32_t thisHandle))curMemory;
	curMemory += sizeof(Plugin::SystemActionInvoke);
	Plugin::ReleaseSystemActionUnityEngineAsyncOperation = *(void (**)(int32_t handle, int32_t classHandle))curMemory;
	curMemory += sizeof(Plugin::ReleaseSystemActionUnityEngineAsyncOperation);
	Plugin::SystemActionUnityEngineAsyncOperationConstructor = *(void (**)(int32_t cppHandle, int32_t* handle, int32_t* classHandle))curMemory;
	curMemory += sizeof(Plugin::SystemActionUnityEngineAsyncOperationConstructor);
	Plugin::SystemActionUnityEngineAsyncOperationAdd = *(void (**)(int32_t thisHandle, int32_t delHandle))curMemory;
	curMemory += sizeof(Plugin::SystemActionUnityEngineAsyncOperationAdd);
	Plugin::SystemActionUnityEngineAsyncOperationRemove = *(void (**)(int32_t thisHandle, int32_t delHandle))curMemory;
	curMemory += sizeof(Plugin::SystemActionUnityEngineAsyncOperationRemove);
	Plugin::SystemActionUnityEngineAsyncOperationInvoke = *(void (**)(int32_t thisHandle, int32_t objHandle))curMemory;
	curMemory += sizeof(Plugin::SystemActionUnityEngineAsyncOperationInvoke);
	/*END INIT BODY PARAMETER READS*/

	// Init managed object ref counting
	Plugin::RefCountsLenClass = maxManagedObjects;
	Plugin::RefCountsClass = (int32_t*)curMemory;
	curMemory += maxManagedObjects * sizeof(int32_t);

	/*BEGIN INIT BODY ARRAYS*/
	Plugin::RefCountsSystemDecimal = (int32_t*)curMemory;
	curMemory += 1000 * sizeof(int32_t);
	Plugin::RefCountsLenSystemDecimal = 1000;
	
	Plugin::BaseNativeDownloadHandlerFreeListSize = 1000;
	Plugin::BaseNativeDownloadHandlerFreeList = (CesiumForUnity::BaseNativeDownloadHandler**)curMemory;
	curMemory += 1000 * sizeof(CesiumForUnity::BaseNativeDownloadHandler*);
	
	Plugin::BaseNativeDownloadHandlerFreeWholeListSize = 1000;
	Plugin::BaseNativeDownloadHandlerFreeWholeList = (Plugin::BaseNativeDownloadHandlerFreeWholeListEntry*)curMemory;
	curMemory += 1000 * sizeof(Plugin::BaseNativeDownloadHandlerFreeWholeListEntry);
	
	Plugin::BaseCesium3DTilesetFreeListSize = 1000;
	Plugin::BaseCesium3DTilesetFreeList = (CesiumForUnity::BaseCesium3DTileset**)curMemory;
	curMemory += 1000 * sizeof(CesiumForUnity::BaseCesium3DTileset*);
	
	Plugin::BaseCesium3DTilesetFreeWholeListSize = 1000;
	Plugin::BaseCesium3DTilesetFreeWholeList = (Plugin::BaseCesium3DTilesetFreeWholeListEntry*)curMemory;
	curMemory += 1000 * sizeof(Plugin::BaseCesium3DTilesetFreeWholeListEntry);
	
	Plugin::SystemActionFreeListSize = 1000;
	Plugin::SystemActionFreeList = (System::Action**)curMemory;
	curMemory += 1000 * sizeof(System::Action*);
	
	Plugin::SystemActionUnityEngineAsyncOperationFreeListSize = 1000;
	Plugin::SystemActionUnityEngineAsyncOperationFreeList = (System::Action_1<UnityEngine::AsyncOperation>**)curMemory;
	curMemory += 1000 * sizeof(System::Action_1<UnityEngine::AsyncOperation>*);
	/*END INIT BODY ARRAYS*/

	// Make sure there was enough memory
	int32_t usedMemory = (int32_t)(curMemory - (uint8_t*)memory);
	if (usedMemory > memorySize)
	{
		System::String msg = "Plugin memory size is too low";
		System::Exception ex(msg);
		Plugin::SetException(ex.Handle);
		return;
	}

	if (initMode == InitMode::FirstBoot)
	{
		// Clear memory
		memset(memory, 0, memorySize);

		/*BEGIN INIT BODY FIRST BOOT*/
		for (int32_t i = 0, end = Plugin::BaseNativeDownloadHandlerFreeListSize - 1; i < end; ++i)
		{
			Plugin::BaseNativeDownloadHandlerFreeList[i] = (CesiumForUnity::BaseNativeDownloadHandler*)(Plugin::BaseNativeDownloadHandlerFreeList + i + 1);
		}
		Plugin::BaseNativeDownloadHandlerFreeList[Plugin::BaseNativeDownloadHandlerFreeListSize - 1] = nullptr;
		Plugin::NextFreeBaseNativeDownloadHandler = Plugin::BaseNativeDownloadHandlerFreeList + 1;
		
		for (int32_t i = 0, end = Plugin::BaseNativeDownloadHandlerFreeWholeListSize - 1; i < end; ++i)
		{
			Plugin::BaseNativeDownloadHandlerFreeWholeList[i].Next = Plugin::BaseNativeDownloadHandlerFreeWholeList + i + 1;
		}
		Plugin::BaseNativeDownloadHandlerFreeWholeList[Plugin::BaseNativeDownloadHandlerFreeWholeListSize - 1].Next = nullptr;
		Plugin::NextFreeWholeBaseNativeDownloadHandler = Plugin::BaseNativeDownloadHandlerFreeWholeList + 1;
		
		for (int32_t i = 0, end = Plugin::BaseCesium3DTilesetFreeListSize - 1; i < end; ++i)
		{
			Plugin::BaseCesium3DTilesetFreeList[i] = (CesiumForUnity::BaseCesium3DTileset*)(Plugin::BaseCesium3DTilesetFreeList + i + 1);
		}
		Plugin::BaseCesium3DTilesetFreeList[Plugin::BaseCesium3DTilesetFreeListSize - 1] = nullptr;
		Plugin::NextFreeBaseCesium3DTileset = Plugin::BaseCesium3DTilesetFreeList + 1;
		
		for (int32_t i = 0, end = Plugin::BaseCesium3DTilesetFreeWholeListSize - 1; i < end; ++i)
		{
			Plugin::BaseCesium3DTilesetFreeWholeList[i].Next = Plugin::BaseCesium3DTilesetFreeWholeList + i + 1;
		}
		Plugin::BaseCesium3DTilesetFreeWholeList[Plugin::BaseCesium3DTilesetFreeWholeListSize - 1].Next = nullptr;
		Plugin::NextFreeWholeBaseCesium3DTileset = Plugin::BaseCesium3DTilesetFreeWholeList + 1;
		
		for (int32_t i = 0, end = Plugin::SystemActionFreeListSize - 1; i < end; ++i)
		{
			Plugin::SystemActionFreeList[i] = (System::Action*)(Plugin::SystemActionFreeList + i + 1);
		}
		Plugin::SystemActionFreeList[Plugin::SystemActionFreeListSize - 1] = nullptr;
		Plugin::NextFreeSystemAction = Plugin::SystemActionFreeList + 1;
		
		for (int32_t i = 0, end = Plugin::SystemActionUnityEngineAsyncOperationFreeListSize - 1; i < end; ++i)
		{
			Plugin::SystemActionUnityEngineAsyncOperationFreeList[i] = (System::Action_1<UnityEngine::AsyncOperation>*)(Plugin::SystemActionUnityEngineAsyncOperationFreeList + i + 1);
		}
		Plugin::SystemActionUnityEngineAsyncOperationFreeList[Plugin::SystemActionUnityEngineAsyncOperationFreeListSize - 1] = nullptr;
		Plugin::NextFreeSystemActionUnityEngineAsyncOperation = Plugin::SystemActionUnityEngineAsyncOperationFreeList + 1;
		/*END INIT BODY FIRST BOOT*/
	}

	try
	{
		PluginMain(
			curMemory,
			(int32_t)(memorySize - usedMemory),
			initMode == InitMode::FirstBoot);
	}
	catch (System::Exception ex)
	{
		Plugin::SetException(ex.Handle);
	}
	catch (...)
	{
		System::String msg = "Unhandled exception in PluginMain";
		System::Exception ex(msg);
		Plugin::SetException(ex.Handle);
	}
}

// Receive an unhandled exception from C#
DLLEXPORT void SetCsharpException(int32_t handle)
{
	Plugin::unhandledCsharpException = new System::Exception(
		Plugin::InternalUse::Only,
		handle);
}

