using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class User
{
    public string userName;
    public int userScore;
    public string userEmail;


    public User()
    {
        userName = AuthManager.playerName;
        userScore = AuthManager.playerScore;
        userEmail = AuthManager.playerEmail;
    }
}
