﻿#ifndef _IFPIPELINE_PUBLIC_H
#define _IFPIPELINE_PUBLIC_H
#include "Common.hlsl"




struct clusternumlights_t
{
	uint offset;
	uint numItems;
};


StructuredBuffer<uint> _ItemsIDList;
StructuredBuffer<clusternumlights_t> _ClusterNumItems;

uint GetClusterIndexZ(float viewZ)
{
	float slice = log2(max(1.0, viewZ / _ClusterLighting.x)) / _ClusterLighting.y;
	//float a1 = log2(max(1.0, viewZ / _ClusterLighting.x));
	//float a2 = log2(max(1.0, _ClusterLighting.y / _ClusterLighting.x));//todoCPU算
	//float slice = (_ClusterInfo.z - 1) * (a1/a2);

	return  min(_ClusterInfo.z - 1.0, floor(slice));
}
uint3 ComputeClusterIndex3D (float2 screenPos, float viewZ)
{
	uint3 clusterCoordinate;
	clusterCoordinate.z = GetClusterIndexZ(viewZ);
	clusterCoordinate.x = floor (screenPos.x / _ClusterCB_Size.x);
	/*clusterCoordinate.x = min (clusterCoordinate.x, MACRO_NAME (_ClusterInfo).x);
	clusterCoordinate.x = max (0, clusterCoordinate.x);*/

	clusterCoordinate.y = floor (screenPos.y / _ClusterCB_Size.y);
	//clusterCoordinate.y = min (clusterCoordinate.y, MACRO_NAME (_ClusterInfo).y);
	//clusterCoordinate.y = max (0, clusterCoordinate.y);

	return clusterCoordinate;
}

uint ComputeClusterIndex1D (uint3 clusterCoordinate)
{
	return (clusterCoordinate.x + (clusterCoordinate.y * _ClusterInfo.x))
		+ ((clusterCoordinate.z * _ClusterInfo.x) * _ClusterInfo.y);
}

uint GetLightClusterIndex(float2 tc, float z_in)
{
//#if UNITY_REVERSED_Z
//	float z = -z_in;
//#else
//	float z = z_in;
//#endif
	float z = z_in;
//#if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D12) || defined(SHADER_API_D3D11_9X) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_PSSL)
//	tc.y = _IFScreenSize.y - tc.y - 1;//HLSL y 方向反的...
//#else
//	tc.y = _IFScreenSize.y - tc.y - 1;//HLSL y 方向反的...
//#endif
	//tc.y = 1 - tc.y;
	//tc.x = tc.x * _IFScreenSize.x;
	//tc.y = _IFScreenSize.y - tc.y * _IFScreenSize.y - 1;
	uint3 clusterCoordinate;
	clusterCoordinate.x = uint(tc.x * float(_ClusterInfo.x));
	clusterCoordinate.y = uint((1 - tc.y) * float(_ClusterInfo.y));
	clusterCoordinate.z = GetClusterIndexZ(z_in);
	//clusterCoordinate = ComputeClusterIndex3D(tc, z_in);
	return ComputeClusterIndex1D(clusterCoordinate);
}

struct ClusterData
{
	uint lightsMin;
	uint lightsMax;
	uint decalsMin;
	uint decalsMax;
};


ClusterData GetClusterData (int clusterOffset)
{
	ClusterData data;
	ZERO_INITIALIZE(ClusterData, data);
	clusternumlights_t cluster = _ClusterNumItems[clusterOffset]; 
	int dataOffset = cluster.offset & (~(cluster.offset >> 31));
	int numItems = cluster.numItems & (~(cluster.numItems >> 31));
	data.lightsMin = uint((dataOffset/* >> 16*/)* _ClusterInfo.w);
	data.lightsMax = data.lightsMin + uint(numItems & 255);
	data.decalsMin = data.lightsMin;
	data.decalsMax = data.decalsMin + uint((numItems >> 8) & 255);

	return data;
}

uint ItemGetLightID (uint lightIdx)
{
	return _ItemsIDList[lightIdx] & 4095u;
}


#endif