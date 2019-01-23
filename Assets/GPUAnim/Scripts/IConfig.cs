using UnityEngine;

namespace AnimBakery.Cook
{
    public interface IConfig
    {
        GameObject Prototype { get; }

        int Count { get; }
        
        int AnimationId { get; }
        
        float FrameRate { get; }

        float TimeMultiplier { get; }

        float NormalizedTime { get;  }

        bool HasAnimationDiff { get; }

        bool IsAnimated  { get; }
        
        float RotationAngle  { get; }
        
        float Scale { get; }
    }
}