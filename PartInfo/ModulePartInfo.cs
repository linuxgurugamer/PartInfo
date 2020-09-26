using System;
using System.Text.RegularExpressions ;
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

        const string MODULENAME = "ModulePartInfo";
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
        string bold = "<b>", unbold = "</b>";
        private void OnGUI()
        {
            if (isVisible)
            {
                if (!HighLogic.CurrentGame.Parameters.CustomParams<PartInfoSettings>().useAltSkin)
                {
                    GUI.skin = HighLogic.Skin;
                    bold = "<b>";
                    unbold = "</b>";
                }
                else
                {
                    bold = "";
                    unbold = "";
                }
                //winRect.height = (float)(Screen.height * HighLogic.CurrentGame.Parameters.CustomParams<PartInfoSettings>().WindowHeightPercentage + 50);
                //winRect.width = maxPrintWidth;

                winRect = ClickThruBlocker.GUILayoutWindow((int)part.persistentId, winRect, Window, "Part Information");
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

        string FormatMass(double mass)
        {
            if (mass < 1)
                return (mass * 1000).ToString("F2") + " kg";
            return mass.ToString("F3") + " t";
        }
        string GetResourceValues()
        {
            tmpSb.Clear();
            tmpSb.AppendLine(bold + "Mass: " + unbold + FormatMass(part.mass));

            if (part.Resources.Count > 0)
            {

                tmpSb.AppendLine(bold + "Resources:" + unbold);
                foreach (PartResource r in part.Resources)
                {
                    double mass = r.amount * r.info.density;
                    tmpSb.AppendLine("    " + r.resourceName + ": " + r.amount.ToString("F1") + "/" + r.maxAmount.ToString("F1") + ", mass: " + FormatMass(mass));
                }
            }
            return tmpSb.ToString();
        }


        private string StripHtml(string source)
        {
            string output;

            //get rid of HTML tags
            output = Regex.Replace(source, "<[^>]*>", string.Empty);

            //get rid of multiple blank lines
            output = Regex.Replace(output, @"^\s*$\n", string.Empty, RegexOptions.Multiline);

            return output;
        }
        void AddDashedLine()
        {
            sbPrint.AppendLine("-----------------------------------------------");
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

            string str = GetInfo().TrimEnd('\r', '\n', ' ');
            sb.AppendLine(str);
            sbPrint.Append(sb);
            AddDashedLine();

            string resVal = GetResourceValues();
            sb.Append(tmpSb);
            sbPrint.Append(tmpSb);
            AddDashedLine();

            GUILayout.BeginVertical();

            int cnt = 0;
            winRect.height = (float)(Screen.height * HighLogic.CurrentGame.Parameters.CustomParams<PartInfoSettings>().WindowHeightPercentage);

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(winRect.height - 70));

            GUILayout.BeginHorizontal();
            GUILayout.TextArea(str + "\n" + resVal);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            copyAll = GUILayout.Toggle(copyAll, "Copy All");
            GUILayout.EndHorizontal();

            foreach (var m in part.Modules)
            {
                if (m.moduleName != MODULENAME)
                {
                    tmpSb.Clear();
                    var info = m.GetInfo().TrimEnd(' ', '\r', '\n');
                    if (info != null && info != "")
                    {
                        tmpSb.AppendLine(bold + m.moduleName + unbold);
                        tmpSb.AppendLine();
                        tmpSb.AppendLine(info);

                        sb.Append(tmpSb);
                        if (printModule[cnt] || copyAll)
                        {
                            sbPrint.Append(tmpSb);
                            AddDashedLine();
                        }

                        GUILayout.BeginHorizontal();
                        printModule[cnt] = GUILayout.Toggle(printModule[cnt], "");
                        if (!HighLogic.CurrentGame.Parameters.CustomParams<PartInfoSettings>().useAltSkin)
                            GUILayout.TextArea(StripHtml(tmpSb.ToString()), GUILayout.Width(winRect.width - 90));
                        else
                            GUILayout.TextArea(StripHtml( tmpSb.ToString()), GUILayout.Width(winRect.width - 80));
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
