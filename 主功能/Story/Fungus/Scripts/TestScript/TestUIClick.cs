using System.Collections;
using System.Collections.Generic;
using UISupport;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestUIClick : MonoBehaviour
{

    [SerializeField]
    private GameObject detectTarObj=null;


    // Update is called once per frame
    void Update()
    {



      /*  if (detectTarObj!=null) 
        {
            detectTarObj.ReturnSlideResultOfGameObj(
                (pos, res) => {
                    Debug.Log("被偵測到拖曳=>" + pos);
                }, DetectTouchSlideStatus.TouchDetectStatus.Normal);
        }*/

        /*    if (Input.touchCount > 0)
            {
                EventSystem sys = EventSystem.current;
                PointerEventData point = new PointerEventData(sys);
                point.position = Input.touches[0].position;
                List<RaycastResult> rayResult = new List<RaycastResult>();
                sys.RaycastAll(point, rayResult);
                Debug.Log("rayCount=>"+rayResult.Count);



                if (rayResult.Count>0) {
                    foreach (var obj in rayResult) 
                    {
                        Debug.Log("objName=>"+obj.gameObject.name);
                    }
                }
            }*/

      /*  DetectTouchSlideStatus.ReturnSlideResult((pos, result) => {
            Debug.Log("滑鼠位置=>"+pos);
        });*/



    }
}
