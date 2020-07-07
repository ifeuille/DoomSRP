using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DoomSRP.FG.DES
{
    public struct ComputeBufferDescriptor
    {
        public int count;
        public int stride;
        public ComputeBufferType computeBufferType;
        public ComputeBufferMode computeBufferMode;
        public string name;

        public ComputeBufferDescriptor(int count, int stride, ComputeBufferType computeBufferType, ComputeBufferMode computeBufferMode, string name)
        {
            this.count = count;
            this.stride = stride;
            this.computeBufferType = computeBufferType;
            this.computeBufferMode = computeBufferMode;
            this.name = name;
        }
    }
}
