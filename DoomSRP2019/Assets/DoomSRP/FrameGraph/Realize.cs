using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DoomSRP.FG
{
    public interface IRealize<DescriptionType, ActualType>
    {
        ActualType RealizeDes(DescriptionType description);
    }
    public class Realize<DescriptionType, ActualType> : IRealize<DescriptionType, ActualType>
    {
        public static readonly IRealize<DescriptionType, ActualType> Instance = Realize.Instance as IRealize<DescriptionType, ActualType> ?? new Realize<DescriptionType, ActualType>();

        //default implementation
        ActualType IRealize<DescriptionType, ActualType>.RealizeDes(DescriptionType description)
        {
            Debug.AssertFormat(false, "Missing realize implementation for description-type pair");
            return default;
        }
    }

    partial class Realize
    {
        public static Realize Instance = new Realize();
    }

    public static class GlobalRealize
    {
        public static ActualType RealizeDes<DescriptionType, ActualType>(DescriptionType description)
        {
            return Realize<DescriptionType, ActualType>.Instance.RealizeDes(description);
        }

    }

}
