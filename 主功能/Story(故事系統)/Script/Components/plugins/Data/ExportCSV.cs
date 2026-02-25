#if UNITY_EDITOR
using System.Collections.Generic;
using System.Collections;
using System.Data;
using System.IO;
using UnityEngine;
using System;
using System.Linq;
using Unity.VisualScripting;
using YKO.Support.Expansion;
using static Fungus.JsonToExcel;
using static Proto_11094_Request;
using UnityEngine.UI;
using UnityEditor;
using YKO.Support;
using JetBrains.Annotations;


namespace Fungus
{


    public class JsonToExcel
    {
        /// <summary>
        /// 根據參數命名,轉換Csv輸出與輸入Data的位置 
        /// </summary>
        public class ConvertScriptToCsv
        {
            /// <summary>
            /// 實際上在腳本的value名稱
            /// </summary>
            public string fieldName="";
            /// <summary>
            /// 顯示在csv的名稱(方便企劃閱讀,預設是fieldName
            /// </summary>
            public string fieldNameDisplayToCsv="";

            public ValueType parentValueType= ValueType.None;
            /// <summary>
            /// 顯示在csv的值字串
            /// </summary>
            public Func<Command, string> GetDataString;
            /// <summary>
            /// 透過字串獲得值
            /// </summary>
            public Func<Command,string, IEnumerator> GetData;

            public ConvertScriptToCsv(string _fieldName,
                Func<Command,object, string> getDataStringFunc = null,
                Func<Command,string, IEnumerator> getDataFunc = null,
                string displayFieldName=null,
                ValueType valueType= ValueType.None
                )
            {
                fieldName = _fieldName;
                parentValueType = valueType;
                if (string.IsNullOrWhiteSpace(displayFieldName))
                {
                    fieldNameDisplayToCsv = _fieldName;
                }
                else
                {
                    fieldNameDisplayToCsv = displayFieldName;
                }

                if (getDataStringFunc == null)
                {
                    GetDataString = GetDataStringFunc;
                }
                else
                {
                    // GetStringData = getDataFunc;
                    switch (parentValueType)
                    {
                        case ValueType.None:
                            GetDataString = (com) =>
                            {
                                return getDataStringFunc(com, com.GetType().GetField(fieldName, ExportData.DefaultBindingFlags).GetValue(com));
                            };
                            break;
                        case ValueType.Struct://因為母物件可能有很多個值類型,而只需要結構裡的其中一個值,所以需要這個步驟 ex: spineCharaAni的 fadeAniDurAtion
                            GetDataString = (com) =>
                            {
                                return getDataStringFunc(com, null);
                            };
                            break;
                    }


                }

                if (getDataFunc == null)
                {
                    GetData = GetDataFunc;
                }
                else
                {
                    GetData = getDataFunc;
                }
            }

            public string GetFieldName()    
            {
                return fieldName;
            }
            private string GetDataStringFunc(Command com)
            {
                Debug.Log("Command名稱=>"+com+"值名=>" + fieldName);
                //var value= com.GetType().GetField(fieldName, ExportData.DefaultBindingFlags).GetValue(com)
                return com.GetType().GetField(fieldName, ExportData.DefaultBindingFlags).GetValue(com)?.ToString();
            }
            /// <summary>
            /// 透過 fieldName 去附值 command的value 獲得數據
            /// </summary>
            /// <param name="valueName"></param>
            /// <param name="com"></param>
            /// <returns></returns>
            private IEnumerator GetDataFunc(Command com,string valueName)
            {
                object value;
                if (string.IsNullOrEmpty(valueName))
                {
                    yield break;
                }
                else
                {
                    switch (com.GetType().GetField(fieldName, ExportData.DefaultBindingFlags).FieldType.Name)
                    {
                        case "Boolean":
                            value = bool.Parse(valueName);
                            break;
                        case "Int32":
                            value = int.Parse(valueName);
                            break;
                        case "Single":
                            value = float.Parse(valueName);
                            break;
                        case "Vector3":
                            var str = valueName.Trim().Trim('(',')');
                            string[] parts = str.Split(',');
                            float x, y, z;
                            bool isXValid = float.TryParse(parts[0].Trim(), out x);
                            bool isYValid = float.TryParse(parts[1].Trim(), out y);
                            bool isZValid = float.TryParse(parts[2].Trim(), out z);

                            if (!isXValid || !isYValid || !isZValid)
                            {
                                Debug.LogError("字串中的某些數值無法解析為Float。");
                                value = Vector3.zero;
                            }
                            value = new Vector3(x,y,z);
                            break;
                        default:
                            switch (com.GetType().GetField(fieldName, ExportData.DefaultBindingFlags).FieldType.BaseType.ToString())
                            {
                                case "System.Enum":
                                    value=Enum.Parse(
                                        Type.GetType(com.GetType().GetField(fieldName, ExportData.DefaultBindingFlags).FieldType.FullName),
                                        valueName);
                                    break;
                                default:
                                    value = valueName;
                                    break;
                            }
                            break;
                    }
                    
                    com.GetType().GetField(fieldName, ExportData.DefaultBindingFlags).SetValue(com, value);
                }

            }
            /// <summary>
            /// 母物件的值類型
            /// </summary>
            public enum ValueType
            {
                None,
                Struct,
                List

            }


        }
            /// <summary>
            /// 紀錄每個Class 個別參數名稱的輸出輸入模式,如沒特別指定,則遵照預設(預設通常為可序列化參數使用 ex string ,float等),如有mono不可序列化腳本,需要自訂才可正常獲取或返回
            /// </summary>
        public static Dictionary<Type, List<ConvertScriptToCsv>> CommandExportToValueSettingList = new Dictionary<Type, List<ConvertScriptToCsv>>() {
            { typeof(Say), new List<ConvertScriptToCsv>(){
                new ConvertScriptToCsv("character",
                  (com,value)=>{
                      string answer="";

                      if((value as Character)!=null){
                      answer=(value as Character).name;
                      }
                      return answer;
                  },
                 (com,v)=>{

                     IEnumerator ie()
                     {
                          if(string.IsNullOrEmpty(v)){
                          yield break;
                          }
                         yield return FungusResources.GetCharacter(v, ch =>
                            {
                                (com as Say).Character=ch;
                            });
                     }

                     return ie();

                }
                 ) ,
                new ConvertScriptToCsv("nameText"),
                new ConvertScriptToCsv("loop"),
                new ConvertScriptToCsv("fadeWhenDone"),
                new ConvertScriptToCsv( "aAnimation" ) ,
                new ConvertScriptToCsv( "aSkin") ,
                new ConvertScriptToCsv( "voiceOverClip" ,(com,value)=>{
                    string answer="";
                    if((value as AudioClip)!=null){
                    answer=(value as AudioClip).name;
                    }
                    return answer;
                },
                    (com,v)=>{

                        IEnumerator ie()
                        {
                            if(string.IsNullOrEmpty( v)){
                            yield break;
                            }
                             yield return FungusResources.GetAudioClip(v, voice =>
                            {
                                (com as Say).VoiceOverClip= voice;
                            });
                        }
                        return ie();
                }) ,
                new ConvertScriptToCsv( "aFinishDefaultAnimation") ,
               /*ew ConvertScriptToCsv( "mouthState",null,(com,str)=>{

                  (com as Say).MouthState=(Say.MouthAnimation)Enum.Parse(typeof(Say.MouthAnimation),str) ;

                    return null;
                }) ,*/

                new ConvertScriptToCsv( "mouthState") ,

                new ConvertScriptToCsv( "storyText") ,
                new ConvertScriptToCsv( "description") ,
            } },
            { typeof(Menu), new List<ConvertScriptToCsv>(){
                new ConvertScriptToCsv( "text") ,
                new ConvertScriptToCsv( "targetBlock",
                    (com,value)=>{
                        string answer=null;
                        if((value as Block)!=null){
                        answer=(value as Block).BlockName;
                        }
                        return answer;
                    },
                    (com,v)=>
                    {
                    IEnumerator ie()
                    {

                       if(string.IsNullOrEmpty(v)){
                       yield break;
                       }
                       (com as Menu).TargetBlockName = v;
                    }
                    return ie();
                    }) ,
                new ConvertScriptToCsv( "description")
            } },
            { typeof(SpineCharaAni), new List<ConvertScriptToCsv>(){
                new ConvertScriptToCsv("aTarget",
                  (com,value)=>{
                      string answer="";

                      if((value as Character)!=null){
                      answer=(value as Character).name;
                      }
                      return answer;
                  },
                 (com,v)=>{

                     IEnumerator ie()
                     {
                          if(string.IsNullOrEmpty(v)){
                          yield break;
                          }
                         yield return FungusResources.GetCharacter(v, ch =>
                            {
                                (com as SpineCharaAni).aTarget=ch;
                            });
                     }

                     return ie();

                },
                 "SkeletonGraphic"
                 ) ,
                new ConvertScriptToCsv("display"),
                new ConvertScriptToCsv("fade"),
                new ConvertScriptToCsv("aFadeAniDuration",
                    (com,v)=>
                    {
                    return (com as SpineCharaAni).aTween.aFadeAniDuration.ToString();
                    },
                    (com,v)=>{ 
                        IEnumerator ie()
                        {
                            if(string.IsNullOrEmpty(v))
                            {
                            yield break;
                            }
                            (com as SpineCharaAni).aTween.aFadeAniDuration=float.Parse(v);
                        }
                        return ie();
                    },
                    valueType: ConvertScriptToCsv.ValueType.Struct
                    ),
                new ConvertScriptToCsv( "aInitialSkinName" ){  fieldNameDisplayToCsv="Skin"} ,
                new ConvertScriptToCsv( "aAnimation"){  fieldNameDisplayToCsv="Animation"} ,
                new ConvertScriptToCsv( "loop") ,
                new ConvertScriptToCsv( "toPosition" ,
                 (com,value)=>{
                      string answer="";

                      if((value as RectTransform)!=null){
                      answer=(value as RectTransform).name;
                      }
                      return answer;
                  },
                 (com,v)=>{

                     IEnumerator ie()
                     {
                          if(string.IsNullOrEmpty(v)){
                          yield break;
                          }

                         var target=com.ParentBlock.GetFlowchart().mStage.GetPositions().FirstOrDefault(val => val.name == v);
                         (com as SpineCharaAni).ToPosition=target;
                     }

                     return ie();
                 } ) ,
                new ConvertScriptToCsv( "scaleAni") ,
                new ConvertScriptToCsv( "effectScale") ,
            } },
            { typeof(SetUIImageOfPath), new List<ConvertScriptToCsv>(){
                new ConvertScriptToCsv("path"),
                new ConvertScriptToCsv("images",
                    (com,v)=>{
                        var list=(v as List<Image>);
                        if(list.Count>0)
                        {
                            return list[0].gameObject.name;
                        }
                        else
                        {
                            return "";
                        }
                    },
                    (com,v)=>{ 
                         IEnumerator ie()
                        {
                        yield return null;
                           if(string.IsNullOrEmpty(v))
                            {
                              yield break;
                            }

                            var target=GameObject.Find(v);
                            Image img=null;

                            if(target!=null&&target.TryGetComponent(out img))
                            {
                                (com as SetUIImageOfPath).images?.Clear();
                                (com as SetUIImageOfPath).images.Add(img);
                             }
                        }

                     return ie();
                    
                    }),
                new ConvertScriptToCsv( "effectType") ,
                new ConvertScriptToCsv( "effectDuration",
                    null,
                    (com,v)=>{ 
                    
                        IEnumerator ie()
                        {
                            if(string.IsNullOrWhiteSpace(v))
                            {
                            yield break;
                            }

                            var waitTime=float.Parse(v);
                            var tarCom=(com as SetUIImageOfPath);
                            if(waitTime>0)
                            {
                                tarCom.waitUntilFinished=true;
                                tarCom.effectDuration = waitTime;
                            }
                        }
                          return ie();
                    }
                    ) ,


            } },
            { typeof(SetUIImage), new List<ConvertScriptToCsv>(){
                new ConvertScriptToCsv("sprite",
                    (com,v)=>{

                       return AssetDatabase.GetAssetPath(v as Sprite);
                    },
                    (com,v)=>{ 
                         IEnumerator ie()
                        {
                            if(string.IsNullOrWhiteSpace(v)){
                                 (com as SetUIImage).sprite=null;
                                yield break;
                            }
                            yield return  LoadAssetManager.LoadAsset<Sprite>(v,
                              spr=>{
                              (com as SetUIImage).sprite=spr;
                              
                              });

                         }   
                    return ie();
                         }
                    ),
                new ConvertScriptToCsv("images",
                    (com,v)=>{
                        var list=(v as List<Image>);
                        if(list.Count>0)
                        {
                            return list[0].gameObject.name;
                        }
                        else
                        {
                            return "";
                        }
                    },
                    (com,v)=>{
                         IEnumerator ie()
                        {
                        yield return null;
                           if(string.IsNullOrEmpty(v))
                            {
                              yield break;
                            }

                            var target=GameObject.Find(v);
                            Image img=null;

                            if(target!=null&&target.TryGetComponent(out img))
                            {
                                (com as SetUIImage).images?.Clear();
                                (com as SetUIImage).images.Add(img);

                             }
                        }

                     return ie();

                    }),
                new ConvertScriptToCsv( "effectType") ,
                new ConvertScriptToCsv( "effectDuration",
                    null,
                    (com,v)=>{

                        IEnumerator ie()
                        {
                            if(string.IsNullOrWhiteSpace(v))
                            {
                            yield break;
                            }

                            var waitTime=float.Parse(v);
                            var tarCom=(com as SetUIImage);
                            if(waitTime>0)
                            {
                                tarCom.waitUntilFinished=true;
                                tarCom.effectDuration = waitTime;
                            }
                        }
                          return ie();
                    }
                    ) ,


            } },

        };

        /// <summary>
        /// 預設一定要展示的label
        /// </summary>
        public static List<string> DefaultCommandTitle = new List<string>()
        {
            "InstanceID",
            "BlockName",
            "CommandType"
        };
        /// <summary>
        /// 計算每個command占用的dictory num
        /// </summary>
        public struct ExportFormatCalc
        {
            /// <summary>
            /// command名稱?
            /// </summary>
            public string typeName;
            /// <summary>
            /// 占用的title
            /// </summary>
            public List<string> titleList;
            /// <summary>
            /// 計算每個command占用的dictory num
            /// </summary>
            public ExportFormatCalc(string _typeName, List<string> _titleList)
            {
                typeName = _typeName;
                titleList = _titleList;
            }

        }
        /// <summary>
        /// 該flowchart 在csv上的顯示格式
        /// </summary>
        public struct ConvertToCsvCondtions
        {
            /// <summary>
            /// 主要flowchart
            /// </summary>
            public Flowchart flowchart;
            /// <summary>
            /// flowchart的所有command
            /// </summary>
            public List<Command> comList;
            /// <summary>
            /// 該block在csv上 對應顯示的標題
            /// </summary>
            public List<ExportFormatCalc> typeSerInfo;


            public ConvertToCsvCondtions(Flowchart _flow,List<Command> _comList, List<ExportFormatCalc> _typeSerInfo)
            {
                flowchart = _flow;
                comList = _comList;
                typeSerInfo = _typeSerInfo;
             }
        }

        #region 輸出

        public static IEnumerator ExportToCsv( string path, ConvertToCsvCondtions option)
        {
            DataTable dataTab = FlowchartCommandFormatToDataTable(option);
            yield return DataTableToCsv(dataTab, option.typeSerInfo, path);
        }
        /// <summary>
        /// flowchart 數據 轉 DataTable
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        private static DataTable FlowchartCommandFormatToDataTable(ConvertToCsvCondtions option)
        {
            DataTable dataTable = new DataTable(); 
            DataTable result;


            if (option.comList.Count <= 0)
            {
                result = dataTable;
                return result;
            }

            //set default command info
            if (dataTable.Columns.Count == 0)
            {
                //先加入所有command預設的title ex blockName
                foreach (string comDefTitle in DefaultCommandTitle)
                {
                    dataTable.Columns.Add(comDefTitle, typeof(string));
                }

                foreach (var title in option.typeSerInfo) 
                {
                    foreach (var dic in title.titleList)
                    {
                        dataTable.Columns.Add(dic, typeof(string));
                    }
                }
            }
            foreach (var com in option.comList)
            {
                DataRow dataRow = dataTable.NewRow();
                /*
                Debug.Log("偵測com1=>"+com);
                Debug.Log("偵測com2=>" + com.name);
                Debug.Log("偵測com3=>" + com.GetType());

                if (com.GetType()==typeof(Say)) {

                    Debug.Log("測試1=>"+(com as Say).StoryText);
                    Debug.Log("測試2=>" + com.ParentBlock.BlockName);
                }*/

                dataRow["InstanceID"] = com.GetInstanceID();
                dataRow["BlockName"] = com.ParentBlock.BlockName;
                dataRow["CommandType"] = com.GetType().Name;

                var tarList = CommandExportToValueSettingList[com.GetType()];

                foreach (var dic in tarList)
                {
                    dataRow[com.GetType().Name + "_class_" + dic.fieldNameDisplayToCsv] = dic.GetDataString(com);
                }
                dataTable.Rows.Add(dataRow);
            }

            result = dataTable;
            return result;
        }
        /// <summary>
        /// 導出Csv
        /// </summary>
        /// <param name="table"></param>
        /// <param name="comSerInfoList"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private static IEnumerator DataTableToCsv(DataTable table, List<ExportFormatCalc> comSerInfoList, string file)
        {
            file = file + ".csv";
            /*  if (File.Exists(file))
              {
                  File.Delete(file);
              }*/
            if (table.Columns.Count <= 0)
            {
                Debug.LogError("table.Columns.Count <= 0");
                yield break;
            }

            string title = "";

            FileStream fs = new FileStream(file,System.IO.FileMode.OpenOrCreate);

            StreamWriter sw = new StreamWriter(new BufferedStream(fs), System.Text.Encoding.UTF8);

            #region 第1行
            for (int a = 0; a < DefaultCommandTitle.Count; a++)
            {
                title += "$";
            }

            // write command類別第1行  (ex: say 計算間隔 menu )  
            for (int i = 0; i < comSerInfoList.Count; i++)
            {
                ExportFormatCalc comInfo = comSerInfoList[i];
                title += comInfo.typeName + "$";
                for (int a = 0; a < (comInfo.titleList.Count - 1); a++)
                {
                    title += "$";
                }
            }

            title = title.Substring(0, title.Length - 1) + "\n";
            #endregion

            // write command類別第2行
            //write command display title name
            for (int i = 0; i < table.Columns.Count; i++)
            {
                string value = table.Columns[i].ColumnName;

                if (value.Contains("_class_"))
                {
                    //只抓_class_後面的名稱(多這個步驟是避免前面再設置table的ColumnName時參數名稱重複而導致無法設置的狀況
                    //ex: say有descript menu也有descript  ( say_class_descript )=>(descript)
                    value = value.Substring(value.IndexOf("_class_") + 7);
                }

                title += value + "$";
            }

            title = title.Substring(0, title.Length - 1) + "\n";


            yield return sw.WriteAsync(title);

            foreach (DataRow row in table.Rows)
            {
                string line = "";
                for (int i = 0; i < table.Columns.Count; i++)
                {

                    line += row[i].ToString().Trim() + "$"; //内容：自動跳到下一單元格
                }
                line = line.Substring(0, line.Length - 1) + "\n";
                yield return sw.WriteAsync(line);

            }

            sw.Close();
            fs.Close();
        }

        #endregion

        #region 輸入
        /// <summary>
        /// 透過csv以檔案覆蓋當前紀錄
        /// </summary>
        /// <returns></returns>
        public static IEnumerator FileCsvOverrideFlowChart(string data, ConvertToCsvCondtions format)
        {
            var dataArr = data.Split("\n");
            List<string> titles = new List<string>();


            titles = dataArr[1].Split("$").ToList();

            Dictionary<string, int> labelIndex = new Dictionary<string, int>();
            //設置commad在csv上各個對應的field
            for (int i = 0; i < dataArr[0].Split("$").Length; i++)
            {
                var fieldValue = dataArr[0].Split("$")[i].Trim();
                if (!string.IsNullOrWhiteSpace(fieldValue))
                {
                    labelIndex.Add(fieldValue, i);
                }
            }
            //如果是新增的command跑這 (方便記錄前後新增的command 位置 因為csv沒這些command的紀錄 不方便查閱
            Dictionary<Block, List<Command>> addCommands = new Dictionary<Block, List<Command>>();

            for (int i = 2; i < (dataArr.Length-1); i++)
            {

                Command com = null;
                string[] lastField = dataArr[(i-1)].Split("$");
                string[] fields = dataArr[i].Split("$");

                //設置data值至com
                IEnumerator SetDataToCom() {
                    Debug.Log("say的type=>" + com);
                    Debug.Log("say的type=>"+com.GetType());
                    List<ConvertScriptToCsv> fd = CommandExportToValueSettingList[com.GetType()];
                    int startIndex = labelIndex[com.GetType().Name];
                    for (int a = startIndex; a < (fd.Count + startIndex); a++)
                    {
                        string value = fields[a];
                        string convertStrField = "";

                        if (DefaultCommandTitle.Contains(titles[a]))
                        {
                            convertStrField = titles[a];
                        }
                        else
                        {
                            try
                            {
                                convertStrField = fd.First(v => v.fieldNameDisplayToCsv == titles[a].Trim()).fieldName;
                                Debug.Log("value名稱=>" + titles[a]);
                            }
                            catch
                            {
                                Debug.LogError("非體制預設內的值名稱=>" + titles[a]);
                            }
                        }

                        yield return com.SetDataToCommand(convertStrField, value);
                    }
                }

                //匯入csv後 所需要的command 狀態變化(此狀態將以字串的形式記錄在InstanceID上
                switch (fields[DefaultCommandTitle.IndexOf("InstanceID")].Split("_")[0].ToLower()) 
                {
                    case "add"://新增command
                        {
                            var blockArr = format.flowchart.GetComponents<Block>();
                            Block tarBlock = null;
                            try
                            {
                                if (blockArr.Any(v => v.BlockName == fields[DefaultCommandTitle.IndexOf("BlockName")]))
                                {
                                    tarBlock = blockArr.First(v => v.BlockName == fields[DefaultCommandTitle.IndexOf("BlockName")]);
                                }
                                else
                                {
                                    tarBlock = format.flowchart.CreateBlock(Vector2.zero);
                                }
                            }
                            catch
                            {
                                Debug.LogError("找不到BlockName=>" + fields[DefaultCommandTitle.IndexOf("BlockName")]);
                            }

                            try
                            {
                                com = tarBlock.gameObject.AddComponent(Type.GetType("Fungus." + fields[DefaultCommandTitle.IndexOf("CommandType")].Trim())) as Command;
                            }
                            catch
                            {
                                Debug.LogError("無法轉化成comand=>" + fields[DefaultCommandTitle.IndexOf("CommandType")].Trim());
                            }

                            if (addCommands.ContainsKey(tarBlock)) 
                            {
                                addCommands[tarBlock].Add(com);
                                Debug.Log("目前的數量1=>" + addCommands[tarBlock].Count()+"加入=>"+(com as Say).Character);
                            }
                            else
                            {
                                addCommands.Add(tarBlock, new List<Command>() { com });
                                Debug.Log("目前的數量2=>" + addCommands[tarBlock].Count() + "加入=>" + (com as Say).Character);
                            }

                            //設置順序 這行跟上一航的blockName是不同名稱 如果是第一行加  上一行是標題   所以也會自動歸類在第一行
                            if (fields[DefaultCommandTitle.IndexOf("BlockName")] != lastField[DefaultCommandTitle.IndexOf("BlockName")])
                            {
                                tarBlock.CommandList.Insert(0, com);
                            }
                            else
                            {
                                Debug.Log("#1測試=>"+ fields[DefaultCommandTitle.IndexOf("InstanceID")].Split("_")[0].ToLower());
                                Debug.Log("#2測試=>" + (fields[DefaultCommandTitle.IndexOf("InstanceID")].Split("_")[0].ToLower()=="add"));
                                if (lastField[DefaultCommandTitle.IndexOf("InstanceID")].Split("_")[0].ToLower()=="add")
                                {
                                    /*try
                                       {
                                           var lastCom = addCommands[tarBlock][addCommands.Count-2];
                                           tarBlock.CommandList.Insert((tarBlock.CommandList.IndexOf(lastCom) + 1), com);

                                       }
                                       catch(Exception ex)
                                       {
                                           Debug.Log("無法取得 add後的 command=>" + lastField[DefaultCommandTitle.IndexOf("InstanceID")]);
                                       }*/
                                    var lastCom = addCommands[tarBlock][addCommands[tarBlock].Count - 2];
                                    tarBlock.CommandList.Insert((tarBlock.CommandList.IndexOf(lastCom) + 1), com);
                                }
                                else
                                {
                                    /*   try
                                       {
                                           var lastCom = tarBlock.CommandList.First(v => v.GetInstanceID() == int.Parse(lastField[DefaultCommandTitle.IndexOf("InstanceID")]));
                                           tarBlock.CommandList.Insert((tarBlock.CommandList.IndexOf(lastCom) + 1), com);

                                       }
                                       catch (Exception ex)
                                       {
                                           Debug.Log("無法取得上一個command的InstanceID=>" + lastField[DefaultCommandTitle.IndexOf("InstanceID")]);
                                       }*/
                                    var lastInsID = lastField[DefaultCommandTitle.IndexOf("InstanceID")].Split("_")[0].ToLower();//因為有可能Instance前面被改成remove_
                                    switch (lastInsID)
                                    {
                                        case "remove":
                                            {
                                                var lastCom = tarBlock.CommandList.First(v => {
                                                    Debug.Log("遍歷=>"+v.GetInstanceID()+"需要找到=>"+ lastField[DefaultCommandTitle.IndexOf("InstanceID")].Split("_")[1]);
                                                    return v.GetInstanceID() == int.Parse(lastField[DefaultCommandTitle.IndexOf("InstanceID")].Split("_")[1]);

                                                    });
                                                tarBlock.CommandList.Insert((tarBlock.CommandList.IndexOf(lastCom) + 1), com);
                                            }
                                            break;
                                        default:
                                            { 
                                                var lastCom = tarBlock.CommandList.First(v => v.GetInstanceID() == int.Parse(lastInsID));
                                                tarBlock.CommandList.Insert((tarBlock.CommandList.IndexOf(lastCom) + 1), com);
                                             }
                                            break;
                                    }

                                }
                            }
                            yield return SetDataToCom();
                        }
                        break;
                    case "remove":
                        break;
                    default://覆蓋原有的command          設置每個參數的值
                        int insID = 0;
                        try
                        {
                            if (int.TryParse(fields[0], out insID))
                            {
                                com = format.comList.FirstOrDefault(v => v.GetInstanceID() == insID);
                            }
                        }
                        catch
                        {
                            Debug.LogError("找不到InstanceID編號=>" + fields[0]);
                        }
                        yield return SetDataToCom();
                        break;
                }
                
            }

            for (int i = 2; i < (dataArr.Length - 1); i++)
            {
                Command com = null;
                string[] lastField = dataArr[(i - 1)].Split("$");
                string[] fields = dataArr[i].Split("$");

                //匯入csv後 所需要的command 狀態變化(此狀態將以字串的形式記錄在InstanceID上
                switch (fields[DefaultCommandTitle.IndexOf("InstanceID")].Split("_")[0].ToLower())
                {
                    case "remove"://刪除該Command(需要設置在csv的InstanceID欄位   ex:Remove_-23658 如左圖所示,需要自己加底線
                        {

                            var removeInsIDStr = fields[DefaultCommandTitle.IndexOf("InstanceID")].Split("_")[1];
                            int removeInsID = 0;
                            if (int.TryParse(removeInsIDStr, out removeInsID))
                            {

                                var blockArr = format.flowchart.GetComponents<Block>();
                                Block tarBlock = null;
                                try
                                {
                                    tarBlock = blockArr.First(v => v.BlockName == fields[DefaultCommandTitle.IndexOf("BlockName")]);
                                }
                                catch
                                {
                                    Debug.LogWarning("找不到Block編號,InstanceID編號=>" + removeInsID);
                                    break;
                                }

                                if (tarBlock.CommandList.Any(v => v.GetInstanceID() == removeInsID))
                                {
                                    com = tarBlock.CommandList.First(v => v.GetInstanceID() == removeInsID);
                                    tarBlock.CommandList.Remove(com);
                                    format.flowchart.DestroyObj(com);
                                }
                                else
                                {
                                    Debug.Log("找不到該指令編號=>" + removeInsID);
                                }
                            }
                            else
                            {
                                Debug.LogWarning("ID無法轉換成數字=>" + removeInsIDStr);
                            }
                        }
                        break;
                }

            }

        }

        /// <summary>
        /// 透過csv生成flow的block與command
        /// </summary>
        /// <param name="data">csv文字檔</param>
        /// <param name="flow"></param>
        /// <returns></returns>
        public static IEnumerator CreateBlockOfCsvToFlowchart(string data, Flowchart flow)
        {
            flow.InitData();


            List<Block> blocks = new List<Block>();
            //指令 在csv標題上的標籤開始的位置

            string[] dataArr = data.Split("\n");
            string[] titles = dataArr[1].Split("$");
            Vector2 blockPos = Vector2.zero;

            Dictionary<string, int> labelIndex = new Dictionary<string, int>();
            //設置commad在csv上各個對應的field
            for (int i = 0; i < dataArr[0].Split("$").Length;i++)
            {
                var fieldValue = dataArr[0].Split("$")[i].Trim();
                if (!string.IsNullOrWhiteSpace(fieldValue))
                {
                    labelIndex.Add(fieldValue,i);
                }
            }

            //先生成block(通常一個block有很多重複的command
            for (int i = DefaultCommandTitle.Count(); i < (dataArr.Length - 1); i++)
            {
                string[] fields = dataArr[i].Split("$");
                Block block = null;
                if (!blocks.Any(b => b.BlockName == fields[DefaultCommandTitle.IndexOf("BlockName")]))
                {
                    block = flow.FindBlock(fields[DefaultCommandTitle.IndexOf("BlockName")]);
                    if (block == null)
                    {

                        block = flow.CreateBlock(blockPos);
                        block.BlockName = fields[DefaultCommandTitle.IndexOf("BlockName")];
                        blockPos.y += 50;
                    }

                    blocks.Add(block);
                }
            }

            flow.ScrollPos = new Vector2();

            for (int i = 2; i < (dataArr.Length - 1); i++)
            {

                string[] fields = dataArr[i].Split("$");

                Block block = null;
                Command com = null;

                if (blocks.Find(b => b.BlockName == fields[ DefaultCommandTitle.IndexOf("BlockName") ]))
                {
                    block = blocks.Find(b => b.BlockName == fields[DefaultCommandTitle.IndexOf("BlockName")]);
                }

              /*  switch (fields[1].Trim())
                {
                    case "Say":
                        com = block.AddComponent<Say>();
                        break;

                    case "Menu":
                        com = block.AddComponent<Menu>();
                        break;
                }*/

                try 
                {
                    Debug.Log("轉化成comand=>" + fields[DefaultCommandTitle.IndexOf("CommandType")].Trim());
                    //需要包含區域名稱 避免抓到同樣class不同區域名的
                    Debug.Log("有找到block=>?" + block);
                    com = block.gameObject.AddComponent(Type.GetType("Fungus."+fields[DefaultCommandTitle.IndexOf("CommandType")].Trim())) as Command;
                    com.ParentBlock = block;
                }
                catch (Exception ex)
                {
                    Debug.Log("error顯示=>"+ex);
                    Debug.LogError("無法轉化成comand=>"+ fields[DefaultCommandTitle.IndexOf("CommandType")].Trim());
                }
                List<ConvertScriptToCsv> fd = CommandExportToValueSettingList[com.GetType()];
                block.CommandList.Add(com);

                //設置到
                var startIndex = labelIndex[fields[DefaultCommandTitle.IndexOf("CommandType")]];
                for (int a = startIndex ; a < (fd.Count+startIndex); a++)
                {
                    if (string.IsNullOrWhiteSpace(fields[a])) 
                    {
                        Debug.Log("值為空=>" + fields[a]);
                        continue;
                    }
                    string convertStrValue = "";
                    if (DefaultCommandTitle.Contains(titles[a]))
                    {
                        convertStrValue = titles[a];
                    }
                    else
                    {
                        try
                        {
                            convertStrValue = fd.First(v => v.fieldNameDisplayToCsv== titles[a].Trim()).fieldName;
                        }
                        catch(Exception ex)
                        {
                            Debug.LogError("非體制預設內的值名稱=>" + titles[a]+"ErrorMsg=>" + ex);
                        }
                    }
                    yield return com.SetDataToCommand( convertStrValue, fields[a]);
                }

            }
        }

       /* /// <summary>
        /// 設置數據至Command
        /// </summary>
        /// <param name="com"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static IEnumerator SetDataToCommand( Command com, string fieldName, string value)
        {
            //   Debug.Log("類別=>" + com.GetType() + "欄名稱=>" + fieldName);

            var list = CommandExportToValueSettingList[com.GetType()];
            ConvertScriptToCsv c = list.Find(csv => csv.fieldName == fieldName);
            
            if (c==null) {
                yield break;
            }
            yield return c.GetData(value, com);

        }*/


        #endregion
        /// <summary>
        ///  匯入csv後 所需要的command 狀態變化(此狀態將以字串的形式記錄在InstanceID上
        /// </summary>
        public enum CommandStatus
        {
            Add,//增加的comand
            Remove//刪除該Command(需要設置在csv的InstanceID欄位   ex:Remove_-23658 如左圖所示,需要自己加底線

        }

    }
    /// <summary>
    /// flowchart輸出與輸入擴充腳本
    /// </summary>
     static class ExpansionCommandFuncAboutExport
    {
        /// <summary>
        /// 設置command數據至指定的fieldName (詳細設定都紀載在CommandExportToValueSettingList
        /// </summary>
        /// <param name="com"></param>
        /// <param name="fieldName">參數名稱</param>
        /// <param name="value">設定的值</param>
        /// <returns></returns>
        public static IEnumerator SetDataToCommand(this Command com, string fieldName, string value)
        {
            Debug.Log("指令=>" + com);
            Debug.Log("類別=>" + com.GetType());
            Debug.Log("欄名稱=>" + fieldName);
            var list = CommandExportToValueSettingList[com.GetType()];
            ConvertScriptToCsv c = list.Find(csv => csv.fieldName == fieldName);

            if (c == null)
            {
                yield break;
            }
            yield return c.GetData(com,value);
        }

    }
}
#endif