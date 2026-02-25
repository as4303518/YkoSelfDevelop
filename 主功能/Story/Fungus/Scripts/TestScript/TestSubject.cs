using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public static class TestSubject 
{

    public static Subject<SendTest> adjustMain=new Subject<SendTest>();


}


public class SendTest
{
    public string str = "";
    public int num = 5;

}
