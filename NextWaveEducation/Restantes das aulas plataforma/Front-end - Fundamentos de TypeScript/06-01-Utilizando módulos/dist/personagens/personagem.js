"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.VIDA_MAX = exports.Personagem = void 0;
class Personagem {
    constructor(nome, tipo) {
        this.nome = nome;
        this.tipo = tipo;
    }
    caminhar() {
        console.log(`${this.nome} caminhou.`);
    }
}
exports.Personagem = Personagem;
const VIDA_MAXIMA = 100;
exports.VIDA_MAX = VIDA_MAXIMA;
