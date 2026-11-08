"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.Mago = void 0;
const tipo_personagem_1 = require("../enums/tipo-personagem");
const personagem_1 = require("./personagem");
class Mago extends personagem_1.Personagem {
    constructor(nome) {
        super(nome, tipo_personagem_1.TipoPersonagem.Mago);
    }
    caminhar() {
        console.log(`${this.nome} voou.`);
    }
    atacar() {
        console.log(`${this.nome} atacou com magia.`);
    }
}
exports.Mago = Mago;
