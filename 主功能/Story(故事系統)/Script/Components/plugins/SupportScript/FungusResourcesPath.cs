using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FungusResourcesPath
{
    /// <summary>
    /// 會出現的圖片格式(影響路徑判斷
    /// </summary>
    public static string[] ImageFormat = new string[] { "png", "jpg" };


    public static readonly string PrefabsPath = "Prefabs/PenguinPrefab/";

    public static readonly string ShaderEffectPath = "ShaderEffect/";

    public static readonly string StoryFlowchartPrefabsPath = "FlowchartPrefab/";

    public static readonly string FlowchartSaveData = "/Application/Story/Fungus/FlowChartDataToCsv/";

    public static readonly string Chara2DPortraits= "Portrait2D/";

    public static readonly string CharaVoice = "Audio/Voice/";

    public static readonly string SpineChara = "SpineChara/";

    public static readonly string StoryImage = "Image/Story/";

    public static readonly string Font = "Font/";

    public static readonly string AssetTextPath = "AssetText/";
    /// <summary>
    /// HCG資料夾名稱
    /// </summary>
    public static readonly string HCGFloderName = "HCG";

    #region Fungus Addressable Path

    /// <summary>
    /// Sns UI系統
    /// </summary>
    public static readonly string SnsSettingSprite = "Assets/Application/SnsSetting/Image/Atlas/{0}.png";
    /// <summary>
    /// 頭像
    /// </summary>
    public static readonly string SnsAvatarSprite = "Assets/Application/Common/UI/Avatar/avatar_{0}.png";



    /// <summary>
    /// Fungus Addressable Path
    /// </summary>
    public static readonly string AddressPath = "Assets/Application/Story/Fungus/Resources/";

    /// <summary>
    /// Fungus Addressable Path
    /// </summary>
    public static readonly string ShaderEffectAddressPath = "Assets/Application/Story/Fungus/Resources/ShaderEffect/";

    /// <summary>
    /// HCG路徑
    /// {0} spine or image
    /// {1} file name
    /// {2} file format ex: jpg,png
    /// </summary>
    public static readonly string HCGAddressPath = "Assets/Application/Story/Fungus/Resources/Image/HCG/{0}/{1}.{2}";



    #endregion


}
