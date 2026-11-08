// See https://aka.ms/new-console-template for more information
using CursoPooNotas;

Console.WriteLine("Hello, World!");

var gerenciador = new Gerenciador();

while (true)
{
    Console.WriteLine("1. Adicionar Nota");
    Console.WriteLine("2. Exibir");
    Console.WriteLine("3. Sair");

    var opcao = Console.ReadLine();

    switch (opcao)
    {
        case "1":
            Console.WriteLine("Digite uma descricao");
            var titulo = Console.ReadLine();

            gerenciador.Adicionar(titulo);
            break;
        case "2":
            gerenciador.ExibirTodas();
            break;
        case "3":
            return;
        default: 
            Console.WriteLine("Opcao invalida");
            break;
    }
}