using System;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine;
using ClickThroughFix;

namespace PartInfo
{
    public class ModulePartInfo : PartModule
    {
        [KSPField]
        public string originalPartName;

        internal  static Part currentPart = null;
        internal static ModulePartInfo currentModule = null;

        internal const string MODULENAME = "ModulePartInfo";

        internal string bold = "<b>", unbold = "</b>";

        public override string GetInfo()
        {
            moduleName = MODULENAME;

            string st = "";
            //if (HighLogic.CurrentGame != null)
            {
                string path = this.part.partInfo.partUrl;

                string mod = "";
                if (path.Length > 0)
                    mod = path.Substring(0, path.IndexOf('/'));

                st = path = bold + "Mod: " + mod + unbold;
                if (originalPartName == part.partInfo.name)
                {
                    st += "\nName: " + this.originalPartName;
                }
                else
                {
                    st += "\nOrig Name: " + this.originalPartName;
                    st += "\nUpdt Name: " + this.part.partInfo.name;
                }
                st += "\nPath: " + this.part.partInfo.partUrl;

                st += "\nSize: " + this.part.partInfo.partSize.ToString("F2");
                st += "\nBulkhead Profiles: " + this.part.partInfo.bulkheadProfiles + "\n";
            }

            return st;
        }

        [KSPEvent(active = true, guiActive = false, guiActiveEditor = true, guiName = "Show Part Info")]
        void ShowPartInfo()
        {
            AddPartInfoWindow();
        }
        void Start()
        {
            if (!HighLogic.LoadedSceneIsEditor && !HighLogic.CurrentGame.Parameters.CustomParams<PartInfoSettings>().availableInFlight)
                Destroy(this);
            if (HighLogic.LoadedSceneIsFlight)
                Events["ShowPartInfo"].guiActive = true;

        }


        void AddPartInfoWindow()
        {
            currentPart = this.part;
            currentModule = this;
            this.gameObject.AddComponent<PartInfoWindow>();
        }
    }
}
