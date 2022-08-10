using FairyGUI;

namespace FGUIFW
{
    /// <summary>
    /// UI层级类型
    /// </summary>
    public enum UIHierarchyType
    {
        固定层 = 0,
        普通层,
        弹出层,
        置顶层,
        加载层
    }
    /// <summary>
    /// 是否是多重Ui
    /// </summary>
    public enum UIMultiple
    {
        单一,
        多重
    }
    /// <summary>
    /// UI功能
    /// </summary>
    public enum UIFunctional
    {
        显示时暂停游戏,
        异形屏适配
    }
    /// <summary>
    /// UI组
    /// </summary>
    public enum UIGroup
    {
        主界面,
        战斗界面,
    }

    /// <summary>
    /// UI框架使用的一些常量
    /// </summary>
    public static class FGUIConst
    {
        /// <summary>
        /// UI父物体的预制体
        /// </summary>
        public const string FGUIRoot = "FGUIRoot";
        /// <summary>
        /// 固定节点
        /// </summary>
        public const string FixedNode = "FixedNode";
        /// <summary>
        /// 普通节点
        /// </summary>
        public const string NormalNode = "NormalNode";
        /// <summary>
        /// 弹出节点
        /// </summary>
        public const string PopNode = "PopNode";
        /// <summary>
        /// 顶级节点
        /// </summary>
        public const string TopNode = "TopNode";
        /// <summary>
        /// 加载节点
        /// </summary>
        public const string LoadingNode = "LoadingNode";
        /// <summary>
        /// 摄像机
        /// </summary>
        public const string FUICamera = "Stage Camera";
        /// <summary>
        /// 多重UI池
        /// </summary>
        public const string MultiplePool = "MultiplePool";
    }

    /// <summary>
    /// UI特效代理
    /// </summary>
    public struct UIEffxProxy
    {
        public GGraph gGraph;
        public GoWrapper goWrapper;

        public UIEffxProxy(GGraph gGraph, GoWrapper goWrapper)
        {
            this.gGraph = gGraph;
            this.goWrapper = goWrapper;
        }
    }

    public abstract class Singleton<T> where T : class, new()
    {
        private static T _instance;
        private static readonly object syslock = new object();

        public static T Ins
        {
            get
            {
                if (_instance == null)
                {
                    lock (syslock)
                    {
                        if (_instance == null)
                        {
                            _instance = new T();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}