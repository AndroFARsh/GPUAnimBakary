#ifndef GPU_COMMON_ANIMATION
#define GPU_COMMON_ANIMATION
    
    StructuredBuffer<float4> objectRotationsBuffer;
	StructuredBuffer<float4> objectPositionsBuffer;
			
    inline bool IsEqual(float4x4 m1, float4x4 m2)  
    {
        return m1._m00 == m2._m00 && m1._m01 == m2._m01 && m1._m02 == m2._m02 && m1._m03 == m2._m03 &&
               m1._m10 == m2._m10 && m1._m11 == m2._m11 && m1._m12 == m2._m12 && m1._m13 == m2._m13 &&
               m1._m20 == m2._m20 && m1._m21 == m2._m21 && m1._m22 == m2._m22 && m1._m23 == m2._m23 &&
               m1._m30 == m2._m30 && m1._m31 == m2._m31 && m1._m32 == m2._m32 && m1._m33 == m2._m33;
    }

    inline float4x4 PositionToMatrix(float3 p) 
    {
        return float4x4(1,   0,   0,   p.x,
                        0,   1,   0,   p.y,
                        0,   0,   1,   p.z,
                        0,   0,   0,   1);
    }
    
    inline float4x4 ScaleToMatrix(float s) 
    {
        return float4x4(s,   0,   0,   0,
                        0,   s,   0,   0,
                        0,   0,   s,   0,
                        0,   0,   0,   1);
    }

    inline float4x4 PositionScaleToMatrix(float4 p) 
    {
        return float4x4(p.w, 0,   0,   p.x,
                        0,   p.w, 0,   p.y,
                        0,   0,   p.w, p.z,
                        0,   0,   0,   1);
    }
        
    inline float4x4 QuaternionToMatrix(float4 q) 
    {
        float xx = q.x * q.x;
        float xy = q.x * q.y;
        float xz = q.x * q.z;
        float xw = q.x * q.w;
    
        float yy = q.y * q.y;
        float yz = q.y * q.z;
        float yw = q.y * q.w;
    
        float zz = q.z * q.z;
        float zw = q.z * q.w;
    
        float4x4 m;
        m._m00 = 1 - 2 * ( yy + zz );
        m._m01 =     2 * ( xy - zw );
        m._m02 =     2 * ( xz + yw );
    
        m._m10 =     2 * ( xy + zw );
        m._m11 = 1 - 2 * ( xx + zz );
        m._m12 =     2 * ( yz - xw );
    
        m._m20 =     2 * ( xz - yw );
        m._m21 =     2 * ( yz + xw );
        m._m22 = 1 - 2 * ( xx + yy );
    
        m._m03 = m._m13 = m._m23 = m._m30 = m._m31 = m._m32 = 0;
        m._m33 = 1;
        return m;
    }

    inline float4 QuaternionMul(float4 v, float4 q)
	{
        return float4(v + 2 * cross(q.xyz, cross(q.xyz, v.xyz) + q.w * v), v.w);
    }
	
	inline float4x4 TransformMatrix(uint instanceID) 
	{
	    return mul(PositionToMatrix(objectPositionsBuffer[instanceID].xyz), 
	               mul(QuaternionToMatrix(objectRotationsBuffer[instanceID]), 
	                   ScaleToMatrix(objectPositionsBuffer[instanceID].w)));
	} 
	
#endif // GPU_COMMON_ANIMATION