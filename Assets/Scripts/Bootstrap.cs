using UnityEngine;

// por onde o jogo comeca. o metodo com [RuntimeInitializeOnLoadMethod] a Unity
// chama sozinha assim que a cena carrega, tanto no Play do editor quanto no .exe.
// por isso nao preciso montar nada na cena na mao, comeca tudo daqui.
public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Launch()
    {
        // se ja existe um GameManager nao cria outro (acontece ao recarregar a cena)
        if (GameManager.Instance != null) return;

        var go = new GameObject("GameManager");
        var gm = go.AddComponent<GameManager>();
        gm.Init();
    }
}
