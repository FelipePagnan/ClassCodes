interface Personagem {
    nome: string;
    tipo: string;

    caminhar() : void;
}

let p1 : Personagem;

p1 = {
    nome: 'Gandalf',
    tipo: 'Mago',
    caminhar: () => {
        console.log('Personagem caminhou.');
    }
};

p1.caminhar();

class Mago implements Personagem {
    nome: string;
    tipo: string;

    constructor(nome: string) {
        this.nome = nome;
        this.tipo = 'Mago';
    }

    caminhar(): void {
        console.log('O mago caminhou usando magia.');
    }
}

class Elfo implements Personagem {
    nome: string;
    tipo: string;

    constructor(nome: string) {
        this.nome = nome;
        this.tipo = 'Elfo';
    }

    caminhar(): void {
        console.log('O elfo caminhou saltando.');
    }
}

let gandalf = new Mago('Gandalf');

gandalf.caminhar();

let player1 : Personagem;

player1 = new Mago('Galdanf');
player1 = new Elfo('Legolas');

player1.caminhar();