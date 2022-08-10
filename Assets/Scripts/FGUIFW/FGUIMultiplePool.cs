using UnityEngine;
using System.Collections.Generic;

namespace FGUIFW
{
    public class FGUIMultiplePool : Singleton<FGUIMultiplePool>
    {
        private Dictionary<string, List<FGUIBase>> pool = new Dictionary<string, List<FGUIBase>>();

        /// <summary>
        /// 回收UI
        /// </summary>
        /// <param name="fGUIBase"></param>
        public void RecycleUI(FGUIBase fGUIBase)
        {
            if (fGUIBase != null)
            {
                fGUIBase.order = 0;
                fGUIBase.transform.SetParent(FGUIManager.Ins.multiplePool);
                fGUIBase.transform.localPosition = Vector3.zero;
                fGUIBase.transform.localRotation = Quaternion.identity;
                fGUIBase.transform.localScale = Vector3.one;
                fGUIBase.gameObject.SetActive(false);

                if (pool.ContainsKey(fGUIBase.name))
                {
                    pool[fGUIBase.name].Add(fGUIBase);
                }
                else
                {
                    pool[fGUIBase.name] = new List<FGUIBase>() { fGUIBase };
                }
            }
        }
        /// <summary>
        /// 获得UI
        /// </summary>
        /// <param name="uiName"></param>
        /// <returns></returns>
        public FGUIBase GetUI(string uiName)
        {
            FGUIBase result = null;
            if (pool.ContainsKey(uiName))
            {
                if (pool[uiName].Count > 0)
                {
                    result = pool[uiName][0];
                    result.gameObject.SetActive(true);
                    pool[uiName].Remove(result);
                    return result;
                }
            }
            return null;
        }
    }
}