#define USE_SAFE_JOBS

using System;
using System.Collections.Generic;
using System.Linq;
using AnimBakery.Cook;
using AnimBakery.Cook.Model;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace AnimBakery.Draw
{
    public class GPUAnimDrawer : IDrawer, IDisposable
    {
        private static readonly int TextureCoordinatesBufferProperty = Shader.PropertyToID("textureCoordinatesBuffer");
        private static readonly int ObjectPositionsBufferProperty = Shader.PropertyToID("objectPositionsBuffer");
        private static readonly int ObjectRotationsBufferProperty = Shader.PropertyToID("objectRotationsBuffer");

        private static readonly int AnimationTextureSizeProperty = Shader.PropertyToID("animationTextureSize");
        private static readonly int AnimationTextureProperty = Shader.PropertyToID("animationTexture");

        private readonly uint[] indirectArgs = {0, 0, 0, 0, 0};
        private readonly IConfig config;
        private readonly BakedData data;

        private ComputeBuffer argsBuffer;
        private ComputeBuffer textureCoordinatesBuffer;
        private ComputeBuffer objectRotationsBuffer;
        private ComputeBuffer objectPositionsBuffer;
        private List<float> times;
        
        private int count = -1;

        public GPUAnimDrawer(IBakery bakery, IConfig config)
        {
            this.data = bakery.BakeClips(config.FrameRate);
            this.config = config;
        }

        private void InitBuffers()
        {
            Dispose();

            count = config.Count;

            argsBuffer = new ComputeBuffer(1, indirectArgs.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

            objectRotationsBuffer = new ComputeBuffer(count, sizeof(float) * 4);
            objectPositionsBuffer = new ComputeBuffer(count, sizeof(float) * 4);
            textureCoordinatesBuffer = new ComputeBuffer(count, sizeof(float));
            
            times = Enumerable.Repeat(0.0f, count).ToList();
        }

        public void Draw(float deltaTime)
        {
            if (config.Count == 0) return;

            if (config.Count != count) InitBuffers();
            
            Profiler.BeginSample("Prepare shader data");
            var gridSize = (int) Math.Sqrt(count);
            var textureCoordinates = new NativeList<float>(count, Allocator.Temp);
            var objectPositions = new NativeList<float4>(count, Allocator.Temp);
            var objectRotations = new NativeList<quaternion>(count, Allocator.Temp);
            for (var index = 0; index < count; index++)
            {
                var x = (float) index / gridSize + 1 - (float) gridSize / 2;
                var z = (float) index % gridSize + 1 - (float) gridSize / 2;
                
                var clip = data[math.clamp(config.AnimationId, 0, data.Count - 1)];
                var dt = deltaTime + deltaTime * (config.HasAnimationDiff ? Random.Range(-0.5f, 0.5f) : 0);
                
                times[index] += dt * config.TimeMultiplier;
                if (times[index] > clip.ClipLength) times[index] %= clip.ClipLength;

                var normalizedTime = config.IsAnimated ? times[index] / clip.ClipLength : config.NormalizedTime;
                var frameIndex = (int) ((clip.FramesCount - 1) * normalizedTime);
                
                textureCoordinates.Add(clip.Start + frameIndex * data.BonesCount * 3.0f);
                objectPositions.Add(new float4(x, 0, z, config.Scale));
                objectRotations.Add(quaternion.RotateY(config.RotationAngle));
            }
            Profiler.EndSample();
                
            Profiler.BeginSample("Shader set data");
            
            objectRotationsBuffer.SetData((NativeArray<quaternion>)objectRotations, 0, 0, count);
            objectPositionsBuffer.SetData((NativeArray<float4>)objectPositions, 0, 0, count);
            textureCoordinatesBuffer.SetData((NativeArray<float>)textureCoordinates, 0, 0, count);

            data.Material.SetBuffer(TextureCoordinatesBufferProperty, textureCoordinatesBuffer);
            data.Material.SetBuffer(ObjectPositionsBufferProperty, objectPositionsBuffer);
            data.Material.SetBuffer(ObjectRotationsBufferProperty, objectRotationsBuffer);
            
            data.Material.SetVector(AnimationTextureSizeProperty, new Vector2(data.Texture.width,data.Texture.height));
            data.Material.SetTexture(AnimationTextureProperty, data.Texture);
            
            Profiler.EndSample();
 
            indirectArgs[0] = data.Mesh.GetIndexCount(0);
            indirectArgs[1] = (uint)count;
            argsBuffer.SetData(indirectArgs);

            Graphics.DrawMeshInstancedIndirect(data.Mesh,
                0,
                data.Material,
                new Bounds(Vector3.zero, 1000 * Vector3.one),
                argsBuffer,
                0,
                new MaterialPropertyBlock());

            textureCoordinates.Dispose();
            objectPositions.Dispose();
            objectRotations.Dispose();
        }

        public void Dispose()
        {
            argsBuffer?.Dispose();
            objectPositionsBuffer?.Dispose();
            objectRotationsBuffer?.Dispose();
            textureCoordinatesBuffer?.Dispose();
            
            count = -1;
        }
    }
}