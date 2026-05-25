using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通用页面跳转脚本：隐藏一批面板，显示另一批面板
/// </summary>
public class NavigateToPanel : MonoBehaviour
{
    public GameObject[] panelsToShow; // 要显示的面板（容器和内容页放一起，按外层→内层顺序）
    public GameObject[] panelsToHide; // 要隐藏的面板

    private void Start()
    {
        var button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(Navigate);
    }

    public void Navigate()
    {
        // 只记录当前真正激活的面板，防止返回时恢复本已隐藏的
        var activeHides = panelsToHide != null
            ? System.Array.FindAll(panelsToHide, p => p != null && p.activeInHierarchy)
            : null;
        NavHistory.Push(panelsToShow, activeHides);

        if (panelsToHide != null)
            foreach (var p in panelsToHide)
                if (p != null) p.SetActive(false);

        if (panelsToShow != null)
            foreach (var p in panelsToShow)
                if (p != null) p.SetActive(true);

        FindObjectOfType<BgmController>()?.CheckBgm();
    }
}
