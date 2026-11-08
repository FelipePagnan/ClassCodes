function calcularNovoPreco(precoOriginal: number, taxaReajuste: number) : number {
    let novoPreco = precoOriginal * (1 + taxaReajuste / 100);
    return novoPreco;
}

let precoAjustado = calcularNovoPreco(100, -50);

console.log(precoAjustado);