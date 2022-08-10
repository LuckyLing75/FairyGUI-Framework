using System;
using FairyGUI;
using UnityEngine;
using System.Collections.Generic;

namespace FGUIFW
{
    public abstract class FGUIBase : MonoBehaviour
    {
        /// <summary>
        /// 层级
        /// </summary>
        public int order;
        /// <summary>
        /// ui名
        /// </summary>
        public string uiName;
        /// <summary>
        /// 主面板
        /// </summary>
        public UIPanel uiPanel;
        /// <summary>
        /// 多重ui状态
        /// </summary>
        public UIMultiple uiMultiple;
        /// <summary>
        /// ui特效代理
        /// </summary>
        public List<UIEffxProxy> uiEffxProxies = new List<UIEffxProxy>();

        /// <summary>
        /// UI所属组
        /// </summary>
        public abstract UIGroup UIGroup { get; }
        /// <summary>
        /// UI层级
        /// </summary>
        public abstract UIHierarchyType UIHierarchyType { get; }
        /// <summary>
        /// ui功能
        /// </summary>
        public virtual List<UIFunctional> Functionals { get; } = new List<UIFunctional>();

        /// <summary>
        /// 当实例化时执行
        /// 实例化有且只有一次
        /// </summary>
        public virtual void OnInstantiate()
        {
            if (uiMultiple == UIMultiple.单一)
            {
                uiPanel.ui.MakeFullScreen();
            }
        }
        /// <summary>
        /// 当打开时执行
        /// 每次打开都会执行
        /// </summary>
        public virtual void OnOpen()
        {
            gameObject.SetActive(true);
            if (FGUIManager.Ins.FindUIFunctional(this, UIFunctional.显示时暂停游戏))
            {
                FGUIManager.Ins.PauseGame();
            }
        }
        /// <summary>
        /// 当关闭时执行
        /// 每次关闭都会执行
        /// </summary>
        public virtual void OnClose()
        {
            gameObject.SetActive(false);
            if (FGUIManager.Ins.FindUIFunctional(this, UIFunctional.显示时暂停游戏))
            {
                if (AllPauseUIClose())
                {
                    FGUIManager.Ins.ContinueGame();
                }
            }
        }
        /// <summary>
        /// UI的Update
        /// 每帧执行
        /// </summary>
        public virtual void OnUpdate()
        {

        }

        #region PUBLIC & Protected
        /// <summary>
        /// 设置UI特效
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="effxName"></param>
        /// <param name="ignoreParticleTimeScale"></param>
        public void SetUIEffx(GGraph graph, string effxName, bool ignoreParticleTimeScale = false)
        {
            if (graph == null) return;
            GoWrapper existWrapper = FindGraphGoWrapper(graph);// 找到已经存在的wrapper
            if (existWrapper != null)// 找到了
            {
                SetUIEffxObj(graph, existWrapper, effxName);
            }
            else
            {
                GoWrapper newWrapper = NewGraphGoWrapper(graph);
                SetUIEffxObj(graph, newWrapper, effxName);
            }
            void SetUIEffxObj(GGraph graph, GoWrapper wrapper, string effxName)
            {
                var go = FGUIManager.Ins.loadResourceFunc(effxName);
                var effx = FGUIManager.Ins.insResourceFunc(go);
                if (effx != null)
                {
                    EndUIEffx(graph);
                    if (!ignoreParticleTimeScale) FGUIManager.Ins.IgnoreParticleTimeScale(effx);
                    wrapper.wrapTarget = effx;
                    graph.SetNativeObject(wrapper);
                }
            }
        }
        /// <summary>
        /// 关闭UI特效
        /// </summary>
        /// <param name="gGraph"></param>
        public void EndUIEffx(GGraph gGraph)
        {
            if (gGraph == null) return;
            GoWrapper data = FindGraphGoWrapper(gGraph);
            if (data != null && data.wrapTarget != null)
            {
                FGUIManager.Ins.destroyResourceFunc(data.wrapTarget);
                data.wrapTarget = null;
            }
        }
        /// <summary>
        /// 设置层级
        /// </summary>
        public void SetSortingOrder()
        {
            uiPanel.SetSortingOrder(this.order, true);
        }


        /// <summary>
        /// 关闭UI
        /// </summary>
        protected void CloseThisUI()
        {
            FGUIManager.Ins.CloseUIByName(uiName);
        }
        /// <summary>
        /// 初始化FGUI组件层
        /// </summary>
        /// <typeparam name="TComs"></typeparam>
        /// <returns></returns>
        protected TComs InitFGUIComs<TComs>() where TComs : FGUIComsBase
        {
            var coms = Activator.CreateInstance(Type.GetType($"{uiName}Coms")) as TComs;
            coms.Init(uiPanel.ui);
            return coms;
        }
        #endregion

        #region PRIVATE
        /// <summary>
        /// 新建一个图像包装器
        /// </summary>
        /// <param name="gGraph"></param>
        /// <returns></returns>
        private GoWrapper NewGraphGoWrapper(GGraph gGraph)
        {
            GoWrapper wrapper = new GoWrapper();
            uiEffxProxies.Add(new UIEffxProxy(gGraph, wrapper));
            return wrapper;
        }
        /// <summary>
        /// 找到一个图像包装器
        /// </summary>
        /// <param name="graph"></param>
        /// <returns></returns>
        private GoWrapper FindGraphGoWrapper(GGraph graph)
        {
            for (int i = 0; i < uiEffxProxies.Count; i++)
            {
                if (uiEffxProxies[i].gGraph == graph)
                {
                    return uiEffxProxies[i].goWrapper;
                }
            }
            return null;
        }
        /// <summary>
        /// 所有包含暂停功能的UI都已经关闭
        /// </summary>
        /// <returns></returns>
        private bool AllPauseUIClose()
        {
            foreach (var dic in FGUIManager.Ins.curOpenUIDic)
            {
                if (FGUIManager.Ins.FindUIFunctional(dic.Value, UIFunctional.显示时暂停游戏) && dic.Value != this)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}