using UnityEngine;

namespace AnimBakery.Cook
{
    public class SimpleDrawerMono : BaseDrawerMono
    {
        protected override IDrawer Create(GameObject prototype, IConfig config)
        {
            return new SimpleDrawer(prototype, config);
        }
    }
}