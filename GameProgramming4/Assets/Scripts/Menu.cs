using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;


public class Menu : MonoBehaviour
{
    GameManager gamemanager;
    public AudioClip[] clip;
    int num;
    public bool isMenu;
    public Slider[] slider;

    public GameObject menu;
    public GameObject player;
    public PhotonView PV;

    void Awake()
    {
        //soundmanager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        gamemanager = GameObject.Find("GameManager").GetComponent<GameManager>();
        isMenu = true;

        slider[0].value = gamemanager.val1;
        slider[1].value = gamemanager.val2;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            if (gamemanager.isMenu)
            {
                menu.SetActive(false);
                gamemanager.isMenu = false;
                player.GetComponent<PlayerScript>().isShoot = true;
            }
            else
            {
                menu.SetActive(true);
                gamemanager.isMenu = true;
                player.GetComponent<PlayerScript>().isShoot = false;
            }
        };


    }
    void OnEnable()
    {
        
    }
    void OnDisable()
    {
        //SoundManager.instance.Click();
    }

    public void BGSoundVolume(float val)
    {
        gamemanager.BGSoundVolume(val);
    }

    public void SFXVolume(float val)
    {
        gamemanager.SFXVolume(val);
    }

    public void LeaveRoom()
    {
        Destroy(GameObject.Find("GameManager"));
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Main");
    }

    public void Exit()
    {
        PhotonNetwork.Disconnect();
        Application.Quit();
    }




}
