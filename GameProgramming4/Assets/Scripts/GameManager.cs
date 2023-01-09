using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    //public InputField NickNameInput;
    public int skin;//스킨
    public int gskin;//총스킨
    public bool isMenu = false;//메뉴 열렸는지

    public Animator AN;
    public Animator GN; //총 애니메이션

    public float val1;//효과음
    public float val2;//배경음
    public AudioMixer mixer;


    public bool isMaster;

    public string PlayerName;
    public int PlayerNum;

    public List<PlayerScript> Players = new List<PlayerScript>();


    void Awake()
    {
        if (Instance != null) //중복파괴
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        mixer.SetFloat("BG", Mathf.Log10(val1) * 20);
        mixer.SetFloat("SFX", Mathf.Log10(val2) * 20);
    }

    public void SortPlayers() => Players.Sort((p1, p2) => p1.actor.CompareTo(p2.actor));

    public void ChangeSkin(int i)
    {
        skin = i;
        AN.SetFloat("Blend", i);
        AN.SetBool("isSpawn", true);
    }
    public void ChangeGSkin(int i)
    {
        gskin = i;
        GN.SetFloat("Blend", i);
        GN.SetTrigger("isShot");
    }
    public void BGSoundVolume(float val)
    {
        val1 = val;
        mixer.SetFloat("BG", Mathf.Log10(val) * 20);
    }

    public void SFXVolume(float val)
    {
        val2 = val;
        mixer.SetFloat("SFX", Mathf.Log10(val) * 20);
    }



}
