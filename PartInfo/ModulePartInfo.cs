using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PartInfo
{
    public class ModulePartInfo : PartModule
    {
        [KSPField]
        public string originalPartName;

        public override string GetInfo()
        {
            moduleName = "ModulePartInfo";

            string st = "";
            //if (HighLogic.CurrentGame != null)
            {
                st = "Orig Name: " + this.originalPartName;
             st += "\nUpdt Name: " + this.part.partInfo.name;

                st += "\nPath: " + this.part.partInfo.partUrl;

            }


            return st;
        }
    }
}
