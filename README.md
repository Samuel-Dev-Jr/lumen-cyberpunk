# 🔆 Lumen — Jogo de Plataforma 2D

> Projeto da disciplina **Game Development** — UniFECAF
> Desenvolvido em **Unity 6 (6000.4)** com **C#** por **Samuel Nunes**

**Lumen** é um jogo de plataforma 2D em que você controla uma pequena **centelha de luz**
presa nas cavernas escuras do subsolo. Para escapar, ela precisa atravessar fendas e abismos,
escalar cipós, desviar de espinhos e de criaturas das sombras, coletando **cristais de luz**
até alcançar o portal de saída de cada fase e, enfim, voltar à superfície.

![Lumen — Fase 1](Docs/screenshots/fase_1.png)

---

## 🎮 Mecânicas

- **Andar** e **correr** (movimento horizontal responsivo)
- **Pular** (com *coyote time*, *jump buffer* e altura variável — sensação de controle "justo")
- **Escalar** cipós/escadas (a gravidade é desativada enquanto agarrado)
- **Pisar** em inimigos para derrotá-los
- **Coletar** cristais que valem pontos
- **Vidas, dano e respawn**: ao cair no abismo você renasce no início da fase; ao zerar as vidas, é Game Over

## ⌨️ Controles

| Ação | Tecla |
|---|---|
| Mover | `←` `→` ou `A` `D` |
| Correr | segurar `Shift` |
| Pular | `Espaço` |
| Escalar (em escadas) | `↑` `↓` ou `W` `S` |
| Reiniciar (após Game Over / Vitória) | `R` |
| Ativar/desativar som | `M` |
| Sair (no executável) | `Esc` |

## 🗺️ Fases

São **4 fases** com dificuldade crescente:

1. **Despertar** — tutorial em terreno plano.
2. **Fendas** — abismos para pular, primeiros espinhos e escalada obrigatória.
3. **Abismo** — verticalidade, mais inimigos e uma torre de escalada longa.
4. **A Luz** — combina todos os desafios em densidade máxima.

---

## ▶️ Como jogar

### Opção 1 — Executável (Windows)
1. Baixe/extraia a pasta `Build/`.
2. Execute **`Lumen.exe`**.

### Opção 2 — No editor Unity
1. Abra o projeto no **Unity 6 (6000.4.11f1)** via Unity Hub (*Add project from disk*).
2. Abra a cena `Assets/Scenes/SampleScene.unity`.
3. Pressione **Play**.

> O jogo é **dirigido por código**: ao iniciar a cena, a classe `Bootstrap` constrói
> automaticamente todo o jogo (personagem, fases, HUD, áudio). Não é preciso montar nada
> manualmente no editor.

## 🛠️ Como exportar (Build)

`File → Build Settings → Windows → Build` e escolha uma pasta (ex.: `Build/`).
A cena `SampleScene` já está incluída na build.

---

## 🧩 Arquitetura técnica

O jogo NÃO usa assets externos: **toda a arte (sprites) e todo o áudio (trilha + efeitos)
são gerados proceduralmente por código**, o que o torna 100% reprodutível.

| Script | Responsabilidade |
|---|---|
| `Bootstrap.cs` | Ponto de entrada (inicia o jogo ao carregar a cena) |
| `GameManager.cs` | Singleton: vidas, pontuação, fases, vitória/derrota |
| `PlayerController.cs` | Movimento (andar/correr/pular/escalar) e animação |
| `LevelData.cs` | Mapas das fases em texto (ASCII) |
| `LevelBuilder.cs` | Constrói a fase a partir do mapa |
| `Enemy.cs` | Inimigo patrulheiro (com pisão) |
| `Collectible.cs` | Cristal coletável |
| `Hazard.cs` | Espinhos (dano) |
| `Ladder.cs` | Marcador de escada |
| `LevelExit.cs` | Portal de saída da fase |
| `HUDController.cs` | Interface (vidas, cristais, pontuação, mensagens) |
| `AudioManager.cs` | Síntese de áudio (chiptune) |
| `SpriteFactory.cs` | Geração procedural de sprites (pixel art) |
| `SpriteAnimator.cs` | Animação por troca de quadros |
| `CameraFollow.cs` | Câmera que segue o jogador com limites |
| `Spark.cs` | Partícula de feedback visual |

## 📁 Estrutura do projeto

```
Lumen/
├── Assets/
│   ├── Scripts/        # todo o código C#
│   └── Scenes/         # SampleScene.unity (cena única)
├── Docs/
│   └── RELATORIO.pdf   # relatório teórico
├── Build/              # executável Windows (após o build)
└── README.md
```

## 🎨 Créditos

- **Arte e áudio:** originais, gerados proceduralmente pelo código do projeto.
- **Fonte do HUD:** fonte interna da Unity (`LegacyRuntime.ttf`).
- **Engine:** Unity 6 • **Linguagem:** C#.

---

*Projeto acadêmico — UniFECAF, 2026.*
