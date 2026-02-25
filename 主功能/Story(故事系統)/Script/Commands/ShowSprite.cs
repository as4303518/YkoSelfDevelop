// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace Fungus
{
    /// <summary>
    /// Makes a sprite visible / invisible by setting the color alpha.
    /// </summary>
    [CommandInfo("Sprite", 
                 "Show Sprite", 
                 "Makes a sprite visible / invisible by setting the color alpha.")]
    [AddComponentMenu("")]
    [ExecuteInEditMode]
    public class ShowSprite : Command
    {
        [Tooltip("Sprite object to be made visible / invisible")]
        [SerializeField] protected Image image;

        [Tooltip("Make the sprite visible or invisible")]
        [SerializeField] protected BooleanData _visible = new BooleanData(false);

        [Tooltip("Affect the visibility of child sprites")]
        [SerializeField] protected bool affectChildren = true;

        protected virtual void SetSpriteAlpha(Image _image, bool visible)
        {
            Color spriteColor = _image.color;
            spriteColor.a = visible ? 1f : 0f;
            _image.color = spriteColor;
        }

        #region Public members

        public override void OnEnter()
        {
            if (image != null)
            {
                if (affectChildren)
                {
                    var images = image.gameObject.GetComponentsInChildren<Image>();
                    for (int i = 0; i < images.Length; i++)
                    {
                        var sr = images[i];
                        SetSpriteAlpha(sr, _visible.Value);
                    }
                }
                else
                {
                    SetSpriteAlpha(image, _visible.Value);
                }
            }

            Continue();
        }

        public override string GetSummary()
        {
            if (image == null)
            {
                return "Error: No sprite renderer selected";
            }

            return image.name + " to " + (_visible.Value ? "visible" : "invisible");
        }

        public override Color GetButtonColor()
        {
            return new Color32(221, 184, 169, 255);
        }

        public override bool HasReference(Variable variable)
        {
            return _visible.booleanRef == variable || base.HasReference(variable);
        }

        #endregion

        #region Backwards compatibility

        [HideInInspector] [FormerlySerializedAs("visible")] public bool visibleOLD;

        protected virtual void OnEnable()
        {
            if (visibleOLD != default(bool))
            {
                _visible.Value = visibleOLD;
                visibleOLD = default(bool);
            }
        }

        #endregion
    }
}