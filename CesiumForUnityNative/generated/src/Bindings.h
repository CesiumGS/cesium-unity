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

	using Void = void;

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
	namespace Collections
	{
		namespace Generic
		{
			template<typename TT0> struct IEnumerator_1;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<typename TT0> struct IEnumerable_1;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<typename TT0> struct ICollection_1;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<typename TT0> struct IReadOnlyCollection_1;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<typename TT0> struct IList_1;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<typename TT0> struct IReadOnlyList_1;
		}
	}
}

namespace Unity
{
	namespace Collections
	{
		template<typename TT0> struct NativeArray_1;
	}
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
	namespace Collections
	{
		struct IStructuralEquatable;
	}
}

namespace System
{
	namespace Collections
	{
		struct IStructuralComparable;
	}
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
	struct Vector2;
}

namespace UnityEngine
{
	struct Vector3;
}

namespace UnityEngine
{
	struct Vector4;
}

namespace UnityEngine
{
	struct Quaternion;
}

namespace UnityEngine
{
	struct Matrix4x4;
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

namespace Unity
{
	namespace Collections
	{
		struct NativeArrayOptions;
	}
}

namespace Unity
{
	namespace Collections
	{
		struct Allocator;
	}
}

namespace Unity
{
	namespace Collections
	{
		namespace LowLevel
		{
			namespace Unsafe
			{
				namespace NativeArrayUnsafeUtility
				{
				}
			}
		}
	}
}

namespace UnityEngine
{
	struct MeshTopology;
}

namespace UnityEngine
{
	struct Mesh;
}

namespace UnityEngine
{
	struct MeshFilter;
}

namespace UnityEngine
{
	struct Renderer;
}

namespace UnityEngine
{
	struct MeshRenderer;
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

namespace UnityEngine
{
	struct Material;
}

namespace UnityEngine
{
	struct Resources;
}

namespace UnityEngine
{
	struct ScriptableObject;
}

namespace UnityEditor
{
	struct EditorWindow;
}

namespace UnityEditor
{
	struct SearchableEditorWindow;
}

namespace UnityEditor
{
	struct IHasCustomMenu;
}

namespace UnityEditor
{
	namespace Overlays
	{
		struct ISupportsOverlays;
	}
}

namespace UnityEditor
{
	struct SceneView;
}

namespace UnityEngine
{
	struct Texture;
}

namespace UnityEngine
{
	struct TextureFormat;
}

namespace UnityEngine
{
	struct Texture2D;
}

namespace UnityEngine
{
	struct Application;
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
	template<> struct IEquatable_1<UnityEngine::Vector2>;
}

namespace System
{
	template<> struct IEquatable_1<UnityEngine::Vector3>;
}

namespace System
{
	template<> struct IEquatable_1<UnityEngine::Vector4>;
}

namespace System
{
	template<> struct IEquatable_1<UnityEngine::Quaternion>;
}

namespace System
{
	template<> struct IEquatable_1<UnityEngine::Matrix4x4>;
}

namespace System
{
	template<> struct IEquatable_1<Unity::Collections::NativeArray_1<System::Byte>>;
}

namespace System
{
	template<> struct IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector3>>;
}

namespace System
{
	template<> struct IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector2>>;
}

namespace System
{
	template<> struct IEquatable_1<Unity::Collections::NativeArray_1<System::Int32>>;
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
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IEnumerator_1<UnityEngine::Vector2>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IEnumerator_1<UnityEngine::Vector3>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IEnumerator_1<System::Byte>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IEnumerator_1<System::Int32>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IEnumerable_1<UnityEngine::Vector2>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IEnumerable_1<UnityEngine::Vector3>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IEnumerable_1<System::Byte>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IEnumerable_1<System::Int32>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct ICollection_1<UnityEngine::Vector2>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct ICollection_1<UnityEngine::Vector3>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct ICollection_1<System::Byte>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct ICollection_1<System::Int32>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IReadOnlyCollection_1<UnityEngine::Vector2>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IReadOnlyCollection_1<UnityEngine::Vector3>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IReadOnlyCollection_1<System::Int32>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IReadOnlyCollection_1<System::Byte>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IList_1<UnityEngine::Vector2>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IList_1<UnityEngine::Vector3>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IList_1<System::Byte>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IList_1<System::Int32>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IReadOnlyList_1<UnityEngine::Vector2>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IReadOnlyList_1<UnityEngine::Vector3>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IReadOnlyList_1<System::Byte>;
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IReadOnlyList_1<System::Int32>;
		}
	}
}

namespace Unity
{
	namespace Collections
	{
		template<> struct NativeArray_1<System::Byte>;
	}
}

namespace Unity
{
	namespace Collections
	{
		template<> struct NativeArray_1<UnityEngine::Vector3>;
	}
}

namespace Unity
{
	namespace Collections
	{
		template<> struct NativeArray_1<UnityEngine::Vector2>;
	}
}

namespace Unity
{
	namespace Collections
	{
		template<> struct NativeArray_1<System::Int32>;
	}
}

namespace Plugin
{
	template<> struct ArrayElementProxy1_1<UnityEngine::Vector2>;
}

namespace System
{
	template<> struct Array1<UnityEngine::Vector2>;
}

namespace Plugin
{
	template<> struct ArrayElementProxy1_1<UnityEngine::Vector3>;
}

namespace System
{
	template<> struct Array1<UnityEngine::Vector3>;
}

namespace Plugin
{
	template<> struct ArrayElementProxy1_1<System::Byte>;
}

namespace System
{
	template<> struct Array1<System::Byte>;
}

namespace Plugin
{
	template<> struct ArrayElementProxy1_1<System::Int32>;
}

namespace System
{
	template<> struct Array1<System::Int32>;
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
		explicit operator UnityEngine::Vector2();
		explicit operator UnityEngine::Vector3();
		explicit operator UnityEngine::Vector4();
		explicit operator UnityEngine::Quaternion();
		explicit operator UnityEngine::Matrix4x4();
		explicit operator Unity::Collections::NativeArrayOptions();
		explicit operator Unity::Collections::NativeArray_1<System::Byte>();
		explicit operator Unity::Collections::NativeArray_1<UnityEngine::Vector3>();
		explicit operator Unity::Collections::NativeArray_1<UnityEngine::Vector2>();
		explicit operator Unity::Collections::NativeArray_1<System::Int32>();
		explicit operator Unity::Collections::Allocator();
		explicit operator UnityEngine::MeshTopology();
		explicit operator UnityEngine::PrimitiveType();
		explicit operator CesiumForUnity::RawDownloadedData();
		explicit operator UnityEngine::TextureFormat();
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
		virtual void Dispose();
	};
}

namespace System
{
	namespace Collections
	{
		struct IStructuralEquatable : virtual System::Object
		{
			IStructuralEquatable(decltype(nullptr));
			IStructuralEquatable(Plugin::InternalUse, int32_t handle);
			IStructuralEquatable(const IStructuralEquatable& other);
			IStructuralEquatable(IStructuralEquatable&& other);
			virtual ~IStructuralEquatable();
			IStructuralEquatable& operator=(const IStructuralEquatable& other);
			IStructuralEquatable& operator=(decltype(nullptr));
			IStructuralEquatable& operator=(IStructuralEquatable&& other);
			bool operator==(const IStructuralEquatable& other) const;
			bool operator!=(const IStructuralEquatable& other) const;
		};
	}
}

namespace System
{
	namespace Collections
	{
		struct IStructuralComparable : virtual System::Object
		{
			IStructuralComparable(decltype(nullptr));
			IStructuralComparable(Plugin::InternalUse, int32_t handle);
			IStructuralComparable(const IStructuralComparable& other);
			IStructuralComparable(IStructuralComparable&& other);
			virtual ~IStructuralComparable();
			IStructuralComparable& operator=(const IStructuralComparable& other);
			IStructuralComparable& operator=(decltype(nullptr));
			IStructuralComparable& operator=(IStructuralComparable&& other);
			bool operator==(const IStructuralComparable& other) const;
			bool operator!=(const IStructuralComparable& other) const;
		};
	}
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
	template<> struct IEquatable_1<UnityEngine::Vector2> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<UnityEngine::Vector2>& other);
		IEquatable_1(IEquatable_1<UnityEngine::Vector2>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<UnityEngine::Vector2>& operator=(const IEquatable_1<UnityEngine::Vector2>& other);
		IEquatable_1<UnityEngine::Vector2>& operator=(decltype(nullptr));
		IEquatable_1<UnityEngine::Vector2>& operator=(IEquatable_1<UnityEngine::Vector2>&& other);
		bool operator==(const IEquatable_1<UnityEngine::Vector2>& other) const;
		bool operator!=(const IEquatable_1<UnityEngine::Vector2>& other) const;
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
	template<> struct IEquatable_1<UnityEngine::Vector4> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<UnityEngine::Vector4>& other);
		IEquatable_1(IEquatable_1<UnityEngine::Vector4>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<UnityEngine::Vector4>& operator=(const IEquatable_1<UnityEngine::Vector4>& other);
		IEquatable_1<UnityEngine::Vector4>& operator=(decltype(nullptr));
		IEquatable_1<UnityEngine::Vector4>& operator=(IEquatable_1<UnityEngine::Vector4>&& other);
		bool operator==(const IEquatable_1<UnityEngine::Vector4>& other) const;
		bool operator!=(const IEquatable_1<UnityEngine::Vector4>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<UnityEngine::Quaternion> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<UnityEngine::Quaternion>& other);
		IEquatable_1(IEquatable_1<UnityEngine::Quaternion>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<UnityEngine::Quaternion>& operator=(const IEquatable_1<UnityEngine::Quaternion>& other);
		IEquatable_1<UnityEngine::Quaternion>& operator=(decltype(nullptr));
		IEquatable_1<UnityEngine::Quaternion>& operator=(IEquatable_1<UnityEngine::Quaternion>&& other);
		bool operator==(const IEquatable_1<UnityEngine::Quaternion>& other) const;
		bool operator!=(const IEquatable_1<UnityEngine::Quaternion>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<UnityEngine::Matrix4x4> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<UnityEngine::Matrix4x4>& other);
		IEquatable_1(IEquatable_1<UnityEngine::Matrix4x4>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<UnityEngine::Matrix4x4>& operator=(const IEquatable_1<UnityEngine::Matrix4x4>& other);
		IEquatable_1<UnityEngine::Matrix4x4>& operator=(decltype(nullptr));
		IEquatable_1<UnityEngine::Matrix4x4>& operator=(IEquatable_1<UnityEngine::Matrix4x4>&& other);
		bool operator==(const IEquatable_1<UnityEngine::Matrix4x4>& other) const;
		bool operator!=(const IEquatable_1<UnityEngine::Matrix4x4>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<Unity::Collections::NativeArray_1<System::Byte>> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<Unity::Collections::NativeArray_1<System::Byte>>& other);
		IEquatable_1(IEquatable_1<Unity::Collections::NativeArray_1<System::Byte>>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<Unity::Collections::NativeArray_1<System::Byte>>& operator=(const IEquatable_1<Unity::Collections::NativeArray_1<System::Byte>>& other);
		IEquatable_1<Unity::Collections::NativeArray_1<System::Byte>>& operator=(decltype(nullptr));
		IEquatable_1<Unity::Collections::NativeArray_1<System::Byte>>& operator=(IEquatable_1<Unity::Collections::NativeArray_1<System::Byte>>&& other);
		bool operator==(const IEquatable_1<Unity::Collections::NativeArray_1<System::Byte>>& other) const;
		bool operator!=(const IEquatable_1<Unity::Collections::NativeArray_1<System::Byte>>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector3>> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector3>>& other);
		IEquatable_1(IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector3>>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector3>>& operator=(const IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector3>>& other);
		IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector3>>& operator=(decltype(nullptr));
		IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector3>>& operator=(IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector3>>&& other);
		bool operator==(const IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector3>>& other) const;
		bool operator!=(const IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector3>>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector2>> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector2>>& other);
		IEquatable_1(IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector2>>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector2>>& operator=(const IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector2>>& other);
		IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector2>>& operator=(decltype(nullptr));
		IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector2>>& operator=(IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector2>>&& other);
		bool operator==(const IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector2>>& other) const;
		bool operator!=(const IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector2>>& other) const;
	};
}

namespace System
{
	template<> struct IEquatable_1<Unity::Collections::NativeArray_1<System::Int32>> : virtual System::Object
	{
		IEquatable_1(decltype(nullptr));
		IEquatable_1(Plugin::InternalUse, int32_t handle);
		IEquatable_1(const IEquatable_1<Unity::Collections::NativeArray_1<System::Int32>>& other);
		IEquatable_1(IEquatable_1<Unity::Collections::NativeArray_1<System::Int32>>&& other);
		virtual ~IEquatable_1();
		IEquatable_1<Unity::Collections::NativeArray_1<System::Int32>>& operator=(const IEquatable_1<Unity::Collections::NativeArray_1<System::Int32>>& other);
		IEquatable_1<Unity::Collections::NativeArray_1<System::Int32>>& operator=(decltype(nullptr));
		IEquatable_1<Unity::Collections::NativeArray_1<System::Int32>>& operator=(IEquatable_1<Unity::Collections::NativeArray_1<System::Int32>>&& other);
		bool operator==(const IEquatable_1<Unity::Collections::NativeArray_1<System::Int32>>& other) const;
		bool operator!=(const IEquatable_1<Unity::Collections::NativeArray_1<System::Int32>>& other) const;
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
	struct Vector2
	{
		Vector2();
		Vector2(System::Single x, System::Single y);
		System::Single x;
		System::Single y;
		explicit operator System::ValueType();
		explicit operator System::Object();
		explicit operator System::IFormattable();
		explicit operator System::IEquatable_1<UnityEngine::Vector2>();
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
		explicit operator System::ValueType();
		explicit operator System::Object();
		explicit operator System::IFormattable();
		explicit operator System::IEquatable_1<UnityEngine::Vector3>();
	};
}

namespace UnityEngine
{
	struct Vector4
	{
		Vector4();
		Vector4(System::Single x, System::Single y, System::Single z, System::Single w);
		System::Single x;
		System::Single y;
		System::Single z;
		System::Single w;
		explicit operator System::ValueType();
		explicit operator System::Object();
		explicit operator System::IFormattable();
		explicit operator System::IEquatable_1<UnityEngine::Vector4>();
	};
}

namespace UnityEngine
{
	struct Quaternion
	{
		Quaternion();
		Quaternion(System::Single x, System::Single y, System::Single z, System::Single w);
		System::Single x;
		System::Single y;
		System::Single z;
		System::Single w;
		explicit operator System::ValueType();
		explicit operator System::Object();
		explicit operator System::IFormattable();
		explicit operator System::IEquatable_1<UnityEngine::Quaternion>();
	};
}

namespace UnityEngine
{
	struct Matrix4x4
	{
		Matrix4x4();
		Matrix4x4(UnityEngine::Vector4& column0, UnityEngine::Vector4& column1, UnityEngine::Vector4& column2, UnityEngine::Vector4& column3);
		UnityEngine::Quaternion GetRotation();
		UnityEngine::Vector3 GetLossyScale();
		System::Single m00;
		System::Single m10;
		System::Single m20;
		System::Single m30;
		System::Single m01;
		System::Single m11;
		System::Single m21;
		System::Single m31;
		System::Single m02;
		System::Single m12;
		System::Single m22;
		System::Single m32;
		System::Single m03;
		System::Single m13;
		System::Single m23;
		System::Single m33;
		UnityEngine::Vector3 GetPosition();
		explicit operator System::ValueType();
		explicit operator System::Object();
		explicit operator System::IFormattable();
		explicit operator System::IEquatable_1<UnityEngine::Matrix4x4>();
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
		UnityEngine::Quaternion GetRotation();
		void SetRotation(UnityEngine::Quaternion& value);
		UnityEngine::Vector3 GetLocalScale();
		void SetLocalScale(UnityEngine::Vector3& value);
		UnityEngine::Matrix4x4 GetLocalToWorldMatrix();
		UnityEngine::Transform GetParent();
		void SetParent(UnityEngine::Transform& value);
		UnityEngine::Vector3 GetForward();
		void SetForward(UnityEngine::Vector3& value);
		UnityEngine::Vector3 GetUp();
		void SetUp(UnityEngine::Vector3& value);
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
		GameObject(System::String& name);
		UnityEngine::Transform GetTransform();
		template<typename MT0> MT0 AddComponent();
		static UnityEngine::GameObject CreatePrimitive(UnityEngine::PrimitiveType type);
		virtual void SetActive(System::Boolean value);
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
		UnityEngine::GameObject GetGameObject();
	};
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IEnumerator_1<UnityEngine::Vector2> : virtual System::IDisposable, virtual System::Collections::IEnumerator
			{
				IEnumerator_1(decltype(nullptr));
				IEnumerator_1(Plugin::InternalUse, int32_t handle);
				IEnumerator_1(const IEnumerator_1<UnityEngine::Vector2>& other);
				IEnumerator_1(IEnumerator_1<UnityEngine::Vector2>&& other);
				virtual ~IEnumerator_1();
				IEnumerator_1<UnityEngine::Vector2>& operator=(const IEnumerator_1<UnityEngine::Vector2>& other);
				IEnumerator_1<UnityEngine::Vector2>& operator=(decltype(nullptr));
				IEnumerator_1<UnityEngine::Vector2>& operator=(IEnumerator_1<UnityEngine::Vector2>&& other);
				bool operator==(const IEnumerator_1<UnityEngine::Vector2>& other) const;
				bool operator!=(const IEnumerator_1<UnityEngine::Vector2>& other) const;
				UnityEngine::Vector2 GetCurrent();
			};
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IEnumerator_1<UnityEngine::Vector3> : virtual System::IDisposable, virtual System::Collections::IEnumerator
			{
				IEnumerator_1(decltype(nullptr));
				IEnumerator_1(Plugin::InternalUse, int32_t handle);
				IEnumerator_1(const IEnumerator_1<UnityEngine::Vector3>& other);
				IEnumerator_1(IEnumerator_1<UnityEngine::Vector3>&& other);
				virtual ~IEnumerator_1();
				IEnumerator_1<UnityEngine::Vector3>& operator=(const IEnumerator_1<UnityEngine::Vector3>& other);
				IEnumerator_1<UnityEngine::Vector3>& operator=(decltype(nullptr));
				IEnumerator_1<UnityEngine::Vector3>& operator=(IEnumerator_1<UnityEngine::Vector3>&& other);
				bool operator==(const IEnumerator_1<UnityEngine::Vector3>& other) const;
				bool operator!=(const IEnumerator_1<UnityEngine::Vector3>& other) const;
				UnityEngine::Vector3 GetCurrent();
			};
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IEnumerator_1<System::Byte> : virtual System::IDisposable, virtual System::Collections::IEnumerator
			{
				IEnumerator_1(decltype(nullptr));
				IEnumerator_1(Plugin::InternalUse, int32_t handle);
				IEnumerator_1(const IEnumerator_1<System::Byte>& other);
				IEnumerator_1(IEnumerator_1<System::Byte>&& other);
				virtual ~IEnumerator_1();
				IEnumerator_1<System::Byte>& operator=(const IEnumerator_1<System::Byte>& other);
				IEnumerator_1<System::Byte>& operator=(decltype(nullptr));
				IEnumerator_1<System::Byte>& operator=(IEnumerator_1<System::Byte>&& other);
				bool operator==(const IEnumerator_1<System::Byte>& other) const;
				bool operator!=(const IEnumerator_1<System::Byte>& other) const;
				System::Byte GetCurrent();
			};
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IEnumerator_1<System::Int32> : virtual System::IDisposable, virtual System::Collections::IEnumerator
			{
				IEnumerator_1(decltype(nullptr));
				IEnumerator_1(Plugin::InternalUse, int32_t handle);
				IEnumerator_1(const IEnumerator_1<System::Int32>& other);
				IEnumerator_1(IEnumerator_1<System::Int32>&& other);
				virtual ~IEnumerator_1();
				IEnumerator_1<System::Int32>& operator=(const IEnumerator_1<System::Int32>& other);
				IEnumerator_1<System::Int32>& operator=(decltype(nullptr));
				IEnumerator_1<System::Int32>& operator=(IEnumerator_1<System::Int32>&& other);
				bool operator==(const IEnumerator_1<System::Int32>& other) const;
				bool operator!=(const IEnumerator_1<System::Int32>& other) const;
				System::Int32 GetCurrent();
			};
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IEnumerable_1<UnityEngine::Vector2> : virtual System::Collections::IEnumerable
			{
				IEnumerable_1(decltype(nullptr));
				IEnumerable_1(Plugin::InternalUse, int32_t handle);
				IEnumerable_1(const IEnumerable_1<UnityEngine::Vector2>& other);
				IEnumerable_1(IEnumerable_1<UnityEngine::Vector2>&& other);
				virtual ~IEnumerable_1();
				IEnumerable_1<UnityEngine::Vector2>& operator=(const IEnumerable_1<UnityEngine::Vector2>& other);
				IEnumerable_1<UnityEngine::Vector2>& operator=(decltype(nullptr));
				IEnumerable_1<UnityEngine::Vector2>& operator=(IEnumerable_1<UnityEngine::Vector2>&& other);
				bool operator==(const IEnumerable_1<UnityEngine::Vector2>& other) const;
				bool operator!=(const IEnumerable_1<UnityEngine::Vector2>& other) const;
				virtual System::Collections::Generic::IEnumerator_1<UnityEngine::Vector2> GetEnumerator();
			};
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IEnumerable_1<UnityEngine::Vector3> : virtual System::Collections::IEnumerable
			{
				IEnumerable_1(decltype(nullptr));
				IEnumerable_1(Plugin::InternalUse, int32_t handle);
				IEnumerable_1(const IEnumerable_1<UnityEngine::Vector3>& other);
				IEnumerable_1(IEnumerable_1<UnityEngine::Vector3>&& other);
				virtual ~IEnumerable_1();
				IEnumerable_1<UnityEngine::Vector3>& operator=(const IEnumerable_1<UnityEngine::Vector3>& other);
				IEnumerable_1<UnityEngine::Vector3>& operator=(decltype(nullptr));
				IEnumerable_1<UnityEngine::Vector3>& operator=(IEnumerable_1<UnityEngine::Vector3>&& other);
				bool operator==(const IEnumerable_1<UnityEngine::Vector3>& other) const;
				bool operator!=(const IEnumerable_1<UnityEngine::Vector3>& other) const;
				virtual System::Collections::Generic::IEnumerator_1<UnityEngine::Vector3> GetEnumerator();
			};
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IEnumerable_1<System::Byte> : virtual System::Collections::IEnumerable
			{
				IEnumerable_1(decltype(nullptr));
				IEnumerable_1(Plugin::InternalUse, int32_t handle);
				IEnumerable_1(const IEnumerable_1<System::Byte>& other);
				IEnumerable_1(IEnumerable_1<System::Byte>&& other);
				virtual ~IEnumerable_1();
				IEnumerable_1<System::Byte>& operator=(const IEnumerable_1<System::Byte>& other);
				IEnumerable_1<System::Byte>& operator=(decltype(nullptr));
				IEnumerable_1<System::Byte>& operator=(IEnumerable_1<System::Byte>&& other);
				bool operator==(const IEnumerable_1<System::Byte>& other) const;
				bool operator!=(const IEnumerable_1<System::Byte>& other) const;
				virtual System::Collections::Generic::IEnumerator_1<System::Byte> GetEnumerator();
			};
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IEnumerable_1<System::Int32> : virtual System::Collections::IEnumerable
			{
				IEnumerable_1(decltype(nullptr));
				IEnumerable_1(Plugin::InternalUse, int32_t handle);
				IEnumerable_1(const IEnumerable_1<System::Int32>& other);
				IEnumerable_1(IEnumerable_1<System::Int32>&& other);
				virtual ~IEnumerable_1();
				IEnumerable_1<System::Int32>& operator=(const IEnumerable_1<System::Int32>& other);
				IEnumerable_1<System::Int32>& operator=(decltype(nullptr));
				IEnumerable_1<System::Int32>& operator=(IEnumerable_1<System::Int32>&& other);
				bool operator==(const IEnumerable_1<System::Int32>& other) const;
				bool operator!=(const IEnumerable_1<System::Int32>& other) const;
				virtual System::Collections::Generic::IEnumerator_1<System::Int32> GetEnumerator();
			};
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct ICollection_1<UnityEngine::Vector2> : virtual System::Collections::Generic::IEnumerable_1<UnityEngine::Vector2>
			{
				ICollection_1(decltype(nullptr));
				ICollection_1(Plugin::InternalUse, int32_t handle);
				ICollection_1(const ICollection_1<UnityEngine::Vector2>& other);
				ICollection_1(ICollection_1<UnityEngine::Vector2>&& other);
				virtual ~ICollection_1();
				ICollection_1<UnityEngine::Vector2>& operator=(const ICollection_1<UnityEngine::Vector2>& other);
				ICollection_1<UnityEngine::Vector2>& operator=(decltype(nullptr));
				ICollection_1<UnityEngine::Vector2>& operator=(ICollection_1<UnityEngine::Vector2>&& other);
				bool operator==(const ICollection_1<UnityEngine::Vector2>& other) const;
				bool operator!=(const ICollection_1<UnityEngine::Vector2>& other) const;
			};
		}
	}
}

namespace Plugin
{
	struct SystemCollectionsGenericICollectionUnityEngineVector2Iterator
	{
		System::Collections::Generic::IEnumerator_1<UnityEngine::Vector2> enumerator;
		bool hasMore;
		SystemCollectionsGenericICollectionUnityEngineVector2Iterator(decltype(nullptr));
		SystemCollectionsGenericICollectionUnityEngineVector2Iterator(System::Collections::Generic::ICollection_1<UnityEngine::Vector2>& enumerable);
		~SystemCollectionsGenericICollectionUnityEngineVector2Iterator();
		SystemCollectionsGenericICollectionUnityEngineVector2Iterator& operator++();
		bool operator!=(const SystemCollectionsGenericICollectionUnityEngineVector2Iterator& other);
		UnityEngine::Vector2 operator*();
	};
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericICollectionUnityEngineVector2Iterator begin(System::Collections::Generic::ICollection_1<UnityEngine::Vector2>& enumerable);
			Plugin::SystemCollectionsGenericICollectionUnityEngineVector2Iterator end(System::Collections::Generic::ICollection_1<UnityEngine::Vector2>& enumerable);
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct ICollection_1<UnityEngine::Vector3> : virtual System::Collections::Generic::IEnumerable_1<UnityEngine::Vector3>
			{
				ICollection_1(decltype(nullptr));
				ICollection_1(Plugin::InternalUse, int32_t handle);
				ICollection_1(const ICollection_1<UnityEngine::Vector3>& other);
				ICollection_1(ICollection_1<UnityEngine::Vector3>&& other);
				virtual ~ICollection_1();
				ICollection_1<UnityEngine::Vector3>& operator=(const ICollection_1<UnityEngine::Vector3>& other);
				ICollection_1<UnityEngine::Vector3>& operator=(decltype(nullptr));
				ICollection_1<UnityEngine::Vector3>& operator=(ICollection_1<UnityEngine::Vector3>&& other);
				bool operator==(const ICollection_1<UnityEngine::Vector3>& other) const;
				bool operator!=(const ICollection_1<UnityEngine::Vector3>& other) const;
			};
		}
	}
}

namespace Plugin
{
	struct SystemCollectionsGenericICollectionUnityEngineVector3Iterator
	{
		System::Collections::Generic::IEnumerator_1<UnityEngine::Vector3> enumerator;
		bool hasMore;
		SystemCollectionsGenericICollectionUnityEngineVector3Iterator(decltype(nullptr));
		SystemCollectionsGenericICollectionUnityEngineVector3Iterator(System::Collections::Generic::ICollection_1<UnityEngine::Vector3>& enumerable);
		~SystemCollectionsGenericICollectionUnityEngineVector3Iterator();
		SystemCollectionsGenericICollectionUnityEngineVector3Iterator& operator++();
		bool operator!=(const SystemCollectionsGenericICollectionUnityEngineVector3Iterator& other);
		UnityEngine::Vector3 operator*();
	};
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericICollectionUnityEngineVector3Iterator begin(System::Collections::Generic::ICollection_1<UnityEngine::Vector3>& enumerable);
			Plugin::SystemCollectionsGenericICollectionUnityEngineVector3Iterator end(System::Collections::Generic::ICollection_1<UnityEngine::Vector3>& enumerable);
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct ICollection_1<System::Byte> : virtual System::Collections::Generic::IEnumerable_1<System::Byte>
			{
				ICollection_1(decltype(nullptr));
				ICollection_1(Plugin::InternalUse, int32_t handle);
				ICollection_1(const ICollection_1<System::Byte>& other);
				ICollection_1(ICollection_1<System::Byte>&& other);
				virtual ~ICollection_1();
				ICollection_1<System::Byte>& operator=(const ICollection_1<System::Byte>& other);
				ICollection_1<System::Byte>& operator=(decltype(nullptr));
				ICollection_1<System::Byte>& operator=(ICollection_1<System::Byte>&& other);
				bool operator==(const ICollection_1<System::Byte>& other) const;
				bool operator!=(const ICollection_1<System::Byte>& other) const;
			};
		}
	}
}

namespace Plugin
{
	struct SystemCollectionsGenericICollectionSystemByteIterator
	{
		System::Collections::Generic::IEnumerator_1<System::Byte> enumerator;
		bool hasMore;
		SystemCollectionsGenericICollectionSystemByteIterator(decltype(nullptr));
		SystemCollectionsGenericICollectionSystemByteIterator(System::Collections::Generic::ICollection_1<System::Byte>& enumerable);
		~SystemCollectionsGenericICollectionSystemByteIterator();
		SystemCollectionsGenericICollectionSystemByteIterator& operator++();
		bool operator!=(const SystemCollectionsGenericICollectionSystemByteIterator& other);
		System::Byte operator*();
	};
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericICollectionSystemByteIterator begin(System::Collections::Generic::ICollection_1<System::Byte>& enumerable);
			Plugin::SystemCollectionsGenericICollectionSystemByteIterator end(System::Collections::Generic::ICollection_1<System::Byte>& enumerable);
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct ICollection_1<System::Int32> : virtual System::Collections::Generic::IEnumerable_1<System::Int32>
			{
				ICollection_1(decltype(nullptr));
				ICollection_1(Plugin::InternalUse, int32_t handle);
				ICollection_1(const ICollection_1<System::Int32>& other);
				ICollection_1(ICollection_1<System::Int32>&& other);
				virtual ~ICollection_1();
				ICollection_1<System::Int32>& operator=(const ICollection_1<System::Int32>& other);
				ICollection_1<System::Int32>& operator=(decltype(nullptr));
				ICollection_1<System::Int32>& operator=(ICollection_1<System::Int32>&& other);
				bool operator==(const ICollection_1<System::Int32>& other) const;
				bool operator!=(const ICollection_1<System::Int32>& other) const;
			};
		}
	}
}

namespace Plugin
{
	struct SystemCollectionsGenericICollectionSystemInt32Iterator
	{
		System::Collections::Generic::IEnumerator_1<System::Int32> enumerator;
		bool hasMore;
		SystemCollectionsGenericICollectionSystemInt32Iterator(decltype(nullptr));
		SystemCollectionsGenericICollectionSystemInt32Iterator(System::Collections::Generic::ICollection_1<System::Int32>& enumerable);
		~SystemCollectionsGenericICollectionSystemInt32Iterator();
		SystemCollectionsGenericICollectionSystemInt32Iterator& operator++();
		bool operator!=(const SystemCollectionsGenericICollectionSystemInt32Iterator& other);
		System::Int32 operator*();
	};
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericICollectionSystemInt32Iterator begin(System::Collections::Generic::ICollection_1<System::Int32>& enumerable);
			Plugin::SystemCollectionsGenericICollectionSystemInt32Iterator end(System::Collections::Generic::ICollection_1<System::Int32>& enumerable);
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IReadOnlyCollection_1<UnityEngine::Vector2> : virtual System::Collections::Generic::IEnumerable_1<UnityEngine::Vector2>
			{
				IReadOnlyCollection_1(decltype(nullptr));
				IReadOnlyCollection_1(Plugin::InternalUse, int32_t handle);
				IReadOnlyCollection_1(const IReadOnlyCollection_1<UnityEngine::Vector2>& other);
				IReadOnlyCollection_1(IReadOnlyCollection_1<UnityEngine::Vector2>&& other);
				virtual ~IReadOnlyCollection_1();
				IReadOnlyCollection_1<UnityEngine::Vector2>& operator=(const IReadOnlyCollection_1<UnityEngine::Vector2>& other);
				IReadOnlyCollection_1<UnityEngine::Vector2>& operator=(decltype(nullptr));
				IReadOnlyCollection_1<UnityEngine::Vector2>& operator=(IReadOnlyCollection_1<UnityEngine::Vector2>&& other);
				bool operator==(const IReadOnlyCollection_1<UnityEngine::Vector2>& other) const;
				bool operator!=(const IReadOnlyCollection_1<UnityEngine::Vector2>& other) const;
			};
		}
	}
}

namespace Plugin
{
	struct SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector2Iterator
	{
		System::Collections::Generic::IEnumerator_1<UnityEngine::Vector2> enumerator;
		bool hasMore;
		SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector2Iterator(decltype(nullptr));
		SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector2Iterator(System::Collections::Generic::IReadOnlyCollection_1<UnityEngine::Vector2>& enumerable);
		~SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector2Iterator();
		SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector2Iterator& operator++();
		bool operator!=(const SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector2Iterator& other);
		UnityEngine::Vector2 operator*();
	};
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector2Iterator begin(System::Collections::Generic::IReadOnlyCollection_1<UnityEngine::Vector2>& enumerable);
			Plugin::SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector2Iterator end(System::Collections::Generic::IReadOnlyCollection_1<UnityEngine::Vector2>& enumerable);
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IReadOnlyCollection_1<UnityEngine::Vector3> : virtual System::Collections::Generic::IEnumerable_1<UnityEngine::Vector3>
			{
				IReadOnlyCollection_1(decltype(nullptr));
				IReadOnlyCollection_1(Plugin::InternalUse, int32_t handle);
				IReadOnlyCollection_1(const IReadOnlyCollection_1<UnityEngine::Vector3>& other);
				IReadOnlyCollection_1(IReadOnlyCollection_1<UnityEngine::Vector3>&& other);
				virtual ~IReadOnlyCollection_1();
				IReadOnlyCollection_1<UnityEngine::Vector3>& operator=(const IReadOnlyCollection_1<UnityEngine::Vector3>& other);
				IReadOnlyCollection_1<UnityEngine::Vector3>& operator=(decltype(nullptr));
				IReadOnlyCollection_1<UnityEngine::Vector3>& operator=(IReadOnlyCollection_1<UnityEngine::Vector3>&& other);
				bool operator==(const IReadOnlyCollection_1<UnityEngine::Vector3>& other) const;
				bool operator!=(const IReadOnlyCollection_1<UnityEngine::Vector3>& other) const;
			};
		}
	}
}

namespace Plugin
{
	struct SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator
	{
		System::Collections::Generic::IEnumerator_1<UnityEngine::Vector3> enumerator;
		bool hasMore;
		SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator(decltype(nullptr));
		SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator(System::Collections::Generic::IReadOnlyCollection_1<UnityEngine::Vector3>& enumerable);
		~SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator();
		SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator& operator++();
		bool operator!=(const SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator& other);
		UnityEngine::Vector3 operator*();
	};
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator begin(System::Collections::Generic::IReadOnlyCollection_1<UnityEngine::Vector3>& enumerable);
			Plugin::SystemCollectionsGenericIReadOnlyCollectionUnityEngineVector3Iterator end(System::Collections::Generic::IReadOnlyCollection_1<UnityEngine::Vector3>& enumerable);
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IReadOnlyCollection_1<System::Int32> : virtual System::Collections::Generic::IEnumerable_1<System::Int32>
			{
				IReadOnlyCollection_1(decltype(nullptr));
				IReadOnlyCollection_1(Plugin::InternalUse, int32_t handle);
				IReadOnlyCollection_1(const IReadOnlyCollection_1<System::Int32>& other);
				IReadOnlyCollection_1(IReadOnlyCollection_1<System::Int32>&& other);
				virtual ~IReadOnlyCollection_1();
				IReadOnlyCollection_1<System::Int32>& operator=(const IReadOnlyCollection_1<System::Int32>& other);
				IReadOnlyCollection_1<System::Int32>& operator=(decltype(nullptr));
				IReadOnlyCollection_1<System::Int32>& operator=(IReadOnlyCollection_1<System::Int32>&& other);
				bool operator==(const IReadOnlyCollection_1<System::Int32>& other) const;
				bool operator!=(const IReadOnlyCollection_1<System::Int32>& other) const;
			};
		}
	}
}

namespace Plugin
{
	struct SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator
	{
		System::Collections::Generic::IEnumerator_1<System::Int32> enumerator;
		bool hasMore;
		SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator(decltype(nullptr));
		SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator(System::Collections::Generic::IReadOnlyCollection_1<System::Int32>& enumerable);
		~SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator();
		SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator& operator++();
		bool operator!=(const SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator& other);
		System::Int32 operator*();
	};
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator begin(System::Collections::Generic::IReadOnlyCollection_1<System::Int32>& enumerable);
			Plugin::SystemCollectionsGenericIReadOnlyCollectionSystemInt32Iterator end(System::Collections::Generic::IReadOnlyCollection_1<System::Int32>& enumerable);
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IReadOnlyCollection_1<System::Byte> : virtual System::Collections::Generic::IEnumerable_1<System::Byte>
			{
				IReadOnlyCollection_1(decltype(nullptr));
				IReadOnlyCollection_1(Plugin::InternalUse, int32_t handle);
				IReadOnlyCollection_1(const IReadOnlyCollection_1<System::Byte>& other);
				IReadOnlyCollection_1(IReadOnlyCollection_1<System::Byte>&& other);
				virtual ~IReadOnlyCollection_1();
				IReadOnlyCollection_1<System::Byte>& operator=(const IReadOnlyCollection_1<System::Byte>& other);
				IReadOnlyCollection_1<System::Byte>& operator=(decltype(nullptr));
				IReadOnlyCollection_1<System::Byte>& operator=(IReadOnlyCollection_1<System::Byte>&& other);
				bool operator==(const IReadOnlyCollection_1<System::Byte>& other) const;
				bool operator!=(const IReadOnlyCollection_1<System::Byte>& other) const;
			};
		}
	}
}

namespace Plugin
{
	struct SystemCollectionsGenericIReadOnlyCollectionSystemByteIterator
	{
		System::Collections::Generic::IEnumerator_1<System::Byte> enumerator;
		bool hasMore;
		SystemCollectionsGenericIReadOnlyCollectionSystemByteIterator(decltype(nullptr));
		SystemCollectionsGenericIReadOnlyCollectionSystemByteIterator(System::Collections::Generic::IReadOnlyCollection_1<System::Byte>& enumerable);
		~SystemCollectionsGenericIReadOnlyCollectionSystemByteIterator();
		SystemCollectionsGenericIReadOnlyCollectionSystemByteIterator& operator++();
		bool operator!=(const SystemCollectionsGenericIReadOnlyCollectionSystemByteIterator& other);
		System::Byte operator*();
	};
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIReadOnlyCollectionSystemByteIterator begin(System::Collections::Generic::IReadOnlyCollection_1<System::Byte>& enumerable);
			Plugin::SystemCollectionsGenericIReadOnlyCollectionSystemByteIterator end(System::Collections::Generic::IReadOnlyCollection_1<System::Byte>& enumerable);
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IList_1<UnityEngine::Vector2> : virtual System::Collections::Generic::ICollection_1<UnityEngine::Vector2>
			{
				IList_1(decltype(nullptr));
				IList_1(Plugin::InternalUse, int32_t handle);
				IList_1(const IList_1<UnityEngine::Vector2>& other);
				IList_1(IList_1<UnityEngine::Vector2>&& other);
				virtual ~IList_1();
				IList_1<UnityEngine::Vector2>& operator=(const IList_1<UnityEngine::Vector2>& other);
				IList_1<UnityEngine::Vector2>& operator=(decltype(nullptr));
				IList_1<UnityEngine::Vector2>& operator=(IList_1<UnityEngine::Vector2>&& other);
				bool operator==(const IList_1<UnityEngine::Vector2>& other) const;
				bool operator!=(const IList_1<UnityEngine::Vector2>& other) const;
			};
		}
	}
}

namespace Plugin
{
	struct SystemCollectionsGenericIListUnityEngineVector2Iterator
	{
		System::Collections::Generic::IEnumerator_1<UnityEngine::Vector2> enumerator;
		bool hasMore;
		SystemCollectionsGenericIListUnityEngineVector2Iterator(decltype(nullptr));
		SystemCollectionsGenericIListUnityEngineVector2Iterator(System::Collections::Generic::IList_1<UnityEngine::Vector2>& enumerable);
		~SystemCollectionsGenericIListUnityEngineVector2Iterator();
		SystemCollectionsGenericIListUnityEngineVector2Iterator& operator++();
		bool operator!=(const SystemCollectionsGenericIListUnityEngineVector2Iterator& other);
		UnityEngine::Vector2 operator*();
	};
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIListUnityEngineVector2Iterator begin(System::Collections::Generic::IList_1<UnityEngine::Vector2>& enumerable);
			Plugin::SystemCollectionsGenericIListUnityEngineVector2Iterator end(System::Collections::Generic::IList_1<UnityEngine::Vector2>& enumerable);
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IList_1<UnityEngine::Vector3> : virtual System::Collections::Generic::ICollection_1<UnityEngine::Vector3>
			{
				IList_1(decltype(nullptr));
				IList_1(Plugin::InternalUse, int32_t handle);
				IList_1(const IList_1<UnityEngine::Vector3>& other);
				IList_1(IList_1<UnityEngine::Vector3>&& other);
				virtual ~IList_1();
				IList_1<UnityEngine::Vector3>& operator=(const IList_1<UnityEngine::Vector3>& other);
				IList_1<UnityEngine::Vector3>& operator=(decltype(nullptr));
				IList_1<UnityEngine::Vector3>& operator=(IList_1<UnityEngine::Vector3>&& other);
				bool operator==(const IList_1<UnityEngine::Vector3>& other) const;
				bool operator!=(const IList_1<UnityEngine::Vector3>& other) const;
			};
		}
	}
}

namespace Plugin
{
	struct SystemCollectionsGenericIListUnityEngineVector3Iterator
	{
		System::Collections::Generic::IEnumerator_1<UnityEngine::Vector3> enumerator;
		bool hasMore;
		SystemCollectionsGenericIListUnityEngineVector3Iterator(decltype(nullptr));
		SystemCollectionsGenericIListUnityEngineVector3Iterator(System::Collections::Generic::IList_1<UnityEngine::Vector3>& enumerable);
		~SystemCollectionsGenericIListUnityEngineVector3Iterator();
		SystemCollectionsGenericIListUnityEngineVector3Iterator& operator++();
		bool operator!=(const SystemCollectionsGenericIListUnityEngineVector3Iterator& other);
		UnityEngine::Vector3 operator*();
	};
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIListUnityEngineVector3Iterator begin(System::Collections::Generic::IList_1<UnityEngine::Vector3>& enumerable);
			Plugin::SystemCollectionsGenericIListUnityEngineVector3Iterator end(System::Collections::Generic::IList_1<UnityEngine::Vector3>& enumerable);
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IList_1<System::Byte> : virtual System::Collections::Generic::ICollection_1<System::Byte>
			{
				IList_1(decltype(nullptr));
				IList_1(Plugin::InternalUse, int32_t handle);
				IList_1(const IList_1<System::Byte>& other);
				IList_1(IList_1<System::Byte>&& other);
				virtual ~IList_1();
				IList_1<System::Byte>& operator=(const IList_1<System::Byte>& other);
				IList_1<System::Byte>& operator=(decltype(nullptr));
				IList_1<System::Byte>& operator=(IList_1<System::Byte>&& other);
				bool operator==(const IList_1<System::Byte>& other) const;
				bool operator!=(const IList_1<System::Byte>& other) const;
			};
		}
	}
}

namespace Plugin
{
	struct SystemCollectionsGenericIListSystemByteIterator
	{
		System::Collections::Generic::IEnumerator_1<System::Byte> enumerator;
		bool hasMore;
		SystemCollectionsGenericIListSystemByteIterator(decltype(nullptr));
		SystemCollectionsGenericIListSystemByteIterator(System::Collections::Generic::IList_1<System::Byte>& enumerable);
		~SystemCollectionsGenericIListSystemByteIterator();
		SystemCollectionsGenericIListSystemByteIterator& operator++();
		bool operator!=(const SystemCollectionsGenericIListSystemByteIterator& other);
		System::Byte operator*();
	};
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIListSystemByteIterator begin(System::Collections::Generic::IList_1<System::Byte>& enumerable);
			Plugin::SystemCollectionsGenericIListSystemByteIterator end(System::Collections::Generic::IList_1<System::Byte>& enumerable);
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IList_1<System::Int32> : virtual System::Collections::Generic::ICollection_1<System::Int32>
			{
				IList_1(decltype(nullptr));
				IList_1(Plugin::InternalUse, int32_t handle);
				IList_1(const IList_1<System::Int32>& other);
				IList_1(IList_1<System::Int32>&& other);
				virtual ~IList_1();
				IList_1<System::Int32>& operator=(const IList_1<System::Int32>& other);
				IList_1<System::Int32>& operator=(decltype(nullptr));
				IList_1<System::Int32>& operator=(IList_1<System::Int32>&& other);
				bool operator==(const IList_1<System::Int32>& other) const;
				bool operator!=(const IList_1<System::Int32>& other) const;
			};
		}
	}
}

namespace Plugin
{
	struct SystemCollectionsGenericIListSystemInt32Iterator
	{
		System::Collections::Generic::IEnumerator_1<System::Int32> enumerator;
		bool hasMore;
		SystemCollectionsGenericIListSystemInt32Iterator(decltype(nullptr));
		SystemCollectionsGenericIListSystemInt32Iterator(System::Collections::Generic::IList_1<System::Int32>& enumerable);
		~SystemCollectionsGenericIListSystemInt32Iterator();
		SystemCollectionsGenericIListSystemInt32Iterator& operator++();
		bool operator!=(const SystemCollectionsGenericIListSystemInt32Iterator& other);
		System::Int32 operator*();
	};
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIListSystemInt32Iterator begin(System::Collections::Generic::IList_1<System::Int32>& enumerable);
			Plugin::SystemCollectionsGenericIListSystemInt32Iterator end(System::Collections::Generic::IList_1<System::Int32>& enumerable);
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IReadOnlyList_1<UnityEngine::Vector2> : virtual System::Collections::Generic::IReadOnlyCollection_1<UnityEngine::Vector2>
			{
				IReadOnlyList_1(decltype(nullptr));
				IReadOnlyList_1(Plugin::InternalUse, int32_t handle);
				IReadOnlyList_1(const IReadOnlyList_1<UnityEngine::Vector2>& other);
				IReadOnlyList_1(IReadOnlyList_1<UnityEngine::Vector2>&& other);
				virtual ~IReadOnlyList_1();
				IReadOnlyList_1<UnityEngine::Vector2>& operator=(const IReadOnlyList_1<UnityEngine::Vector2>& other);
				IReadOnlyList_1<UnityEngine::Vector2>& operator=(decltype(nullptr));
				IReadOnlyList_1<UnityEngine::Vector2>& operator=(IReadOnlyList_1<UnityEngine::Vector2>&& other);
				bool operator==(const IReadOnlyList_1<UnityEngine::Vector2>& other) const;
				bool operator!=(const IReadOnlyList_1<UnityEngine::Vector2>& other) const;
			};
		}
	}
}

namespace Plugin
{
	struct SystemCollectionsGenericIReadOnlyListUnityEngineVector2Iterator
	{
		System::Collections::Generic::IEnumerator_1<UnityEngine::Vector2> enumerator;
		bool hasMore;
		SystemCollectionsGenericIReadOnlyListUnityEngineVector2Iterator(decltype(nullptr));
		SystemCollectionsGenericIReadOnlyListUnityEngineVector2Iterator(System::Collections::Generic::IReadOnlyList_1<UnityEngine::Vector2>& enumerable);
		~SystemCollectionsGenericIReadOnlyListUnityEngineVector2Iterator();
		SystemCollectionsGenericIReadOnlyListUnityEngineVector2Iterator& operator++();
		bool operator!=(const SystemCollectionsGenericIReadOnlyListUnityEngineVector2Iterator& other);
		UnityEngine::Vector2 operator*();
	};
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIReadOnlyListUnityEngineVector2Iterator begin(System::Collections::Generic::IReadOnlyList_1<UnityEngine::Vector2>& enumerable);
			Plugin::SystemCollectionsGenericIReadOnlyListUnityEngineVector2Iterator end(System::Collections::Generic::IReadOnlyList_1<UnityEngine::Vector2>& enumerable);
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IReadOnlyList_1<UnityEngine::Vector3> : virtual System::Collections::Generic::IReadOnlyCollection_1<UnityEngine::Vector3>
			{
				IReadOnlyList_1(decltype(nullptr));
				IReadOnlyList_1(Plugin::InternalUse, int32_t handle);
				IReadOnlyList_1(const IReadOnlyList_1<UnityEngine::Vector3>& other);
				IReadOnlyList_1(IReadOnlyList_1<UnityEngine::Vector3>&& other);
				virtual ~IReadOnlyList_1();
				IReadOnlyList_1<UnityEngine::Vector3>& operator=(const IReadOnlyList_1<UnityEngine::Vector3>& other);
				IReadOnlyList_1<UnityEngine::Vector3>& operator=(decltype(nullptr));
				IReadOnlyList_1<UnityEngine::Vector3>& operator=(IReadOnlyList_1<UnityEngine::Vector3>&& other);
				bool operator==(const IReadOnlyList_1<UnityEngine::Vector3>& other) const;
				bool operator!=(const IReadOnlyList_1<UnityEngine::Vector3>& other) const;
			};
		}
	}
}

namespace Plugin
{
	struct SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator
	{
		System::Collections::Generic::IEnumerator_1<UnityEngine::Vector3> enumerator;
		bool hasMore;
		SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator(decltype(nullptr));
		SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator(System::Collections::Generic::IReadOnlyList_1<UnityEngine::Vector3>& enumerable);
		~SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator();
		SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator& operator++();
		bool operator!=(const SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator& other);
		UnityEngine::Vector3 operator*();
	};
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator begin(System::Collections::Generic::IReadOnlyList_1<UnityEngine::Vector3>& enumerable);
			Plugin::SystemCollectionsGenericIReadOnlyListUnityEngineVector3Iterator end(System::Collections::Generic::IReadOnlyList_1<UnityEngine::Vector3>& enumerable);
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IReadOnlyList_1<System::Byte> : virtual System::Collections::Generic::IReadOnlyCollection_1<System::Byte>
			{
				IReadOnlyList_1(decltype(nullptr));
				IReadOnlyList_1(Plugin::InternalUse, int32_t handle);
				IReadOnlyList_1(const IReadOnlyList_1<System::Byte>& other);
				IReadOnlyList_1(IReadOnlyList_1<System::Byte>&& other);
				virtual ~IReadOnlyList_1();
				IReadOnlyList_1<System::Byte>& operator=(const IReadOnlyList_1<System::Byte>& other);
				IReadOnlyList_1<System::Byte>& operator=(decltype(nullptr));
				IReadOnlyList_1<System::Byte>& operator=(IReadOnlyList_1<System::Byte>&& other);
				bool operator==(const IReadOnlyList_1<System::Byte>& other) const;
				bool operator!=(const IReadOnlyList_1<System::Byte>& other) const;
			};
		}
	}
}

namespace Plugin
{
	struct SystemCollectionsGenericIReadOnlyListSystemByteIterator
	{
		System::Collections::Generic::IEnumerator_1<System::Byte> enumerator;
		bool hasMore;
		SystemCollectionsGenericIReadOnlyListSystemByteIterator(decltype(nullptr));
		SystemCollectionsGenericIReadOnlyListSystemByteIterator(System::Collections::Generic::IReadOnlyList_1<System::Byte>& enumerable);
		~SystemCollectionsGenericIReadOnlyListSystemByteIterator();
		SystemCollectionsGenericIReadOnlyListSystemByteIterator& operator++();
		bool operator!=(const SystemCollectionsGenericIReadOnlyListSystemByteIterator& other);
		System::Byte operator*();
	};
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIReadOnlyListSystemByteIterator begin(System::Collections::Generic::IReadOnlyList_1<System::Byte>& enumerable);
			Plugin::SystemCollectionsGenericIReadOnlyListSystemByteIterator end(System::Collections::Generic::IReadOnlyList_1<System::Byte>& enumerable);
		}
	}
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			template<> struct IReadOnlyList_1<System::Int32> : virtual System::Collections::Generic::IReadOnlyCollection_1<System::Int32>
			{
				IReadOnlyList_1(decltype(nullptr));
				IReadOnlyList_1(Plugin::InternalUse, int32_t handle);
				IReadOnlyList_1(const IReadOnlyList_1<System::Int32>& other);
				IReadOnlyList_1(IReadOnlyList_1<System::Int32>&& other);
				virtual ~IReadOnlyList_1();
				IReadOnlyList_1<System::Int32>& operator=(const IReadOnlyList_1<System::Int32>& other);
				IReadOnlyList_1<System::Int32>& operator=(decltype(nullptr));
				IReadOnlyList_1<System::Int32>& operator=(IReadOnlyList_1<System::Int32>&& other);
				bool operator==(const IReadOnlyList_1<System::Int32>& other) const;
				bool operator!=(const IReadOnlyList_1<System::Int32>& other) const;
			};
		}
	}
}

namespace Plugin
{
	struct SystemCollectionsGenericIReadOnlyListSystemInt32Iterator
	{
		System::Collections::Generic::IEnumerator_1<System::Int32> enumerator;
		bool hasMore;
		SystemCollectionsGenericIReadOnlyListSystemInt32Iterator(decltype(nullptr));
		SystemCollectionsGenericIReadOnlyListSystemInt32Iterator(System::Collections::Generic::IReadOnlyList_1<System::Int32>& enumerable);
		~SystemCollectionsGenericIReadOnlyListSystemInt32Iterator();
		SystemCollectionsGenericIReadOnlyListSystemInt32Iterator& operator++();
		bool operator!=(const SystemCollectionsGenericIReadOnlyListSystemInt32Iterator& other);
		System::Int32 operator*();
	};
}

namespace System
{
	namespace Collections
	{
		namespace Generic
		{
			Plugin::SystemCollectionsGenericIReadOnlyListSystemInt32Iterator begin(System::Collections::Generic::IReadOnlyList_1<System::Int32>& enumerable);
			Plugin::SystemCollectionsGenericIReadOnlyListSystemInt32Iterator end(System::Collections::Generic::IReadOnlyList_1<System::Int32>& enumerable);
		}
	}
}

namespace Unity
{
	namespace Collections
	{
		struct NativeArrayOptions
		{
			int32_t Value;
			static const Unity::Collections::NativeArrayOptions UninitializedMemory;
			static const Unity::Collections::NativeArrayOptions ClearMemory;
			explicit NativeArrayOptions(int32_t value);
			explicit operator int32_t() const;
			bool operator==(NativeArrayOptions other);
			bool operator!=(NativeArrayOptions other);
			explicit operator System::Enum();
			explicit operator System::ValueType();
			explicit operator System::Object();
			explicit operator System::IFormattable();
			explicit operator System::IComparable();
			explicit operator System::IConvertible();
		};
	}
}

namespace Unity
{
	namespace Collections
	{
		template<> struct NativeArray_1<System::Byte> : Plugin::ManagedType
		{
			NativeArray_1(decltype(nullptr));
			NativeArray_1(Plugin::InternalUse, int32_t handle);
			NativeArray_1(const NativeArray_1<System::Byte>& other);
			NativeArray_1(NativeArray_1<System::Byte>&& other);
			virtual ~NativeArray_1();
			NativeArray_1<System::Byte>& operator=(const NativeArray_1<System::Byte>& other);
			NativeArray_1<System::Byte>& operator=(decltype(nullptr));
			NativeArray_1<System::Byte>& operator=(NativeArray_1<System::Byte>&& other);
			bool operator==(const NativeArray_1<System::Byte>& other) const;
			bool operator!=(const NativeArray_1<System::Byte>& other) const;
			NativeArray_1(System::Int32 length, Unity::Collections::Allocator allocator, Unity::Collections::NativeArrayOptions options = Unity::Collections::NativeArrayOptions::ClearMemory);
			virtual void Dispose();
			explicit operator System::ValueType();
			explicit operator System::Object();
			explicit operator System::Collections::Generic::IEnumerable_1<System::Byte>();
			explicit operator System::Collections::IEnumerable();
			explicit operator System::IDisposable();
			explicit operator System::IEquatable_1<Unity::Collections::NativeArray_1<System::Byte>>();
		};
	}
}

namespace Unity
{
	namespace Collections
	{
		template<> struct NativeArray_1<UnityEngine::Vector3> : Plugin::ManagedType
		{
			NativeArray_1(decltype(nullptr));
			NativeArray_1(Plugin::InternalUse, int32_t handle);
			NativeArray_1(const NativeArray_1<UnityEngine::Vector3>& other);
			NativeArray_1(NativeArray_1<UnityEngine::Vector3>&& other);
			virtual ~NativeArray_1();
			NativeArray_1<UnityEngine::Vector3>& operator=(const NativeArray_1<UnityEngine::Vector3>& other);
			NativeArray_1<UnityEngine::Vector3>& operator=(decltype(nullptr));
			NativeArray_1<UnityEngine::Vector3>& operator=(NativeArray_1<UnityEngine::Vector3>&& other);
			bool operator==(const NativeArray_1<UnityEngine::Vector3>& other) const;
			bool operator!=(const NativeArray_1<UnityEngine::Vector3>& other) const;
			NativeArray_1(System::Int32 length, Unity::Collections::Allocator allocator, Unity::Collections::NativeArrayOptions options = Unity::Collections::NativeArrayOptions::ClearMemory);
			virtual void Dispose();
			explicit operator System::ValueType();
			explicit operator System::Object();
			explicit operator System::Collections::Generic::IEnumerable_1<UnityEngine::Vector3>();
			explicit operator System::Collections::IEnumerable();
			explicit operator System::IDisposable();
			explicit operator System::IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector3>>();
		};
	}
}

namespace Unity
{
	namespace Collections
	{
		template<> struct NativeArray_1<UnityEngine::Vector2> : Plugin::ManagedType
		{
			NativeArray_1(decltype(nullptr));
			NativeArray_1(Plugin::InternalUse, int32_t handle);
			NativeArray_1(const NativeArray_1<UnityEngine::Vector2>& other);
			NativeArray_1(NativeArray_1<UnityEngine::Vector2>&& other);
			virtual ~NativeArray_1();
			NativeArray_1<UnityEngine::Vector2>& operator=(const NativeArray_1<UnityEngine::Vector2>& other);
			NativeArray_1<UnityEngine::Vector2>& operator=(decltype(nullptr));
			NativeArray_1<UnityEngine::Vector2>& operator=(NativeArray_1<UnityEngine::Vector2>&& other);
			bool operator==(const NativeArray_1<UnityEngine::Vector2>& other) const;
			bool operator!=(const NativeArray_1<UnityEngine::Vector2>& other) const;
			NativeArray_1(System::Int32 length, Unity::Collections::Allocator allocator, Unity::Collections::NativeArrayOptions options = Unity::Collections::NativeArrayOptions::ClearMemory);
			virtual void Dispose();
			explicit operator System::ValueType();
			explicit operator System::Object();
			explicit operator System::Collections::Generic::IEnumerable_1<UnityEngine::Vector2>();
			explicit operator System::Collections::IEnumerable();
			explicit operator System::IDisposable();
			explicit operator System::IEquatable_1<Unity::Collections::NativeArray_1<UnityEngine::Vector2>>();
		};
	}
}

namespace Unity
{
	namespace Collections
	{
		template<> struct NativeArray_1<System::Int32> : Plugin::ManagedType
		{
			NativeArray_1(decltype(nullptr));
			NativeArray_1(Plugin::InternalUse, int32_t handle);
			NativeArray_1(const NativeArray_1<System::Int32>& other);
			NativeArray_1(NativeArray_1<System::Int32>&& other);
			virtual ~NativeArray_1();
			NativeArray_1<System::Int32>& operator=(const NativeArray_1<System::Int32>& other);
			NativeArray_1<System::Int32>& operator=(decltype(nullptr));
			NativeArray_1<System::Int32>& operator=(NativeArray_1<System::Int32>&& other);
			bool operator==(const NativeArray_1<System::Int32>& other) const;
			bool operator!=(const NativeArray_1<System::Int32>& other) const;
			NativeArray_1(System::Int32 length, Unity::Collections::Allocator allocator, Unity::Collections::NativeArrayOptions options = Unity::Collections::NativeArrayOptions::ClearMemory);
			virtual void Dispose();
			explicit operator System::ValueType();
			explicit operator System::Object();
			explicit operator System::Collections::Generic::IEnumerable_1<System::Int32>();
			explicit operator System::Collections::IEnumerable();
			explicit operator System::IDisposable();
			explicit operator System::IEquatable_1<Unity::Collections::NativeArray_1<System::Int32>>();
		};
	}
}

namespace Unity
{
	namespace Collections
	{
		struct Allocator
		{
			int32_t Value;
			static const Unity::Collections::Allocator Invalid;
			static const Unity::Collections::Allocator None;
			static const Unity::Collections::Allocator Temp;
			static const Unity::Collections::Allocator TempJob;
			static const Unity::Collections::Allocator Persistent;
			static const Unity::Collections::Allocator AudioKernel;
			explicit Allocator(int32_t value);
			explicit operator int32_t() const;
			bool operator==(Allocator other);
			bool operator!=(Allocator other);
			explicit operator System::Enum();
			explicit operator System::ValueType();
			explicit operator System::Object();
			explicit operator System::IFormattable();
			explicit operator System::IComparable();
			explicit operator System::IConvertible();
		};
	}
}

namespace Unity
{
	namespace Collections
	{
		namespace LowLevel
		{
			namespace Unsafe
			{
				namespace NativeArrayUnsafeUtility
				{
					template<typename MT0> Unity::Collections::NativeArray_1<MT0> ConvertExistingDataToNativeArray(System::Void* dataPointer, System::Int32 length, Unity::Collections::Allocator allocator);
					template<typename MT0> System::Void* GetUnsafeBufferPointerWithoutChecks(Unity::Collections::NativeArray_1<MT0>& nativeArray);
				}
			}
		}
	}
}

namespace UnityEngine
{
	struct MeshTopology
	{
		int32_t Value;
		static const UnityEngine::MeshTopology Triangles;
		static const UnityEngine::MeshTopology Quads;
		static const UnityEngine::MeshTopology Lines;
		static const UnityEngine::MeshTopology LineStrip;
		static const UnityEngine::MeshTopology Points;
		explicit MeshTopology(int32_t value);
		explicit operator int32_t() const;
		bool operator==(MeshTopology other);
		bool operator!=(MeshTopology other);
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
	struct Mesh : virtual UnityEngine::Object
	{
		Mesh(decltype(nullptr));
		Mesh(Plugin::InternalUse, int32_t handle);
		Mesh(const Mesh& other);
		Mesh(Mesh&& other);
		virtual ~Mesh();
		Mesh& operator=(const Mesh& other);
		Mesh& operator=(decltype(nullptr));
		Mesh& operator=(Mesh&& other);
		bool operator==(const Mesh& other) const;
		bool operator!=(const Mesh& other) const;
		Mesh();
		virtual void SetVertices(System::Array1<UnityEngine::Vector3>& inVertices);
		template<typename MT0> void SetVertices(Unity::Collections::NativeArray_1<MT0>& inVertices);
		virtual void SetNormals(System::Array1<UnityEngine::Vector3>& inNormals);
		template<typename MT0> void SetNormals(Unity::Collections::NativeArray_1<MT0>& inNormals);
		virtual void SetTriangles(System::Array1<System::Int32>& triangles, System::Int32 submesh, System::Boolean calculateBounds, System::Int32 baseVertex);
		template<typename MT0> void SetIndices(Unity::Collections::NativeArray_1<MT0>& indices, UnityEngine::MeshTopology topology, MT0 submesh, System::Boolean calculateBounds = true, MT0 baseVertex = 0);
		virtual void SetUVs(System::Int32 channel, System::Array1<UnityEngine::Vector2>& uvs);
		template<typename MT0> void SetUVs(System::Int32 channel, Unity::Collections::NativeArray_1<MT0>& uvs);
	};
}

namespace UnityEngine
{
	struct MeshFilter : virtual UnityEngine::Component
	{
		MeshFilter(decltype(nullptr));
		MeshFilter(Plugin::InternalUse, int32_t handle);
		MeshFilter(const MeshFilter& other);
		MeshFilter(MeshFilter&& other);
		virtual ~MeshFilter();
		MeshFilter& operator=(const MeshFilter& other);
		MeshFilter& operator=(decltype(nullptr));
		MeshFilter& operator=(MeshFilter&& other);
		bool operator==(const MeshFilter& other) const;
		bool operator!=(const MeshFilter& other) const;
		UnityEngine::Mesh GetMesh();
		void SetMesh(UnityEngine::Mesh& value);
	};
}

namespace UnityEngine
{
	struct Renderer : virtual UnityEngine::Component
	{
		Renderer(decltype(nullptr));
		Renderer(Plugin::InternalUse, int32_t handle);
		Renderer(const Renderer& other);
		Renderer(Renderer&& other);
		virtual ~Renderer();
		Renderer& operator=(const Renderer& other);
		Renderer& operator=(decltype(nullptr));
		Renderer& operator=(Renderer&& other);
		bool operator==(const Renderer& other) const;
		bool operator!=(const Renderer& other) const;
	};
}

namespace UnityEngine
{
	struct MeshRenderer : virtual UnityEngine::Renderer
	{
		MeshRenderer(decltype(nullptr));
		MeshRenderer(Plugin::InternalUse, int32_t handle);
		MeshRenderer(const MeshRenderer& other);
		MeshRenderer(MeshRenderer&& other);
		virtual ~MeshRenderer();
		MeshRenderer& operator=(const MeshRenderer& other);
		MeshRenderer& operator=(decltype(nullptr));
		MeshRenderer& operator=(MeshRenderer&& other);
		bool operator==(const MeshRenderer& other) const;
		bool operator!=(const MeshRenderer& other) const;
		UnityEngine::Material GetMaterial();
		void SetMaterial(UnityEngine::Material& value);
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

namespace UnityEngine
{
	struct Material : virtual UnityEngine::Object
	{
		Material(decltype(nullptr));
		Material(Plugin::InternalUse, int32_t handle);
		Material(const Material& other);
		Material(Material&& other);
		virtual ~Material();
		Material& operator=(const Material& other);
		Material& operator=(decltype(nullptr));
		Material& operator=(Material&& other);
		bool operator==(const Material& other) const;
		bool operator!=(const Material& other) const;
		virtual void SetTexture(System::String& name, UnityEngine::Texture& value);
	};
}

namespace UnityEngine
{
	struct Resources : virtual System::Object
	{
		Resources(decltype(nullptr));
		Resources(Plugin::InternalUse, int32_t handle);
		Resources(const Resources& other);
		Resources(Resources&& other);
		virtual ~Resources();
		Resources& operator=(const Resources& other);
		Resources& operator=(decltype(nullptr));
		Resources& operator=(Resources&& other);
		bool operator==(const Resources& other) const;
		bool operator!=(const Resources& other) const;
		template<typename MT0> static MT0 Load(System::String& path);
	};
}

namespace UnityEngine
{
	struct ScriptableObject : virtual UnityEngine::Object
	{
		ScriptableObject(decltype(nullptr));
		ScriptableObject(Plugin::InternalUse, int32_t handle);
		ScriptableObject(const ScriptableObject& other);
		ScriptableObject(ScriptableObject&& other);
		virtual ~ScriptableObject();
		ScriptableObject& operator=(const ScriptableObject& other);
		ScriptableObject& operator=(decltype(nullptr));
		ScriptableObject& operator=(ScriptableObject&& other);
		bool operator==(const ScriptableObject& other) const;
		bool operator!=(const ScriptableObject& other) const;
	};
}

namespace UnityEditor
{
	struct EditorWindow : virtual UnityEngine::ScriptableObject
	{
		EditorWindow(decltype(nullptr));
		EditorWindow(Plugin::InternalUse, int32_t handle);
		EditorWindow(const EditorWindow& other);
		EditorWindow(EditorWindow&& other);
		virtual ~EditorWindow();
		EditorWindow& operator=(const EditorWindow& other);
		EditorWindow& operator=(decltype(nullptr));
		EditorWindow& operator=(EditorWindow&& other);
		bool operator==(const EditorWindow& other) const;
		bool operator!=(const EditorWindow& other) const;
	};
}

namespace UnityEditor
{
	struct SearchableEditorWindow : virtual UnityEditor::EditorWindow
	{
		SearchableEditorWindow(decltype(nullptr));
		SearchableEditorWindow(Plugin::InternalUse, int32_t handle);
		SearchableEditorWindow(const SearchableEditorWindow& other);
		SearchableEditorWindow(SearchableEditorWindow&& other);
		virtual ~SearchableEditorWindow();
		SearchableEditorWindow& operator=(const SearchableEditorWindow& other);
		SearchableEditorWindow& operator=(decltype(nullptr));
		SearchableEditorWindow& operator=(SearchableEditorWindow&& other);
		bool operator==(const SearchableEditorWindow& other) const;
		bool operator!=(const SearchableEditorWindow& other) const;
	};
}

namespace UnityEditor
{
	struct IHasCustomMenu : virtual System::Object
	{
		IHasCustomMenu(decltype(nullptr));
		IHasCustomMenu(Plugin::InternalUse, int32_t handle);
		IHasCustomMenu(const IHasCustomMenu& other);
		IHasCustomMenu(IHasCustomMenu&& other);
		virtual ~IHasCustomMenu();
		IHasCustomMenu& operator=(const IHasCustomMenu& other);
		IHasCustomMenu& operator=(decltype(nullptr));
		IHasCustomMenu& operator=(IHasCustomMenu&& other);
		bool operator==(const IHasCustomMenu& other) const;
		bool operator!=(const IHasCustomMenu& other) const;
	};
}

namespace UnityEditor
{
	namespace Overlays
	{
		struct ISupportsOverlays : virtual System::Object
		{
			ISupportsOverlays(decltype(nullptr));
			ISupportsOverlays(Plugin::InternalUse, int32_t handle);
			ISupportsOverlays(const ISupportsOverlays& other);
			ISupportsOverlays(ISupportsOverlays&& other);
			virtual ~ISupportsOverlays();
			ISupportsOverlays& operator=(const ISupportsOverlays& other);
			ISupportsOverlays& operator=(decltype(nullptr));
			ISupportsOverlays& operator=(ISupportsOverlays&& other);
			bool operator==(const ISupportsOverlays& other) const;
			bool operator!=(const ISupportsOverlays& other) const;
		};
	}
}

namespace UnityEditor
{
	struct SceneView : virtual UnityEditor::SearchableEditorWindow, virtual UnityEditor::IHasCustomMenu, virtual UnityEditor::Overlays::ISupportsOverlays
	{
		SceneView(decltype(nullptr));
		SceneView(Plugin::InternalUse, int32_t handle);
		SceneView(const SceneView& other);
		SceneView(SceneView&& other);
		virtual ~SceneView();
		SceneView& operator=(const SceneView& other);
		SceneView& operator=(decltype(nullptr));
		SceneView& operator=(SceneView&& other);
		bool operator==(const SceneView& other) const;
		bool operator!=(const SceneView& other) const;
		static UnityEditor::SceneView GetLastActiveSceneView();
		UnityEngine::Camera GetCamera();
	};
}

namespace UnityEngine
{
	struct Texture : virtual UnityEngine::Object
	{
		Texture(decltype(nullptr));
		Texture(Plugin::InternalUse, int32_t handle);
		Texture(const Texture& other);
		Texture(Texture&& other);
		virtual ~Texture();
		Texture& operator=(const Texture& other);
		Texture& operator=(decltype(nullptr));
		Texture& operator=(Texture&& other);
		bool operator==(const Texture& other) const;
		bool operator!=(const Texture& other) const;
	};
}

namespace UnityEngine
{
	struct TextureFormat
	{
		int32_t Value;
		static const UnityEngine::TextureFormat Alpha8;
		static const UnityEngine::TextureFormat ARGB4444;
		static const UnityEngine::TextureFormat RGB24;
		static const UnityEngine::TextureFormat RGBA32;
		static const UnityEngine::TextureFormat ARGB32;
		static const UnityEngine::TextureFormat RGB565;
		static const UnityEngine::TextureFormat R16;
		static const UnityEngine::TextureFormat DXT1;
		static const UnityEngine::TextureFormat DXT5;
		static const UnityEngine::TextureFormat RGBA4444;
		static const UnityEngine::TextureFormat BGRA32;
		static const UnityEngine::TextureFormat RHalf;
		static const UnityEngine::TextureFormat RGHalf;
		static const UnityEngine::TextureFormat RGBAHalf;
		static const UnityEngine::TextureFormat RFloat;
		static const UnityEngine::TextureFormat RGFloat;
		static const UnityEngine::TextureFormat RGBAFloat;
		static const UnityEngine::TextureFormat YUY2;
		static const UnityEngine::TextureFormat RGB9e5Float;
		static const UnityEngine::TextureFormat BC4;
		static const UnityEngine::TextureFormat BC5;
		static const UnityEngine::TextureFormat BC6H;
		static const UnityEngine::TextureFormat BC7;
		static const UnityEngine::TextureFormat DXT1Crunched;
		static const UnityEngine::TextureFormat DXT5Crunched;
		static const UnityEngine::TextureFormat PVRTC_RGB2;
		static const UnityEngine::TextureFormat PVRTC_RGBA2;
		static const UnityEngine::TextureFormat PVRTC_RGB4;
		static const UnityEngine::TextureFormat PVRTC_RGBA4;
		static const UnityEngine::TextureFormat ETC_RGB4;
		static const UnityEngine::TextureFormat ATC_RGB4;
		static const UnityEngine::TextureFormat ATC_RGBA8;
		static const UnityEngine::TextureFormat EAC_R;
		static const UnityEngine::TextureFormat EAC_R_SIGNED;
		static const UnityEngine::TextureFormat EAC_RG;
		static const UnityEngine::TextureFormat EAC_RG_SIGNED;
		static const UnityEngine::TextureFormat ETC2_RGB;
		static const UnityEngine::TextureFormat ETC2_RGBA1;
		static const UnityEngine::TextureFormat ETC2_RGBA8;
		static const UnityEngine::TextureFormat ASTC_4x4;
		static const UnityEngine::TextureFormat ASTC_5x5;
		static const UnityEngine::TextureFormat ASTC_6x6;
		static const UnityEngine::TextureFormat ASTC_8x8;
		static const UnityEngine::TextureFormat ASTC_10x10;
		static const UnityEngine::TextureFormat ASTC_12x12;
		static const UnityEngine::TextureFormat ETC_RGB4_3DS;
		static const UnityEngine::TextureFormat ETC_RGBA8_3DS;
		static const UnityEngine::TextureFormat RG16;
		static const UnityEngine::TextureFormat R8;
		static const UnityEngine::TextureFormat ETC_RGB4Crunched;
		static const UnityEngine::TextureFormat ETC2_RGBA8Crunched;
		static const UnityEngine::TextureFormat ASTC_HDR_4x4;
		static const UnityEngine::TextureFormat ASTC_HDR_5x5;
		static const UnityEngine::TextureFormat ASTC_HDR_6x6;
		static const UnityEngine::TextureFormat ASTC_HDR_8x8;
		static const UnityEngine::TextureFormat ASTC_HDR_10x10;
		static const UnityEngine::TextureFormat ASTC_HDR_12x12;
		static const UnityEngine::TextureFormat RG32;
		static const UnityEngine::TextureFormat RGB48;
		static const UnityEngine::TextureFormat RGBA64;
		static const UnityEngine::TextureFormat ASTC_RGB_4x4;
		static const UnityEngine::TextureFormat ASTC_RGB_5x5;
		static const UnityEngine::TextureFormat ASTC_RGB_6x6;
		static const UnityEngine::TextureFormat ASTC_RGB_8x8;
		static const UnityEngine::TextureFormat ASTC_RGB_10x10;
		static const UnityEngine::TextureFormat ASTC_RGB_12x12;
		static const UnityEngine::TextureFormat ASTC_RGBA_4x4;
		static const UnityEngine::TextureFormat ASTC_RGBA_5x5;
		static const UnityEngine::TextureFormat ASTC_RGBA_6x6;
		static const UnityEngine::TextureFormat ASTC_RGBA_8x8;
		static const UnityEngine::TextureFormat ASTC_RGBA_10x10;
		static const UnityEngine::TextureFormat ASTC_RGBA_12x12;
		static const UnityEngine::TextureFormat PVRTC_2BPP_RGB;
		static const UnityEngine::TextureFormat PVRTC_2BPP_RGBA;
		static const UnityEngine::TextureFormat PVRTC_4BPP_RGB;
		static const UnityEngine::TextureFormat PVRTC_4BPP_RGBA;
		explicit TextureFormat(int32_t value);
		explicit operator int32_t() const;
		bool operator==(TextureFormat other);
		bool operator!=(TextureFormat other);
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
	struct Texture2D : virtual UnityEngine::Texture
	{
		Texture2D(decltype(nullptr));
		Texture2D(Plugin::InternalUse, int32_t handle);
		Texture2D(const Texture2D& other);
		Texture2D(Texture2D&& other);
		virtual ~Texture2D();
		Texture2D& operator=(const Texture2D& other);
		Texture2D& operator=(decltype(nullptr));
		Texture2D& operator=(Texture2D&& other);
		bool operator==(const Texture2D& other) const;
		bool operator!=(const Texture2D& other) const;
		Texture2D(System::Int32 width, System::Int32 height, UnityEngine::TextureFormat textureFormat, System::Boolean mipChain, System::Boolean linear);
		template<typename MT0> void SetPixelData(System::Array1<MT0>& data, System::Int32 mipLevel, System::Int32 sourceDataStartIndex = 0);
		template<typename MT0> void SetPixelData(Unity::Collections::NativeArray_1<MT0>& data, System::Int32 mipLevel, System::Int32 sourceDataStartIndex = 0);
		virtual void Apply(System::Boolean updateMipmaps, System::Boolean makeNoLongerReadable);
		virtual void LoadRawTextureData(void* data, System::Int32 size);
	};
}

namespace UnityEngine
{
	struct Application : virtual System::Object
	{
		Application(decltype(nullptr));
		Application(Plugin::InternalUse, int32_t handle);
		Application(const Application& other);
		Application(Application&& other);
		virtual ~Application();
		Application& operator=(const Application& other);
		Application& operator=(decltype(nullptr));
		Application& operator=(Application&& other);
		bool operator==(const Application& other) const;
		bool operator!=(const Application& other) const;
		static System::String GetTemporaryCachePath();
		static System::String GetPersistentDataPath();
	};
}

namespace Plugin
{
	template<> struct ArrayElementProxy1_1<UnityEngine::Vector2>
	{
		int32_t Handle;
		int32_t Index0;
		ArrayElementProxy1_1<UnityEngine::Vector2>(Plugin::InternalUse, int32_t handle, int32_t index0);
		void operator=(UnityEngine::Vector2 item);
		operator UnityEngine::Vector2();
	};
}

namespace System
{
	template<> struct Array1<UnityEngine::Vector2> : virtual System::Array, virtual System::ICloneable, virtual System::Collections::IList, virtual System::Collections::Generic::IList_1<UnityEngine::Vector2>, virtual System::Collections::Generic::IReadOnlyList_1<UnityEngine::Vector2>, virtual System::Collections::IStructuralComparable, virtual System::Collections::IStructuralEquatable
	{
		Array1(decltype(nullptr));
		Array1(Plugin::InternalUse, int32_t handle);
		Array1(const Array1<UnityEngine::Vector2>& other);
		Array1(Array1<UnityEngine::Vector2>&& other);
		virtual ~Array1();
		Array1<UnityEngine::Vector2>& operator=(const Array1<UnityEngine::Vector2>& other);
		Array1<UnityEngine::Vector2>& operator=(decltype(nullptr));
		Array1<UnityEngine::Vector2>& operator=(Array1<UnityEngine::Vector2>&& other);
		bool operator==(const Array1<UnityEngine::Vector2>& other) const;
		bool operator!=(const Array1<UnityEngine::Vector2>& other) const;
		int32_t InternalLength;
		Array1(System::Int32 length0);
		System::Int32 GetLength();
		System::Int32 GetRank();
		Plugin::ArrayElementProxy1_1<UnityEngine::Vector2> operator[](int32_t index);
	};
}

namespace Plugin
{
	struct UnityEngineVector2Array1Iterator
	{
		System::Array1<UnityEngine::Vector2>& array;
		int index;
		UnityEngineVector2Array1Iterator(System::Array1<UnityEngine::Vector2>& array, int32_t index);
		UnityEngineVector2Array1Iterator& operator++();
		bool operator!=(const UnityEngineVector2Array1Iterator& other);
		UnityEngine::Vector2 operator*();
	};
}

namespace System
{
	Plugin::UnityEngineVector2Array1Iterator begin(System::Array1<UnityEngine::Vector2>& array);
	Plugin::UnityEngineVector2Array1Iterator end(System::Array1<UnityEngine::Vector2>& array);
}

namespace Plugin
{
	template<> struct ArrayElementProxy1_1<UnityEngine::Vector3>
	{
		int32_t Handle;
		int32_t Index0;
		ArrayElementProxy1_1<UnityEngine::Vector3>(Plugin::InternalUse, int32_t handle, int32_t index0);
		void operator=(UnityEngine::Vector3 item);
		operator UnityEngine::Vector3();
	};
}

namespace System
{
	template<> struct Array1<UnityEngine::Vector3> : virtual System::Array, virtual System::ICloneable, virtual System::Collections::IList, virtual System::Collections::Generic::IList_1<UnityEngine::Vector3>, virtual System::Collections::Generic::IReadOnlyList_1<UnityEngine::Vector3>, virtual System::Collections::IStructuralComparable, virtual System::Collections::IStructuralEquatable
	{
		Array1(decltype(nullptr));
		Array1(Plugin::InternalUse, int32_t handle);
		Array1(const Array1<UnityEngine::Vector3>& other);
		Array1(Array1<UnityEngine::Vector3>&& other);
		virtual ~Array1();
		Array1<UnityEngine::Vector3>& operator=(const Array1<UnityEngine::Vector3>& other);
		Array1<UnityEngine::Vector3>& operator=(decltype(nullptr));
		Array1<UnityEngine::Vector3>& operator=(Array1<UnityEngine::Vector3>&& other);
		bool operator==(const Array1<UnityEngine::Vector3>& other) const;
		bool operator!=(const Array1<UnityEngine::Vector3>& other) const;
		int32_t InternalLength;
		Array1(System::Int32 length0);
		System::Int32 GetLength();
		System::Int32 GetRank();
		Plugin::ArrayElementProxy1_1<UnityEngine::Vector3> operator[](int32_t index);
	};
}

namespace Plugin
{
	struct UnityEngineVector3Array1Iterator
	{
		System::Array1<UnityEngine::Vector3>& array;
		int index;
		UnityEngineVector3Array1Iterator(System::Array1<UnityEngine::Vector3>& array, int32_t index);
		UnityEngineVector3Array1Iterator& operator++();
		bool operator!=(const UnityEngineVector3Array1Iterator& other);
		UnityEngine::Vector3 operator*();
	};
}

namespace System
{
	Plugin::UnityEngineVector3Array1Iterator begin(System::Array1<UnityEngine::Vector3>& array);
	Plugin::UnityEngineVector3Array1Iterator end(System::Array1<UnityEngine::Vector3>& array);
}

namespace Plugin
{
	template<> struct ArrayElementProxy1_1<System::Byte>
	{
		int32_t Handle;
		int32_t Index0;
		ArrayElementProxy1_1<System::Byte>(Plugin::InternalUse, int32_t handle, int32_t index0);
		void operator=(System::Byte item);
		operator System::Byte();
	};
}

namespace System
{
	template<> struct Array1<System::Byte> : virtual System::Array, virtual System::ICloneable, virtual System::Collections::IList, virtual System::Collections::Generic::IList_1<System::Byte>, virtual System::Collections::Generic::IReadOnlyList_1<System::Byte>, virtual System::Collections::IStructuralComparable, virtual System::Collections::IStructuralEquatable
	{
		Array1(decltype(nullptr));
		Array1(Plugin::InternalUse, int32_t handle);
		Array1(const Array1<System::Byte>& other);
		Array1(Array1<System::Byte>&& other);
		virtual ~Array1();
		Array1<System::Byte>& operator=(const Array1<System::Byte>& other);
		Array1<System::Byte>& operator=(decltype(nullptr));
		Array1<System::Byte>& operator=(Array1<System::Byte>&& other);
		bool operator==(const Array1<System::Byte>& other) const;
		bool operator!=(const Array1<System::Byte>& other) const;
		int32_t InternalLength;
		Array1(System::Int32 length0);
		System::Int32 GetLength();
		System::Int32 GetRank();
		Plugin::ArrayElementProxy1_1<System::Byte> operator[](int32_t index);
	};
}

namespace Plugin
{
	struct SystemByteArray1Iterator
	{
		System::Array1<System::Byte>& array;
		int index;
		SystemByteArray1Iterator(System::Array1<System::Byte>& array, int32_t index);
		SystemByteArray1Iterator& operator++();
		bool operator!=(const SystemByteArray1Iterator& other);
		System::Byte operator*();
	};
}

namespace System
{
	Plugin::SystemByteArray1Iterator begin(System::Array1<System::Byte>& array);
	Plugin::SystemByteArray1Iterator end(System::Array1<System::Byte>& array);
}

namespace Plugin
{
	template<> struct ArrayElementProxy1_1<System::Int32>
	{
		int32_t Handle;
		int32_t Index0;
		ArrayElementProxy1_1<System::Int32>(Plugin::InternalUse, int32_t handle, int32_t index0);
		void operator=(System::Int32 item);
		operator System::Int32();
	};
}

namespace System
{
	template<> struct Array1<System::Int32> : virtual System::Array, virtual System::ICloneable, virtual System::Collections::IList, virtual System::Collections::Generic::IList_1<System::Int32>, virtual System::Collections::Generic::IReadOnlyList_1<System::Int32>, virtual System::Collections::IStructuralComparable, virtual System::Collections::IStructuralEquatable
	{
		Array1(decltype(nullptr));
		Array1(Plugin::InternalUse, int32_t handle);
		Array1(const Array1<System::Int32>& other);
		Array1(Array1<System::Int32>&& other);
		virtual ~Array1();
		Array1<System::Int32>& operator=(const Array1<System::Int32>& other);
		Array1<System::Int32>& operator=(decltype(nullptr));
		Array1<System::Int32>& operator=(Array1<System::Int32>&& other);
		bool operator==(const Array1<System::Int32>& other) const;
		bool operator!=(const Array1<System::Int32>& other) const;
		int32_t InternalLength;
		Array1(System::Int32 length0);
		System::Int32 GetLength();
		System::Int32 GetRank();
		Plugin::ArrayElementProxy1_1<System::Int32> operator[](int32_t index);
	};
}

namespace Plugin
{
	struct SystemInt32Array1Iterator
	{
		System::Array1<System::Int32>& array;
		int index;
		SystemInt32Array1Iterator(System::Array1<System::Int32>& array, int32_t index);
		SystemInt32Array1Iterator& operator++();
		bool operator!=(const SystemInt32Array1Iterator& other);
		System::Int32 operator*();
	};
}

namespace System
{
	Plugin::SystemInt32Array1Iterator begin(System::Array1<System::Int32>& array);
	Plugin::SystemInt32Array1Iterator end(System::Array1<System::Int32>& array);
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
