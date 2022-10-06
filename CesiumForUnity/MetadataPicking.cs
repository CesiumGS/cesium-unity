using UnityEngine;
using CesiumForUnity;
using System;
using System.Linq;
public class MetadataPicking : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity))
            {
                var metadataScript = hit.transform.GetComponentInParent<CesiumMetadata>();
                if(metadataScript != null){
                    metadataScript.loadMetadata(hit.transform, hit.triangleIndex);
                    foreach(var kvp in metadataScript.Keys().Zip(metadataScript.Values(), Tuple.Create)){
                        Debug.Log(kvp.Item1 + ": " + kvp.Item2.GetString("null"));
                    }
                }
            }

        }
    }
}