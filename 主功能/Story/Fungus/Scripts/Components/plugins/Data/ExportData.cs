#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEngine.UI;
using Newtonsoft.Json;
//using Unity.

namespace Fungus
{


    [Serializable]
    public class ExportData
    {
        public static BindingFlags DefaultBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        public FlowChartSaveData flowChartSaveData;
        public StageSaveData stageSaveData;


        public ExportData(Flowchart flowchart)
        {
            stageSaveData = new StageSaveData(flowchart.mStage);
            flowChartSaveData = new FlowChartSaveData(flowchart);

        }

        public ExportData() { }

        public IEnumerator SetDataInfoToGame(Flowchart flowchart)
        {
            stageSaveData.SetDataToClass(flowchart.mStage);
            yield return flowChartSaveData.DataSetToClass(flowchart);

        }


    }


    [Serializable]
    public class FlowChartSaveData
    {
        public List<BlockSaveData> blockSaveDataList = new List<BlockSaveData>();

        public List<VariableData> variables = new List<VariableData>();

        public string description = "";

        public string dataName = "";

        public bool colorCommands = true;

        public bool hideComponents = true;

        public float stepPause = 0f;

        public bool saveSelection = true;

        public string localizationId = "";

        public bool showLineNumbers = false;

        public List<string> hideCommands = new List<string>();

        //public LuaEnvironment luaEnvironment;

        public string luaBindingName = "flowchart";

        public FlowChartSaveData(Flowchart flowchart)
        {

            description = flowchart.Description;
            dataName = flowchart.DataName;
            colorCommands = flowchart.ColorCommands;
            hideComponents = flowchart.HideComponents;
            stepPause = flowchart.StepPause;
            saveSelection = flowchart.SaveSelection;
            localizationId = flowchart.LocalizationId;
            showLineNumbers = flowchart.ShowLineNumbers;
            hideCommands = new List<string>(flowchart.HideCommands);
            luaBindingName = flowchart.LuaBindingName;

            variables.Clear();
            blockSaveDataList.Clear();

            foreach (var variable in flowchart.Variables)
            {
                variables.Add(new VariableData(variable));
            }

            foreach (var data in flowchart.GetComponents<Block>())
            {
                blockSaveDataList.Add(new BlockSaveData(data));
            }
        }

        public IEnumerator DataSetToClass(Flowchart flowchart)
        {
            flowchart.SetOverrideData(this);

            foreach (var blockData in blockSaveDataList)
            {
                var block = flowchart.gameObject.AddComponent<Block>();
                blockData.SetDataToBlock(block);
            }

            int highfinish = 0;
            int midfinish = 0;

            List<BlockSaveData> highBlockData = new List<BlockSaveData>();
            List<BlockSaveData> midBlockData = new List<BlockSaveData>();
            List<BlockSaveData> LowBlockData = new List<BlockSaveData>();



            foreach (var block in highBlockData)
            {
#if UNITY_EDITOR
                EditorCoroutineUtility.StartCoroutineOwnerless(block.SetBlockCommandData(() => { highfinish++; }));
#endif
            }

            yield return new WaitUntil(() => highfinish >= blockSaveDataList.Count);


            foreach (var block in midBlockData)
            {
#if UNITY_EDITOR
                EditorCoroutineUtility.StartCoroutineOwnerless(block.SetBlockCommandData(() => { midfinish++; }));
#endif
            }

            IEnumerator ExecuteLow()
            {
                yield return new WaitUntil(() => midfinish >= blockSaveDataList.Count);

                foreach (var block in LowBlockData)
                {
#if UNITY_EDITOR
                    EditorCoroutineUtility.StartCoroutineOwnerless(block.SetBlockCommandData(() => { }));
#endif
                }
            }

#if UNITY_EDITOR
            EditorCoroutineUtility.StartCoroutineOwnerless(ExecuteLow());
#endif







            //無法賦值 因為未找到set value的方法


        }
    }

    [Serializable]
    public class BlockSaveData
    {
        public string blockName = "New Block";
        public List<CommandSaveData> commandSaveDataList;

        public bool eventIsNull = false;

        public EventHandleSaveData eventSaveData = null;

        public string description = "";

        public ExecutionState executionState;

        public bool useCustomTint = false;

        public Color tint = Color.white;

        public Rect nodeRect = new Rect();

        public Block tempBlock = null;//用意是為了讓command陣列等所有block初始化完成後在執行

        public BlockSaveData(Block block)
        {
            Debug.Log("execute serializable block name=>" + block.BlockName);
            blockName = block.BlockName;
            description = block.Description;
            executionState = block.State;

            useCustomTint = block.UseCustomTint;
            nodeRect = block._NodeRect;
            tint = block.Tint;

            bool isHave = false;
            if (block._EventHandler)
            {
                if (!block._EventHandler.Equals(null) && block._EventHandler != null)
                {
                    isHave = true;
                    eventSaveData = new EventHandleSaveData(block._EventHandler);
                }
            }

            if (!isHave)
            {
                eventSaveData = null;
                eventIsNull = true;
            }
            commandSaveDataList = new List<CommandSaveData>();

            foreach (var com in block.CommandList)
            {

                CommandSaveData sData = new CommandSaveData(com);

                if (sData != null)
                {
                    commandSaveDataList.Add(sData);
                }
            }
        }

        public void SetDataToBlock(Block block)
        {
            block.BlockName = blockName;
            block.Description = description;
            block._NodeRect = nodeRect;
            block.State = executionState;
            block.UseCustomTint = useCustomTint;
            block.Tint = tint;


            if (!eventIsNull)
            {
                SetEventDataToBlock(block);
            }
            else
            {
                eventSaveData = null;
                block._EventHandler = null;
            }
            tempBlock = block;

        }

        private void SetEventDataToBlock(Block block)//給予Block數據
        {
            EventHandler newHandler = block.gameObject.AddComponent(Type.GetType(eventSaveData.typeName)) as EventHandler;

            newHandler.ParentBlock = block;


            for (int i = 0; i < eventSaveData.propertyValues.Count; i++)
            {

                var property = Type.GetType(eventSaveData.typeName).GetFields()[i];

#if UNITY_EDITOR
                EditorCoroutineUtility.StartCoroutine(eventSaveData.propertyValues[i].GetValueData(
                    block.GetComponent<Flowchart>(),
                    res =>
                    {
                        property.SetValue(newHandler, res);
                    }),
                    this
                    );
#endif
            }

            block._EventHandler = newHandler;
        }

        public IEnumerator SetBlockCommandData(Action cb = null)
        {
            if (tempBlock == null)
            {
                Debug.Log("發生錯誤,未加載所有block便加載command");
            }
            foreach (var comSaveData in commandSaveDataList)
            {
                yield return SetSaveData(comSaveData, tempBlock);
            }
            if (cb != null)
            {
                cb();
            }
        }

        private IEnumerator SetSaveData(CommandSaveData saveData, Block block)//設置儲存資料
        {
            Flowchart flowchart = block.gameObject.GetComponent<Flowchart>();
            Debug.Log("command類型名稱=>" + saveData.commandType);
            var type = Type.GetType(saveData.commandType);
            var component = block.gameObject.AddComponent(type) as Command;

            block.CommandList.Add(component);
            component.ParentBlock = block;
            List<string> strlist = new List<string>();

            for (int i = 0; i < type.GetFields(ExportData.DefaultBindingFlags).Length; i++)
            {

                var field = type.GetFields(ExportData.DefaultBindingFlags)[i];

                Debug.Log("目前在獲取的field值名稱==========>" + field.Name);

                if (field.FieldType.IsGenericType)
                {
                    if (field.FieldType == typeof(List<string>))//泛型無法輸入字串type list<>只能接受有泛型的類別 也不接受回傳ilist跟icollect  所以只能很白癡的一一羅列
                    {

                        List<string> list = new List<string>();
                        yield return saveData.fieldDataList[i].GetValueData(flowchart, obj =>
                        {

                            list = (obj as string[]).ToList();

                        });
                        field.SetValue(component, list);

                    }
                    else if (field.FieldType == typeof(List<List<string>>))
                    {
                        List<List<string>> list = new List<List<string>>();

                        list = null;
                        yield return saveData.fieldDataList[i].GetValueData(flowchart, obj =>
                        {

                            list = (obj as List<string>[]).ToList();

                        });
                        field.SetValue(component, list);

                    }
                    else if (field.FieldType == typeof(List<int>))
                    {
                        List<int> list = new List<int>();
                        yield return saveData.fieldDataList[i].GetValueData(flowchart, obj =>
                        {

                            list = (obj as int[]).ToList();

                        });
                        field.SetValue(component, list);

                    }
                    else if (field.FieldType == typeof(List<float>))
                    {
                        List<float> list = new List<float>();
                        yield return saveData.fieldDataList[i].GetValueData(flowchart, obj =>
                        {

                            list = (obj as float[]).ToList();

                        });
                        field.SetValue(component, list);

                    }
                    else if (field.FieldType == typeof(List<bool>))
                    {
                        List<bool> list = new List<bool>();
                        yield return saveData.fieldDataList[i].GetValueData(flowchart, obj =>
                        {

                            list = (obj as bool[]).ToList();

                        });
                        field.SetValue(component, list);

                    }
                    else if (field.FieldType == typeof(List<object>))
                    {
                        List<object> list = new List<object>();
                        yield return saveData.fieldDataList[i].GetValueData(flowchart, obj =>
                        {

                            list = (obj as object[]).ToList();

                        });
                        field.SetValue(component, list);

                    }
                    else if (field.FieldType == typeof(List<Variable>))
                    {
                        List<Variable> list = new List<Variable>();
                        yield return saveData.fieldDataList[i].GetValueData(flowchart, obj =>
                        {

                            list = (obj as Variable[]).ToList();

                        });
                        field.SetValue(component, list);
                    }
                    else if (field.FieldType == typeof(List<Image>))
                    {
                        List<Image> list = new List<Image>();
                        yield return saveData.fieldDataList[i].GetValueData(flowchart, obj =>
                        {

                            list = (obj as Image[]).ToList();

                        });


                        field.SetValue(component, list);
                    }
                    else if (field.FieldType == typeof(List<CharaSnsSetting>))
                    {
                        List<CharaSnsSetting> list = new List<CharaSnsSetting>();
                        yield return saveData.fieldDataList[i].GetValueData(flowchart, obj =>
                        {

                            list = (obj as CharaSnsSetting[]).ToList();

                        });
                        Debug.Log("角色數量=>" + list.Count);


                        field.SetValue(component, list);
                    }
                    else if (field.FieldType == typeof(List<SnsMessage>))
                    {
                        List<SnsMessage> list = new List<SnsMessage>();
                        yield return saveData.fieldDataList[i].GetValueData(flowchart, obj =>
                        {

                            list = (obj as SnsMessage[]).ToList();

                        });
                        Debug.Log("訊息數量=>" + list.Count);

                        field.SetValue(component, list);
                    }

                    else if (field.FieldType == typeof(ReplyAnswer[]))
                    {
                        ReplyAnswer[] list = new ReplyAnswer[0];
                        yield return saveData.fieldDataList[i].GetValueData(flowchart, obj =>
                        {

                            list = (obj as ReplyAnswer[]);

                        });

                        Debug.Log("ReplyAnswer數量=>" + list.Length);

                        field.SetValue(component, list);
                    }
                    else if (field.FieldType == typeof(List<GameObject>))
                    {
                        List<GameObject> list = new List<GameObject>();
                        yield return saveData.fieldDataList[i].GetValueData(flowchart, obj =>
                        {

                            list = (obj as GameObject[]).ToList();

                        });

                        field.SetValue(component, list);
                    }
                    else
                    {
                        Debug.Log("未知的類型=>" + field.FieldType);
                        List<object> list = new List<object>();
                        yield return saveData.fieldDataList[i].GetValueData(flowchart, obj =>
                        {
                            list = (obj as object[]).ToList();
                        });
                        field.SetValue(component, list);
                    }


                }
                else if (field.FieldType.IsEnum)
                {
                    yield return saveData.fieldDataList[i].GetValueData(flowchart, res =>
                    {
                        Debug.Log("enum獲得的值=>" + res);
                        field.SetValue(component, Enum.Parse(field.FieldType, (string)res));
                    });
                }
                else if (field.FieldType.IsValueType || field.FieldType.BaseType.IsValueType || field.FieldType.BaseType == typeof(string))
                {

                    string fieldTypeStr = field.FieldType.ToString();


                    yield return saveData.fieldDataList[i].GetValueData(flowchart, res =>
                    {
                        field.SetValue(component, res);
                    });

                }
                else // 非 list
                {

                    if (field.FieldType == typeof(Camera))
                    {
                        field.SetValue(component, Camera.main);
                        yield break;
                    }

                    yield return saveData.fieldDataList[i].GetValueData(flowchart, res =>
                    {

                        if (field.FieldType == typeof(Block))
                        {
                            field.SetValue(component, (res as Block));
                        }

                        else if (field.FieldType == typeof(Stage))
                        {
                            field.SetValue(component, (res as Stage));
                        }
                        else if (field.FieldType == typeof(RectTransform))
                        {
                            field.SetValue(component, (res as RectTransform));
                        }
                        else if (field.FieldType == typeof(GameObject))
                        {
                            field.SetValue(component, (res as GameObject));
                        }
                        else if (field.FieldType == typeof(Character))
                        {
                            field.SetValue(component, (res as Character));
                        }
                        else if (field.FieldType == typeof(View))
                        {
                            field.SetValue(component, (res as View));
                        }
                        else if (field.FieldType == typeof(SpriteRenderer))
                        {
                            field.SetValue(component, (res as SpriteRenderer));
                        }
                        else if (field.FieldType == typeof(Image))
                        {
                            field.SetValue(component, (res as Image));
                        }
                        else if (field.FieldType == typeof(Sprite)) //say
                        {
                            field.SetValue(component, (res as Sprite));
                            // component.SetSaveDataToValue(field.Name, res);
                            //sprite因為每個command獲取的目標不同,故需要去該command底下撰寫獲取後的執行方法
                            //根據值的名去做銜接 ex component.setValue(field.Name,res);
                        }
                        else if (field.FieldType == typeof(AudioClip))//say show audio
                        {
                            field.SetValue(component, (res as AudioClip));
                        }
                        else if (field.FieldType == typeof(Texture))//say show audio
                        {
                            field.SetValue(component, (res as Texture));
                        }
                        else if (field.FieldType == typeof(Texture2D))//say show audio
                        {
                            field.SetValue(component, (res as Texture2D));
                        }
                        else if (field.FieldType == typeof(SnsMessage))//say show audio
                        {
                            field.SetValue(component, (res as SnsMessage));
                        }
                        else  // sturct的值
                        {
                            Debug.Log("最後捕捉的值=>" + res);
                            Debug.Log("進到預設的欄位=>" + field.FieldType.Name);
                            field.SetValue(component, res);
                        }

                    });

                }

            }

        }
    }

    [Serializable]
    public class EventHandleSaveData
    {
        public string typeName;
        public List<DataObjectValue> propertyValues = new List<DataObjectValue>();//根據不同類別的儲存數據,給予不同的陣列


        public EventHandleSaveData(EventHandler eventHandler)
        {
            if (eventHandler == null)
            {
                return;
            }

            typeName = eventHandler.GetType().FullName;

            propertyValues.Clear();
            foreach (var value in eventHandler.GetType().GetFields())
            {

                DataObjectValue valueData = new DataObjectValue();
                valueData.SetDataToValue(value.GetValue(eventHandler));
                propertyValues.Add(valueData);
            }
        }
    }


    [Serializable]
    public class CommandSaveData//Command儲存檔案都必須繼承
    {
        public string commandType;

        public List<DataObjectValue> fieldDataList = new List<DataObjectValue>();

        public CommandSaveData(Command data)
        {

            commandType = data.GetType().FullName;

            DataObjectValue.JudgeValueType(data, dataObj => { fieldDataList.Add(dataObj); });

        }

    }

    [Serializable]
    public class StageSaveData
    {

        public bool dimPortraits;

        public Color dimColor;
        public float fadeDuration;
        public float moveDuration;
        public LeanTweenType fadeEaseType;
        public Vector2 shiftOffset;

        public string defaultPosition;//recttransform
        public List<RectPositionsInfo> createAreaPositions = new List<RectPositionsInfo>();
        public List<ViewPositionsInfo> createViewPositions = new List<ViewPositionsInfo>();

        //public List<SpriteRenderInfo> spriteRenderers = new List<SpriteRenderInfo>();
        public List<ImageInfo> imageList = new List<ImageInfo>();
        public List<AudioInfo> audioList = new List<AudioInfo>();

        public StageSaveData(Stage stage)
        {
            dimPortraits = stage.DimPortraits;
            dimColor = stage.DimColor;
            fadeDuration = stage.FadeDuration;
            moveDuration = stage.MoveDuration;
            shiftOffset = stage.ShiftOffset;
            fadeEaseType = stage.FadeEaseType;
            if (stage.DefaultPosition != null)
            {
                defaultPosition = stage.DefaultPosition.name;
            }
            else
            {
                defaultPosition = stage.Positions[0].name;
            }


            foreach (var rect in stage.Positions)
            {
                RectPositionsInfo info = new RectPositionsInfo(rect);
                createAreaPositions.Add(info);

            }

            for (int i = 0; i < stage.ViewParent.childCount; i++)
            {
                var child = stage.ViewParent.GetChild(i);
                ViewPositionsInfo info = new ViewPositionsInfo(child.GetComponent<View>());
                createViewPositions.Add(info);
            }

            try
            {
                for (int i = 0; i < stage.LastBackGroundParent.childCount; i++)
                {
                    var child = stage.LastBackGroundParent.GetChild(i);
                    ImageInfo info = new ImageInfo(child.GetComponent<Image>(), ImageOrderParent.LastBackGround);
                    imageList.Add(info);
                }
            }
            catch
            {

            }


            for (int i = 0; i < stage.BackGroundParent.childCount; i++)
            {
                var child = stage.BackGroundParent.GetChild(i);
                ImageInfo info = new ImageInfo(child.GetComponent<Image>(), ImageOrderParent.BackGround);
                imageList.Add(info);
            }

            for (int i = 0; i < stage.ForeImageParent.childCount; i++)
            {
                var child = stage.ForeImageParent.GetChild(i);
                ImageInfo info = new ImageInfo(child.GetComponent<Image>());
                imageList.Add(info);
            }

            for (int i = 0; i < stage.ForeEffectParent.childCount; i++)
            {
                var child = stage.ForeEffectParent.GetChild(i);
                ImageInfo info = new ImageInfo(child.GetComponent<Image>(), ImageOrderParent.ForeGround);
                imageList.Add(info);
            }



            for (int i = 0; i < stage.AudiosParent.childCount; i++)
            {
                var child = stage.AudiosParent.GetChild(i);
                AudioInfo info = new AudioInfo(child.GetComponent<AudioSource>());
                audioList.Add(info);
            }


        }

        public void SetDataToClass(Stage stage)
        {
            stage.ClearData();

            stage.DimPortraits = dimPortraits;
            stage.DimColor = dimColor;
            stage.FadeDuration = fadeDuration;
            stage.FadeEaseType = fadeEaseType;
            stage.MoveDuration = moveDuration;
            stage.ShiftOffset = shiftOffset;

            foreach (var rectInfo in createAreaPositions)
            {
                GameObject sp = new GameObject(rectInfo.rectName, typeof(RectTransform));
                sp.transform.SetParent(stage.PositionsParent, false);
                RectTransform spRect = sp.GetComponent<RectTransform>();
                stage.Positions.Add(spRect);
                rectInfo.SetDataToRectTransform(spRect);
                if (rectInfo.rectName == defaultPosition)
                {
                    stage.DefaultPosition = spRect;
                }
            }
            foreach (var viewInfo in createViewPositions)
            {

                GameObject sp = new GameObject(viewInfo.rectName, typeof(RectTransform), typeof(View));
                sp.transform.SetParent(stage.ViewParent, false);
                View spRect = sp.GetComponent<View>();
                viewInfo.SetViewDataToRectTransform(spRect);

            }
            /*     foreach (var sprite in spriteRenderers)
                 {

                     GameObject sp = new GameObject(sprite.transName, typeof(SpriteRenderer));
                     sp.transform.SetParent(stage.ImageParent, false);
                     SpriteRenderer spRect = sp.GetComponent<SpriteRenderer>();
                     sprite.SetImageDataToTransform(spRect);

                 }*/
            foreach (var image in imageList)
            {

                GameObject sp = new GameObject(image.rectName, typeof(RectTransform), typeof(Image));
                sp.transform.SetParent(image.SetImageParent(stage), false);

                Image spRect = sp.GetComponent<Image>();
                image.SetImageDataToRectTransform(spRect);

            }

            foreach (var audio in audioList)
            {

                GameObject sp = new GameObject(audio.transName, typeof(AudioSource));

                sp.transform.SetParent(stage.AudiosParent, false);
                AudioSource spRect = sp.GetComponent<AudioSource>();
                audio.SetAudioSourceDataToRectTransform(spRect);

            }

        }


    }


    [Serializable]
    public class VariableData
    {

        public VariableScope variableScope;

        public string key;

        public object value;

        public VariableData(Variable var)
        {
            variableScope = var.Scope;
            key = var.Key;
            value = var.GetValue();


            //     type=var.

        }

        public Variable DataSetToClass(Flowchart flowchart)
        {
            Variable var = flowchart.gameObject.AddComponent<Variable>();
            var.Scope = variableScope;
            var.Key = key;
            //需要賦值 value  目前尚無法獲取到value的位置

            return var;
        }

    }
    [Serializable]
    public class RectPositionsInfo
    {

        public string rectName;

        public Vector3 position;

        public Vector2 size;

        public Vector2 pivot;

        public Vector2 anchorMin;

        public Vector2 anchorMax;

        public RectPositionsInfo(string _rectName, Vector3 _pos, Vector2 _size, Vector2 _pivot, Vector2 _anchorMin, Vector2 _anchorMax)
        {
            rectName = _rectName;
            position = _pos;
            size = _size;
            pivot = _pivot;
            anchorMin = _anchorMin;
            anchorMax = _anchorMax;

        }

        public RectPositionsInfo(RectTransform rect)
        {
            rectName = rect.name;
            position = rect.position;
            size = rect.sizeDelta;
            pivot = rect.pivot;
            anchorMin = rect.anchorMin;
            anchorMax = rect.anchorMax;
        }

        public void SetDataToRectTransform(RectTransform rect)
        {
            rect.name = rectName;
            rect.position = position;
            rect.sizeDelta = size;
            rect.pivot = pivot;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;

        }


    }


    [Serializable]

    public class PositionsInfo
    {
        public string transName;

        public Vector2 position;

        public Vector2 Scale;

        public PositionsInfo(Transform _transform)
        {
            transName = _transform.name;
            position = _transform.position;
            Scale = _transform.localScale;
        }
        public void SetDataToTransform(Transform _transform)
        {
            _transform.name = transName;
            _transform.position = position;
            _transform.localScale = Scale;

        }


    }
    [Serializable]
    public class SpriteRenderInfo : PositionsInfo//顯示的圖片
    {

        public string assetSpritePath = "";
        public Color color;

        public string sortOrderName = "";
        public int order = 0;

        public SpriteRenderInfo(SpriteRenderer sprite) : base(sprite.transform)
        {
            if (sprite.sprite)
            {
#if UNITY_EDITOR
                assetSpritePath = AssetDatabase.GetAssetPath(sprite.sprite);
#endif
            }

            color = sprite.color;


            order = sprite.sortingOrder;
            sortOrderName = sprite.sortingLayerName;


        }
        public void SetImageDataToTransform(SpriteRenderer sprite)
        {
            SetDataToTransform(sprite.transform);

            if (!assetSpritePath.Equals("") && !assetSpritePath.Equals(null))
            {
#if UNITY_EDITOR
                sprite.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetSpritePath);//圖片要改成抓resources 目前只有在editor上能正常執行
#endif
            }
            sprite.color = color;

            if (!sortOrderName.Equals("") || sortOrderName.Equals(null))
            {
                sprite.sortingLayerName = sortOrderName;
                sprite.sortingOrder = order;
            }
        }
    }

    [Serializable]
    public class ViewPositionsInfo : RectPositionsInfo
    {

        public float viewSize = 0.5f;


        public Vector2 primaryAspectRatio = new Vector2(4, 3);


        public Vector2 secondaryAspectRatio = new Vector2(2, 1);

        public ViewPositionsInfo(View view) : base(view.GetComponent<RectTransform>())
        {
            viewSize = view.ViewSize;
            primaryAspectRatio = view.PrimaryAspectRatio;
            secondaryAspectRatio = view.SecondaryAspectRatio;

        }

        public void SetViewDataToRectTransform(View view)
        {
            SetDataToRectTransform(view.GetComponent<RectTransform>());
            view.ViewSize = viewSize;
            view.PrimaryAspectRatio = primaryAspectRatio;
            view.SecondaryAspectRatio = secondaryAspectRatio;
        }

    }
    [Serializable]
    public class AudioInfo : PositionsInfo//顯示的音檔
    {

        public string ClipAssetsPath = "";

        public bool mute;
        public bool playOnAwake;
        public bool loop;

        public int priority;
        public float volume;
        public float pitch;

        public AudioInfo(AudioSource audio) : base(audio.transform)
        {
            if (audio.clip)
            {
#if UNITY_EDITOR
                ClipAssetsPath = AssetDatabase.GetAssetPath(audio.clip);
#endif
            }
            mute = audio.mute;
            playOnAwake = audio.playOnAwake;
            loop = audio.loop;
            priority = audio.priority;
            volume = audio.volume;
            pitch = audio.pitch;


        }

        public void SetAudioSourceDataToRectTransform(AudioSource audio)
        {
            SetDataToTransform(audio.transform);
            if (!ClipAssetsPath.Equals("") && !ClipAssetsPath.Equals(null))
            {
#if UNITY_EDITOR
                audio.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClipAssetsPath);
#endif
            }

            audio.mute = mute;
            audio.playOnAwake = playOnAwake;
            audio.loop = loop;
            audio.priority = priority;
            audio.volume = volume;
            audio.pitch = pitch;
        }

    }

    public enum ImageOrderParent
    {
        LastBackGround,
        BackGround,
        Images,
        ForeGround
    }

    [Serializable]
    public class ImageInfo : RectPositionsInfo//顯示的圖片
    {

        public string assetSpritePath = "";

        public ImageOrderParent imageParent;

        public Color color;

        public bool raycastTarget = false;

        public bool PreserveAspect = true;

        public string sortOrderName = "";

        public int order = 0;

        public ImageInfo(Image image, ImageOrderParent _imageParent = ImageOrderParent.Images) : base(image.GetComponent<RectTransform>())
        {
            imageParent = _imageParent;
            if (image.sprite)
            {
#if UNITY_EDITOR
                assetSpritePath = AssetDatabase.GetAssetPath(image.sprite);
#endif
            }

            color = image.color;

            raycastTarget = image.raycastTarget;

            PreserveAspect = image.preserveAspect;

            Canvas canvas = null;

            if (image.TryGetComponent<Canvas>(out canvas))
            {

                order = canvas.sortingOrder;
                sortOrderName = canvas.sortingLayerName;

            }

        }

        public void SetImageDataToRectTransform(Image image)
        {
            SetDataToRectTransform(image.GetComponent<RectTransform>());

            if (!assetSpritePath.Equals("") && !assetSpritePath.Equals(null))
            {
#if UNITY_EDITOR
                image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetSpritePath);
#endif
            }
            image.color = color;
            image.raycastTarget = raycastTarget;
            image.preserveAspect = PreserveAspect;

            if (!sortOrderName.Equals("") || !sortOrderName.Equals(null))
            {

                Canvas canvas = image.gameObject.AddComponent<Canvas>();
                canvas.sortingLayerName = sortOrderName;
                canvas.sortingOrder = order;
            }
        }

        public Transform SetImageParent(Stage stage)
        {

            switch (imageParent)
            {
                case ImageOrderParent.LastBackGround:
                    return stage.LastBackGroundParent;
                case ImageOrderParent.BackGround:
                    return stage.BackGroundParent;
                case ImageOrderParent.Images:
                    return stage.ForeImageParent;
                case ImageOrderParent.ForeGround:
                    return stage.ForeEffectParent;
            }
            return null;

        }

        //sprite資訊  圖層資訊  都用rect transform

    }

    [Serializable]
    public class DataObjectValue
    {
        //class list is dataobjectValue  string is class type
        //enum  int is enum index  string is enum class name
        public string typeName = "";

        public List<DataObjectValue> _class;  //class   list array 也是此
        public string data;

        public bool isNull = false;
        public string _path = "";


        public static void JudgeValueType(object _data, Action<DataObjectValue> _cb = null)//serializable parse class  
        {


            Debug.Log("顯示物件=>" + _data);

            foreach (var field in Type.GetType(_data.GetType().FullName).GetFields(ExportData.DefaultBindingFlags))
            {
                DataObjectValue value = new DataObjectValue();
                Debug.Log("欄位的名稱=>" + field.Name);

                if (field.FieldType.IsGenericType)
                {
                    value.SetDataToValue(field.FieldType.GetField("_items", ExportData.DefaultBindingFlags).GetValue(field.GetValue(_data)));
                }
                else
                {
                    value.SetDataToValue(field.GetValue(_data));
                }
                if (_cb != null)
                {

                    _cb(value);

                }
            }


        }

        public void SetDataToValue(object value)
        {
            if (value == null)//連空類型都沒
            {
                isNull = true;
                return;
            }
            var mType = value.GetType();
            typeName = mType.FullName;
            _class = new List<DataObjectValue>();


            if (mType.IsValueType || mType.BaseType.IsValueType || mType == typeof(string))
            {


                Debug.Log("輸入的value的TYPE=>" + typeName);
                Debug.Log("輸入的value=>" + value as string);

                if (mType.IsEnum)
                {
                    Debug.Log("測試1" );
                    data = value.ToString();
                    return;
                }

                if (mType.FullName.StartsWith("UnityEngine") || mType.FullName.StartsWith("Fungus"))
                {
                    switch (typeName)//指定prefab
                    {
                        case "Fungus.GameObjectData":

                            var objData = (GameObjectData)value;
                            _path = objData.gameObjectVal.transform.parent.name;
                            data = objData.gameObjectVal.name;

                            Debug.Log("關於ObjData=>" + _path + "和數據=>" + data);

                            break;
                        default:
                            data = JsonUtility.ToJson(value);
                            break;
                    }
                }
                else
                {
                    if (mType == typeof(string))
                    {
                        data = value as string;
                    }
                    else
                    {
                                data = JsonConvert.SerializeObject(value);
                    }

                }
                return;
            }

            bool isDefaultExecuteClass = true;

            if (value.Equals(null))//類型是空
            {
                isNull = true;
                return;
            }

            switch (typeName)//指定prefab
            {
                case "UnityEngine.RectTransform":
                    data = (value as RectTransform).name;
                    break;
                case "UnityEngine.AudioSource"://要抓取stage上的audio 必須生成並附上相關的資訊
                    data = (value as AudioSource).name;
                    break;
                case "UnityEngine.Camera"://要抓取stage上的audio 必須生成並附上相關的資訊
                    data = (value as Camera).name;
                    break;
                case "Fungus.Stage":
                    data = (value as Stage).name;
                    break;
                case "Fungus.View":
                    data = (value as View).name;
                    break;

                case "UnityEngine.UI.Image":
                    _path = (value as Image).transform.parent.name;
                    data = (value as Image).name;
                    break;
                case "UnityEngine.SpriteRenderer":
                    data = (value as SpriteRenderer).name;
                    break;
                case "UnityEngine.GameObject":
                    _path = (value as GameObject).transform.parent.name;
                    data = (value as GameObject).name;
                    break;
                case "Fungus.Character":
                    data = (value as Character).name;
                    break;
                case "Fungus.Block":

                    data = (value as Block).BlockName;//menu 的target
                    Debug.Log("有輸出block=>" + data);
                    break;
                case "UnityEngine.Sprite"://需要圖片路徑  可能會抓hierarchy上的
                    data = (value as Sprite).name;
#if UNITY_EDITOR
                    _path = AssetDatabase.GetAssetPath((value as UnityEngine.Object));
#endif
                    break;
                case "UnityEngine.Texture":
                    data = (value as Texture).name;
#if UNITY_EDITOR
                    _path = AssetDatabase.GetAssetPath((value as UnityEngine.Object));
#endif
                    break;
                case "UnityEngine.Texture2D":
                    data = (value as Texture2D).name;
#if UNITY_EDITOR
                    _path = AssetDatabase.GetAssetPath((value as UnityEngine.Object));
#endif
                    break;  
                case "UnityEngine.AudioClip":
                    data = (value as AudioClip).name;
#if UNITY_EDITOR
                    _path = AssetDatabase.GetAssetPath((value as UnityEngine.Object));
#endif

                    break;
                default:
                    isDefaultExecuteClass = false;
                    break;
            }

            if (isDefaultExecuteClass)
            {
                return;
            }

            if (mType.IsGenericType)
            {
                Debug.Log("是清單=>" + typeName);
                SetDataToValue(mType.GetField("_items", ExportData.DefaultBindingFlags).GetValue(value));
            }
            else if (mType.IsArray)
            {

                Debug.Log("是陣列=>" + typeName + "長度=>" + (value as Array).Length);
                ICollection collection = value as ICollection;

                foreach (var col in collection)
                {
                    if (col == null)
                    {
                        continue;
                    }
                    if (col.GetType().IsGenericType)
                    {
                        Debug.Log("還是清單=>" + col);
                        DataObjectValue newValue = new DataObjectValue();

                        newValue.SetDataToValue(col.GetType().GetField("_items", ExportData.DefaultBindingFlags).GetValue(col));
                        _class.Add(newValue);
                        // this = newValue;
                    }
                    else
                    {
                        Debug.Log("輸入陣列值=>" + col);
                        DataObjectValue newValue = new DataObjectValue();
                        newValue.SetDataToValue(col);
                        _class.Add(newValue);
                    }
                }

            }
            else if (mType.IsEnum)
            {
                data = value.ToString();
            }
            else if (mType.IsClass)
            {
                Debug.Log("是類別=>" + typeName + "值名稱=>" + value);

                 _class = new List<DataObjectValue>();
                 List<DataObjectValue> temp = new List<DataObjectValue>();
                 JudgeValueType(value, valueObj => { temp.Add(valueObj); });
                _class.AddRange(temp);
            }
            else
            {
                Debug.LogError("預料外的漏網之魚==>" + value);
            }

        }

        public IEnumerator GetValueData(Flowchart parentObj = null, Action<object> cbValue = null)
        {
            if (isNull)
            {
                cbValue(null);
                yield break;
            }
            var mType = Type.GetType(typeName);
            switch (typeName)
            {
                case "UnityEngine.RectTransform":
                    cbValue(parentObj.mStage.GetPosition((data as string)));
                    yield break;
                case "Fungus.Stage":
                    cbValue(parentObj.mStage);
                    yield break;
                case "Fungus.View":
                    cbValue(parentObj.mStage.GetView((data as string)));
                    yield break;
                case "UnityEngine.UI.Image":

                    var eImageParent = (ImageOrderParent)Enum.Parse(typeof(ImageOrderParent), _path);
                    cbValue(parentObj.mStage.GetImage((data as string), eImageParent));

                    yield break;
                case "UnityEngine.UI.Image[]":
                    mType = typeof(Image[]);
                    break;
                case "UnityEngine.SpriteRenderer":
                    cbValue(parentObj.mStage.GetSpriteRenderer((data as string)));
                    yield break;

                case "UnityEngine.GameObject":
                    var eObjParent = (ImageOrderParent)Enum.Parse(typeof(ImageOrderParent), _path);
                    cbValue(parentObj.mStage.GetImage((data as string), eObjParent).gameObject);
                    yield break;
                case "Fungus.GameObjectData":
                    Debug.Log("objData母物件名稱=>" + _path);
                    var eFunObjParent = (ImageOrderParent)Enum.Parse(typeof(ImageOrderParent),_path);
                    GameObjectData gameobjData = new GameObjectData(parentObj.mStage.GetImage((data as string), eFunObjParent).gameObject);
                    cbValue(gameobjData);
                    yield break;

                case "Fungus.Block":
                    cbValue(parentObj.FindBlock((data as string)));
                    yield break;
                case "Fungus.Character":
                    Character resChara = null;
                    yield return FungusResources.GetCharacter((data as string), _res => { resChara = _res; });
                    cbValue(resChara);
                    yield break;


                case "UnityEngine.Sprite"://需要圖片路徑  可能會抓hierarchy上的
#if UNITY_EDITOR
                    cbValue(AssetDatabase.LoadAssetAtPath<Sprite>(_path));
#endif
                    yield break;
                case "UnityEngine.AudioClip":
#if UNITY_EDITOR
                    cbValue(AssetDatabase.LoadAssetAtPath<AudioClip>(_path));
#endif
                    yield break;
                case "UnityEngine.Texture":
#if UNITY_EDITOR
                    cbValue(AssetDatabase.LoadAssetAtPath<Texture>(_path));
#endif
                    yield break;
                case "UnityEngine.Texture2D":
#if UNITY_EDITOR
                    cbValue(AssetDatabase.LoadAssetAtPath<Texture2D>(_path));
#endif
                    yield break;
                case "UnityEngine.Vector2":
                    mType = typeof(Vector2);
                    break;
                case "UnityEngine.Vector3":
                    mType = typeof(Vector3);
                    break;
                case "UnityEngine.Vector4":
                    mType = typeof(Vector4);
                    break;
                case "UnityEngine.Rect":
                    mType = typeof(Rect);
                    break;
                case "UnityEngine.Color":
                    mType = typeof(Color);
                    break;
                case "UnityEngine.GameObject[]":
                    mType = typeof(GameObject[]);
                    break;
                case "UnityEngine.Space":
                    Debug.Log("是space");
                    mType = typeof(Space);
                    break;
            }

            Debug.Log("目前的mtypeName=>" + typeName);
            Debug.Log("目前的mtype=>" + mType);
            Debug.Log("裡面的value=>" + data);

            if (mType.IsArray)
            {

                Debug.Log("陣列的元素type=>" + mType.GetElementType());
                Debug.Log("陣列的數量=>" + _class.Count);

                Array arr = Array.CreateInstance(mType.GetElementType(), _class.Count);

                for (int i = 0; i < arr.Length; i++)
                {
                    object resVal = null;
                    yield return _class[i].GetValueData(parentObj, val => { resVal = val; });

                    Debug.Log("回傳的數值=>" + resVal);

                    if (mType.GetElementType().IsGenericType)
                    {
                        if (mType.GetElementType() == typeof(List<string>))
                        {
                            List<string> strList = new List<string>(resVal as string[]);
                            arr.SetValue(strList, i);

                        }

                    }
                    else
                    {
                        arr.SetValue(resVal, i);
                    }
                }

                cbValue(arr);
                yield break;
            }

            if (mType.IsValueType || mType.BaseType.IsValueType || (mType.BaseType != null && mType == typeof(string)))
            {

                //cbValue(JsonUtility.FromJson(data, mType));
                if (mType.FullName.StartsWith("System"))
                {
                    switch (mType.FullName.Substring(7, (mType.FullName.Length - 7)))
                    {
                        case "String":
                            cbValue(data);
                            break;
                        case "Boolean":
                            if ((string)data == "true")
                            {
                                cbValue(true);
                            }
                            else
                            {
                                cbValue(false);
                            }
                            break;
                        case "Int32":
                            int tempInt = 0;
                            if (int.TryParse((string)data, out tempInt))
                            {
                                cbValue(tempInt);
                            }
                            else
                            {
                                cbValue(-1);
                            }

                            break;
                        case "Single":
                            float tempFloat = 0;
                            if (float.TryParse((string)data, out tempFloat))
                            {
                                cbValue(tempFloat);
                            }
                            else
                            {
                                cbValue(-1);
                            }
                            break;
                        case "Char":

                            break;
                        case "Double":

                            break;
                        case "Decimal":

                            break;
                        default:
                            Debug.Log("測試值=>" + data);
                            cbValue(data);
                            break;

                    }
                }
                else
                {
                    if (mType.IsEnum)
                    {
                        cbValue(data);
                    }
                    else
                    {
                        Debug.Log("直接解壓?"+data);
                        cbValue(JsonUtility.FromJson(data, mType));
                    }


                }

                yield break;
            }

            else if (mType.IsClass)
            {
                Debug.Log("跑class=>" + mType);


                //主要是enum和valueType文字轉換有問題
                //需要一個共同規範func

                if (mType == typeof(SnsMessage))
                {
                    yield return GetClassDataFunc<SnsMessage>(parentObj, obj =>
                    {
                        cbValue(obj);
                    });
                }
                if (mType == typeof(SnsChara))
                {
                    yield return GetClassDataFunc<SnsChara>(parentObj, obj =>
                    {
                        cbValue(obj);
                    });

                }
                if (mType == typeof(SnsMessageType))
                {
                    yield return GetClassDataFunc<SnsMessageType>(parentObj, obj =>
                    {
                        cbValue(obj);
                    });

                }
                if (mType == typeof(ReplyAnswer))
                {
                    yield return GetClassDataFunc<ReplyAnswer>(parentObj, obj =>
                    {
                        cbValue(obj);
                    });

                }
                if (mType == typeof(CharaSnsSetting))
                {
                    yield return GetClassDataFunc<CharaSnsSetting>(parentObj, obj =>
                    {
                        cbValue(obj);
                    });
                }
                yield break;
            }
            else
            {
                Debug.LogWarning("意外的錯誤,該類別名稱=>" + typeName);
                cbValue(null);
                yield break;

            }

        }

        public IEnumerator GetClassDataFunc<T>(Flowchart parentObj, Action<T> cbValue) where T : new()
        {

            T obj = new T();
            var type = typeof(T);


            Debug.Log("_class陣列數量=>"+_class.Count);

            for (int i = 0; i < type.GetFields().Length; i++)
            {
                var field = type.GetFields()[i];
                yield return _class[i].GetValueData(parentObj,
                  res =>
                  {

                      if (field.FieldType.IsEnum)
                      {

                          field.SetValue(obj, Enum.Parse(field.FieldType, (string)res));
                      }
                      else if
                      (field.FieldType.IsGenericType)
                      {
                          if (field.FieldType == typeof(List<CharaSnsSetting>))
                          {

                              CharaSnsSetting[] tempArr = (CharaSnsSetting[])res;
                              field.SetValue(obj, tempArr.ToList());
                          }
                          else
                          {
                              Debug.Log("未知class項目的輸出陣列類型=>" + field.FieldType.FullName);
                          }
                      }
                      else
                      {
                          field.SetValue(obj, res);
                      }

                  });

            }
            cbValue(obj);
        }



    }
}

#endif