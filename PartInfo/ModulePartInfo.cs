using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ClickThroughFix;

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
                string path = this.part.partInfo.partUrl;

                string mod = "";
                if (path.Length > 0)
                    mod = path.Substring(0, path.IndexOf('/'));

                st = path = "Mod: " + mod;
                st += "\nOrig Name: " + this.originalPartName;
                st += "\nUpdt Name: " + this.part.partInfo.name;

                st += "\nPath: " + this.part.partInfo.partUrl + "\n";
            }


            return st;
        }

        [KSPEvent(active = true, guiActive = false, guiActiveEditor = true, guiName = "Show Part Info")]
        void ShowPartInfo()
        {
            isVisible = true;
        }
        const int WIDTH = 500;
        const int HEIGHT = 200;

        private Rect winRect = new Rect(0, 0, WIDTH, HEIGHT);
        bool isVisible = false;
        void Start()
        {
            if (!HighLogic.LoadedSceneIsEditor)
                Destroy(this);
            winRect.x = (Screen.width - WIDTH) / 2;
            winRect.y = (Screen.height - HEIGHT) / 2;
        }
        private void OnGUI()
        {
            if (HighLogic.LoadedSceneIsEditor && isVisible)
            {
                GUI.skin = HighLogic.Skin;
                winRect = ClickThruBlocker.GUILayoutWindow(456789764, winRect, Window, "Part Information");
            }
        }
        void Window(int id)
        {
            GUILayout.BeginVertical();
            GUIContent str = new GUIContent(GetInfo());
            Vector2 size = GUI.skin.textArea.CalcSize(str);
            winRect.width = size.x + 10;
            GUILayout.TextArea(GetInfo());
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close"))
                isVisible = false;


            str = new GUIContent("Copy to clipboard");
            size = GUI.skin.button.CalcSize(str);

            if (GUILayout.Button(str,GUILayout.Width(size.x + 20)))
            {
                string s = GetInfo();
                s.CopyToClipboard();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
    internal static class StringStuff
    { 
        public static void CopyToClipboard(this string s)
        {
            TextEditor te = new TextEditor();
            te.text = s;
            te.SelectAll();
            te.Copy();
        }
    }
}
