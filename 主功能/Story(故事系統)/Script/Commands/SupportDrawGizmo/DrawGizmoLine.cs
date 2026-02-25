using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Fungus
{

    public static class DrawGizmoLine
    {
        // Start is called before the first frame update
        public static void DrawTexture(Vector3[] vec3Arr)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < vec3Arr.Length; i++)
            {
                if (i == (vec3Arr.Length - 1))
                {

                    Gizmos.DrawLine(vec3Arr[i], vec3Arr[0]);
                }
                else
                {

                    Gizmos.DrawLine(vec3Arr[i], vec3Arr[i + 1]);
                }
            }
        }
        public static void DrawTexture(RectTransform rect)
        {
            Vector3[] vec3s = new Vector3[4];
            rect.GetWorldCorners(vec3s);
            DrawTexture(vec3s);

        }
    }

    public interface SupportDrawGizmo
    {

        public void DrawTexture();

        //public void DrawTexture(Vector3[] vec3Arr);

    }




}