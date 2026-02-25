using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YKO.Common.UI;
using YKO.SnsSetting;

/// <summary>
/// 為了分開執行PopupBase跟SnsSettingController所創建的腳本
/// (因為PopupBase有固定的開啟方式,但snsSetting需要跑 coroutinue流程)
/// </summary>
public class SnsPopup : PopupBase
{
    public SnsSettingController control;

    public override void Init()
    {
        base.Init();
        StartCoroutine( control.Init());
    }


}
