// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fungus.EditorUtils
{
    /// <summary>
    /// Custom drawer for the BlockReference, allows for more easily selecting a target block in external c#
    /// scripts.
    /// </summary>
    [CustomPropertyDrawer(typeof(Fungus.BlockReference))]
    public class BlockReferenceDrawer : PropertyDrawer
    {
        public Fungus.Flowchart lastFlowchart;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var pro = EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, pro);

            position.height = EditorGUIUtility.singleLineHeight;

            var block = property.FindPropertyRelative("block");//獲取參數 block

            Fungus.Block ex_b = block.objectReferenceValue as Fungus.Block;//資產的類別

            if (block.objectReferenceValue != null && lastFlowchart == null)
            {
                if (ex_b != null)
                {
                    lastFlowchart = ex_b.GetFlowchart();
                }
            }

            lastFlowchart = EditorGUI.ObjectField(position, lastFlowchart, typeof(Fungus.Flowchart), true) as Fungus.Flowchart;
            position.y += EditorGUIUtility.singleLineHeight;

            if (lastFlowchart != null)
            {
                ex_b = Fungus.EditorUtils.BlockEditor.BlockField(position, new GUIContent("None"), lastFlowchart, ex_b);
            }
            else
            {
                EditorGUI.PrefixLabel(position, new GUIContent("Flowchart Required"));
            }

            block.objectReferenceValue = ex_b;
            block.serializedObject.ApplyModifiedProperties();
            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2;
        }
    }
}