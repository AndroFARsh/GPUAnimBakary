using AnimBakery.Cook;
using AnimBakery.Draw;
using UnityEngine;

namespace AnimBakery
{
    public class GPUAnimDrawerMono : BaseDrawerMono
    {
        protected override IDrawer Create(GameObject prototype, IConfig config)
        {
            var factory = new BakeryFactory(prototype);
            return new GPUAnimDrawer(factory.Create(), config);
        }
    }
}