using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Sxer.ThirdPart
{
    public class LeapMotionHelper : MonoBehaviour
    {
        public static string leapmotionExePath;
        public static void StartLeapMotionServer(string lpmExePath)
        {
            //leapmotion 设备追踪服务启动
            if (!FindLeapService())
            {
                Debug.LogError("UltraLeap Tracking Service未启动!");
                //手势路径
                leapmotionExePath = lpmExePath;
                if (!File.Exists(leapmotionExePath))
                {
                    Debug.LogError("未找到UltraLeap Tracking Service路径!");
                }
                else
                {
                    Debug.Log("启动手势追踪服务。");
                    Application.OpenURL(leapmotionExePath);
                }
            }
        }


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }


        //找leapmotion服务进程
        private static bool FindLeapService()
        {
            System.Diagnostics.Process[] pro = System.Diagnostics.Process.GetProcesses();//获取已开启的所有进程
                                                                                         //遍历所有查找到的进程
            for (int i = 0; i < pro.Length; i++)
            {
                //判断此进程是否是要查找的进程
                if (pro[i].ProcessName.ToString().ToLower() == "leapsvc")
                {
                    return true;
                }
            }
            return false;
        }
    }
}

