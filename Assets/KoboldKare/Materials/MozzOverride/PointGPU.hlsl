struct Point {
    float3 position;
    float3 prevPosition;
    float3 savedPosition;
    float volume;
};
float _PointScale;

#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	StructuredBuffer<Point> _Points;
#endif

void ConfigureProcedural () {
	#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
		Point p = _Points[unity_InstanceID];

		unity_ObjectToWorld = 0.0;
		// Set the XYZ translation on the matrix
		unity_ObjectToWorld._m03_m13_m23_m33 = float4(p.position.xyz, 1.0);
		// Set the XYZ scale on the matrix
		unity_ObjectToWorld._m00_m11_m22 = p.volume*_PointScale;
	#endif
}

void ShaderGraphFunction_half (half3 In, out half3 Out) {
	Out = In;
}
void ShaderGraphFunction_float (float3 In, out float3 Out) {
	Out = In;
}