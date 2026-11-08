let coordenada: [number, number];

coordenada = [10, 20];

console.log(coordenada[0]);
console.log(coordenada[1]);

let [x, y] = coordenada;

console.log(x, y);

type Coordenada = [number, number];

type Coordenada2 = { x: number, y: number };

let casa: Coordenada;
casa = [9,28];

casa[0] = 19;
casa[1] = 29;

console.log(casa);

let escola: Coordenada2 = { x : 99, y: 78 };
console.log(escola.x, escola.y);