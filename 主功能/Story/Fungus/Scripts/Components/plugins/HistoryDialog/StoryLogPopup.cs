using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UniRx;

namespace Fungus
{
    public class StoryLogPopup : MonoBehaviour
    {
        public GameObject LogCellContentParent = null;

        public GameObject LogCellPrefab = null;

        public Button btnClose;

        public IEnumerator Init(List<DialogInfo> dia, Action closeButtonCb)
        {
            yield return LoadPrefabs();

            SetPrefabsPivot(dia);
            CreateCell(dia);

            btnClose.OnClickAsObservable().Subscribe(_ =>
            {
                //Debug.Log("關閉歷史對話");
                closeButtonCb();
            }).AddTo(this);

            //LayoutRebuilder.ForceRebuildLayoutImmediate(ChildTextRect);
        }

        private void SetPrefabsPivot(List<DialogInfo> dia)
        {
            if (dia.Count*300 > (transform.parent.parent.GetComponent<RectTransform>().sizeDelta.y-300))
            {
                LogCellContentParent.GetComponent<RectTransform>().pivot = Vector3.zero;
            }
        }

        public void CreateCell(List<DialogInfo> dia)
        {
            if (LogCellPrefab == null)
            {
                return;
            }

            GameObject sp = null;

            foreach (DialogInfo info in dia)
            {
                sp = Instantiate(LogCellPrefab);
                sp.transform.SetParent(LogCellContentParent.transform,false);

                StartCoroutine( sp.GetComponent<DialogHistoryCell>().Init(info));
            }

            if (dia.Count == 1) // 會有bug必須重新開啟物件才會正常
            {
                sp.gameObject.SetActive(false);
                sp.gameObject.SetActive(true);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(LogCellContentParent.GetComponent<RectTransform>());
        }

        private IEnumerator LoadPrefabs()
        {
            ResourceRequest resRe = Resources.LoadAsync<GameObject>("Prefabs/PenguinPrefab/DialogHistoryCell");
            yield return new WaitUntil(() => resRe.isDone);
            LogCellPrefab = resRe.asset as GameObject;
        }
    }
}

