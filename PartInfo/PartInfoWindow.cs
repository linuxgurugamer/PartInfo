using ClickThroughFix;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;


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

        FloatCurveGraph[,] floatCurveGraphs = new FloatCurveGraph[MAXMODULES, 5];

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
            GameEvents.onEditorPartEvent .Add(onEditorPartEvent);
        }

        void onEditorPartEvent(ConstructionEventType  cet, Part e)
        {
            Debug.Log("PartInfo:  onEditorPartEvent");
            floatCurveGraphs = new FloatCurveGraph[MAXMODULES, 5];
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

            using (new GUILayout.VerticalScope())
            {

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

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.TextArea(str + "\n" + resVal);
                }
                using (new GUILayout.HorizontalScope())
                {
                    bool newCopyAll = GUILayout.Toggle(copyAll, "Copy All");

                    if (newCopyAll != copyAll)
                    {
                        for (int i = 0; i < part.Modules.Count; i++)
                            printModule[i] = newCopyAll;
                        copyAll = newCopyAll;
                    }
                }
                if (showFull)
                {
                    using (new GUILayout.VerticalScope())
                    {
                        for (int i = 0; i < part.Modules.Count; i++)
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

                                    using (new GUILayout.HorizontalScope())
                                    {

                                        printModule[cnt] = GUILayout.Toggle(printModule[cnt], "");
                                        if (!printModule[cnt])
                                            copyAll = false;
                                        if (!HighLogic.CurrentGame.Parameters.CustomParams<PartInfoSettings>().useAltSkin)
                                            GUILayout.TextArea(StripHtml(tmpSb.ToString()), GUILayout.Width(winRect.width - 90));
                                        else
                                            GUILayout.TextArea(StripHtml(tmpSb.ToString()), GUILayout.Width(winRect.width - 80));
                                    }
                                    cnt++;
                                }

                                if (m is ModuleEngines || m is ModuleEnginesFX || m is ModuleRCS || m is ModuleRCSFX) 
                                {
                                    int i1 = 0;

                                    if (!HighLogic.CurrentGame.Parameters.CustomParams<PartInfoSettings>().useAltSkin)
                                        i1 = 110;
                                    else
                                        i1 = 100;
                                    if (m is ModuleEngines || m is ModuleEnginesFX)
                                    {
                                        for (int curveCnt = 0; curveCnt < 5; curveCnt++)
                                        {

                                            bool useCurve = false;
                                            switch (curveCnt)
                                            {
                                                case 0:
                                                    if (((ModuleEngines)m).useThrustCurve)
                                                    {
                                                        using (new GUILayout.HorizontalScope())
                                                        {
                                                            GUILayout.Space(i1 - 60);
                                                            GUILayout.Label("Use Thrust Curve: " + ((ModuleEngines)m).useThrustCurve);
                                                        }

                                                        if (((ModuleEngines)m).useThrustCurve)
                                                        {
                                                            if (floatCurveGraphs[i, curveCnt] == null)
                                                                floatCurveGraphs[i, curveCnt] = new FloatCurveGraph(WIDTH - i1 + 10, ((ModuleEngines)m).thrustCurve);
                                                            useCurve = true;
                                                        }
                                                    }
                                                    break;
                                                case 1:
                                                    if (((ModuleEngines)m).useVelCurve)
                                                    {
                                                        using (new GUILayout.HorizontalScope())
                                                        {
                                                            GUILayout.Space(i1 - 60);
                                                            GUILayout.Label("Use Velocity Curve: " + ((ModuleEngines)m).useVelCurve);
                                                        }
                                                        if (((ModuleEngines)m).useVelCurve)
                                                        {
                                                            if (floatCurveGraphs[i, curveCnt] == null)
                                                                floatCurveGraphs[i, curveCnt] = new FloatCurveGraph(WIDTH - i1 + 10, ((ModuleEngines)m).velCurve);
                                                            useCurve = true;
                                                        }
                                                    }
                                                    break;
                                                case 2:
                                                    if (((ModuleEngines)m).useThrottleIspCurve)
                                                    {
                                                        using (new GUILayout.HorizontalScope())
                                                        {
                                                            GUILayout.Space(i1 - 60);
                                                            GUILayout.Label("Use Throttle Isp Curve: " + ((ModuleEngines)m).useThrottleIspCurve);
                                                        }
                                                        if (((ModuleEngines)m).useThrottleIspCurve)
                                                        {
                                                            if (floatCurveGraphs[i, curveCnt] == null)
                                                                floatCurveGraphs[i, curveCnt] = new FloatCurveGraph(WIDTH - i1 + 10, ((ModuleEngines)m).throttleIspCurve);
                                                            useCurve = true;
                                                        }
                                                    }
                                                    break;
                                                case 3:
                                                    if (((ModuleEngines)m).useAtmCurve)
                                                    {
                                                        using (new GUILayout.HorizontalScope())
                                                        {
                                                            GUILayout.Space(i1 - 60);
                                                            GUILayout.Label("Use Atmo Curve: " + ((ModuleEngines)m).useAtmCurve);
                                                        }
                                                        if (((ModuleEngines)m).useAtmCurve)
                                                        {
                                                            if (floatCurveGraphs[i, curveCnt] == null)
                                                                floatCurveGraphs[i, curveCnt] = new FloatCurveGraph(WIDTH - i1 + 10, ((ModuleEngines)m).atmCurve);
                                                            useCurve = true;
                                                        }
                                                    }
                                                    break;
                                                case 4:
                                                    if (((ModuleEngines)m).useAtmCurveIsp)
                                                    {
                                                        using (new GUILayout.HorizontalScope())
                                                        {
                                                            GUILayout.Space(i1 - 60);
                                                            GUILayout.Label("Use Atmo ISP Curve: " + ((ModuleEngines)m).useAtmCurveIsp);
                                                        }
                                                        if (((ModuleEngines)m).useAtmCurveIsp)
                                                        {
                                                            if (floatCurveGraphs[i, curveCnt] == null)
                                                                floatCurveGraphs[i, curveCnt] = new FloatCurveGraph(WIDTH - i1 + 10, ((ModuleEngines)m).atmCurveIsp);
                                                            useCurve = true;
                                                        }
                                                    }
                                                    break;
                                            }

                                            if (useCurve)
                                            {
                                                using (new GUILayout.HorizontalScope())
                                                {
                                                    GUILayout.Space(i1 - 60);
                                                    GUILayout.Box(floatCurveGraphs[i, curveCnt].graph);
                                                    floatCurveGraphs[i, curveCnt].graph.Apply();
                                                }
                                                using (new GUILayout.HorizontalScope())
                                                {
                                                    sb.Append(floatCurveGraphs[i, curveCnt].floatCurveString);
                                                    GUILayout.Space(i1 - 60);

                                                    if (!HighLogic.CurrentGame.Parameters.CustomParams<PartInfoSettings>().useAltSkin)
                                                        GUILayout.TextArea(StripHtml(floatCurveGraphs[i, curveCnt].floatCurveString.ToString()), GUILayout.Width(winRect.width - 90));
                                                    else
                                                        GUILayout.TextArea(StripHtml(floatCurveGraphs[i, curveCnt].floatCurveString.ToString()), GUILayout.Width(winRect.width - 80));

                                                    sbPrint.Append(floatCurveGraphs[i, curveCnt].floatCurveString);
                                                }
                                            }
                                        }
                                    }

                                    if (m is ModuleRCS || m is ModuleRCSFX)
                                    {
                                        for (int curveCnt = 0; curveCnt < 2; curveCnt++)
                                        {

                                            bool useCurve = false;
                                            switch (curveCnt)
                                            {
                                                case 0:
                                                    if (((ModuleRCS)m).useThrustCurve)
                                                    {
                                                        using (new GUILayout.HorizontalScope())
                                                        {
                                                            GUILayout.Space(i1 - 60);
                                                            GUILayout.Label("Use Thrust Curve: " + ((ModuleRCS)m).useThrustCurve);
                                                        }

                                                        if (((ModuleRCS)m).useThrustCurve)
                                                        {
                                                            if (floatCurveGraphs[i, curveCnt] == null)
                                                                floatCurveGraphs[i, curveCnt] = new FloatCurveGraph(WIDTH - i1 + 10, ((ModuleRCS)m).thrustCurve);
                                                            useCurve = true;
                                                        }
                                                    }
                                                    break;
                                                case 1:
                                                    using (new GUILayout.HorizontalScope())
                                                    {
                                                        GUILayout.Space(i1 - 60);
                                                        GUILayout.Label("Atmo curve");
                                                    }
                                                        if (floatCurveGraphs[i, curveCnt] == null)
                                                            floatCurveGraphs[i, curveCnt] = new FloatCurveGraph(WIDTH - i1 + 10, ((ModuleRCS)m).atmosphereCurve);
                                                        useCurve = true;
                                                    break;
                                            }

                                            if (useCurve)
                                            {
                                                using (new GUILayout.HorizontalScope())
                                                {
                                                    GUILayout.Space(i1 - 60);
                                                    GUILayout.Box(floatCurveGraphs[i, curveCnt].graph);
                                                    floatCurveGraphs[i, curveCnt].graph.Apply();
                                                }
                                                using (new GUILayout.HorizontalScope())
                                                {
                                                    sb.Append(floatCurveGraphs[i, curveCnt].floatCurveString);
                                                    GUILayout.Space(i1 - 60);

                                                    if (!HighLogic.CurrentGame.Parameters.CustomParams<PartInfoSettings>().useAltSkin)
                                                        GUILayout.TextArea(StripHtml(floatCurveGraphs[i, curveCnt].floatCurveString.ToString()), GUILayout.Width(winRect.width - 90));
                                                    else
                                                        GUILayout.TextArea(StripHtml(floatCurveGraphs[i, curveCnt].floatCurveString.ToString()), GUILayout.Width(winRect.width - 80));

                                                    sbPrint.Append(floatCurveGraphs[i, curveCnt].floatCurveString);
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                }

                GUILayout.EndScrollView();
                GUILayout.FlexibleSpace();
                using (new GUILayout.HorizontalScope())
                {
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
                }
            }
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

