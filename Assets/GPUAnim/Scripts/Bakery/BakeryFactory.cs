using System;
using UnityEngine;

namespace AnimBakery.Cook
{
    public class BakeryFactory
    {
        private readonly SkinnedMeshRenderer skinnedMeshRenderer;
        private readonly Animation animation;
        private readonly Animator animator;
        private readonly GameObject prototype;

        public BakeryFactory(GameObject p)
        {
            prototype = p;
            skinnedMeshRenderer = p.GetComponentInChildren<SkinnedMeshRenderer>();
            animation = p.GetComponent<Animation>();
            animator = p.GetComponent<Animator>();

            if (animator == null && animation == null)
            {
                throw new ArgumentException("Animation and Animator couldn't be null at same time");
            }
            if (animator != null && animation != null)
            {
                throw new ArgumentException("Animation and Animator couldn't be not null at same time");
            }
        }

        public IBakery Create()
        {
            if (skinnedMeshRenderer != null && animation != null)
            {
                return new AnimationBakery(skinnedMeshRenderer, animation);
            }

            if (skinnedMeshRenderer != null && animator != null)
            {
                return new AnimatorBakery(prototype);
            }

            throw new SystemException("Not valid state");
        }
    }
}