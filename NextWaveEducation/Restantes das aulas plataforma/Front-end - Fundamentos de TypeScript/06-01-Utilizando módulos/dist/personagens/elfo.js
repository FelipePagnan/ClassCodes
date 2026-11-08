"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.Elfo = void 0;
const tipo_personagem_1 = require("../enums/tipo-personagem");
const personagem_1 = require("./personagem");
class Elfo extends personagem_1.Personagem {
    constructor(nome) {
        super(nome, tipo_personagem_1.TipoPersonagem.Elfo);
        this.vida = personagem_1.VIDA_MAX;
    }
    atacar() {
        console.log(`${this.nome} atacou com arco e flecha.`);
    }
}
exports.Elfo = Elfo;
