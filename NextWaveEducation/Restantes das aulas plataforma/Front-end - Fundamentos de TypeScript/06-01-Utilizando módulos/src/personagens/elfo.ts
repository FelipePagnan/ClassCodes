import { TipoPersonagem as Tipo } from "../enums/tipo-personagem";
import { Personagem, VIDA_MAX } from "./personagem";

export class Elfo extends Personagem {
    vida: number;

    constructor(nome: string) {
        super(nome, Tipo.Elfo);
        this.vida = VIDA_MAX;
    }

    atacar() {
        console.log(`${this.nome} atacou com arco e flecha.`)
    }
}