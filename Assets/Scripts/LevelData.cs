// aqui ficam as fases desenhadas em texto. cada caractere o LevelBuilder transforma
// num objeto la na hora de montar a fase.
// legenda dos caracteres:
//   '#'=bloco  'H'=escada  'o'=cristal  '^'=espinho  'E'=inimigo  'F'=drone
//   'W'=arma  'L'=coracao  'B'=chefe  'P'=inicio  'X'=saida  ' '=vazio
public static class LevelData
{
    public static readonly string[] Names = {
        "1 - Boot", "2 - Submundo", "3 - Arranha-Ceu", "4 - O Nucleo", "5 - MAINFRAME"
    };

    public static readonly string[][] Maps = {
        // FASE 1: 1 - Boot (largura 48)
        new string[] {
            "                                                ",
            "                                                ",
            "                                                ",
            "                                                ",
            "                                                ",
            "                                                ",
            "                                                ",
            "                                        ooHooo  ",
            "                                    F   ##H###  ",
            "                                          H     ",
            "           oooo   ooo         oooo        H     ",
            "           ####               ####        H     ",
            "  P   ooo             W   E               H   X ",
            "################################################",
            "################################################",
        },
        // FASE 2: 2 - Submundo (largura 58)
        new string[] {
            "                                                          ",
            "                                                          ",
            "                                                          ",
            "                                                          ",
            "                                                          ",
            "                                                          ",
            "                                                 oooHoXoo ",
            "                                                 ###H#### ",
            "                            F                       H     ",
            "                                                    H     ",
            "            ooo            ooo              ooo     H     ",
            "                  oo             ooo                H     ",
            "  P  ooo W          E  ^^            E  ^^        L H     ",
            "############   ############   ##############   ###########",
            "############   ############   ##############   ###########",
        },
        // FASE 3: 3 - Arranha-Ceu (largura 66)
        new string[] {
            "                                                                  ",
            "                                                                  ",
            "                                                                  ",
            "                                                                  ",
            "                                                      oooHoXoooo  ",
            "                                                      ###H######  ",
            "                                                         H        ",
            "                                                         H        ",
            "                 F                                       H        ",
            "                                        F                H        ",
            "          ooo         ooo                    ooo         H        ",
            "    oo             oo    ooo        ooo                  H        ",
            "  P   E W       ^^^         E          ^^^ E      W      H        ",
            "##########   #########   ########   #########   ##################",
            "##########   #########   ########   #########   ##################",
        },
        // FASE 4: 4 - O Nucleo (largura 74)
        new string[] {
            "                                                                          ",
            "                                                                          ",
            "                                                                          ",
            "                                                            ooooHoXoo     ",
            "                                                            ####H####     ",
            "                                                                H         ",
            "                                                                H         ",
            "                                                                H         ",
            "                      F                                         H         ",
            "                                      F                         H         ",
            "        ooo              ooo               ooo        ooo       H         ",
            "    o             oo        oooo   oooo                         H         ",
            "  P  EW      ^^E      ^^      E      ^^^ E      W ^^            H         ",
            "########   ######   #####   ####   ########   ########   #################",
            "########   ######   #####   ####   ########   ########   #################",
        },
        // FASE 5: 5 - MAINFRAME (largura 42)
        new string[] {
            "                                          ",
            "                                          ",
            "                                          ",
            "                                          ",
            "                                          ",
            "                                          ",
            "                                          ",
            "              F             F             ",
            "        ooo                    ooo        ",
            "        ###                    ###        ",
            "                                          ",
            "                    B                     ",
            "   P   W                          W   L   ",
            "##########################################",
            "##########################################",
        },
    };
}
