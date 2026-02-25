using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fungus.EditorUtils{
    //[CustomEditor(typeof(SpawnObject))]
    public class SpawnObjectEditor : CommandEditor
    {
        private SpawnObject tar;









        public override void OnEnable()
        {
            tar= (SpawnObject)target;


        }

        public override void DrawCommandGUI()
        {
            



        }


    }
}
