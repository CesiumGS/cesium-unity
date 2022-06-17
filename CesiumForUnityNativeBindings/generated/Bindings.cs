using AOT;

using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

using UnityEngine;

namespace NativeScript
{
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
	public static class Bindings
	{
		// Holds objects and provides handles to them in the form of ints
		public static class ObjectStore
		{
			// Lookup handles by object.
			static Dictionary<object, int> objectHandleCache;

			// Stored objects. The first is never used so 0 can be "null".
			static object[] objects;

			// Stack of available handles.
			static int[] handles;

			// Index of the next available handle
			static int nextHandleIndex;

			// The maximum number of objects to store. Must be positive.
			static int maxObjects;
			
			public static void Init(int maxObjects)
			{
				ObjectStore.maxObjects = maxObjects;
				objectHandleCache = new Dictionary<object, int>(maxObjects);	
				
				// Initialize the objects as all null plus room for the
				// first to always be null.
				objects = new object[maxObjects + 1];

				// Initialize the handles stack as 1, 2, 3, ...
				handles = new int[maxObjects];
				for (
					int i = 0, handle = maxObjects;
					i < maxObjects;
					++i, --handle)
				{
					handles[i] = handle;
				}
				nextHandleIndex = maxObjects - 1;
			}
			
			public static int Store(object obj)
			{
				// Null is always zero
				if (object.ReferenceEquals(obj, null))
				{
					return 0;
				}
				
				lock (objects)
				{
					// Pop a handle off the stack
					int handle = handles[nextHandleIndex];
					nextHandleIndex--;
					
					// Store the object
					objects[handle] = obj;
					objectHandleCache.Add(obj, handle);
					
					return handle;
				}
			}
			
			public static object Get(int handle)
			{
				return objects[handle];
			}
			
			public static int GetHandle(object obj)
			{
				// Null is always zero
				if (object.ReferenceEquals(obj, null))
				{
					return 0;
				}
				
				lock (objects)
				{
					int handle;

					// Get handle from object cache
					if (objectHandleCache.TryGetValue(obj, out handle))
					{
						return handle;
					}
				}
				
				// Object not found
				return Store(obj);
			}
			
			public static object Remove(int handle)
			{
				// Null is never stored, so there's nothing to remove
				if (handle == 0)
				{
					return null;
				}
				
				lock (objects)
				{
					// Forget the object
					object obj = objects[handle];
					objects[handle] = null;
					
					// Push the handle onto the stack
					nextHandleIndex++;
					handles[nextHandleIndex] = handle;

					// Remove the object from the cache
					objectHandleCache.Remove(obj);
					
					return obj;
				}
			}
		}
		
		// Holds structs and provides handles to them in the form of ints
		public static class StructStore<T>
			where T : struct
		{
			// Stored structs. The first is never used so 0 can be "null".
			static T[] structs;
			
			// Stack of available handles
			static int[] handles;
			
			// Index of the next available handle
			static int nextHandleIndex;
			
			public static void Init(int maxStructs)
			{
				// Initialize the objects as all default plus room for the
				// first to always be unused.
				structs = new T[maxStructs + 1];

				// Initialize the handles stack as 1, 2, 3, ...
				handles = new int[maxStructs];
				for (
					int i = 0, handle = maxStructs;
					i < maxStructs;
					++i, --handle)
				{
					handles[i] = handle;
				}
				nextHandleIndex = maxStructs - 1;
			}
			
			public static int Store(T structToStore)
			{
				lock (structs)
				{
					// Pop a handle off the stack
					int handle = handles[nextHandleIndex];
					nextHandleIndex--;
					
					// Store the struct
					structs[handle] = structToStore;
					
					return handle;
				}
			}
			
			public static void Replace(int handle, ref T structToStore)
			{
				structs[handle] = structToStore;
			}
			
			public static T Get(int handle)
			{
				return structs[handle];
			}
			
			public static void Remove(int handle)
			{
				if (handle != 0)
				{
					lock (structs)
					{
						// Forget the struct
						structs[handle] = default(T);

						// Push the handle onto the stack
						nextHandleIndex++;
						handles[nextHandleIndex] = handle;
					}
				}
			}
		}
		
		/// <summary>
		/// A reusable version of UnityEngine.WaitForSecondsRealtime to avoid
		/// GC allocs
		/// </summary>
		class ReusableWaitForSecondsRealtime : CustomYieldInstruction
		{
			private float waitTime;
			
			public float WaitTime
			{
				set
				{
					waitTime = Time.realtimeSinceStartup + value;
				}
			}

			public override bool keepWaiting
			{
				get
				{
					return Time.realtimeSinceStartup < waitTime;
				}
			}

			public ReusableWaitForSecondsRealtime(float time)
			{
				WaitTime = time;
			}
		}

		public enum DestroyFunction
		{
			/*BEGIN DESTROY FUNCTION ENUMERATORS*/
			BaseNativeDownloadHandler,
			BaseCesium3DTileset
			/*END DESTROY FUNCTION ENUMERATORS*/
		}

		struct DestroyEntry
		{
			public DestroyFunction Function;
			public int CppHandle;

			public DestroyEntry(DestroyFunction function, int cppHandle)
			{
				Function = function;
				CppHandle = cppHandle;
			}
		}
		
		// Name of the plugin when using [DllImport]
#if !UNITY_EDITOR && UNITY_IOS
		const string PLUGIN_NAME = "__Internal";
#else
		const string PLUGIN_NAME = "CesiumForUnityNative";
#endif
		
		// Path to load the plugin from when running inside the editor
#if UNITY_EDITOR_OSX
		const string PLUGIN_PATH = "/Plugins/Editor/CesiumForUnityNative.bundle/Contents/MacOS/CesiumForUnityNative";
#elif UNITY_EDITOR_LINUX
		const string PLUGIN_PATH = "/Plugins/Editor/libCesiumForUnityNative.so";
#elif UNITY_EDITOR_WIN
		const string PLUGIN_PATH = "/CesiumForUnityNative.dll";
		const string PLUGIN_TEMP_PATH = "/CesiumForUnityNative_temp.dll";
#endif

		enum InitMode : byte
		{
			FirstBoot,
			Reload
		}
		
#if UNITY_EDITOR
		// Handle to the C++ DLL
		static IntPtr libraryHandle;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void InitDelegate(
			IntPtr memory,
			int memorySize,
			InitMode initMode);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void SetCsharpExceptionDelegate(int handle);
		
		/*BEGIN CPP DELEGATES*/
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public unsafe delegate int NewBaseNativeDownloadHandlerDelegateType(int param0);
		public static NewBaseNativeDownloadHandlerDelegateType NewBaseNativeDownloadHandler;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public unsafe delegate void DestroyBaseNativeDownloadHandlerDelegateType(int param0);
		public static DestroyBaseNativeDownloadHandlerDelegateType DestroyBaseNativeDownloadHandler;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public unsafe delegate bool CesiumForUnityAbstractBaseNativeDownloadHandlerReceiveDataNativeDelegateType(int thisHandle, System.IntPtr param0, int param1);
		public static CesiumForUnityAbstractBaseNativeDownloadHandlerReceiveDataNativeDelegateType CesiumForUnityAbstractBaseNativeDownloadHandlerReceiveDataNative;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public unsafe delegate int NewBaseCesium3DTilesetDelegateType(int param0);
		public static NewBaseCesium3DTilesetDelegateType NewBaseCesium3DTileset;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public unsafe delegate void DestroyBaseCesium3DTilesetDelegateType(int param0);
		public static DestroyBaseCesium3DTilesetDelegateType DestroyBaseCesium3DTileset;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public unsafe delegate void CesiumForUnityAbstractBaseCesium3DTilesetStartDelegateType(int thisHandle);
		public static CesiumForUnityAbstractBaseCesium3DTilesetStartDelegateType CesiumForUnityAbstractBaseCesium3DTilesetStart;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public unsafe delegate void CesiumForUnityAbstractBaseCesium3DTilesetUpdateDelegateType(int thisHandle);
		public static CesiumForUnityAbstractBaseCesium3DTilesetUpdateDelegateType CesiumForUnityAbstractBaseCesium3DTilesetUpdate;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public unsafe delegate void SystemActionNativeInvokeDelegateType(int thisHandle);
		public static SystemActionNativeInvokeDelegateType SystemActionNativeInvoke;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public unsafe delegate void SystemActionUnityEngineAsyncOperationNativeInvokeDelegateType(int thisHandle, int param0);
		public static SystemActionUnityEngineAsyncOperationNativeInvokeDelegateType SystemActionUnityEngineAsyncOperationNativeInvoke;
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public unsafe delegate void SetCsharpExceptionSystemNullReferenceExceptionDelegateType(int param0);
		public static SetCsharpExceptionSystemNullReferenceExceptionDelegateType SetCsharpExceptionSystemNullReferenceException;
		/*END CPP DELEGATES*/
#endif

#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr dlopen(
			string path,
			int flag);

		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr dlsym(
			IntPtr handle,
			string symbolName);

		[DllImport("__Internal", CallingConvention = CallingConvention.Cdecl)]
		static extern int dlclose(
			IntPtr handle);

		static IntPtr OpenLibrary(
			string path)
		{
			IntPtr handle = dlopen(path, 1); // 1 = lazy, 2 = now
			if (handle == IntPtr.Zero)
			{
				throw new Exception("Couldn't open native library: " + path);
			}
			return handle;
		}
		
		static void CloseLibrary(
			IntPtr libraryHandle)
		{
			dlclose(libraryHandle);
		}
		
		static T GetDelegate<T>(
			IntPtr libraryHandle,
			string functionName) where T : class
		{
			IntPtr symbol = dlsym(libraryHandle, functionName);
			if (symbol == IntPtr.Zero)
			{
				throw new Exception("Couldn't get function: " + functionName);
			}
			return Marshal.GetDelegateForFunctionPointer(
				symbol,
				typeof(T)) as T;
		}
#elif UNITY_EDITOR_WIN
		[DllImport("kernel32", SetLastError=true, CharSet = CharSet.Ansi)]
		static extern IntPtr LoadLibrary(
			string path);
		
		[DllImport("kernel32", CharSet=CharSet.Ansi, ExactSpelling=true, SetLastError=true)]
		static extern IntPtr GetProcAddress(
			IntPtr libraryHandle,
			string symbolName);
		
		[DllImport("kernel32.dll", SetLastError=true)]
		static extern bool FreeLibrary(
			IntPtr libraryHandle);
		
		static IntPtr OpenLibrary(string path)
		{
			IntPtr handle = LoadLibrary(path);
			if (handle == IntPtr.Zero)
			{
				throw new Exception("Couldn't open native library: " + path);
			}
			return handle;
		}
		
		static void CloseLibrary(IntPtr libraryHandle)
		{
			FreeLibrary(libraryHandle);
		}
		
		static T GetDelegate<T>(
			IntPtr libraryHandle,
			string functionName) where T : class
		{
			IntPtr symbol = GetProcAddress(libraryHandle, functionName);
			if (symbol == IntPtr.Zero)
			{
				throw new Exception("Couldn't get function: " + functionName);
			}
			return Marshal.GetDelegateForFunctionPointer(
				symbol,
				typeof(T)) as T;
		}
#else
		[DllImport(PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
		static extern void Init(
			IntPtr memory,
			int memorySize,
			InitMode initMode);
		
		[DllImport(PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
		static extern void SetCsharpException(int handle);
		
		/*BEGIN IMPORTS*/
		[DllImport(PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int NewBaseNativeDownloadHandler(int thisHandle);
		
		[DllImport(PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DestroyBaseNativeDownloadHandler(int thisHandle);
		
		[DllImport(PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool CesiumForUnityAbstractBaseNativeDownloadHandlerReceiveDataNative(int thisHandle, System.IntPtr param0, int param1);
		
		[DllImport(PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern int NewBaseCesium3DTileset(int thisHandle);
		
		[DllImport(PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void DestroyBaseCesium3DTileset(int thisHandle);
		
		[DllImport(PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void CesiumForUnityAbstractBaseCesium3DTilesetStart(int thisHandle);
		
		[DllImport(PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void CesiumForUnityAbstractBaseCesium3DTilesetUpdate(int thisHandle);
		
		[DllImport(PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SystemActionNativeInvoke(int thisHandle);
		
		[DllImport(PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SystemActionUnityEngineAsyncOperationNativeInvoke(int thisHandle, int param0);
		
		[DllImport(PLUGIN_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void SetCsharpExceptionSystemNullReferenceException(int thisHandle);
		/*END IMPORTS*/
#endif
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void ReleaseObjectDelegateType(int handle);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int StringNewDelegateType(string chars);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void SetExceptionDelegateType(int handle);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int ArrayGetLengthDelegateType(int handle);
		
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int EnumerableGetEnumeratorDelegateType(int handle);
		
		/*BEGIN DELEGATE TYPES*/
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void SystemIDisposableMethodDisposeDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void ReleaseSystemDecimalDelegateType(int handle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int SystemDecimalConstructorSystemDoubleDelegateType(double value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int SystemDecimalConstructorSystemUInt64DelegateType(ulong value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxDecimalDelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnboxDecimalDelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Vector2 UnityEngineVector2ConstructorSystemSingle_SystemSingleDelegateType(float x, float y);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxVector2DelegateType(ref UnityEngine.Vector2 val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Vector2 UnboxVector2DelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Vector3 UnityEngineVector3ConstructorSystemSingle_SystemSingle_SystemSingleDelegateType(float x, float y, float z);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxVector3DelegateType(ref UnityEngine.Vector3 val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Vector3 UnboxVector3DelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Vector4 UnityEngineVector4ConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingleDelegateType(float x, float y, float z, float w);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxVector4DelegateType(ref UnityEngine.Vector4 val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Vector4 UnboxVector4DelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Quaternion UnityEngineQuaternionConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingleDelegateType(float x, float y, float z, float w);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxQuaternionDelegateType(ref UnityEngine.Quaternion val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Quaternion UnboxQuaternionDelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Matrix4x4 UnityEngineMatrix4x4ConstructorUnityEngineVector4_UnityEngineVector4_UnityEngineVector4_UnityEngineVector4DelegateType(ref UnityEngine.Vector4 column0, ref UnityEngine.Vector4 column1, ref UnityEngine.Vector4 column2, ref UnityEngine.Vector4 column3);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Quaternion UnityEngineMatrix4x4PropertyGetRotationDelegateType(ref UnityEngine.Matrix4x4 thiz);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Vector3 UnityEngineMatrix4x4PropertyGetLossyScaleDelegateType(ref UnityEngine.Matrix4x4 thiz);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Vector3 UnityEngineMatrix4x4MethodGetPositionDelegateType(ref UnityEngine.Matrix4x4 thiz);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxMatrix4x4DelegateType(ref UnityEngine.Matrix4x4 val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Matrix4x4 UnboxMatrix4x4DelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineObjectPropertyGetNameDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineObjectPropertySetNameDelegateType(int thisHandle, int valueHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineComponentPropertyGetTransformDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Vector3 UnityEngineTransformPropertyGetPositionDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineTransformPropertySetPositionDelegateType(int thisHandle, ref UnityEngine.Vector3 value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Quaternion UnityEngineTransformPropertyGetRotationDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineTransformPropertySetRotationDelegateType(int thisHandle, ref UnityEngine.Quaternion value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Vector3 UnityEngineTransformPropertyGetLocalScaleDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineTransformPropertySetLocalScaleDelegateType(int thisHandle, ref UnityEngine.Vector3 value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Matrix4x4 UnityEngineTransformPropertyGetLocalToWorldMatrixDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineTransformPropertyGetParentDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineTransformPropertySetParentDelegateType(int thisHandle, int valueHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Vector3 UnityEngineTransformPropertyGetForwardDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineTransformPropertySetForwardDelegateType(int thisHandle, ref UnityEngine.Vector3 value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Vector3 UnityEngineTransformPropertyGetUpDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineTransformPropertySetUpDelegateType(int thisHandle, ref UnityEngine.Vector3 value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int SystemCollectionsIEnumeratorPropertyGetCurrentDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate bool SystemCollectionsIEnumeratorMethodMoveNextDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineGameObjectConstructorSystemStringDelegateType(int nameHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineGameObjectPropertyGetTransformDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineGameObjectMethodAddComponentCesiumForUnityBaseCesium3DTilesetDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineGameObjectMethodAddComponentUnityEngineMeshFilterDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineGameObjectMethodAddComponentUnityEngineMeshRendererDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveTypeDelegateType(UnityEngine.PrimitiveType type);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineGameObjectMethodSetActiveSystemBooleanDelegateType(int thisHandle, bool value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineDebugMethodLogSystemObjectDelegateType(int messageHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineMonoBehaviourPropertyGetTransformDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineMonoBehaviourPropertyGetGameObjectDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Vector2 SystemCollectionsGenericIEnumeratorUnityEngineVector2PropertyGetCurrentDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Vector3 SystemCollectionsGenericIEnumeratorUnityEngineVector3PropertyGetCurrentDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate byte SystemCollectionsGenericIEnumeratorSystemBytePropertyGetCurrentDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int SystemCollectionsGenericIEnumeratorSystemInt32PropertyGetCurrentDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int SystemCollectionsGenericIEnumerableUnityEngineVector2MethodGetEnumeratorDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int SystemCollectionsGenericIEnumerableUnityEngineVector3MethodGetEnumeratorDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int SystemCollectionsGenericIEnumerableSystemByteMethodGetEnumeratorDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int SystemCollectionsGenericIEnumerableSystemInt32MethodGetEnumeratorDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxNativeArrayOptionsDelegateType(Unity.Collections.NativeArrayOptions val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate Unity.Collections.NativeArrayOptions UnboxNativeArrayOptionsDelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void ReleaseUnityCollectionsNativeArraySystemByteDelegateType(int handle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityCollectionsNativeArraySystemByteConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegateType(int length, Unity.Collections.Allocator allocator, Unity.Collections.NativeArrayOptions options);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityCollectionsNativeArraySystemByteMethodDisposeDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxNativeArraySystemByteDelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnboxNativeArraySystemByteDelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void ReleaseUnityCollectionsNativeArrayUnityEngineVector3DelegateType(int handle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityCollectionsNativeArrayUnityEngineVector3ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegateType(int length, Unity.Collections.Allocator allocator, Unity.Collections.NativeArrayOptions options);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityCollectionsNativeArrayUnityEngineVector3MethodDisposeDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxNativeArrayUnityEngineVector3DelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnboxNativeArrayUnityEngineVector3DelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void ReleaseUnityCollectionsNativeArrayUnityEngineVector2DelegateType(int handle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityCollectionsNativeArrayUnityEngineVector2ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegateType(int length, Unity.Collections.Allocator allocator, Unity.Collections.NativeArrayOptions options);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityCollectionsNativeArrayUnityEngineVector2MethodDisposeDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxNativeArrayUnityEngineVector2DelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnboxNativeArrayUnityEngineVector2DelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void ReleaseUnityCollectionsNativeArraySystemInt32DelegateType(int handle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityCollectionsNativeArraySystemInt32ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegateType(int length, Unity.Collections.Allocator allocator, Unity.Collections.NativeArrayOptions options);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityCollectionsNativeArraySystemInt32MethodDisposeDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxNativeArraySystemInt32DelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnboxNativeArraySystemInt32DelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxAllocatorDelegateType(Unity.Collections.Allocator val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate Unity.Collections.Allocator UnboxAllocatorDelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArraySystemByteSystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegateType(void* dataPointer, int length, Unity.Collections.Allocator allocator);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArrayUnityEngineVector3SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegateType(void* dataPointer, int length, Unity.Collections.Allocator allocator);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArrayUnityEngineVector2SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegateType(void* dataPointer, int length, Unity.Collections.Allocator allocator);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArraySystemInt32SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegateType(void* dataPointer, int length, Unity.Collections.Allocator allocator);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void* UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksUnityEngineVector3UnityCollectionsNativeArrayDelegateType(int nativeArrayHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void* UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksUnityEngineVector2UnityCollectionsNativeArrayDelegateType(int nativeArrayHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void* UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksSystemInt32UnityCollectionsNativeArrayDelegateType(int nativeArrayHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxMeshTopologyDelegateType(UnityEngine.MeshTopology val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.MeshTopology UnboxMeshTopologyDelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineMeshConstructorDelegateType();
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineMeshMethodSetVerticesUnityEngineVector3Array1DelegateType(int thisHandle, int inVerticesHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineMeshMethodSetVerticesUnityEngineVector3UnityCollectionsNativeArrayDelegateType(int thisHandle, int inVerticesHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineMeshMethodSetNormalsUnityEngineVector3Array1DelegateType(int thisHandle, int inNormalsHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineMeshMethodSetNormalsUnityEngineVector3UnityCollectionsNativeArrayDelegateType(int thisHandle, int inNormalsHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineMeshMethodSetTrianglesSystemInt32Array1_SystemInt32_SystemBoolean_SystemInt32DelegateType(int thisHandle, int trianglesHandle, int submesh, bool calculateBounds, int baseVertex);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineMeshMethodSetIndicesSystemInt32UnityCollectionsNativeArray_UnityEngineMeshTopology_SystemInt32_SystemBoolean_SystemInt32DelegateType(int thisHandle, int indicesHandle, UnityEngine.MeshTopology topology, int submesh, bool calculateBounds, int baseVertex);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineMeshMethodSetUVsSystemInt32_UnityEngineVector2Array1DelegateType(int thisHandle, int channel, int uvsHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineMeshMethodSetUVsUnityEngineVector2SystemInt32_UnityCollectionsNativeArrayDelegateType(int thisHandle, int channel, int uvsHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineMeshFilterPropertyGetMeshDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineMeshFilterPropertySetMeshDelegateType(int thisHandle, int valueHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineMeshRendererPropertyGetMaterialDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineMeshRendererPropertySetMaterialDelegateType(int thisHandle, int valueHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int SystemExceptionConstructorSystemStringDelegateType(int messageHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxPrimitiveTypeDelegateType(UnityEngine.PrimitiveType val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.PrimitiveType UnboxPrimitiveTypeDelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate float UnityEngineTimePropertyGetDeltaTimeDelegateType();
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineCameraPropertyGetMainDelegateType();
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate float UnityEngineCameraPropertyGetFieldOfViewDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineCameraPropertySetFieldOfViewDelegateType(int thisHandle, float value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate float UnityEngineCameraPropertyGetAspectDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineCameraPropertySetAspectDelegateType(int thisHandle, float value);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineCameraPropertyGetPixelWidthDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineCameraPropertyGetPixelHeightDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxRawDownloadedDataDelegateType(ref CesiumForUnity.RawDownloadedData val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate CesiumForUnity.RawDownloadedData UnboxRawDownloadedDataDelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void BaseNativeDownloadHandlerConstructorDelegateType(int cppHandle, ref int handle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void ReleaseBaseNativeDownloadHandlerDelegateType(int handle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineNetworkingUnityWebRequestPropertyGetErrorDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate bool UnityEngineNetworkingUnityWebRequestPropertyGetIsDoneDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate long UnityEngineNetworkingUnityWebRequestPropertyGetResponseCodeDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineNetworkingUnityWebRequestPropertyGetUrlDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineNetworkingUnityWebRequestPropertySetUrlDelegateType(int thisHandle, int valueHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineNetworkingUnityWebRequestPropertyGetMethodDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineNetworkingUnityWebRequestPropertySetMethodDelegateType(int thisHandle, int valueHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineNetworkingUnityWebRequestPropertyGetDownloadHandlerDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineNetworkingUnityWebRequestPropertySetDownloadHandlerDelegateType(int thisHandle, int valueHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineNetworkingUnityWebRequestMethodGetSystemStringDelegateType(int uriHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineNetworkingUnityWebRequestMethodSetRequestHeaderSystemString_SystemStringDelegateType(int thisHandle, int nameHandle, int valueHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineNetworkingUnityWebRequestMethodSendWebRequestDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineNetworkingUnityWebRequestMethodGetResponseHeaderSystemStringDelegateType(int thisHandle, int nameHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineNetworkingUnityWebRequestAsyncOperationAddEventCompletedDelegateType(int thisHandle, int delHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineNetworkingUnityWebRequestAsyncOperationRemoveEventCompletedDelegateType(int thisHandle, int delHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate System.IntPtr SystemRuntimeInteropServicesMarshalMethodStringToCoTaskMemUTF8SystemStringDelegateType(int sHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void SystemRuntimeInteropServicesMarshalMethodFreeCoTaskMemSystemIntPtrDelegateType(System.IntPtr ptr);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void BaseCesium3DTilesetConstructorDelegateType(int cppHandle, ref int handle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void ReleaseBaseCesium3DTilesetDelegateType(int handle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int SystemThreadingTasksTaskMethodRunSystemActionDelegateType(int actionHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineMaterialMethodSetTextureSystemString_UnityEngineTextureDelegateType(int thisHandle, int nameHandle, int valueHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineResourcesMethodLoadUnityEngineMaterialSystemStringDelegateType(int pathHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEditorSceneViewPropertyGetLastActiveSceneViewDelegateType();
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEditorSceneViewPropertyGetCameraDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxTextureFormatDelegateType(UnityEngine.TextureFormat val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.TextureFormat UnboxTextureFormatDelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineTexture2DConstructorSystemInt32_SystemInt32_UnityEngineTextureFormat_SystemBoolean_SystemBooleanDelegateType(int width, int height, UnityEngine.TextureFormat textureFormat, bool mipChain, bool linear);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineTexture2DMethodSetPixelDataSystemByteSystemByteArray1_SystemInt32_SystemInt32DelegateType(int thisHandle, int dataHandle, int mipLevel, int sourceDataStartIndex);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineTexture2DMethodSetPixelDataSystemByteUnityCollectionsNativeArray_SystemInt32_SystemInt32DelegateType(int thisHandle, int dataHandle, int mipLevel, int sourceDataStartIndex);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineTexture2DMethodApplySystemBoolean_SystemBooleanDelegateType(int thisHandle, bool updateMipmaps, bool makeNoLongerReadable);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineTexture2DMethodLoadRawTextureDataSystemIntPtr_SystemInt32DelegateType(int thisHandle, System.IntPtr data, int size);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineApplicationPropertyGetTemporaryCachePathDelegateType();
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineApplicationPropertyGetPersistentDataPathDelegateType();
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxBooleanDelegateType(bool val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate bool UnboxBooleanDelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxSByteDelegateType(sbyte val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate sbyte UnboxSByteDelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxByteDelegateType(byte val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate byte UnboxByteDelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxInt16DelegateType(short val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate short UnboxInt16DelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxUInt16DelegateType(ushort val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate ushort UnboxUInt16DelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxInt32DelegateType(int val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnboxInt32DelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxUInt32DelegateType(uint val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate uint UnboxUInt32DelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxInt64DelegateType(long val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate long UnboxInt64DelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxUInt64DelegateType(ulong val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate ulong UnboxUInt64DelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxCharDelegateType(char val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate char UnboxCharDelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxSingleDelegateType(float val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate float UnboxSingleDelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int BoxDoubleDelegateType(double val);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate double UnboxDoubleDelegateType(int valHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineUnityEngineVector2Array1Constructor1DelegateType(int length0);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Vector2 UnityEngineVector2Array1GetItem1DelegateType(int thisHandle, int index0);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineVector2Array1SetItem1DelegateType(int thisHandle, int index0, ref UnityEngine.Vector2 item);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int UnityEngineUnityEngineVector3Array1Constructor1DelegateType(int length0);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate UnityEngine.Vector3 UnityEngineVector3Array1GetItem1DelegateType(int thisHandle, int index0);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void UnityEngineVector3Array1SetItem1DelegateType(int thisHandle, int index0, ref UnityEngine.Vector3 item);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int SystemSystemByteArray1Constructor1DelegateType(int length0);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate byte SystemByteArray1GetItem1DelegateType(int thisHandle, int index0);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void SystemByteArray1SetItem1DelegateType(int thisHandle, int index0, byte item);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int SystemSystemInt32Array1Constructor1DelegateType(int length0);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate int SystemInt32Array1GetItem1DelegateType(int thisHandle, int index0);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void SystemInt32Array1SetItem1DelegateType(int thisHandle, int index0, int item);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void SystemActionInvokeDelegateType(int thisHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void SystemActionConstructorDelegateType(int cppHandle, ref int handle, ref int classHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void ReleaseSystemActionDelegateType(int handle, int classHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void SystemActionAddDelegateType(int thisHandle, int delHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void SystemActionRemoveDelegateType(int thisHandle, int delHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void SystemActionUnityEngineAsyncOperationInvokeDelegateType(int thisHandle, int objHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void SystemActionUnityEngineAsyncOperationConstructorDelegateType(int cppHandle, ref int handle, ref int classHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void ReleaseSystemActionUnityEngineAsyncOperationDelegateType(int handle, int classHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void SystemActionUnityEngineAsyncOperationAddDelegateType(int thisHandle, int delHandle);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		unsafe delegate void SystemActionUnityEngineAsyncOperationRemoveDelegateType(int thisHandle, int delHandle);
		/*END DELEGATE TYPES*/

#if UNITY_EDITOR_WIN
		private static readonly string pluginTempPath = Application.dataPath + PLUGIN_TEMP_PATH;
#endif
		public static Exception UnhandledCppException;
#if UNITY_EDITOR
		private static readonly string pluginPath = Application.dataPath + PLUGIN_PATH;
		public static SetCsharpExceptionDelegate SetCsharpException;
#endif
		static IntPtr memory;
		static int memorySize;
		static DestroyEntry[] destroyQueue;
		static int destroyQueueCount;
		static int destroyQueueCapacity;
		static object destroyQueueLockObj;
		
		// Fixed delegates
		static readonly ReleaseObjectDelegateType ReleaseObjectDelegate = new ReleaseObjectDelegateType(ReleaseObject);
		static readonly StringNewDelegateType StringNewDelegate = new StringNewDelegateType(StringNew);
		static readonly SetExceptionDelegateType SetExceptionDelegate = new SetExceptionDelegateType(SetException);
		static readonly ArrayGetLengthDelegateType ArrayGetLengthDelegate = new ArrayGetLengthDelegateType(ArrayGetLength);
		static readonly EnumerableGetEnumeratorDelegateType EnumerableGetEnumeratorDelegate = new EnumerableGetEnumeratorDelegateType(EnumerableGetEnumerator);
		
		// Generated delegates
		/*BEGIN CSHARP DELEGATES*/
		static unsafe readonly SystemIDisposableMethodDisposeDelegateType SystemIDisposableMethodDisposeDelegate = new SystemIDisposableMethodDisposeDelegateType(SystemIDisposableMethodDispose);
		static unsafe readonly ReleaseSystemDecimalDelegateType ReleaseSystemDecimalDelegate = new ReleaseSystemDecimalDelegateType(ReleaseSystemDecimal);
		static unsafe readonly SystemDecimalConstructorSystemDoubleDelegateType SystemDecimalConstructorSystemDoubleDelegate = new SystemDecimalConstructorSystemDoubleDelegateType(SystemDecimalConstructorSystemDouble);
		static unsafe readonly SystemDecimalConstructorSystemUInt64DelegateType SystemDecimalConstructorSystemUInt64Delegate = new SystemDecimalConstructorSystemUInt64DelegateType(SystemDecimalConstructorSystemUInt64);
		static unsafe readonly BoxDecimalDelegateType BoxDecimalDelegate = new BoxDecimalDelegateType(BoxDecimal);
		static unsafe readonly UnboxDecimalDelegateType UnboxDecimalDelegate = new UnboxDecimalDelegateType(UnboxDecimal);
		static unsafe readonly UnityEngineVector2ConstructorSystemSingle_SystemSingleDelegateType UnityEngineVector2ConstructorSystemSingle_SystemSingleDelegate = new UnityEngineVector2ConstructorSystemSingle_SystemSingleDelegateType(UnityEngineVector2ConstructorSystemSingle_SystemSingle);
		static unsafe readonly BoxVector2DelegateType BoxVector2Delegate = new BoxVector2DelegateType(BoxVector2);
		static unsafe readonly UnboxVector2DelegateType UnboxVector2Delegate = new UnboxVector2DelegateType(UnboxVector2);
		static unsafe readonly UnityEngineVector3ConstructorSystemSingle_SystemSingle_SystemSingleDelegateType UnityEngineVector3ConstructorSystemSingle_SystemSingle_SystemSingleDelegate = new UnityEngineVector3ConstructorSystemSingle_SystemSingle_SystemSingleDelegateType(UnityEngineVector3ConstructorSystemSingle_SystemSingle_SystemSingle);
		static unsafe readonly BoxVector3DelegateType BoxVector3Delegate = new BoxVector3DelegateType(BoxVector3);
		static unsafe readonly UnboxVector3DelegateType UnboxVector3Delegate = new UnboxVector3DelegateType(UnboxVector3);
		static unsafe readonly UnityEngineVector4ConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingleDelegateType UnityEngineVector4ConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingleDelegate = new UnityEngineVector4ConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingleDelegateType(UnityEngineVector4ConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingle);
		static unsafe readonly BoxVector4DelegateType BoxVector4Delegate = new BoxVector4DelegateType(BoxVector4);
		static unsafe readonly UnboxVector4DelegateType UnboxVector4Delegate = new UnboxVector4DelegateType(UnboxVector4);
		static unsafe readonly UnityEngineQuaternionConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingleDelegateType UnityEngineQuaternionConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingleDelegate = new UnityEngineQuaternionConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingleDelegateType(UnityEngineQuaternionConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingle);
		static unsafe readonly BoxQuaternionDelegateType BoxQuaternionDelegate = new BoxQuaternionDelegateType(BoxQuaternion);
		static unsafe readonly UnboxQuaternionDelegateType UnboxQuaternionDelegate = new UnboxQuaternionDelegateType(UnboxQuaternion);
		static unsafe readonly UnityEngineMatrix4x4ConstructorUnityEngineVector4_UnityEngineVector4_UnityEngineVector4_UnityEngineVector4DelegateType UnityEngineMatrix4x4ConstructorUnityEngineVector4_UnityEngineVector4_UnityEngineVector4_UnityEngineVector4Delegate = new UnityEngineMatrix4x4ConstructorUnityEngineVector4_UnityEngineVector4_UnityEngineVector4_UnityEngineVector4DelegateType(UnityEngineMatrix4x4ConstructorUnityEngineVector4_UnityEngineVector4_UnityEngineVector4_UnityEngineVector4);
		static unsafe readonly UnityEngineMatrix4x4PropertyGetRotationDelegateType UnityEngineMatrix4x4PropertyGetRotationDelegate = new UnityEngineMatrix4x4PropertyGetRotationDelegateType(UnityEngineMatrix4x4PropertyGetRotation);
		static unsafe readonly UnityEngineMatrix4x4PropertyGetLossyScaleDelegateType UnityEngineMatrix4x4PropertyGetLossyScaleDelegate = new UnityEngineMatrix4x4PropertyGetLossyScaleDelegateType(UnityEngineMatrix4x4PropertyGetLossyScale);
		static unsafe readonly UnityEngineMatrix4x4MethodGetPositionDelegateType UnityEngineMatrix4x4MethodGetPositionDelegate = new UnityEngineMatrix4x4MethodGetPositionDelegateType(UnityEngineMatrix4x4MethodGetPosition);
		static unsafe readonly BoxMatrix4x4DelegateType BoxMatrix4x4Delegate = new BoxMatrix4x4DelegateType(BoxMatrix4x4);
		static unsafe readonly UnboxMatrix4x4DelegateType UnboxMatrix4x4Delegate = new UnboxMatrix4x4DelegateType(UnboxMatrix4x4);
		static unsafe readonly UnityEngineObjectPropertyGetNameDelegateType UnityEngineObjectPropertyGetNameDelegate = new UnityEngineObjectPropertyGetNameDelegateType(UnityEngineObjectPropertyGetName);
		static unsafe readonly UnityEngineObjectPropertySetNameDelegateType UnityEngineObjectPropertySetNameDelegate = new UnityEngineObjectPropertySetNameDelegateType(UnityEngineObjectPropertySetName);
		static unsafe readonly UnityEngineComponentPropertyGetTransformDelegateType UnityEngineComponentPropertyGetTransformDelegate = new UnityEngineComponentPropertyGetTransformDelegateType(UnityEngineComponentPropertyGetTransform);
		static unsafe readonly UnityEngineTransformPropertyGetPositionDelegateType UnityEngineTransformPropertyGetPositionDelegate = new UnityEngineTransformPropertyGetPositionDelegateType(UnityEngineTransformPropertyGetPosition);
		static unsafe readonly UnityEngineTransformPropertySetPositionDelegateType UnityEngineTransformPropertySetPositionDelegate = new UnityEngineTransformPropertySetPositionDelegateType(UnityEngineTransformPropertySetPosition);
		static unsafe readonly UnityEngineTransformPropertyGetRotationDelegateType UnityEngineTransformPropertyGetRotationDelegate = new UnityEngineTransformPropertyGetRotationDelegateType(UnityEngineTransformPropertyGetRotation);
		static unsafe readonly UnityEngineTransformPropertySetRotationDelegateType UnityEngineTransformPropertySetRotationDelegate = new UnityEngineTransformPropertySetRotationDelegateType(UnityEngineTransformPropertySetRotation);
		static unsafe readonly UnityEngineTransformPropertyGetLocalScaleDelegateType UnityEngineTransformPropertyGetLocalScaleDelegate = new UnityEngineTransformPropertyGetLocalScaleDelegateType(UnityEngineTransformPropertyGetLocalScale);
		static unsafe readonly UnityEngineTransformPropertySetLocalScaleDelegateType UnityEngineTransformPropertySetLocalScaleDelegate = new UnityEngineTransformPropertySetLocalScaleDelegateType(UnityEngineTransformPropertySetLocalScale);
		static unsafe readonly UnityEngineTransformPropertyGetLocalToWorldMatrixDelegateType UnityEngineTransformPropertyGetLocalToWorldMatrixDelegate = new UnityEngineTransformPropertyGetLocalToWorldMatrixDelegateType(UnityEngineTransformPropertyGetLocalToWorldMatrix);
		static unsafe readonly UnityEngineTransformPropertyGetParentDelegateType UnityEngineTransformPropertyGetParentDelegate = new UnityEngineTransformPropertyGetParentDelegateType(UnityEngineTransformPropertyGetParent);
		static unsafe readonly UnityEngineTransformPropertySetParentDelegateType UnityEngineTransformPropertySetParentDelegate = new UnityEngineTransformPropertySetParentDelegateType(UnityEngineTransformPropertySetParent);
		static unsafe readonly UnityEngineTransformPropertyGetForwardDelegateType UnityEngineTransformPropertyGetForwardDelegate = new UnityEngineTransformPropertyGetForwardDelegateType(UnityEngineTransformPropertyGetForward);
		static unsafe readonly UnityEngineTransformPropertySetForwardDelegateType UnityEngineTransformPropertySetForwardDelegate = new UnityEngineTransformPropertySetForwardDelegateType(UnityEngineTransformPropertySetForward);
		static unsafe readonly UnityEngineTransformPropertyGetUpDelegateType UnityEngineTransformPropertyGetUpDelegate = new UnityEngineTransformPropertyGetUpDelegateType(UnityEngineTransformPropertyGetUp);
		static unsafe readonly UnityEngineTransformPropertySetUpDelegateType UnityEngineTransformPropertySetUpDelegate = new UnityEngineTransformPropertySetUpDelegateType(UnityEngineTransformPropertySetUp);
		static unsafe readonly SystemCollectionsIEnumeratorPropertyGetCurrentDelegateType SystemCollectionsIEnumeratorPropertyGetCurrentDelegate = new SystemCollectionsIEnumeratorPropertyGetCurrentDelegateType(SystemCollectionsIEnumeratorPropertyGetCurrent);
		static unsafe readonly SystemCollectionsIEnumeratorMethodMoveNextDelegateType SystemCollectionsIEnumeratorMethodMoveNextDelegate = new SystemCollectionsIEnumeratorMethodMoveNextDelegateType(SystemCollectionsIEnumeratorMethodMoveNext);
		static unsafe readonly UnityEngineGameObjectConstructorSystemStringDelegateType UnityEngineGameObjectConstructorSystemStringDelegate = new UnityEngineGameObjectConstructorSystemStringDelegateType(UnityEngineGameObjectConstructorSystemString);
		static unsafe readonly UnityEngineGameObjectPropertyGetTransformDelegateType UnityEngineGameObjectPropertyGetTransformDelegate = new UnityEngineGameObjectPropertyGetTransformDelegateType(UnityEngineGameObjectPropertyGetTransform);
		static unsafe readonly UnityEngineGameObjectMethodAddComponentCesiumForUnityBaseCesium3DTilesetDelegateType UnityEngineGameObjectMethodAddComponentCesiumForUnityBaseCesium3DTilesetDelegate = new UnityEngineGameObjectMethodAddComponentCesiumForUnityBaseCesium3DTilesetDelegateType(UnityEngineGameObjectMethodAddComponentCesiumForUnityBaseCesium3DTileset);
		static unsafe readonly UnityEngineGameObjectMethodAddComponentUnityEngineMeshFilterDelegateType UnityEngineGameObjectMethodAddComponentUnityEngineMeshFilterDelegate = new UnityEngineGameObjectMethodAddComponentUnityEngineMeshFilterDelegateType(UnityEngineGameObjectMethodAddComponentUnityEngineMeshFilter);
		static unsafe readonly UnityEngineGameObjectMethodAddComponentUnityEngineMeshRendererDelegateType UnityEngineGameObjectMethodAddComponentUnityEngineMeshRendererDelegate = new UnityEngineGameObjectMethodAddComponentUnityEngineMeshRendererDelegateType(UnityEngineGameObjectMethodAddComponentUnityEngineMeshRenderer);
		static unsafe readonly UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveTypeDelegateType UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveTypeDelegate = new UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveTypeDelegateType(UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveType);
		static unsafe readonly UnityEngineGameObjectMethodSetActiveSystemBooleanDelegateType UnityEngineGameObjectMethodSetActiveSystemBooleanDelegate = new UnityEngineGameObjectMethodSetActiveSystemBooleanDelegateType(UnityEngineGameObjectMethodSetActiveSystemBoolean);
		static unsafe readonly UnityEngineDebugMethodLogSystemObjectDelegateType UnityEngineDebugMethodLogSystemObjectDelegate = new UnityEngineDebugMethodLogSystemObjectDelegateType(UnityEngineDebugMethodLogSystemObject);
		static unsafe readonly UnityEngineMonoBehaviourPropertyGetTransformDelegateType UnityEngineMonoBehaviourPropertyGetTransformDelegate = new UnityEngineMonoBehaviourPropertyGetTransformDelegateType(UnityEngineMonoBehaviourPropertyGetTransform);
		static unsafe readonly UnityEngineMonoBehaviourPropertyGetGameObjectDelegateType UnityEngineMonoBehaviourPropertyGetGameObjectDelegate = new UnityEngineMonoBehaviourPropertyGetGameObjectDelegateType(UnityEngineMonoBehaviourPropertyGetGameObject);
		static unsafe readonly SystemCollectionsGenericIEnumeratorUnityEngineVector2PropertyGetCurrentDelegateType SystemCollectionsGenericIEnumeratorUnityEngineVector2PropertyGetCurrentDelegate = new SystemCollectionsGenericIEnumeratorUnityEngineVector2PropertyGetCurrentDelegateType(SystemCollectionsGenericIEnumeratorUnityEngineVector2PropertyGetCurrent);
		static unsafe readonly SystemCollectionsGenericIEnumeratorUnityEngineVector3PropertyGetCurrentDelegateType SystemCollectionsGenericIEnumeratorUnityEngineVector3PropertyGetCurrentDelegate = new SystemCollectionsGenericIEnumeratorUnityEngineVector3PropertyGetCurrentDelegateType(SystemCollectionsGenericIEnumeratorUnityEngineVector3PropertyGetCurrent);
		static unsafe readonly SystemCollectionsGenericIEnumeratorSystemBytePropertyGetCurrentDelegateType SystemCollectionsGenericIEnumeratorSystemBytePropertyGetCurrentDelegate = new SystemCollectionsGenericIEnumeratorSystemBytePropertyGetCurrentDelegateType(SystemCollectionsGenericIEnumeratorSystemBytePropertyGetCurrent);
		static unsafe readonly SystemCollectionsGenericIEnumeratorSystemInt32PropertyGetCurrentDelegateType SystemCollectionsGenericIEnumeratorSystemInt32PropertyGetCurrentDelegate = new SystemCollectionsGenericIEnumeratorSystemInt32PropertyGetCurrentDelegateType(SystemCollectionsGenericIEnumeratorSystemInt32PropertyGetCurrent);
		static unsafe readonly SystemCollectionsGenericIEnumerableUnityEngineVector2MethodGetEnumeratorDelegateType SystemCollectionsGenericIEnumerableUnityEngineVector2MethodGetEnumeratorDelegate = new SystemCollectionsGenericIEnumerableUnityEngineVector2MethodGetEnumeratorDelegateType(SystemCollectionsGenericIEnumerableUnityEngineVector2MethodGetEnumerator);
		static unsafe readonly SystemCollectionsGenericIEnumerableUnityEngineVector3MethodGetEnumeratorDelegateType SystemCollectionsGenericIEnumerableUnityEngineVector3MethodGetEnumeratorDelegate = new SystemCollectionsGenericIEnumerableUnityEngineVector3MethodGetEnumeratorDelegateType(SystemCollectionsGenericIEnumerableUnityEngineVector3MethodGetEnumerator);
		static unsafe readonly SystemCollectionsGenericIEnumerableSystemByteMethodGetEnumeratorDelegateType SystemCollectionsGenericIEnumerableSystemByteMethodGetEnumeratorDelegate = new SystemCollectionsGenericIEnumerableSystemByteMethodGetEnumeratorDelegateType(SystemCollectionsGenericIEnumerableSystemByteMethodGetEnumerator);
		static unsafe readonly SystemCollectionsGenericIEnumerableSystemInt32MethodGetEnumeratorDelegateType SystemCollectionsGenericIEnumerableSystemInt32MethodGetEnumeratorDelegate = new SystemCollectionsGenericIEnumerableSystemInt32MethodGetEnumeratorDelegateType(SystemCollectionsGenericIEnumerableSystemInt32MethodGetEnumerator);
		static unsafe readonly BoxNativeArrayOptionsDelegateType BoxNativeArrayOptionsDelegate = new BoxNativeArrayOptionsDelegateType(BoxNativeArrayOptions);
		static unsafe readonly UnboxNativeArrayOptionsDelegateType UnboxNativeArrayOptionsDelegate = new UnboxNativeArrayOptionsDelegateType(UnboxNativeArrayOptions);
		static unsafe readonly ReleaseUnityCollectionsNativeArraySystemByteDelegateType ReleaseUnityCollectionsNativeArraySystemByteDelegate = new ReleaseUnityCollectionsNativeArraySystemByteDelegateType(ReleaseUnityCollectionsNativeArraySystemByte);
		static unsafe readonly UnityCollectionsNativeArraySystemByteConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegateType UnityCollectionsNativeArraySystemByteConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegate = new UnityCollectionsNativeArraySystemByteConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegateType(UnityCollectionsNativeArraySystemByteConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptions);
		static unsafe readonly UnityCollectionsNativeArraySystemByteMethodDisposeDelegateType UnityCollectionsNativeArraySystemByteMethodDisposeDelegate = new UnityCollectionsNativeArraySystemByteMethodDisposeDelegateType(UnityCollectionsNativeArraySystemByteMethodDispose);
		static unsafe readonly BoxNativeArraySystemByteDelegateType BoxNativeArraySystemByteDelegate = new BoxNativeArraySystemByteDelegateType(BoxNativeArraySystemByte);
		static unsafe readonly UnboxNativeArraySystemByteDelegateType UnboxNativeArraySystemByteDelegate = new UnboxNativeArraySystemByteDelegateType(UnboxNativeArraySystemByte);
		static unsafe readonly ReleaseUnityCollectionsNativeArrayUnityEngineVector3DelegateType ReleaseUnityCollectionsNativeArrayUnityEngineVector3Delegate = new ReleaseUnityCollectionsNativeArrayUnityEngineVector3DelegateType(ReleaseUnityCollectionsNativeArrayUnityEngineVector3);
		static unsafe readonly UnityCollectionsNativeArrayUnityEngineVector3ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegateType UnityCollectionsNativeArrayUnityEngineVector3ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegate = new UnityCollectionsNativeArrayUnityEngineVector3ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegateType(UnityCollectionsNativeArrayUnityEngineVector3ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptions);
		static unsafe readonly UnityCollectionsNativeArrayUnityEngineVector3MethodDisposeDelegateType UnityCollectionsNativeArrayUnityEngineVector3MethodDisposeDelegate = new UnityCollectionsNativeArrayUnityEngineVector3MethodDisposeDelegateType(UnityCollectionsNativeArrayUnityEngineVector3MethodDispose);
		static unsafe readonly BoxNativeArrayUnityEngineVector3DelegateType BoxNativeArrayUnityEngineVector3Delegate = new BoxNativeArrayUnityEngineVector3DelegateType(BoxNativeArrayUnityEngineVector3);
		static unsafe readonly UnboxNativeArrayUnityEngineVector3DelegateType UnboxNativeArrayUnityEngineVector3Delegate = new UnboxNativeArrayUnityEngineVector3DelegateType(UnboxNativeArrayUnityEngineVector3);
		static unsafe readonly ReleaseUnityCollectionsNativeArrayUnityEngineVector2DelegateType ReleaseUnityCollectionsNativeArrayUnityEngineVector2Delegate = new ReleaseUnityCollectionsNativeArrayUnityEngineVector2DelegateType(ReleaseUnityCollectionsNativeArrayUnityEngineVector2);
		static unsafe readonly UnityCollectionsNativeArrayUnityEngineVector2ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegateType UnityCollectionsNativeArrayUnityEngineVector2ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegate = new UnityCollectionsNativeArrayUnityEngineVector2ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegateType(UnityCollectionsNativeArrayUnityEngineVector2ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptions);
		static unsafe readonly UnityCollectionsNativeArrayUnityEngineVector2MethodDisposeDelegateType UnityCollectionsNativeArrayUnityEngineVector2MethodDisposeDelegate = new UnityCollectionsNativeArrayUnityEngineVector2MethodDisposeDelegateType(UnityCollectionsNativeArrayUnityEngineVector2MethodDispose);
		static unsafe readonly BoxNativeArrayUnityEngineVector2DelegateType BoxNativeArrayUnityEngineVector2Delegate = new BoxNativeArrayUnityEngineVector2DelegateType(BoxNativeArrayUnityEngineVector2);
		static unsafe readonly UnboxNativeArrayUnityEngineVector2DelegateType UnboxNativeArrayUnityEngineVector2Delegate = new UnboxNativeArrayUnityEngineVector2DelegateType(UnboxNativeArrayUnityEngineVector2);
		static unsafe readonly ReleaseUnityCollectionsNativeArraySystemInt32DelegateType ReleaseUnityCollectionsNativeArraySystemInt32Delegate = new ReleaseUnityCollectionsNativeArraySystemInt32DelegateType(ReleaseUnityCollectionsNativeArraySystemInt32);
		static unsafe readonly UnityCollectionsNativeArraySystemInt32ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegateType UnityCollectionsNativeArraySystemInt32ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegate = new UnityCollectionsNativeArraySystemInt32ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegateType(UnityCollectionsNativeArraySystemInt32ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptions);
		static unsafe readonly UnityCollectionsNativeArraySystemInt32MethodDisposeDelegateType UnityCollectionsNativeArraySystemInt32MethodDisposeDelegate = new UnityCollectionsNativeArraySystemInt32MethodDisposeDelegateType(UnityCollectionsNativeArraySystemInt32MethodDispose);
		static unsafe readonly BoxNativeArraySystemInt32DelegateType BoxNativeArraySystemInt32Delegate = new BoxNativeArraySystemInt32DelegateType(BoxNativeArraySystemInt32);
		static unsafe readonly UnboxNativeArraySystemInt32DelegateType UnboxNativeArraySystemInt32Delegate = new UnboxNativeArraySystemInt32DelegateType(UnboxNativeArraySystemInt32);
		static unsafe readonly BoxAllocatorDelegateType BoxAllocatorDelegate = new BoxAllocatorDelegateType(BoxAllocator);
		static unsafe readonly UnboxAllocatorDelegateType UnboxAllocatorDelegate = new UnboxAllocatorDelegateType(UnboxAllocator);
		static unsafe readonly UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArraySystemByteSystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegateType UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArraySystemByteSystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegate = new UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArraySystemByteSystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegateType(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArraySystemByteSystemVoidPointer_SystemInt32_UnityCollectionsAllocator);
		static unsafe readonly UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArrayUnityEngineVector3SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegateType UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArrayUnityEngineVector3SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegate = new UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArrayUnityEngineVector3SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegateType(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArrayUnityEngineVector3SystemVoidPointer_SystemInt32_UnityCollectionsAllocator);
		static unsafe readonly UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArrayUnityEngineVector2SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegateType UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArrayUnityEngineVector2SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegate = new UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArrayUnityEngineVector2SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegateType(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArrayUnityEngineVector2SystemVoidPointer_SystemInt32_UnityCollectionsAllocator);
		static unsafe readonly UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArraySystemInt32SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegateType UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArraySystemInt32SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegate = new UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArraySystemInt32SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegateType(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArraySystemInt32SystemVoidPointer_SystemInt32_UnityCollectionsAllocator);
		static unsafe readonly UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksUnityEngineVector3UnityCollectionsNativeArrayDelegateType UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksUnityEngineVector3UnityCollectionsNativeArrayDelegate = new UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksUnityEngineVector3UnityCollectionsNativeArrayDelegateType(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksUnityEngineVector3UnityCollectionsNativeArray);
		static unsafe readonly UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksUnityEngineVector2UnityCollectionsNativeArrayDelegateType UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksUnityEngineVector2UnityCollectionsNativeArrayDelegate = new UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksUnityEngineVector2UnityCollectionsNativeArrayDelegateType(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksUnityEngineVector2UnityCollectionsNativeArray);
		static unsafe readonly UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksSystemInt32UnityCollectionsNativeArrayDelegateType UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksSystemInt32UnityCollectionsNativeArrayDelegate = new UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksSystemInt32UnityCollectionsNativeArrayDelegateType(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksSystemInt32UnityCollectionsNativeArray);
		static unsafe readonly BoxMeshTopologyDelegateType BoxMeshTopologyDelegate = new BoxMeshTopologyDelegateType(BoxMeshTopology);
		static unsafe readonly UnboxMeshTopologyDelegateType UnboxMeshTopologyDelegate = new UnboxMeshTopologyDelegateType(UnboxMeshTopology);
		static unsafe readonly UnityEngineMeshConstructorDelegateType UnityEngineMeshConstructorDelegate = new UnityEngineMeshConstructorDelegateType(UnityEngineMeshConstructor);
		static unsafe readonly UnityEngineMeshMethodSetVerticesUnityEngineVector3Array1DelegateType UnityEngineMeshMethodSetVerticesUnityEngineVector3Array1Delegate = new UnityEngineMeshMethodSetVerticesUnityEngineVector3Array1DelegateType(UnityEngineMeshMethodSetVerticesUnityEngineVector3Array1);
		static unsafe readonly UnityEngineMeshMethodSetVerticesUnityEngineVector3UnityCollectionsNativeArrayDelegateType UnityEngineMeshMethodSetVerticesUnityEngineVector3UnityCollectionsNativeArrayDelegate = new UnityEngineMeshMethodSetVerticesUnityEngineVector3UnityCollectionsNativeArrayDelegateType(UnityEngineMeshMethodSetVerticesUnityEngineVector3UnityCollectionsNativeArray);
		static unsafe readonly UnityEngineMeshMethodSetNormalsUnityEngineVector3Array1DelegateType UnityEngineMeshMethodSetNormalsUnityEngineVector3Array1Delegate = new UnityEngineMeshMethodSetNormalsUnityEngineVector3Array1DelegateType(UnityEngineMeshMethodSetNormalsUnityEngineVector3Array1);
		static unsafe readonly UnityEngineMeshMethodSetNormalsUnityEngineVector3UnityCollectionsNativeArrayDelegateType UnityEngineMeshMethodSetNormalsUnityEngineVector3UnityCollectionsNativeArrayDelegate = new UnityEngineMeshMethodSetNormalsUnityEngineVector3UnityCollectionsNativeArrayDelegateType(UnityEngineMeshMethodSetNormalsUnityEngineVector3UnityCollectionsNativeArray);
		static unsafe readonly UnityEngineMeshMethodSetTrianglesSystemInt32Array1_SystemInt32_SystemBoolean_SystemInt32DelegateType UnityEngineMeshMethodSetTrianglesSystemInt32Array1_SystemInt32_SystemBoolean_SystemInt32Delegate = new UnityEngineMeshMethodSetTrianglesSystemInt32Array1_SystemInt32_SystemBoolean_SystemInt32DelegateType(UnityEngineMeshMethodSetTrianglesSystemInt32Array1_SystemInt32_SystemBoolean_SystemInt32);
		static unsafe readonly UnityEngineMeshMethodSetIndicesSystemInt32UnityCollectionsNativeArray_UnityEngineMeshTopology_SystemInt32_SystemBoolean_SystemInt32DelegateType UnityEngineMeshMethodSetIndicesSystemInt32UnityCollectionsNativeArray_UnityEngineMeshTopology_SystemInt32_SystemBoolean_SystemInt32Delegate = new UnityEngineMeshMethodSetIndicesSystemInt32UnityCollectionsNativeArray_UnityEngineMeshTopology_SystemInt32_SystemBoolean_SystemInt32DelegateType(UnityEngineMeshMethodSetIndicesSystemInt32UnityCollectionsNativeArray_UnityEngineMeshTopology_SystemInt32_SystemBoolean_SystemInt32);
		static unsafe readonly UnityEngineMeshMethodSetUVsSystemInt32_UnityEngineVector2Array1DelegateType UnityEngineMeshMethodSetUVsSystemInt32_UnityEngineVector2Array1Delegate = new UnityEngineMeshMethodSetUVsSystemInt32_UnityEngineVector2Array1DelegateType(UnityEngineMeshMethodSetUVsSystemInt32_UnityEngineVector2Array1);
		static unsafe readonly UnityEngineMeshMethodSetUVsUnityEngineVector2SystemInt32_UnityCollectionsNativeArrayDelegateType UnityEngineMeshMethodSetUVsUnityEngineVector2SystemInt32_UnityCollectionsNativeArrayDelegate = new UnityEngineMeshMethodSetUVsUnityEngineVector2SystemInt32_UnityCollectionsNativeArrayDelegateType(UnityEngineMeshMethodSetUVsUnityEngineVector2SystemInt32_UnityCollectionsNativeArray);
		static unsafe readonly UnityEngineMeshFilterPropertyGetMeshDelegateType UnityEngineMeshFilterPropertyGetMeshDelegate = new UnityEngineMeshFilterPropertyGetMeshDelegateType(UnityEngineMeshFilterPropertyGetMesh);
		static unsafe readonly UnityEngineMeshFilterPropertySetMeshDelegateType UnityEngineMeshFilterPropertySetMeshDelegate = new UnityEngineMeshFilterPropertySetMeshDelegateType(UnityEngineMeshFilterPropertySetMesh);
		static unsafe readonly UnityEngineMeshRendererPropertyGetMaterialDelegateType UnityEngineMeshRendererPropertyGetMaterialDelegate = new UnityEngineMeshRendererPropertyGetMaterialDelegateType(UnityEngineMeshRendererPropertyGetMaterial);
		static unsafe readonly UnityEngineMeshRendererPropertySetMaterialDelegateType UnityEngineMeshRendererPropertySetMaterialDelegate = new UnityEngineMeshRendererPropertySetMaterialDelegateType(UnityEngineMeshRendererPropertySetMaterial);
		static unsafe readonly SystemExceptionConstructorSystemStringDelegateType SystemExceptionConstructorSystemStringDelegate = new SystemExceptionConstructorSystemStringDelegateType(SystemExceptionConstructorSystemString);
		static unsafe readonly BoxPrimitiveTypeDelegateType BoxPrimitiveTypeDelegate = new BoxPrimitiveTypeDelegateType(BoxPrimitiveType);
		static unsafe readonly UnboxPrimitiveTypeDelegateType UnboxPrimitiveTypeDelegate = new UnboxPrimitiveTypeDelegateType(UnboxPrimitiveType);
		static unsafe readonly UnityEngineTimePropertyGetDeltaTimeDelegateType UnityEngineTimePropertyGetDeltaTimeDelegate = new UnityEngineTimePropertyGetDeltaTimeDelegateType(UnityEngineTimePropertyGetDeltaTime);
		static unsafe readonly UnityEngineCameraPropertyGetMainDelegateType UnityEngineCameraPropertyGetMainDelegate = new UnityEngineCameraPropertyGetMainDelegateType(UnityEngineCameraPropertyGetMain);
		static unsafe readonly UnityEngineCameraPropertyGetFieldOfViewDelegateType UnityEngineCameraPropertyGetFieldOfViewDelegate = new UnityEngineCameraPropertyGetFieldOfViewDelegateType(UnityEngineCameraPropertyGetFieldOfView);
		static unsafe readonly UnityEngineCameraPropertySetFieldOfViewDelegateType UnityEngineCameraPropertySetFieldOfViewDelegate = new UnityEngineCameraPropertySetFieldOfViewDelegateType(UnityEngineCameraPropertySetFieldOfView);
		static unsafe readonly UnityEngineCameraPropertyGetAspectDelegateType UnityEngineCameraPropertyGetAspectDelegate = new UnityEngineCameraPropertyGetAspectDelegateType(UnityEngineCameraPropertyGetAspect);
		static unsafe readonly UnityEngineCameraPropertySetAspectDelegateType UnityEngineCameraPropertySetAspectDelegate = new UnityEngineCameraPropertySetAspectDelegateType(UnityEngineCameraPropertySetAspect);
		static unsafe readonly UnityEngineCameraPropertyGetPixelWidthDelegateType UnityEngineCameraPropertyGetPixelWidthDelegate = new UnityEngineCameraPropertyGetPixelWidthDelegateType(UnityEngineCameraPropertyGetPixelWidth);
		static unsafe readonly UnityEngineCameraPropertyGetPixelHeightDelegateType UnityEngineCameraPropertyGetPixelHeightDelegate = new UnityEngineCameraPropertyGetPixelHeightDelegateType(UnityEngineCameraPropertyGetPixelHeight);
		static unsafe readonly BoxRawDownloadedDataDelegateType BoxRawDownloadedDataDelegate = new BoxRawDownloadedDataDelegateType(BoxRawDownloadedData);
		static unsafe readonly UnboxRawDownloadedDataDelegateType UnboxRawDownloadedDataDelegate = new UnboxRawDownloadedDataDelegateType(UnboxRawDownloadedData);
		static unsafe readonly ReleaseBaseNativeDownloadHandlerDelegateType ReleaseBaseNativeDownloadHandlerDelegate = new ReleaseBaseNativeDownloadHandlerDelegateType(ReleaseBaseNativeDownloadHandler);
		static unsafe readonly BaseNativeDownloadHandlerConstructorDelegateType BaseNativeDownloadHandlerConstructorDelegate = new BaseNativeDownloadHandlerConstructorDelegateType(BaseNativeDownloadHandlerConstructor);
		static unsafe readonly UnityEngineNetworkingUnityWebRequestPropertyGetErrorDelegateType UnityEngineNetworkingUnityWebRequestPropertyGetErrorDelegate = new UnityEngineNetworkingUnityWebRequestPropertyGetErrorDelegateType(UnityEngineNetworkingUnityWebRequestPropertyGetError);
		static unsafe readonly UnityEngineNetworkingUnityWebRequestPropertyGetIsDoneDelegateType UnityEngineNetworkingUnityWebRequestPropertyGetIsDoneDelegate = new UnityEngineNetworkingUnityWebRequestPropertyGetIsDoneDelegateType(UnityEngineNetworkingUnityWebRequestPropertyGetIsDone);
		static unsafe readonly UnityEngineNetworkingUnityWebRequestPropertyGetResponseCodeDelegateType UnityEngineNetworkingUnityWebRequestPropertyGetResponseCodeDelegate = new UnityEngineNetworkingUnityWebRequestPropertyGetResponseCodeDelegateType(UnityEngineNetworkingUnityWebRequestPropertyGetResponseCode);
		static unsafe readonly UnityEngineNetworkingUnityWebRequestPropertyGetUrlDelegateType UnityEngineNetworkingUnityWebRequestPropertyGetUrlDelegate = new UnityEngineNetworkingUnityWebRequestPropertyGetUrlDelegateType(UnityEngineNetworkingUnityWebRequestPropertyGetUrl);
		static unsafe readonly UnityEngineNetworkingUnityWebRequestPropertySetUrlDelegateType UnityEngineNetworkingUnityWebRequestPropertySetUrlDelegate = new UnityEngineNetworkingUnityWebRequestPropertySetUrlDelegateType(UnityEngineNetworkingUnityWebRequestPropertySetUrl);
		static unsafe readonly UnityEngineNetworkingUnityWebRequestPropertyGetMethodDelegateType UnityEngineNetworkingUnityWebRequestPropertyGetMethodDelegate = new UnityEngineNetworkingUnityWebRequestPropertyGetMethodDelegateType(UnityEngineNetworkingUnityWebRequestPropertyGetMethod);
		static unsafe readonly UnityEngineNetworkingUnityWebRequestPropertySetMethodDelegateType UnityEngineNetworkingUnityWebRequestPropertySetMethodDelegate = new UnityEngineNetworkingUnityWebRequestPropertySetMethodDelegateType(UnityEngineNetworkingUnityWebRequestPropertySetMethod);
		static unsafe readonly UnityEngineNetworkingUnityWebRequestPropertyGetDownloadHandlerDelegateType UnityEngineNetworkingUnityWebRequestPropertyGetDownloadHandlerDelegate = new UnityEngineNetworkingUnityWebRequestPropertyGetDownloadHandlerDelegateType(UnityEngineNetworkingUnityWebRequestPropertyGetDownloadHandler);
		static unsafe readonly UnityEngineNetworkingUnityWebRequestPropertySetDownloadHandlerDelegateType UnityEngineNetworkingUnityWebRequestPropertySetDownloadHandlerDelegate = new UnityEngineNetworkingUnityWebRequestPropertySetDownloadHandlerDelegateType(UnityEngineNetworkingUnityWebRequestPropertySetDownloadHandler);
		static unsafe readonly UnityEngineNetworkingUnityWebRequestMethodGetSystemStringDelegateType UnityEngineNetworkingUnityWebRequestMethodGetSystemStringDelegate = new UnityEngineNetworkingUnityWebRequestMethodGetSystemStringDelegateType(UnityEngineNetworkingUnityWebRequestMethodGetSystemString);
		static unsafe readonly UnityEngineNetworkingUnityWebRequestMethodSetRequestHeaderSystemString_SystemStringDelegateType UnityEngineNetworkingUnityWebRequestMethodSetRequestHeaderSystemString_SystemStringDelegate = new UnityEngineNetworkingUnityWebRequestMethodSetRequestHeaderSystemString_SystemStringDelegateType(UnityEngineNetworkingUnityWebRequestMethodSetRequestHeaderSystemString_SystemString);
		static unsafe readonly UnityEngineNetworkingUnityWebRequestMethodSendWebRequestDelegateType UnityEngineNetworkingUnityWebRequestMethodSendWebRequestDelegate = new UnityEngineNetworkingUnityWebRequestMethodSendWebRequestDelegateType(UnityEngineNetworkingUnityWebRequestMethodSendWebRequest);
		static unsafe readonly UnityEngineNetworkingUnityWebRequestMethodGetResponseHeaderSystemStringDelegateType UnityEngineNetworkingUnityWebRequestMethodGetResponseHeaderSystemStringDelegate = new UnityEngineNetworkingUnityWebRequestMethodGetResponseHeaderSystemStringDelegateType(UnityEngineNetworkingUnityWebRequestMethodGetResponseHeaderSystemString);
		static unsafe readonly UnityEngineNetworkingUnityWebRequestAsyncOperationAddEventCompletedDelegateType UnityEngineNetworkingUnityWebRequestAsyncOperationAddEventCompletedDelegate = new UnityEngineNetworkingUnityWebRequestAsyncOperationAddEventCompletedDelegateType(UnityEngineNetworkingUnityWebRequestAsyncOperationAddEventCompleted);
		static unsafe readonly UnityEngineNetworkingUnityWebRequestAsyncOperationRemoveEventCompletedDelegateType UnityEngineNetworkingUnityWebRequestAsyncOperationRemoveEventCompletedDelegate = new UnityEngineNetworkingUnityWebRequestAsyncOperationRemoveEventCompletedDelegateType(UnityEngineNetworkingUnityWebRequestAsyncOperationRemoveEventCompleted);
		static unsafe readonly SystemRuntimeInteropServicesMarshalMethodStringToCoTaskMemUTF8SystemStringDelegateType SystemRuntimeInteropServicesMarshalMethodStringToCoTaskMemUTF8SystemStringDelegate = new SystemRuntimeInteropServicesMarshalMethodStringToCoTaskMemUTF8SystemStringDelegateType(SystemRuntimeInteropServicesMarshalMethodStringToCoTaskMemUTF8SystemString);
		static unsafe readonly SystemRuntimeInteropServicesMarshalMethodFreeCoTaskMemSystemIntPtrDelegateType SystemRuntimeInteropServicesMarshalMethodFreeCoTaskMemSystemIntPtrDelegate = new SystemRuntimeInteropServicesMarshalMethodFreeCoTaskMemSystemIntPtrDelegateType(SystemRuntimeInteropServicesMarshalMethodFreeCoTaskMemSystemIntPtr);
		static unsafe readonly ReleaseBaseCesium3DTilesetDelegateType ReleaseBaseCesium3DTilesetDelegate = new ReleaseBaseCesium3DTilesetDelegateType(ReleaseBaseCesium3DTileset);
		static unsafe readonly BaseCesium3DTilesetConstructorDelegateType BaseCesium3DTilesetConstructorDelegate = new BaseCesium3DTilesetConstructorDelegateType(BaseCesium3DTilesetConstructor);
		static unsafe readonly SystemThreadingTasksTaskMethodRunSystemActionDelegateType SystemThreadingTasksTaskMethodRunSystemActionDelegate = new SystemThreadingTasksTaskMethodRunSystemActionDelegateType(SystemThreadingTasksTaskMethodRunSystemAction);
		static unsafe readonly UnityEngineMaterialMethodSetTextureSystemString_UnityEngineTextureDelegateType UnityEngineMaterialMethodSetTextureSystemString_UnityEngineTextureDelegate = new UnityEngineMaterialMethodSetTextureSystemString_UnityEngineTextureDelegateType(UnityEngineMaterialMethodSetTextureSystemString_UnityEngineTexture);
		static unsafe readonly UnityEngineResourcesMethodLoadUnityEngineMaterialSystemStringDelegateType UnityEngineResourcesMethodLoadUnityEngineMaterialSystemStringDelegate = new UnityEngineResourcesMethodLoadUnityEngineMaterialSystemStringDelegateType(UnityEngineResourcesMethodLoadUnityEngineMaterialSystemString);
		static unsafe readonly UnityEditorSceneViewPropertyGetLastActiveSceneViewDelegateType UnityEditorSceneViewPropertyGetLastActiveSceneViewDelegate = new UnityEditorSceneViewPropertyGetLastActiveSceneViewDelegateType(UnityEditorSceneViewPropertyGetLastActiveSceneView);
		static unsafe readonly UnityEditorSceneViewPropertyGetCameraDelegateType UnityEditorSceneViewPropertyGetCameraDelegate = new UnityEditorSceneViewPropertyGetCameraDelegateType(UnityEditorSceneViewPropertyGetCamera);
		static unsafe readonly BoxTextureFormatDelegateType BoxTextureFormatDelegate = new BoxTextureFormatDelegateType(BoxTextureFormat);
		static unsafe readonly UnboxTextureFormatDelegateType UnboxTextureFormatDelegate = new UnboxTextureFormatDelegateType(UnboxTextureFormat);
		static unsafe readonly UnityEngineTexture2DConstructorSystemInt32_SystemInt32_UnityEngineTextureFormat_SystemBoolean_SystemBooleanDelegateType UnityEngineTexture2DConstructorSystemInt32_SystemInt32_UnityEngineTextureFormat_SystemBoolean_SystemBooleanDelegate = new UnityEngineTexture2DConstructorSystemInt32_SystemInt32_UnityEngineTextureFormat_SystemBoolean_SystemBooleanDelegateType(UnityEngineTexture2DConstructorSystemInt32_SystemInt32_UnityEngineTextureFormat_SystemBoolean_SystemBoolean);
		static unsafe readonly UnityEngineTexture2DMethodSetPixelDataSystemByteSystemByteArray1_SystemInt32_SystemInt32DelegateType UnityEngineTexture2DMethodSetPixelDataSystemByteSystemByteArray1_SystemInt32_SystemInt32Delegate = new UnityEngineTexture2DMethodSetPixelDataSystemByteSystemByteArray1_SystemInt32_SystemInt32DelegateType(UnityEngineTexture2DMethodSetPixelDataSystemByteSystemByteArray1_SystemInt32_SystemInt32);
		static unsafe readonly UnityEngineTexture2DMethodSetPixelDataSystemByteUnityCollectionsNativeArray_SystemInt32_SystemInt32DelegateType UnityEngineTexture2DMethodSetPixelDataSystemByteUnityCollectionsNativeArray_SystemInt32_SystemInt32Delegate = new UnityEngineTexture2DMethodSetPixelDataSystemByteUnityCollectionsNativeArray_SystemInt32_SystemInt32DelegateType(UnityEngineTexture2DMethodSetPixelDataSystemByteUnityCollectionsNativeArray_SystemInt32_SystemInt32);
		static unsafe readonly UnityEngineTexture2DMethodApplySystemBoolean_SystemBooleanDelegateType UnityEngineTexture2DMethodApplySystemBoolean_SystemBooleanDelegate = new UnityEngineTexture2DMethodApplySystemBoolean_SystemBooleanDelegateType(UnityEngineTexture2DMethodApplySystemBoolean_SystemBoolean);
		static unsafe readonly UnityEngineTexture2DMethodLoadRawTextureDataSystemIntPtr_SystemInt32DelegateType UnityEngineTexture2DMethodLoadRawTextureDataSystemIntPtr_SystemInt32Delegate = new UnityEngineTexture2DMethodLoadRawTextureDataSystemIntPtr_SystemInt32DelegateType(UnityEngineTexture2DMethodLoadRawTextureDataSystemIntPtr_SystemInt32);
		static unsafe readonly UnityEngineApplicationPropertyGetTemporaryCachePathDelegateType UnityEngineApplicationPropertyGetTemporaryCachePathDelegate = new UnityEngineApplicationPropertyGetTemporaryCachePathDelegateType(UnityEngineApplicationPropertyGetTemporaryCachePath);
		static unsafe readonly UnityEngineApplicationPropertyGetPersistentDataPathDelegateType UnityEngineApplicationPropertyGetPersistentDataPathDelegate = new UnityEngineApplicationPropertyGetPersistentDataPathDelegateType(UnityEngineApplicationPropertyGetPersistentDataPath);
		static unsafe readonly BoxBooleanDelegateType BoxBooleanDelegate = new BoxBooleanDelegateType(BoxBoolean);
		static unsafe readonly UnboxBooleanDelegateType UnboxBooleanDelegate = new UnboxBooleanDelegateType(UnboxBoolean);
		static unsafe readonly BoxSByteDelegateType BoxSByteDelegate = new BoxSByteDelegateType(BoxSByte);
		static unsafe readonly UnboxSByteDelegateType UnboxSByteDelegate = new UnboxSByteDelegateType(UnboxSByte);
		static unsafe readonly BoxByteDelegateType BoxByteDelegate = new BoxByteDelegateType(BoxByte);
		static unsafe readonly UnboxByteDelegateType UnboxByteDelegate = new UnboxByteDelegateType(UnboxByte);
		static unsafe readonly BoxInt16DelegateType BoxInt16Delegate = new BoxInt16DelegateType(BoxInt16);
		static unsafe readonly UnboxInt16DelegateType UnboxInt16Delegate = new UnboxInt16DelegateType(UnboxInt16);
		static unsafe readonly BoxUInt16DelegateType BoxUInt16Delegate = new BoxUInt16DelegateType(BoxUInt16);
		static unsafe readonly UnboxUInt16DelegateType UnboxUInt16Delegate = new UnboxUInt16DelegateType(UnboxUInt16);
		static unsafe readonly BoxInt32DelegateType BoxInt32Delegate = new BoxInt32DelegateType(BoxInt32);
		static unsafe readonly UnboxInt32DelegateType UnboxInt32Delegate = new UnboxInt32DelegateType(UnboxInt32);
		static unsafe readonly BoxUInt32DelegateType BoxUInt32Delegate = new BoxUInt32DelegateType(BoxUInt32);
		static unsafe readonly UnboxUInt32DelegateType UnboxUInt32Delegate = new UnboxUInt32DelegateType(UnboxUInt32);
		static unsafe readonly BoxInt64DelegateType BoxInt64Delegate = new BoxInt64DelegateType(BoxInt64);
		static unsafe readonly UnboxInt64DelegateType UnboxInt64Delegate = new UnboxInt64DelegateType(UnboxInt64);
		static unsafe readonly BoxUInt64DelegateType BoxUInt64Delegate = new BoxUInt64DelegateType(BoxUInt64);
		static unsafe readonly UnboxUInt64DelegateType UnboxUInt64Delegate = new UnboxUInt64DelegateType(UnboxUInt64);
		static unsafe readonly BoxCharDelegateType BoxCharDelegate = new BoxCharDelegateType(BoxChar);
		static unsafe readonly UnboxCharDelegateType UnboxCharDelegate = new UnboxCharDelegateType(UnboxChar);
		static unsafe readonly BoxSingleDelegateType BoxSingleDelegate = new BoxSingleDelegateType(BoxSingle);
		static unsafe readonly UnboxSingleDelegateType UnboxSingleDelegate = new UnboxSingleDelegateType(UnboxSingle);
		static unsafe readonly BoxDoubleDelegateType BoxDoubleDelegate = new BoxDoubleDelegateType(BoxDouble);
		static unsafe readonly UnboxDoubleDelegateType UnboxDoubleDelegate = new UnboxDoubleDelegateType(UnboxDouble);
		static unsafe readonly UnityEngineUnityEngineVector2Array1Constructor1DelegateType UnityEngineUnityEngineVector2Array1Constructor1Delegate = new UnityEngineUnityEngineVector2Array1Constructor1DelegateType(UnityEngineUnityEngineVector2Array1Constructor1);
		static unsafe readonly UnityEngineVector2Array1GetItem1DelegateType UnityEngineVector2Array1GetItem1Delegate = new UnityEngineVector2Array1GetItem1DelegateType(UnityEngineVector2Array1GetItem1);
		static unsafe readonly UnityEngineVector2Array1SetItem1DelegateType UnityEngineVector2Array1SetItem1Delegate = new UnityEngineVector2Array1SetItem1DelegateType(UnityEngineVector2Array1SetItem1);
		static unsafe readonly UnityEngineUnityEngineVector3Array1Constructor1DelegateType UnityEngineUnityEngineVector3Array1Constructor1Delegate = new UnityEngineUnityEngineVector3Array1Constructor1DelegateType(UnityEngineUnityEngineVector3Array1Constructor1);
		static unsafe readonly UnityEngineVector3Array1GetItem1DelegateType UnityEngineVector3Array1GetItem1Delegate = new UnityEngineVector3Array1GetItem1DelegateType(UnityEngineVector3Array1GetItem1);
		static unsafe readonly UnityEngineVector3Array1SetItem1DelegateType UnityEngineVector3Array1SetItem1Delegate = new UnityEngineVector3Array1SetItem1DelegateType(UnityEngineVector3Array1SetItem1);
		static unsafe readonly SystemSystemByteArray1Constructor1DelegateType SystemSystemByteArray1Constructor1Delegate = new SystemSystemByteArray1Constructor1DelegateType(SystemSystemByteArray1Constructor1);
		static unsafe readonly SystemByteArray1GetItem1DelegateType SystemByteArray1GetItem1Delegate = new SystemByteArray1GetItem1DelegateType(SystemByteArray1GetItem1);
		static unsafe readonly SystemByteArray1SetItem1DelegateType SystemByteArray1SetItem1Delegate = new SystemByteArray1SetItem1DelegateType(SystemByteArray1SetItem1);
		static unsafe readonly SystemSystemInt32Array1Constructor1DelegateType SystemSystemInt32Array1Constructor1Delegate = new SystemSystemInt32Array1Constructor1DelegateType(SystemSystemInt32Array1Constructor1);
		static unsafe readonly SystemInt32Array1GetItem1DelegateType SystemInt32Array1GetItem1Delegate = new SystemInt32Array1GetItem1DelegateType(SystemInt32Array1GetItem1);
		static unsafe readonly SystemInt32Array1SetItem1DelegateType SystemInt32Array1SetItem1Delegate = new SystemInt32Array1SetItem1DelegateType(SystemInt32Array1SetItem1);
		static unsafe readonly ReleaseSystemActionDelegateType ReleaseSystemActionDelegate = new ReleaseSystemActionDelegateType(ReleaseSystemAction);
		static unsafe readonly SystemActionConstructorDelegateType SystemActionConstructorDelegate = new SystemActionConstructorDelegateType(SystemActionConstructor);
		static unsafe readonly SystemActionAddDelegateType SystemActionAddDelegate = new SystemActionAddDelegateType(SystemActionAdd);
		static unsafe readonly SystemActionRemoveDelegateType SystemActionRemoveDelegate = new SystemActionRemoveDelegateType(SystemActionRemove);
		static unsafe readonly SystemActionInvokeDelegateType SystemActionInvokeDelegate = new SystemActionInvokeDelegateType(SystemActionInvoke);
		static unsafe readonly ReleaseSystemActionUnityEngineAsyncOperationDelegateType ReleaseSystemActionUnityEngineAsyncOperationDelegate = new ReleaseSystemActionUnityEngineAsyncOperationDelegateType(ReleaseSystemActionUnityEngineAsyncOperation);
		static unsafe readonly SystemActionUnityEngineAsyncOperationConstructorDelegateType SystemActionUnityEngineAsyncOperationConstructorDelegate = new SystemActionUnityEngineAsyncOperationConstructorDelegateType(SystemActionUnityEngineAsyncOperationConstructor);
		static unsafe readonly SystemActionUnityEngineAsyncOperationAddDelegateType SystemActionUnityEngineAsyncOperationAddDelegate = new SystemActionUnityEngineAsyncOperationAddDelegateType(SystemActionUnityEngineAsyncOperationAdd);
		static unsafe readonly SystemActionUnityEngineAsyncOperationRemoveDelegateType SystemActionUnityEngineAsyncOperationRemoveDelegate = new SystemActionUnityEngineAsyncOperationRemoveDelegateType(SystemActionUnityEngineAsyncOperationRemove);
		static unsafe readonly SystemActionUnityEngineAsyncOperationInvokeDelegateType SystemActionUnityEngineAsyncOperationInvokeDelegate = new SystemActionUnityEngineAsyncOperationInvokeDelegateType(SystemActionUnityEngineAsyncOperationInvoke);
		/*END CSHARP DELEGATES*/
		
		/// <summary>
		/// Open the C++ plugin and call its PluginMain()
		/// </summary>
		/// 
		/// <param name="memorySize">
		/// Number of bytes of memory to make available to the C++ plugin
		/// </param>
		public static void Open(int memorySize)
		{
			/*BEGIN STORE INIT CALLS*/
			NativeScript.Bindings.ObjectStore.Init(10000);
			NativeScript.Bindings.StructStore<System.Decimal>.Init(10000);
			NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<byte>>.Init(10000);
			NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector3>>.Init(10000);
			NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector2>>.Init(10000);
			NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<int>>.Init(10000);
			/*END STORE INIT CALLS*/

			// Allocate unmanaged memory
			Bindings.memorySize = memorySize;
			memory = Marshal.AllocHGlobal(memorySize);

			// Allocate destroy queue
			destroyQueueCapacity = 128;
			destroyQueue = new DestroyEntry[destroyQueueCapacity];
			destroyQueueLockObj = new object();

			OpenPlugin(InitMode.FirstBoot);
		}
		
		// Reloading requires dynamic loading of the C++ plugin, which is only
		// available in the editor
#if UNITY_EDITOR
		/// <summary>
		/// Reload the C++ plugin. Its memory is intact and false is passed for
		/// the isFirstBoot parameter of PluginMain().
		/// </summary>
		public static void Reload()
		{
			DestroyAll();
			ClosePlugin();
			OpenPlugin(InitMode.Reload);
		}
		
		/// <summary>
		/// Poll the plugin for changes and reload if any are found.
		/// </summary>
		/// 
		/// <param name="pollTime">
		/// Number of seconds between polls.
		/// </param>
		/// 
		/// <returns>
		/// Enumerator for this iterator function. Can be passed to
		/// MonoBehaviour.StartCoroutine for easy usage.
		/// </returns>
		public static IEnumerator AutoReload(float pollTime)
		{
			// Get the original time
			long lastWriteTime = File.GetLastWriteTime(pluginPath).Ticks;
			
			ReusableWaitForSecondsRealtime poll
				= new ReusableWaitForSecondsRealtime(pollTime);
			do
			{
				// Poll. Reload if the last write time changed.
				long cur = File.GetLastWriteTime(pluginPath).Ticks;
				if (cur != lastWriteTime)
				{
					lastWriteTime = cur;
					Reload();
				}
				
				// Wait to poll again
				poll.WaitTime = pollTime;
				yield return poll;
			}
			while (true);
		}
#endif
		
		private static void OpenPlugin(InitMode initMode)
		{
#if UNITY_EDITOR
			string loadPath;
#if UNITY_EDITOR_WIN
			// Copy native library to temporary file
			try {
				File.Copy(pluginPath, pluginTempPath, true);
			} catch (Exception e)
			{
				Debug.Log("Failed to copy a new native DLL. If you changed it, please restart the Unity Editor or application. Error was: " + e.ToString());
			}
			loadPath = pluginTempPath;
#else
			loadPath = pluginPath;
#endif
			// Open native library
			libraryHandle = OpenLibrary(loadPath);
			InitDelegate Init = GetDelegate<InitDelegate>(
				libraryHandle,
				"Init");
			SetCsharpException = GetDelegate<SetCsharpExceptionDelegate>(
				libraryHandle,
				"SetCsharpException");
			/*BEGIN GETDELEGATE CALLS*/
			NewBaseNativeDownloadHandler = GetDelegate<NewBaseNativeDownloadHandlerDelegateType>(libraryHandle, "NewBaseNativeDownloadHandler");
			DestroyBaseNativeDownloadHandler = GetDelegate<DestroyBaseNativeDownloadHandlerDelegateType>(libraryHandle, "DestroyBaseNativeDownloadHandler");
			CesiumForUnityAbstractBaseNativeDownloadHandlerReceiveDataNative = GetDelegate<CesiumForUnityAbstractBaseNativeDownloadHandlerReceiveDataNativeDelegateType>(libraryHandle, "CesiumForUnityAbstractBaseNativeDownloadHandlerReceiveDataNative");
			NewBaseCesium3DTileset = GetDelegate<NewBaseCesium3DTilesetDelegateType>(libraryHandle, "NewBaseCesium3DTileset");
			DestroyBaseCesium3DTileset = GetDelegate<DestroyBaseCesium3DTilesetDelegateType>(libraryHandle, "DestroyBaseCesium3DTileset");
			CesiumForUnityAbstractBaseCesium3DTilesetStart = GetDelegate<CesiumForUnityAbstractBaseCesium3DTilesetStartDelegateType>(libraryHandle, "CesiumForUnityAbstractBaseCesium3DTilesetStart");
			CesiumForUnityAbstractBaseCesium3DTilesetUpdate = GetDelegate<CesiumForUnityAbstractBaseCesium3DTilesetUpdateDelegateType>(libraryHandle, "CesiumForUnityAbstractBaseCesium3DTilesetUpdate");
			SystemActionNativeInvoke = GetDelegate<SystemActionNativeInvokeDelegateType>(libraryHandle, "SystemActionNativeInvoke");
			SystemActionUnityEngineAsyncOperationNativeInvoke = GetDelegate<SystemActionUnityEngineAsyncOperationNativeInvokeDelegateType>(libraryHandle, "SystemActionUnityEngineAsyncOperationNativeInvoke");
			SetCsharpExceptionSystemNullReferenceException = GetDelegate<SetCsharpExceptionSystemNullReferenceExceptionDelegateType>(libraryHandle, "SetCsharpExceptionSystemNullReferenceException");
			/*END GETDELEGATE CALLS*/
#endif
			// Pass parameters through 'memory'
			int curMemory = 0;
			Marshal.WriteIntPtr(
				memory,
				curMemory,
				Marshal.GetFunctionPointerForDelegate(ReleaseObjectDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(
				memory,
				curMemory,
				Marshal.GetFunctionPointerForDelegate(StringNewDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(
				memory,
				curMemory,
				Marshal.GetFunctionPointerForDelegate(SetExceptionDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(
				memory,
				curMemory,
				Marshal.GetFunctionPointerForDelegate(ArrayGetLengthDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(
				memory,
				curMemory,
				Marshal.GetFunctionPointerForDelegate(EnumerableGetEnumeratorDelegate));
			curMemory += IntPtr.Size;
			
			/*BEGIN INIT CALL*/
			Marshal.WriteInt32(memory, curMemory, 10000); // max managed objects
			curMemory += sizeof(int);
 			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemIDisposableMethodDisposeDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(ReleaseSystemDecimalDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemDecimalConstructorSystemDoubleDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemDecimalConstructorSystemUInt64Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxDecimalDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxDecimalDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineVector2ConstructorSystemSingle_SystemSingleDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxVector2Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxVector2Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineVector3ConstructorSystemSingle_SystemSingle_SystemSingleDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxVector3Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxVector3Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineVector4ConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingleDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxVector4Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxVector4Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineQuaternionConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingleDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxQuaternionDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxQuaternionDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMatrix4x4ConstructorUnityEngineVector4_UnityEngineVector4_UnityEngineVector4_UnityEngineVector4Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMatrix4x4PropertyGetRotationDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMatrix4x4PropertyGetLossyScaleDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMatrix4x4MethodGetPositionDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxMatrix4x4Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxMatrix4x4Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineObjectPropertyGetNameDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineObjectPropertySetNameDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineComponentPropertyGetTransformDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTransformPropertyGetPositionDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTransformPropertySetPositionDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTransformPropertyGetRotationDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTransformPropertySetRotationDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTransformPropertyGetLocalScaleDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTransformPropertySetLocalScaleDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTransformPropertyGetLocalToWorldMatrixDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTransformPropertyGetParentDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTransformPropertySetParentDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTransformPropertyGetForwardDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTransformPropertySetForwardDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTransformPropertyGetUpDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTransformPropertySetUpDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemCollectionsIEnumeratorPropertyGetCurrentDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemCollectionsIEnumeratorMethodMoveNextDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineGameObjectConstructorSystemStringDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineGameObjectPropertyGetTransformDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineGameObjectMethodAddComponentCesiumForUnityBaseCesium3DTilesetDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineGameObjectMethodAddComponentUnityEngineMeshFilterDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineGameObjectMethodAddComponentUnityEngineMeshRendererDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveTypeDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineGameObjectMethodSetActiveSystemBooleanDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineDebugMethodLogSystemObjectDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMonoBehaviourPropertyGetTransformDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMonoBehaviourPropertyGetGameObjectDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemCollectionsGenericIEnumeratorUnityEngineVector2PropertyGetCurrentDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemCollectionsGenericIEnumeratorUnityEngineVector3PropertyGetCurrentDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemCollectionsGenericIEnumeratorSystemBytePropertyGetCurrentDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemCollectionsGenericIEnumeratorSystemInt32PropertyGetCurrentDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemCollectionsGenericIEnumerableUnityEngineVector2MethodGetEnumeratorDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemCollectionsGenericIEnumerableUnityEngineVector3MethodGetEnumeratorDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemCollectionsGenericIEnumerableSystemByteMethodGetEnumeratorDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemCollectionsGenericIEnumerableSystemInt32MethodGetEnumeratorDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxNativeArrayOptionsDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxNativeArrayOptionsDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(ReleaseUnityCollectionsNativeArraySystemByteDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityCollectionsNativeArraySystemByteConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityCollectionsNativeArraySystemByteMethodDisposeDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxNativeArraySystemByteDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxNativeArraySystemByteDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(ReleaseUnityCollectionsNativeArrayUnityEngineVector3Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityCollectionsNativeArrayUnityEngineVector3ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityCollectionsNativeArrayUnityEngineVector3MethodDisposeDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxNativeArrayUnityEngineVector3Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxNativeArrayUnityEngineVector3Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(ReleaseUnityCollectionsNativeArrayUnityEngineVector2Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityCollectionsNativeArrayUnityEngineVector2ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityCollectionsNativeArrayUnityEngineVector2MethodDisposeDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxNativeArrayUnityEngineVector2Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxNativeArrayUnityEngineVector2Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(ReleaseUnityCollectionsNativeArraySystemInt32Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityCollectionsNativeArraySystemInt32ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityCollectionsNativeArraySystemInt32MethodDisposeDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxNativeArraySystemInt32Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxNativeArraySystemInt32Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxAllocatorDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxAllocatorDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArraySystemByteSystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArrayUnityEngineVector3SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArrayUnityEngineVector2SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArraySystemInt32SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksUnityEngineVector3UnityCollectionsNativeArrayDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksUnityEngineVector2UnityCollectionsNativeArrayDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksSystemInt32UnityCollectionsNativeArrayDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxMeshTopologyDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxMeshTopologyDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMeshConstructorDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMeshMethodSetVerticesUnityEngineVector3Array1Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMeshMethodSetVerticesUnityEngineVector3UnityCollectionsNativeArrayDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMeshMethodSetNormalsUnityEngineVector3Array1Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMeshMethodSetNormalsUnityEngineVector3UnityCollectionsNativeArrayDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMeshMethodSetTrianglesSystemInt32Array1_SystemInt32_SystemBoolean_SystemInt32Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMeshMethodSetIndicesSystemInt32UnityCollectionsNativeArray_UnityEngineMeshTopology_SystemInt32_SystemBoolean_SystemInt32Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMeshMethodSetUVsSystemInt32_UnityEngineVector2Array1Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMeshMethodSetUVsUnityEngineVector2SystemInt32_UnityCollectionsNativeArrayDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMeshFilterPropertyGetMeshDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMeshFilterPropertySetMeshDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMeshRendererPropertyGetMaterialDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMeshRendererPropertySetMaterialDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemExceptionConstructorSystemStringDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxPrimitiveTypeDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxPrimitiveTypeDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTimePropertyGetDeltaTimeDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineCameraPropertyGetMainDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineCameraPropertyGetFieldOfViewDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineCameraPropertySetFieldOfViewDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineCameraPropertyGetAspectDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineCameraPropertySetAspectDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineCameraPropertyGetPixelWidthDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineCameraPropertyGetPixelHeightDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxRawDownloadedDataDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxRawDownloadedDataDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(ReleaseBaseNativeDownloadHandlerDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BaseNativeDownloadHandlerConstructorDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineNetworkingUnityWebRequestPropertyGetErrorDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineNetworkingUnityWebRequestPropertyGetIsDoneDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineNetworkingUnityWebRequestPropertyGetResponseCodeDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineNetworkingUnityWebRequestPropertyGetUrlDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineNetworkingUnityWebRequestPropertySetUrlDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineNetworkingUnityWebRequestPropertyGetMethodDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineNetworkingUnityWebRequestPropertySetMethodDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineNetworkingUnityWebRequestPropertyGetDownloadHandlerDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineNetworkingUnityWebRequestPropertySetDownloadHandlerDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineNetworkingUnityWebRequestMethodGetSystemStringDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineNetworkingUnityWebRequestMethodSetRequestHeaderSystemString_SystemStringDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineNetworkingUnityWebRequestMethodSendWebRequestDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineNetworkingUnityWebRequestMethodGetResponseHeaderSystemStringDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineNetworkingUnityWebRequestAsyncOperationAddEventCompletedDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineNetworkingUnityWebRequestAsyncOperationRemoveEventCompletedDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemRuntimeInteropServicesMarshalMethodStringToCoTaskMemUTF8SystemStringDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemRuntimeInteropServicesMarshalMethodFreeCoTaskMemSystemIntPtrDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(ReleaseBaseCesium3DTilesetDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BaseCesium3DTilesetConstructorDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemThreadingTasksTaskMethodRunSystemActionDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineMaterialMethodSetTextureSystemString_UnityEngineTextureDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineResourcesMethodLoadUnityEngineMaterialSystemStringDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEditorSceneViewPropertyGetLastActiveSceneViewDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEditorSceneViewPropertyGetCameraDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxTextureFormatDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxTextureFormatDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTexture2DConstructorSystemInt32_SystemInt32_UnityEngineTextureFormat_SystemBoolean_SystemBooleanDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTexture2DMethodSetPixelDataSystemByteSystemByteArray1_SystemInt32_SystemInt32Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTexture2DMethodSetPixelDataSystemByteUnityCollectionsNativeArray_SystemInt32_SystemInt32Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTexture2DMethodApplySystemBoolean_SystemBooleanDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineTexture2DMethodLoadRawTextureDataSystemIntPtr_SystemInt32Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineApplicationPropertyGetTemporaryCachePathDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineApplicationPropertyGetPersistentDataPathDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxBooleanDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxBooleanDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxSByteDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxSByteDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxByteDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxByteDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxInt16Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxInt16Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxUInt16Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxUInt16Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxInt32Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxInt32Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxUInt32Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxUInt32Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxInt64Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxInt64Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxUInt64Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxUInt64Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxCharDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxCharDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxSingleDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxSingleDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(BoxDoubleDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnboxDoubleDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineUnityEngineVector2Array1Constructor1Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineVector2Array1GetItem1Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineVector2Array1SetItem1Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineUnityEngineVector3Array1Constructor1Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineVector3Array1GetItem1Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(UnityEngineVector3Array1SetItem1Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemSystemByteArray1Constructor1Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemByteArray1GetItem1Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemByteArray1SetItem1Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemSystemInt32Array1Constructor1Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemInt32Array1GetItem1Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemInt32Array1SetItem1Delegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(ReleaseSystemActionDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemActionConstructorDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemActionAddDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemActionRemoveDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemActionInvokeDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(ReleaseSystemActionUnityEngineAsyncOperationDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemActionUnityEngineAsyncOperationConstructorDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemActionUnityEngineAsyncOperationAddDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemActionUnityEngineAsyncOperationRemoveDelegate));
			curMemory += IntPtr.Size;
			Marshal.WriteIntPtr(memory, curMemory, Marshal.GetFunctionPointerForDelegate(SystemActionUnityEngineAsyncOperationInvokeDelegate));
			curMemory += IntPtr.Size;
			/*END INIT CALL*/
			
			// Init C++ library
			Init(memory, memorySize, initMode);
			if (UnhandledCppException != null)
			{
				Exception ex = UnhandledCppException;
				UnhandledCppException = null;
				throw new Exception("Unhandled C++ exception in Init", ex);
			}
		}
		
		/// <summary>
		/// Close the C++ plugin
		/// </summary>
		public static void Close()
		{
			ClosePlugin();
			Marshal.FreeHGlobal(memory);
			memory = IntPtr.Zero;
		}

		/// <summary>
		/// Perform updates over time
		/// </summary>
		public static void Update()
		{
			DestroyAll();
		}
		
		private static void ClosePlugin()
		{
#if UNITY_EDITOR
			CloseLibrary(libraryHandle);
			libraryHandle = IntPtr.Zero;
#endif
#if UNITY_EDITOR_WIN
			File.Delete(pluginTempPath);
#endif
		}

		public static void QueueDestroy(DestroyFunction function, int cppHandle)
		{
			lock (destroyQueueLockObj)
			{
				// Grow capacity if necessary
				int count = destroyQueueCount;
				int capacity = destroyQueueCapacity;
				DestroyEntry[] queue = destroyQueue;
				if (count == capacity)
				{
					int newCapacity = capacity * 2;
					DestroyEntry[] newQueue = new DestroyEntry[newCapacity];
					for (int i = 0; i < capacity; ++i)
					{
						newQueue[i] = queue[i];
					}
					destroyQueueCapacity = newCapacity;
					destroyQueue = newQueue;
					queue = newQueue;
				}

				// Add to the end
				queue[count] = new DestroyEntry(function, cppHandle);
				destroyQueueCount = count + 1;
			}
		}

		static void DestroyAll()
		{
			lock (destroyQueueLockObj)
			{
				int count = destroyQueueCount;
				DestroyEntry[] queue = destroyQueue;
				for (int i = 0; i < count; ++i)
				{
					DestroyEntry entry = queue[i];
					switch (entry.Function)
					{
						/*BEGIN DESTROY QUEUE CASES*/
						case DestroyFunction.BaseNativeDownloadHandler:
							DestroyBaseNativeDownloadHandler(entry.CppHandle);
							break;
						case DestroyFunction.BaseCesium3DTileset:
							DestroyBaseCesium3DTileset(entry.CppHandle);
							break;
						/*END DESTROY QUEUE CASES*/
					}
				}
				destroyQueueCount = 0;
			}
		}
		
		////////////////////////////////////////////////////////////////
		// C# functions for C++ to call
		////////////////////////////////////////////////////////////////
		
		[MonoPInvokeCallback(typeof(ReleaseObjectDelegateType))]
		static void ReleaseObject(
			int handle)
		{
			if (handle != 0)
			{
				ObjectStore.Remove(handle);
			}
		}
		
		[MonoPInvokeCallback(typeof(StringNewDelegateType))]
		static int StringNew(
			string chars)
		{
			int handle = ObjectStore.Store(chars);
			return handle;
		}
		
		[MonoPInvokeCallback(typeof(SetExceptionDelegateType))]
		static void SetException(int handle)
		{
			UnhandledCppException = ObjectStore.Get(handle) as Exception;
		}
		
		[MonoPInvokeCallback(typeof(ArrayGetLengthDelegateType))]
		static int ArrayGetLength(int handle)
		{
			return ((Array)ObjectStore.Get(handle)).Length;
		}
		
		[MonoPInvokeCallback(typeof(EnumerableGetEnumeratorDelegateType))]
		static int EnumerableGetEnumerator(int handle)
		{
			return ObjectStore.Store(((IEnumerable)ObjectStore.Get(handle)).GetEnumerator());
		}

		/*BEGIN FUNCTIONS*/
		[MonoPInvokeCallback(typeof(SystemIDisposableMethodDisposeDelegateType))]
		static unsafe void SystemIDisposableMethodDispose(int thisHandle)
		{
			try
			{
				var thiz = (System.IDisposable)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				thiz.Dispose();
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(ReleaseSystemDecimalDelegateType))]
		static unsafe void ReleaseSystemDecimal(int handle)
		{
			try
			{
				if (handle != 0)
			{
				NativeScript.Bindings.StructStore<System.Decimal>.Remove(handle);
			}
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemDecimalConstructorSystemDoubleDelegateType))]
		static unsafe int SystemDecimalConstructorSystemDouble(double value)
		{
			try
			{
				var returnValue = NativeScript.Bindings.StructStore<System.Decimal>.Store(new System.Decimal(value));
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemDecimalConstructorSystemUInt64DelegateType))]
		static unsafe int SystemDecimalConstructorSystemUInt64(ulong value)
		{
			try
			{
				var returnValue = NativeScript.Bindings.StructStore<System.Decimal>.Store(new System.Decimal(value));
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxDecimalDelegateType))]
		static unsafe int BoxDecimal(int valHandle)
		{
			try
			{
				var val = (System.Decimal)NativeScript.Bindings.StructStore<System.Decimal>.Get(valHandle);
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxDecimalDelegateType))]
		static unsafe int UnboxDecimal(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = NativeScript.Bindings.StructStore<System.Decimal>.Store((System.Decimal)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineVector2ConstructorSystemSingle_SystemSingleDelegateType))]
		static unsafe UnityEngine.Vector2 UnityEngineVector2ConstructorSystemSingle_SystemSingle(float x, float y)
		{
			try
			{
				var returnValue = new UnityEngine.Vector2(x, y);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector2);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector2);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxVector2DelegateType))]
		static unsafe int BoxVector2(ref UnityEngine.Vector2 val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxVector2DelegateType))]
		static unsafe UnityEngine.Vector2 UnboxVector2(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (UnityEngine.Vector2)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector2);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector2);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineVector3ConstructorSystemSingle_SystemSingle_SystemSingleDelegateType))]
		static unsafe UnityEngine.Vector3 UnityEngineVector3ConstructorSystemSingle_SystemSingle_SystemSingle(float x, float y, float z)
		{
			try
			{
				var returnValue = new UnityEngine.Vector3(x, y, z);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxVector3DelegateType))]
		static unsafe int BoxVector3(ref UnityEngine.Vector3 val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxVector3DelegateType))]
		static unsafe UnityEngine.Vector3 UnboxVector3(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (UnityEngine.Vector3)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineVector4ConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingleDelegateType))]
		static unsafe UnityEngine.Vector4 UnityEngineVector4ConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingle(float x, float y, float z, float w)
		{
			try
			{
				var returnValue = new UnityEngine.Vector4(x, y, z, w);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector4);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector4);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxVector4DelegateType))]
		static unsafe int BoxVector4(ref UnityEngine.Vector4 val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxVector4DelegateType))]
		static unsafe UnityEngine.Vector4 UnboxVector4(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (UnityEngine.Vector4)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector4);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector4);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineQuaternionConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingleDelegateType))]
		static unsafe UnityEngine.Quaternion UnityEngineQuaternionConstructorSystemSingle_SystemSingle_SystemSingle_SystemSingle(float x, float y, float z, float w)
		{
			try
			{
				var returnValue = new UnityEngine.Quaternion(x, y, z, w);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Quaternion);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Quaternion);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxQuaternionDelegateType))]
		static unsafe int BoxQuaternion(ref UnityEngine.Quaternion val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxQuaternionDelegateType))]
		static unsafe UnityEngine.Quaternion UnboxQuaternion(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (UnityEngine.Quaternion)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Quaternion);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Quaternion);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMatrix4x4ConstructorUnityEngineVector4_UnityEngineVector4_UnityEngineVector4_UnityEngineVector4DelegateType))]
		static unsafe UnityEngine.Matrix4x4 UnityEngineMatrix4x4ConstructorUnityEngineVector4_UnityEngineVector4_UnityEngineVector4_UnityEngineVector4(ref UnityEngine.Vector4 column0, ref UnityEngine.Vector4 column1, ref UnityEngine.Vector4 column2, ref UnityEngine.Vector4 column3)
		{
			try
			{
				var returnValue = new UnityEngine.Matrix4x4(column0, column1, column2, column3);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Matrix4x4);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Matrix4x4);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMatrix4x4PropertyGetRotationDelegateType))]
		static unsafe UnityEngine.Quaternion UnityEngineMatrix4x4PropertyGetRotation(ref UnityEngine.Matrix4x4 thiz)
		{
			try
			{
				var returnValue = thiz.rotation;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Quaternion);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Quaternion);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMatrix4x4PropertyGetLossyScaleDelegateType))]
		static unsafe UnityEngine.Vector3 UnityEngineMatrix4x4PropertyGetLossyScale(ref UnityEngine.Matrix4x4 thiz)
		{
			try
			{
				var returnValue = thiz.lossyScale;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMatrix4x4MethodGetPositionDelegateType))]
		static unsafe UnityEngine.Vector3 UnityEngineMatrix4x4MethodGetPosition(ref UnityEngine.Matrix4x4 thiz)
		{
			try
			{
				var returnValue = thiz.GetPosition();
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxMatrix4x4DelegateType))]
		static unsafe int BoxMatrix4x4(ref UnityEngine.Matrix4x4 val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxMatrix4x4DelegateType))]
		static unsafe UnityEngine.Matrix4x4 UnboxMatrix4x4(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (UnityEngine.Matrix4x4)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Matrix4x4);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Matrix4x4);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineObjectPropertyGetNameDelegateType))]
		static unsafe int UnityEngineObjectPropertyGetName(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Object)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.name;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineObjectPropertySetNameDelegateType))]
		static unsafe void UnityEngineObjectPropertySetName(int thisHandle, int valueHandle)
		{
			try
			{
				var thiz = (UnityEngine.Object)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var value = (string)NativeScript.Bindings.ObjectStore.Get(valueHandle);
				thiz.name = value;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineComponentPropertyGetTransformDelegateType))]
		static unsafe int UnityEngineComponentPropertyGetTransform(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Component)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.transform;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTransformPropertyGetPositionDelegateType))]
		static unsafe UnityEngine.Vector3 UnityEngineTransformPropertyGetPosition(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Transform)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.position;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTransformPropertySetPositionDelegateType))]
		static unsafe void UnityEngineTransformPropertySetPosition(int thisHandle, ref UnityEngine.Vector3 value)
		{
			try
			{
				var thiz = (UnityEngine.Transform)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				thiz.position = value;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTransformPropertyGetRotationDelegateType))]
		static unsafe UnityEngine.Quaternion UnityEngineTransformPropertyGetRotation(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Transform)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.rotation;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Quaternion);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Quaternion);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTransformPropertySetRotationDelegateType))]
		static unsafe void UnityEngineTransformPropertySetRotation(int thisHandle, ref UnityEngine.Quaternion value)
		{
			try
			{
				var thiz = (UnityEngine.Transform)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				thiz.rotation = value;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTransformPropertyGetLocalScaleDelegateType))]
		static unsafe UnityEngine.Vector3 UnityEngineTransformPropertyGetLocalScale(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Transform)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.localScale;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTransformPropertySetLocalScaleDelegateType))]
		static unsafe void UnityEngineTransformPropertySetLocalScale(int thisHandle, ref UnityEngine.Vector3 value)
		{
			try
			{
				var thiz = (UnityEngine.Transform)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				thiz.localScale = value;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTransformPropertyGetLocalToWorldMatrixDelegateType))]
		static unsafe UnityEngine.Matrix4x4 UnityEngineTransformPropertyGetLocalToWorldMatrix(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Transform)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.localToWorldMatrix;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Matrix4x4);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Matrix4x4);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTransformPropertyGetParentDelegateType))]
		static unsafe int UnityEngineTransformPropertyGetParent(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Transform)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.parent;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTransformPropertySetParentDelegateType))]
		static unsafe void UnityEngineTransformPropertySetParent(int thisHandle, int valueHandle)
		{
			try
			{
				var thiz = (UnityEngine.Transform)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var value = (UnityEngine.Transform)NativeScript.Bindings.ObjectStore.Get(valueHandle);
				thiz.parent = value;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTransformPropertyGetForwardDelegateType))]
		static unsafe UnityEngine.Vector3 UnityEngineTransformPropertyGetForward(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Transform)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.forward;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTransformPropertySetForwardDelegateType))]
		static unsafe void UnityEngineTransformPropertySetForward(int thisHandle, ref UnityEngine.Vector3 value)
		{
			try
			{
				var thiz = (UnityEngine.Transform)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				thiz.forward = value;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTransformPropertyGetUpDelegateType))]
		static unsafe UnityEngine.Vector3 UnityEngineTransformPropertyGetUp(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Transform)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.up;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTransformPropertySetUpDelegateType))]
		static unsafe void UnityEngineTransformPropertySetUp(int thisHandle, ref UnityEngine.Vector3 value)
		{
			try
			{
				var thiz = (UnityEngine.Transform)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				thiz.up = value;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemCollectionsIEnumeratorPropertyGetCurrentDelegateType))]
		static unsafe int SystemCollectionsIEnumeratorPropertyGetCurrent(int thisHandle)
		{
			try
			{
				var thiz = (System.Collections.IEnumerator)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.Current;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemCollectionsIEnumeratorMethodMoveNextDelegateType))]
		static unsafe bool SystemCollectionsIEnumeratorMethodMoveNext(int thisHandle)
		{
			try
			{
				var thiz = (System.Collections.IEnumerator)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.MoveNext();
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(bool);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(bool);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineGameObjectConstructorSystemStringDelegateType))]
		static unsafe int UnityEngineGameObjectConstructorSystemString(int nameHandle)
		{
			try
			{
				var name = (string)NativeScript.Bindings.ObjectStore.Get(nameHandle);
				var returnValue = NativeScript.Bindings.ObjectStore.Store(new UnityEngine.GameObject(name));
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineGameObjectPropertyGetTransformDelegateType))]
		static unsafe int UnityEngineGameObjectPropertyGetTransform(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.GameObject)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.transform;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineGameObjectMethodAddComponentCesiumForUnityBaseCesium3DTilesetDelegateType))]
		static unsafe int UnityEngineGameObjectMethodAddComponentCesiumForUnityBaseCesium3DTileset(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.GameObject)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.AddComponent<CesiumForUnity.BaseCesium3DTileset>();
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineGameObjectMethodAddComponentUnityEngineMeshFilterDelegateType))]
		static unsafe int UnityEngineGameObjectMethodAddComponentUnityEngineMeshFilter(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.GameObject)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.AddComponent<UnityEngine.MeshFilter>();
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineGameObjectMethodAddComponentUnityEngineMeshRendererDelegateType))]
		static unsafe int UnityEngineGameObjectMethodAddComponentUnityEngineMeshRenderer(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.GameObject)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.AddComponent<UnityEngine.MeshRenderer>();
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveTypeDelegateType))]
		static unsafe int UnityEngineGameObjectMethodCreatePrimitiveUnityEnginePrimitiveType(UnityEngine.PrimitiveType type)
		{
			try
			{
				var returnValue = UnityEngine.GameObject.CreatePrimitive(type);
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineGameObjectMethodSetActiveSystemBooleanDelegateType))]
		static unsafe void UnityEngineGameObjectMethodSetActiveSystemBoolean(int thisHandle, bool value)
		{
			try
			{
				var thiz = (UnityEngine.GameObject)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				thiz.SetActive(value);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineDebugMethodLogSystemObjectDelegateType))]
		static unsafe void UnityEngineDebugMethodLogSystemObject(int messageHandle)
		{
			try
			{
				var message = NativeScript.Bindings.ObjectStore.Get(messageHandle);
				UnityEngine.Debug.Log(message);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMonoBehaviourPropertyGetTransformDelegateType))]
		static unsafe int UnityEngineMonoBehaviourPropertyGetTransform(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.MonoBehaviour)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.transform;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMonoBehaviourPropertyGetGameObjectDelegateType))]
		static unsafe int UnityEngineMonoBehaviourPropertyGetGameObject(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.MonoBehaviour)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.gameObject;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemCollectionsGenericIEnumeratorUnityEngineVector2PropertyGetCurrentDelegateType))]
		static unsafe UnityEngine.Vector2 SystemCollectionsGenericIEnumeratorUnityEngineVector2PropertyGetCurrent(int thisHandle)
		{
			try
			{
				var thiz = (System.Collections.Generic.IEnumerator<UnityEngine.Vector2>)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.Current;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector2);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector2);
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemCollectionsGenericIEnumeratorUnityEngineVector3PropertyGetCurrentDelegateType))]
		static unsafe UnityEngine.Vector3 SystemCollectionsGenericIEnumeratorUnityEngineVector3PropertyGetCurrent(int thisHandle)
		{
			try
			{
				var thiz = (System.Collections.Generic.IEnumerator<UnityEngine.Vector3>)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.Current;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemCollectionsGenericIEnumeratorSystemBytePropertyGetCurrentDelegateType))]
		static unsafe byte SystemCollectionsGenericIEnumeratorSystemBytePropertyGetCurrent(int thisHandle)
		{
			try
			{
				var thiz = (System.Collections.Generic.IEnumerator<byte>)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.Current;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(byte);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(byte);
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemCollectionsGenericIEnumeratorSystemInt32PropertyGetCurrentDelegateType))]
		static unsafe int SystemCollectionsGenericIEnumeratorSystemInt32PropertyGetCurrent(int thisHandle)
		{
			try
			{
				var thiz = (System.Collections.Generic.IEnumerator<int>)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.Current;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemCollectionsGenericIEnumerableUnityEngineVector2MethodGetEnumeratorDelegateType))]
		static unsafe int SystemCollectionsGenericIEnumerableUnityEngineVector2MethodGetEnumerator(int thisHandle)
		{
			try
			{
				var thiz = (System.Collections.Generic.IEnumerable<UnityEngine.Vector2>)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.GetEnumerator();
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemCollectionsGenericIEnumerableUnityEngineVector3MethodGetEnumeratorDelegateType))]
		static unsafe int SystemCollectionsGenericIEnumerableUnityEngineVector3MethodGetEnumerator(int thisHandle)
		{
			try
			{
				var thiz = (System.Collections.Generic.IEnumerable<UnityEngine.Vector3>)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.GetEnumerator();
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemCollectionsGenericIEnumerableSystemByteMethodGetEnumeratorDelegateType))]
		static unsafe int SystemCollectionsGenericIEnumerableSystemByteMethodGetEnumerator(int thisHandle)
		{
			try
			{
				var thiz = (System.Collections.Generic.IEnumerable<byte>)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.GetEnumerator();
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemCollectionsGenericIEnumerableSystemInt32MethodGetEnumeratorDelegateType))]
		static unsafe int SystemCollectionsGenericIEnumerableSystemInt32MethodGetEnumerator(int thisHandle)
		{
			try
			{
				var thiz = (System.Collections.Generic.IEnumerable<int>)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.GetEnumerator();
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxNativeArrayOptionsDelegateType))]
		static unsafe int BoxNativeArrayOptions(Unity.Collections.NativeArrayOptions val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxNativeArrayOptionsDelegateType))]
		static unsafe Unity.Collections.NativeArrayOptions UnboxNativeArrayOptions(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (Unity.Collections.NativeArrayOptions)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(Unity.Collections.NativeArrayOptions);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(Unity.Collections.NativeArrayOptions);
			}
		}
		
		[MonoPInvokeCallback(typeof(ReleaseUnityCollectionsNativeArraySystemByteDelegateType))]
		static unsafe void ReleaseUnityCollectionsNativeArraySystemByte(int handle)
		{
			try
			{
				if (handle != 0)
			{
				NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<byte>>.Remove(handle);
			}
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityCollectionsNativeArraySystemByteConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegateType))]
		static unsafe int UnityCollectionsNativeArraySystemByteConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptions(int length, Unity.Collections.Allocator allocator, Unity.Collections.NativeArrayOptions options)
		{
			try
			{
				var returnValue = NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<byte>>.Store(new Unity.Collections.NativeArray<byte>(length, allocator, options));
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityCollectionsNativeArraySystemByteMethodDisposeDelegateType))]
		static unsafe void UnityCollectionsNativeArraySystemByteMethodDispose(int thisHandle)
		{
			try
			{
				var thiz = (Unity.Collections.NativeArray<byte>)NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<byte>>.Get(thisHandle);
				thiz.Dispose();
				NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<byte>>.Replace(thisHandle, ref thiz);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxNativeArraySystemByteDelegateType))]
		static unsafe int BoxNativeArraySystemByte(int valHandle)
		{
			try
			{
				var val = (Unity.Collections.NativeArray<byte>)NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<byte>>.Get(valHandle);
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxNativeArraySystemByteDelegateType))]
		static unsafe int UnboxNativeArraySystemByte(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<byte>>.Store((Unity.Collections.NativeArray<byte>)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(ReleaseUnityCollectionsNativeArrayUnityEngineVector3DelegateType))]
		static unsafe void ReleaseUnityCollectionsNativeArrayUnityEngineVector3(int handle)
		{
			try
			{
				if (handle != 0)
			{
				NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector3>>.Remove(handle);
			}
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityCollectionsNativeArrayUnityEngineVector3ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegateType))]
		static unsafe int UnityCollectionsNativeArrayUnityEngineVector3ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptions(int length, Unity.Collections.Allocator allocator, Unity.Collections.NativeArrayOptions options)
		{
			try
			{
				var returnValue = NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector3>>.Store(new Unity.Collections.NativeArray<UnityEngine.Vector3>(length, allocator, options));
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityCollectionsNativeArrayUnityEngineVector3MethodDisposeDelegateType))]
		static unsafe void UnityCollectionsNativeArrayUnityEngineVector3MethodDispose(int thisHandle)
		{
			try
			{
				var thiz = (Unity.Collections.NativeArray<UnityEngine.Vector3>)NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector3>>.Get(thisHandle);
				thiz.Dispose();
				NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector3>>.Replace(thisHandle, ref thiz);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxNativeArrayUnityEngineVector3DelegateType))]
		static unsafe int BoxNativeArrayUnityEngineVector3(int valHandle)
		{
			try
			{
				var val = (Unity.Collections.NativeArray<UnityEngine.Vector3>)NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector3>>.Get(valHandle);
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxNativeArrayUnityEngineVector3DelegateType))]
		static unsafe int UnboxNativeArrayUnityEngineVector3(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector3>>.Store((Unity.Collections.NativeArray<UnityEngine.Vector3>)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(ReleaseUnityCollectionsNativeArrayUnityEngineVector2DelegateType))]
		static unsafe void ReleaseUnityCollectionsNativeArrayUnityEngineVector2(int handle)
		{
			try
			{
				if (handle != 0)
			{
				NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector2>>.Remove(handle);
			}
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityCollectionsNativeArrayUnityEngineVector2ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegateType))]
		static unsafe int UnityCollectionsNativeArrayUnityEngineVector2ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptions(int length, Unity.Collections.Allocator allocator, Unity.Collections.NativeArrayOptions options)
		{
			try
			{
				var returnValue = NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector2>>.Store(new Unity.Collections.NativeArray<UnityEngine.Vector2>(length, allocator, options));
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityCollectionsNativeArrayUnityEngineVector2MethodDisposeDelegateType))]
		static unsafe void UnityCollectionsNativeArrayUnityEngineVector2MethodDispose(int thisHandle)
		{
			try
			{
				var thiz = (Unity.Collections.NativeArray<UnityEngine.Vector2>)NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector2>>.Get(thisHandle);
				thiz.Dispose();
				NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector2>>.Replace(thisHandle, ref thiz);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxNativeArrayUnityEngineVector2DelegateType))]
		static unsafe int BoxNativeArrayUnityEngineVector2(int valHandle)
		{
			try
			{
				var val = (Unity.Collections.NativeArray<UnityEngine.Vector2>)NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector2>>.Get(valHandle);
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxNativeArrayUnityEngineVector2DelegateType))]
		static unsafe int UnboxNativeArrayUnityEngineVector2(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector2>>.Store((Unity.Collections.NativeArray<UnityEngine.Vector2>)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(ReleaseUnityCollectionsNativeArraySystemInt32DelegateType))]
		static unsafe void ReleaseUnityCollectionsNativeArraySystemInt32(int handle)
		{
			try
			{
				if (handle != 0)
			{
				NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<int>>.Remove(handle);
			}
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityCollectionsNativeArraySystemInt32ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptionsDelegateType))]
		static unsafe int UnityCollectionsNativeArraySystemInt32ConstructorSystemInt32_UnityCollectionsAllocator_UnityCollectionsNativeArrayOptions(int length, Unity.Collections.Allocator allocator, Unity.Collections.NativeArrayOptions options)
		{
			try
			{
				var returnValue = NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<int>>.Store(new Unity.Collections.NativeArray<int>(length, allocator, options));
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityCollectionsNativeArraySystemInt32MethodDisposeDelegateType))]
		static unsafe void UnityCollectionsNativeArraySystemInt32MethodDispose(int thisHandle)
		{
			try
			{
				var thiz = (Unity.Collections.NativeArray<int>)NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<int>>.Get(thisHandle);
				thiz.Dispose();
				NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<int>>.Replace(thisHandle, ref thiz);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxNativeArraySystemInt32DelegateType))]
		static unsafe int BoxNativeArraySystemInt32(int valHandle)
		{
			try
			{
				var val = (Unity.Collections.NativeArray<int>)NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<int>>.Get(valHandle);
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxNativeArraySystemInt32DelegateType))]
		static unsafe int UnboxNativeArraySystemInt32(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<int>>.Store((Unity.Collections.NativeArray<int>)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxAllocatorDelegateType))]
		static unsafe int BoxAllocator(Unity.Collections.Allocator val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxAllocatorDelegateType))]
		static unsafe Unity.Collections.Allocator UnboxAllocator(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (Unity.Collections.Allocator)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(Unity.Collections.Allocator);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(Unity.Collections.Allocator);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArraySystemByteSystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegateType))]
		static unsafe int UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArraySystemByteSystemVoidPointer_SystemInt32_UnityCollectionsAllocator(void* dataPointer, int length, Unity.Collections.Allocator allocator)
		{
			try
			{
				var returnValue = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(dataPointer, length, allocator);
				return NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<byte>>.Store(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArrayUnityEngineVector3SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegateType))]
		static unsafe int UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArrayUnityEngineVector3SystemVoidPointer_SystemInt32_UnityCollectionsAllocator(void* dataPointer, int length, Unity.Collections.Allocator allocator)
		{
			try
			{
				var returnValue = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<UnityEngine.Vector3>(dataPointer, length, allocator);
				return NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector3>>.Store(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArrayUnityEngineVector2SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegateType))]
		static unsafe int UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArrayUnityEngineVector2SystemVoidPointer_SystemInt32_UnityCollectionsAllocator(void* dataPointer, int length, Unity.Collections.Allocator allocator)
		{
			try
			{
				var returnValue = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<UnityEngine.Vector2>(dataPointer, length, allocator);
				return NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector2>>.Store(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArraySystemInt32SystemVoidPointer_SystemInt32_UnityCollectionsAllocatorDelegateType))]
		static unsafe int UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodConvertExistingDataToNativeArraySystemInt32SystemVoidPointer_SystemInt32_UnityCollectionsAllocator(void* dataPointer, int length, Unity.Collections.Allocator allocator)
		{
			try
			{
				var returnValue = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(dataPointer, length, allocator);
				return NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<int>>.Store(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksUnityEngineVector3UnityCollectionsNativeArrayDelegateType))]
		static unsafe void* UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksUnityEngineVector3UnityCollectionsNativeArray(int nativeArrayHandle)
		{
			try
			{
				var nativeArray = (Unity.Collections.NativeArray<UnityEngine.Vector3>)NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector3>>.Get(nativeArrayHandle);
				var returnValue = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks<UnityEngine.Vector3>(nativeArray);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(void*);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(void*);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksUnityEngineVector2UnityCollectionsNativeArrayDelegateType))]
		static unsafe void* UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksUnityEngineVector2UnityCollectionsNativeArray(int nativeArrayHandle)
		{
			try
			{
				var nativeArray = (Unity.Collections.NativeArray<UnityEngine.Vector2>)NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector2>>.Get(nativeArrayHandle);
				var returnValue = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks<UnityEngine.Vector2>(nativeArray);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(void*);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(void*);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksSystemInt32UnityCollectionsNativeArrayDelegateType))]
		static unsafe void* UnityCollectionsLowLevelUnsafeNativeArrayUnsafeUtilityMethodGetUnsafeBufferPointerWithoutChecksSystemInt32UnityCollectionsNativeArray(int nativeArrayHandle)
		{
			try
			{
				var nativeArray = (Unity.Collections.NativeArray<int>)NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<int>>.Get(nativeArrayHandle);
				var returnValue = Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks<int>(nativeArray);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(void*);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(void*);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxMeshTopologyDelegateType))]
		static unsafe int BoxMeshTopology(UnityEngine.MeshTopology val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxMeshTopologyDelegateType))]
		static unsafe UnityEngine.MeshTopology UnboxMeshTopology(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (UnityEngine.MeshTopology)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.MeshTopology);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.MeshTopology);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMeshConstructorDelegateType))]
		static unsafe int UnityEngineMeshConstructor()
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store(new UnityEngine.Mesh());
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMeshMethodSetVerticesUnityEngineVector3Array1DelegateType))]
		static unsafe void UnityEngineMeshMethodSetVerticesUnityEngineVector3Array1(int thisHandle, int inVerticesHandle)
		{
			try
			{
				var thiz = (UnityEngine.Mesh)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var inVertices = (UnityEngine.Vector3[])NativeScript.Bindings.ObjectStore.Get(inVerticesHandle);
				thiz.SetVertices(inVertices);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMeshMethodSetVerticesUnityEngineVector3UnityCollectionsNativeArrayDelegateType))]
		static unsafe void UnityEngineMeshMethodSetVerticesUnityEngineVector3UnityCollectionsNativeArray(int thisHandle, int inVerticesHandle)
		{
			try
			{
				var thiz = (UnityEngine.Mesh)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var inVertices = (Unity.Collections.NativeArray<UnityEngine.Vector3>)NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector3>>.Get(inVerticesHandle);
				thiz.SetVertices<UnityEngine.Vector3>(inVertices);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMeshMethodSetNormalsUnityEngineVector3Array1DelegateType))]
		static unsafe void UnityEngineMeshMethodSetNormalsUnityEngineVector3Array1(int thisHandle, int inNormalsHandle)
		{
			try
			{
				var thiz = (UnityEngine.Mesh)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var inNormals = (UnityEngine.Vector3[])NativeScript.Bindings.ObjectStore.Get(inNormalsHandle);
				thiz.SetNormals(inNormals);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMeshMethodSetNormalsUnityEngineVector3UnityCollectionsNativeArrayDelegateType))]
		static unsafe void UnityEngineMeshMethodSetNormalsUnityEngineVector3UnityCollectionsNativeArray(int thisHandle, int inNormalsHandle)
		{
			try
			{
				var thiz = (UnityEngine.Mesh)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var inNormals = (Unity.Collections.NativeArray<UnityEngine.Vector3>)NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector3>>.Get(inNormalsHandle);
				thiz.SetNormals<UnityEngine.Vector3>(inNormals);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMeshMethodSetTrianglesSystemInt32Array1_SystemInt32_SystemBoolean_SystemInt32DelegateType))]
		static unsafe void UnityEngineMeshMethodSetTrianglesSystemInt32Array1_SystemInt32_SystemBoolean_SystemInt32(int thisHandle, int trianglesHandle, int submesh, bool calculateBounds, int baseVertex)
		{
			try
			{
				var thiz = (UnityEngine.Mesh)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var triangles = (int[])NativeScript.Bindings.ObjectStore.Get(trianglesHandle);
				thiz.SetTriangles(triangles, submesh, calculateBounds, baseVertex);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMeshMethodSetIndicesSystemInt32UnityCollectionsNativeArray_UnityEngineMeshTopology_SystemInt32_SystemBoolean_SystemInt32DelegateType))]
		static unsafe void UnityEngineMeshMethodSetIndicesSystemInt32UnityCollectionsNativeArray_UnityEngineMeshTopology_SystemInt32_SystemBoolean_SystemInt32(int thisHandle, int indicesHandle, UnityEngine.MeshTopology topology, int submesh, bool calculateBounds, int baseVertex)
		{
			try
			{
				var thiz = (UnityEngine.Mesh)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var indices = (Unity.Collections.NativeArray<int>)NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<int>>.Get(indicesHandle);
				thiz.SetIndices<int>(indices, topology, submesh, calculateBounds, baseVertex);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMeshMethodSetUVsSystemInt32_UnityEngineVector2Array1DelegateType))]
		static unsafe void UnityEngineMeshMethodSetUVsSystemInt32_UnityEngineVector2Array1(int thisHandle, int channel, int uvsHandle)
		{
			try
			{
				var thiz = (UnityEngine.Mesh)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var uvs = (UnityEngine.Vector2[])NativeScript.Bindings.ObjectStore.Get(uvsHandle);
				thiz.SetUVs(channel, uvs);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMeshMethodSetUVsUnityEngineVector2SystemInt32_UnityCollectionsNativeArrayDelegateType))]
		static unsafe void UnityEngineMeshMethodSetUVsUnityEngineVector2SystemInt32_UnityCollectionsNativeArray(int thisHandle, int channel, int uvsHandle)
		{
			try
			{
				var thiz = (UnityEngine.Mesh)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var uvs = (Unity.Collections.NativeArray<UnityEngine.Vector2>)NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<UnityEngine.Vector2>>.Get(uvsHandle);
				thiz.SetUVs<UnityEngine.Vector2>(channel, uvs);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMeshFilterPropertyGetMeshDelegateType))]
		static unsafe int UnityEngineMeshFilterPropertyGetMesh(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.MeshFilter)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.mesh;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMeshFilterPropertySetMeshDelegateType))]
		static unsafe void UnityEngineMeshFilterPropertySetMesh(int thisHandle, int valueHandle)
		{
			try
			{
				var thiz = (UnityEngine.MeshFilter)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var value = (UnityEngine.Mesh)NativeScript.Bindings.ObjectStore.Get(valueHandle);
				thiz.mesh = value;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMeshRendererPropertyGetMaterialDelegateType))]
		static unsafe int UnityEngineMeshRendererPropertyGetMaterial(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.MeshRenderer)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.material;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMeshRendererPropertySetMaterialDelegateType))]
		static unsafe void UnityEngineMeshRendererPropertySetMaterial(int thisHandle, int valueHandle)
		{
			try
			{
				var thiz = (UnityEngine.MeshRenderer)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var value = (UnityEngine.Material)NativeScript.Bindings.ObjectStore.Get(valueHandle);
				thiz.material = value;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemExceptionConstructorSystemStringDelegateType))]
		static unsafe int SystemExceptionConstructorSystemString(int messageHandle)
		{
			try
			{
				var message = (string)NativeScript.Bindings.ObjectStore.Get(messageHandle);
				var returnValue = NativeScript.Bindings.ObjectStore.Store(new System.Exception(message));
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxPrimitiveTypeDelegateType))]
		static unsafe int BoxPrimitiveType(UnityEngine.PrimitiveType val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxPrimitiveTypeDelegateType))]
		static unsafe UnityEngine.PrimitiveType UnboxPrimitiveType(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (UnityEngine.PrimitiveType)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.PrimitiveType);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.PrimitiveType);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTimePropertyGetDeltaTimeDelegateType))]
		static unsafe float UnityEngineTimePropertyGetDeltaTime()
		{
			try
			{
				var returnValue = UnityEngine.Time.deltaTime;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(float);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(float);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineCameraPropertyGetMainDelegateType))]
		static unsafe int UnityEngineCameraPropertyGetMain()
		{
			try
			{
				var returnValue = UnityEngine.Camera.main;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineCameraPropertyGetFieldOfViewDelegateType))]
		static unsafe float UnityEngineCameraPropertyGetFieldOfView(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Camera)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.fieldOfView;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(float);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(float);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineCameraPropertySetFieldOfViewDelegateType))]
		static unsafe void UnityEngineCameraPropertySetFieldOfView(int thisHandle, float value)
		{
			try
			{
				var thiz = (UnityEngine.Camera)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				thiz.fieldOfView = value;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineCameraPropertyGetAspectDelegateType))]
		static unsafe float UnityEngineCameraPropertyGetAspect(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Camera)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.aspect;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(float);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(float);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineCameraPropertySetAspectDelegateType))]
		static unsafe void UnityEngineCameraPropertySetAspect(int thisHandle, float value)
		{
			try
			{
				var thiz = (UnityEngine.Camera)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				thiz.aspect = value;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineCameraPropertyGetPixelWidthDelegateType))]
		static unsafe int UnityEngineCameraPropertyGetPixelWidth(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Camera)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.pixelWidth;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineCameraPropertyGetPixelHeightDelegateType))]
		static unsafe int UnityEngineCameraPropertyGetPixelHeight(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Camera)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.pixelHeight;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxRawDownloadedDataDelegateType))]
		static unsafe int BoxRawDownloadedData(ref CesiumForUnity.RawDownloadedData val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxRawDownloadedDataDelegateType))]
		static unsafe CesiumForUnity.RawDownloadedData UnboxRawDownloadedData(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (CesiumForUnity.RawDownloadedData)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(CesiumForUnity.RawDownloadedData);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(CesiumForUnity.RawDownloadedData);
			}
		}
		
		[MonoPInvokeCallback(typeof(BaseNativeDownloadHandlerConstructorDelegateType))]
		static unsafe void BaseNativeDownloadHandlerConstructor(int cppHandle, ref int handle)
		{
			try
			{
				var thiz = new CesiumForUnity.BaseNativeDownloadHandler(cppHandle);
				handle = NativeScript.Bindings.ObjectStore.Store(thiz);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				handle = default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				handle = default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(ReleaseBaseNativeDownloadHandlerDelegateType))]
		static unsafe void ReleaseBaseNativeDownloadHandler(int handle)
		{
			try
			{
				CesiumForUnity.BaseNativeDownloadHandler thiz;
				thiz = (CesiumForUnity.BaseNativeDownloadHandler)ObjectStore.Get(handle);
				int cppHandle = thiz.CppHandle;
				thiz.CppHandle = 0;
				QueueDestroy(DestroyFunction.BaseNativeDownloadHandler, cppHandle);
				ObjectStore.Remove(handle);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineNetworkingUnityWebRequestPropertyGetErrorDelegateType))]
		static unsafe int UnityEngineNetworkingUnityWebRequestPropertyGetError(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Networking.UnityWebRequest)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.error;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineNetworkingUnityWebRequestPropertyGetIsDoneDelegateType))]
		static unsafe bool UnityEngineNetworkingUnityWebRequestPropertyGetIsDone(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Networking.UnityWebRequest)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.isDone;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(bool);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(bool);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineNetworkingUnityWebRequestPropertyGetResponseCodeDelegateType))]
		static unsafe long UnityEngineNetworkingUnityWebRequestPropertyGetResponseCode(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Networking.UnityWebRequest)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.responseCode;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(long);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(long);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineNetworkingUnityWebRequestPropertyGetUrlDelegateType))]
		static unsafe int UnityEngineNetworkingUnityWebRequestPropertyGetUrl(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Networking.UnityWebRequest)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.url;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineNetworkingUnityWebRequestPropertySetUrlDelegateType))]
		static unsafe void UnityEngineNetworkingUnityWebRequestPropertySetUrl(int thisHandle, int valueHandle)
		{
			try
			{
				var thiz = (UnityEngine.Networking.UnityWebRequest)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var value = (string)NativeScript.Bindings.ObjectStore.Get(valueHandle);
				thiz.url = value;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineNetworkingUnityWebRequestPropertyGetMethodDelegateType))]
		static unsafe int UnityEngineNetworkingUnityWebRequestPropertyGetMethod(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Networking.UnityWebRequest)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.method;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineNetworkingUnityWebRequestPropertySetMethodDelegateType))]
		static unsafe void UnityEngineNetworkingUnityWebRequestPropertySetMethod(int thisHandle, int valueHandle)
		{
			try
			{
				var thiz = (UnityEngine.Networking.UnityWebRequest)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var value = (string)NativeScript.Bindings.ObjectStore.Get(valueHandle);
				thiz.method = value;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineNetworkingUnityWebRequestPropertyGetDownloadHandlerDelegateType))]
		static unsafe int UnityEngineNetworkingUnityWebRequestPropertyGetDownloadHandler(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Networking.UnityWebRequest)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.downloadHandler;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineNetworkingUnityWebRequestPropertySetDownloadHandlerDelegateType))]
		static unsafe void UnityEngineNetworkingUnityWebRequestPropertySetDownloadHandler(int thisHandle, int valueHandle)
		{
			try
			{
				var thiz = (UnityEngine.Networking.UnityWebRequest)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var value = (UnityEngine.Networking.DownloadHandler)NativeScript.Bindings.ObjectStore.Get(valueHandle);
				thiz.downloadHandler = value;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineNetworkingUnityWebRequestMethodGetSystemStringDelegateType))]
		static unsafe int UnityEngineNetworkingUnityWebRequestMethodGetSystemString(int uriHandle)
		{
			try
			{
				var uri = (string)NativeScript.Bindings.ObjectStore.Get(uriHandle);
				var returnValue = UnityEngine.Networking.UnityWebRequest.Get(uri);
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineNetworkingUnityWebRequestMethodSetRequestHeaderSystemString_SystemStringDelegateType))]
		static unsafe void UnityEngineNetworkingUnityWebRequestMethodSetRequestHeaderSystemString_SystemString(int thisHandle, int nameHandle, int valueHandle)
		{
			try
			{
				var thiz = (UnityEngine.Networking.UnityWebRequest)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var name = (string)NativeScript.Bindings.ObjectStore.Get(nameHandle);
				var value = (string)NativeScript.Bindings.ObjectStore.Get(valueHandle);
				thiz.SetRequestHeader(name, value);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineNetworkingUnityWebRequestMethodSendWebRequestDelegateType))]
		static unsafe int UnityEngineNetworkingUnityWebRequestMethodSendWebRequest(int thisHandle)
		{
			try
			{
				var thiz = (UnityEngine.Networking.UnityWebRequest)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.SendWebRequest();
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineNetworkingUnityWebRequestMethodGetResponseHeaderSystemStringDelegateType))]
		static unsafe int UnityEngineNetworkingUnityWebRequestMethodGetResponseHeaderSystemString(int thisHandle, int nameHandle)
		{
			try
			{
				var thiz = (UnityEngine.Networking.UnityWebRequest)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var name = (string)NativeScript.Bindings.ObjectStore.Get(nameHandle);
				var returnValue = thiz.GetResponseHeader(name);
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineNetworkingUnityWebRequestAsyncOperationAddEventCompletedDelegateType))]
		static unsafe void UnityEngineNetworkingUnityWebRequestAsyncOperationAddEventCompleted(int thisHandle, int delHandle)
		{
			try
			{
				var thiz = (UnityEngine.Networking.UnityWebRequestAsyncOperation)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var del = (System.Action<UnityEngine.AsyncOperation>)NativeScript.Bindings.ObjectStore.Get(delHandle);
				thiz.completed += del;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineNetworkingUnityWebRequestAsyncOperationRemoveEventCompletedDelegateType))]
		static unsafe void UnityEngineNetworkingUnityWebRequestAsyncOperationRemoveEventCompleted(int thisHandle, int delHandle)
		{
			try
			{
				var thiz = (UnityEngine.Networking.UnityWebRequestAsyncOperation)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var del = (System.Action<UnityEngine.AsyncOperation>)NativeScript.Bindings.ObjectStore.Get(delHandle);
				thiz.completed -= del;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemRuntimeInteropServicesMarshalMethodStringToCoTaskMemUTF8SystemStringDelegateType))]
		static unsafe System.IntPtr SystemRuntimeInteropServicesMarshalMethodStringToCoTaskMemUTF8SystemString(int sHandle)
		{
			try
			{
				var s = (string)NativeScript.Bindings.ObjectStore.Get(sHandle);
				var returnValue = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUTF8(s);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(System.IntPtr);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(System.IntPtr);
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemRuntimeInteropServicesMarshalMethodFreeCoTaskMemSystemIntPtrDelegateType))]
		static unsafe void SystemRuntimeInteropServicesMarshalMethodFreeCoTaskMemSystemIntPtr(System.IntPtr ptr)
		{
			try
			{
				System.Runtime.InteropServices.Marshal.FreeCoTaskMem(ptr);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(BaseCesium3DTilesetConstructorDelegateType))]
		static unsafe void BaseCesium3DTilesetConstructor(int cppHandle, ref int handle)
		{
			try
			{
				var thiz = new CesiumForUnity.BaseCesium3DTileset(cppHandle);
				handle = NativeScript.Bindings.ObjectStore.Store(thiz);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				handle = default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				handle = default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(ReleaseBaseCesium3DTilesetDelegateType))]
		static unsafe void ReleaseBaseCesium3DTileset(int handle)
		{
			try
			{
				CesiumForUnity.BaseCesium3DTileset thiz;
				thiz = (CesiumForUnity.BaseCesium3DTileset)ObjectStore.Get(handle);
				int cppHandle = thiz.CppHandle;
				thiz.CppHandle = 0;
				QueueDestroy(DestroyFunction.BaseCesium3DTileset, cppHandle);
				ObjectStore.Remove(handle);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemThreadingTasksTaskMethodRunSystemActionDelegateType))]
		static unsafe int SystemThreadingTasksTaskMethodRunSystemAction(int actionHandle)
		{
			try
			{
				var action = (System.Action)NativeScript.Bindings.ObjectStore.Get(actionHandle);
				var returnValue = System.Threading.Tasks.Task.Run(action);
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineMaterialMethodSetTextureSystemString_UnityEngineTextureDelegateType))]
		static unsafe void UnityEngineMaterialMethodSetTextureSystemString_UnityEngineTexture(int thisHandle, int nameHandle, int valueHandle)
		{
			try
			{
				var thiz = (UnityEngine.Material)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var name = (string)NativeScript.Bindings.ObjectStore.Get(nameHandle);
				var value = (UnityEngine.Texture)NativeScript.Bindings.ObjectStore.Get(valueHandle);
				thiz.SetTexture(name, value);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineResourcesMethodLoadUnityEngineMaterialSystemStringDelegateType))]
		static unsafe int UnityEngineResourcesMethodLoadUnityEngineMaterialSystemString(int pathHandle)
		{
			try
			{
				var path = (string)NativeScript.Bindings.ObjectStore.Get(pathHandle);
				var returnValue = UnityEngine.Resources.Load<UnityEngine.Material>(path);
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEditorSceneViewPropertyGetLastActiveSceneViewDelegateType))]
		static unsafe int UnityEditorSceneViewPropertyGetLastActiveSceneView()
		{
			try
			{
				var returnValue = UnityEditor.SceneView.lastActiveSceneView;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEditorSceneViewPropertyGetCameraDelegateType))]
		static unsafe int UnityEditorSceneViewPropertyGetCamera(int thisHandle)
		{
			try
			{
				var thiz = (UnityEditor.SceneView)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz.camera;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxTextureFormatDelegateType))]
		static unsafe int BoxTextureFormat(UnityEngine.TextureFormat val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxTextureFormatDelegateType))]
		static unsafe UnityEngine.TextureFormat UnboxTextureFormat(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (UnityEngine.TextureFormat)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.TextureFormat);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.TextureFormat);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTexture2DConstructorSystemInt32_SystemInt32_UnityEngineTextureFormat_SystemBoolean_SystemBooleanDelegateType))]
		static unsafe int UnityEngineTexture2DConstructorSystemInt32_SystemInt32_UnityEngineTextureFormat_SystemBoolean_SystemBoolean(int width, int height, UnityEngine.TextureFormat textureFormat, bool mipChain, bool linear)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store(new UnityEngine.Texture2D(width, height, textureFormat, mipChain, linear));
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTexture2DMethodSetPixelDataSystemByteSystemByteArray1_SystemInt32_SystemInt32DelegateType))]
		static unsafe void UnityEngineTexture2DMethodSetPixelDataSystemByteSystemByteArray1_SystemInt32_SystemInt32(int thisHandle, int dataHandle, int mipLevel, int sourceDataStartIndex)
		{
			try
			{
				var thiz = (UnityEngine.Texture2D)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var data = (byte[])NativeScript.Bindings.ObjectStore.Get(dataHandle);
				thiz.SetPixelData<byte>(data, mipLevel, sourceDataStartIndex);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTexture2DMethodSetPixelDataSystemByteUnityCollectionsNativeArray_SystemInt32_SystemInt32DelegateType))]
		static unsafe void UnityEngineTexture2DMethodSetPixelDataSystemByteUnityCollectionsNativeArray_SystemInt32_SystemInt32(int thisHandle, int dataHandle, int mipLevel, int sourceDataStartIndex)
		{
			try
			{
				var thiz = (UnityEngine.Texture2D)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var data = (Unity.Collections.NativeArray<byte>)NativeScript.Bindings.StructStore<Unity.Collections.NativeArray<byte>>.Get(dataHandle);
				thiz.SetPixelData<byte>(data, mipLevel, sourceDataStartIndex);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTexture2DMethodApplySystemBoolean_SystemBooleanDelegateType))]
		static unsafe void UnityEngineTexture2DMethodApplySystemBoolean_SystemBoolean(int thisHandle, bool updateMipmaps, bool makeNoLongerReadable)
		{
			try
			{
				var thiz = (UnityEngine.Texture2D)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				thiz.Apply(updateMipmaps, makeNoLongerReadable);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineTexture2DMethodLoadRawTextureDataSystemIntPtr_SystemInt32DelegateType))]
		static unsafe void UnityEngineTexture2DMethodLoadRawTextureDataSystemIntPtr_SystemInt32(int thisHandle, System.IntPtr data, int size)
		{
			try
			{
				var thiz = (UnityEngine.Texture2D)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				thiz.LoadRawTextureData(data, size);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineApplicationPropertyGetTemporaryCachePathDelegateType))]
		static unsafe int UnityEngineApplicationPropertyGetTemporaryCachePath()
		{
			try
			{
				var returnValue = UnityEngine.Application.temporaryCachePath;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineApplicationPropertyGetPersistentDataPathDelegateType))]
		static unsafe int UnityEngineApplicationPropertyGetPersistentDataPath()
		{
			try
			{
				var returnValue = UnityEngine.Application.persistentDataPath;
				return NativeScript.Bindings.ObjectStore.GetHandle(returnValue);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxBooleanDelegateType))]
		static unsafe int BoxBoolean(bool val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxBooleanDelegateType))]
		static unsafe bool UnboxBoolean(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (bool)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(bool);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(bool);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxSByteDelegateType))]
		static unsafe int BoxSByte(sbyte val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxSByteDelegateType))]
		static unsafe sbyte UnboxSByte(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (sbyte)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(sbyte);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(sbyte);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxByteDelegateType))]
		static unsafe int BoxByte(byte val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxByteDelegateType))]
		static unsafe byte UnboxByte(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (byte)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(byte);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(byte);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxInt16DelegateType))]
		static unsafe int BoxInt16(short val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxInt16DelegateType))]
		static unsafe short UnboxInt16(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (short)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(short);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(short);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxUInt16DelegateType))]
		static unsafe int BoxUInt16(ushort val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxUInt16DelegateType))]
		static unsafe ushort UnboxUInt16(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (ushort)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(ushort);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(ushort);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxInt32DelegateType))]
		static unsafe int BoxInt32(int val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxInt32DelegateType))]
		static unsafe int UnboxInt32(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (int)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxUInt32DelegateType))]
		static unsafe int BoxUInt32(uint val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxUInt32DelegateType))]
		static unsafe uint UnboxUInt32(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (uint)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(uint);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(uint);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxInt64DelegateType))]
		static unsafe int BoxInt64(long val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxInt64DelegateType))]
		static unsafe long UnboxInt64(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (long)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(long);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(long);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxUInt64DelegateType))]
		static unsafe int BoxUInt64(ulong val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxUInt64DelegateType))]
		static unsafe ulong UnboxUInt64(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (ulong)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(ulong);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(ulong);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxCharDelegateType))]
		static unsafe int BoxChar(char val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxCharDelegateType))]
		static unsafe char UnboxChar(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (char)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(char);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(char);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxSingleDelegateType))]
		static unsafe int BoxSingle(float val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxSingleDelegateType))]
		static unsafe float UnboxSingle(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (float)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(float);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(float);
			}
		}
		
		[MonoPInvokeCallback(typeof(BoxDoubleDelegateType))]
		static unsafe int BoxDouble(double val)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store((object)val);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnboxDoubleDelegateType))]
		static unsafe double UnboxDouble(int valHandle)
		{
			try
			{
				var val = NativeScript.Bindings.ObjectStore.Get(valHandle);
				var returnValue = (double)val;
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(double);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(double);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineUnityEngineVector2Array1Constructor1DelegateType))]
		static unsafe int UnityEngineUnityEngineVector2Array1Constructor1(int length0)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store(new UnityEngine.Vector2[length0]);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineVector2Array1GetItem1DelegateType))]
		static unsafe UnityEngine.Vector2 UnityEngineVector2Array1GetItem1(int thisHandle, int index0)
		{
			try
			{
				var thiz = (UnityEngine.Vector2[])NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz[index0];
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector2);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector2);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineVector2Array1SetItem1DelegateType))]
		static unsafe void UnityEngineVector2Array1SetItem1(int thisHandle, int index0, ref UnityEngine.Vector2 item)
		{
			try
			{
				var thiz = (UnityEngine.Vector2[])NativeScript.Bindings.ObjectStore.Get(thisHandle);
				thiz[index0] = item;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineUnityEngineVector3Array1Constructor1DelegateType))]
		static unsafe int UnityEngineUnityEngineVector3Array1Constructor1(int length0)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store(new UnityEngine.Vector3[length0]);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineVector3Array1GetItem1DelegateType))]
		static unsafe UnityEngine.Vector3 UnityEngineVector3Array1GetItem1(int thisHandle, int index0)
		{
			try
			{
				var thiz = (UnityEngine.Vector3[])NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz[index0];
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(UnityEngine.Vector3);
			}
		}
		
		[MonoPInvokeCallback(typeof(UnityEngineVector3Array1SetItem1DelegateType))]
		static unsafe void UnityEngineVector3Array1SetItem1(int thisHandle, int index0, ref UnityEngine.Vector3 item)
		{
			try
			{
				var thiz = (UnityEngine.Vector3[])NativeScript.Bindings.ObjectStore.Get(thisHandle);
				thiz[index0] = item;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemSystemByteArray1Constructor1DelegateType))]
		static unsafe int SystemSystemByteArray1Constructor1(int length0)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store(new byte[length0]);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemByteArray1GetItem1DelegateType))]
		static unsafe byte SystemByteArray1GetItem1(int thisHandle, int index0)
		{
			try
			{
				var thiz = (byte[])NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz[index0];
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(byte);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(byte);
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemByteArray1SetItem1DelegateType))]
		static unsafe void SystemByteArray1SetItem1(int thisHandle, int index0, byte item)
		{
			try
			{
				var thiz = (byte[])NativeScript.Bindings.ObjectStore.Get(thisHandle);
				thiz[index0] = item;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemSystemInt32Array1Constructor1DelegateType))]
		static unsafe int SystemSystemInt32Array1Constructor1(int length0)
		{
			try
			{
				var returnValue = NativeScript.Bindings.ObjectStore.Store(new int[length0]);
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemInt32Array1GetItem1DelegateType))]
		static unsafe int SystemInt32Array1GetItem1(int thisHandle, int index0)
		{
			try
			{
				var thiz = (int[])NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var returnValue = thiz[index0];
				return returnValue;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				return default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemInt32Array1SetItem1DelegateType))]
		static unsafe void SystemInt32Array1SetItem1(int thisHandle, int index0, int item)
		{
			try
			{
				var thiz = (int[])NativeScript.Bindings.ObjectStore.Get(thisHandle);
				thiz[index0] = item;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemActionInvokeDelegateType))]
		static unsafe void SystemActionInvoke(int thisHandle)
		{
			try
			{
				((System.Action)NativeScript.Bindings.ObjectStore.Get(thisHandle))();
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemActionConstructorDelegateType))]
		static unsafe void SystemActionConstructor(int cppHandle, ref int handle, ref int classHandle)
		{
			try
			{
				var thiz = new SystemAction(cppHandle);
				classHandle = NativeScript.Bindings.ObjectStore.Store(thiz);
				handle = NativeScript.Bindings.ObjectStore.Store(thiz.Delegate);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				handle = default(int);
				classHandle = default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				handle = default(int);
				classHandle = default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(ReleaseSystemActionDelegateType))]
		static unsafe void ReleaseSystemAction(int handle, int classHandle)
		{
			try
			{
				SystemAction thiz;
				if (classHandle != 0)
				{
					thiz = (SystemAction)ObjectStore.Remove(classHandle);
					thiz.CppHandle = 0;
				}
				
				ObjectStore.Remove(handle);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemActionAddDelegateType))]
		static unsafe void SystemActionAdd(int thisHandle, int delHandle)
		{
			try
			{
				var thiz = (System.Action)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var del = (System.Action)NativeScript.Bindings.ObjectStore.Get(delHandle);
				thiz += del;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemActionRemoveDelegateType))]
		static unsafe void SystemActionRemove(int thisHandle, int delHandle)
		{
			try
			{
				var thiz = (System.Action)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var del = (System.Action)NativeScript.Bindings.ObjectStore.Get(delHandle);
				thiz -= del;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemActionUnityEngineAsyncOperationInvokeDelegateType))]
		static unsafe void SystemActionUnityEngineAsyncOperationInvoke(int thisHandle, int objHandle)
		{
			try
			{
				var obj = (UnityEngine.AsyncOperation)NativeScript.Bindings.ObjectStore.Get(objHandle);
				((System.Action<UnityEngine.AsyncOperation>)NativeScript.Bindings.ObjectStore.Get(thisHandle))(obj);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemActionUnityEngineAsyncOperationConstructorDelegateType))]
		static unsafe void SystemActionUnityEngineAsyncOperationConstructor(int cppHandle, ref int handle, ref int classHandle)
		{
			try
			{
				var thiz = new SystemActionUnityEngineAsyncOperation(cppHandle);
				classHandle = NativeScript.Bindings.ObjectStore.Store(thiz);
				handle = NativeScript.Bindings.ObjectStore.Store(thiz.Delegate);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
				handle = default(int);
				classHandle = default(int);
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
				handle = default(int);
				classHandle = default(int);
			}
		}
		
		[MonoPInvokeCallback(typeof(ReleaseSystemActionUnityEngineAsyncOperationDelegateType))]
		static unsafe void ReleaseSystemActionUnityEngineAsyncOperation(int handle, int classHandle)
		{
			try
			{
				SystemActionUnityEngineAsyncOperation thiz;
				if (classHandle != 0)
				{
					thiz = (SystemActionUnityEngineAsyncOperation)ObjectStore.Remove(classHandle);
					thiz.CppHandle = 0;
				}
				
				ObjectStore.Remove(handle);
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemActionUnityEngineAsyncOperationAddDelegateType))]
		static unsafe void SystemActionUnityEngineAsyncOperationAdd(int thisHandle, int delHandle)
		{
			try
			{
				var thiz = (System.Action<UnityEngine.AsyncOperation>)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var del = (System.Action<UnityEngine.AsyncOperation>)NativeScript.Bindings.ObjectStore.Get(delHandle);
				thiz += del;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		
		[MonoPInvokeCallback(typeof(SystemActionUnityEngineAsyncOperationRemoveDelegateType))]
		static unsafe void SystemActionUnityEngineAsyncOperationRemove(int thisHandle, int delHandle)
		{
			try
			{
				var thiz = (System.Action<UnityEngine.AsyncOperation>)NativeScript.Bindings.ObjectStore.Get(thisHandle);
				var del = (System.Action<UnityEngine.AsyncOperation>)NativeScript.Bindings.ObjectStore.Get(delHandle);
				thiz -= del;
			}
			catch (System.NullReferenceException ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpExceptionSystemNullReferenceException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogException(ex);
				NativeScript.Bindings.SetCsharpException(NativeScript.Bindings.ObjectStore.Store(ex));
			}
		}
		/*END FUNCTIONS*/
	}
}

/*BEGIN BASE TYPES*/
namespace CesiumForUnity
{
	class BaseNativeDownloadHandler : CesiumForUnity.AbstractBaseNativeDownloadHandler
	{
		public int CppHandle;
		
		public BaseNativeDownloadHandler()
		{
			int handle = NativeScript.Bindings.ObjectStore.Store(this);
			CppHandle = NativeScript.Bindings.NewBaseNativeDownloadHandler(handle);
		}
		
		~BaseNativeDownloadHandler()
		{
			if (CppHandle != 0)
			{
				NativeScript.Bindings.QueueDestroy(NativeScript.Bindings.DestroyFunction.BaseNativeDownloadHandler, CppHandle);
				CppHandle = 0;
			}
		}
		
		public BaseNativeDownloadHandler(int cppHandle)
			: base()
		{
			CppHandle = cppHandle;
		}
		
		public override bool ReceiveDataNative(System.IntPtr data, int dataLength)
		{
			if (CppHandle != 0)
			{
				int thisHandle = CppHandle;
				var returnVal = NativeScript.Bindings.CesiumForUnityAbstractBaseNativeDownloadHandlerReceiveDataNative(thisHandle, data, dataLength);
				if (NativeScript.Bindings.UnhandledCppException != null)
				{
					Exception ex = NativeScript.Bindings.UnhandledCppException;
					NativeScript.Bindings.UnhandledCppException = null;
					throw ex;
				}
				return returnVal;
			}
			return default(bool);
		}
	
	}
}

namespace CesiumForUnity
{
	class BaseCesium3DTileset : CesiumForUnity.AbstractBaseCesium3DTileset
	{
		public int CppHandle;
		
		public BaseCesium3DTileset()
		{
		}
		
		public void Awake()
		{
			Plugin.Init();
			if (CppHandle == 0)
			{
				int handle = NativeScript.Bindings.ObjectStore.Store(this);
				CppHandle = NativeScript.Bindings.NewBaseCesium3DTileset(handle);
			}
		}
		
		~BaseCesium3DTileset()
		{
			if (CppHandle != 0)
			{
				NativeScript.Bindings.QueueDestroy(NativeScript.Bindings.DestroyFunction.BaseCesium3DTileset, CppHandle);
				CppHandle = 0;
			}
		}
		
		public BaseCesium3DTileset(int cppHandle)
			: base()
		{
			CppHandle = cppHandle;
		}
		
		public override void Start()
		{
			if (CppHandle != 0)
			{
				int thisHandle = CppHandle;
				NativeScript.Bindings.CesiumForUnityAbstractBaseCesium3DTilesetStart(thisHandle);
				if (NativeScript.Bindings.UnhandledCppException != null)
				{
					Exception ex = NativeScript.Bindings.UnhandledCppException;
					NativeScript.Bindings.UnhandledCppException = null;
					throw ex;
				}
			}
		}
	
		public override void Update()
		{
			if (CppHandle != 0)
			{
				int thisHandle = CppHandle;
				NativeScript.Bindings.CesiumForUnityAbstractBaseCesium3DTilesetUpdate(thisHandle);
				if (NativeScript.Bindings.UnhandledCppException != null)
				{
					Exception ex = NativeScript.Bindings.UnhandledCppException;
					NativeScript.Bindings.UnhandledCppException = null;
					throw ex;
				}
			}
		}
	
	}
}

class SystemAction
{
	public int CppHandle;
	public System.Action Delegate;
	
	public SystemAction(int cppHandle)
	{
		CppHandle = cppHandle;
		Delegate = NativeInvoke;
	}
	
		public void NativeInvoke()
		{
			if (CppHandle != 0)
			{
				int thisHandle = CppHandle;
				NativeScript.Bindings.SystemActionNativeInvoke(thisHandle);
				if (NativeScript.Bindings.UnhandledCppException != null)
				{
					Exception ex = NativeScript.Bindings.UnhandledCppException;
					NativeScript.Bindings.UnhandledCppException = null;
					throw ex;
				}
			}
		}
	
}

class SystemActionUnityEngineAsyncOperation
{
	public int CppHandle;
	public System.Action<UnityEngine.AsyncOperation> Delegate;
	
	public SystemActionUnityEngineAsyncOperation(int cppHandle)
	{
		CppHandle = cppHandle;
		Delegate = NativeInvoke;
	}
	
		public void NativeInvoke(UnityEngine.AsyncOperation obj)
		{
			if (CppHandle != 0)
			{
				int thisHandle = CppHandle;
				int objHandle = NativeScript.Bindings.ObjectStore.GetHandle(obj);
				NativeScript.Bindings.SystemActionUnityEngineAsyncOperationNativeInvoke(thisHandle, objHandle);
				if (NativeScript.Bindings.UnhandledCppException != null)
				{
					Exception ex = NativeScript.Bindings.UnhandledCppException;
					NativeScript.Bindings.UnhandledCppException = null;
					throw ex;
				}
			}
		}
	
}
/*END BASE TYPES*/