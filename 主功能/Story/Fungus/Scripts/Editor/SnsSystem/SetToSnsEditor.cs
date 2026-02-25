using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Fungus.EditorUtils
{




    [CustomEditor(typeof(SetToSns))]
    public class SetToSnsEditor : CommandEditor
    {
        private SerializedProperty SnsProp;
        private SetToSns tar = null;



        public override void OnEnable()
        {

            // base.OnEnable();
        }

        public override void DrawCommandGUI()
        {

            
            tar = (SetToSns)target;
            SnsProp = serializedObject.FindProperty("sns");

            SerializedProperty messageInfo = SnsProp.FindPropertyRelative("mMessageType");

            StartSns startSns =GetDialogStartSns();

            if (startSns&& startSns.DialogChara!=null&& startSns.DialogChara.Count>0) {
                tar.sns.mChara.Charas = startSns.DialogChara;
               // SnsProp = serializedObject.FindProperty("sns");
            }
            else
            {
                //Debug.Log("Not Setting CharaSetting");
                return;
            }

            int availableChara = startSns.DialogChara.Count;

            foreach (var chara in tar.sns.mChara.Charas) {
                if (chara.mFungusChara==null) {
                    availableChara--;
                }
            }
            if (availableChara<=0) {
                return;
            }

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(SnsProp.FindPropertyRelative("mChara"));

            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }

            // EditorGUILayout.PropertyField(messageInfo.FindPropertyRelative("_snsType"));//根據不同角色

            foreach (var chara in tar.sns.mChara.Charas)
            {

                if (chara.mFungusChara.NameText==tar.sns.mChara.mName) {
    
                    List<string> displaySnsType = new List<string>();

                    switch (chara.mCharaRole)
                    {

                        case CharaRole.self:

                            displaySnsType.Add("Message");
                            displaySnsType.Add("Reply");
                            displaySnsType.Add("OneClickReply");
                            displaySnsType.Add("Image");


                                 CommandEditor.EnumField<SnsType>(
                                messageInfo.FindPropertyRelative("_snsType"),
                                new GUIContent("SnsType", "Change Sns Type"),
                                 displaySnsType);

                            serializedObject.ApplyModifiedProperties();
                            switch (tar.sns.mMessageType._snsType)
                            {
                                case SnsType.Message:
                               //     sts.sns.targetBlock = null;

                                     EditorGUILayout.PropertyField(messageInfo.FindPropertyRelative("_message"));
                                    EditorGUILayout.PropertyField(messageInfo.FindPropertyRelative("_dialogWaitTime"));
                                    break;
                                case SnsType.OneClickReply:
                                case SnsType.Reply:
                                    var replys= messageInfo.FindPropertyRelative("_replyMessage");

                                    // EditorGUILayout.PropertyField(messageInfo.FindPropertyRelative("_replyMessage"));
                                    GUILayout.Space(10);
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("MessageCount",GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth/3));
                                    replys.arraySize = EditorGUILayout.IntField(replys.arraySize);
                                    EditorGUILayout.EndHorizontal();
                                    for (int i=0;i<replys.arraySize;i++) { 
                                   var reply= replys.GetArrayElementAtIndex(i);


                                        EditorGUILayout.PropertyField(reply.FindPropertyRelative("targetBlock"));
                                        EditorStyles.label.normal.textColor = new Color(1,0,0.3f,1);
                                        EditorGUILayout.PropertyField(reply.FindPropertyRelative("message"),new GUIContent("完整句子"));
                                        EditorStyles.label.normal.textColor = new Color(1, 0.6f, 0, 1);
                                        EditorGUILayout.PropertyField(reply.FindPropertyRelative("introduction"), new GUIContent("簡短回覆"));
                                        EditorStyles.label.normal.textColor = Color.white;

                                    }

                                    // EditorGUILayout.PropertyField(messageInfo.FindPropertyRelative("_replyMessage"));
                                    break;
                                case SnsType.Image:
                              //      sts.sns.targetBlock = null;

                                    EditorGUILayout.PropertyField(messageInfo.FindPropertyRelative("_sprite"));
                                    break;
                            }
                            break;
                        case CharaRole.otherSide:

                        //    sts.sns.targetBlock = null;

                            displaySnsType.Add("Message");
                            displaySnsType.Add("Image");
                            CommandEditor.EnumField<SnsType>(
                                messageInfo.FindPropertyRelative("_snsType"),
                                new GUIContent("SnsType", "Change Sns Type"),
                                 displaySnsType);
                            serializedObject.ApplyModifiedProperties();
                            switch (tar.sns.mMessageType._snsType)
                            {
                                case SnsType.Message:
                                    EditorGUILayout.PropertyField(messageInfo.FindPropertyRelative("_message"));
                                    EditorGUILayout.PropertyField(messageInfo.FindPropertyRelative("_dialogWaitTime"));
                                    break;
                                case SnsType.Image:
                                    EditorGUILayout.PropertyField(messageInfo.FindPropertyRelative("_sprite"));
                                    EditorGUILayout.PropertyField(messageInfo.FindPropertyRelative("_dialogWaitTime"));
                                    break;
                            }
                            break;

                    }
                }
                //根據不同的角色定位,給予不同的選項
            }
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

        }//---DrawCommandGUI()

        private StartSns GetDialogStartSns()//獲取正確的startSns  所有的command的腳本都會掛在 flowchart 這個物件底下
        {
            List<CalcStartSnsIndex> Snss = new List<CalcStartSnsIndex>();
            int mIndex = 0;//set to sns number

            for (int i=0;i< tar.ParentBlock.CommandList.Count;i++) {
                Command childSns = tar.ParentBlock.CommandList[i];


                if (childSns.GetType() == typeof(StartSns)&&childSns!=null)
                {
                    Snss.Add(new CalcStartSnsIndex( i , (StartSns)childSns));
                }
                if (childSns==tar) {
                    mIndex = i;
                
                }
            }


            List<CalcStartSnsIndex> SnssCopy = new List<CalcStartSnsIndex>(Snss);
          //  Debug.Log("1.篩選出的sns數量=>" + Snss.Count);

            foreach (var s in Snss)
            {
                if (s.index>mIndex) {
                    SnssCopy.Remove(s);
                }
            }
            //Debug.Log("2.過濾出的sns數量=>" +SnssCopy.Count);

            //sns.CommandIndex

            if (SnssCopy.Count<=0) {
            return null;
            }

            return SnssCopy[(SnssCopy.Count-1)].mSns;

        }

        private class CalcStartSnsIndex
        {
            public int index = 0;
            public StartSns mSns=null;
            public CalcStartSnsIndex(int _index,StartSns _mSns) {
                index = _index;
                mSns=_mSns;
            
            }



        }



    }





}
