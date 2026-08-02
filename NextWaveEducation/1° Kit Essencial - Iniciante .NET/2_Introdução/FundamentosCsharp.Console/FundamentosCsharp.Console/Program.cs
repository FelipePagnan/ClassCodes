//------------------ TIPOS E VARIÁVEIS ------------------------------------
#region
/*
 * int: Numeros inteiros
 * double: Numeros com ponto flutuante
 * float: Numeros com ponto flutuante
 * decimal: Armazena numeros decimais com alta precisao
 * char: Armazena um unico carctere
 * bool: Armazena valores booleanos
 * string: Armazena palavras
 * var: Um tipo gerar que se modifica após receber o valor
*/

/*
double valorDouble = 3.14;
float valorFloat = 3.14F;
decimal valorDecimal = 3.14m;
int valorInteiro = 100;
bool valorBoolean = false;
char valorChar = 'A';
string valorString = "Pagnan";

const double PI = 3.1416;

Console.WriteLine($"Double: {valorDouble}, Float: {valorFloat}, Decima {valorDecimal}");
Console.WriteLine($"Int: {valorInteiro}, Bool: {valorBoolean}");
Console.WriteLine($"Chat: {valorChar}, String: {valorString}");
*/
#endregion
//------------------ OPERADORES ARITMETICOS -------------------------------
#region
/*
 * Adição ( + ) - Soma dois ou mais valores
 * Substração ( - ) - Subtrai dois ou mais valores
 * Multiplicação ( * ) - Multiplica dois ou mais valores
 * Divisão ( / ) - Divide o valores da esquerda pelo da direita
 * Módulo ( % ) - Retorna o resto da divisão do valor da esquerda pelo da direita
*/

/*
int a = 1;
int b = 2;

int soma = a + b;
int subtracao = a - b;
int multiplicacao = a * b;
int divisao = b / a;
int modulo = a % b;

Console.WriteLine($"Soma: {soma}, Subtracao: {subtracao}, Multiplicacao: {multiplicacao} ," +
    $"Divisao: {divisao}, Modulo: {modulo}");
*/
#endregion
//------------------ OPERADORES DE COMPARACOES ----------------------------
#region
/*
 * Igual ( == ) - Verifica se dois valroes sao iguais
 * Nao igual a ( != ) - Verifica se dois valores sao diferentes
 * Maior que ( > ) - Verifica se o valor a esqueda e maior que o da direita
 * Menor que ( < ) - Verifica se o valor a direita e maior que o da esquerda
 * Maior ou Igual a ( >= ) Verifica se o valor a esqueda e maior ou igual que o da direita
 * Menor ou Igual a ( <= ) Verifica se o valor a direita e maior ou igual que o da esquerda
*/
/*
var p = 5;
var q = 10;

bool maior = p > q;
bool maiorOuIgual = p >= q;
bool menor = p < q;
bool menorOuIgual = p == q;
bool igual = p == q;
bool diferente = p != q;

Console.WriteLine($"Maior: {maior}, Maior Ou Igual: {maiorOuIgual}");
Console.WriteLine($"Menor: {menor}, Menor ou Igual: {menorOuIgual}, " +
                  $"Igual: {igual}, Diferente: {diferente}");
*/
#endregion
//------------------ OPERADORES LOGICOS -----------------------------------
#region
/*
 * AND ( && ) - Retorna true se ambas condicoes forem verdadeiras
 * OR ( || ) - Retorna truse se qualquer uma for verdadeira
 * NOT ( ! ) - Inverte o valor logico da condicao
*/
/*
bool x = true;
bool y = false;

var and = x && y; // AND
var or = x || y; // OR
var notX = !x; // NOT
var notY = !y; // NOT

Console.WriteLine("x = true, y = false");
Console.WriteLine($"AND: {and}, OR: {or}, NOT X: {notX}, NOT Y: {notY}");
*/
#endregion
//------------------ OPERADORES DE IGUALDADE E ATRIBUICAO -----------------
#region
/*
 * Atribuicao ( = )
 * Adicao e atribuicao ( += )
 * Subtracao e atribuicao ( -= )
 * Multiplicacao e atribuicao ( *= )
 * Divisao e atribuicao ( /= )
 * Modulo e atribuicao ( %= )
*/
/*
int k = 5;
Console.WriteLine("K:" + k);
k = 10;
Console.WriteLine("K:" + k);
k += 5; // k = k + 5 = 10 + 5 = 15
Console.WriteLine("K:" + k);
k -= 3; // k = k - 3 = 15 - 3 = 12
Console.WriteLine("K:" + k);
k *= 3; // k = k * 2 = 12 * 2 = 24
Console.WriteLine("K:" + k);
k /= 4; // k = k / 4 = 24 / 4 = 6
Console.WriteLine("K:" + k);
k %= 5; // k = k % 5 =  6 % 5 = 1
Console.WriteLine("K:" + k);
*/

#endregion
//------------------ ESTRUTURAS CONDICIONAIS - if-else --------------------
#region
/*
 * if
 * else if
 * else
*/
/*
var nota = 75;

if( nota >= 70){
    Console.WriteLine("Aprovado!");
} 
else if ( nota >= 30){
    Console.WriteLine("Em Recuperacao!");
}
else{
    Console.WriteLine("Reprovado");
}
*/
#endregion
//------------------ ESTRUTURAS CONDICIONAIS - switch-case ----------------
#region
/*
 * switch case
*/
/*
var opcao = 5;
var nota = 75;

switch (opcao)
{
    case 1:
        Console.WriteLine("Opcao UM");
        break;
    case 2:
        Console.WriteLine("Opcao DOIS");
        break;
    case 3:
        Console.WriteLine("Opcao TRES");
        break;
    default:
        Console.WriteLine("Opcao DESCONHECIDA");
        break;
}

switch (nota){
    case int n when (n >= 70):
        Console.WriteLine("Aprovado");
        break;
    case int n when n <= 30:
        Console.WriteLine("Reprovado!");
        break;
}

string textoNumero = opcao switch
{
    1 => "Um",
    2 => "DOIS",
    3 => "TRES",
    _ => "DESCONHECIDO"
};
Console.WriteLine(textoNumero);
*/
#endregion
//------------------ MATRIZES (ou Arrays) ---------------------------------
#region
/*
int[] matriz = {1, 4, 3, 2, 6};
int[] matrizVazia = new int[5];

var tamanhoMatriz = matriz.Length; //Tamanho
var dimensaoMatriz = matriz.Rank; //Dimensao

Console.WriteLine($"Tamanho: {tamanhoMatriz}, Dimensao: {dimensaoMatriz}");

Array.Sort(matriz);

Array.Reverse(matriz);
*/
#endregion
//------------------ ESTRUTURAS DE REPETICAO ------------------------------
#region
/*
 * for - E usado quando sabendos quantas vezes queremos que um bloco de codigo seja executado
 * foreach - E utlizada para percorrer colecoes (Como matrizes)
 * while - Executa um block de codigo uma condicao for verdadeira
 * do-while - Similar ao While mas as codicoes e avaliada apos a execucao do bloco
*/
int[] matriz = { 1, 4, 3, 2, 6 };

Console.WriteLine("while");
int contador = 0;

while (contador < matriz.Length)
{
    Console.WriteLine(matriz[contador]); // matriz[0], matriz[1], matriz[2]
    contador++;
}

Console.WriteLine("do-while");

contador = 0;

do
{
    Console.WriteLine(matriz[contador]);
    contador++;
} while (contador < matriz.Length);

Console.WriteLine("for");
for (int i = 0; i < matriz.Length; i++)
{
    Console.WriteLine(matriz[i]);
}
Console.WriteLine("foreach");

foreach(int numero in matriz)
{
    Console.WriteLine(numero);
}

#endregion