using System.Collections.Generic;
using UnityEngine;

namespace AnimBakery.Cook
{
    public static class  BakeryUtils
    {
        public static int NextPowerOfTwo(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        public static Mesh CopyMesh(this Mesh originalMesh)
        {
            var newMesh = new Mesh();
            var vertices = originalMesh.vertices;

            newMesh.vertices = vertices;
            newMesh.triangles = originalMesh.triangles;
            newMesh.normals = originalMesh.normals;
            newMesh.uv = originalMesh.uv;
            newMesh.tangents = originalMesh.tangents;
            newMesh.name = originalMesh.name;
            newMesh.bindposes = originalMesh.bindposes;
            newMesh.bounds = originalMesh.bounds;
            return newMesh;
        }

        public static List<AnimationClip> GetAllAnimationClips(this Animation animation)
        {
            var animationClips = new List<AnimationClip>();
            foreach (AnimationState state in animation)
            {
                animationClips.Add(state.clip);
            }
            return animationClips;
        }
        
        public static string Format(Vector2Int v)
        {
            return "(" + v.x + ", " + v.y + ")";
        }

        public static string Format(Vector4 v)
        {
            return "(" + v.x + ", " + v.y + ", " + v.z + ", " + v.w + ")";
        }

        public static string Format(Color v)
        {
            return "(" + v.r + ", " + v.g + ", " + v.b + ", " + v.a + ")";
        }

        public static Vector2Int To2D(int index, int width)
        {
            var y = index / width;
            var x = index % width;
            return new Vector2Int(x, y);
        }
    }
}