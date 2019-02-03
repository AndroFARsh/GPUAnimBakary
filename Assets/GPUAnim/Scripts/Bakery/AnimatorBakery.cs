using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AnimBakery.Cook
{
    public class AnimatorBakery: BaseBakery
    {
        private readonly GameObject prototype;
        
        private GameObject go;
        private SkinnedMeshRenderer skinnedMeshRenderer;
        private Animator animator;

        public AnimatorBakery(GameObject prototype)
        {
            if (prototype == null)
            {    
                throw new ArgumentException("GameObject couldn't be null ");
            } 
           
            this.prototype = prototype;
        }

        protected override void OnBeginBakeClips()
        {   
            go = Object.Instantiate(prototype);
            go.SetActive(true);
   
            skinnedMeshRenderer = go.GetComponentInChildren<SkinnedMeshRenderer>();
            animator = go.GetComponent<Animator>();
        }

        protected override void OnEndBakeClips()
        {   
            go.SetActive(false);
            Object.Destroy(go);
        }
        
        protected override Mesh CreateMesh()
        {
            return CreateMesh(skinnedMeshRenderer.sharedMesh);
        }

        protected override Material CreateMaterial()
        {
            return new Material(skinnedMeshRenderer.sharedMaterial);
        }

        protected override List<Matrix4x4[,]> SampleAnimationClips( 
                                                              float frameRate, 
                                                              out List<AnimationClip> animationClips,
                                                              out int numberOfKeyFrames,
                                                              out int numberOfBones)
        {
            animationClips = animator.GetAllAnimationClips();
            
            numberOfKeyFrames = 0;
            var sampledBoneMatrices = new List<Matrix4x4[,]>();
            foreach (var animationClip in animationClips)
            {
                var sampledMatrix = SampleAnimationClip(go, 
                                                        animationClip,
                                                        skinnedMeshRenderer,
                                                        frameRate);
                sampledBoneMatrices.Add(sampledMatrix);

                numberOfKeyFrames += sampledMatrix.GetLength(0);
            }

            numberOfBones = sampledBoneMatrices[0].GetLength(1);
            return sampledBoneMatrices;
        }

        private static Matrix4x4[,] SampleAnimationClip(
            GameObject go,
            AnimationClip clip,
            SkinnedMeshRenderer renderer,
            float frameRate)
        {
            var numFrames = Mathf.CeilToInt(frameRate * clip.length);
            var boneMatrices = new Matrix4x4[numFrames, renderer.bones.Length];
            
            for (var frameIndex = 0; frameIndex < boneMatrices.GetLength(0); frameIndex++)
            {
                var t = clip.length * ((float)frameIndex / numFrames);
                clip.SampleAnimation(go, t);               

                for (var boneIndex = 0; boneIndex < renderer.bones.Length; boneIndex++)
                {
                    // Put it into model space for better compression.
                    boneMatrices[frameIndex, boneIndex] = renderer.bones[boneIndex].localToWorldMatrix *
                                                          renderer.sharedMesh.bindposes[boneIndex];
                }
            }
            
            return boneMatrices;
        }
    }
}