using UnityEngine;

[CreateAssetMenu(fileName = "ComputeMaterialLibrary", menuName = "Scriptable Objects/ComputeMaterialLibrary")]
public class ComputeMaterialLibrary : ScriptableObject
{
    public ComputeShader CleanBufer;
    public ComputeShader LatLongArrayGather;
    public ComputeShader LatLongAverage;
    public ComputeShader LUTArrayGather;
}
