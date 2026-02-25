using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using YKO.Common.UI;
using YKO.Guild;
using YKO.Heaven;
using YKO.Main;
using YKO.MallShop;
using YKO.Mypage;

namespace YKO.SevenDay
{

    public static class Transimt
    {

        /// <summary>
        /// 根據編號傳送場景
        /// </summary>
        /// <param name="num"></param>
        public static void TransimtOfNum(long num)
        {

            switch (num)
            {
                case 100://布陣陣法
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.EditTeamScene);
                    break;
                case 120://召喚
                    var param120 = new MypageScene.SceneParameter()
                    {
                        showPage = MypageScene.MypagePage.GachaPage
                    };
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.MypageScene, param120);

                    break;
                case 121://背包碎片
                    var param121 = new MypageScene.SceneParameter()
                    {
                        showPage = MypageScene.MypagePage.StoragePage
                    };
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.MypageScene, param121);

                    break;
                case 122://英雄商城
                    MainManager.Instance.ShowCommonPopup("", "並沒找到英雄商城");
                    break;
                case 123://金幣兌換
                    MainManager.Instance.ShowCommonPopup("", "開發時,尚無實作金幣兌換功能");
                    break;
                case 126://遠航

                    var param126 = new HeavenScene.HeavenParam
                    {
                        showPopup= HeavenScene.ShowPopup.Assign
                    };
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.HeavenScene, param126);
                    break;
                case 129://日常
                    var param129 = new MypageScene.SceneParameter()
                    {
                        showPage = MypageScene.MypagePage.MissionPopup
                    };
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.MypageScene, param129);

                    break;
                case 130://成就
                    var param130 = new MypageScene.SceneParameter()
                    {
                        showPage = MypageScene.MypagePage.MissionPopup
                    };
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.MypageScene, param130);

                    break;
                case 131://充值
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.PaymentShopScene);
                    break;
                case 132://快速作戰
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.GuildScene);
                    break;
                case 134://雜貨店
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.MallScene);
                    break;
                case 138://鑽石商城
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.MallScene);
                    break;
                case 144://道具背包
                    var param144 = new MypageScene.SceneParameter()
                    {
                        showPage = MypageScene.MypagePage.StoragePage
                    };
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.MypageScene, param144);

                    break;
                case 145://聯盟捐獻
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.GuildScene);
                    break;
                case 146://公會副本
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.GuildScene);
                    break;
                case 150://星河神殿
                    break;
                case 151://英雄遠征
                    var param151 = new HeavenScene.HeavenParam
                    {
                        showPopup = HeavenScene.ShowPopup.Assign
                    };
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.HeavenScene, param151);
                    break;
                case 152://日常副本
                    break;
                case 153://無盡試煉
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.DungeonScene);
                    break;
                case 154://段造屋
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.ForgeHouseScene);
                    break;
                case 155://融合祭壇

                    break;
                case 156://祭祀小屋
                    break;
                case 157://劇情副本
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.StageScene);
                    break;
                case 158://競技場
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.ArenaScene);
                    break;
                case 159://冠軍賽
                    MainManager.Instance.ShowCommonPopup("", "開發時,尚無找到冠軍賽");
                    break;
                case 160://試煉塔
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.TrainingScene);
                    break;
                case 161://鍛造坊符文 同408
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.ForgeHouseScene);
                    break;
                case 162://金幣兌換
                    MainManager.Instance.ShowCommonPopup("", "開發時,尚無找到金幣兌換");
                    break;
                case 200://英雄背包
                    var param200 = new MypageScene.SceneParameter()
                    {
                        showPage = MypageScene.MypagePage.HeroPage
                    };
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.MypageScene,param200);

                    break;
                case 201://神器升级

                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.StrangthenScene);
                    break;
                case 202://聯盟技能
                    break;
                case 203://英雄資訊
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.HeroDetailScene);
                    break;
                case 204://先知殿
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.ObservatoryScene);
                    break;
                case 401://登入
                    break;
                case 402://好友
                    var param402 = new MypageScene.SceneParameter()
                    {
                        showPage = MypageScene.MypagePage.FriendPopup
                    };
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.MypageScene, param402);

                    break;
                case 404://英雄介面
                    break;
                case 405://幸運探寶 賭坊
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.CasinoScene);
                    break;
                case 406://探寶商店
                    var param406 = new MallShopController.SceneParameter()
                    {
                        page = ShopMallPage.Casino
                    };
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.MallScene,param406);
                    break;
                case 407://冒險
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.StageScene);
                    break;
                case 408://鍛造坊符文 同161
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.ForgeHouseScene);
                    break;
                case 410://菁英段位賽商店
                    break;
                case 411://限時召喚
                    MainManager.Instance.ShowCommonPopup("", "找不到限時召喚");
                    break;
                case 412://打開錄像館
                    break;
                case 413://打開錄像館個人紀錄
                    break;
                case 414://元素神殿
                    break;
                case 415://試煉之境
                    break;
                case 416://榮譽牆
                    break;
                case 417://成長之路
                    break;
                case 418://家園
                    break;
                case 419://公會
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.GuildScene);
                    break;
                case 420://進階歷練
                    break;
                case 421://神器精炼
                    break;
                case 422://周冠军赛
                    break;
                case 423://精灵
                    break;
                case 424://公会秘境
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.GuildScene);
                    break;
                case 425://神装祈祷界面（天界祈祷）
                    break;
                case 426://神装洗练界面
                    break;
                case 427://先知殿 转换
                    break;
                case 428://超凡段位赛
                    break;
                case 429://神装副本（天界副本）
                    break;
                case 430://组队竞技场
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.ArenaScene);
                    break;
                case 431://多人竞技场
                    GameSceneManager.Instance.AddScene(SceneConst.SceneName.ArenaScene);
                    break;
            }

            /*
        elseif evt_type == 409 then
            JumpController:getInstance():jumpViewByEvtData({20})
   
        elseif evt_type == 421 then --神器精炼
            MainuiController:getInstance():changeMainUIStatus(MainuiConst.btn_index.hallows, nil, {0, HallowsConst.Tab_Index.refine})
        elseif evt_type == 422 then --周冠军赛
            MainuiController:getInstance():changeMainUIStatus(MainuiConst.btn_index.main_scene, MainuiConst.sub_type.crosschampion)
        elseif evt_type == 423 then --精灵
            JumpController:getInstance():jumpViewByEvtData({60})
        elseif evt_type == 424 then --公会秘境
            JumpController:getInstance():jumpViewByEvtData({62})
        elseif evt_type == 425 then --神装祈祷界面（天界祈祷）
            JumpController:getInstance():jumpViewByEvtData({48})
        elseif evt_type == 426 then --神装洗练界面
            JumpController:getInstance():jumpViewByEvtData({8,BackPackConst.item_tab_type.HOLYEQUIPMENT})
        elseif evt_type == 427 then --先知殿 转换
            JumpController:getInstance():jumpViewByEvtData({24,2})
        elseif evt_type == 428 then --超凡段位赛
            JumpController:getInstance():jumpViewByEvtData({28})
        elseif evt_type == 429 then --神装副本（天界副本）
            JumpController:getInstance():jumpViewByEvtData({47})
        elseif evt_type == 430 then --组队竞技场
            JumpController:getInstance():jumpViewByEvtData({65})
        elseif evt_type == 431 then --多人竞技场
            JumpController:getInstance():jumpViewByEvtData({76})
        end*/


        }
    }
}
