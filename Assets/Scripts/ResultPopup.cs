using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class ResultPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private GameObject panel;
    public void SetState(bool victory)
    {
        if (!victory)
        {
            title.color = Color.red;
            title.text = "Failed!";
        }
        else 
        {
            title.color = Color.blue;
            title.text = "Victory!";
        }
        ShowPanelWithDelay(1000);
    }

    private async void ShowPanelWithDelay(int delay)
    {
        await Task.Delay(delay);
        panel.gameObject.SetActive(true);
    }
    void OnDisable()
    {
        panel.gameObject.SetActive(false);
    }
}
