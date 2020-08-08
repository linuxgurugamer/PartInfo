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

        Vector2 scrollPos;
        const int MAXMODULES = 100;
        bool[] printModule = null;
        float maxPrintWidth = 0;
        bool copyAll = true;

        const int WIDTH = 500;
        float HEIGHT = Screen.height * 0.75f;

        private Rect winRect;
        bool isVisible = false;

        StringBuilder sb = new StringBuilder();
        StringBuilder tmpSb = new StringBuilder();
        StringBuilder sbPrint = new StringBuilder();


        public override string GetInfo()
        {
            moduleName = "Part Info";

            string st = "";
            //if (HighLogic.CurrentGame != null)
            {
                string path = this.part.partInfo.partUrl;

                string mod = "";
                if (path.Length > 0)
                    mod = path.Substring(0, path.IndexOf('/'));

                st = path = "<b>Mod: " + mod + "</b>";
                if (originalPartName == part.partInfo.name)
                {
                    st += "\nName: " + this.originalPartName;
                }
                else
                {
                    st += "\nOrig Name: " + this.originalPartName;
                    st += "\nUpdt Name: " + this.part.partInfo.name;
                }
                st += "\nPath: " + this.part.partInfo.partUrl + "\n";
            }

            return st;
        }

        [KSPEvent(active = true, guiActive = false, guiActiveEditor = true, guiName = "Show Part Info")]
        void ShowPartInfo()
        {
            isVisible = true;
            if (printModule == null)
                printModule = new bool[MAXMODULES];
        }
        void Start()
        {
            if (!HighLogic.LoadedSceneIsEditor && !HighLogic.CurrentGame.Parameters.CustomParams<PartInfoSettings>().availableInFlight)
                Destroy(this);
            winRect = new Rect(0, 0, WIDTH, HEIGHT);
            winRect.x = (Screen.width - WIDTH) / 2;
            winRect.y = (Screen.height - HEIGHT) / 2;
            if (HighLogic.LoadedSceneIsFlight)
                Events["ShowPartInfo"].guiActive = true;
        }
        private void OnGUI()
        {
            if (isVisible)
            {
                GUI.skin = HighLogic.Skin;
                winRect.height = (float)(Screen.height * .75 + 50);
                winRect.width = maxPrintWidth;

                winRect = ClickThruBlocker.GUILayoutWindow(456789764, winRect, Window, "Part Information");
            }
        }

        void CalcWindowSize()
        {
            foreach (var m in part.Modules)
            {
                var info = m.GetInfo().TrimEnd(' ', '\r', '\n');

                tmpSb.Append(info);
                string str = tmpSb.ToString();
                GUIContent tmpContent = new GUIContent(str);
                Vector2 tmpSize = GUI.skin.textArea.CalcSize(tmpContent);
                maxPrintWidth = Math.Max(tmpSize.x + 10, maxPrintWidth);
                tmpSb.Clear();

            }

        }
        void Window(int id)
        {
            sb.Clear();
            tmpSb.Clear();
            sbPrint.Clear();
            if (maxPrintWidth == 0)
            {
                CalcWindowSize();
            }

            sb.Append(GetInfo().TrimEnd('\r', '\n', ' '));
            sbPrint.Append(sb);
            sbPrint.Append("\n-----------------------------------------------\n");

            GUILayout.BeginVertical();

            int cnt = 0;
            winRect.height = (float)(Screen.height * .75 + 20);
            GUILayout.BeginHorizontal();
            copyAll = GUILayout.Toggle(copyAll, "Copy All");
            GUILayout.EndHorizontal();
            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(winRect.height - 70));

            foreach (var m in part.Modules)
            {
                if (m.moduleName != "ModulePartInfo")
                {
                    tmpSb.Clear();
                    var info = m.GetInfo().TrimEnd(' ', '\r', '\n');
                    if (info != null && info != "")
                    {
                        tmpSb.AppendLine("<b>" + m.moduleName + "</b>");
                        tmpSb.AppendLine();
                        tmpSb.Append(info);

                        sb.Append(tmpSb);
                        if (printModule[cnt] || copyAll)
                        {
                            sbPrint.Append(tmpSb);
                            sbPrint.Append("\n-----------------------------------------------\n");
                        }

                        string str = tmpSb.ToString();
                        GUIContent tmpContent = new GUIContent(str);
                        Vector2 tmpSize = GUI.skin.textArea.CalcSize(tmpContent);
                        winRect.width = Math.Max(tmpSize.x + 10, winRect.width);
                        GUILayout.BeginHorizontal();
                        printModule[cnt] = GUILayout.Toggle(printModule[cnt], "");
                        GUILayout.TextArea(str);
                        GUILayout.EndHorizontal();
                        cnt++;
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close"))
            {
                isVisible = false;
                printModule = null;
            }


            GUIContent strContent;
            if (copyAll)
                strContent = new GUIContent("Copy all to clipboard");
            else
                strContent = new GUIContent("Copy to clipboard");
            var size = GUI.skin.button.CalcSize(strContent);

            if (GUILayout.Button(strContent, GUILayout.Width(size.x + 20)))
            {
                sbPrint.ToString().CopyToClipboard();
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
