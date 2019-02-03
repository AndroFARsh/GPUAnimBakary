using AnimBakery.Draw;
using UnityEngine;

namespace AnimBakery
{
    public class CPUAnimDrawerMono : BaseDrawerMono
    {
        protected override IDrawer Create(GameObject prototype, IConfig config)
        {
            return new CPUAnimDrawer(prototype, config);
        }
    }
}