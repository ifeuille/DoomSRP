#ifndef UNITY_PACKING_INCLUDED1
#define UNITY_PACKING_INCLUDED1

// Unpack from normal map
half3 UnpackNormalRGB(half4 packedNormal, half scale = 1.0)
{
	half3 normal;
	normal.xyz = packedNormal.rgb * 2.0 - 1.0;
	normal.xy *= scale;
	return normalize(normal);
}

half3 UnpackNormalRGBNoScale(half3 packedNormal)
{
	return packedNormal.rgb * 2.0 - 1.0;
}

half3 UnpackNormalAG(half4 packedNormal, half scale = 1.0)
{
	half3 normal;
	normal.xy = packedNormal.wy * 2.0 - 1.0;
	normal.xy *= scale;
	normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
	return normal;
}
// Unpack normal as DXT5nm (1, y, 0, x) or BC5 (x, y, 0, 1)
half3 UnpackNormalmapRGorAG(half4 packedNormal, half scale = 1.0)
{
	// This do the trick
	packedNormal.w *= packedNormal.x;
	return UnpackNormalAG(packedNormal, scale);
}

#endif