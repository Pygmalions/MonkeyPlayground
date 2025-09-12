using MonkeyPlayground.Data;
using UnityEngine;

namespace MonkeyPlayground.Objects;

[DisallowMultipleComponent]
public class Floor : PerceptibleObject<FloorData>
{
    protected override FloorData OnGenerateData()
    {
        return new FloorData(transform);
    }
}