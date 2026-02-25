// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

﻿// Original code by Martin Ecker (http://martinecker.com)

using UnityEngine;

namespace Fungus.EditorUtils
{
    // Helper Rect extension methods
    public static class RectExtensions
    {
        public static Vector2 TopLeft(this Rect rect)
        {
            return new Vector2(rect.xMin, rect.yMin);
        }

        public static Rect ScaleSizeBy(this Rect rect, float scale)
        {
            return rect.ScaleSizeBy(scale, rect.center);
        }

        public static Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
        {
            Rect result = rect;
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale;
            result.xMax *= scale;
            result.yMin *= scale;
            result.yMax *= scale;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }

        public static Rect ScaleSizeBy(this Rect rect, Vector2 scale)
        {
            return rect.ScaleSizeBy(scale, rect.center);
        }

        public static Rect ScaleSizeBy(this Rect rect, Vector2 scale, Vector2 pivotPoint)
        {
            Rect result = rect;
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale.x;
            result.xMax *= scale.x;
            result.yMin *= scale.y;
            result.yMax *= scale.y;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }


        public static Rect SnapPosition(this Rect rect, float snapInterval)
        {
            var tmp = rect;
            var x = tmp.position.x;
            var y = tmp.position.y;
            tmp.position = new Vector2(Mathf.RoundToInt(x / snapInterval) * snapInterval, Mathf.RoundToInt(y / snapInterval) * snapInterval);
            return tmp;
        }

        public static Rect SnapWidth(this Rect rect, float snapInterval)
        {
            var tmp = rect;
            tmp.width = Mathf.RoundToInt(tmp.width / snapInterval) * snapInterval;
            return tmp;
        }
    }

    public class EditorZoomArea
    {
        private static Matrix4x4 _prevGuiMatrix;

        /// <summary>
        /// 類似對準中央畫布的攝影機位移空間  zoomScale越大(體積越小,靠近畫面,UI的大小越大)反之則亦然 
        /// </summary>
        private static Vector2 offset = new Vector2(2.0f, 19.0f);
        public static Vector2 Offset { get { return offset; } set { offset = value; } }
        /// <summary>
        /// 切換整張window的圖的大小來進行縮放
        /// </summary>
        /// <param name="zoomScale"></param>
        /// <param name="screenCoordsArea"></param>
        /// <returns></returns>
        public static Rect Begin(float zoomScale, Rect screenCoordsArea)//攝影機縮放
        {
            GUI.EndGroup();        // End the group Unity begins automatically for an EditorWindow to clip out the window tab. This allows us to draw outside of the size of the EditorWindow.  

            Rect clippedArea = screenCoordsArea.ScaleSizeBy(1.0f / zoomScale, screenCoordsArea.TopLeft());

            clippedArea.position += offset;

            GUI.BeginGroup(clippedArea);
            
            _prevGuiMatrix = GUI.matrix;
            
            Matrix4x4 translation = Matrix4x4.TRS(clippedArea.TopLeft(), Quaternion.identity, Vector3.one);
            Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1.0f));

            //錨點在左上,所以縮放會以左上為標準進行位移(詳請可以看image毛點設置後的變化)


            GUI.matrix = translation * scale * translation.inverse * GUI.matrix;//切換整張圖的大小及定義錨點


            return clippedArea;
        }
        
        public static void End()
        {
            
            GUI.matrix = _prevGuiMatrix;
            GUI.EndGroup();
            GUI.BeginGroup(new Rect(offset.x, offset.y, Screen.width, Screen.height));
        }
    }
}