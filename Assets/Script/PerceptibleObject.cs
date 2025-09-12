using System;
using UnityEngine;

namespace MonkeyPlayground;

public abstract class PerceptibleObject<TData> : MonoBehaviour where TData : struct
{
    /// <summary>
    /// Event triggered when generating the data of this object.
    /// </summary>
    public event Func<TData, TData> OnGeneratingData;

    /// <summary>
    /// Override this method to generate the initial data of this object.
    /// </summary>
    /// <returns>Initial data of this object.</returns>
    protected abstract TData OnGenerateData();

    /// <summary>
    /// Generate the data of this object.
    /// </summary>
    /// <returns>Data of this object.</returns>
    public TData GenerateData()
    {
        var data = OnGenerateData();
        data = OnGeneratingData?.Invoke(data) ?? data;
        return data;
    }
}