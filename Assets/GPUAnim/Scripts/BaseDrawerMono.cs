using UnityEngine;

namespace AnimBakery.Cook
{
    [RequireComponent(typeof(ConfigMono))]
    public abstract class BaseDrawerMono : MonoBehaviour
    {
        private IDrawer drawer;

        private void OnEnable()
        {
            var config = GetComponent<ConfigMono>();
            if (config != null)
                drawer = Create(config.Prototype, config);
        }

        protected abstract IDrawer Create(GameObject prototype, IConfig config);
        
        private void Update()
        {
            drawer?.Draw(Time.deltaTime);
        }

        private void OnDisable()
        {
            drawer?.Dispose();
        }
    }
}