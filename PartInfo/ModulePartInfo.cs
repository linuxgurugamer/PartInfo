using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PartInfo
{
    public class ModulePartInfo : PartModule
    {
        public override string GetInfo()
        {
             moduleName = "ModulePartInfo";

            string st = "";
            //if (HighLogic.CurrentGame != null)
            {

               // if (HighLogic.CurrentGame == null || HighLogic.CurrentGame.Parameters.CustomParams<PartInfoSettings>().showPartName)
                    st = "Part Name: " + this.part.partInfo.name;
                //if (HighLogic.CurrentGame == null || HighLogic.CurrentGame.Parameters.CustomParams<PartInfoSettings>().showPartPath)
                {
                    if (st != "")
                        st += "\n";
                    st += "Part Path: " + this.part.partInfo.partUrl;
                }
            }

     
            return st;
        }
    }
}
