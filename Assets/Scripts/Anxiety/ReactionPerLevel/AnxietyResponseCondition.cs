using System;
using System.Collections;
using Anxiety;
using UnityEngine;
using Random = UnityEngine.Random;

public class AnxietyResponseCondition : PlayerModule
{
    [Serializable]
    public class LevelAudio
    {
        public int fearLevel;
        public AudioClip[] clips;
        public bool allowReplay = true;
        [HideInInspector] public bool hasPlayed = false;
        public bool playSequentially = false;         // New: play in order
        [HideInInspector] public int nextIndex = 0;   // New: next clip index
    }

    [Header("Level Audio Settings")]
    [SerializeField] private LevelAudio[] levelAudios;

    public bool IsAudioPlaying => _audioPlaying;
    private bool _audioPlaying = false;
    private int _currentLevel = -1;
    private AudioSource _currentResponse;
    private PlayerMonologue _playerMonologue;
    private bool _wasMonologuePlaying = false;
    private int _pendingLevel = -1;
    public event Action OnConditionAudioStarted;

    private void Start()
    {
        _playerMonologue = Player.Instance.GetModule<PlayerMonologue>();
        _currentLevel = AnxietyManager.Instance.DeterminePassiveFearLevel();
    }

    private void Update()
    {
        bool nowPlaying = _playerMonologue != null && _playerMonologue.isPlaying;

        // detect monologue end
        if (_wasMonologuePlaying && !nowPlaying && _pendingLevel != -1)
        {
            TryPlayAudioForLevel(_pendingLevel, ignoreMonologue: true);
            _pendingLevel = -1;
        }
        _wasMonologuePlaying = nowPlaying;

        if (nowPlaying && _audioPlaying)
        {
            // interrupt any playing response
            if (_currentResponse != null)
            {
                Destroy(_currentResponse);
                _audioPlaying = false;
            }
        }

        int newLevel = AnxietyManager.Instance.DeterminePassiveFearLevel();
        if (newLevel != _currentLevel)
        {
            TryPlayAudioForLevel(newLevel);
            _currentLevel = newLevel;
        }
    }

    private void TryPlayAudioForLevel(int level, bool ignoreMonologue = false)
    {
        var lvl = GetLevelAudio(level);
        if (lvl == null || lvl.clips == null || lvl.clips.Length == 0)
            return;

        if (lvl.hasPlayed && !lvl.allowReplay)
            return;

        bool monologuePlaying = _playerMonologue != null && _playerMonologue.isPlaying;
        if (monologuePlaying && !ignoreMonologue)
        {
            _pendingLevel = level;
            return;
        }

        PlayClip(lvl);
    }

    private void PlayClip(LevelAudio lvl)
    {
        if (_audioPlaying) return;

        AudioClip clip;
        if (lvl.playSequentially)
        {
            clip = lvl.clips[lvl.nextIndex];
            lvl.nextIndex = (lvl.nextIndex + 1) % lvl.clips.Length;
        }
        else
        {
            clip = lvl.clips[Random.Range(0, lvl.clips.Length)];
        }
        if (clip == null) return;

        _audioPlaying = true;
        lvl.hasPlayed = true;
        _currentResponse = SoundFXManager.Instance.Play2DSoundFXClipDestroyOn(
            clip, transform, volume: 1f, destroyTime: clip.length, onLoop: false, audioMixerGroup: null);

        OnConditionAudioStarted?.Invoke();
        StartCoroutine(ResetAudioFlagAfter(clip.length));
    }

    private IEnumerator ResetAudioFlagAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        _audioPlaying = false;
    }

    private LevelAudio GetLevelAudio(int level)
    {
        foreach (var la in levelAudios)
            if (la.fearLevel == level)
                return la;
        return null;
    }

    public void ResetAllPlays()
    {
        foreach (var la in levelAudios)
        {
            la.hasPlayed = false;
            la.nextIndex = 0;
        }
        _pendingLevel = -1;
    }
}