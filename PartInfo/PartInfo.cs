using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PartInfo
{
    public class ModulePartInfo : PartModule
    {
        public override string GetInfo()
        {
            string st = "";
            if (HighLogic.CurrentGame != null)
            {
                if (HighLogic.CurrentGame.Parameters.CustomParams<Workshop_Settings>().showPartName)
                    st = "Part Name: " + this.part.partInfo.name;
                if (HighLogic.CurrentGame.Parameters.CustomParams<Workshop_Settings>().showPartPath)
                {
                    if (st != "")
                        st += "\n";
                    st = "Part Path: " + this.part.partInfo.partUrl;
                }
            }
            return st;
        }
    }
}
