
using System;

namespace AnimBakery.Cook
{
    public interface IDrawer : IDisposable
    {
        void Draw(float dt);
    }
}