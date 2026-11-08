import { TipoPersonagem } from "../enums/tipo-personagem";
import { Personagem } from "./personagem";

export class Mago extends Personagem {
    constructor(nome: string) {
        super(nome, TipoPersonagem.Mago);
    }

    caminhar() {
        console.log(`${this.nome} voou.`);
    }

    atacar() {
        console.log(`${this.nome} atacou com magia.`)
    }
}