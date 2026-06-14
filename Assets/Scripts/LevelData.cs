using UnityEngine;

/// <summary>
/// Dados das fases em formato de texto (mapa ASCII). Cada caractere vira um
/// elemento do jogo, montado em tempo de execucao pelo LevelBuilder.
///
/// Legenda:
///   '#' = bloco solido (chao/plataforma)   'H' = escada (cipo) para escalar
///   'o' = cristal coletavel                '^' = espinhos (perigo)
///   'E' = inimigo                          'P' = inicio do jogador
///   'X' = portal de saida                  ' ' = vazio
/// A primeira string e o topo da fase; a ultima e a base.
/// </summary>
public static class LevelData
{
    public static readonly string[] Names = {
        "1 - Despertar", "2 - Fendas", "3 - Abismo", "4 - A Luz"
    };

    public static readonly string[][] Maps = {
        // FASE 1: 1 - Despertar (largura 48)
        new string[] {
            "                                                ",
            "                                                ",
            "                                                ",
            "                                                ",
            "                                                ",
            "                                                ",
            "                                                ",
            "                                        ooHooo  ",
            "                                        ##H###  ",
            "                                          H     ",
            "           oooo   ooo         oooo        H     ",
            "           ####               ####        H     ",
            "  P   ooo                 E               H   X ",
            "################################################",
            "################################################",
        },
        // FASE 2: 2 - Fendas (largura 58)
        new string[] {
            "                                                          ",
            "                                                          ",
            "                                                          ",
            "                                                          ",
            "                                                          ",
            "                                                          ",
            "                                                 oooHoXoo ",
            "                                                 ###H#### ",
            "                                                    H     ",
            "                                                    H     ",
            "            ooo            ooo              ooo     H     ",
            "                  oo             ooo                H     ",
            "  P  ooo            E  ^^            E  ^^          H     ",
            "############   ############   ##############   ###########",
            "############   ############   ##############   ###########",
        },
        // FASE 3: 3 - Abismo (largura 66)
        new string[] {
            "                                                                  ",
            "                                                                  ",
            "                                                                  ",
            "                                                                  ",
            "                                                      oooHoXoooo  ",
            "                                                      ###H######  ",
            "                                                         H        ",
            "                                                         H        ",
            "                                                         H        ",
            "                                                         H        ",
            "          ooo         ooo                    ooo         H        ",
            "    oo             oo    ooo        ooo                  H        ",
            "  P   E         ^^^         E          ^^^ E             H        ",
            "##########   #########   ########   #########   ##################",
            "##########   #########   ########   #########   ##################",
        },
        // FASE 4: 4 - A Luz (largura 74)
        new string[] {
            "                                                                          ",
            "                                                                          ",
            "                                                                          ",
            "                                                            ooooHoXoo     ",
            "                                                            ####H####     ",
            "                                                                H         ",
            "                                                                H         ",
            "                                                                H         ",
            "                                                                H         ",
            "                                                                H         ",
            "        ooo              ooo               ooo        ooo       H         ",
            "    o             oo        oooo   oooo         oo              H         ",
            "  P  E       ^^E      ^^      E      ^^^ E        ^^            H         ",
            "########   ######   #####   ####   ########   ########   #################",
            "########   ######   #####   ####   ########   ########   #################",
        },
    };
}
