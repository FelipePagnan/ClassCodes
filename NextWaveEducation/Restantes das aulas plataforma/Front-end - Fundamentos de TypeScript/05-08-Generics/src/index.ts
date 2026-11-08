abstract class OpcaoMenu {
    imprimir() {
        console.log(this);
    }
}

class Personagem extends OpcaoMenu {
    nome: string;
    constructor(nome: string) {
        super();
        this.nome = nome;
    }

    imprimir() {
        console.log(`[${this.nome}]`);
    }
}

class Armamento {
}

class Mapa {
    clima: string;
    constructor(clima: string) {
        this.clima = clima;
    }
}

class Menu<T extends OpcaoMenu> {
    private opcoes: T[] = [];

    adicionarOpcao(opcao: T) {
        this.opcoes.push(opcao);
    }

    imprimirOpcoes() {
        this.opcoes.forEach(opcao => opcao.imprimir());
    }
}

let p1 = new Personagem('Saruman');
let p2 = new Personagem('Elrond');
let m1 = new Mapa('Deserto');
let m2 = new Mapa('Floresta');

let menu = new Menu<Personagem>();

menu.adicionarOpcao(p1);
menu.adicionarOpcao(p2);

menu.imprimirOpcoes();