using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap;

namespace Sxer.ThirdPart
{
    public class LeapGestureTool : MonoBehaviour
    {
        LeapProvider _lp;
        //当前帧手数量
        public enum HandState
        {
            none,
            leftHand,
            rightHand,
            bothHands
        }
        public HandState _HandState = HandState.none;
        public GestureHelper l_helper = new GestureHelper();
        public GestureHelper r_helper = new GestureHelper();
        void Start()
        {
            _lp = GetComponent<LeapProvider>();
        }


        void Update()
        {
            //获取追踪帧数据
            if (_lp != null)
                AnalyseLeapFrameData(_lp.CurrentFrame);
        }



        /// <summary>
        /// 解析leapmotion的帧数据
        /// </summary>
        /// <param name="frame_data"></param>
        private void AnalyseLeapFrameData(Frame frame_data)
        {
            //左右手判断 并更新手
            _HandState = GetHandState(frame_data.Hands);

            //手势信息更新
            l_helper.UpdateInf();
            r_helper.UpdateInf();

            //双手距离获取
            BothHandScale(frame_data);
        }



        #region 基础
        /// <summary>
        /// 当前使用手判断 并添加helper
        /// </summary>
        /// <param name="lpHands"></param>
        /// <returns></returns>
        private HandState GetHandState(List<Hand> lpHands)
        {
            if (lpHands.Count == 1)//单手
            {
                if (lpHands[0].IsLeft)
                {
                    l_helper.ChangeHandData(lpHands[0]);
                    return HandState.leftHand;
                }

                if (lpHands[0].IsRight)
                {
                    r_helper.ChangeHandData(lpHands[0]);
                    return HandState.rightHand;
                }
                l_helper.ChangeHandData(null);
                r_helper.ChangeHandData(null);
                return HandState.none;
            }
            else if (lpHands.Count == 2)//双手
            {
                foreach (var x in lpHands)
                {
                    if (x.IsLeft)
                    {
                        l_helper.ChangeHandData(x);
                    }

                    if (x.IsRight)
                    {
                        r_helper.ChangeHandData(x);
                    }
                }
                return HandState.bothHands;
            }
            //无效数据
            l_helper.ChangeHandData(null);
            r_helper.ChangeHandData(null);
            return HandState.none;
        }

        #endregion




        #region  功能
        /// <summary>
        /// 双手离远或拉近趋势
        /// </summary>
        public float HandsZoom = 0;
        /// <summary>
        /// 双手距离
        /// </summary>
        public float lrHandDistance = 0;
        /// <summary>
        /// 【双手】获取双手之间的距离
        /// </summary>
        /// <param name="lpframe"></param>
        private void BothHandScale(Frame lpframe)
        {
            if (_HandState == HandState.bothHands)
            {
                //双手不动时不计算
                if (l_helper.isStayPos && r_helper.isStayPos)
                {
                    lrHandDistance = 0;
                    HandsZoom = 0;
                }


                if (lrHandDistance == 0)
                    lrHandDistance = Vector3.Distance(l_helper.handPos, r_helper.handPos);

                float tempdis = Vector3.Distance(l_helper.handPos, r_helper.handPos);
                HandsZoom = tempdis - lrHandDistance;
                lrHandDistance = tempdis;
            }
            else
            {
                lrHandDistance = 0;
                HandsZoom = 0;
            }
        }
        #endregion
    }


    public class GestureHelper
    {
        /// <summary>
        /// 获取当前手掌位置
        /// </summary>
        public Vector3 handPos
        {
            get
            {
                if (m_hand != null)
                    return m_hand.PalmPosition.ToVector3();
                return Vector3.zero;
            }
        }

        //手掌朝向
        public Vector3 palmNormal
        {
            get
            {
                if (m_hand != null)
                    return m_hand.PalmNormal.ToVector3();
                return Vector3.zero;
            }
        }

        public float grabValue
        {
            get
            {
                if (m_hand != null)
                    return m_hand.GrabStrength;
                return -1;
            }
        }
        public int fingerExtend
        {
            get
            {
                if (m_hand != null)
                    return tt;
                return -1;
            }
        }

        public Hand _Hand
        {
            get => m_hand;
        }

        private Hand m_hand;//手部数据
        public GestureHelper() { }

        public GestureHelper(Hand hand)
        {
            m_hand = hand;
        }

        public void ChangeHandData(Hand hand)
        {
            m_hand = hand;
        }


        /// <summary>
        /// 更新手部 数据  提取  手势
        /// </summary>
        public void UpdateInf()
        {
            if (m_hand == null)
            {
                //丢失数据重置信息
                NullReset();
                return;
            }
            GetHandGesture();
        }

        //手势触发事件执行
        public delegate void GestureEventHandle();
        public delegate void GestureEventHandle<T>(T value);//带返回数据的
        public GestureEventHandle GestureEvent_Grab;//握拳
        public GestureEventHandle GestureEvent_Open;//张开
        public GestureEventHandle GestureEvent_Pinch;//捏
        public GestureEventHandle GestureEvent_Stay;//原地不动

        public GestureEventHandle GestureEvent_OnlyIndexFinger;//仅食指伸出
        public GestureEventHandle<Hand> GestureEvent_OnlyIndexFinger_BackHand;

        public GestureEventHandle GestureEvent_JianDao;

        public GestureEventHandle<float> GestureEvent_Bending;//手掌弯曲(程度)
        public GestureEventHandle<float> GestureEvent_MoveX;//x轴移动(左右)
        public GestureEventHandle<float> GestureEvent_MoveY;//y轴移动(上下)
        public GestureEventHandle<float> GestureEvent_MoveZ;//z轴移动(前后)

        /// <summary>
        /// 握拳(除大拇指外四个手指弯曲平均程度)
        /// </summary>
        public bool isGrabing = false;
        /// <summary>
        /// 捏(大拇指和其他任意手指)
        /// </summary>
        public bool isPinch = false;
        public bool isExtend = false;
        public bool isStayPos = false;
        /// <summary>
        /// 剪刀石头布("剪刀""石头""布")
        /// </summary>
        public string JSB = string.Empty;

        private int tt;//伸直手指数

        /// <summary>
        /// 姿势提取
        /// </summary>
        private void GetHandGesture()
        {
            //grab测试的是  除大拇指外的四个手指的 平均 值   （依据指向方向来）
            //position是相对于LeapProvider物体的位置(单位缩小10)
            if (m_hand.GrabStrength == 1)
            {
                isExtend = false;
                isGrabing = true;
                if (GestureEvent_Grab != null)
                    GestureEvent_Grab();
                JSB = "石头";
            }
            else if (m_hand.GrabStrength == 0)//只要不是紧握
            {
                isExtend = true;
                isGrabing = false;
                if (GestureEvent_Open != null)
                    GestureEvent_Open();
                JSB = "布";
            }
            else// 0-1之间，手指在不断弯曲
            {
                isExtend = false;
                isGrabing = false;
                JSB = string.Empty;
                if (GestureEvent_Bending != null)
                    GestureEvent_Bending(m_hand.GrabStrength);
            }

            //捏 和 握拳 会有交叉   必须用到大拇指
            if (m_hand.PinchStrength == 1)
            {
                isPinch = true;
                if (GestureEvent_Pinch != null)
                    GestureEvent_Pinch();
            }
            else
            {
                isPinch = false;
            }

            //剪刀手的判断 
            //1.先判断打开的手指数
            //2.判断是否是 中指 食指
            //
            //判断中指
            //Debug.Log(m_hand.GetMiddle().IsExtended);

            tt = 0;
            foreach (var t in m_hand.Fingers)
            {
                if (t.IsExtended)
                    tt++;
            }
            if (tt == 2)
            {
                if (m_hand.GetMiddle().IsExtended && m_hand.GetIndex().IsExtended)
                {
                    if (GestureEvent_JianDao != null)
                        GestureEvent_JianDao();
                    JSB = "剪刀";
                }
                else
                {
                    JSB = string.Empty;
                }
            }

            if (tt == 1)
            {
                if (m_hand.GetIndex().IsExtended)
                {
                    //箭头 手势
                    if (GestureEvent_OnlyIndexFinger != null)
                        GestureEvent_OnlyIndexFinger();

                    //返回手部数据
                    if (GestureEvent_OnlyIndexFinger_BackHand != null)
                        GestureEvent_OnlyIndexFinger_BackHand(m_hand);
                }
            }
            //手掌方向


            //手的【原地不动】【移动方向】
            GetMoveDir();
        }



        Vector3 oldMovePosition;
        //原地不动判断条件：手的三方向上的速度和阈值
        public static float smallestVelocity = 0.1f;
        float move_X;
        float move_Y;
        float move_Z;

        //手部移动方向获取
        private void GetMoveDir()
        {
            //原地不动
            if (m_hand.PalmVelocity.Magnitude < smallestVelocity)
            {
                if (GestureEvent_Stay != null)
                    GestureEvent_Stay();

                isStayPos = true;
                //位置重置
                oldMovePosition = Vector3.zero;
                move_X = move_Y = move_Z = 0;
            }
            else//
            {
                isStayPos = false;

                if (oldMovePosition == Vector3.zero)
                    oldMovePosition = m_hand.PalmPosition.ToVector3();

                //效果不好的话加阈值
                move_X += m_hand.PalmPosition.ToVector3().x - oldMovePosition.x;
                move_Y += m_hand.PalmPosition.ToVector3().y - oldMovePosition.y;
                move_Z += m_hand.PalmPosition.ToVector3().z - oldMovePosition.z;
                if (move_X > 0)//右移
                {
                    if (GestureEvent_MoveX != null)
                        GestureEvent_MoveX(move_X);
                    //   Debug.LogError("→");
                    //move_X = 0;
                }

                if (move_X < 0)//左移
                {
                    if (GestureEvent_MoveX != null)
                        GestureEvent_MoveX(move_X);
                    // Debug.LogError("←");
                    //move_X = 0;
                }

                if (move_Y > 0)//上
                {
                    if (GestureEvent_MoveY != null)
                        GestureEvent_MoveY(move_Y);
                    //  Debug.LogError("↑");
                }

                if (move_Y < 0)//下
                {
                    if (GestureEvent_MoveY != null)
                        GestureEvent_MoveY(move_Y);
                    // Debug.LogError("↓");
                }

                if (move_Z > 0)//前
                {
                    if (GestureEvent_MoveZ != null)
                        GestureEvent_MoveZ(move_Z);
                    //  Debug.LogError("前");
                }

                if (move_Z < 0)//后
                {
                    if (GestureEvent_MoveZ != null)
                        GestureEvent_MoveZ(move_Z);
                    //  Debug.LogError("后");
                }
                //替换当前位置
                oldMovePosition = m_hand.PalmPosition.ToVector3();
            }
        }

        private void NullReset()
        {
            //抓握
            isGrabing = false;
            isPinch = false;
            JSB = string.Empty;
            //上一帧位置
            oldMovePosition = Vector3.zero;
            //移动趋势
            move_X = move_Y = move_Z = 0;
        }



    }

}
