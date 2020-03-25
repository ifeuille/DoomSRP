﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomSRP
{
    [Serializable]
    public class SpriteData
    {
        public string name = "Sprite";
        public int x = 0;
        public int y = 0;
        public int width = 0;
        public int height = 0;

        public int borderLeft = 0;
        public int borderRight = 0;
        public int borderTop = 0;
        public int borderBottom = 0;

        public int paddingLeft = 0;
        public int paddingRight = 0;
        public int paddingTop = 0;
        public int paddingBottom = 0;
        public bool hasBorder { get { return (borderLeft | borderRight | borderTop | borderBottom) != 0; } }

        public bool hasPadding { get { return (paddingLeft | paddingRight | paddingTop | paddingBottom) != 0; } }


        public void SetRect(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public void SetPadding(int left, int bottom, int right, int top)
        {
            paddingLeft = left;
            paddingBottom = bottom;
            paddingRight = right;
            paddingTop = top;
        }
        public void SetBorder(int left, int bottom, int right, int top)
        {
            borderLeft = left;
            borderBottom = bottom;
            borderRight = right;
            borderTop = top;
        }


        public void CopyFrom(SpriteData sd)
        {
            name = sd.name;

            x = sd.x;
            y = sd.y;
            width = sd.width;
            height = sd.height;

            borderLeft = sd.borderLeft;
            borderRight = sd.borderRight;
            borderTop = sd.borderTop;
            borderBottom = sd.borderBottom;

            paddingLeft = sd.paddingLeft;
            paddingRight = sd.paddingRight;
            paddingTop = sd.paddingTop;
            paddingBottom = sd.paddingBottom;
        }
        public void CopyBorderFrom(SpriteData sd)
        {
            borderLeft = sd.borderLeft;
            borderRight = sd.borderRight;
            borderTop = sd.borderTop;
            borderBottom = sd.borderBottom;
        }

    }
}
