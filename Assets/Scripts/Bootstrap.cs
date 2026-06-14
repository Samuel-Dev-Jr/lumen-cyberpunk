using UnityEngine;

/// <summary>
/// Ponto de entrada do jogo. O método marcado com [RuntimeInitializeOnLoadMethod]
/// é chamado automaticamente pela Unity assim que a cena carrega — tanto no Editor
/// (Play) quanto no executável final. Por isso NÃO é preciso montar nada na cena
/// manualmente: o jogo inteiro é construído por código a partir daqui.
/// </summary>
public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Launch()
    {
        // evita criar dois GameManagers ao recarregar a cena
        if (GameManager.Instance != null) return;

        var go = new GameObject("GameManager");
        var gm = go.AddComponent<GameManager>();
        gm.Init();
    }
}
