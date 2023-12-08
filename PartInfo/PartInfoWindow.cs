using ClickThroughFix;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

//using KSP_Log;

namespace PartInfo
{
    internal class PartInfoWindow : MonoBehaviour
    {
        static Dictionary<uint, PartInfoWindow> instanceList;

        const int WIDTH = 500;
        float HEIGHT = Screen.height * 0.75f;

        private Rect winRect;
        bool showFull = false;
        bool showFullToggle = true;

        string bold = "<b>", unbold = "</b>";

        float maxPrintWidth = 0;

        StringBuilder sb = new StringBuilder();
        StringBuilder tmpSb = new StringBuilder();
        StringBuilder sbPrint = new StringBuilder();

        Vector2 scrollPos;

        const int MAXMODULES = 100;
        bool[] printModule = null;
        bool copyAll = true;

        ModulePartInfo mpi;
        Part part;

        //internal static Log Log = null;

        void Start()
        {
            if (!HighLogic.LoadedSceneIsEditor && !HighLogic.CurrentGame.Parameters.CustomParams<PartInfoSettings>().availableInFlight)
                Destroy(this);
            //if (Log == null)
            //    Log = new Log("PartInfo", Log.LEVEL.INFO);
            //Log.Info("Start, ModulePartInfo.currentPart.persistentId: " + ModulePartInfo.currentPart.persistentId);

            if (instanceList == null)
            {
                //Log.Info("Creating new instanceList");
                instanceList = new Dictionary<uint, PartInfoWindow>();
            }
            part = ModulePartInfo.currentPart;
            mpi = ModulePartInfo.currentModule;

            if (instanceList.ContainsKey(part.persistentId))
            {
                part = null; mpi = null;
                Destroy(this);
                return;
            }

            instanceList.Add(part.persistentId, this);

            winRect = new Rect(0, 0, WIDTH, HEIGHT);
            winRect.x = (Screen.width - WIDTH) / 2;
            winRect.y = (Screen.height - HEIGHT) / 2;
            showFullToggle = HighLogic.CurrentGame.Parameters.CustomParams<PartInfoSettings>().showFullWindow;

            //if (printModule == null)
            printModule = new bool[MAXMODULES];


        }

        private void OnGUI()
        {
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
                info = info.Replace(@"\n", "\n");
                tmpSb.AppendLine(info);

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

            string str = mpi.GetInfo().TrimEnd('\r', '\n', ' ');
            str = str.Replace(@"\n", "\n");

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

            if (HighLogic.LoadedSceneIsEditor)
            {
                showFull = (EditorLogic.RootPart == this.part || this.part.parent != null);
            }
            else
            {
                showFull = true;
            }
            if (showFull)
                showFullToggle = GUILayout.Toggle(showFullToggle, "Show all modules");
            else
                showFullToggle = false;
            if (!showFullToggle)
                winRect.height /= 3;

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(winRect.height - 70));

            GUILayout.BeginHorizontal();
            GUILayout.TextArea(str + "\n" + resVal);

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            bool newCopyAll = GUILayout.Toggle(copyAll, "Copy All");

            if (newCopyAll != copyAll)
            {
                for (int i = 0; i < part.Modules.Count; i++)
                    printModule[i] = newCopyAll;
                copyAll = newCopyAll;
            }

            GUILayout.EndHorizontal();

            if (showFull)
            {
                for (int i = 0; i < part.Modules.Count; i++)
                //foreach (var m in part.Modules)
                {
                    var m = part.Modules[i];
                    if (m.moduleName != ModulePartInfo.MODULENAME)
                    {
                        tmpSb.Clear();
                        var info = m.GetInfo().TrimEnd(' ', '\r', '\n');

                        if (info != null && info != "")
                        {
                            tmpSb.AppendLine(bold + m.moduleName + unbold);
                            tmpSb.AppendLine();

                            info = info.Replace(@"\n", "\n");
                            tmpSb.AppendLine(info);

                            sb.Append(tmpSb);
                            if (printModule[cnt] || copyAll)
                            {
                                sbPrint.Append(tmpSb);
                                AddDashedLine();
                            }

                            GUILayout.BeginHorizontal();
                            printModule[cnt] = GUILayout.Toggle(printModule[cnt], "");
                            if (!printModule[cnt])
                                copyAll = false;
                            if (!HighLogic.CurrentGame.Parameters.CustomParams<PartInfoSettings>().useAltSkin)
                                GUILayout.TextArea(StripHtml(tmpSb.ToString()), GUILayout.Width(winRect.width - 90));
                            else
                                GUILayout.TextArea(StripHtml(tmpSb.ToString()), GUILayout.Width(winRect.width - 80));
                            GUILayout.EndHorizontal();
                            cnt++;
                        }
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Close"))
            {
                Destroy(this);
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
        void OnDestroy()
        {
            //Log.Info("OnDestroy");
            if (part != null)
                instanceList.Remove(part.persistentId);
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

