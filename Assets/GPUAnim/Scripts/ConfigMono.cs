using UnityEngine;

namespace AnimBakery.Cook
{
    public class ConfigMono : MonoBehaviour, IConfig
    {
        [SerializeField] private GameObject prototype;
        [SerializeField] private int count = 1000;
        [SerializeField] private float frameRate = 30;

        [Space]
        [Range(0.0f, 360.0f)] [SerializeField] private float rotationAngle = 0f;
        [Range(0.5f, 2.0f)] [SerializeField] private float scale = 1f;

        [Space]
        [SerializeField] private int animationId;

        [Space]
        [Range(0.0f, 2.0f)] 
        [SerializeField] private float timeMultiplier = 1.0f;
        [SerializeField] private bool addAnimationDifference;
        
        [SerializeField] private bool animated = true;
        [Range(0.0f, 1.0f)] 
        [SerializeField] private float normalizedTime = 1.0f;
        
        public GameObject Prototype => prototype;
        
        public int Count => count;
        public int AnimationId => animationId;

        public float FrameRate => frameRate;
        public float NormalizedTime => normalizedTime;
        
        public float TimeMultiplier => timeMultiplier;

        public bool HasAnimationDiff => addAnimationDifference;
        public bool IsAnimated => animated;

        public float RotationAngle => rotationAngle * Mathf.PI / 180;
        public float Scale => scale;
    }
}