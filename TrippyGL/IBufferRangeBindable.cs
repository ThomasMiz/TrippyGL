﻿namespace TrippyGL
{
    /// <summary>
    /// As of right now the only use of this interface is for ShaderBlockUniform to be able to hold a UniformBufferSubset
    /// without having to use a type param and also for being able to bind it to the proper uniform block buffer binding index
    /// </summary>
    internal interface IBufferRangeBindable
    {
        void BindBufferRange(int bindingIndex, int storageOffsetBytes, int storageLengthBytes);
    }
}
