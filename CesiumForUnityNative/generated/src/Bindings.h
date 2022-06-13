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
namespace System
{
	template<typename TT0> struct IEquatable_1;
}

namespace System
{
	template<typename TT0> struct IComparable_1;
}

namespace System
{
	template<typename TT0> struct Action_1;
}
/*END TEMPLATE DECLARATIONS*/

/*BEGIN TYPE DECLARATIONS*/
namespace System
{
	struct IFormattable;
}

namespace System
{
	struct IConvertible;
}

namespace System
{
	struct IComparable;
}

namespace System
{
	struct ISpanFormattable;
}

namespace System
{
	struct IDisposable;
}

namespace System
{
	namespace Runtime
	{
		namespace Serialization
		{
			struct IDeserializationCallback;
		}
	}
}

namespace System
{
	struct Decimal;
}

namespace UnityEngine
{
	struct Vector3;
}

namespace UnityEngine
{
	struct Object;
}

namespace UnityEngine
{
	struct Component;
}

namespace UnityEngine
{
	struct Transform;
}

namespace System
{
	namespace Collections
	{
		struct IEnumerator;
	}
}

namespace System
{
	namespace Runtime
	{
		namespace Serialization
		{
			struct ISerializable;
		}
	}
}

namespace System
{
	namespace Runtime
	{
		namespace InteropServices
		{
			struct _Exception;
		}
	}
}

namespace UnityEngine
{
	struct GameObject;
}

namespace UnityEngine
{
	struct Debug;
}

namespace UnityEngine
{
	struct Behaviour;
}

namespace UnityEngine
{
	struct MonoBehaviour;
}

namespace System
{
	struct Exception;
}

namespace System
{
	struct SystemException;
}

namespace System
{
	struct NullReferenceException;
}

namespace UnityEngine
{
	struct PrimitiveType;
}

namespace UnityEngine
{
	struct Time;
}

namespace UnityEngine
{
	struct Camera;
}

namespace UnityEngine
{
	struct YieldInstruction;
}

namespace UnityEngine
{
	struct AsyncOperation;
}

namespace UnityEngine
{
	namespace Networking
	{
		struct DownloadHandler;
	}
}

namespace UnityEngine
{
	namespace Networking
	{
		struct DownloadHandlerScript;
	}
}

namespace CesiumForUnity
{
	struct RawDownloadedData;
}

namespace CesiumForUnity
{
	struct AbstractBaseNativeDownloadHandler;
}

namespace CesiumForUnity
{
	struct BaseNativeDownloadHandler;
}

namespace UnityEngine
{
	namespace Networking
	{
		struct UnityWebRequest;
	}
}

namespace UnityEngine
{
	namespace Networking
	{
		struct UnityWebRequestAsyncOperation;
	}
}

namespace System
{
	namespace Runtime
	{
		namespace InteropServices
		{
			namespace Marshal
			{
			}
		}
	}
}

namespace CesiumForUnity
{
	struct AbstractBaseCesium3DTileset;
}

namespace CesiumForUnity
{
	struct BaseCesium3DTileset;
}

namespace System
{
	struct IAsyncResult;
}

namespace System
{
	namespace Threading
	{
		struct IThreadPoolWorkItem;
	}
}

namespace System
{
	namespace Threading
	{
		namespace Tasks
		{
			struct Task;
		}
	}
}

namespace System
{
	struct Action;
}
/*END TYPE DECLARATIONS*/

/*BEGIN TEMPLATE SPECIALIZATION DECLARATIONS*/
namespace System
{
	template<> struct IEquatable_1<System::Boolean>;
}

namespace System
{
	template<> struct IEquatable_1<System::Char>;
}

namespace System
{
	template<> struct IEquatable_1<System::SByte>;
}

namespace System
{
	template<> struct IEquatable_1<System::Byte>;
}

namespace System
{
	template<> struct IEquatable_1<System::Int16>;
}

namespace System
{
	template<> struct IEquatable_1<System::UInt16>;
}

namespace System
{
	template<> struct IEquatable_1<System::Int32>;
}

namespace System
{
	template<> struct IEquatable_1<System::UInt32>;
}

namespace System
{
	template<> struct IEquatable_1<System::Int64>;
}

namespace System
{
	template<> struct IEquatable_1<System::UInt64>;
}

namespace System
{
	template<> struct IEquatable_1<System::Single>;
}

namespace System
{
	template<> struct IEquatable_1<System::Double>;
}

namespace System
{
	template<> struct IEquatable_1<System::Decimal>;
}

namespace System
{
	template<> struct IEquatable_1<UnityEngine::Vector3>;
}

namespace System
{
	template<> struct IComparable_1<System::Boolean>;
}

namespace System
{
	template<> struct IComparable_1<System::Char>;
}

namespace System
{
	template<> struct IComparable_1<System::SByte>;
}

namespace System
{
	template<> struct IComparable_1<System::Byte>;
}

namespace System
{
	template<> struct IComparable_1<System::Int16>;
}

namespace System
{
	template<> struct IComparable_1<System::UInt16>;
}

namespace System
{
	template<> struct IComparable_1<System::Int32>;
}

namespace System
{
	template<> struct IComparable_1<System::UInt32>;
}

namespace System
{
	template<> struct IComparable_1<System::Int64>;
}

namespace System
{
	template<> struct IComparable_1<System::UInt64>;
}

namespace System
{
	template<> struct IComparable_1<System::Single>;
}

namespace System
{
	template<> struct IComparable_1<System::Double>;
}

namespace System
{
	template<> struct IComparable_1<System::Decimal>;
}

namespace System
{
	template<> struct Action_1<UnityEngine::AsyncOperation>;
}
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
		explicit operator System::Decimal();
		explicit operator UnityEngine::Vector3();
		explicit operator UnityEngine::PrimitiveType();
		explicit operator CesiumForUnity::RawDownloadedData();
		explicit operator System::Boolean();
		explicit operator System::SByte();
		explicit operator System::Byte();
		explicit operator System::Int16();
		explicit operator System::UInt16();
		explicit operator System::Int32();
		explicit operator System::UInt32();
		explicit operator System::Int64();
		explicit operator System::UInt64();
		explicit operator System::Char();
		explicit operator System::Single();
		explicit operator System::Double();
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
namespace System
{
	struct IFormattable : virtual System::Object
	{
		IFormattable(decltype(nullptr));
		IFormattable(Plugin::InternalUse, int32_t handle);
		IFormattable(const IFormattable& other);
		IFormattable(IFormattable&& other);
		virtual ~IFormattable();
		IFormattable& operator=(const IFormattable& other);
		IFormattable& operator=(decltype(nullptr));
		IFormattable& operator=(IFormattable&& other);
		bool operator==(const IFormattable& other) const;
		bool operator!=(const IFormattable& other) const;
	};
}

namespace System
{
	struct IConvertible : virtual System::Object
	{
		IConvertible(decltype(nullptr));
		IConvertible(Plugin::InternalUse, int32_t handle);
		IConvertible(const IConvertible& other);
		IConvertible(IConvertible&& other);
		virtual ~IConvertible();
		IConvertible& operator=(const IConvertible& other);
		IConvertible& operator=(decltype(nullptr));
		IConvertible& operator=(IConvertible&& other);
		bool operator==(const IConvertible& other) const;
		bool operator!=(const IConvertible& other) const;
	};
}

namespace System
{
	struct IComparable : virtual System::Object
	{
		IComparable(decltype(nullptr));
		IComparable(Plugin::InternalUse, int32_t handle);
		IComparable(const IComparable& other);
		IComparable(IComparable&& other);
		virtual ~IComparable();
		IComparable& operator=(const IComparable& other);
		IComparable& operator=(decltype(nullptr));
		IComparable& operator=(IComparable&& other);
		bool operator==(const IComparable& other) const;
		bool operator!=(const IComparable& other) const;
	};
}

namespace System
{
	struct ISpanFormattable : virtual System::Object
	{
		ISpanFormattable(decltype(nullptr));
		ISpanFormattable(Plugin::InternalUse, int32_t handle);
		ISpanFormattable(const ISpanFormattable& other);
		ISpanFormattable(ISpanFormattable&& other);
		virtual ~ISpanFormattable();
		ISpanFormattable& operator=(const ISpanFormattable& other);
		ISpanFormattable& operator=(decltype(nullptr));
		ISpanFormattable& operator=(ISpanFormattable&& other);
		bool operator==(const ISpanFormattable& other) const;
		bool operator!=(const ISpanFormattable& other) const;
	};
}

namespace System
{
	struct IDisposable : virtual System::Object
	{
		IDisposable(decltype(nullptr));
		IDisposable(Plugin::InternalUse, int32_t handle);
		IDisposable(const IDisposable& other);
		IDisposable(IDisposable&& other);
		virtual ~IDisposable();
		IDisposable& operator=(const IDisposable& other);
		IDisposable& operator=(decltype(nullptr));
		IDisposable& operator=(IDisposable&& other);
		bool operator==(const IDisposable& other) const;
		bool operator!=(const IDisposable& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<System::Boolean> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<System::Boolean>& other);
		IEquatable_1(IEquatable_1<System::Boolean>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<System::Boolean>& operator=(const IEquatable_1<System::Boolean>& other);
		IEquatable_1<System::Boolean>& operator=(decltype(nullptr));
		IEquatable_1<System::Boolean>& operator=(IEquatable_1<System::Boolean>&& other);
		bool operator==(const IEquatable_1<System::Boolean>& other) const;
		bool operator!=(const IEquatable_1<System::Boolean>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<System::Char> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<System::Char>& other);
		IEquatable_1(IEquatable_1<System::Char>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<System::Char>& operator=(const IEquatable_1<System::Char>& other);
		IEquatable_1<System::Char>& operator=(decltype(nullptr));
		IEquatable_1<System::Char>& operator=(IEquatable_1<System::Char>&& other);
		bool operator==(const IEquatable_1<System::Char>& other) const;
		bool operator!=(const IEquatable_1<System::Char>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<System::SByte> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<System::SByte>& other);
		IEquatable_1(IEquatable_1<System::SByte>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<System::SByte>& operator=(const IEquatable_1<System::SByte>& other);
		IEquatable_1<System::SByte>& operator=(decltype(nullptr));
		IEquatable_1<System::SByte>& operator=(IEquatable_1<System::SByte>&& other);
		bool operator==(const IEquatable_1<System::SByte>& other) const;
		bool operator!=(const IEquatable_1<System::SByte>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<System::Byte> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<System::Byte>& other);
		IEquatable_1(IEquatable_1<System::Byte>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<System::Byte>& operator=(const IEquatable_1<System::Byte>& other);
		IEquatable_1<System::Byte>& operator=(decltype(nullptr));
		IEquatable_1<System::Byte>& operator=(IEquatable_1<System::Byte>&& other);
		bool operator==(const IEquatable_1<System::Byte>& other) const;
		bool operator!=(const IEquatable_1<System::Byte>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<System::Int16> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<System::Int16>& other);
		IEquatable_1(IEquatable_1<System::Int16>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<System::Int16>& operator=(const IEquatable_1<System::Int16>& other);
		IEquatable_1<System::Int16>& operator=(decltype(nullptr));
		IEquatable_1<System::Int16>& operator=(IEquatable_1<System::Int16>&& other);
		bool operator==(const IEquatable_1<System::Int16>& other) const;
		bool operator!=(const IEquatable_1<System::Int16>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<System::UInt16> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<System::UInt16>& other);
		IEquatable_1(IEquatable_1<System::UInt16>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<System::UInt16>& operator=(const IEquatable_1<System::UInt16>& other);
		IEquatable_1<System::UInt16>& operator=(decltype(nullptr));
		IEquatable_1<System::UInt16>& operator=(IEquatable_1<System::UInt16>&& other);
		bool operator==(const IEquatable_1<System::UInt16>& other) const;
		bool operator!=(const IEquatable_1<System::UInt16>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<System::Int32> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<System::Int32>& other);
		IEquatable_1(IEquatable_1<System::Int32>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<System::Int32>& operator=(const IEquatable_1<System::Int32>& other);
		IEquatable_1<System::Int32>& operator=(decltype(nullptr));
		IEquatable_1<System::Int32>& operator=(IEquatable_1<System::Int32>&& other);
		bool operator==(const IEquatable_1<System::Int32>& other) const;
		bool operator!=(const IEquatable_1<System::Int32>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<System::UInt32> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<System::UInt32>& other);
		IEquatable_1(IEquatable_1<System::UInt32>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<System::UInt32>& operator=(const IEquatable_1<System::UInt32>& other);
		IEquatable_1<System::UInt32>& operator=(decltype(nullptr));
		IEquatable_1<System::UInt32>& operator=(IEquatable_1<System::UInt32>&& other);
		bool operator==(const IEquatable_1<System::UInt32>& other) const;
		bool operator!=(const IEquatable_1<System::UInt32>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<System::Int64> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<System::Int64>& other);
		IEquatable_1(IEquatable_1<System::Int64>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<System::Int64>& operator=(const IEquatable_1<System::Int64>& other);
		IEquatable_1<System::Int64>& operator=(decltype(nullptr));
		IEquatable_1<System::Int64>& operator=(IEquatable_1<System::Int64>&& other);
		bool operator==(const IEquatable_1<System::Int64>& other) const;
		bool operator!=(const IEquatable_1<System::Int64>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<System::UInt64> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<System::UInt64>& other);
		IEquatable_1(IEquatable_1<System::UInt64>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<System::UInt64>& operator=(const IEquatable_1<System::UInt64>& other);
		IEquatable_1<System::UInt64>& operator=(decltype(nullptr));
		IEquatable_1<System::UInt64>& operator=(IEquatable_1<System::UInt64>&& other);
		bool operator==(const IEquatable_1<System::UInt64>& other) const;
		bool operator!=(const IEquatable_1<System::UInt64>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<System::Single> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<System::Single>& other);
		IEquatable_1(IEquatable_1<System::Single>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<System::Single>& operator=(const IEquatable_1<System::Single>& other);
		IEquatable_1<System::Single>& operator=(decltype(nullptr));
		IEquatable_1<System::Single>& operator=(IEquatable_1<System::Single>&& other);
		bool operator==(const IEquatable_1<System::Single>& other) const;
		bool operator!=(const IEquatable_1<System::Single>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<System::Double> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<System::Double>& other);
		IEquatable_1(IEquatable_1<System::Double>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<System::Double>& operator=(const IEquatable_1<System::Double>& other);
		IEquatable_1<System::Double>& operator=(decltype(nullptr));
		IEquatable_1<System::Double>& operator=(IEquatable_1<System::Double>&& other);
		bool operator==(const IEquatable_1<System::Double>& other) const;
		bool operator!=(const IEquatable_1<System::Double>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<System::Decimal> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<System::Decimal>& other);
		IEquatable_1(IEquatable_1<System::Decimal>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<System::Decimal>& operator=(const IEquatable_1<System::Decimal>& other);
		IEquatable_1<System::Decimal>& operator=(decltype(nullptr));
		IEquatable_1<System::Decimal>& operator=(IEquatable_1<System::Decimal>&& other);
		bool operator==(const IEquatable_1<System::Decimal>& other) const;
		bool operator!=(const IEquatable_1<System::Decimal>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<UnityEngine::Vector3> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<UnityEngine::Vector3>& other);
		IEquatable_1(IEquatable_1<UnityEngine::Vector3>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<UnityEngine::Vector3>& operator=(const IEquatable_1<UnityEngine::Vector3>& other);
		IEquatable_1<UnityEngine::Vector3>& operator=(decltype(nullptr));
		IEquatable_1<UnityEngine::Vector3>& operator=(IEquatable_1<UnityEngine::Vector3>&& other);
		bool operator==(const IEquatable_1<UnityEngine::Vector3>& other) const;
		bool operator!=(const IEquatable_1<UnityEngine::Vector3>& other) const;
	};
}

namespace System
{
	template<> struct IComparable_1<System::Boolean> : virtual System::Object
	{
		IComparable_1(decltype(nullptr));
		IComparable_1(Plugin::InternalUse, int32_t handle);
		IComparable_1(const IComparable_1<System::Boolean>& other);
		IComparable_1(IComparable_1<System::Boolean>&& other);
		virtual ~IComparable_1();
		IComparable_1<System::Boolean>& operator=(const IComparable_1<System::Boolean>& other);
		IComparable_1<System::Boolean>& operator=(decltype(nullptr));
		IComparable_1<System::Boolean>& operator=(IComparable_1<System::Boolean>&& other);
		bool operator==(const IComparable_1<System::Boolean>& other) const;
		bool operator!=(const IComparable_1<System::Boolean>& other) const;
	};
}

namespace System
{
	template<> struct IComparable_1<System::Char> : virtual System::Object
	{
		IComparable_1(decltype(nullptr));
		IComparable_1(Plugin::InternalUse, int32_t handle);
		IComparable_1(const IComparable_1<System::Char>& other);
		IComparable_1(IComparable_1<System::Char>&& other);
		virtual ~IComparable_1();
		IComparable_1<System::Char>& operator=(const IComparable_1<System::Char>& other);
		IComparable_1<System::Char>& operator=(decltype(nullptr));
		IComparable_1<System::Char>& operator=(IComparable_1<System::Char>&& other);
		bool operator==(const IComparable_1<System::Char>& other) const;
		bool operator!=(const IComparable_1<System::Char>& other) const;
	};
}

namespace System
{
	template<> struct IComparable_1<System::SByte> : virtual System::Object
	{
		IComparable_1(decltype(nullptr));
		IComparable_1(Plugin::InternalUse, int32_t handle);
		IComparable_1(const IComparable_1<System::SByte>& other);
		IComparable_1(IComparable_1<System::SByte>&& other);
		virtual ~IComparable_1();
		IComparable_1<System::SByte>& operator=(const IComparable_1<System::SByte>& other);
		IComparable_1<System::SByte>& operator=(decltype(nullptr));
		IComparable_1<System::SByte>& operator=(IComparable_1<System::SByte>&& other);
		bool operator==(const IComparable_1<System::SByte>& other) const;
		bool operator!=(const IComparable_1<System::SByte>& other) const;
	};
}

namespace System
{
	template<> struct IComparable_1<System::Byte> : virtual System::Object
	{
		IComparable_1(decltype(nullptr));
		IComparable_1(Plugin::InternalUse, int32_t handle);
		IComparable_1(const IComparable_1<System::Byte>& other);
		IComparable_1(IComparable_1<System::Byte>&& other);
		virtual ~IComparable_1();
		IComparable_1<System::Byte>& operator=(const IComparable_1<System::Byte>& other);
		IComparable_1<System::Byte>& operator=(decltype(nullptr));
		IComparable_1<System::Byte>& operator=(IComparable_1<System::Byte>&& other);
		bool operator==(const IComparable_1<System::Byte>& other) const;
		bool operator!=(const IComparable_1<System::Byte>& other) const;
	};
}

namespace System
{
	template<> struct IComparable_1<System::Int16> : virtual System::Object
	{
		IComparable_1(decltype(nullptr));
		IComparable_1(Plugin::InternalUse, int32_t handle);
		IComparable_1(const IComparable_1<System::Int16>& other);
		IComparable_1(IComparable_1<System::Int16>&& other);
		virtual ~IComparable_1();
		IComparable_1<System::Int16>& operator=(const IComparable_1<System::Int16>& other);
		IComparable_1<System::Int16>& operator=(decltype(nullptr));
		IComparable_1<System::Int16>& operator=(IComparable_1<System::Int16>&& other);
		bool operator==(const IComparable_1<System::Int16>& other) const;
		bool operator!=(const IComparable_1<System::Int16>& other) const;
	};
}

namespace System
{
	template<> struct IComparable_1<System::UInt16> : virtual System::Object
	{
		IComparable_1(decltype(nullptr));
		IComparable_1(Plugin::InternalUse, int32_t handle);
		IComparable_1(const IComparable_1<System::UInt16>& other);
		IComparable_1(IComparable_1<System::UInt16>&& other);
		virtual ~IComparable_1();
		IComparable_1<System::UInt16>& operator=(const IComparable_1<System::UInt16>& other);
		IComparable_1<System::UInt16>& operator=(decltype(nullptr));
		IComparable_1<System::UInt16>& operator=(IComparable_1<System::UInt16>&& other);
		bool operator==(const IComparable_1<System::UInt16>& other) const;
		bool operator!=(const IComparable_1<System::UInt16>& other) const;
	};
}

namespace System
{
	template<> struct IComparable_1<System::Int32> : virtual System::Object
	{
		IComparable_1(decltype(nullptr));
		IComparable_1(Plugin::InternalUse, int32_t handle);
		IComparable_1(const IComparable_1<System::Int32>& other);
		IComparable_1(IComparable_1<System::Int32>&& other);
		virtual ~IComparable_1();
		IComparable_1<System::Int32>& operator=(const IComparable_1<System::Int32>& other);
		IComparable_1<System::Int32>& operator=(decltype(nullptr));
		IComparable_1<System::Int32>& operator=(IComparable_1<System::Int32>&& other);
		bool operator==(const IComparable_1<System::Int32>& other) const;
		bool operator!=(const IComparable_1<System::Int32>& other) const;
	};
}

namespace System
{
	template<> struct IComparable_1<System::UInt32> : virtual System::Object
	{
		IComparable_1(decltype(nullptr));
		IComparable_1(Plugin::InternalUse, int32_t handle);
		IComparable_1(const IComparable_1<System::UInt32>& other);
		IComparable_1(IComparable_1<System::UInt32>&& other);
		virtual ~IComparable_1();
		IComparable_1<System::UInt32>& operator=(const IComparable_1<System::UInt32>& other);
		IComparable_1<System::UInt32>& operator=(decltype(nullptr));
		IComparable_1<System::UInt32>& operator=(IComparable_1<System::UInt32>&& other);
		bool operator==(const IComparable_1<System::UInt32>& other) const;
		bool operator!=(const IComparable_1<System::UInt32>& other) const;
	};
}

namespace System
{
	template<> struct IComparable_1<System::Int64> : virtual System::Object
	{
		IComparable_1(decltype(nullptr));
		IComparable_1(Plugin::InternalUse, int32_t handle);
		IComparable_1(const IComparable_1<System::Int64>& other);
		IComparable_1(IComparable_1<System::Int64>&& other);
		virtual ~IComparable_1();
		IComparable_1<System::Int64>& operator=(const IComparable_1<System::Int64>& other);
		IComparable_1<System::Int64>& operator=(decltype(nullptr));
		IComparable_1<System::Int64>& operator=(IComparable_1<System::Int64>&& other);
		bool operator==(const IComparable_1<System::Int64>& other) const;
		bool operator!=(const IComparable_1<System::Int64>& other) const;
	};
}

namespace System
{
	template<> struct IComparable_1<System::UInt64> : virtual System::Object
	{
		IComparable_1(decltype(nullptr));
		IComparable_1(Plugin::InternalUse, int32_t handle);
		IComparable_1(const IComparable_1<System::UInt64>& other);
		IComparable_1(IComparable_1<System::UInt64>&& other);
		virtual ~IComparable_1();
		IComparable_1<System::UInt64>& operator=(const IComparable_1<System::UInt64>& other);
		IComparable_1<System::UInt64>& operator=(decltype(nullptr));
		IComparable_1<System::UInt64>& operator=(IComparable_1<System::UInt64>&& other);
		bool operator==(const IComparable_1<System::UInt64>& other) const;
		bool operator!=(const IComparable_1<System::UInt64>& other) const;
	};
}

namespace System
{
	template<> struct IComparable_1<System::Single> : virtual System::Object
	{
		IComparable_1(decltype(nullptr));
		IComparable_1(Plugin::InternalUse, int32_t handle);
		IComparable_1(const IComparable_1<System::Single>& other);
		IComparable_1(IComparable_1<System::Single>&& other);
		virtual ~IComparable_1();
		IComparable_1<System::Single>& operator=(const IComparable_1<System::Single>& other);
		IComparable_1<System::Single>& operator=(decltype(nullptr));
		IComparable_1<System::Single>& operator=(IComparable_1<System::Single>&& other);
		bool operator==(const IComparable_1<System::Single>& other) const;
		bool operator!=(const IComparable_1<System::Single>& other) const;
	};
}

namespace System
{
	template<> struct IComparable_1<System::Double> : virtual System::Object
	{
		IComparable_1(decltype(nullptr));
		IComparable_1(Plugin::InternalUse, int32_t handle);
		IComparable_1(const IComparable_1<System::Double>& other);
		IComparable_1(IComparable_1<System::Double>&& other);
		virtual ~IComparable_1();
		IComparable_1<System::Double>& operator=(const IComparable_1<System::Double>& other);
		IComparable_1<System::Double>& operator=(decltype(nullptr));
		IComparable_1<System::Double>& operator=(IComparable_1<System::Double>&& other);
		bool operator==(const IComparable_1<System::Double>& other) const;
		bool operator!=(const IComparable_1<System::Double>& other) const;
	};
}

namespace System
{
	template<> struct IComparable_1<System::Decimal> : virtual System::Object
	{
		IComparable_1(decltype(nullptr));
		IComparable_1(Plugin::InternalUse, int32_t handle);
		IComparable_1(const IComparable_1<System::Decimal>& other);
		IComparable_1(IComparable_1<System::Decimal>&& other);
		virtual ~IComparable_1();
		IComparable_1<System::Decimal>& operator=(const IComparable_1<System::Decimal>& other);
		IComparable_1<System::Decimal>& operator=(decltype(nullptr));
		IComparable_1<System::Decimal>& operator=(IComparable_1<System::Decimal>&& other);
		bool operator==(const IComparable_1<System::Decimal>& other) const;
		bool operator!=(const IComparable_1<System::Decimal>& other) const;
	};
}

namespace System
{
	namespace Runtime
	{
		namespace Serialization
		{
			struct IDeserializationCallback : virtual System::Object
			{
				IDeserializationCallback(decltype(nullptr));
				IDeserializationCallback(Plugin::InternalUse, int32_t handle);
				IDeserializationCallback(const IDeserializationCallback& other);
				IDeserializationCallback(IDeserializationCallback&& other);
				virtual ~IDeserializationCallback();
				IDeserializationCallback& operator=(const IDeserializationCallback& other);
				IDeserializationCallback& operator=(decltype(nullptr));
				IDeserializationCallback& operator=(IDeserializationCallback&& other);
				bool operator==(const IDeserializationCallback& other) const;
				bool operator!=(const IDeserializationCallback& other) const;
			};
		}
	}
}

namespace System
{
	struct Decimal : Plugin::ManagedType
	{
		Decimal(decltype(nullptr));
		Decimal(Plugin::InternalUse, int32_t handle);
		Decimal(const Decimal& other);
		Decimal(Decimal&& other);
		virtual ~Decimal();
		Decimal& operator=(const Decimal& other);
		Decimal& operator=(decltype(nullptr));
		Decimal& operator=(Decimal&& other);
		bool operator==(const Decimal& other) const;
		bool operator!=(const Decimal& other) const;
		Decimal(System::Double value);
		Decimal(System::UInt64 value);
		explicit operator System::ValueType();
		explicit operator System::Object();
		explicit operator System::Runtime::Serialization::IDeserializationCallback();
		explicit operator System::IFormattable();
		explicit operator System::ISpanFormattable();
		explicit operator System::IComparable();
		explicit operator System::IComparable_1<System::Decimal>();
		explicit operator System::IConvertible();
		explicit operator System::IEquatable_1<System::Decimal>();
	};
}

namespace UnityEngine
{
	struct Vector3
	{
		Vector3();
		Vector3(System::Single x, System::Single y, System::Single z);
		System::Single x;
		System::Single y;
		System::Single z;
		virtual UnityEngine::Vector3 operator+(UnityEngine::Vector3& a);
		explicit operator System::ValueType();
		explicit operator System::Object();
		explicit operator System::IFormattable();
		explicit operator System::IEquatable_1<UnityEngine::Vector3>();
	};
}

namespace UnityEngine
{
	struct Object : virtual System::Object
	{
		Object(decltype(nullptr));
		Object(Plugin::InternalUse, int32_t handle);
		Object(const Object& other);
		Object(Object&& other);
		virtual ~Object();
		Object& operator=(const Object& other);
		Object& operator=(decltype(nullptr));
		Object& operator=(Object&& other);
		bool operator==(const Object& other) const;
		bool operator!=(const Object& other) const;
		System::String GetName();
		void SetName(System::String& value);
	};
}

namespace UnityEngine
{
	struct Component : virtual UnityEngine::Object
	{
		Component(decltype(nullptr));
		Component(Plugin::InternalUse, int32_t handle);
		Component(const Component& other);
		Component(Component&& other);
		virtual ~Component();
		Component& operator=(const Component& other);
		Component& operator=(decltype(nullptr));
		Component& operator=(Component&& other);
		bool operator==(const Component& other) const;
		bool operator!=(const Component& other) const;
		UnityEngine::Transform GetTransform();
	};
}

namespace UnityEngine
{
	struct Transform : virtual UnityEngine::Component, virtual System::Collections::IEnumerable
	{
		Transform(decltype(nullptr));
		Transform(Plugin::InternalUse, int32_t handle);
		Transform(const Transform& other);
		Transform(Transform&& other);
		virtual ~Transform();
		Transform& operator=(const Transform& other);
		Transform& operator=(decltype(nullptr));
		Transform& operator=(Transform&& other);
		bool operator==(const Transform& other) const;
		bool operator!=(const Transform& other) const;
		UnityEngine::Vector3 GetPosition();
		void SetPosition(UnityEngine::Vector3& value);
	};
}

namespace System
{
	namespace Collections
	{
		struct IEnumerator : virtual System::Object
		{
			IEnumerator(decltype(nullptr));
			IEnumerator(Plugin::InternalUse, int32_t handle);
			IEnumerator(const IEnumerator& other);
			IEnumerator(IEnumerator&& other);
			virtual ~IEnumerator();
			IEnumerator& operator=(const IEnumerator& other);
			IEnumerator& operator=(decltype(nullptr));
			IEnumerator& operator=(IEnumerator&& other);
			bool operator==(const IEnumerator& other) const;
			bool operator!=(const IEnumerator& other) const;
			System::Object GetCurrent();
			virtual System::Boolean MoveNext();
		};
	}
}

namespace System
{
	namespace Runtime
	{
		namespace Serialization
		{
			struct ISerializable : virtual System::Object
			{
				ISerializable(decltype(nullptr));
				ISerializable(Plugin::InternalUse, int32_t handle);
				ISerializable(const ISerializable& other);
				ISerializable(ISerializable&& other);
				virtual ~ISerializable();
				ISerializable& operator=(const ISerializable& other);
				ISerializable& operator=(decltype(nullptr));
				ISerializable& operator=(ISerializable&& other);
				bool operator==(const ISerializable& other) const;
				bool operator!=(const ISerializable& other) const;
			};
		}
	}
}

namespace System
{
	namespace Runtime
	{
		namespace InteropServices
		{
			struct _Exception : virtual System::Object
			{
				_Exception(decltype(nullptr));
				_Exception(Plugin::InternalUse, int32_t handle);
				_Exception(const _Exception& other);
				_Exception(_Exception&& other);
				virtual ~_Exception();
				_Exception& operator=(const _Exception& other);
				_Exception& operator=(decltype(nullptr));
				_Exception& operator=(_Exception&& other);
				bool operator==(const _Exception& other) const;
				bool operator!=(const _Exception& other) const;
			};
		}
	}
}

namespace UnityEngine
{
	struct GameObject : virtual UnityEngine::Object
	{
		GameObject(decltype(nullptr));
		GameObject(Plugin::InternalUse, int32_t handle);
		GameObject(const GameObject& other);
		GameObject(GameObject&& other);
		virtual ~GameObject();
		GameObject& operator=(const GameObject& other);
		GameObject& operator=(decltype(nullptr));
		GameObject& operator=(GameObject&& other);
		bool operator==(const GameObject& other) const;
		bool operator!=(const GameObject& other) const;
		template<typename MT0> MT0 AddComponent();
		static UnityEngine::GameObject CreatePrimitive(UnityEngine::PrimitiveType type);
	};
}

namespace UnityEngine
{
	struct Debug : virtual System::Object
	{
		Debug(decltype(nullptr));
		Debug(Plugin::InternalUse, int32_t handle);
		Debug(const Debug& other);
		Debug(Debug&& other);
		virtual ~Debug();
		Debug& operator=(const Debug& other);
		Debug& operator=(decltype(nullptr));
		Debug& operator=(Debug&& other);
		bool operator==(const Debug& other) const;
		bool operator!=(const Debug& other) const;
		static void Log(System::Object& message);
	};
}

namespace UnityEngine
{
	struct Behaviour : virtual UnityEngine::Component
	{
		Behaviour(decltype(nullptr));
		Behaviour(Plugin::InternalUse, int32_t handle);
		Behaviour(const Behaviour& other);
		Behaviour(Behaviour&& other);
		virtual ~Behaviour();
		Behaviour& operator=(const Behaviour& other);
		Behaviour& operator=(decltype(nullptr));
		Behaviour& operator=(Behaviour&& other);
		bool operator==(const Behaviour& other) const;
		bool operator!=(const Behaviour& other) const;
	};
}

namespace UnityEngine
{
	struct MonoBehaviour : virtual UnityEngine::Behaviour
	{
		MonoBehaviour(decltype(nullptr));
		MonoBehaviour(Plugin::InternalUse, int32_t handle);
		MonoBehaviour(const MonoBehaviour& other);
		MonoBehaviour(MonoBehaviour&& other);
		virtual ~MonoBehaviour();
		MonoBehaviour& operator=(const MonoBehaviour& other);
		MonoBehaviour& operator=(decltype(nullptr));
		MonoBehaviour& operator=(MonoBehaviour&& other);
		bool operator==(const MonoBehaviour& other) const;
		bool operator!=(const MonoBehaviour& other) const;
		UnityEngine::Transform GetTransform();
	};
}

namespace System
{
	struct Exception : virtual System::Runtime::InteropServices::_Exception, virtual System::Runtime::Serialization::ISerializable
	{
		Exception(decltype(nullptr));
		Exception(Plugin::InternalUse, int32_t handle);
		Exception(const Exception& other);
		Exception(Exception&& other);
		virtual ~Exception();
		Exception& operator=(const Exception& other);
		Exception& operator=(decltype(nullptr));
		Exception& operator=(Exception&& other);
		bool operator==(const Exception& other) const;
		bool operator!=(const Exception& other) const;
		Exception(System::String& message);
	};
}

namespace System
{
	struct SystemException : virtual System::Exception, virtual System::Runtime::InteropServices::_Exception, virtual System::Runtime::Serialization::ISerializable
	{
		SystemException(decltype(nullptr));
		SystemException(Plugin::InternalUse, int32_t handle);
		SystemException(const SystemException& other);
		SystemException(SystemException&& other);
		virtual ~SystemException();
		SystemException& operator=(const SystemException& other);
		SystemException& operator=(decltype(nullptr));
		SystemException& operator=(SystemException&& other);
		bool operator==(const SystemException& other) const;
		bool operator!=(const SystemException& other) const;
	};
}

namespace System
{
	struct NullReferenceException : virtual System::SystemException, virtual System::Runtime::InteropServices::_Exception, virtual System::Runtime::Serialization::ISerializable
	{
		NullReferenceException(decltype(nullptr));
		NullReferenceException(Plugin::InternalUse, int32_t handle);
		NullReferenceException(const NullReferenceException& other);
		NullReferenceException(NullReferenceException&& other);
		virtual ~NullReferenceException();
		NullReferenceException& operator=(const NullReferenceException& other);
		NullReferenceException& operator=(decltype(nullptr));
		NullReferenceException& operator=(NullReferenceException&& other);
		bool operator==(const NullReferenceException& other) const;
		bool operator!=(const NullReferenceException& other) const;
	};
}

namespace UnityEngine
{
	struct PrimitiveType
	{
		int32_t Value;
		static const UnityEngine::PrimitiveType Sphere;
		static const UnityEngine::PrimitiveType Capsule;
		static const UnityEngine::PrimitiveType Cylinder;
		static const UnityEngine::PrimitiveType Cube;
		static const UnityEngine::PrimitiveType Plane;
		static const UnityEngine::PrimitiveType Quad;
		explicit PrimitiveType(int32_t value);
		explicit operator int32_t() const;
		bool operator==(PrimitiveType other);
		bool operator!=(PrimitiveType other);
		explicit operator System::Enum();
		explicit operator System::ValueType();
		explicit operator System::Object();
		explicit operator System::IFormattable();
		explicit operator System::IComparable();
		explicit operator System::IConvertible();
	};
}

namespace UnityEngine
{
	struct Time : virtual System::Object
	{
		Time(decltype(nullptr));
		Time(Plugin::InternalUse, int32_t handle);
		Time(const Time& other);
		Time(Time&& other);
		virtual ~Time();
		Time& operator=(const Time& other);
		Time& operator=(decltype(nullptr));
		Time& operator=(Time&& other);
		bool operator==(const Time& other) const;
		bool operator!=(const Time& other) const;
		static System::Single GetDeltaTime();
	};
}

namespace UnityEngine
{
	struct Camera : virtual UnityEngine::Behaviour
	{
		Camera(decltype(nullptr));
		Camera(Plugin::InternalUse, int32_t handle);
		Camera(const Camera& other);
		Camera(Camera&& other);
		virtual ~Camera();
		Camera& operator=(const Camera& other);
		Camera& operator=(decltype(nullptr));
		Camera& operator=(Camera&& other);
		bool operator==(const Camera& other) const;
		bool operator!=(const Camera& other) const;
		static UnityEngine::Camera GetMain();
		System::Single GetFieldOfView();
		void SetFieldOfView(System::Single value);
		System::Single GetAspect();
		void SetAspect(System::Single value);
		System::Int32 GetPixelWidth();
		System::Int32 GetPixelHeight();
	};
}

namespace UnityEngine
{
	struct YieldInstruction : virtual System::Object
	{
		YieldInstruction(decltype(nullptr));
		YieldInstruction(Plugin::InternalUse, int32_t handle);
		YieldInstruction(const YieldInstruction& other);
		YieldInstruction(YieldInstruction&& other);
		virtual ~YieldInstruction();
		YieldInstruction& operator=(const YieldInstruction& other);
		YieldInstruction& operator=(decltype(nullptr));
		YieldInstruction& operator=(YieldInstruction&& other);
		bool operator==(const YieldInstruction& other) const;
		bool operator!=(const YieldInstruction& other) const;
	};
}

namespace UnityEngine
{
	struct AsyncOperation : virtual UnityEngine::YieldInstruction
	{
		AsyncOperation(decltype(nullptr));
		AsyncOperation(Plugin::InternalUse, int32_t handle);
		AsyncOperation(const AsyncOperation& other);
		AsyncOperation(AsyncOperation&& other);
		virtual ~AsyncOperation();
		AsyncOperation& operator=(const AsyncOperation& other);
		AsyncOperation& operator=(decltype(nullptr));
		AsyncOperation& operator=(AsyncOperation&& other);
		bool operator==(const AsyncOperation& other) const;
		bool operator!=(const AsyncOperation& other) const;
	};
}

namespace UnityEngine
{
	namespace Networking
	{
		struct DownloadHandler : virtual System::IDisposable
		{
			DownloadHandler(decltype(nullptr));
			DownloadHandler(Plugin::InternalUse, int32_t handle);
			DownloadHandler(const DownloadHandler& other);
			DownloadHandler(DownloadHandler&& other);
			virtual ~DownloadHandler();
			DownloadHandler& operator=(const DownloadHandler& other);
			DownloadHandler& operator=(decltype(nullptr));
			DownloadHandler& operator=(DownloadHandler&& other);
			bool operator==(const DownloadHandler& other) const;
			bool operator!=(const DownloadHandler& other) const;
		};
	}
}

namespace UnityEngine
{
	namespace Networking
	{
		struct DownloadHandlerScript : virtual UnityEngine::Networking::DownloadHandler, virtual System::IDisposable
		{
			DownloadHandlerScript(decltype(nullptr));
			DownloadHandlerScript(Plugin::InternalUse, int32_t handle);
			DownloadHandlerScript(const DownloadHandlerScript& other);
			DownloadHandlerScript(DownloadHandlerScript&& other);
			virtual ~DownloadHandlerScript();
			DownloadHandlerScript& operator=(const DownloadHandlerScript& other);
			DownloadHandlerScript& operator=(decltype(nullptr));
			DownloadHandlerScript& operator=(DownloadHandlerScript&& other);
			bool operator==(const DownloadHandlerScript& other) const;
			bool operator!=(const DownloadHandlerScript& other) const;
		};
	}
}

namespace CesiumForUnity
{
	struct RawDownloadedData
	{
		RawDownloadedData();
		void* pointer;
		System::Int32 length;
		explicit operator System::ValueType();
		explicit operator System::Object();
	};
}

namespace CesiumForUnity
{
	struct AbstractBaseNativeDownloadHandler : virtual UnityEngine::Networking::DownloadHandlerScript, virtual System::IDisposable
	{
		AbstractBaseNativeDownloadHandler(decltype(nullptr));
		AbstractBaseNativeDownloadHandler(Plugin::InternalUse, int32_t handle);
		AbstractBaseNativeDownloadHandler(const AbstractBaseNativeDownloadHandler& other);
		AbstractBaseNativeDownloadHandler(AbstractBaseNativeDownloadHandler&& other);
		virtual ~AbstractBaseNativeDownloadHandler();
		AbstractBaseNativeDownloadHandler& operator=(const AbstractBaseNativeDownloadHandler& other);
		AbstractBaseNativeDownloadHandler& operator=(decltype(nullptr));
		AbstractBaseNativeDownloadHandler& operator=(AbstractBaseNativeDownloadHandler&& other);
		bool operator==(const AbstractBaseNativeDownloadHandler& other) const;
		bool operator!=(const AbstractBaseNativeDownloadHandler& other) const;
	};
}

namespace CesiumForUnity
{
	struct BaseNativeDownloadHandler : virtual CesiumForUnity::AbstractBaseNativeDownloadHandler
	{
		BaseNativeDownloadHandler(decltype(nullptr));
		BaseNativeDownloadHandler(Plugin::InternalUse, int32_t handle);
		BaseNativeDownloadHandler(const BaseNativeDownloadHandler& other);
		BaseNativeDownloadHandler(BaseNativeDownloadHandler&& other);
		virtual ~BaseNativeDownloadHandler();
		BaseNativeDownloadHandler& operator=(const BaseNativeDownloadHandler& other);
		BaseNativeDownloadHandler& operator=(decltype(nullptr));
		BaseNativeDownloadHandler& operator=(BaseNativeDownloadHandler&& other);
		bool operator==(const BaseNativeDownloadHandler& other) const;
		bool operator!=(const BaseNativeDownloadHandler& other) const;
		int32_t CppHandle;
		BaseNativeDownloadHandler();
		virtual System::Boolean ReceiveDataNative(void* data, System::Int32 dataLength);
	};
}

namespace UnityEngine
{
	namespace Networking
	{
		struct UnityWebRequest : virtual System::IDisposable
		{
			UnityWebRequest(decltype(nullptr));
			UnityWebRequest(Plugin::InternalUse, int32_t handle);
			UnityWebRequest(const UnityWebRequest& other);
			UnityWebRequest(UnityWebRequest&& other);
			virtual ~UnityWebRequest();
			UnityWebRequest& operator=(const UnityWebRequest& other);
			UnityWebRequest& operator=(decltype(nullptr));
			UnityWebRequest& operator=(UnityWebRequest&& other);
			bool operator==(const UnityWebRequest& other) const;
			bool operator!=(const UnityWebRequest& other) const;
			System::String GetError();
			System::Boolean GetIsDone();
			System::Int64 GetResponseCode();
			System::String GetUrl();
			void SetUrl(System::String& value);
			System::String GetMethod();
			void SetMethod(System::String& value);
			UnityEngine::Networking::DownloadHandler GetDownloadHandler();
			void SetDownloadHandler(UnityEngine::Networking::DownloadHandler& value);
			static UnityEngine::Networking::UnityWebRequest Get(System::String& uri);
			virtual void SetRequestHeader(System::String& name, System::String& value);
			virtual UnityEngine::Networking::UnityWebRequestAsyncOperation SendWebRequest();
			virtual System::String GetResponseHeader(System::String& name);
		};
	}
}

namespace UnityEngine
{
	namespace Networking
	{
		struct UnityWebRequestAsyncOperation : virtual UnityEngine::AsyncOperation
		{
			UnityWebRequestAsyncOperation(decltype(nullptr));
			UnityWebRequestAsyncOperation(Plugin::InternalUse, int32_t handle);
			UnityWebRequestAsyncOperation(const UnityWebRequestAsyncOperation& other);
			UnityWebRequestAsyncOperation(UnityWebRequestAsyncOperation&& other);
			virtual ~UnityWebRequestAsyncOperation();
			UnityWebRequestAsyncOperation& operator=(const UnityWebRequestAsyncOperation& other);
			UnityWebRequestAsyncOperation& operator=(decltype(nullptr));
			UnityWebRequestAsyncOperation& operator=(UnityWebRequestAsyncOperation&& other);
			bool operator==(const UnityWebRequestAsyncOperation& other) const;
			bool operator!=(const UnityWebRequestAsyncOperation& other) const;
			void AddCompleted(System::Action_1<UnityEngine::AsyncOperation>& del);
			void RemoveCompleted(System::Action_1<UnityEngine::AsyncOperation>& del);
		};
	}
}

namespace System
{
	namespace Runtime
	{
		namespace InteropServices
		{
			namespace Marshal
			{
				void* StringToCoTaskMemUTF8(System::String& s);
				void FreeCoTaskMem(void* ptr);
			}
		}
	}
}

namespace CesiumForUnity
{
	struct AbstractBaseCesium3DTileset : virtual UnityEngine::MonoBehaviour
	{
		AbstractBaseCesium3DTileset(decltype(nullptr));
		AbstractBaseCesium3DTileset(Plugin::InternalUse, int32_t handle);
		AbstractBaseCesium3DTileset(const AbstractBaseCesium3DTileset& other);
		AbstractBaseCesium3DTileset(AbstractBaseCesium3DTileset&& other);
		virtual ~AbstractBaseCesium3DTileset();
		AbstractBaseCesium3DTileset& operator=(const AbstractBaseCesium3DTileset& other);
		AbstractBaseCesium3DTileset& operator=(decltype(nullptr));
		AbstractBaseCesium3DTileset& operator=(AbstractBaseCesium3DTileset&& other);
		bool operator==(const AbstractBaseCesium3DTileset& other) const;
		bool operator!=(const AbstractBaseCesium3DTileset& other) const;
	};
}

namespace CesiumForUnity
{
	struct BaseCesium3DTileset : virtual CesiumForUnity::AbstractBaseCesium3DTileset
	{
		BaseCesium3DTileset(decltype(nullptr));
		BaseCesium3DTileset(Plugin::InternalUse, int32_t handle);
		BaseCesium3DTileset(const BaseCesium3DTileset& other);
		BaseCesium3DTileset(BaseCesium3DTileset&& other);
		virtual ~BaseCesium3DTileset();
		BaseCesium3DTileset& operator=(const BaseCesium3DTileset& other);
		BaseCesium3DTileset& operator=(decltype(nullptr));
		BaseCesium3DTileset& operator=(BaseCesium3DTileset&& other);
		bool operator==(const BaseCesium3DTileset& other) const;
		bool operator!=(const BaseCesium3DTileset& other) const;
		int32_t CppHandle;
		BaseCesium3DTileset();
		virtual void Start();
		virtual void Update();
	};
}

namespace System
{
	struct IAsyncResult : virtual System::Object
	{
		IAsyncResult(decltype(nullptr));
		IAsyncResult(Plugin::InternalUse, int32_t handle);
		IAsyncResult(const IAsyncResult& other);
		IAsyncResult(IAsyncResult&& other);
		virtual ~IAsyncResult();
		IAsyncResult& operator=(const IAsyncResult& other);
		IAsyncResult& operator=(decltype(nullptr));
		IAsyncResult& operator=(IAsyncResult&& other);
		bool operator==(const IAsyncResult& other) const;
		bool operator!=(const IAsyncResult& other) const;
	};
}

namespace System
{
	namespace Threading
	{
		struct IThreadPoolWorkItem : virtual System::Object
		{
			IThreadPoolWorkItem(decltype(nullptr));
			IThreadPoolWorkItem(Plugin::InternalUse, int32_t handle);
			IThreadPoolWorkItem(const IThreadPoolWorkItem& other);
			IThreadPoolWorkItem(IThreadPoolWorkItem&& other);
			virtual ~IThreadPoolWorkItem();
			IThreadPoolWorkItem& operator=(const IThreadPoolWorkItem& other);
			IThreadPoolWorkItem& operator=(decltype(nullptr));
			IThreadPoolWorkItem& operator=(IThreadPoolWorkItem&& other);
			bool operator==(const IThreadPoolWorkItem& other) const;
			bool operator!=(const IThreadPoolWorkItem& other) const;
		};
	}
}

namespace System
{
	namespace Threading
	{
		namespace Tasks
		{
			struct Task : virtual System::IAsyncResult, virtual System::IDisposable, virtual System::Threading::IThreadPoolWorkItem
			{
				Task(decltype(nullptr));
				Task(Plugin::InternalUse, int32_t handle);
				Task(const Task& other);
				Task(Task&& other);
				virtual ~Task();
				Task& operator=(const Task& other);
				Task& operator=(decltype(nullptr));
				Task& operator=(Task&& other);
				bool operator==(const Task& other) const;
				bool operator!=(const Task& other) const;
				static System::Threading::Tasks::Task Run(System::Action& action);
			};
		}
	}
}

namespace System
{
	struct Action : virtual System::Object
	{
		Action(decltype(nullptr));
		Action(Plugin::InternalUse, int32_t handle);
		Action(const Action& other);
		Action(Action&& other);
		virtual ~Action();
		Action& operator=(const Action& other);
		Action& operator=(decltype(nullptr));
		Action& operator=(Action&& other);
		bool operator==(const Action& other) const;
		bool operator!=(const Action& other) const;
		int32_t CppHandle;
		int32_t ClassHandle;
		Action();
		void operator+=(System::Action& del);
		void operator-=(System::Action& del);
		virtual void operator()();
		void Invoke();
	};
}

namespace System
{
	template<> struct Action_1<UnityEngine::AsyncOperation> : virtual System::Object
	{
		Action_1(decltype(nullptr));
		Action_1(Plugin::InternalUse, int32_t handle);
		Action_1(const Action_1<UnityEngine::AsyncOperation>& other);
		Action_1(Action_1<UnityEngine::AsyncOperation>&& other);
		virtual ~Action_1();
		Action_1<UnityEngine::AsyncOperation>& operator=(const Action_1<UnityEngine::AsyncOperation>& other);
		Action_1<UnityEngine::AsyncOperation>& operator=(decltype(nullptr));
		Action_1<UnityEngine::AsyncOperation>& operator=(Action_1<UnityEngine::AsyncOperation>&& other);
		bool operator==(const Action_1<UnityEngine::AsyncOperation>& other) const;
		bool operator!=(const Action_1<UnityEngine::AsyncOperation>& other) const;
		int32_t CppHandle;
		int32_t ClassHandle;
		Action_1();
		void operator+=(System::Action_1<UnityEngine::AsyncOperation>& del);
		void operator-=(System::Action_1<UnityEngine::AsyncOperation>& del);
		virtual void operator()(UnityEngine::AsyncOperation& obj);
		void Invoke(UnityEngine::AsyncOperation& obj);
	};
}
/*END TYPE DEFINITIONS*/

/*BEGIN MACROS*/
#define CESIUM_FOR_UNITY_NATIVE_DOWNLOAD_HANDLER_DEFAULT_CONSTRUCTOR_DECLARATION \
	NativeDownloadHandler(Plugin::InternalUse iu, int32_t handle);
	
#define CESIUM_FOR_UNITY_NATIVE_DOWNLOAD_HANDLER_DEFAULT_CONSTRUCTOR_DEFINITION \
	NativeDownloadHandler::NativeDownloadHandler(Plugin::InternalUse iu, int32_t handle) \
		: System::IDisposable(nullptr) \
		, UnityEngine::Networking::DownloadHandler(nullptr) \
		, UnityEngine::Networking::DownloadHandlerScript(nullptr) \
		, CesiumForUnity::AbstractBaseNativeDownloadHandler(nullptr) \
		, CesiumForUnity::BaseNativeDownloadHandler(iu, handle) \
	{ \
	}
	
#define CESIUM_FOR_UNITY_NATIVE_DOWNLOAD_HANDLER_DEFAULT_CONSTRUCTOR \
	NativeDownloadHandler(Plugin::InternalUse iu, int32_t handle) \
		: System::IDisposable(nullptr) \
		, UnityEngine::Networking::DownloadHandler(nullptr) \
		, UnityEngine::Networking::DownloadHandlerScript(nullptr) \
		, CesiumForUnity::AbstractBaseNativeDownloadHandler(nullptr) \
		, CesiumForUnity::BaseNativeDownloadHandler(iu, handle) \
	{ \
	}
	
#define CESIUM_FOR_UNITY_NATIVE_DOWNLOAD_HANDLER_DEFAULT_CONTENTS_DECLARATION \
	void* operator new(size_t, void* p) noexcept; \
	void operator delete(void*, size_t) noexcept; \
	
#define CESIUM_FOR_UNITY_NATIVE_DOWNLOAD_HANDLER_DEFAULT_CONTENTS_DEFINITION \
	void* NativeDownloadHandler::operator new(size_t, void* p) noexcept\
	{ \
		return p; \
	} \
	void NativeDownloadHandler::operator delete(void*, size_t) noexcept \
	{ \
	}
	
#define CESIUM_FOR_UNITY_NATIVE_DOWNLOAD_HANDLER_DEFAULT_CONTENTS\
	void* operator new(size_t, void* p) noexcept \
	{ \
		return p; \
	} \
	void operator delete(void*, size_t) noexcept \
	{ \
	}
	
#define CESIUM_FOR_UNITY_CESIUM3DTILESET_DEFAULT_CONSTRUCTOR_DECLARATION \
	Cesium3DTileset(Plugin::InternalUse iu, int32_t handle);
	
#define CESIUM_FOR_UNITY_CESIUM3DTILESET_DEFAULT_CONSTRUCTOR_DEFINITION \
	Cesium3DTileset::Cesium3DTileset(Plugin::InternalUse iu, int32_t handle) \
		: UnityEngine::Object(nullptr) \
		, UnityEngine::Component(nullptr) \
		, UnityEngine::Behaviour(nullptr) \
		, UnityEngine::MonoBehaviour(nullptr) \
		, CesiumForUnity::AbstractBaseCesium3DTileset(nullptr) \
		, CesiumForUnity::BaseCesium3DTileset(iu, handle) \
	{ \
	}
	
#define CESIUM_FOR_UNITY_CESIUM3DTILESET_DEFAULT_CONSTRUCTOR \
	Cesium3DTileset(Plugin::InternalUse iu, int32_t handle) \
		: UnityEngine::Object(nullptr) \
		, UnityEngine::Component(nullptr) \
		, UnityEngine::Behaviour(nullptr) \
		, UnityEngine::MonoBehaviour(nullptr) \
		, CesiumForUnity::AbstractBaseCesium3DTileset(nullptr) \
		, CesiumForUnity::BaseCesium3DTileset(iu, handle) \
	{ \
	}
	
#define CESIUM_FOR_UNITY_CESIUM3DTILESET_DEFAULT_CONTENTS_DECLARATION \
	void* operator new(size_t, void* p) noexcept; \
	void operator delete(void*, size_t) noexcept; \
	
#define CESIUM_FOR_UNITY_CESIUM3DTILESET_DEFAULT_CONTENTS_DEFINITION \
	void* Cesium3DTileset::operator new(size_t, void* p) noexcept\
	{ \
		return p; \
	} \
	void Cesium3DTileset::operator delete(void*, size_t) noexcept \
	{ \
	}
	
#define CESIUM_FOR_UNITY_CESIUM3DTILESET_DEFAULT_CONTENTS\
	void* operator new(size_t, void* p) noexcept \
	{ \
		return p; \
	} \
	void operator delete(void*, size_t) noexcept \
	{ \
	}
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
