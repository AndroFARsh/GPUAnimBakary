using System;
using UnityEngine;

namespace AnimBakery.Cook
{
    public class BakeryFactory
    {
        private readonly SkinnedMeshRenderer skinnedMeshRenderer;
        private readonly Animation animation;
        private readonly Animator animator;

        public BakeryFactory(GameObject prototype)
        {
            skinnedMeshRenderer = prototype.GetComponentInChildren<SkinnedMeshRenderer>();
            animation = prototype.GetComponent<Animation>();
            animator = prototype.GetComponent<Animator>();
        }

        public IBakery Create()
        {
            if (skinnedMeshRenderer != null && animation != null)
                return new SingleTextureAnimationBakery(skinnedMeshRenderer, animation);
            
            throw new SystemException("Not valid state");
        }
    }
}