using UnityEngine;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    public GameObject targetPanel;
    public GameObject[] panelsToShow;
    public GameObject[] panelsToHide;

    private void Start()
    {
        var button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(GoBack);
    }

    public void GoBack()
    {
        // 优先从导航历史弹栈还原
        if (NavHistory.Pop(out var toShow, out var toHide))
        {
            if (toHide != null)
                foreach (var p in toHide) if (p != null) p.SetActive(false);
            if (toShow != null)
                foreach (var p in toShow) if (p != null) p.SetActive(true);
        }
        else
        {
            // 栈空时走 Inspector 配置的兜底逻辑
            if (panelsToHide != null)
                foreach (var p in panelsToHide) if (p != null) p.SetActive(false);
            if (panelsToShow != null && panelsToShow.Length > 0)
                foreach (var p in panelsToShow) if (p != null) p.SetActive(true);
            else if (targetPanel != null)
                targetPanel.SetActive(true);
        }

        FindObjectOfType<BgmController>()?.CheckBgm();
    }
}
