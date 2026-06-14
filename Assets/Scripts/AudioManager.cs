using UnityEngine;

/// <summary>
/// Gera TODO o áudio do jogo por código (síntese procedural): efeitos sonoros
/// estilo "chiptune" e uma trilha sonora em loop. Assim não dependemos de
/// arquivos de áudio externos — tudo é criado em tempo de execução com ondas.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    const int SR = 44100; // taxa de amostragem

    AudioSource _sfx;   // efeitos (PlayOneShot)
    AudioSource _music; // trilha em loop

    AudioClip _jump, _coin, _hurt, _stomp, _levelUp, _win, _over, _theme;

    public bool Muted { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _sfx = gameObject.AddComponent<AudioSource>();
        _sfx.playOnAwake = false;
        _music = gameObject.AddComponent<AudioSource>();
        _music.playOnAwake = false;
        _music.loop = true;
        _music.volume = 0.30f;

        BuildClips();
    }

    void BuildClips()
    {
        _jump = Sweep("jump", 320f, 720f, 0.14f, 0.6f);
        _coin = Sequence("coin", new[] { 988f, 1319f }, 0.06f, 0.5f);
        _hurt = NoiseSweep("hurt", 520f, 110f, 0.30f, 0.55f);
        _stomp = Sweep("stomp", 200f, 90f, 0.12f, 0.6f);
        _levelUp = Sequence("levelup", new[] { 523f, 659f, 784f, 1047f }, 0.09f, 0.5f);
        _win = Sequence("win", new[] { 523f, 659f, 784f, 1047f, 784f, 1047f, 1319f }, 0.11f, 0.5f);
        _over = Sequence("over", new[] { 392f, 330f, 262f, 196f }, 0.18f, 0.55f);
        _theme = BuildTheme();
    }

    // ---------- Geradores de onda ----------
    static float Square(float phase) { return (phase % 1f) < 0.5f ? 1f : -1f; }

    // Envelope simples: ataque rápido + decaimento exponencial
    static float Env(float t, float dur)
    {
        float attack = 0.005f;
        if (t < attack) return t / attack;
        return Mathf.Exp(-(t - attack) * 6f / dur);
    }

    AudioClip Sweep(string name, float fromHz, float toHz, float dur, float vol)
    {
        int n = Mathf.CeilToInt(dur * SR);
        var data = new float[n];
        float phase = 0f;
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float f = Mathf.Lerp(fromHz, toHz, t / dur);
            phase += f / SR;
            data[i] = Square(phase) * Env(t, dur) * vol;
        }
        return ToClip(name, data);
    }

    AudioClip NoiseSweep(string name, float fromHz, float toHz, float dur, float vol)
    {
        int n = Mathf.CeilToInt(dur * SR);
        var data = new float[n];
        float phase = 0f;
        var rng = new System.Random(12345);
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / SR;
            float f = Mathf.Lerp(fromHz, toHz, t / dur);
            phase += f / SR;
            float tone = Square(phase);
            float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
            data[i] = (tone * 0.7f + noise * 0.3f) * Env(t, dur) * vol;
        }
        return ToClip(name, data);
    }

    AudioClip Sequence(string name, float[] notes, float noteDur, float vol)
    {
        int per = Mathf.CeilToInt(noteDur * SR);
        var data = new float[per * notes.Length];
        for (int k = 0; k < notes.Length; k++)
        {
            float phase = 0f;
            for (int i = 0; i < per; i++)
            {
                float t = (float)i / SR;
                phase += notes[k] / SR;
                data[k * per + i] = Square(phase) * Env(t, noteDur) * vol;
            }
        }
        return ToClip(name, data);
    }

    // ---------- Trilha sonora (loop) ----------
    AudioClip BuildTheme()
    {
        float beat = 0.25f;          // duração de cada nota (semínima rápida)
        // Melodia em Lá menor pentatônica (0 = silêncio)
        float[] mel = {
            440, 523, 659, 523,  587, 523, 440, 0,
            392, 440, 523, 440,  330, 392, 440, 0,
            440, 523, 659, 784,  659, 523, 440, 0,
            587, 523, 440, 392,  440, 0,   330, 0
        };
        // Linha de baixo (uma nota grave a cada 4 batidas) — progressão Am F C G
        float[] bass = { 220, 220, 220, 220, 174.61f, 174.61f, 174.61f, 174.61f,
                         261.63f, 261.63f, 261.63f, 261.63f, 196f, 196f, 196f, 196f,
                         220, 220, 220, 220, 174.61f, 174.61f, 174.61f, 174.61f,
                         261.63f, 261.63f, 261.63f, 261.63f, 196f, 196f, 196f, 196f };

        int per = Mathf.CeilToInt(beat * SR);
        int n = per * mel.Length;
        var data = new float[n];
        float bassPhase = 0f;

        for (int k = 0; k < mel.Length; k++)
        {
            float melPhase = 0f;
            for (int i = 0; i < per; i++)
            {
                int idx = k * per + i;
                float t = (float)i / SR;
                float s = 0f;
                if (mel[k] > 0f)
                {
                    melPhase += mel[k] / SR;
                    s += Square(melPhase) * 0.18f * Mathf.Exp(-t * 1.5f / beat);
                }
                bassPhase += bass[k] / SR;
                s += Square(bassPhase) * 0.10f;
                data[idx] = s;
            }
        }
        // pequeno fade nas pontas para o loop não estalar
        int fade = 300;
        for (int i = 0; i < fade && i < n; i++)
        {
            float g = (float)i / fade;
            data[i] *= g;
            data[n - 1 - i] *= g;
        }
        return ToClip("theme", data);
    }

    AudioClip ToClip(string name, float[] data)
    {
        var clip = AudioClip.Create(name, data.Length, 1, SR, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ---------- API pública ----------
    void Play(AudioClip c, float vol = 1f) { if (!Muted && c != null) _sfx.PlayOneShot(c, vol); }

    public void PlayJump() => Play(_jump, 0.5f);
    public void PlayCoin() => Play(_coin, 0.6f);
    public void PlayHurt() => Play(_hurt, 0.6f);
    public void PlayStomp() => Play(_stomp, 0.6f);
    public void PlayLevelComplete() => Play(_levelUp, 0.7f);
    public void PlayWin() => Play(_win, 0.8f);
    public void PlayGameOver() => Play(_over, 0.7f);

    public void StartMusic()
    {
        if (_music.clip == null) _music.clip = _theme;
        if (!Muted && !_music.isPlaying) _music.Play();
    }

    public void ToggleMute()
    {
        Muted = !Muted;
        if (Muted) _music.Pause();
        else _music.UnPause();
    }
}
