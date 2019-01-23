using AnimBakery.Cook.Model;
using UnityEngine;

namespace AnimBakery.Cook
{
    public interface IBakery
    {
        BakedData BakeClips(float frameRate = 30f);
    }
}
