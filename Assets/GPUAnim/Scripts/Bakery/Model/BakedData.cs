using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace AnimBakery.Cook.Model
{
    public struct BakedData
    {
        private readonly Texture2D[] textures;
        private readonly Mesh mesh;
        private readonly Material material;
        private readonly float frameRate;
        private readonly int bonesCount;
        private readonly List<AnimationClipData> animations;
        private readonly Dictionary<string, AnimationClipData> animationsDictionary;

        public Material Material => material;
        public Texture2D Texture => textures[0];
        public Mesh Mesh => mesh;

        public float FrameRate => frameRate;
        public int BonesCount => bonesCount;
        public int Count => animations.Count;
        public AnimationClipData this[int index] => animations[index];
        public AnimationClipData this[string index] => animationsDictionary[index];

        public Texture2D GetTexture(int index) => index < textures.Length ? textures[index] : null;

        private BakedData(Texture2D[] textures,
                          Mesh mesh,
                          Material material,
                          float frameRate,
                          int bonesCount,
                          List<AnimationClipData> animations)
        {
            this.textures = textures;
            this.mesh = mesh;
            this.material = material;
            this.frameRate = frameRate;
            this.bonesCount = bonesCount;
            this.animations = animations;
            
            this.animationsDictionary = new Dictionary<string, AnimationClipData>();
            foreach (var clipData in animations)
                animationsDictionary[clipData.Name] = clipData;
        }

        public static BakedDataBuilder Bulder(uint textCapasity = 1)
        {
            return new BakedDataBuilder(textCapasity);
        }

        public class BakedDataBuilder
        {
            private Texture2D[] textures;
            private Mesh mesh;
            private Material material;
            private float frameRate = 30;
            private int bonesCount = -1;
            private List<AnimationClipData> animations = new List<AnimationClipData>();

            internal BakedDataBuilder(uint textCapasity)
            {
                textures = new Texture2D[textCapasity];
            }

            public BakedDataBuilder SetTexture(uint id, Texture2D texture)
            {
                textures[id] = texture;
                return this;
            }

            public BakedDataBuilder SetMesh(Mesh m)
            {
                mesh = m;
                return this;
            }

            public BakedDataBuilder SetMaterial(Material m)
            {
                material = m;
                return this;
            }

            public BakedDataBuilder SetFrameRate(float fr)
            {
                frameRate = fr;
                return this;
            }

            public BakedDataBuilder SetBonesCount(int bc)
            {
                bonesCount = bc;
                return this;
            }

            public BakedDataBuilder AddClip(AnimationClipData clipData)
            {
                animations.Add(clipData);
                return this;
            }

            public BakedData Build()
            {
                if (bonesCount == -1) throw new System.NullReferenceException("Bones count shouldn't be -1");
                if (mesh == null) throw new System.NullReferenceException("Mesh shouldn't be null");
                if (material == null) throw new System.NullReferenceException("Material shouldn't be null");
                for (var index = 0; index < textures.Length; ++index)
                    if (textures[index] == null)
                        throw new System.NullReferenceException($"Texture {index} shouldn't be null");


                return new BakedData(textures, mesh, material, frameRate, bonesCount, animations);
            }
        }
    }
}