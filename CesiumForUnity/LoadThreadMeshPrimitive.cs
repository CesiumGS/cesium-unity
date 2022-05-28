using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace CesiumForUnity
{
    /// <summary>
    /// Holds details of a glTF mesh primitive populated on the load thread
    /// and ready for use on the main thread
    /// </summary>
    internal class LoadThreadMeshPrimitive
    {
        NativeArray<Vector3>? positions;
        NativeArray<Vector3>? normals;
        NativeArray<Vector4>? tangents;
        NativeArray<Vector2>[]? uvs;
        NativeArray<Color>? colors;
        NativeArray<Color32>? colors32;

        // TODO: with something like the below, we can probably use the glTF buffers directly and avoid a copy.
        //VertexAttributeDescriptor[]? attributeDescriptors;
        //NativeArray<byte>[]? buffers;
        //NativeArray<int>? triangles;
    }
}
