using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CarouselView : MonoBehaviour {
    
    public RectTransform viewWindow;
    [FormerlySerializedAs("images")] public List<RectTransform> elements;
    
    private bool  canSwipe;
    private float imageWidth;
    private float lerpTimer;
    private float lerpPosition;
    private float mousePositionStartX;
    private float mousePositionEndX;
    private float dragAmount;
    private float screenPosition;
    private float lastScreenPosition;

    /// <summary>
    /// Space between images.
    /// </summary>
    public  float imageGap = 30;

    public int swipeThrustHold = 30;
    
    private int currentIndex;
    
    /// <summary>
    /// The index of the current image on display.
    /// </summary>
    public int CurrentIndex { get { return currentIndex; } }

    #region mono
    // Use this for initialization
    void Start ()
    {
        //Add all children of the carousel to the list automatically
        for (var i = 0; i < viewWindow.childCount; i++)
        {
            elements.Add((RectTransform)viewWindow.GetChild(i));
        }
        
        imageWidth = viewWindow.rect.width;
        for (int i = 1; i < elements.Count; i++)
        {
            elements[i].anchoredPosition = new Vector2(((imageWidth + imageGap) * i), 0);
        }
    }

    // Update is called once per frame
    void Update ()
    {
        UpdateCarouselView();
    }
    #endregion

    #region private methods
    
    void UpdateCarouselView()
    {
        lerpTimer = lerpTimer + Time.deltaTime;

        if (lerpTimer < 0.333f)
        {
            screenPosition = Mathf.Lerp(lastScreenPosition, lerpPosition * -1, lerpTimer * 3);
            lastScreenPosition = screenPosition;
        }

        if (Input.GetMouseButtonDown(0))
        {
            canSwipe = true;
            mousePositionStartX = Input.mousePosition.x;
        }


        if (Input.GetMouseButton(0))
        {
            if (canSwipe)
            {
                mousePositionEndX = Input.mousePosition.x;
                dragAmount = mousePositionEndX - mousePositionStartX;
                screenPosition = lastScreenPosition + dragAmount;
            }
        }

        if (Mathf.Abs(dragAmount) > swipeThrustHold && canSwipe)
        {
            canSwipe = false;
            lastScreenPosition = screenPosition;
            if (currentIndex < elements.Count)
                OnSwipeComplete();
            else if (currentIndex == elements.Count && dragAmount < 0)
                lerpTimer = 0;
            else if (currentIndex == elements.Count && dragAmount > 0)
                OnSwipeComplete();
        }

        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].anchoredPosition = new Vector2(screenPosition + ((imageWidth + imageGap) * i), 0);
            if (i == currentIndex)
            {
                elements[i].localScale = Vector3.Lerp(elements[i].localScale, new Vector3(1.2f, 1.2f, 1.2f), Time.deltaTime * 5);
            }
            else
            {
                elements[i].localScale = Vector3.Lerp(elements[i].localScale, new Vector3(0.7f, 0.7f, 0.7f), Time.deltaTime * 5);
            }
        }
    }

    void OnSwipeComplete()
    {
        lastScreenPosition = screenPosition;

        if (dragAmount > 0)
        {
            if (dragAmount >= swipeThrustHold)
            {
                if (currentIndex == 0)
                {
                    lerpTimer = 0; lerpPosition = 0;
                }
                else
                {
                    currentIndex--;
                    lerpTimer = 0;
                    if (currentIndex < 0)
                        currentIndex = 0;
                    lerpPosition = (imageWidth + imageGap) * currentIndex;
                }
            }
            else
            {
                lerpTimer = 0;
            }
        }
        else if (dragAmount < 0)
        {
            if (Mathf.Abs(dragAmount) >= swipeThrustHold)
            {
                if (currentIndex == elements.Count-1)
                {
                    lerpTimer = 0;
                    lerpPosition = (imageWidth + imageGap) * currentIndex;
                }
                else
                {
                    lerpTimer = 0;
                    currentIndex++;
                    lerpPosition = (imageWidth + imageGap) * currentIndex;
                }
            }
            else
            {
                lerpTimer = 0;
            }
        }
        dragAmount = 0;
    }
    #endregion
    
    #region public methods
    public void GoToIndex(int value)
    {
        currentIndex = value;
        lerpTimer = 0;
        lerpPosition = (imageWidth + imageGap) * currentIndex;
        screenPosition = lerpPosition * -1;
        lastScreenPosition = screenPosition;
        
        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].anchoredPosition = new Vector2(screenPosition + ((imageWidth + imageGap) * i), 0);
        }
    }

    public void GoToIndexSmooth(int value)
    {
        currentIndex = value;
        lerpTimer = 0;
        lerpPosition = (imageWidth + imageGap) * currentIndex;
    }
    
    public void ClickEvent(int ID)
    {
        //Subtracted 1 from the IDs, as they have to be 0 indexed
        if (ID - 1 != currentIndex)
        {
            GoToIndexSmooth(ID - 1);
        }
        else if(ID - 1 == currentIndex)
        {
            SceneLoader.Instance.LoadScene(ID);
        }
    }

    public void GoToNextIndex()
    {
        if(currentIndex + 1 <= elements.Count - 1)
            GoToIndexSmooth(currentIndex + 1);
    }
    
    public void GoToPreviousIndex()
    {
        if(currentIndex - 1 >= 0)
            GoToIndexSmooth(currentIndex - 1);
    }
    #endregion
}
