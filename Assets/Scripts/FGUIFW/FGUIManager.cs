using FGUIFW;
using UnityEngine;
using System.Collections.Generic;
using System;
using FairyGUI;

public class FGUIManager : Singleton<FGUIManager>
{
    /// <summary>
    /// 固定UI节点
    /// </summary>
    public Transform fixedNode;
    /// <summary>
    /// 普通UI节点
    /// </summary>
    public Transform normalNode;
    /// <summary>
    /// 弹出UI节点
    /// </summary>
    public Transform popupNode;
    /// <summary>
    /// 顶级UI节点
    /// </summary>
    public Transform topNode;
    /// <summary>
    /// 读取UI节点
    /// </summary>
    public Transform loadingNode;
    /// <summary>
    /// 多重UI池节点
    /// </summary>
    public Transform multiplePool;

    /// <summary>
    /// 当前固定
    /// </summary>
    public int curFixedOrder = 1;
    /// <summary>
    /// 当前普通
    /// </summary>
    public int curNormalOrder = 1000;
    /// <summary>
    /// 当前弹出
    /// </summary>
    public int curPopupOrder = 2000;
    /// <summary>
    /// 当前顶层
    /// </summary>
    public int curTopOrder = 3000;
    /// <summary>
    /// 当前加载
    /// </summary>
    public int curLoadingOrder = 4000;
    /// <summary>
    /// 摄像机
    /// </summary>
    public Camera uiCamera;

    /// <summary>
    /// 当前打开的UI字典
    /// </summary>
    public Dictionary<string, FGUIBase> curOpenUIDic = new Dictionary<string, FGUIBase>();
    /// <summary>
    /// 所有UI字典
    /// </summary>
    public Dictionary<string, FGUIBase> allUIDic = new Dictionary<string, FGUIBase>();
    /// <summary>
    /// 已经加载的资源字典
    /// </summary>
    public Dictionary<string, UnityEngine.Object> resourceLoadedDic = new Dictionary<string, UnityEngine.Object>();

    /// <summary>
    /// 资源加载委托
    /// </summary>
    public Func<string, UnityEngine.Object> loadResourceFunc;
    /// <summary>
    /// 资源实例化委托
    /// </summary>
    public Func<UnityEngine.Object, GameObject> insResourceFunc;
    /// <summary>
    /// 资源销毁委托
    /// </summary>
    public Action<GameObject> destroyResourceFunc;

    #region PUBLIC
    /// <summary>
    /// 初始化管理器
    /// </summary>
    /// <param name="loadResourceFunc"></param>
    /// <param name="insResourceFunc"></param>
    /// <param name="destroyResourceFunc"></param>
    /// <exception cref="Exception"></exception>
    public void InitManager(Func<string, UnityEngine.Object> loadResourceFunc,
        Func<UnityEngine.Object, GameObject> insResourceFunc,
        Action<GameObject> destroyResourceFunc)
    {
        if (loadResourceFunc == null)
        {
            throw new Exception("UI框架所需的资源加载函数为NULL!");
        }
        if (insResourceFunc == null)
        {
            throw new Exception("UI框架所需的资源实例化函数为NULL!");
        }
        if (destroyResourceFunc == null)
        {
            throw new Exception("UI框架所需的资源销毁函数为NULL!");
        }

        this.loadResourceFunc = loadResourceFunc;
        this.insResourceFunc = insResourceFunc;
        this.destroyResourceFunc = destroyResourceFunc;

        UnityEngine.Object rootObj = this.loadResourceFunc(FGUIConst.FGUIRoot);
        if (rootObj == null)
        {
            throw new Exception("UI框架所需的 根节点预制体 加载失败!");
        }
        GameObject rootGO = this.insResourceFunc(rootObj);
        if (rootGO == null)
        {
            throw new Exception("UI框架所需的 根节点预制体 实例化失败!");
        }

        fixedNode = rootGO.transform.Find(FGUIConst.FixedNode);
        normalNode = rootGO.transform.Find(FGUIConst.NormalNode);
        popupNode = rootGO.transform.Find(FGUIConst.PopNode);
        topNode = rootGO.transform.Find(FGUIConst.TopNode);
        loadingNode = rootGO.transform.Find(FGUIConst.LoadingNode);
        uiCamera = rootGO.transform.Find(FGUIConst.FUICamera).GetComponent<Camera>();
        multiplePool = rootGO.transform.Find(FGUIConst.MultiplePool);

        UnityEngine.Object.DontDestroyOnLoad(rootGO);
    }
    /// <summary>
    /// 打开普通UI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TComponent"></typeparam>
    public FGUIBase OpenUI<T, TComponent>() where T : FGUIBase where TComponent : FGUIComsBase
    {
        var uiName = typeof(T).Name;
        var ui = InstantiateUI<TComponent>(uiName);
        DoOpenUI(ui);
        return ui;
    }
    /// <summary>
    /// 关闭普通UI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void CloseUI<T>() where T : FGUIBase
    {
        CloseFGUI(typeof(T).Name);
    }
    /// <summary>
    /// 通过名字关闭普通UI
    /// </summary>
    /// <param name="uiName"></param>
    public void CloseUIByName(string uiName)
    {
        CloseFGUI(uiName);
    }
    /// <summary>
    /// 打开多重UI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TComponent"></typeparam>
    /// <returns></returns>
    public FGUIBase OpenMultipleUI<T, TComponent>() where T : FGUIBase where TComponent : FGUIComsBase
    {
        string uiName = typeof(T).Name;
        FGUIBase ui = FGUIMultiplePool.Ins.GetUI(uiName);
        if (ui != null)
        {
            ui.OnOpen();
        }
        else
        {
            ui = InstantiateUIFromFGUIBytes<TComponent>(uiName, UIMultiple.多重);
            ui.OnOpen();
        }
        return ui;
    }
    /// <summary>
    /// 关闭多重UI
    /// </summary>
    /// <param name="multipleFGUI"></param>
    public void CloseMultipleUI(FGUIBase multipleFGUI)
    {
        if (multipleFGUI != null)
        {
            multipleFGUI.OnClose();
            FGUIMultiplePool.Ins.RecycleUI(multipleFGUI);
        }
    }
    /// <summary>
    /// 忽略UI特效时间缩放
    /// </summary>
    /// <param name="go">忽略的GameObject</param>
    public void IgnoreParticleTimeScale(GameObject go)
    {
        ParticleSystem[] particlesC = go.GetComponentsInChildren<ParticleSystem>();
        ParticleSystem[] particlesF = go.GetComponentsInParent<ParticleSystem>();
        foreach (var particle in particlesC)
        {
            var mainModule = particle.main;
            mainModule.useUnscaledTime = true; //关键代码
        }
        foreach (var particle in particlesF)
        {
            var mainModule = particle.main;
            mainModule.useUnscaledTime = true; //关键代码
        }
    }
    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0;
    }
    /// <summary>
    /// 继续游戏
    /// </summary>
    public void ContinueGame()
    {
        Time.timeScale = 1;
    }
    /// <summary>
    /// 找到一个UI的功能
    /// </summary>
    /// <param name="uIFunctional"></param>
    /// <returns></returns>
    public bool FindUIFunctional(FGUIBase fGUIBase, UIFunctional uIFunctional)
    {
        for (int i = 0; i < fGUIBase.Functionals.Count; i++)
        {
            if (fGUIBase.Functionals[i] == uIFunctional)
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// UI关闭的时候处理层级
    /// </summary>
    /// <param name="fUIBase"></param>
    public void SetOrderWhenCloseUI(FGUIBase fUIBase)
    {
        if (fUIBase.uiMultiple == UIMultiple.多重) return;
        List<FGUIBase> tempFuiBase = new List<FGUIBase>();

        foreach (var dic in curOpenUIDic)
        {
            if (fUIBase.UIHierarchyType == dic.Value.UIHierarchyType)// 相同层的UI
            {
                tempFuiBase.Add(dic.Value);// 存储层级相同的UI
            }
        }

        for (int i = 0; i < tempFuiBase.Count; i++)
        {
            if (tempFuiBase[i].order >= fUIBase.order)// 如果Order大于关闭的Order就减一
            {
                tempFuiBase[i].order--;
            }
        }
        switch (fUIBase.UIHierarchyType)// 再把当前对应的UI显示类型减一，因为只关闭了一个UI所以是安全的
        {
            case UIHierarchyType.普通层:
                curNormalOrder--;
                break;
            case UIHierarchyType.固定层:
                curFixedOrder--;
                break;
            case UIHierarchyType.弹出层:
                curPopupOrder--;
                break;
            case UIHierarchyType.置顶层:
                curTopOrder--;
                break;
            case UIHierarchyType.加载层:
                curLoadingOrder--;
                break;
            default:
                break;
        }
        fUIBase.order = 0;// 关闭的UIOrder置零
        fUIBase.SetSortingOrder();
    }
    /// <summary>
    /// 打开UI的时候处理层级
    /// </summary>
    /// <param name="fUIBase"></param>
    public void SetOrderWhenOpenUI(FGUIBase fUIBase)
    {
        if (fUIBase.uiMultiple == UIMultiple.多重) return;
        switch (fUIBase.UIHierarchyType)
        {
            case UIHierarchyType.普通层:
                fUIBase.order = ++curNormalOrder;
                break;

            case UIHierarchyType.固定层:
                fUIBase.order = ++curFixedOrder;
                break;

            case UIHierarchyType.弹出层:
                fUIBase.order = ++curPopupOrder;
                break;
            case UIHierarchyType.置顶层:
                fUIBase.order = ++curTopOrder;
                break;
            case UIHierarchyType.加载层:
                fUIBase.order = ++curLoadingOrder;
                break;
            default:
                break;
        }
        fUIBase.SetSortingOrder();
    }
    #endregion

    #region PRIVATE
    /// <summary>
    /// 打开UI
    /// </summary>
    /// <param name="fGUIBase"></param>
    private void DoOpenUI(FGUIBase fGUIBase)
    {
        if (fGUIBase == null)
        {
            throw new Exception("尝试打开的UI为NULL!");
        }
        FGUIBase result = null;
        curOpenUIDic.TryGetValue(fGUIBase.uiName, out result);
        if (result == null)
        {
            curOpenUIDic[fGUIBase.uiName] = fGUIBase;
            SetOrderWhenOpenUI(fGUIBase);
            fGUIBase.OnOpen();
        }
    }
    /// <summary>
    /// 实例化普通UI
    /// </summary>
    /// <typeparam name="TComponents"></typeparam>
    /// <param name="uiName"></param>
    /// <param name="multiple"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private FGUIBase InstantiateUI<TComponents>(string uiName) where TComponents : FGUIComsBase
    {
        if (string.IsNullOrEmpty(uiName))
        {
            throw new Exception("尝试创建名字为NULL的UI!");
        }

        allUIDic.TryGetValue(uiName, out FGUIBase ui);
        if (ui == null)
        {
            return InstantiateUIFromFGUIBytes<TComponents>(uiName, UIMultiple.单一);
        }
        return ui;
    }
    /// <summary>
    /// 实例化UI
    /// </summary>
    /// <typeparam name="TComponents">组件层泛型</typeparam>
    /// <param name="uiName">UI名字</param>
    /// <param name="multiple">UI多重类型</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private FGUIBase InstantiateUIFromFGUIBytes<TComponents>(string uiName, UIMultiple multiple) where TComponents : FGUIComsBase
    {
        if (string.IsNullOrEmpty(uiName))
        {
            throw new Exception("尝试创建名字为NULL的UI!");
        }

        Type type = Type.GetType(uiName);
        if (type == null)
        {
            throw new Exception($"没有找到 {uiName} 对应的类，请注意UI名和类名对应!");
        }

        var uiPackage = LoadUIFromFGUIBytes(uiName);
        if (uiPackage == null)
        {
            throw new Exception($"{uiName} FGUI资源包加载失败!");
        }

        var ui = new GameObject(uiName).AddComponent(type);
        UIPanel uiPanel = null;
        List<PackageItem> uiItems = uiPackage.GetItems();
        for (int i = 0; i < uiItems.Count; i++)
        {
            if (uiItems[i].objectType == ObjectType.Component)
            {
                uiPanel = new GameObject("UIPanel").AddComponent<UIPanel>();
                uiPanel.packageName = uiPackage.name;
                uiPanel.componentName = uiItems[i].name;
                uiPanel.container.fairyBatching = true;
                uiPanel.container.renderMode = RenderMode.ScreenSpaceCamera;
                uiPanel.container.renderCamera = uiCamera;
                uiPanel.CreateUI();
                uiPanel.transform.SetParent(ui.transform);
                break;
            }
        }

        FGUIBase fGUIBase = ui.GetComponent<FGUIBase>();
        fGUIBase.uiName = uiName;
        fGUIBase.uiPanel = uiPanel;
        fGUIBase.uiMultiple = multiple;
        fGUIBase.gameObject.SetActive(false);

        if (fGUIBase.uiMultiple == UIMultiple.单一)
        {
            switch (fGUIBase.UIHierarchyType)// 区分层级，放在不同node
            {
                case UIHierarchyType.普通层:
                    ui.transform.SetParent(normalNode);
                    break;

                case UIHierarchyType.固定层:
                    ui.transform.SetParent(fixedNode);
                    break;
                case UIHierarchyType.弹出层:
                    ui.transform.SetParent(popupNode);
                    break;
                case UIHierarchyType.置顶层:
                    ui.transform.SetParent(topNode);
                    break;
                case UIHierarchyType.加载层:
                    ui.transform.SetParent(loadingNode);
                    break;
                default:
                    break;
            }
            ui.gameObject.layer = LayerMask.NameToLayer("UI");
            allUIDic[uiName] = fGUIBase;
        }
        else
        {
            fGUIBase.transform.SetParent(null);
        }

        fGUIBase.OnInstantiate();
        return fGUIBase;
    }
    /// <summary>
    /// 加载UI需要的资源
    /// </summary>
    /// <param name="uiName"></param>
    private UIPackage LoadUIFromFGUIBytes(string uiName)
    {
        return UIPackage.AddPackage(uiName, Load);

        object Load(string name, string extension, Type type, out DestroyMethod destroyMethod)
        {
            destroyMethod = DestroyMethod.None;
            resourceLoadedDic.TryGetValue(uiName, out UnityEngine.Object res);
            if (res != null) return res;

            if (type == typeof(TextAsset))
            {
                return LoadAsset<TextAsset>(name);
            }
            else if (type == typeof(Texture))
            {
                return LoadAsset<Texture>(name);
            }
            return null;
        }

        T LoadAsset<T>(string resName) where T : UnityEngine.Object
        {
            T asset = loadResourceFunc(resName) as T;
            if (asset != null)
            {
                resourceLoadedDic[resName] = asset;
            }
            return asset;
        }
    }
    /// <summary>
    /// 关闭FGUI
    /// </summary>
    private void CloseFGUI(string name)
    {
        curOpenUIDic.TryGetValue(name, out FGUIBase value);
        if (value != null)
        {
            SetOrderWhenCloseUI(value);
            value.OnClose();
            curOpenUIDic.Remove(name);
        }
    }
    #endregion
}