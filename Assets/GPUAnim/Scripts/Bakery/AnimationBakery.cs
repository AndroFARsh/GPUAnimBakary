using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnimBakery.Cook
{
    public class AnimationBakery : BaseBakery
    {
        private readonly Animation animationComponent;
        private readonly SkinnedMeshRenderer originalRenderer;

        public AnimationBakery(SkinnedMeshRenderer originalRenderer, Animation animationComponent)
        {
            if (animationComponent == null)
            {    
                throw new ArgumentException("Animation couldn't be null ");
            } 
            
            if (originalRenderer == null)
            {    
                throw new ArgumentException("SkinnedMeshRenderer couldn't be null ");
            }
            if (originalRenderer.sharedMesh == null)
            {    
                throw new ArgumentException("SkinnedMeshRenderer.Mesh couldn't be null ");
            } 
            if (originalRenderer.bones == null)
            {    
                throw new ArgumentException("SkinnedMeshRenderer.Bones couldn't be null ");
            } 
            
            this.animationComponent = animationComponent;
            this.originalRenderer = originalRenderer;
        }

        protected override List<Matrix4x4[,]> SampleAnimationClips(
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
                    originalRenderer,
                    animationComponent,
                    frameRate);
                sampledBoneMatrices.Add(sampledMatrix);

                numberOfKeyFrames += sampledMatrix.GetLength(0);
            }

            numberOfBones = sampledBoneMatrices[0].GetLength(1);

            return sampledBoneMatrices;
        }

        protected override Mesh CreateMesh()
        {
            return CreateMesh(originalRenderer.sharedMesh);
        }

        protected override Material CreateMaterial()
        {
            return new Material(originalRenderer.sharedMaterial);
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