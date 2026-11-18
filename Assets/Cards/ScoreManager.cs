using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ScoreManager : CardColleague
{
    private const int TripletPoints = 30;
    private const int SequencePoints = 10;
    private const int ElementMultiplier = 2;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI totalLabel;
    [SerializeField] private TextMeshProUGUI comboLabel;

    [Header("Timing")]
    [SerializeField] private float announceDuration = 1f;

    private struct PendingScore
    {
        public string Text;
        public int Points;
    }

    private readonly Queue<PendingScore> queue = new Queue<PendingScore>();
    private bool processing;
    private int totalScore;

    public int Total => totalScore;
    public bool IsBusy => processing || queue.Count > 0;
    public event Action<int> ScoreChanged;

    void Start()
    {
        if (comboLabel != null) comboLabel.text = string.Empty;
        UpdateTotalLabel();
    }

    /// Called by the mediator right after a valid combo is assembled.
    /// Everything is calculated synchronously: ComboManager clears the selected list
    /// right after the notification, and the cards get destroyed later.
    public void ScoreCombo(ComboType comboType, IReadOnlyList<Card> cards)
    {
        if (comboType == ComboType.None || cards.Count < 3) return;

        PendingScore result = comboType == ComboType.Triplet
            ? EvaluateTriplet(cards)
            : new PendingScore { Text = $"Sequence  +{SequencePoints}", Points = SequencePoints };

        queue.Enqueue(result);
        if (!processing) StartCoroutine(ProcessQueue());
    }

    private PendingScore EvaluateTriplet(IReadOnlyList<Card> cards)
    {
        // Find the value shared by all 3 cards; prefer an element if several match
        CardValue? best = null;
        foreach (CardValue value in cards[0].GetCardBase().GetCardValues())
        {
            if (!Shares(cards[1], value) || !Shares(cards[2], value)) continue;
            if (best == null || IsElement(value)) best = value;
        }

        bool element = best.HasValue && IsElement(best.Value);
        int points = element ? TripletPoints * ElementMultiplier : TripletPoints;
        string name = best.HasValue ? best.Value.ToString() : "Triplet";

        return new PendingScore
        {
            Text = element ? $"Triplet of {name}  x2  +{points}" : $"Triplet of {name}  +{points}",
            Points = points
        };
    }

    private static bool Shares(Card card, CardValue value) =>
        Array.IndexOf(card.GetCardBase().GetCardValues(), value) >= 0;

    private static bool IsElement(CardValue value) => value >= CardValue.Earth;

    private IEnumerator ProcessQueue()
    {
        processing = true;

        while (queue.Count > 0)
        {
            PendingScore score = queue.Dequeue();

            if (comboLabel != null)
            {
                comboLabel.text = score.Text;
                comboLabel.transform.DOKill(true);   // finish any previous punch so the scale resets cleanly
                comboLabel.transform.DOPunchScale(new Vector3(0.3f, 0.3f, 0f), 0.4f, 8, 0.5f);
            }
            yield return new WaitForSeconds(announceDuration);
            if (comboLabel != null) comboLabel.text = string.Empty;

            totalScore += score.Points;
            UpdateTotalLabel();
            ScoreChanged?.Invoke(totalScore);
            Mediator?.NotifyScoreChanged(totalScore);
        }

        processing = false;
    }

    private void UpdateTotalLabel()
    {
        if (totalLabel != null) totalLabel.text = totalScore.ToString();
    }
}