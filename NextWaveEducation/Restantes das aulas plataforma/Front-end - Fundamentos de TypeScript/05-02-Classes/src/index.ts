type Direcao = "cima" | "baixo" | "esquerda" | "direita";

type Movimento = {
    direcao: Direcao;
    passos: number;
}

type TipoPersonagem = "Mago" | "Elfo";

class Personagem {
    nome: string;
    tipo: TipoPersonagem;

    constructor(nome: string, tipo: TipoPersonagem) {
        this.nome = nome;
        this.tipo = tipo;
    }

    caminhar(movimento: Movimento) {
        console.log(`${this.nome} caminhou ${movimento.passos} passos para ${movimento.direcao}`);
    }
}

let gandalf = new Personagem('Gandalf', 'Mago');
let legolas = new Personagem('Legolas', 'Elfo');

gandalf.caminhar({ direcao: "cima", passos: 5 });
legolas.caminhar({ direcao: "baixo", passos: 4 });