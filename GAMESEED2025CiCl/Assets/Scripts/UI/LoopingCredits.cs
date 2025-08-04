using UnityEngine;

public class LoopingCredits : MonoBehaviour
{
    public RectTransform creditContent;   // The parent of all credit texts
    public RectTransform viewport;        // The panel (mask area)
    public float scrollSpeed = 50f;       // Pixels per second

    private float startY;                 
    private float endY;                   

    void Start()
    {
        // Record the starting position
        startY = creditContent.anchoredPosition.y;

        float contentHeight = creditContent.rect.height;
        float viewportHeight = viewport.rect.height;

        // This is the Y position where the content is fully out of view
        endY = startY + contentHeight + viewportHeight;
    }

    void Update()
    {
        // Move upward each frame
        Vector2 pos = creditContent.anchoredPosition;
        pos.y += scrollSpeed * Time.deltaTime;
        creditContent.anchoredPosition = pos;

        // Once the content has completely left the screen, move it back down
        if (pos.y >= endY)
        {
            // Move content back down to start
            pos.y = startY - (creditContent.rect.height + viewport.rect.height);
            creditContent.anchoredPosition = pos;
        }
    }
}
