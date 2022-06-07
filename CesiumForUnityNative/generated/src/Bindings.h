/// <summary>
/// Declaration of the various .NET types exposed to C++
/// </summary>
/// <author>
/// Jackson Dunstan, 2017, http://JacksonDunstan.com
/// </author>
/// <license>
/// MIT
/// </license>

#pragma once

// For int32_t, etc.
#include <stdint.h>

// For size_t to support placement new and delete
#include <stdlib.h>

////////////////////////////////////////////////////////////////
// Plugin internals. Do not name these in game code as they may
// change without warning. For example:
//   // Good. Uses behavior, not names.
//   int x = myArray[5];
//   // Bad. Directly uses names.
//   ArrayElementProxy1_1 proxy = myArray[5];
//   int x = proxy;
////////////////////////////////////////////////////////////////

namespace Plugin
{
	enum struct InternalUse
	{
		Only
	};
	
	struct ManagedType
	{
		int32_t Handle;
		
		ManagedType();
		ManagedType(decltype(nullptr));
		ManagedType(InternalUse, int32_t handle);
	};
	
	template <typename TElement> struct ArrayElementProxy1_1;
	
	template <typename TElement> struct ArrayElementProxy1_2;
	template <typename TElement> struct ArrayElementProxy2_2;
	
	template <typename TElement> struct ArrayElementProxy1_3;
	template <typename TElement> struct ArrayElementProxy2_3;
	template <typename TElement> struct ArrayElementProxy3_3;
	
	template <typename TElement> struct ArrayElementProxy1_4;
	template <typename TElement> struct ArrayElementProxy2_4;
	template <typename TElement> struct ArrayElementProxy3_4;
	template <typename TElement> struct ArrayElementProxy4_4;
	
	template <typename TElement> struct ArrayElementProxy1_5;
	template <typename TElement> struct ArrayElementProxy2_5;
	template <typename TElement> struct ArrayElementProxy3_5;
	template <typename TElement> struct ArrayElementProxy4_5;
	template <typename TElement> struct ArrayElementProxy5_5;
}

////////////////////////////////////////////////////////////////
// C# basic types
////////////////////////////////////////////////////////////////

namespace System
{
	struct Object;
	struct ValueType;
	struct Enum;
	struct String;
	struct Array;
	template <typename TElement> struct Array1;
	template <typename TElement> struct Array2;
	template <typename TElement> struct Array3;
	template <typename TElement> struct Array4;
	template <typename TElement> struct Array5;
	struct IComparable;
	template <typename TElement> struct IComparable_1;
	template <typename TElement> struct IEquatable_1;
	struct IFormattable;
	struct IConvertible;
	
	// .NET booleans are four bytes long
	// This struct makes them feel like C++'s bool, int32_t, and uint32_t types
	struct Boolean
	{
		int32_t Value;
		
		Boolean();
		Boolean(bool value);
		Boolean(int32_t value);
		Boolean(uint32_t value);
		operator bool() const;
		operator int32_t() const;
		operator uint32_t() const;
		explicit operator Object() const;
		explicit operator ValueType() const;
		explicit operator IComparable() const;
		explicit operator IFormattable() const;
		explicit operator IConvertible() const;
		explicit operator IComparable_1<Boolean>() const;
		explicit operator IEquatable_1<Boolean>() const;
	};
	
	// .NET chars are two bytes long
	// This struct helps them interoperate with C++'s char and int16_t types
	struct Char
	{
		int16_t Value;
		
		Char();
		Char(char value);
		Char(int16_t value);
		operator int16_t() const;
		explicit operator Object() const;
		explicit operator ValueType() const;
		explicit operator IComparable() const;
		explicit operator IFormattable() const;
		explicit operator IConvertible() const;
		explicit operator IComparable_1<Char>() const;
		explicit operator IEquatable_1<Char>() const;
	};
	
	struct SByte
	{
		int8_t Value;
		
		SByte();
		SByte(int8_t value);
		operator int8_t() const;
		explicit operator Object() const;
		explicit operator ValueType() const;
		explicit operator IComparable() const;
		explicit operator IFormattable() const;
		explicit operator IConvertible() const;
		explicit operator IComparable_1<SByte>() const;
		explicit operator IEquatable_1<SByte>() const;
	};
	
	struct Byte
	{
		uint8_t Value;
		
		Byte();
		Byte(uint8_t value);
		operator uint8_t() const;
		explicit operator Object() const;
		explicit operator ValueType() const;
		explicit operator IComparable() const;
		explicit operator IFormattable() const;
		explicit operator IConvertible() const;
		explicit operator IComparable_1<Byte>() const;
		explicit operator IEquatable_1<Byte>() const;
	};
	
	struct Int16
	{
		int16_t Value;
		
		Int16();
		Int16(int16_t value);
		operator int16_t() const;
		explicit operator Object() const;
		explicit operator ValueType() const;
		explicit operator IComparable() const;
		explicit operator IFormattable() const;
		explicit operator IConvertible() const;
		explicit operator IComparable_1<Int16>() const;
		explicit operator IEquatable_1<Int16>() const;
	};
	
	struct UInt16
	{
		uint16_t Value;
		
		UInt16();
		UInt16(uint16_t value);
		operator uint16_t() const;
		explicit operator Object() const;
		explicit operator ValueType() const;
		explicit operator IComparable() const;
		explicit operator IFormattable() const;
		explicit operator IConvertible() const;
		explicit operator IComparable_1<UInt16>() const;
		explicit operator IEquatable_1<UInt16>() const;
	};
	
	struct Int32
	{
		int32_t Value;
		
		Int32();
		Int32(int32_t value);
		operator int32_t() const;
		explicit operator Object() const;
		explicit operator ValueType() const;
		explicit operator IComparable() const;
		explicit operator IFormattable() const;
		explicit operator IConvertible() const;
		explicit operator IComparable_1<Int32>() const;
		explicit operator IEquatable_1<Int32>() const;
	};
	
	struct UInt32
	{
		uint32_t Value;
		
		UInt32();
		UInt32(uint32_t value);
		operator uint32_t() const;
		explicit operator Object() const;
		explicit operator ValueType() const;
		explicit operator IComparable() const;
		explicit operator IFormattable() const;
		explicit operator IConvertible() const;
		explicit operator IComparable_1<UInt32>() const;
		explicit operator IEquatable_1<UInt32>() const;
	};
	
	struct Int64
	{
		int64_t Value;
		
		Int64();
		Int64(int64_t value);
		operator int64_t() const;
		explicit operator Object() const;
		explicit operator ValueType() const;
		explicit operator IComparable() const;
		explicit operator IFormattable() const;
		explicit operator IConvertible() const;
		explicit operator IComparable_1<Int64>() const;
		explicit operator IEquatable_1<Int64>() const;
	};
	
	struct UInt64
	{
		uint64_t Value;
		
		UInt64();
		UInt64(uint64_t value);
		operator uint64_t() const;
		explicit operator Object() const;
		explicit operator ValueType() const;
		explicit operator IComparable() const;
		explicit operator IFormattable() const;
		explicit operator IConvertible() const;
		explicit operator IComparable_1<UInt64>() const;
		explicit operator IEquatable_1<UInt64>() const;
	};
	
	struct Single
	{
		float Value;
		
		Single();
		Single(float value);
		operator float() const;
		explicit operator Object() const;
		explicit operator ValueType() const;
		explicit operator IComparable() const;
		explicit operator IFormattable() const;
		explicit operator IConvertible() const;
		explicit operator IComparable_1<Single>() const;
		explicit operator IEquatable_1<Single>() const;
	};
	
	struct Double
	{
		double Value;
		
		Double();
		Double(double value);
		operator double() const;
		explicit operator Object() const;
		explicit operator ValueType() const;
		explicit operator IComparable() const;
		explicit operator IFormattable() const;
		explicit operator IConvertible() const;
		explicit operator IComparable_1<Double>() const;
		explicit operator IEquatable_1<Double>() const;
	};
}

/*BEGIN TEMPLATE DECLARATIONS*/

/*END TEMPLATE DECLARATIONS*/

/*BEGIN TYPE DECLARATIONS*/

/*END TYPE DECLARATIONS*/

/*BEGIN TEMPLATE SPECIALIZATION DECLARATIONS*/

/*END TEMPLATE SPECIALIZATION DECLARATIONS*/

////////////////////////////////////////////////////////////////
// C# type definitions
////////////////////////////////////////////////////////////////

namespace System
{
	struct Object : Plugin::ManagedType
	{
		Object();
		Object(Plugin::InternalUse iu, int32_t handle);
		Object(decltype(nullptr));
		virtual ~Object();
		bool operator==(decltype(nullptr)) const;
		bool operator!=(decltype(nullptr)) const;
		virtual void ThrowReferenceToThis();
		
		/*BEGIN UNBOXING METHOD DECLARATIONS*/

		/*END UNBOXING METHOD DECLARATIONS*/
	};
	
	struct ValueType : virtual Object
	{
		ValueType(Plugin::InternalUse iu, int32_t handle);
		ValueType(decltype(nullptr));
	};
	
	struct Enum : virtual ValueType
	{
		Enum(Plugin::InternalUse iu, int32_t handle);
		Enum(decltype(nullptr));
	};
	
	struct String : virtual Object
	{
		String(Plugin::InternalUse iu, int32_t handle);
		String(decltype(nullptr));
		String(const String& other);
		String(String&& other);
		virtual ~String();
		String& operator=(const String& other);
		String& operator=(decltype(nullptr));
		String& operator=(String&& other);
		String(const char* chars);
	};
	
	struct ICloneable : virtual Object
	{
		ICloneable(Plugin::InternalUse iu, int32_t handle);
		ICloneable(decltype(nullptr));
	};
	
	namespace Collections
	{
		struct IEnumerable : virtual Object
		{
			IEnumerable(Plugin::InternalUse iu, int32_t handle);
			IEnumerable(decltype(nullptr));
			IEnumerator GetEnumerator();
		};
		
		struct ICollection : virtual IEnumerable
		{
			ICollection(Plugin::InternalUse iu, int32_t handle);
			ICollection(decltype(nullptr));
		};
		
		struct IList : virtual ICollection, virtual IEnumerable
		{
			IList(Plugin::InternalUse iu, int32_t handle);
			IList(decltype(nullptr));
		};
	}
	
	struct Array : virtual ICloneable, virtual Collections::IList
	{
		Array(Plugin::InternalUse iu, int32_t handle);
		Array(decltype(nullptr));
		int32_t GetLength();
		int32_t GetRank();
	};
}

////////////////////////////////////////////////////////////////
// Global variables
////////////////////////////////////////////////////////////////

namespace Plugin
{
	extern System::String NullString;
}

/*BEGIN TYPE DEFINITIONS*/

/*END TYPE DEFINITIONS*/

/*BEGIN MACROS*/

/*END MACROS*/

////////////////////////////////////////////////////////////////
// Support for using IEnumerable with range for loops
////////////////////////////////////////////////////////////////

namespace Plugin
{
	struct EnumerableIterator
	{
		System::Collections::IEnumerator enumerator;
		bool hasMore;
		EnumerableIterator(decltype(nullptr));
		EnumerableIterator(System::Collections::IEnumerable& enumerable);
		EnumerableIterator& operator++();
		bool operator!=(const EnumerableIterator& other);
		System::Object operator*();
	};
}

namespace System
{
	namespace Collections
	{
		Plugin::EnumerableIterator begin(IEnumerable& enumerable);
		Plugin::EnumerableIterator end(IEnumerable& enumerable);
	}
}

////////////////////////////////////////////////////////////////
// User-defined literals for creating decimals (System.Decimal)
////////////////////////////////////////////////////////////////

System::Decimal operator"" _m(long double x);
System::Decimal operator"" _m(unsigned long long x);
