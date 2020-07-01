using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DoomSRP.FG
{
    public partial class Realize
    {
        static Realize ins = new Realize();
        public static Realize Instance { get { return ins; } }

        public ActualType realize<DescriptionType, ActualType>(DescriptionType description) where ActualType : class
        {
            Debug.AssertFormat(false, "Missing realize implementation for description-type pair");
            return default;
        }
    }
}
