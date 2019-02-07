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

            string st = "Part Name: " + this.part.partInfo.name + "\n" + "Part Path: " + this.part.partInfo.partUrl;
            return st;
        }
    }
}
