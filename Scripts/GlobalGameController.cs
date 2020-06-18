using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameController : MonoBehaviour
{
    public static GlobalGameController instance;
    public static AudioClip dig1Sound, dig2Sound, alienDeathSound, playerDeathSound;
    public static AudioSource soundEffectsSource, soundMusicSource; 

    public static int lives, saveTimes, level;
    public static float redAlienSpeed, currency;

    public static int[,] spawnAliensNumberPerLevel;

    private const float DEFAULT_MUSIC_VOLUME = .5F, DEFAULT_SOUND_EFFECT_VOLUME = .5F;
    private const string MUSIC_VOLUME = "MusicVolume", SOUND_EFFECT_VOLUME = "SFXVolume";

    void Awake()
    {
        if (instance == null)
        {
            InitializeEnemysPerLevel();

            AudioSource[] sources = GetComponents<AudioSource>();

            soundMusicSource = sources[0];
            soundEffectsSource = sources[1];
            
            LoadSounds();
            LoadSoundsVolumes();

            saveTimes = 0;
            level = 1;
            redAlienSpeed = 0;
            currency = 0;

            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeEnemysPerLevel()
    {
        // Spawn aliens based on priority: { green,red,boss }
        spawnAliensNumberPerLevel = new int[,]{
                                                 { 1,0,0 } , {2,0,0} , {3,0,0} , {4,0,0} , {0,0,1},//Levels 1 - 5
                                                 { 0,1,0 } , {1,1,0} , {2,1,0} , {3,1,0} , {1,0,1},//Levels 6 - 10
                                                 { 1,1,0 } , {2,1,0} , {3,1,0} , {4,1,0} , {2,0,1},//Levels 11 - 15
                                                 { 2,1,0 } , {3,1,0} , {4,1,0} , {3,2,0} , {3,0,1},//Levels 16 - 20 
                                                 { 3,1,0 } , {4,1,0} , {3,2,0} , {2,3,0} , {0,1,1},//Levels 21 - 25
                                                 { 3,2,0 } , {2,3,0} , {2,3,0} , {1,4,0} , {0,2,1},//Levels 26 - 30
                                                 { 1,4,0 } , {0,5,0} , {0,5,0} , {0,5,0} , {0,3,2},//Levels 31 - 35  
                                               };
    }

    private void LoadSounds()
    {
        dig1Sound = Resources.Load<AudioClip>("Sounds/digg01metal less");
        dig2Sound = Resources.Load<AudioClip>("Sounds/digg02metal less");
        alienDeathSound = Resources.Load<AudioClip>("Sounds/Dead Alien");
        playerDeathSound = Resources.Load<AudioClip>("Sounds/Dead Man");
    }

    private void LoadSoundsVolumes()
    {
        if (PlayerPrefs.HasKey(MUSIC_VOLUME) || PlayerPrefs.HasKey(SOUND_EFFECT_VOLUME))
        {
            soundMusicSource.volume = PlayerPrefs.GetFloat(MUSIC_VOLUME);
            soundEffectsSource.volume = PlayerPrefs.GetFloat(SOUND_EFFECT_VOLUME);
        }
        else
        {
            soundMusicSource.volume = DEFAULT_MUSIC_VOLUME;
            soundEffectsSource.volume = DEFAULT_SOUND_EFFECT_VOLUME;
        }
    }

    public static void PlaySoundEffect(string soundEffectName)
    {
        switch(soundEffectName)
        {
            case "Dig1":
                soundEffectsSource.PlayOneShot(dig1Sound);
                break;
            case "Dig2":
                soundEffectsSource.PlayOneShot(dig2Sound);
                break;
            case "AlienDead":
                soundEffectsSource.PlayOneShot(alienDeathSound);
                break;
            case "DeadMan":
                soundEffectsSource.PlayOneShot(playerDeathSound);
                break;
        }
    }
}
