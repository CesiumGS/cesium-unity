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

// Game type definitions
#include "Game.h"

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

	/*END INIT BODY PARAMETER READS*/
	
	// Init managed object ref counting
	Plugin::RefCountsLenClass = maxManagedObjects;
	Plugin::RefCountsClass = (int32_t*)curMemory;
	curMemory += maxManagedObjects * sizeof(int32_t);
	
	/*BEGIN INIT BODY ARRAYS*/

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

