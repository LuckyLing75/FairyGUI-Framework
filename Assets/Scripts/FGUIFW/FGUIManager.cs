using FGUIFW;
using UnityEngine;
using System.Collections.Generic;
using System;
using FairyGUI;

public class FGUIManager : Singleton<FGUIManager>
{
    /// <summary>
    /// �̶�UI�ڵ�
    /// </summary>
    public Transform fixedNode;
    /// <summary>
    /// ��ͨUI�ڵ�
    /// </summary>
    public Transform normalNode;
    /// <summary>
    /// ����UI�ڵ�
    /// </summary>
    public Transform popupNode;
    /// <summary>
    /// ����UI�ڵ�
    /// </summary>
    public Transform topNode;
    /// <summary>
    /// ��ȡUI�ڵ�
    /// </summary>
    public Transform loadingNode;
    /// <summary>
    /// ����UI�ؽڵ�
    /// </summary>
    public Transform multiplePool;

    /// <summary>
    /// ��ǰ�̶�
    /// </summary>
    public int curFixedOrder = 1;
    /// <summary>
    /// ��ǰ��ͨ
    /// </summary>
    public int curNormalOrder = 1000;
    /// <summary>
    /// ��ǰ����
    /// </summary>
    public int curPopupOrder = 2000;
    /// <summary>
    /// ��ǰ����
    /// </summary>
    public int curTopOrder = 3000;
    /// <summary>
    /// ��ǰ����
    /// </summary>
    public int curLoadingOrder = 4000;
    /// <summary>
    /// �����
    /// </summary>
    public Camera uiCamera;

    /// <summary>
    /// ��ǰ�򿪵�UI�ֵ�
    /// </summary>
    public Dictionary<string, FGUIBase> curOpenUIDic = new Dictionary<string, FGUIBase>();
    /// <summary>
    /// ����UI�ֵ�
    /// </summary>
    public Dictionary<string, FGUIBase> allUIDic = new Dictionary<string, FGUIBase>();
    /// <summary>
    /// �Ѿ����ص���Դ�ֵ�
    /// </summary>
    public Dictionary<string, UnityEngine.Object> resourceLoadedDic = new Dictionary<string, UnityEngine.Object>();

    /// <summary>
    /// ��Դ����ί��
    /// </summary>
    public Func<string, UnityEngine.Object> loadResourceFunc;
    /// <summary>
    /// ��Դʵ����ί��
    /// </summary>
    public Func<UnityEngine.Object, GameObject> insResourceFunc;
    /// <summary>
    /// ��Դ����ί��
    /// </summary>
    public Action<GameObject> destroyResourceFunc;

    #region PUBLIC
    /// <summary>
    /// ��ʼ��������
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
            throw new Exception("UI����������Դ���غ���ΪNULL!");
        }
        if (insResourceFunc == null)
        {
            throw new Exception("UI����������Դʵ��������ΪNULL!");
        }
        if (destroyResourceFunc == null)
        {
            throw new Exception("UI����������Դ���ٺ���ΪNULL!");
        }

        this.loadResourceFunc = loadResourceFunc;
        this.insResourceFunc = insResourceFunc;
        this.destroyResourceFunc = destroyResourceFunc;

        UnityEngine.Object rootObj = this.loadResourceFunc(FGUIConst.FGUIRoot);
        if (rootObj == null)
        {
            throw new Exception("UI�������� ���ڵ�Ԥ���� ����ʧ��!");
        }
        GameObject rootGO = this.insResourceFunc(rootObj);
        if (rootGO == null)
        {
            throw new Exception("UI�������� ���ڵ�Ԥ���� ʵ����ʧ��!");
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
    /// ����ͨUI
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
    /// �ر���ͨUI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void CloseUI<T>() where T : FGUIBase
    {
        CloseFGUI(typeof(T).Name);
    }
    /// <summary>
    /// ͨ�����ֹر���ͨUI
    /// </summary>
    /// <param name="uiName"></param>
    public void CloseUIByName(string uiName)
    {
        CloseFGUI(uiName);
    }
    /// <summary>
    /// �򿪶���UI
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
            ui = InstantiateUIFromFGUIBytes<TComponent>(uiName, UIMultiple.����);
            ui.OnOpen();
        }
        return ui;
    }
    /// <summary>
    /// �رն���UI
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
    /// ����UI��Чʱ������
    /// </summary>
    /// <param name="go">���Ե�GameObject</param>
    public void IgnoreParticleTimeScale(GameObject go)
    {
        ParticleSystem[] particlesC = go.GetComponentsInChildren<ParticleSystem>();
        ParticleSystem[] particlesF = go.GetComponentsInParent<ParticleSystem>();
        foreach (var particle in particlesC)
        {
            var mainModule = particle.main;
            mainModule.useUnscaledTime = true; //�ؼ�����
        }
        foreach (var particle in particlesF)
        {
            var mainModule = particle.main;
            mainModule.useUnscaledTime = true; //�ؼ�����
        }
    }
    /// <summary>
    /// ��ͣ��Ϸ
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0;
    }
    /// <summary>
    /// ������Ϸ
    /// </summary>
    public void ContinueGame()
    {
        Time.timeScale = 1;
    }
    /// <summary>
    /// �ҵ�һ��UI�Ĺ���
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
    /// UI�رյ�ʱ����㼶
    /// </summary>
    /// <param name="fUIBase"></param>
    public void SetOrderWhenCloseUI(FGUIBase fUIBase)
    {
        if (fUIBase.uiMultiple == UIMultiple.����) return;
        List<FGUIBase> tempFuiBase = new List<FGUIBase>();

        foreach (var dic in curOpenUIDic)
        {
            if (fUIBase.UIHierarchyType == dic.Value.UIHierarchyType)// ��ͬ���UI
            {
                tempFuiBase.Add(dic.Value);// �洢�㼶��ͬ��UI
            }
        }

        for (int i = 0; i < tempFuiBase.Count; i++)
        {
            if (tempFuiBase[i].order >= fUIBase.order)// ���Order���ڹرյ�Order�ͼ�һ
            {
                tempFuiBase[i].order--;
            }
        }
        switch (fUIBase.UIHierarchyType)// �ٰѵ�ǰ��Ӧ��UI��ʾ���ͼ�һ����Ϊֻ�ر���һ��UI�����ǰ�ȫ��
        {
            case UIHierarchyType.��ͨ��:
                curNormalOrder--;
                break;
            case UIHierarchyType.�̶���:
                curFixedOrder--;
                break;
            case UIHierarchyType.������:
                curPopupOrder--;
                break;
            case UIHierarchyType.�ö���:
                curTopOrder--;
                break;
            case UIHierarchyType.���ز�:
                curLoadingOrder--;
                break;
            default:
                break;
        }
        fUIBase.order = 0;// �رյ�UIOrder����
        fUIBase.SetSortingOrder();
    }
    /// <summary>
    /// ��UI��ʱ����㼶
    /// </summary>
    /// <param name="fUIBase"></param>
    public void SetOrderWhenOpenUI(FGUIBase fUIBase)
    {
        if (fUIBase.uiMultiple == UIMultiple.����) return;
        switch (fUIBase.UIHierarchyType)
        {
            case UIHierarchyType.��ͨ��:
                fUIBase.order = ++curNormalOrder;
                break;

            case UIHierarchyType.�̶���:
                fUIBase.order = ++curFixedOrder;
                break;

            case UIHierarchyType.������:
                fUIBase.order = ++curPopupOrder;
                break;
            case UIHierarchyType.�ö���:
                fUIBase.order = ++curTopOrder;
                break;
            case UIHierarchyType.���ز�:
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
    /// ��UI
    /// </summary>
    /// <param name="fGUIBase"></param>
    private void DoOpenUI(FGUIBase fGUIBase)
    {
        if (fGUIBase == null)
        {
            throw new Exception("���Դ򿪵�UIΪNULL!");
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
    /// ʵ������ͨUI
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
            throw new Exception("���Դ�������ΪNULL��UI!");
        }

        allUIDic.TryGetValue(uiName, out FGUIBase ui);
        if (ui == null)
        {
            return InstantiateUIFromFGUIBytes<TComponents>(uiName, UIMultiple.��һ);
        }
        return ui;
    }
    /// <summary>
    /// ʵ����UI
    /// </summary>
    /// <typeparam name="TComponents">����㷺��</typeparam>
    /// <param name="uiName">UI����</param>
    /// <param name="multiple">UI��������</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private FGUIBase InstantiateUIFromFGUIBytes<TComponents>(string uiName, UIMultiple multiple) where TComponents : FGUIComsBase
    {
        if (string.IsNullOrEmpty(uiName))
        {
            throw new Exception("���Դ�������ΪNULL��UI!");
        }

        Type type = Type.GetType(uiName);
        if (type == null)
        {
            throw new Exception($"û���ҵ� {uiName} ��Ӧ���࣬��ע��UI����������Ӧ!");
        }

        var uiPackage = LoadUIFromFGUIBytes(uiName);
        if (uiPackage == null)
        {
            throw new Exception($"{uiName} FGUI��Դ������ʧ��!");
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

        if (fGUIBase.uiMultiple == UIMultiple.��һ)
        {
            switch (fGUIBase.UIHierarchyType)// ���ֲ㼶�����ڲ�ͬnode
            {
                case UIHierarchyType.��ͨ��:
                    ui.transform.SetParent(normalNode);
                    break;

                case UIHierarchyType.�̶���:
                    ui.transform.SetParent(fixedNode);
                    break;
                case UIHierarchyType.������:
                    ui.transform.SetParent(popupNode);
                    break;
                case UIHierarchyType.�ö���:
                    ui.transform.SetParent(topNode);
                    break;
                case UIHierarchyType.���ز�:
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
    /// ����UI��Ҫ����Դ
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
    /// �ر�FGUI
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