using System;
using System.Collections.Generic;
using AnimBakery.Cook.Model;
using UnityEngine;

namespace AnimBakery.Cook
{
    public class SingleTextureAnimationBakery: IBakery
    {
        private const int MATRIX_ROWS_COUNT = 3;

        private readonly SkinnedMeshRenderer originalRenderer;
        private readonly Animation animationComponent;
        
        public SingleTextureAnimationBakery(SkinnedMeshRenderer originalRenderer, Animation animationComponent)
        {
            this.originalRenderer = originalRenderer;
            this.animationComponent = animationComponent;
        }
        
        /// <summary>
        /// Bake all animation clips to texture in format:
        /// [clip0[frame0[bone0[row0, row1, row0]..boneN[row0, row1, row0]]..frameM[bone0[row0, row1, row0]..boneN[row0, row1, row0]]]..clipK[..
        /// </summary>
        /// <returns>BakedData - baked animation matrix to texture</returns>
        public BakedData BakeClips(float frameRate = 30f)
        {
            var bakedDataBuilder = BakedData.Bulder(1)
                .SetMaterial(new Material(originalRenderer.sharedMaterial))
                .SetMesh(CreateMesh(originalRenderer))
                .SetFrameRate(frameRate);

            var sampledBoneMatrices = SampleAnimationClips(animationComponent, 
                                                           originalRenderer, 
                                                           frameRate, 
                                                           out var animationClips,
                                                           out var numberOfKeyFrames, 
                                                           out var numberOfBones);
            
            // find minimum square texture size, size should be power of 2
            var size = BakeryUtils.NextPowerOfTwo(
                (int) Math.Sqrt(numberOfBones * numberOfKeyFrames * MATRIX_ROWS_COUNT));
            var texture = new Texture2D(size, size, TextureFormat.RGBAFloat, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point,
                anisoLevel = 0
            };
            var textureColor = new Color[texture.width * texture.height];
            bakedDataBuilder.SetTexture(0, texture);
            bakedDataBuilder.SetBonesCount(numberOfBones);
            
            var clipOffset = 0;
            for (var clipIndex = 0; clipIndex < sampledBoneMatrices.Count; clipIndex++)
            {
                var framesCount = sampledBoneMatrices[clipIndex].GetLength(0);
                for (var keyframeIndex = 0; keyframeIndex < framesCount; keyframeIndex++)
                {
                    var frameOffset = keyframeIndex * numberOfBones * MATRIX_ROWS_COUNT;
                    for (var boneIndex = 0; boneIndex < numberOfBones; boneIndex++)    
                    {
                        var index = clipOffset + frameOffset + boneIndex * MATRIX_ROWS_COUNT;
                        var matrix = sampledBoneMatrices[clipIndex][keyframeIndex, boneIndex];

                        if ((Vector4)textureColor[index + 0] != Vector4.zero) Debug.LogError($"Index {index + 0} not empty");
                        if ((Vector4)textureColor[index + 1] != Vector4.zero) Debug.LogError($"Index {index + 1} not empty");
                        if ((Vector4)textureColor[index + 2] != Vector4.zero) Debug.LogError($"Index {index + 2} not empty");
                        
                        textureColor[index + 0] = matrix.GetRow(0);
                        textureColor[index + 1] = matrix.GetRow(1);
                        textureColor[index + 2] = matrix.GetRow(2);
                    }
                }

                var clip = animationClips[clipIndex];
                var start = clipOffset;
                var end = clipOffset + (framesCount - 1) * MATRIX_ROWS_COUNT;

                var clipData = AnimationClipData.Create(clip,
                                                        start,
                                                        end,
                                                        framesCount);
                
                bakedDataBuilder.AddClip(clipData);

                clipOffset += framesCount * numberOfBones * MATRIX_ROWS_COUNT;
            }
            
            texture.SetPixels(textureColor);
            texture.Apply(false, false);
            
            clipOffset = 0;
            for (var clipIndex = 0; clipIndex < sampledBoneMatrices.Count; clipIndex++)
            {
                var framesCount = sampledBoneMatrices[clipIndex].GetLength(0);
                for (var keyframeIndex = 0; keyframeIndex < framesCount; keyframeIndex++)
                {
                    var frameOffset = keyframeIndex * numberOfBones * MATRIX_ROWS_COUNT;
                    for (var boneIndex = 0; boneIndex < numberOfBones; boneIndex++)    
                    {   
                        var index = clipOffset + frameOffset + boneIndex * MATRIX_ROWS_COUNT;
                        var matrix = sampledBoneMatrices[clipIndex][keyframeIndex, boneIndex];
                        
                        var color0 = textureColor[index];
                        var index2D0 = BakeryUtils.To2D(index, texture.width);
                        var pixel0 = texture.GetPixel(index2D0.x, index2D0.y);
                        var row0 = (Color)matrix.GetRow(0);
                        index++;

                        var color1 = textureColor[index];
                        var index2D1 = BakeryUtils.To2D(index, texture.width);
                        var pixel1 = texture.GetPixel(index2D1.x, index2D1.y);
                        var row1 = (Color)matrix.GetRow(1);
                        index++;

                        var color2 = textureColor[index];
                        var index2D2 = BakeryUtils.To2D(index, texture.width);
                        var pixel2 = texture.GetPixel(index2D2.x, index2D2.y);
                        var row2 = (Color)matrix.GetRow(2);

                        if (!Verify(pixel0, row0, color0, index2D0, clipIndex, keyframeIndex, boneIndex)) break;
                        if (!Verify(pixel1, row1, color1, index2D1, clipIndex, keyframeIndex, boneIndex)) break;
                        if (!Verify(pixel2, row2, color2, index2D2, clipIndex, keyframeIndex, boneIndex)) break;
                    }
                }

                clipOffset += numberOfBones * framesCount * MATRIX_ROWS_COUNT;
            }
            
            return bakedDataBuilder.Build();
        }
        
        private static Mesh CreateMesh(SkinnedMeshRenderer originalRenderer)
        {
            var originalMesh = originalRenderer.sharedMesh;
            var newMesh = originalMesh.CopyMesh();

            var boneWeights = originalMesh.boneWeights;

            var boneIds = new List<Vector4>(originalMesh.vertexCount);
            var boneInfluences = new List<Vector4>(originalMesh.vertexCount);

            for (var i = 0; i < originalMesh.vertexCount; i++)
            {
                var bw = boneWeights[i];
                var bonesWeightsSorted = new List<Tuple<int, float>>(4)
                {
                    Tuple.Create(bw.boneIndex0, bw.weight0),
                    Tuple.Create(bw.boneIndex1, bw.weight1),
                    Tuple.Create(bw.boneIndex2, bw.weight2),
                    Tuple.Create(bw.boneIndex3, bw.weight3)
                };
                bonesWeightsSorted.Sort((b1, b2) => b1.Item2 < b2.Item2 ? 1 : -1);

                boneIds.Add(new Vector4
                {
                    x = bonesWeightsSorted[0].Item1,
                    y = bonesWeightsSorted[1].Item1,
                    z = bonesWeightsSorted[2].Item1,
                    w = bonesWeightsSorted[3].Item1
                });

                boneInfluences.Add(new Vector4
                {
                    x = bonesWeightsSorted[0].Item2,
                    y = bonesWeightsSorted[1].Item2,
                    z = bonesWeightsSorted[2].Item2,
                    w = bonesWeightsSorted[3].Item2
                });
            }

            newMesh.SetUVs(1, boneIds);
            newMesh.SetUVs(2, boneInfluences);
            
            return newMesh;
        }
        
        private static bool Verify(Color pixel, Color row, Color color, Vector2Int index2D,  
            int clipIndex, int keyframeIndex, int boneIndex)
        {
            if (pixel != row && row != color)
            {
                Debug.LogError("Error at (" + clipIndex + ", " + keyframeIndex + ", " + boneIndex + ")" +
                               " expected " + BakeryUtils.Format(row) +
                               " Texture(" + BakeryUtils.Format(index2D) +
                               " but got " + BakeryUtils.Format(pixel) +
                               " in color array " + BakeryUtils.Format(color));
                return false;
            }
            return true;
        }
        
        private static List<Matrix4x4[,]> SampleAnimationClips(Animation animationComponent,     
                                                              SkinnedMeshRenderer renderer, 
                                                              float frameRate, 
                                                              out List<AnimationClip> animationClips,
                                                              out int numberOfKeyFrames,
                                                              out int numberOfBones)
        {
            animationClips = animationComponent.GetAllAnimationClips();
            foreach (var clip in animationClips)
            {
                animationComponent[clip.name].enabled = false;
                animationComponent[clip.name].weight = 0f;
            }

            numberOfKeyFrames = 0;
            var sampledBoneMatrices = new List<Matrix4x4[,]>();
            foreach (var animationClip in animationClips)
            {
                var sampledMatrix = SampleAnimationClip(animationClip, 
                                                        renderer, 
                                                        animationComponent, 
                                                        frameRate);
                sampledBoneMatrices.Add(sampledMatrix);

                numberOfKeyFrames += sampledMatrix.GetLength(0);
            }

            numberOfBones = sampledBoneMatrices[0].GetLength(1);

            return sampledBoneMatrices;
        }

        private static Matrix4x4[,] SampleAnimationClip(
            AnimationClip clip,
            SkinnedMeshRenderer renderer,
            Animation animation,
            float frameRate)
        {
            var boneMatrices = new Matrix4x4[Mathf.CeilToInt(frameRate * clip.length), renderer.bones.Length];
            var bakingState = animation[clip.name];
            
            // enable clip
            bakingState.enabled = true;
            bakingState.weight = 1f;
            for (var frameIndex = 0; frameIndex < boneMatrices.GetLength(0); frameIndex++)
            {
                var t = (float) frameIndex / (boneMatrices.GetLength(0));

                bakingState.normalizedTime = t;
                animation.Sample();

                for (var boneIndex = 0; boneIndex < renderer.bones.Length; boneIndex++)
                {
                    // Put it into model space for better compression.
                    boneMatrices[frameIndex, boneIndex] = renderer.bones[boneIndex].localToWorldMatrix *
                                                          renderer.sharedMesh.bindposes[boneIndex];
                }
            }

            // disable clip
            bakingState.enabled = false;
            bakingState.weight = 0f;

            return boneMatrices;
        }
    }
}