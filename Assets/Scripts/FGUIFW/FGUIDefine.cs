using FairyGUI;

namespace FGUIFW
{
    /// <summary>
    /// UI�㼶����
    /// </summary>
    public enum UIHierarchyType
    {
        �̶��� = 0,
        ��ͨ��,
        ������,
        �ö���,
        ���ز�
    }
    /// <summary>
    /// �Ƿ��Ƕ���Ui
    /// </summary>
    public enum UIMultiple
    {
        ��һ,
        ����
    }
    /// <summary>
    /// UI����
    /// </summary>
    public enum UIFunctional
    {
        ��ʾʱ��ͣ��Ϸ,
        ����������
    }
    /// <summary>
    /// UI��
    /// </summary>
    public enum UIGroup
    {
        ������,
        ս������,
    }

    /// <summary>
    /// UI���ʹ�õ�һЩ����
    /// </summary>
    public static class FGUIConst
    {
        /// <summary>
        /// UI�������Ԥ����
        /// </summary>
        public const string FGUIRoot = "FGUIRoot";
        /// <summary>
        /// �̶��ڵ�
        /// </summary>
        public const string FixedNode = "FixedNode";
        /// <summary>
        /// ��ͨ�ڵ�
        /// </summary>
        public const string NormalNode = "NormalNode";
        /// <summary>
        /// �����ڵ�
        /// </summary>
        public const string PopNode = "PopNode";
        /// <summary>
        /// �����ڵ�
        /// </summary>
        public const string TopNode = "TopNode";
        /// <summary>
        /// ���ؽڵ�
        /// </summary>
        public const string LoadingNode = "LoadingNode";
        /// <summary>
        /// �����
        /// </summary>
        public const string FUICamera = "Stage Camera";
        /// <summary>
        /// ����UI��
        /// </summary>
        public const string MultiplePool = "MultiplePool";
    }

    /// <summary>
    /// UI��Ч����
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