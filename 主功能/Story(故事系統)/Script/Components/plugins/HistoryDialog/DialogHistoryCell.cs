using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




namespace Fungus {

    public class DialogHistoryCell : MonoBehaviour
    {

        [SerializeField] private Text ContentText;
        [SerializeField] private Text CharaNameText;
        [SerializeField] private Image NameTab;
        [SerializeField] private Button PlayAudioButton;
        private DialogInfo mData;
        [SerializeField] private AudioSource aAudioSource;
        [SerializeField] private Sprite PlayAudioImg;
        [SerializeField] private Sprite UnPlayAudioImg;

        public IEnumerator Init(DialogInfo aData)
        {
            mData = aData;
            UpdateUI();
            SetAudioSource();
            SetButton();
            yield return null;


        }


        private void UpdateUI()
        {
            CharaNameText.text = mData.CharaName;
            ContentText.text = mData.DialogContent;
            NameTab.color = mData.aColor;
            NameTab.sprite = mData.nameImageBg;

        }

        private void SetButton() {

            PlayAudioButton.onClick.AddListener(() => {
                if (mData.aAudioClip==null) {
                              return;
                   }
                Image img = PlayAudioButton.GetComponent<Image>();
                if (aAudioSource.isPlaying) {

                    aAudioSource.Stop();
                    img.sprite = UnPlayAudioImg;

                }
                else
                {
                    aAudioSource.Play();
                    img.sprite = PlayAudioImg;
                    IEnumerator onComplete (){
                        yield return new WaitForSeconds(aAudioSource.clip.length);
                        img.sprite = UnPlayAudioImg;
                    }
                    StartCoroutine(onComplete());
                    
                }


               
            });
        
        }

        private void SetAudioSource()
        {
            aAudioSource.clip = mData.aAudioClip;


        }

        // Start is called before the first frame update
     
    }
}