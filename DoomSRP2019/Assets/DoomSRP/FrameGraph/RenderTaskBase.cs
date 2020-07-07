﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomSRP.FG
{
    public class RenderTaskBase
    {
        protected string name = "";
        protected bool cullImmune = false;
        protected List<ResourceBase> creates = new List<ResourceBase>();
        protected List<ResourceBase> reads = new List<ResourceBase>();
        protected List<ResourceBase> writes = new List<ResourceBase>();
        protected uint refCount = 0;

        public List<ResourceBase> Creates { get { return creates; } }
        public List<ResourceBase> Reads { get { return reads; } }
        public List<ResourceBase> Writes { get { return writes; } }
        public uint ReferenceCount { get { return refCount; } set { refCount = value; } }
        public string Name { get { return this.name; } set { this.name = value; } }
        public bool CullImmune { get { return this.cullImmune; } set { this.cullImmune = value; } }


        public RenderTaskBase(string name)
        {
            this.name = name;
        }
        public virtual void Setup(RenderTaskBuilder builder) { }
        public virtual void Execute() { }
        public void UpdateReferenceCount()
        {
            refCount = (uint)(creates.Count + writes.Count);
        }
        
    }
}