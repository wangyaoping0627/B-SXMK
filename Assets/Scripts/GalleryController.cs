using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GalleryController : MonoBehaviour
{
    public Image largeDisplay;
    public Image[] thumbnailImages;
    public Sprite[] thumbnailSprites;
    public GameObject fullscreenOverlay;
    public Image fullscreenImage;
    public Button overlayBgBtn;
    public GameObject backButton;
    public float cycleInterval = 3f;
    public float transitionDuration = 0.3f;

    private bool isFullscreen;
    private bool isTransitioning;
    private int currentIndex = -1;
    private float cycleTimer;
    private Coroutine transitionRoutine;

    private void Awake()
    {
        var sr = GetComponentInChildren<ScrollRect>();
        if (sr == null)
            sr = CreateScrollView();

        var c = sr.content;

        if (c != null && (thumbnailImages == null || thumbnailImages.Length == 0))
        {
            if (c.childCount == 0 && thumbnailSprites != null && thumbnailSprites.Length > 0)
            {
                for (int i = 0; i < thumbnailSprites.Length; i++)
                    CreateThumbnail(c, thumbnailSprites[i]);
            }
            thumbnailImages = new Image[c.childCount];
            for (int i = 0; i < c.childCount; i++)
                thumbnailImages[i] = c.GetChild(i).GetComponent<Image>();
        }

        if (largeDisplay == null)
            largeDisplay = CreateLargeDisplay();

        if (fullscreenOverlay == null)
            CreateFullscreenOverlay();
    }

    private void Start()
    {
        for (int i = 0; i < thumbnailImages.Length; i++)
        {
            int captured = i;
            var img = thumbnailImages[i];
            if (img == null) continue;
            var btn = img.GetComponent<Button>();
            if (btn == null) btn = img.gameObject.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnThumbnailClicked(captured));
        }

        if (largeDisplay != null)
        {
            var btn = largeDisplay.GetComponent<Button>();
            if (btn == null) btn = largeDisplay.gameObject.AddComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnLargeDisplayClicked);
        }

        if (overlayBgBtn != null)
            overlayBgBtn.onClick.AddListener(CloseFullscreen);
        if (fullscreenOverlay != null)
            fullscreenOverlay.SetActive(false);
    }

    private void OnEnable()
    {
        if (fullscreenOverlay != null)
            fullscreenOverlay.SetActive(false);
        isFullscreen = false;
        isTransitioning = false;
        cycleTimer = 0f;
        currentIndex = -1;

        if (largeDisplay != null)
        {
            largeDisplay.transform.localScale = Vector3.one;
            largeDisplay.sprite = null;
        }

        if (thumbnailSprites != null && thumbnailSprites.Length > 0)
        {
            currentIndex = 0;
            if (largeDisplay != null)
            {
                largeDisplay.sprite = thumbnailSprites[0];
                largeDisplay.transform.localScale = Vector3.one;
            }
        }

        FindObjectOfType<BgmController>()?.CheckBgm();
    }

    private void OnDisable()
    {
        if (fullscreenOverlay != null)
            fullscreenOverlay.SetActive(false);
        isFullscreen = false;
    }

    private void Update()
    {
        if (isFullscreen || isTransitioning) return;
        if (thumbnailSprites == null || thumbnailSprites.Length == 0) return;

        cycleTimer += Time.deltaTime;
        if (cycleTimer >= cycleInterval)
        {
            cycleTimer = 0f;
            int next = (currentIndex + 1) % thumbnailSprites.Length;
            if (next != currentIndex)
                TransitionTo(next);
        }
    }

    private void TransitionTo(int index)
    {
        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);
        transitionRoutine = StartCoroutine(TransitionRoutine(index));
    }

    private IEnumerator TransitionRoutine(int targetIndex)
    {
        isTransitioning = true;
        var t = largeDisplay != null ? largeDisplay.transform : null;
        float half = transitionDuration * 0.5f;

        // shrink out
        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.Lerp(1f, 0.6f, elapsed / half);
            if (t != null) t.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        if (t != null) t.localScale = new Vector3(0.6f, 0.6f, 1f);

        // swap
        currentIndex = targetIndex;
        if (largeDisplay != null)
            largeDisplay.sprite = thumbnailSprites[targetIndex];

        // grow in
        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.Lerp(0.6f, 1f, elapsed / half);
            if (t != null) t.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        if (t != null) t.localScale = Vector3.one;

        isTransitioning = false;
        transitionRoutine = null;
    }

    private void OnThumbnailClicked(int index)
    {
        if (thumbnailSprites != null && index >= 0 && index < thumbnailSprites.Length)
        {
            cycleTimer = 0f;
            if (index != currentIndex)
                TransitionTo(index);
            ShowFullscreen(thumbnailSprites[index]);
        }
    }

    private void OnLargeDisplayClicked()
    {
        if (thumbnailSprites != null && currentIndex >= 0 && currentIndex < thumbnailSprites.Length)
            ShowFullscreen(thumbnailSprites[currentIndex]);
    }

    private void ShowFullscreen(Sprite sprite)
    {
        if (fullscreenOverlay == null || fullscreenImage == null) return;
        fullscreenImage.sprite = sprite;
        fullscreenOverlay.SetActive(true);
        isFullscreen = true;
        if (backButton != null) backButton.SetActive(false);
    }

    private void CloseFullscreen()
    {
        if (fullscreenOverlay != null)
            fullscreenOverlay.SetActive(false);
        isFullscreen = false;
        cycleTimer = 0f;
        if (backButton != null) backButton.SetActive(true);
    }

    private ScrollRect CreateScrollView()
    {
        var go = new GameObject("Scroll View", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        go.transform.SetParent(transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0.38f);
        rt.sizeDelta = Vector2.zero;
        go.GetComponent<Image>().color = new Color(1, 1, 1, 0);

        var vpGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        vpGo.transform.SetParent(go.transform, false);
        var vpRt = vpGo.GetComponent<RectTransform>();
        vpRt.anchorMin = Vector2.zero;
        vpRt.anchorMax = Vector2.one;
        vpRt.sizeDelta = Vector2.zero;
        vpGo.GetComponent<Image>().color = Color.white;

        var contentGo = new GameObject("Content", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        contentGo.transform.SetParent(vpGo.transform, false);
        var contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 0);
        contentRt.anchorMax = new Vector2(0, 1);
        contentRt.pivot = new Vector2(0, 0.5f);
        contentRt.sizeDelta = new Vector2(0, 0);
        var hlg = contentGo.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 300;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        var csf = contentGo.GetComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        var sr = go.GetComponent<ScrollRect>();
        sr.content = contentRt;
        sr.viewport = vpRt;
        sr.horizontal = true;
        sr.vertical = false;
        sr.movementType = ScrollRect.MovementType.Elastic;

        return sr;
    }

    private void CreateThumbnail(Transform content, Sprite sprite)
    {
        var go = new GameObject("Thumbnail", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(content, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 260);
    }

    private Image CreateLargeDisplay()
    {
        var go = new GameObject("LargeDisplayImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.38f);
        rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = new Vector2(20, 10);
        rt.offsetMax = new Vector2(-20, -10);
        var img = go.GetComponent<Image>();
        img.color = Color.white;
        img.preserveAspect = true;
        return img;
    }

    private void CreateFullscreenOverlay()
    {
        var overlayGo = new GameObject("FullscreenOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        var canvas = GetComponentInParent<Canvas>().rootCanvas;
        overlayGo.transform.SetParent(canvas.transform, false);
        var overlayRt = overlayGo.GetComponent<RectTransform>();
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.sizeDelta = Vector2.zero;
        var overlayImg = overlayGo.GetComponent<Image>();
        overlayImg.color = new Color(0, 0, 0, 0.85f);
        overlayGo.SetActive(false);
        fullscreenOverlay = overlayGo;
        overlayBgBtn = overlayGo.GetComponent<Button>();
        overlayBgBtn.onClick.AddListener(CloseFullscreen);

        var contentGo = new GameObject("FullscreenContent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        contentGo.transform.SetParent(overlayGo.transform, false);
        var contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0.1f, 0.1f);
        contentRt.anchorMax = new Vector2(0.9f, 0.9f);
        contentRt.sizeDelta = Vector2.zero;
        var contentImg = contentGo.GetComponent<Image>();
        contentImg.preserveAspect = true;
        contentImg.color = Color.white;
        fullscreenImage = contentImg;
    }
}
