
using System;

namespace AnimBakery.Draw
{
    public interface IDrawer : IDisposable
    {
        void Draw(float dt);
    }
}