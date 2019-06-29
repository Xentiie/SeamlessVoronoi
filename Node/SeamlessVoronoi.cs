
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.ShaderGraph;
using System.Reflection;

[Title("Procedural", "Noise", "Seamless Voronoi")]
public class SeamlessVoronoi : CodeFunctionNode {

	public SeamlessVoronoi() {

		name = "Seamless Voronoi";
		previewExpanded = false;

	}

	public override bool hasPreview { get { return true; } }

protected override MethodInfo GetFunctionToConvert() {
	return GetType().GetMethod("CalculateVoronoi", BindingFlags.Static | BindingFlags.NonPublic);
}

static string CalculateVoronoi(
		[Slot(0, Binding.MeshUV0)] Vector2 UV,
		[Slot(1, Binding.None, 2.0f, 0, 0, 0)] Vector1 Height,
		[Slot(2, Binding.None, 5.0f, 5.0f, 5.0f, 5.0f)] Vector1 CellDensity,
		[Slot(3, Binding.None, 2.0f, 2.0f, 2.0f, 2.0f)] Vector3 Period,
		[Slot(4, Binding.None)] out Vector1 Voronoi,
		[Slot(5, Binding.None)] out Vector1 Cells,
		[Slot(6, Binding.None)] out Vector1 CellBorder) {
	return
		@"
{

float3 value = float3(UV, Height) * CellDensity;
float3 noise = voronoiNoise(value, Period);


Cells = noise.y;
Voronoi = noise.x;
CellBorder = noise.z;
}
";
}

public override void GenerateNodeFunction(FunctionRegistry registry, GraphContext graphContext, GenerationMode generationMode) {

	registry.ProvideFunction("modulo", s => s.Append(@"
inline float3 modulo(float3 divident, float3 divisor){
float3 positiveDivident = divident % divisor + divisor;
return positiveDivident % divisor;
}

"));


	registry.ProvideFunction("rand3dto1d", s => s.Append(@"

float rand3dTo1d(float3 value, float3 dotDir = float3(12.9898, 78.233, 37.719)){
//make value smaller to avoid artefacts
float3 smallValue = sin(value);
//get scalar value from 3d vector
float random = dot(smallValue, dotDir);
//make value more random by making it bigger and then taking the factional part
random = frac(sin(random) * 143758.5453);
return random;
}

"));


	registry.ProvideFunction("rand3dTo3d", s => s.Append(@"

float3 rand3dTo3d(float3 value){
return float3(
	rand3dTo1d(value, float3(12.989, 78.233, 37.719)),
	rand3dTo1d(value, float3(39.346, 11.135, 83.155)),
	rand3dTo1d(value, float3(73.156, 52.235, 09.151))
);
}

"));


	registry.ProvideFunction("voronoiNoise", s => s.Append(@"
float3 voronoiNoise(float3 value, float3 period){
float3 baseCell = floor(value);

//first pass to find the closest cell
float minDistToCell = 10;
float3 toClosestCell;
float3 closestCell;
[unroll]
for(int x1=-1; x1<=1; x1++){
	[unroll]
	for(int y1=-1; y1<=1; y1++){
		[unroll]
		for(int z1=-1; z1<=1; z1++){
			float3 cell = baseCell + float3(x1, y1, z1);
			float3 tiledCell = modulo(cell, period);
			float3 cellPosition = cell + rand3dTo3d(tiledCell);
			float3 toCell = cellPosition - value;
			float distToCell = length(toCell);
			if(distToCell < minDistToCell){
				minDistToCell = distToCell;
				closestCell = cell;
				toClosestCell = toCell;
			}
		}
	}
}

//second pass to find the distance to the closest edge
float minEdgeDistance = 10;
[unroll]
for(int x2=-1; x2<=1; x2++){
	[unroll]
	for(int y2=-1; y2<=1; y2++){
		[unroll]
		for(int z2=-1; z2<=1; z2++){
			float3 cell = baseCell + float3(x2, y2, z2);
			float3 tiledCell = modulo(cell, period);
			float3 cellPosition = cell + rand3dTo3d(tiledCell);
			float3 toCell = cellPosition - value;

			float3 diffToClosestCell = abs(closestCell - cell);
			bool isClosestCell = diffToClosestCell.x + diffToClosestCell.y + diffToClosestCell.z < 0.1;
			if(!isClosestCell){
				float3 toCenter = (toClosestCell + toCell) * 0.5;
				float3 cellDifference = normalize(toCell - toClosestCell);
				float edgeDistance = dot(toCenter, cellDifference);
				minEdgeDistance = min(minEdgeDistance, edgeDistance);
			}
		}
	}
}

float random = rand3dTo1d(closestCell);
return float3(minDistToCell, random, minEdgeDistance);
}
"));

	base.GenerateNodeFunction(registry, graphContext, generationMode);
}

}
