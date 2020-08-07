using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject resultPanel;
    [SerializeField] Text resultText;
    [SerializeField] Transform playerHandTransform;
    [SerializeField] Transform playerFieldTransform;
    [SerializeField] Transform enemyHandTransform;
    [SerializeField] Transform enemyFieldTransform;
    [SerializeField] CardController cardPrefab;
    public bool isPlayerTurn;
    List<int> playerDeck = new List<int>() { 3, 1, 2, 2, 3 };
    List<int> enemyDeck = new List<int>() { 2, 1, 3, 1, 3 };
    [SerializeField] Text playerHeroHpText;
    [SerializeField] Text enemyHeroHpText;
    int playerHeroHp;
    int enemyHeroHp;
    [SerializeField] Transform playerHero;
    [SerializeField] Text playerManaCostText;
    [SerializeField] Text enemyManaCostText;
    public int playerManaCost;
    int enemyManaCost;
    int playerDefaultManaCost;
    int enemyDefaultManaCost;
    [SerializeField] Text timeCountText;
    int timeCount;
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        StarGame();
    }

    void StarGame()
    {
        resultPanel.SetActive(false);
        playerHeroHp = 1;
        enemyHeroHp = 1;
        playerManaCost = playerDefaultManaCost = 10;
        enemyManaCost = enemyDefaultManaCost = 10;
        timeCount = 8;
        timeCountText.text = timeCount.ToString();
        ShowHeroHP();
        ShowManaCost();
        SettingInitHand();
        isPlayerTurn = true;
        TurnCalc();
    }

    void ShowManaCost()
    {
        playerManaCostText.text = playerManaCost.ToString();
        enemyManaCostText.text = enemyManaCost.ToString();
    }

    public void ReduceManaCost(int cost, bool isPlayerCard)
    {
        if (isPlayerCard)
        {
            playerManaCost -= cost;
        }
        else
        {
            enemyManaCost -= cost;
        }
        ShowManaCost();
    }

    public void Restart()
    {
        foreach (Transform card in playerHandTransform)
        {
            Destroy(card.gameObject);

        }
        foreach (Transform card in playerFieldTransform)
        {
            Destroy(card.gameObject);

        }
        foreach (Transform card in enemyHandTransform)
        {
            Destroy(card.gameObject);

        }
        foreach (Transform card in enemyFieldTransform)
        {
            Destroy(card.gameObject);

        }
        playerDeck = new List<int>() { 3, 1, 2, 2, 3 };
        enemyDeck = new List<int>() { 2, 1, 3, 1, 3 };
        StarGame();
    }

    void SettingInitHand()
    {
        for (int i = 0; i < 3; i++)
        {
            GiveCardToHand(playerDeck, playerHandTransform);
            GiveCardToHand(enemyDeck, enemyHandTransform);
        }
    }

    void GiveCardToHand(List<int> deck, Transform hand)
    {
        if (deck.Count == 0)
        {
            return;
        }
        int cardID = deck[0];
        deck.RemoveAt(0);
        CreateCard(cardID, hand);
    }

    void CreateCard(int cardID, Transform hand)
    {
        CardController card = Instantiate(cardPrefab, hand, false);
        if (hand.name == "PlayerHand")
        {
            card.Init(cardID, true);
        }
        else
        {
            card.Init(cardID, false);
        }
    }

    void TurnCalc()
    {
        StopAllCoroutines();
        StartCoroutine(CountDown());
        if (isPlayerTurn)
        {
            PlayerTurn();
        }
        else
        {
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator CountDown()
    {
        timeCount = 8;
        timeCountText.text = timeCount.ToString();
        while (timeCount > 0)
        {
            yield return new WaitForSeconds(1);
            timeCount--;
            timeCountText.text = timeCount.ToString();
        }
        ChangeTurn();
    }

    public void OnClickTurnEndButton()
    {
        if (isPlayerTurn)
        {
            ChangeTurn();
        }
    }

    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn;
        CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(playerFieldCardList, false);
        CardController[] enemyFieldCardList = enemyFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(enemyFieldCardList, false);
        if (isPlayerTurn)
        {
            playerDefaultManaCost++;
            playerManaCost = playerDefaultManaCost;
            GiveCardToHand(playerDeck, playerHandTransform);
        }
        else
        {
            enemyDefaultManaCost++;
            enemyManaCost = enemyDefaultManaCost;
            GiveCardToHand(enemyDeck, enemyHandTransform);
        }
        ShowManaCost();
        TurnCalc();
    }

    void SettingCanAttackView(CardController[] fieldCardList, bool canAttack)
    {
        foreach (CardController card in fieldCardList)
        {
            card.SetCanAttack(canAttack);
        }
    }

    void PlayerTurn()
    {
        Debug.Log("Player Turn");
        CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(playerFieldCardList, true);
    }

    IEnumerator EnemyTurn()
    {
        Debug.Log("Enemy Turn");
        CardController[] enemyFieldCardList = enemyFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(enemyFieldCardList, true);
        yield return new WaitForSeconds(1);
        CardController[] handCardList = enemyHandTransform.GetComponentsInChildren<CardController>();
        while (Array.Exists(handCardList, card => card.model.cost <= enemyManaCost))
        {
            CardController[] selectableHandCardList = Array.FindAll(handCardList, card => card.model.cost <= enemyManaCost);
            CardController enemyCard = handCardList[0];
            StartCoroutine(enemyCard.movement.MoveToField(enemyFieldTransform));
            ReduceManaCost(enemyCard.model.cost, false);
            enemyCard.model.isFieldCard = true;
            handCardList = enemyHandTransform.GetComponentsInChildren<CardController>();
            yield return new WaitForSeconds(1);
        }
        yield return new WaitForSeconds(1);
        CardController[] fieldCardList = enemyFieldTransform.GetComponentsInChildren<CardController>();
        while (Array.Exists(fieldCardList, card => card.model.canAttack))
        {
            CardController[] enemyCanAttackCardList = Array.FindAll(fieldCardList, card => card.model.canAttack);
            CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();
            CardController attacker = enemyCanAttackCardList[0];
            if (playerFieldCardList.Length > 0)
            {
                CardController defender = playerFieldCardList[0];
                StartCoroutine(attacker.movement.MoveToTarget(defender.transform));
                yield return new WaitForSeconds(0.25f);
                CardBattle(attacker, defender);
            }
            else
            {
                StartCoroutine(attacker.movement.MoveToTarget(playerHero));
                yield return new WaitForSeconds(0.25f);
                AttackToHero(attacker, false);
                yield return new WaitForSeconds(0.25f);
                CheckHeroHP();
            }
            fieldCardList = enemyFieldTransform.GetComponentsInChildren<CardController>();
            yield return new WaitForSeconds(1);
        }
        yield return new WaitForSeconds(1);
        ChangeTurn();
    }

    public void CardBattle(CardController attacker, CardController defender)
    {
        Debug.Log("CardBattle");
        Debug.Log("attacker HP : " + attacker.model.hp);
        Debug.Log("defender HP : " + defender.model.hp);
        attacker.Attack(defender);
        defender.Attack(attacker);
        Debug.Log("attacker HP : " + attacker.model.hp);
        Debug.Log("defender HP : " + defender.model.hp);
        attacker.CheckAlive();
        defender.CheckAlive();
    }

    void ShowHeroHP()
    {
        playerHeroHpText.text = playerHeroHp.ToString();
        enemyHeroHpText.text = enemyHeroHp.ToString();
    }

    public void AttackToHero(CardController attacker, bool isPlayerCard)
    {
        if (isPlayerCard)
        {
            enemyHeroHp -= attacker.model.at;
        }
        else
        {
            playerHeroHp -= attacker.model.at;
        }
        attacker.SetCanAttack(false);
        ShowHeroHP();
    }

    void CheckHeroHP()
    {
        if (playerHeroHp <= 0 || enemyHeroHp <= 0)
        {
            ShowResultPanel(playerHeroHp);
        }
    }

    void ShowResultPanel(int heroHp)
    {
        StopAllCoroutines();
        resultPanel.SetActive(true);
        if (heroHp <= 0)
        {
            resultText.text = "LOSE";
        }
        else
        {
            resultText.text = "WIN";
        }
    }
}
