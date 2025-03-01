using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Cell : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ResourcesConfig resConfig;
    [SerializeField] private TMP_Text neighborsMinesCount;
    [SerializeField] private Image stateIcon;
    [SerializeField] private Image baseCellIcon;
    [SerializeField] private Image closedCellIcon;
    
    private bool marked = false;
    private bool opened = false;
    private int x;
    private int y;
    private int state;
    private ICellClicked minefield;

    public void InitCell(int xPos, int yPos, ICellClicked minefieldController)
    {
        minefield = minefieldController;
        x = xPos;
        y = yPos;
        state = CellState.EMPTY;
    }

    public void ResetCell()
    {
        SetState(CellState.EMPTY);
        closedCellIcon.sprite = null;
        closedCellIcon.gameObject.SetActive(true);
        neighborsMinesCount.gameObject.SetActive(false);
        opened = false;
        marked = false;
    }
    public bool IsMine()
    {
        return state == CellState.MINE; 
    }
    public bool IsMarked()
    {
        return marked; 
    }
    public bool IsEmpty()
    {
        return state == CellState.EMPTY;
    }
    public void OpenCell()
    {
        closedCellIcon.gameObject.SetActive(false);
        opened = true;
    }

    public bool Opened()
    {
        return opened;
    }
    public void SetState(int cellState)
    {   
        state = cellState;
        if (state == CellState.MINE)
        {
            UpdateStateIcon(resConfig.MineIcon);
        } 
        else if (state == CellState.EMPTY)
        {
            SetDefaults();
        } 
        else
        {
            SetDefaults();
            neighborsMinesCount.gameObject.SetActive(true);
            neighborsMinesCount.text = state.ToString();
        } 
    }
    private void SetDefaults()
    {
        stateIcon.gameObject.SetActive(false);
        baseCellIcon.color = Color.gray;
    }
    private void UpdateStateIcon(Sprite sprite)
    {
        stateIcon.gameObject.SetActive(true);
        stateIcon.sprite = sprite; 
        neighborsMinesCount.gameObject.SetActive(false);

    }
    private void MarkWithFlag()
    {
        marked = !marked;
        if (marked)
        {
            closedCellIcon.sprite = resConfig.RedFlagIcon;
        } 
        else
        {
            closedCellIcon.sprite = null;
        } 

    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            minefield.OnCellClick(x, y, true);
        } 
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            MarkWithFlag();
            minefield.OnCellClick(x, y, false);
        }
    }
}
