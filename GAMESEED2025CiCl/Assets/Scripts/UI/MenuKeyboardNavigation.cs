using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuKeyboardNavigation : MonoBehaviour
{
    public Button[] menuButtons;

    private int currentSelectedIndex = -1;
    private ButtonHoverAnimation previousHoverAnimation;

    void Start()
    {
    }

    public void Initialize()
    {
        if (menuButtons.Length == 0)
        {
            EventSystem.current.SetSelectedGameObject(menuButtons[0].gameObject);
            previousHoverAnimation = menuButtons[0].GetComponent<ButtonHoverAnimation>();
            if (previousHoverAnimation != null)
            {
                previousHoverAnimation.OnSelect();
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            SelectPreviousButton();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SelectNextButton();
        }
        
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (menuButtons.Length > 0 && currentSelectedIndex >= 0 && currentSelectedIndex < menuButtons.Length)
            {
                menuButtons[currentSelectedIndex].onClick.Invoke();
            }
        }
    }

    void SelectPreviousButton()
    {
        if (menuButtons.Length == 0) return;
        
        if (previousHoverAnimation != null)
        {
            previousHoverAnimation.OnDeselect();
        }
        currentSelectedIndex--;
        if (currentSelectedIndex < 0)
        {
            currentSelectedIndex = menuButtons.Length - 1;
        }
        
        Button currentButton = menuButtons[currentSelectedIndex];
        EventSystem.current.SetSelectedGameObject(currentButton.gameObject);
        previousHoverAnimation = currentButton.GetComponent<ButtonHoverAnimation>();
        if (previousHoverAnimation != null)
        {
            previousHoverAnimation.OnSelect();
        }
    }

    void SelectNextButton()
    {
        if (menuButtons.Length == 0) return;
        
        if (previousHoverAnimation != null)
        {
            previousHoverAnimation.OnDeselect();
        }
        currentSelectedIndex++;
        if (currentSelectedIndex >= menuButtons.Length)
        {
            currentSelectedIndex = 0;
        }
        
        Button currentButton = menuButtons[currentSelectedIndex];
        EventSystem.current.SetSelectedGameObject(currentButton.gameObject);
        previousHoverAnimation = currentButton.GetComponent<ButtonHoverAnimation>();
        if (previousHoverAnimation != null)
        {
            previousHoverAnimation.OnSelect();
        }
    }
}