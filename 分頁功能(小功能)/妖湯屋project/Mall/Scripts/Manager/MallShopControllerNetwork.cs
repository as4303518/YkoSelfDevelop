using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UniRx;
using UnityEngine;
using YKO.CONST;
using YKO.MallShop;
using YKO.Network;

namespace YKO.MallShop
{

    /// <summary>
    /// 處理MallShopContorller Proto與Respone的部份
    /// </summary>
    public partial class MallShopController
    {
        /// <summary>
        /// 分辨場景對應的proto編號(轉換資料
        /// </summary>
        public Dictionary<ShopMallPage, MallPageInfo> mallSaveData = new Dictionary<ShopMallPage, MallPageInfo>()
        {
            { ShopMallPage.Item,new MallPageInfo( MallConst.MallType.GodShop)},//1
            { ShopMallPage.GodPersonality,new MallPageInfo( MallConst.MallType.Recovery)},//3-2
            { ShopMallPage.HeroHeart,new MallPageInfo( MallConst.MallType.HeroSoulShop)},//53
            { ShopMallPage.Prophet,new MallPageInfo( MallConst.MallType.Seerpalace)},//31
            { ShopMallPage.SeaChallenge,new MallPageInfo( MallConst.MallType.FriendShop)},//8
            { ShopMallPage.Sports,new MallPageInfo( MallConst.MallType.ArenaShop)},//6
            { ShopMallPage.Guild,new MallPageInfo( MallConst.MallType.UnionShop)},//5
            { ShopMallPage.Casino,new MallPageInfo( MallConst.MallType.GuessShop)},//3-16
            { ShopMallPage.Level,new MallPageInfo( MallConst.MallType.EliteShop)},//17
            { ShopMallPage.Skill,new MallPageInfo( MallConst.MallType.SkillShop)},//3-9

            { ShopMallPage.PlumeShop,new MallPageInfo( MallConst.MallType.PlumeShop)},//50  需求等級過高,可能不會做
            { ShopMallPage.None,new MallPageInfo( MallConst.MallType.GodShop)},//0
            //{ ShopMallPage.Skill,new MallInfo( MallConst.MallType.VarietyShop,typeof(Proto_13403_Request))},
        };



        //道具  13401- 1
        //命介/ 神格兌換=> 商城/英雄 13403  -2  所有商品購買完會自動填充  (命介的功能會出現篩選屬性表
        //命介/ 神魂之心=>福利/每周限購/英魂商店 13401 -53
        //命介/先知結晶兌換  =>先知召喚/先知商店  13401 -31

        //積分
        //征戰 13401 -8
        //競技 13401 -6
        //公會 13401 -5
        //賭坊 (13403 -16) (13401-16)閃爍一次打兩個
        //段位 13401 -17
        //技能 13403-9 技能每次點擊都會重複拿到proto  刷新13405  -9   免費刷新重製 讀秒

        //其餘與商城照舊  積分 13401 13403


        /// <summary>
        /// 獲得對應商店頁面當下資訊的Class
        /// </summary>
        /// <param name="mallType"></param>
        /// <returns></returns>
        private MallPageInfo GetMallInfoPage(ShopMallPage mallType)
        {
            if (!mallSaveData.ContainsKey(mallType))
            {
                Debug.LogError("Not Setting mall from mallSaveData" + mallType.ToString());
                return null;
            }
            else
            {
                return mallSaveData[mallType];
            }

        }
    }
}


/// <summary>
/// 紀錄每個不同的頁面開啟的商場類型
/// </summary>
public class MallPageInfo
{
    /// <summary>
    /// 對應的商場類型
    /// </summary>
    public MallConst.MallType mallType;

    public MallPageInfo(MallConst.MallType _mallType)
    {
        mallType = _mallType;
    }
}