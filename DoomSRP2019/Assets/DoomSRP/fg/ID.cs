using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoomSRP.FG
{
    public static class ID
    {
        static uint id = 0;
        static List<uint> returnedIDs = new List<uint>();

        public static uint GenerateID()
        {
            uint retId = 0;
            if(returnedIDs.Count > 0)
            {
                retId = returnedIDs[returnedIDs.Count - 1];
                returnedIDs.RemoveAt(returnedIDs.Count - 1);
            }
            else
            {
                retId = id++;
            }
            return retId;
        }
        public static void returnID(uint id)
        {
            if (returnedIDs.Contains(id)) return;
            returnedIDs.Add(id);
        }
    }
}
