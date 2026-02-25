using System.Collections;
using UnityEngine;
using YKO.Support.Expansion;
using System;
using UnityEngine.UI;
using Fungus;
using DG.Tweening;
using UniRx;
using UnityEditor.SceneManagement;

namespace YKO.SnsSetting
{
    /// <summary>
    /// Sns話題管理員
    /// 管理三個頁面的UI顯示
    /// 切換場景的zoom in out
    /// </summary>
    public partial class SnsSettingController : MonoBehaviour
    {
        /// <summary>
        /// 多語言設定(暫定
        /// </summary>
        public SettingLanguage mLanguage;
        /// <summary>
        /// 其他頁面狀態資訊(紀錄每個頁面離開後的最後狀態,以方便玩家回到頁面時,能再進入其他頁前的狀態
        /// </summary>
        public SnsInterFaceInfo interFaceInfo = new SnsInterFaceInfo();


        //必須記錄有哪些話題還未結束,有哪些話題已經完成
        #region Panel UI 
        /// <summary>
        /// Sns頁面母物件
        /// </summary>
        [SerializeField] private Transform snsPageGroupParent;
        /// <summary>
        /// 讀取加載圖片(因為sns必須等待proto及時回傳,跟以往先加載,修改靜態資料不同
        /// </summary>
        [SerializeField] private RectTransform loadingIcon;

        #endregion


        #region control and prefab
        /// <summary>
        /// 好友選單頁
        /// </summary>
        [SerializeField]
        private SnsCharaListControl charaListPage;

        /// <summary>
        /// 對話視窗
        /// 當玩家點擊對應的角色時,進入該對話框
        /// </summary>
        [SerializeField]
        private SnsChatRoomControl chatRoomPage;

        /// <summary>
        /// 話題歷史對話頁面
        /// </summary>
        [SerializeField]
        private TopicHistoryControl topicHistoryPage;

        #endregion

        #region Common Test UI
        /// <summary>
        /// (測試用)清除過去的紀錄
        /// </summary>
        [SerializeField] private Button clearRecordButton;

        #endregion

        #region Param

        public bool canTween = false;
        /// <summary>
        /// 當前顯示的頁面
        /// </summary>
        private SnsControlPage curPage;

        public SnsControlPage CurPage { get { return curPage; } }

        /// <summary>
        /// 當前顯示的角色紀錄(聊天室跟話題歷史紀錄需要使用到
        /// </summary>
        private CharaSnsRecord curDisplayCharaRecord = null;
        /// <summary>
        /// 當前的角色紀錄
        /// </summary>
        public CharaSnsRecord DisplayCharaRecord { get { return curDisplayCharaRecord; } }


        /// <summary>
        /// 選擇的顏色(鮮紅色
        /// </summary>
        public Color defaultChooseColor = new Color(0.85f, 0.17f, 0.21f);
        /// <summary>
        /// 進入計時後執行的func
        /// </summary>
        private Coroutine executeCalcTimeAction = null;

        public static readonly string defaultAvatar = "char001";
        #endregion


        /// <summary>
        /// 初始化(將資料型態轉化成charaTopicDic的狀態
        /// </summary>
        /// <returns></returns>
        public IEnumerator Init()
        {
            canTween = false;
            gameObject.SetCanvasGroup(0);
            charaListPage.gameObject.SetActive(true);
            chatRoomPage.gameObject.SetActive(true);
            topicHistoryPage.gameObject.SetActive(true);
            SnsFixedData.LoadStaticData();
            RegisterProtoHandle();
            SendProtoRequest("30200");
            yield return WaitHandleProto("30200");
            yield return new WaitUntil(() => firstLoad);
            //  yield return new WaitUntil(() => firstLoad);
            //   yield return new WaitUntil(() => proto_30200Data!=null);
            SetButtonSetting();
            SortTopicList();
            yield return charaListPage.Init(this);
            yield return TweenIn();
            canTween = true;

        }

        private void SetButtonSetting()
        {
            clearRecordButton.OnClickAsObservable().Subscribe(_ => SendProtoRequest("30290"));

        }


        /// <summary>
        /// 切換到聯絡人列表
        /// </summary>
        public IEnumerator GoCharaListPage()
        {
            canTween = false;
            StopCoroutine("WaitHandleProto");
            ShowLoading();
            charaListPage.SettingContactPersonButtonEnable(false);
            yield return charaListPage.Init(this);
            yield return SwitchSnsPage(SnsControlPage.SnsCharaList);
            charaListPage.SettingContactPersonButtonEnable(true);
            CloseLoading();
            //為聊天視窗設定聯絡人資料
            //生成對話並移動到最新對話
            //移動到聊天室窗
            canTween = true;
        }

        /// <summary>
        /// 切換到聯絡人列表(從歷史頁
        /// </summary>
        public IEnumerator TopicHistoryGoToCharaListPage()
        {
            canTween = false;
            StopCoroutine("WaitHandleProto");
            ShowLoading();
            yield return charaListPage.Init(this);
            chatRoomPage.gameObject.SetActive(false);
            var rect = snsPageGroupParent.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(-960, 0);
            charaListPage.SettingContactPersonButtonEnable(false);
            yield return SwitchSnsPage(SnsControlPage.SnsCharaList);
            charaListPage.SettingContactPersonButtonEnable(true);
            chatRoomPage.gameObject.SetActive(true);
            CloseLoading();
            //為聊天視窗設定聯絡人資料
            //生成對話並移動到最新對話
            //移動到聊天室窗
            canTween = true;
        }

        /// <summary>
        /// 切換到聯絡人聊天室
        /// </summary>
        public IEnumerator GoChatRoomPage()
        {
            canTween = false;
            StopCoroutine("WaitHandleProto");
            ShowLoading();
            charaListPage.SettingContactPersonButtonEnable(false);
            yield return chatRoomPage.Init(this);
            Debug.Log("資料加載完後,移動到聊天視窗");
            yield return SwitchSnsPage(SnsControlPage.SnsChatRoom);
            CloseLoading();
            //為聊天視窗設定聯絡人資料
            //生成對話並移動到最新對話
            //移動到聊天室窗
            canTween = true;
        }

        /// <summary>
        /// 切換到聯絡人話題列表
        /// </summary>
        public IEnumerator GoTopicHistoryPage()
        {
            canTween = false;
            StopCoroutine("WaitHandleProto");
            ShowLoading();
            yield return topicHistoryPage.Init(this);
            Debug.Log("資料加載完後,移動到話題列表");
            yield return SwitchSnsPage(SnsControlPage.SnsTopicHistory);
            CloseLoading();
            //陳列所有話題,以解鎖或沒解鎖的
            canTween = true;
        }
        /// <summary>
        /// 設置對話的角色
        /// </summary>
        /// <param name="chara"></param>
        public void SetDisplayCharaRecord(string chara)
        {
            Debug.Log("設置角色數據" + chara);
            curDisplayCharaRecord = GetCharaSnsRecordFormConversationChara(chara);
        }

        /// <summary>
        /// 橫移切換頁面
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        private IEnumerator SwitchSnsPage(SnsControlPage page)
        {

            switch (page)
            {
                case SnsControlPage.SnsCharaList:
                    yield return snsPageGroupParent.GetComponent<RectTransform>().DOAnchorPosX(0, 0.2f).WaitForCompletion();
                    break;
                case SnsControlPage.SnsChatRoom:
                    yield return snsPageGroupParent.GetComponent<RectTransform>().DOAnchorPosX(-960, 0.2f).WaitForCompletion();
                    break;
                case SnsControlPage.SnsTopicHistory:
                    yield return snsPageGroupParent.GetComponent<RectTransform>().DOAnchorPosX(-1920, 0.2f).WaitForCompletion();
                    break;

            }

        }
        /// <summary>
        /// 開始讀取
        /// </summary>
        public void ShowLoading()
        {
            executeCalcTimeAction = StartCoroutine(WaitCalcTimeExecute(
                1f,
                () => {
                    loadingIcon.parent.gameObject.SetActive(true);
                    RectTransform parentRect = loadingIcon.parent.GetComponent<RectTransform>();
                    parentRect.DOKill();
                    loadingIcon.DOKill();
                    loadingIcon.parent.gameObject.SetCanvasGroup(0);
                    loadingIcon.eulerAngles = Vector3.zero;
                    parentRect.localScale = Vector3.zero;
                    StartCoroutine(loadingIcon.parent.gameObject.eSetCanvasGroup(1));
                    parentRect.DOScale(Vector3.one, 0.1f);


                    loadingIcon.DORotate(new Vector3(0, 0, loadingIcon.eulerAngles.z + 180), 1f, RotateMode.Fast)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Incremental);


                }));

        }
        /// <summary>
        /// 關閉讀取
        /// </summary>
        public void CloseLoading()
        {
            StopCoroutine(executeCalcTimeAction);
            loadingIcon.parent.gameObject.SetActive(false);
            RectTransform parentRect = loadingIcon.parent.GetComponent<RectTransform>();
            parentRect.DOKill();
            loadingIcon.DOKill();

        }
        public IEnumerator WaitCalcTimeExecute(float duration,Action cb=null)
        {
            yield return new WaitForSeconds(duration);
            if (cb!=null)
            {
                cb();
            }
        }

        /// <summary>
        /// 動畫載入
        /// </summary>
        /// <returns></returns>
        private IEnumerator TweenIn()
        {
            gameObject.SetCanvasGroup(0);
            yield return gameObject.eSetCanvasGroup(1);
            charaListPage.SettingContactPersonButtonEnable(true);


        }

        /// <summary>
        /// 動畫淡出
        /// </summary>
        private IEnumerator TweenOut()
        {
            yield return null;
        }
        /// <summary>
        /// Sns的頁面類型
        /// </summary>
        public enum SnsControlPage
        {
            SnsCharaList,
            SnsChatRoom,
            SnsTopicHistory
        }

        /// <summary>
        /// Sns介面資訊
        /// </summary>
        public struct SnsInterFaceInfo
        {
            /// <summary>
            /// 聯絡人資訊頁 y軸位置
            /// </summary>
            public float charaListVerticalNormalizedPosition;


        }


    }// ----class SnsSettingController




}//-nameSpace



