using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Fungus
{
    /// <summary>
    /// Sets the Renderer sorting layer of every child of a game object. Applies to all Renderers (including mesh, skinned mesh, and sprite).
    /// </summary>
    [CommandInfo("Sprite",
                 "Set Image Sorting Layer",
                 "Sets the Renderer sorting layer of every child of a game object. Applies to all Renderers (including mesh, skinned mesh, and sprite).")]
    [AddComponentMenu("")]
    public class SetImageLayer : Command
    {
        [Tooltip("Root Object that will have the Sorting Layer set. Any children will also be affected")]
        [SerializeField] protected Image targetObject;

        [Tooltip("The New Layer Name to apply")]
        [SerializeField] protected string sortingLayer;

        [SerializeField] protected int sortingOrder;

        protected void ApplySortingLayer(Transform target, string layerName)
        {

            Canvas canvas = null;
            if (!target.TryGetComponent<Canvas>(out canvas)) {
            canvas = targetObject.gameObject.AddComponent<Canvas>();
            }

                canvas.overrideSorting = true;
                canvas.sortingLayerName = layerName;
                canvas.sortingOrder = sortingOrder;

        }

        #region Public members

        public override void OnEnter()
        {
            if (targetObject != null)
            {
                ApplySortingLayer(targetObject.transform, sortingLayer);
            }

            Continue();
        }

        public override string GetSummary()//°»´ú´£¥Ü
        {
            if (targetObject == null)
            {
                return "Error: No game object selected";
            }

            return targetObject.name;
        }

        public override Color GetButtonColor()
        {
            return new Color32(235, 191, 217, 255);
        }

        #endregion
    }
}