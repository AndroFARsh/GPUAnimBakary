using UnityEngine;

namespace AnimBakery.Cook.Model
{
    public struct AnimationClipData
    {
        private AnimationClip clip;
        private int framesCount;
        private int start;
        private int end;

        public static AnimationClipData Create(
            AnimationClip clip,
            int start,
            int end,
            int frameCount)
        {
            return new AnimationClipData
            {
                clip            = clip,
                start           = start,
                end             = end,
                framesCount     = frameCount
            };
        }
            
        public string Name => clip.name;

        public float ClipLength => clip.length;
        public AnimationClip Clip => clip;
        public int Start => start;
        public int End => end;
        public int FramesCount => framesCount;
    }
}