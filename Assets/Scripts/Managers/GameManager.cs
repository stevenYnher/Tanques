using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour{
    public int m_NumRoundsToWin = 5;        
    public float m_StartDelay = 3f;         
    public float m_EndDelay = 3f;           
    public CameraControl m_CameraControl;   
    public Text m_MessageText;              
    public GameObject m_TankPrefab;         
    public TankManager[] m_Tanks;         
    private AudioSource m_GameMusic;   
    public AudioClip m_MusicSource; 


    private int m_RoundNumber;              
    private WaitForSeconds m_StartWait;     
    private WaitForSeconds m_EndWait;       
    private TankManager m_RoundWinner;
    private TankManager m_GameWinner;       


    private void Start(){
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        SpawnAllTanks();
        SetCameraTargets();

        StartCoroutine(GameLoop());
        m_GameMusic.clip = m_MusicSource;
        m_GameMusic.Play();
    }


    private void SpawnAllTanks(){
        for (int i = 0; i < m_Tanks.Length; i++){
            m_Tanks[i].m_Instance =
                Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();
        }
    }


    private void SetCameraTargets(){
        Transform[] targets = new Transform[m_Tanks.Length];

        for (int i = 0; i < targets.Length; i++){
            targets[i] = m_Tanks[i].m_Instance.transform;
        }

        m_CameraControl.m_Targets = targets;
    }


    private IEnumerator GameLoop(){
        yield return StartCoroutine(RoundStarting());

        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        if (m_GameWinner != null){
            SceneManager.LoadScene(0);
        }else{
            StartCoroutine(GameLoop());
        }
        
    }


    private IEnumerator RoundStarting(){
        ResetAllTanks(); 
        DisableTankControl();

        m_CameraControl.SetStartPositionAndSize();

        m_RoundNumber++;
        m_MessageText.text = "ROUND " + m_RoundNumber;

        yield return m_StartWait;
    }


    private IEnumerator RoundPlaying()
    {
        EnableTankControl ();
        // Borro el texto de la pantalla.
        m_MessageText.text = string.Empty;
        // Mientras haya más de un tanque...
        while (!OneTankLeft()) {
        // ... vuelvo al frame siguiente.
            yield return null;
        }
    }


    private IEnumerator RoundEnding(){
        DisableTankControl();
        // Borro al ganador de la ronda anterior.
        m_RoundWinner = null;
        // Miro si hay un ganador de la ronda.
        m_RoundWinner = GetRoundWinner();
        // Si lo hay, incremento su puntuación.
        if (m_RoundWinner != null) 
            m_RoundWinner.m_Wins++;
        // Compruebo si alguien ha ganado el juego.
        m_GameWinner = GetGameWinner();
        // Genero el mensaje según si hay un gaandor del juego o no.
        string message = EndMessage(); 
        m_MessageText.text = message;
        // Espero a que pase el tiempo de espera antes de volver al bucle.
        yield return m_EndWait;
    }


    private bool OneTankLeft(){
        int numTanksLeft = 0;

        for (int i = 0; i < m_Tanks.Length; i++){
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }

        return numTanksLeft <= 1;
    }


    private TankManager GetRoundWinner(){
        for (int i = 0; i < m_Tanks.Length; i++){
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }

        return null;
    }


    private TankManager GetGameWinner(){
        for (int i = 0; i < m_Tanks.Length; i++){
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }

        return null;
    }


    private string EndMessage(){
        string message = "EMPATE!";

        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " GANA LA RONDA!";

        message += "\n\n\n\n";

        for (int i = 0; i < m_Tanks.Length; i++){
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " GANA\n";
        }

        if (m_GameWinner != null)
            message = m_GameWinner.m_ColoredPlayerText + " GANA EL JUEGO!";

        return message;
    }


    private void ResetAllTanks(){
        for (int i = 0; i < m_Tanks.Length; i++){
            m_Tanks[i].Reset();
        }
    }


    private void EnableTankControl(){
        for (int i = 0; i < m_Tanks.Length; i++){
            m_Tanks[i].EnableControl();
        }
    }


    private void DisableTankControl(){
        for (int i = 0; i < m_Tanks.Length; i++){
            m_Tanks[i].DisableControl();
        }
    }
}