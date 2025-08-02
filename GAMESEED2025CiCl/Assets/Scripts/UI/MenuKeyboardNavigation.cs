using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuKeyboardNavigation : MonoBehaviour
{
    // Array untuk menampung semua tombol menu
    public Button[] menuButtons;

    private int currentSelectedIndex = 0;

    void Start()
    {
        if (menuButtons.Length > 0)
        {
            // Atur tombol pertama sebagai yang terpilih di awal
            EventSystem.current.SetSelectedGameObject(menuButtons[0].gameObject);
        }
    }

    void Update()
    {
        // Mendengarkan tombol 'W' untuk navigasi ke atas
        if (Input.GetKeyDown(KeyCode.W))
        {
            SelectPreviousButton();
        }

        // Mendengarkan tombol 'S' untuk navigasi ke bawah
        if (Input.GetKeyDown(KeyCode.S))
        {
            SelectNextButton();
        }
    }

    void SelectPreviousButton()
    {
        if (menuButtons.Length == 0) return;

        currentSelectedIndex--;
        if (currentSelectedIndex < 0)
        {
            currentSelectedIndex = menuButtons.Length - 1;
        }

        EventSystem.current.SetSelectedGameObject(menuButtons[currentSelectedIndex].gameObject);
    }

    void SelectNextButton()
    {
        if (menuButtons.Length == 0) return;

        currentSelectedIndex++;
        if (currentSelectedIndex >= menuButtons.Length)
        {
            currentSelectedIndex = 0;
        }

        EventSystem.current.SetSelectedGameObject(menuButtons[currentSelectedIndex].gameObject);
    }
}