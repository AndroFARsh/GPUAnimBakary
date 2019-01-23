#ifndef GPU_ANIMATION
#define GPU_ANIMATION
    
    #include "CommonCore.cginc" 		
    
    Buffer<float> textureCoordinatesBuffer;
    
    sampler2D animationTexture;
    float2    animationTextureSize;

    inline float4 IndexToUV(int index, float2 size) 
	{
	    return float4(((float)((int)(index % size.x)) + 0.5) / size.x, 
		              ((float)((int)(index / size.x)) + 0.5) / size.y, 
		              0, 
		              0);
    }
	
	inline float4x4 CreateMatrix(float frameOffset, float boneIndex) 
	{
	    int index = frameOffset + boneIndex * 3;
	    
	    float4 row0 = tex2Dlod(animationTexture, IndexToUV(index + 0, animationTextureSize));
		float4 row1 = tex2Dlod(animationTexture, IndexToUV(index + 1, animationTextureSize));
		float4 row2 = tex2Dlod(animationTexture, IndexToUV(index + 2, animationTextureSize));
		float4 row3 = float4(0, 0, 0, 1);

        float4x4 m = float4x4(row0, row1, row2, row3);
		return m;
	}
	
	inline float4x4 AnimationMatrix(in float4 bones, in float4 boneInfluences, in uint instanceID) 
	{
	    float3 frameOffset = textureCoordinatesBuffer[instanceID];
        
        float4x4 m0 = CreateMatrix(frameOffset, bones.x) * boneInfluences.x;
        float4x4 m1 = CreateMatrix(frameOffset, bones.y) * boneInfluences.y;
        float4x4 m2 = CreateMatrix(frameOffset, bones.z) * boneInfluences.z;
        float4x4 m3 = CreateMatrix(frameOffset, bones.w) * boneInfluences.w;
				      
        float4x4 m = m0 + m1 + m2 + m3;
        return m;
	}
	
#endif // GPU_ANIMATION