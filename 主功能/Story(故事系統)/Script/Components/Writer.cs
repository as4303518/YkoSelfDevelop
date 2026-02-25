// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Globalization;
using System;
using YKO.Common.Data;

namespace Fungus
{
    /// <summary>
    /// Current state of the writing process.
    /// </summary>
    public enum WriterState
    {
        /// <summary> Invalid state. </summary>
        Invalid,
        /// <summary> Writer has started writing. </summary>
        Start,
        /// <summary> Writing has been paused. </summary>
        Pause,
        /// <summary> Writing has resumed after a pause. </summary>
        Resume,
        /// <summary> Writing has ended. </summary>
        End,
    }

    /// <summary>
    /// Writes text using a typewriter effect to a UI text object.
    /// </summary>
    public class Writer : MonoBehaviour, IDialogInputListener
    {
        [Tooltip("Gameobject containing a Text, Inout Field or Text Mesh object to write to")]
        [SerializeField] protected GameObject targetTextObject;

        [Tooltip("Gameobject to punch when the punch tags are displayed. If none is set, the main camera will shake instead.")]
        [SerializeField] protected GameObject punchObject;

        [Tooltip("Writing characters per second")]
        [SerializeField] protected float writingSpeed = 60;

        [Tooltip("Pause duration for punctuation characters")]
        [SerializeField] protected float punctuationPause = 0.25f;

        [Tooltip("Pause duration for punctuation characters")]
        [SerializeField] protected float AutoReadSpeed = 1f;

        [Tooltip("Color of text that has not been revealed yet")]
        [SerializeField] protected Color hiddenTextColor = new Color(1,1,1,0);

        [Tooltip("Write one word at a time rather one character at a time")]
        [SerializeField] protected bool writeWholeWords = false;

        [Tooltip("Force the target text object to use Rich Text mode so text color and alpha appears correctly")]
        [SerializeField] protected bool forceRichText = true;

        [Tooltip("Click while text is writing to finish writing immediately")]
        [SerializeField] protected bool instantComplete = true;

        [SerializeField] protected bool doReadAheadText = true;


        // This property is true when the writer is waiting for user input to continue
        protected bool isWaitingForInput;

        // This property is true when the writer is writing text or waiting (i.e. still processing tokens)
        protected bool isWriting;

        protected float currentWritingSpeed;
        protected float currentPunctuationPause;
        protected TextAdapter textAdapter = new TextAdapter();

        protected bool boldActive = false;
        protected bool italicActive = false;
        protected bool colorActive = false;
        protected string colorText = "";
        protected bool linkActive = false;
        protected string linkText = string.Empty;
        protected bool sizeActive = false;
        protected float sizeValue = 16f;
        protected bool inputFlag;
        protected bool exitFlag;
        /// <summary>
        /// 是否跳過文字逐漸產生
        /// </summary>
        private bool isSkipCreateWord=false;
        public  bool AutoPlay = false;

        //holds number of Word tokens in the currently running Write
        public int WordTokensFound { get; protected set; }

        /// <summary>
        /// Updated during writing of Word tokens, when processed tips over found, fires NotifyAllWordsWritten
        /// </summary>
        public virtual int WordTokensProcessed
        {
            get { return wordTokensProcessed; }
            protected set
            {
                if(wordTokensProcessed < WordTokensFound && value >= WordTokensFound)
                {
                    NotifyAllWordsWritten();
                }
                wordTokensProcessed = value;
            }
        }
        //holds count of number of Word tokens completed
        protected int wordTokensProcessed;

        /// <summary>
        /// Does the currently processing list of Tokens have Word Tokens that are not yet processed
        /// </summary>
        public bool HasWordsRemaining { get { return WordTokensProcessed < WordTokensFound; } }

        protected List<IWriterListener> writerListeners = new List<IWriterListener>();

        protected StringBuilder openString = new StringBuilder(256);
        protected StringBuilder closeString = new StringBuilder(256);
        protected StringBuilder leftString = new StringBuilder(1024);
        protected StringBuilder rightString = new StringBuilder(1024);
        protected StringBuilder outputString = new StringBuilder(1024);
        protected StringBuilder readAheadString = new StringBuilder(1024);

        protected string hiddenColorOpen = "";
        protected string hiddenColorClose = "";

        protected int visibleCharacterCount = 0;
        protected int readAheadStartIndex = 0;
        public WriterAudio AttachedWriterAudio { get; set; }

        private Coroutine stopCreateWordCoroutinue = null;

        private Coroutine createWordCoroutinue = null;

        protected virtual void Awake()
        {
            GameObject go = targetTextObject;
            if (go == null)
            {
                go = gameObject;
            }

            textAdapter.InitFromGameObject(go);
            
            // Cache the list of child writer listeners
            var allComponents = GetComponentsInChildren<Component>();
            for (int i = 0; i < allComponents.Length; i++)
            {
                var component = allComponents[i];
                IWriterListener writerListener = component as IWriterListener;

                if (writerListener != null)
                {   
                    writerListeners.Add(writerListener);
                }
            }

            CacheHiddenColorStrings();
        }

        protected virtual void CacheHiddenColorStrings()
        {
            // Cache the hidden color string
            Color32 c = hiddenTextColor;
            hiddenColorOpen = string.Format("<color=#{0:X2}{1:X2}{2:X2}{3:X2}>", c.r, c.g, c.b, c.a);
            hiddenColorClose = "</color>";
        }

        protected virtual void Start()
        {
            if (forceRichText)
            {
                textAdapter.ForceRichText();
            }
        }
        
        protected virtual void UpdateOpenMarkup()
        {
            openString.Length = 0;
            
            if (textAdapter.SupportsRichText())
            {
                if (sizeActive)
                {
                    openString.Append("<size=");
                    openString.Append(sizeValue);
                    openString.Append(">"); 
                }
                if (colorActive)
                {
                    openString.Append("<color=");
                    openString.Append(colorText);
                    openString.Append(">");
                }
                if (linkActive)
                {
                    openString.Append("<link=");
                    openString.Append(linkText);
                    openString.Append(">");
                }
                if (boldActive)
                {
                    openString.Append("<b>"); 
                }
                if (italicActive)
                {
                    openString.Append("<i>"); 
                }           
            }
        }
        
        protected virtual void UpdateCloseMarkup()
        {
            closeString.Length = 0;
            
            if (textAdapter.SupportsRichText())
            {
                if (italicActive)
                {
                    closeString.Append("</i>"); 
                }           
                if (boldActive)
                {
                    closeString.Append("</b>"); 
                }
                if (colorActive)
                {
                    closeString.Append("</color>");
                }
                if (linkActive)
                {
                    closeString.Append("</link>");
                }
                if (sizeActive)
                {
                    closeString.Append("</size>"); 
                }
            }
        }

        protected virtual bool CheckParamCount(List<string> paramList, int count) 
        {
            if (paramList == null)
            {
                Debug.LogError("paramList is null");
                return false;
            }
            if (paramList.Count != count)
            {
                Debug.LogError("There must be exactly " + paramList.Count + " parameters.");
                return false;
            }
            return true;
        }

        protected virtual bool TryGetSingleParam(List<string> paramList, int index, float defaultValue, out float value) 
        {
            value = defaultValue;
            if (paramList.Count > index)
            {
                float.TryParse(paramList[index], NumberStyles.Any, CultureInfo.InvariantCulture, out value);
                return true;
            }
            return false;
        }

      /*  public IEnumerator WaitForClick()//動畫的中止發生在動畫結束後撥放
        {

            yield return DoWaitForInput(true);

        }*/

      /*  public IEnumerator WaitForClickButton(RectTransform pos,Vector2 size)
        {

            var sp = InputUISupportScript.CreateButtonArea(new InputCallBack.InputOptions() { parentPos=pos,touchSize=size}, OnNextLineEvent);

            sp.GetComponent<Button>().onClick.AddListener(() => { Destroy(sp); });

            yield return DoWaitForInput(true);

            
           // inputUISupport

            //產生button
        }*/



        protected virtual IEnumerator ProcessTokens(List<TextTagToken> tokens, Sayinfo sayInfo)
        {
            // Reset control members


            boldActive = false;
            italicActive = false;
            colorActive = false;
            sizeActive = false;
            WordTokensFound = tokens.Count(x => x.type == TokenType.Words);
            WordTokensProcessed = 0;
            colorText = "";
            sizeValue = 16f;
            currentPunctuationPause = punctuationPause;
            currentWritingSpeed = writingSpeed;

            exitFlag = false;
            isWriting = true;
            TokenType previousTokenType = TokenType.Invalid;

            bool storyTextCreate = false;
            IEnumerator StopCreateWordShowCompleteSentence()
            {
                yield return DoWaitForInput(
                    true, 
                    () => {
                        if (createWordCoroutinue != null) {
                            StopCoroutine(createWordCoroutinue);
                            createWordCoroutinue = null;
                        }

                    });
                storyTextCreate = true;
                isSkipCreateWord = true;
                Debug.Log("#2這裡觸發結束");
            }
            IEnumerator CreateWord()
            {
                for (int i = 0; i < tokens.Count; ++i)
                {
                    // Pause between tokens if Paused is set
                    Debug.Log("開始判斷token for=>"+storyTextCreate);
                    while (isSkipCreateWord)
                    {
                        yield break;
                    }
                    while (Paused)
                    {
                        yield return null;//如果暫停則空轉
                    }
                    var token = tokens[i];
                    // Notify listeners about new token
                    WriterSignals.DoTextTagToken(this, token, i, tokens.Count);

                    /* for(int a=0;a<token.paramList.Count;a++)
                    {
                        Debug.Log("順位=>"+i+"裡面的參數順位=>"+a+"值=>"+token.paramList[a]);
                    }*/

                    // Update the read ahead string buffer. This contains the text for any 
                    // Word tags which are further ahead in the list. 
                    if (doReadAheadText && !textAdapter.SupportsHiddenCharacters())
                    {
                        readAheadString.Length = 0;
                        for (int j = i + 1; j < tokens.Count; ++j)
                        {

                            var readAheadToken = tokens[j];

                            if (readAheadToken.type == TokenType.Words && readAheadToken.paramList.Count == 1)
                            {
                                readAheadString.Append(readAheadToken.paramList[0]);
                            }
                            else if (readAheadToken.type == TokenType.WaitForInputAndClear)
                            {
                                break;
                            }
                        }
                    }

                    switch (token.type)
                    {
                        case TokenType.Words:

                            yield return DoWords(token.paramList, previousTokenType, sayInfo.flowchart);
                            if (sayInfo.mouthAniState == Say.MouthAnimation.WaitStoryTextComplete)
                            {
                                sayInfo.StopSayAni();
                            }
                            sayInfo.whenDialogFinish?.Invoke();
                            Debug.Log("這裡有執行嗎?");
                            WordTokensProcessed++;
                            break;

                        case TokenType.BoldStart:
                            boldActive = true;
                            break;

                        case TokenType.BoldEnd:
                            boldActive = false;
                            break;

                        case TokenType.ItalicStart:
                            italicActive = true;
                            break;

                        case TokenType.ItalicEnd:
                            italicActive = false;
                            break;

                        case TokenType.ColorStart:
                            if (CheckParamCount(token.paramList, 1))
                            {
                                colorActive = true;
                                colorText = token.paramList[0];
                            }
                            break;

                        case TokenType.ColorEnd:
                            colorActive = false;
                            break;

                        case TokenType.LinkStart:
                            if (CheckParamCount(token.paramList, 1))
                            {
                                linkActive = true;
                                linkText = token.paramList[0];
                            }
                            break;

                        case TokenType.LinkEnd:
                            linkActive = false;
                            break;

                        case TokenType.SizeStart:
                            if (TryGetSingleParam(token.paramList, 0, 16f, out sizeValue))
                            {
                                sizeActive = true;
                            }
                            break;

                        case TokenType.SizeEnd:
                            sizeActive = false;
                            break;

                        case TokenType.Wait:
                            yield return StartCoroutine(DoWait(token.paramList));
                            break;

                        case TokenType.WaitForInputNoClear://等待點擊
                           if(stopCreateWordCoroutinue!=null) StopCoroutine(stopCreateWordCoroutinue);
                            stopCreateWordCoroutinue = null;
                             yield return StartCoroutine(DoWaitForInput(false));
                            break;

                        case TokenType.WaitForInputAndClear://等待點擊
                            if (stopCreateWordCoroutinue != null) StopCoroutine(stopCreateWordCoroutinue);
                            stopCreateWordCoroutinue = null;
                            yield return StartCoroutine(DoWaitForInput(true));
                            break;

                        case TokenType.WaitForVoiceOver:
                            yield return StartCoroutine(DoWaitVO());
                            break;

                        case TokenType.WaitOnPunctuationStart:
                            TryGetSingleParam(token.paramList, 0, punctuationPause, out currentPunctuationPause);
                            break;

                        case TokenType.WaitOnPunctuationEnd:
                            currentPunctuationPause = punctuationPause;
                            break;

                        case TokenType.Clear:
                            textAdapter.Text = "";
                            break;

                        case TokenType.SpeedStart:
                            TryGetSingleParam(token.paramList, 0, writingSpeed, out currentWritingSpeed);
                            break;

                        case TokenType.SpeedEnd:
                            currentWritingSpeed = writingSpeed;
                            break;

                        case TokenType.Exit:
                            exitFlag = true;
                            break;

                        case TokenType.Message:
                            if (CheckParamCount(token.paramList, 1))
                            {
                                Flowchart.BroadcastFungusMessage(token.paramList[0]);
                            }
                            break;

                        case TokenType.VerticalPunch:
                            {
                                float vintensity;
                                float time;
                                TryGetSingleParam(token.paramList, 0, 10.0f, out vintensity);
                                TryGetSingleParam(token.paramList, 1, 0.5f, out time);
                                Punch(new Vector3(0, vintensity, 0), time);
                            }
                            break;

                        case TokenType.HorizontalPunch:
                            {
                                float hintensity;
                                float time;
                                TryGetSingleParam(token.paramList, 0, 10.0f, out hintensity);
                                TryGetSingleParam(token.paramList, 1, 0.5f, out time);
                                Punch(new Vector3(hintensity, 0, 0), time);
                            }
                            break;

                        case TokenType.Punch:
                            {
                                float intensity;
                                float time;
                                TryGetSingleParam(token.paramList, 0, 10.0f, out intensity);
                                TryGetSingleParam(token.paramList, 1, 0.5f, out time);
                                Punch(new Vector3(intensity, intensity, 0), time);
                            }
                            break;

                        case TokenType.Flash:
                            float flashDuration;
                            TryGetSingleParam(token.paramList, 0, 0.2f, out flashDuration);
                            Flash(flashDuration);
                            break;

                        case TokenType.Audio:
                            {
                                AudioSource audioSource = null;
                                if (CheckParamCount(token.paramList, 1))
                                {
                                    audioSource = FindAudio(token.paramList[0]);
                                }
                                if (audioSource != null)
                                {
                                    audioSource.PlayOneShot(audioSource.clip);
                                }
                            }
                            break;

                        case TokenType.AudioLoop:
                            {
                                AudioSource audioSource = null;
                                if (CheckParamCount(token.paramList, 1))
                                {
                                    audioSource = FindAudio(token.paramList[0]);
                                }
                                if (audioSource != null)
                                {
                                    audioSource.Play();
                                    audioSource.loop = true;
                                }
                            }
                            break;

                        case TokenType.AudioPause:
                            {
                                AudioSource audioSource = null;
                                if (CheckParamCount(token.paramList, 1))
                                {
                                    audioSource = FindAudio(token.paramList[0]);
                                }
                                if (audioSource != null)
                                {
                                    audioSource.Pause();
                                }
                            }
                            break;

                        case TokenType.AudioStop:
                            {
                                AudioSource audioSource = null;
                                if (CheckParamCount(token.paramList, 1))
                                {
                                    audioSource = FindAudio(token.paramList[0]);
                                }
                                if (audioSource != null)
                                {
                                    audioSource.Stop();
                                }
                            }
                            break;
                    }

                    previousTokenType = token.type;

                    if (exitFlag)
                    {
                        break;
                    }
                }
                storyTextCreate = true;
            }
            
             stopCreateWordCoroutinue=StartCoroutine(StopCreateWordShowCompleteSentence());
             createWordCoroutinue = StartCoroutine(CreateWord());
            yield return new WaitUntil(() => storyTextCreate);
            //如果有跳過產生文字,直接顯示完整文字的過程
            if (isSkipCreateWord)
            {
                isSkipCreateWord = false;
                var token = tokens.First(v => v.type == TokenType.Words);
                textAdapter.Text = token.paramList[0].Replace("\\n", "\n");
                if (sayInfo.mouthAniState == Say.MouthAnimation.WaitStoryTextComplete)
                {
                    sayInfo.StopSayAni();
                }
                sayInfo.whenDialogFinish?.Invoke();
                var waitToken= tokens.First(v => v.type == TokenType.WaitForInputNoClear|| v.type == TokenType.WaitForInputAndClear);
                switch (waitToken.type) 
                {
                    case TokenType.WaitForInputNoClear://等待點擊
                        yield return DoWaitForInput(false);
                        break;

                    case TokenType.WaitForInputAndClear://等待點擊
                        yield return DoWaitForInput(true);
                        break;
                }
            }


            var needToken = tokens.First(v => v.type == TokenType.Words);
            string normalString = "";
            foreach (var param in needToken.paramList)
            {
                normalString += param.ToString();
            }
            sayInfo.setLogDialog(GetComponent<SayDialog>().NameText, normalString);


            inputFlag = false;
            exitFlag = false;
            isWaitingForInput = false;
            isWriting = false;

            NotifyEnd(sayInfo.stopVoiceover);

        }

        protected virtual IEnumerator DoWords(List<string> paramList, TokenType previousTokenType,Flowchart flowchart)
        {
            if (!CheckParamCount(paramList, 1))
            {
                yield break;
            }

            string param = paramList[0].Replace("\\n", "\n");


            // Trim whitespace after a {wc} or {c} tag
            if (previousTokenType == TokenType.WaitForInputAndClear ||
                previousTokenType == TokenType.Clear)
            {
                param = param.TrimStart(' ', '\t', '\r', '\n');
            }

            // Start with the visible portion of any existing displayed text.
            string startText = "";
            if (visibleCharacterCount > 0 &&
                visibleCharacterCount <= textAdapter.Text.Length)
            {
                startText = textAdapter.Text.Substring(0, visibleCharacterCount);
            }
                
            UpdateOpenMarkup();
            UpdateCloseMarkup();

            float timeAccumulator = Time.deltaTime;
            float invWritingSpeed = 1f / currentWritingSpeed;

            //refactor this, its mostly the same 30 lines of code
            if (textAdapter.SupportsHiddenCharacters())
            {
                Debug.Log("跑上面");
                //pausing for 1 frame means we can get better first data, but is conflicting with animation ?
                //  or is it something else inserting the color alpha invis 
                yield return null;
                //this works for first thing being shown but then no subsequent, as the char counts have not been update
                // by tmpro after the set to ""
                var startingReveal = Mathf.Min(readAheadStartIndex, textAdapter.CharactersToReveal);
                PartitionString(writeWholeWords, param, param.Length + 1);

                ConcatenateString(startText);//連接文字
                textAdapter.Text = outputString.ToString();

                NotifyGlyph();
                textAdapter.RevealedCharacters = startingReveal;
                yield return null;

                while (textAdapter.RevealedCharacters < Mathf.Min(readAheadStartIndex, textAdapter.CharactersToReveal))
                {
                    // No delay if user has clicked and Instant Complete is enabled
                    if (instantComplete && inputFlag)
                    {
                        textAdapter.RevealedCharacters = textAdapter.CharactersToReveal;
                    }

                    textAdapter.RevealedCharacters++;

                    NotifyGlyph();

                    // Punctuation pause
                    if (IsPunctuation(textAdapter.LastRevealedCharacter))
                    {
                        yield return StartCoroutine(DoWait(currentPunctuationPause));
                    }
                    
                    if (currentWritingSpeed > 0f)//
                    {
                        timeAccumulator -= invWritingSpeed;//
                        if (timeAccumulator <= 0f)
                        {
                            var waitTime = Mathf.Max(invWritingSpeed, Time.deltaTime);
                            
                            yield return new WaitForSeconds(waitTime); 
                            timeAccumulator += waitTime;
                        }
                    }
                }
            }
            else
            {
                Debug.Log("跑下面");

                for (int i = 0; i < param.Length + 1; ++i)
                {   
                    if (exitFlag||(flowchart!=null&&flowchart.mStoryControl.isSkipPlay))
                    {
                        break;
                    }

                    // Pause mid sentence if Paused is set
                    while (Paused)
                    {
                        yield return null;
                    }

                    PartitionString(writeWholeWords, param, i);
                    ConcatenateString(startText);

                    //Debug.Log("output=>" + outputString.ToString());

                    textAdapter.Text = outputString.ToString();
                    NotifyGlyph();

                    // No delay if user has clicked and Instant Complete is enabled
                    if (instantComplete && inputFlag)
                    {
                        continue;
                    }

                    // Punctuation pause
                    if (leftString.Length > 0 &&
                        rightString.Length > 0 &&
                        IsPunctuation(leftString.ToString(leftString.Length - 1, 1)[0]))
                    {
                        //timeAccumulator -= currentPunctuationPause; ???

                        //#!等待點點點的時間
                      //  yield return StartCoroutine(DoWait(currentPunctuationPause));  
                    }

                    // Delay between characters
                    if (currentWritingSpeed > 0f)
                    {
                        timeAccumulator -= invWritingSpeed;

                        if (timeAccumulator <= 0f)
                        {
                            var waitTime = Mathf.Max(invWritingSpeed, Time.deltaTime);
                            yield return new WaitForSeconds(waitTime);
                            timeAccumulator += waitTime;
                        }

                    }

                }//--- for
                
            }

          //  Debug.Log("最後=>" +param);

           // textAdapter.Text = param;

        }

        protected virtual void PartitionString(bool wholeWords, string inputString, int i)
        {
            leftString.Length = 0;
            rightString.Length = 0;

            // Reached last character
            leftString.Append(inputString);
            if (i >= inputString.Length)
            {
                return;
            }

            rightString.Append(inputString);

            if (wholeWords)
            {
                // Look ahead to find next whitespace or end of string
                for (int j = i; j < inputString.Length + 1; ++j)
                {
                    if (j == inputString.Length || char.IsWhiteSpace(inputString[j]))
                    {
                        leftString.Length = j;
                        rightString.Remove(0, j);
                        break;
                    }
                }
            }
            else
            {
                leftString.Remove(i, inputString.Length - i);
                rightString.Remove(0, i);
            }
        }
        /// <summary>
        /// 連接文字
        /// </summary>
        /// <param name="startText"></param>
        protected virtual void ConcatenateString(string startText)
        {
            outputString.Length = 0;
            readAheadStartIndex = int.MaxValue;

            // string tempText = startText + openText + leftText + closeText;
            outputString.Append(startText);
            outputString.Append(openString);
            outputString.Append(leftString);
            outputString.Append(closeString);
            

            // Track how many visible characters are currently displayed so
            // we can easily extract the visible portion again later.   
            visibleCharacterCount = outputString.Length;
            
            // Make right hand side text hidden
            if (textAdapter.SupportsRichText() &&
                rightString.Length + readAheadString.Length > 0)
            {
                // Ensure the hidden color strings are populated
                if (hiddenColorOpen.Length == 0)
                {
                    CacheHiddenColorStrings();
                }

                readAheadStartIndex = outputString.Length;

                outputString.Append(hiddenColorOpen);
                outputString.Append(rightString);
                outputString.Append(readAheadString);
                outputString.Append(hiddenColorClose);
            }
        }

        protected virtual IEnumerator DoWait(List<string> paramList)
        {
            var param = "";
            if (paramList.Count == 1)
            {
                param = paramList[0];
            }

            float duration = 1f;
            if (!float.TryParse(param, NumberStyles.Any, CultureInfo.InvariantCulture, out duration))
            {
                duration = 1f;
            }

            yield return StartCoroutine( DoWait(duration) );
        }

        protected virtual IEnumerator DoWaitVO()
        {
            float duration = 0f;

            if (AttachedWriterAudio != null)
            {
                duration = AttachedWriterAudio.GetSecondsRemaining();
            }

            yield return StartCoroutine(DoWait(duration));
        }

        protected virtual IEnumerator DoWait(float duration)
        {
            NotifyPause();

            float timeRemaining = duration;
            while (timeRemaining > 0f && !exitFlag)
            {
                if (instantComplete && inputFlag)
                {
                    break;
                }

                timeRemaining -= Time.deltaTime;
                yield return null;
            }

            NotifyResume();
        }

        public virtual IEnumerator DoWaitForInput(bool clear=true,Action closeWordCoro=null)//等待點擊
        {
            NotifyPause();

            inputFlag = false;
            isWaitingForInput = true;
            StartCoroutine(GetComponent<DialogInput>().OpenDetectInput());
            float calcStart = AutoReadSpeed;
            Debug.Log("測試0");
            while (!inputFlag && !exitFlag)
            {
                    if (AutoPlay) {
                        calcStart -= Time.deltaTime;
                        if (calcStart<0)
                        {
                            yield break;
                        }
                    }
                    yield return null;
            }
            closeWordCoro?.Invoke();
            Debug.Log("測試1");
            yield return null;
            isWaitingForInput = false;          
            inputFlag = false;

            if (clear)
            {
                NotifyEnd(false);
                textAdapter.Text = "";
            }
            Debug.Log("測試2");
            NotifyResume();
        }
        /// <summary>
        /// 點擊直接產生完整句子
        /// </summary>
        /// <param name="clear"></param>
        /// <returns></returns>
        protected virtual IEnumerator DoWaitForInput()//等待點擊
        {
            NotifyPause();

            inputFlag = false;
            isWaitingForInput = true;
            StartCoroutine(GetComponent<DialogInput>().OpenDetectInput());
            float calcStart = AutoReadSpeed;

            while (!inputFlag && !exitFlag)
            {
                yield return null;
            }

            yield return null;
            isWaitingForInput = false;
            inputFlag = false;


            NotifyEnd(false);
            NotifyResume();
        }


        protected virtual bool IsPunctuation(char character)
        {
            return character == '.' || 
                character == '?' ||  
                    character == '!' || 
                    character == ',' ||
                    character == ':' ||
                    character == ';' ||
                    character == ')';
        }
        
        protected virtual void Punch(Vector3 axis, float time)
        {
            GameObject go = punchObject;
            if (go == null)
            {
                go = Camera.main.gameObject;
            }

            if (go != null)
            {
                iTween.ShakePosition(go, axis, time);
            }
        }
        
        protected virtual void Flash(float duration)
        {
            var cameraManager = FungusManager.Instance.CameraManager;

            cameraManager.ScreenFadeTexture = CameraManager.CreateColorTexture(new Color(1f,1f,1f,1f), 32, 32);
            cameraManager.Fade(1f, duration, delegate {
                cameraManager.ScreenFadeTexture = CameraManager.CreateColorTexture(new Color(1f,1f,1f,1f), 32, 32);
                cameraManager.Fade(0f, duration, null);
            });
        }
        
        protected virtual AudioSource FindAudio(string audioObjectName)
        {
            GameObject go = GameObject.Find(audioObjectName);
            if (go == null)
            {
                return null;
            }
            
            return go.GetComponent<AudioSource>();
        }

        protected virtual void NotifyInput()
        {
            WriterSignals.DoWriterInput(this);

            for (int i = 0; i < writerListeners.Count; i++)
            {
                var writerListener = writerListeners[i];
                writerListener.OnInput();//打字音特效
            }
        }

        protected virtual void NotifyStart(AudioClip audioClip)
        {
            WriterSignals.DoWriterState(this, WriterState.Start);

            for (int i = 0; i < writerListeners.Count; i++)
            {
                var writerListener = writerListeners[i];
                writerListener.OnStart(audioClip);
            }
        }

        protected virtual void NotifyPause()
        {
            WriterSignals.DoWriterState(this, WriterState.Pause);

            for (int i = 0; i < writerListeners.Count; i++)
            {
                var writerListener = writerListeners[i];
                writerListener.OnPause();
            }
        }

        protected virtual void NotifyResume()
        {
            WriterSignals.DoWriterState(this, WriterState.Resume);

            for (int i = 0; i < writerListeners.Count; i++)
            {
                var writerListener = writerListeners[i];
                writerListener.OnResume();
            }
        }

        protected virtual void NotifyAllWordsWritten()
        {
            for (int i = 0; i < writerListeners.Count; i++)
            {
                var writerListener = writerListeners[i];
                writerListener.OnAllWordsWritten();
            }
        }

        protected virtual void NotifyEnd(bool stopAudio)
        {
            WriterSignals.DoWriterState(this, WriterState.End);

            for (int i = 0; i < writerListeners.Count; i++)
            {
                var writerListener = writerListeners[i];
                writerListener.OnEnd(stopAudio);
            }
        }

        protected virtual void NotifyGlyph()
        {
            WriterSignals.DoWriterGlyph(this); 

            for (int i = 0; i < writerListeners.Count; i++)
            {
                var writerListener = writerListeners[i];
                writerListener.OnGlyph();
            }
        }

        #region Public members

        /// <summary>
        /// This property is true when the writer is writing text or waiting (i.e. still processing tokens).
        /// </summary>
        public virtual bool IsWriting { get { return isWriting; } }

        /// <summary>
        /// This property is true when the writer is waiting for user input to continue.
        /// </summary>
        public virtual bool IsWaitingForInput { get { return isWaitingForInput; } }

        /// <summary>
        /// Pauses the writer.
        /// </summary>
        public virtual bool Paused { set; get; }

        /// <summary>
        /// Stop writing text.
        /// </summary>
        public virtual void Stop()
        {
            if (isWriting || isWaitingForInput)
            {
                exitFlag = true;
            }
        }

        /// <summary>
        /// Writes text using a typewriter effect to a UI text object.
        /// </summary>
        /// <param name="content">Text to be written</param>
        /// <param name="clear">If true clears the previous text.</param>
        /// <param name="waitForInput">Writes the text and then waits for player input before calling onComplete.</param>
        /// <param name="stopAudio">Stops any currently playing audioclip.</param>
        /// <param name="waitForVO">Wait for the Voice over to complete before proceeding</param>
        /// <param name="audioClip">Audio clip to play when text starts writing.</param>
        /// <param name="onComplete">Callback to call when writing is finished.</param>
        /// string content, bool clear, bool waitForInput, bool stopAudio, bool waitForVO, AudioClip audioClip, System.Action onComplete

        public virtual IEnumerator Write(Sayinfo sayinfo)
        {// 根據tag
            if (sayinfo.clearPrevious)
            {
                textAdapter.Text = "";
                visibleCharacterCount = 0;
            }

            if (!textAdapter.HasTextObject())
            {
                yield break;
            }
            //AutoPlay=sayinfo.isAutoPlayFromDefaultSetting;
            // If this clip is null then WriterAudio will play the default sound effect (if any)

            //NotifyStart(sayinfo.voiceOverClip);
            

            string tokenText = TextVariationHandler.SelectVariations(sayinfo.content);
            if (sayinfo.waitForVO)//等待音效
            {
                tokenText += "{wvo}";
            }

            if (sayinfo.waitForClick)//等待點擊
            {
                tokenText += "{wi}";
            }



            List<TextTagToken> tokens = TextTagParser.Tokenize(tokenText);//獲得相應的功能 tag 

            gameObject.SetActive(true);

            yield return StartCoroutine(ProcessTokens(tokens, sayinfo));
        }

        public void SetTextColor(Color textColor)
        {
            textAdapter.SetTextColor(textColor);
        }

        public void SetTextAlpha(float textAlpha)
        {
            textAdapter.SetTextAlpha(textAlpha);
        }



        #endregion

        #region IDialogInputListener implementation

        public virtual void OnNextLineEvent()
        {
            

            if (isWriting || isWaitingForInput)
            {
                inputFlag = true;
                NotifyInput();
            }
        }

        #endregion
    }
}
