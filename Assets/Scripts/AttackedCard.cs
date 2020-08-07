using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class AttackedCard : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {

        CardController attacker = eventData.pointerDrag.GetComponent<CardController>();
        CardController defender = GetComponent<CardController>();
        if (attacker == null || defender == null)
        {
            return;
        }
        if (attacker.model.isPlayerCard == defender.model.isPlayerCard)
        {
            return;
        }
        if (attacker.model.canAttack)
        {
            GameManager.instance.CardBattle(attacker, defender);
        }
    }
}
