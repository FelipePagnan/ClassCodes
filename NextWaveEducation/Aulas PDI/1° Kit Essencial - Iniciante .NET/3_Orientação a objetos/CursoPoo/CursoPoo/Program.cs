#region
var pessoa = new Pessoa();
pessoa.Nome = "Pagnan";
pessoa.Profissao = "Dev";
pessoa.Telefone = "99879798979989";

var pessoa2 = new Pessoa("Pagnan", "Dev","231321231");
pessoa2.Nome = "teste";
//var pessoa = new Pessoa
//{
//    Nome = "Pagnan",
//    Profissao = "Programador",
//   Telefone = "99879798979989"
//};

pessoa.Apresentar();
pessoa.Apresentar("Teste");

var funcionario = new Funcionario("Pagnan Funcionario", "Dev", "123123123123", 15000);
funcionario.Apresentar();

Pessoa[] pessoas = { pessoa, funcionario };

foreach(var p in pessoas)
{
    p.Apresentar();
}

public class Pessoa
{
    public Pessoa() { }

    public Pessoa(string nome, string profissao, string telefone)
    {
        Nome = nome;
        Profissao = profissao;
        Telefone = telefone;
    }

    public string Nome { get; set; }
    public string Profissao { get; set; }
    public string Telefone { get; set; }

    public virtual void Apresentar()
    {
        Console.WriteLine(FormatarMensagem());
    }

    public void Apresentar(string nome)
    {
        Console.WriteLine($"Ola, {nome}: {FormatarMensagem()}");
    }

    private string FormatarMensagem()
    {
        return $"{Nome}, {Profissao}, {Telefone}";
    }
}
#endregion

#region 
// Heranca
public class Funcionario : Pessoa
{
    public Funcionario(string nome, string profissao, string telefone, int salario) : base(nome, profissao, telefone)
    {
        Salario = salario;
    }
    public int Salario { get; set; }

    public override void Apresentar()
    {
        base.Apresentar();
        Console.WriteLine($"Salario: {Salario}");
    }
}

#endregion

#region
//Encapsulamento
#endregion

#region
//Abastracao
#endregion

#region
//Polimorfismo pratica
#endregion
