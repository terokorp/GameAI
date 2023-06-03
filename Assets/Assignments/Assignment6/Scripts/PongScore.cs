using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PongScore : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI[] playerScore;
    int[] scores = new int[2];

    public void AddAcore(PongPlayer.PlayerSide side)
    {
        scores[(int)side]++;
        playerScore[(int)side].text = scores[(int)side].ToString();
    }
}