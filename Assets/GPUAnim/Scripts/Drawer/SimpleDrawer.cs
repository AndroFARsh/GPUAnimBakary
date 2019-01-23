#define USE_SAFE_JOBS

using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace AnimBakery.Cook
{
    public class SimpleDrawer : IDrawer
    {
        private readonly List<Data> datas = new List<Data>();
        private readonly GameObject prototype;
        private readonly IConfig config;

        public SimpleDrawer(GameObject prototype, IConfig config)
        {
            this.prototype = prototype;
            this.config = config;
        }

        private void InitBuffers()
        {
            Dispose();
            
            var gridSize = (int) Mathf.Sqrt(config.Count);
            for (var index = 0; index < config.Count; index++)
            {
                var x = (float) index / gridSize + 1 - (float) gridSize / 2;
                var z = (float) index % gridSize + 1 - (float) gridSize / 2;

                datas.Add(Object.Instantiate(prototype, new Vector3(x, 0, z), Quaternion.identity));
            }
        }

        public void Draw(float deltaTime)
        {
            if (config.Count == 0) return;

            if (config.Count != datas.Count) InitBuffers();
            
            Profiler.BeginSample("Prepare data");
            foreach (var data in datas)
            {
                data.RefreshAnimation(config.AnimationId);
                data.UpdateRotation(config.RotationAngle);
                data.UpdateScale(config.Scale);
            }
            Profiler.EndSample();
        }

        public void Dispose()
        {
            foreach (var data in datas)
            {
                data.Dispose();
            }
            
            datas.Clear();
        }

        class Data : IDisposable
        {
            private readonly GameObject gameObject;
            private readonly Transform transform;
            private readonly Animation animation;
            
            private List<AnimationClip> animationClips;
            private List<AnimationClip> AnimationClips => animationClips ?? (animationClips = animation.GetAllAnimationClips()); 
            
            private Data(GameObject go)
            {
                gameObject = go;
                transform = go.transform;
                animation = go.GetComponent<Animation>();
            }

            public void UpdatePosition(Vector3 pos)
            {
                transform.localPosition = pos;
            }

            public void UpdateScale(float scale)
            {
                transform.localScale = new Vector3(scale,scale,scale);
            }
            
            public void UpdateRotation(float angle)
            {
                transform.localRotation = Quaternion.AxisAngle(Vector3.up, angle);
            }

            public void RefreshAnimation(int requestAnimId)
            {
                var animationClips = AnimationClips;
                var activeClip = math.clamp(requestAnimId, 0, animationClips.Count - 1);
                if (!animation[animationClips[activeClip].name].enabled)
                {
                    for (var i = 0; i < animationClips.Count; ++i)
                    {
                        var clip = animationClips[i];
                        animation[clip.name].enabled = i == activeClip;
                        animation[clip.name].weight = i == activeClip ? 1f : 0f;
                        animation[clip.name].wrapMode = WrapMode.Loop;
                    }
                }
            }

            public void Dispose()
            {
                Object.Destroy(gameObject);
            }
            
            public static implicit operator Data(GameObject go) 
            {
                return new Data(go);
            }
        }
    }
}