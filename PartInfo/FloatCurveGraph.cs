using Smooth.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PartInfo
{
    public class FloatString4 : IComparable<FloatString4>
    {
        public Vector4 floats;
        public string[] strings;

        public int CompareTo(FloatString4 other)
        {
            if (other == null)
            {
                return 1;
            }
            return floats.x.CompareTo(other.floats.x);
        }

        public FloatString4()
        {
            floats = new Vector4();
            strings = new string[] { "0", "0", "0", "0" };
        }

        public FloatString4(float x, float y, float z, float w)
        {
            floats = new Vector4(x, y, z, w);
            UpdateStrings();
        }

        public void UpdateFloats()
        {
            float x, y, z, w;
            float.TryParse(strings[0], out x);
            float.TryParse(strings[1], out y);
            float.TryParse(strings[2], out z);
            float.TryParse(strings[3], out w);
            floats = new Vector4(x, y, z, w);
        }

        public void UpdateStrings()
        {
            strings = new string[] { floats.x.ToString(), floats.y.ToString(), floats.z.ToString(), floats.w.ToString() };
        }
    }

    internal class FloatCurveGraph
    {
        private int texWidth = 500;
        private int texHeight = 128;
        const int GraphLabels = 4;
        const float labelSpace = 20f * (GraphLabels + 1) / GraphLabels;

        private List<FloatString4> points = new List<FloatString4>();
        private FloatCurve curve;
        public Texture2D graph;
        public StringBuilder floatCurveString = new StringBuilder();
        private float minY;
        private float maxY;

        public  FloatCurveGraph(float width, FloatCurve floatCurve)
        {
            texWidth = (int)width;

            foreach (Keyframe v in floatCurve.Curve.keys)
            {
                points.Add(new FloatString4(v.time, v.value, v.outTangent, v.inTangent));
            }

            ConfigNode curveNode = new ConfigNode();

           floatCurve.Save(curveNode);

            graph = new Texture2D(texWidth, texHeight, TextureFormat.RGB24, false, true);
            
            points.Sort();

            UpdateCurve();
        }

        public FloatCurveGraph(float width)
        {
            texWidth = (int)width;

            graph = new Texture2D(texWidth, texHeight, TextureFormat.RGB24, false, true);

            points.Add(new FloatString4(0, 0, 0, 0));
            points.Add(new FloatString4(1, 1, 0, 0));

            points.Sort();

            UpdateCurve();
        }


        private void UpdateCurve()
        {
            curve = new FloatCurve();

            minY = float.MaxValue;
            maxY = float.MinValue;

            foreach (FloatString4 v in points)
            {
                curve.Add(v.floats.x, v.floats.y, v.floats.z, v.floats.w);
            }


            for (int x = 0; x < texWidth; x++)
            {
                for (int y = 0; y < texHeight; y++)
                {
                    graph.SetPixel(x, y, Color.black);
                }
                float fY = curve.Evaluate(curve.minTime + curve.maxTime * x / (texWidth - 1));
                minY = Mathf.Min(minY, fY);
                maxY = Mathf.Max(maxY, fY);
            }

            for (int x = 0; x < texWidth; x++)
            {
                float step = texHeight / (float)GraphLabels;
                for (int y = 0; y < GraphLabels; y++)
                {
                    graph.SetPixel(x, Mathf.RoundToInt(y * step), Color.gray);
                }
            }

            for (int x = 0; x < texWidth; x++)
            {
                float fY = curve.Evaluate(curve.minTime + curve.maxTime * x / (texWidth - 1));
                graph.SetPixel(x, Mathf.RoundToInt((fY - minY) / (maxY - minY) * (texHeight - 1)), Color.green);
            }
            graph.Apply();
            CurveToString();
        }

        private string CurveToString()
        {
            string buff = "";
            floatCurveString.Clear();

            foreach (FloatString4 p in points)
            {
                floatCurveString.AppendLine("key" + " = " + p.floats.x + " " + p.floats.y + " " + p.floats.z + " " + p.floats.w);
            }
            return buff;
        }

    }
}
