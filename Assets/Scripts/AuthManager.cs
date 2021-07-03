using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Proyecto26;
using UnityEngine.UI;
///ADICIONADOS: 
using FirebaseWebGL.Scripts.FirebaseBridge;
using FirebaseWebGL.Scripts.Objects;
using FirebaseWebGL.Examples.Utils;



public class AuthManager : MonoBehaviour
{
    //////////////////////////////////////////////////////////////////
    //////////////////////////DEFINIR ABAIXO//////////////////////////
    public static int GameNumber = 2; //DEFINIR ENTRE 1, 2 ou 3.

    //////////////////////////////////////////////////////////////////

    //INICIALIZA UM NOVO USUARIO

    User user = new User();

    public static AuthManager instance;

    public static string UserID;

    public static int playerScore;
    public static string playerName;
    public static string playerEmail;

    [HideInInspector]
    public int playerBestScore;

    //Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    //Register variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    //Logged variables
    [Header("Logged")]
    public TMP_Text scoreText;
    public TMP_Text nameText;
    public TMP_Text emailText;


    private void Start()
    {
        Debug.Log("Start");
        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            Debug.Log("The code is not running on a WebGL build; as such, the Javascript functions will not be recognized.");
            return;
        }
        Debug.Log("OnAuthStateChanged");
        FirebaseAuth.OnAuthStateChanged(gameObject.name, "DisplayUserInfo", "DisplayInfo");
    }

    //Function for the login button
    public void LoginButton()
    {
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }
    //Function for the register button
    public void RegisterButton()
    {
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        Debug.Log("Tentando logar: " + _email + " senha: " + _password);
        FirebaseAuth.SignInWithEmailAndPassword(_email, _password, gameObject.name, "DisplayInfo", "DisplayErrorObject");
        yield return new WaitForSeconds(2f);
        UIManager.instance.GameScreen();
        StartCoroutine(RetrieveFromDatabase(UserID));

    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        warningRegisterText.text = "";
        if (_username == "")
        {
            warningRegisterText.text = "Faltando Username";
        }
        else if (passwordRegisterField.text == "")
        {
            warningRegisterText.text = "Digite uma senha!";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            warningRegisterText.text = "Senhas não conferem!";
        }
        else if (emailRegisterField.text == "")
        {
            warningRegisterText.text = "Digite um e-mail valido!";
        }
        else
        {
            warningRegisterText.text = "";
            FirebaseAuth.CreateUserWithEmailAndPassword(_email, _password, gameObject.name, "DisplayInfo", "DisplayErrorObject");
            yield return new WaitForSeconds(4f);
            PostDataBaseInitial(UserID, playerName, _email, 0);
        }
    }

    public IEnumerator DisplayUserInfo(string user)
    {
        var parsedUser = StringSerializationAPI.Deserialize(typeof(FirebaseUser), user) as FirebaseUser;
        Debug.Log($"Email: {parsedUser.email}, UserId: {parsedUser.uid}");
        UserID = parsedUser.uid;

        //NECESSARIO AJUSTAR ABAIXO


        if (emailLoginField.text != "" && passwordLoginField.text != ""){
            //TRATAMOS COMO LOGIN
            Debug.Log("DisplayInfoLogin");
            StartCoroutine(RetrieveFromDatabase(UserID));
            yield return new WaitForSeconds(2f);
            scoreText.text = playerScore.ToString();
            nameText.text = playerName;
            emailText.text = playerEmail;
            emailLoginField.text = "";
            passwordLoginField.text = "";
        }else{
            if (usernameRegisterField.text != ""){
                //TRATAMOS COMO UM REGISTRO
                Debug.Log("DisplayInfoRegister");
                playerName = usernameRegisterField.text;
                
                playerEmail = parsedUser.email;
                playerScore = 0;

                emailLoginField.text = parsedUser.email;
                usernameRegisterField.text = "";
                passwordRegisterField.text = "";
                passwordRegisterVerifyField.text = "";
                emailRegisterField.text = "";
                UIManager.instance.LoginScreen();
                warningRegisterText.text = "";
            }
        }
        SetAllBlank();
    }

    private void SetAllBlank(){
        usernameRegisterField.text = "";
        passwordRegisterField.text = "";
        emailRegisterField.text = "";
        warningRegisterText.text = "";
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }


    public void DisplayInfo(string info)
    {
        Debug.Log("DisplayInfo:");
        Debug.Log(info);
    }

    public void DisplayErrorObject(string error)
    {        
        Debug.Log("DisplayErrorObject:");
        var parsedError = StringSerializationAPI.Deserialize(typeof(FirebaseError), error) as FirebaseError;
        Debug.Log(parsedError.message);
        
    }

    public void DisplayError(string error)
    {
        Debug.Log("DisplayError:");
        Debug.LogError(error);
    }

    // public string RetrieveUserName(string userid)
    // {
    //     string NickName = "";
    //     RestClient.Get<User>("https://ui-ux-gameproject-e70cc-default-rtdb.firebaseio.com/Game" + GameNumber + "/" + userid + ".json").Then(response =>
    //     {
    //         NickName = response.userName;
    //         return response.userName;
    //     });
    //     return NickName;
    // }

    public IEnumerator RetrieveFromDatabase(string userid)
    {
        RestClient.Get<User>("https://ui-ux-gameproject-e70cc-default-rtdb.firebaseio.com/Game" + GameNumber + "/" + userid + ".json").Then(response =>
        {
            Debug.Log("Dados recebidos do BD: " + user.userName);
            user = response;
            playerName = user.userName;
            playerBestScore = playerScore = user.userScore;
            playerEmail = user.userEmail;
        });
        UIManager.instance.GameScreen();

        yield return new WaitForSeconds(4f);
    }

    ////////////////////UTILIZAR FUN��O ABAIXO PARA ENVIO DO SCORE AP�S A MORTE////////////////////
    ////////////////////USERID: variavel unica gerada pelo firebase. Ela � adquirida ap�s o usuario realizar o login. Salva na linha 136 (User = LoginTask.Result;) e pode ser utilizada usando "User.UserId".
    ////////////////////USERNAME: variavel criada de acordo com o usuario. Acesse usando "User.DisplayName"
    ////////////////////EMAIL: variavel unica criada pelo usuario. Acesse usando "User.Email"
    ////////////////////SCORE: variavel global criada pelo desenvolvedor. � o score do jogo. Lembrar de reseta-la ao finalizar o POST.
    public void PostToDatabase(int score)
    {
        Debug.Log("PostToDatabase ID:" + UserID);
        User user = new User();
        user.userEmail = playerEmail;
        playerBestScore = user.userScore = score;
        user.userName = playerName;
        RestClient.Put("https://ui-ux-gameproject-e70cc-default-rtdb.firebaseio.com/Game" + GameNumber + "/" + UserID + ".json", user);
    }

    ////////////////////ESTA FUN��O SER� CHAMADA APENAS DURANTE O REGISTRO INICIAL DO USUARIO////////////////////
    ////////////////////N�O UTILIZAR POSTERIORMENTE////////////////////
    public void PostDataBaseInitial(string userid, string username, string email, int score)
    {
        Debug.Log("PostDataBaseInitial ID:" + userid);
        User user = new User();
        user.userEmail = email;
        user.userScore = score;
        user.userName = username;
        RestClient.Put("https://ui-ux-gameproject-e70cc-default-rtdb.firebaseio.com/" + "Game" + 1 + "/" + userid + ".json", user);
        RestClient.Put("https://ui-ux-gameproject-e70cc-default-rtdb.firebaseio.com/" + "Game" + 2 + "/" + userid + ".json", user);
        RestClient.Put("https://ui-ux-gameproject-e70cc-default-rtdb.firebaseio.com/" + "Game" + 3 + "/" + userid + ".json", user);
    }

}
