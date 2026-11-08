using BancoSangue.Console.Entidades;
using BancoSangue.Console.Enums;
using BancoSangue.Console.Excecoes;
using BancoSangue.Console.Servicos;

namespace BancoSangue.Console;

public static class Program
{
    private static readonly DoadorService _doadorService = new();
    private static readonly EstoqueService _estoqueService = new();
    private static readonly DoacaoService _doacaoService = new(_doadorService, _estoqueService);
    private static readonly CepService _cepService = new();

    public static async Task Main(string[] args)
    {
        System.Console.OutputEncoding = System.Text.Encoding.UTF8;
        CarregarDadosDeExemplo();

        var sair = false;
        while (!sair)
        {
            ExibirMenu();
            var opcao = System.Console.ReadLine();
            System.Console.WriteLine();

            // Entrada encerrada (Ctrl+D ou stdin redirecionado): encerra o programa.
            if (opcao is null) break;

            try
            {
                switch (opcao)
                {
                    case "1": await CadastrarDoadorAsync(); break;
                    case "2": RegistrarDoacao(); break;
                    case "3": ConsultarHistoricoDoador(); break;
                    case "4": ListarDoadores(); break;
                    case "5": ExibirEstoque(); break;
                    case "6": RelatorioEstoquePorTipo(); break;
                    case "7": RelatorioDoacoesUltimos30Dias(); break;
                    case "0": sair = true; break;
                    default:
                        System.Console.WriteLine("Opção inválida.");
                        break;
                }
            }
            catch (RegraNegocioException ex)
            {
                System.Console.WriteLine($"[Regra de negócio] {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[Erro inesperado] {ex.Message}");
            }

            if (!sair)
            {
                System.Console.WriteLine("\nPressione ENTER para continuar...");
                System.Console.ReadLine();
            }
        }

        System.Console.WriteLine("Encerrando. Até logo!");
    }

    private static void ExibirMenu()
    {
        System.Console.Clear();
        System.Console.WriteLine("=====================================================");
        System.Console.WriteLine(" SISTEMA DE BANCO DE DOAÇÃO DE SANGUE - Nível Básico I");
        System.Console.WriteLine("=====================================================");
        System.Console.WriteLine("1 - Cadastrar doador");
        System.Console.WriteLine("2 - Registrar doação");
        System.Console.WriteLine("3 - Consultar histórico de doações de um doador");
        System.Console.WriteLine("4 - Listar doadores");
        System.Console.WriteLine("5 - Ver estoque de sangue");
        System.Console.WriteLine("6 - Relatório: quantidade total de sangue por tipo");
        System.Console.WriteLine("7 - Relatório: doações nos últimos 30 dias");
        System.Console.WriteLine("0 - Sair");
        System.Console.Write("\nEscolha uma opção: ");
    }

    private static async Task CadastrarDoadorAsync()
    {
        System.Console.WriteLine("--- Cadastro de Doador ---");

        System.Console.Write("Nome completo: ");
        var nome = System.Console.ReadLine() ?? string.Empty;

        System.Console.Write("E-mail: ");
        var email = System.Console.ReadLine() ?? string.Empty;

        System.Console.Write("Data de nascimento (dd/MM/yyyy): ");
        var dataNascimentoTexto = System.Console.ReadLine();
        if (!DateTime.TryParseExact(dataNascimentoTexto, "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var dataNascimento))
        {
            System.Console.WriteLine("Data inválida.");
            return;
        }

        System.Console.Write("Gênero (Masculino/Feminino/Outro): ");
        if (!Enum.TryParse<Genero>(System.Console.ReadLine(), true, out var genero))
        {
            System.Console.WriteLine("Gênero inválido.");
            return;
        }

        System.Console.Write("Peso (kg): ");
        if (!double.TryParse(System.Console.ReadLine(), out var peso))
        {
            System.Console.WriteLine("Peso inválido.");
            return;
        }

        System.Console.Write("Tipo sanguíneo (A/B/AB/O): ");
        if (!Enum.TryParse<TipoSanguineo>(System.Console.ReadLine(), true, out var tipoSanguineo))
        {
            System.Console.WriteLine("Tipo sanguíneo inválido.");
            return;
        }

        System.Console.Write("Fator Rh (Positivo/Negativo): ");
        if (!Enum.TryParse<FatorRh>(System.Console.ReadLine(), true, out var fatorRh))
        {
            System.Console.WriteLine("Fator Rh inválido.");
            return;
        }

        var doador = new Doador
        {
            NomeCompleto = nome,
            Email = email,
            DataNascimento = dataNascimento,
            Genero = genero,
            Peso = peso,
            TipoSanguineo = tipoSanguineo,
            FatorRh = fatorRh
        };

        System.Console.Write("Deseja informar o endereço via CEP? (s/n): ");
        if ((System.Console.ReadLine() ?? "n").Trim().Equals("s", StringComparison.OrdinalIgnoreCase))
        {
            System.Console.Write("CEP: ");
            var cep = System.Console.ReadLine() ?? string.Empty;
            System.Console.WriteLine("Consultando CEP (requer internet)...");
            var endereco = await _cepService.ConsultarAsync(cep);
            if (endereco is not null)
            {
                doador.Endereco = endereco;
                System.Console.WriteLine($"Endereço encontrado: {endereco}");
            }
            else
            {
                System.Console.WriteLine("Não foi possível consultar o CEP. Cadastro seguirá sem endereço.");
            }
        }

        var doadorCadastrado = _doadorService.Cadastrar(doador);
        System.Console.WriteLine($"\nDoador cadastrado com sucesso! {doadorCadastrado}");
    }

    private static void RegistrarDoacao()
    {
        System.Console.WriteLine("--- Registro de Doação ---");
        ListarDoadores();

        System.Console.Write("\nId do doador: ");
        if (!int.TryParse(System.Console.ReadLine(), out var doadorId))
        {
            System.Console.WriteLine("Id inválido.");
            return;
        }

        System.Console.Write("Quantidade doada em ml (420 a 470): ");
        if (!int.TryParse(System.Console.ReadLine(), out var quantidade))
        {
            System.Console.WriteLine("Quantidade inválida.");
            return;
        }

        var (doacao, estoqueBaixo) = _doacaoService.Registrar(doadorId, quantidade);
        System.Console.WriteLine($"\nDoação registrada com sucesso: {doacao}");

        if (estoqueBaixo)
        {
            var doador = _doadorService.BuscarPorId(doadorId)!;
            System.Console.WriteLine($"[ALERTA] O estoque do tipo {doador.TipoSanguineoCompleto} está abaixo do mínimo recomendado!");
        }
    }

    private static void ConsultarHistoricoDoador()
    {
        System.Console.WriteLine("--- Histórico de Doações ---");
        ListarDoadores();

        System.Console.Write("\nId do doador: ");
        if (!int.TryParse(System.Console.ReadLine(), out var doadorId))
        {
            System.Console.WriteLine("Id inválido.");
            return;
        }

        var historico = _doadorService.ConsultarHistoricoDoacoes(doadorId).ToList();
        if (historico.Count == 0)
        {
            System.Console.WriteLine("Nenhuma doação registrada para este doador.");
            return;
        }

        foreach (var doacao in historico)
            System.Console.WriteLine($"  {doacao}");
    }

    private static void ListarDoadores()
    {
        var doadores = _doadorService.ListarTodos();
        if (doadores.Count == 0)
        {
            System.Console.WriteLine("Nenhum doador cadastrado.");
            return;
        }

        System.Console.WriteLine("--- Doadores Cadastrados ---");
        foreach (var doador in doadores)
            System.Console.WriteLine($"  {doador}");
    }

    private static void ExibirEstoque()
    {
        System.Console.WriteLine("--- Estoque de Sangue ---");
        foreach (var item in _estoqueService.ListarTodos())
            System.Console.WriteLine($"  {item}");
    }

    private static void RelatorioEstoquePorTipo()
    {
        System.Console.WriteLine("--- Relatório: Quantidade Total de Sangue por Tipo ---");
        foreach (var item in _estoqueService.ListarTodos().OrderBy(e => e.Descricao))
            System.Console.WriteLine($"  {item.Descricao}: {item.QuantidadeML}ml");

        var totalGeral = _estoqueService.ListarTodos().Sum(e => e.QuantidadeML);
        System.Console.WriteLine($"\n  TOTAL GERAL: {totalGeral}ml");
    }

    private static void RelatorioDoacoesUltimos30Dias()
    {
        System.Console.WriteLine("--- Relatório: Doações nos Últimos 30 Dias ---");
        var doacoes = _doacaoService.RelatorioUltimos30Dias().ToList();

        if (doacoes.Count == 0)
        {
            System.Console.WriteLine("Nenhuma doação registrada nos últimos 30 dias.");
            return;
        }

        foreach (var doacao in doacoes)
        {
            var doador = doacao.Doador ?? _doadorService.BuscarPorId(doacao.DoadorId);
            System.Console.WriteLine(
                $"  {doacao.DataDoacao:dd/MM/yyyy} - {doador?.NomeCompleto} ({doador?.Email}) - {doacao.QuantidadeML}ml - Tipo {doador?.TipoSanguineoCompleto}");
        }
    }

    /// <summary>
    /// Carrega alguns dados de exemplo para facilitar os testes manuais do sistema.
    /// </summary>
    private static void CarregarDadosDeExemplo()
    {
        var joao = _doadorService.Cadastrar(new Doador
        {
            NomeCompleto = "João da Silva",
            Email = "joao.silva@example.com",
            DataNascimento = new DateTime(1990, 5, 10),
            Genero = Genero.Masculino,
            Peso = 78,
            TipoSanguineo = TipoSanguineo.O,
            FatorRh = FatorRh.Negativo
        });

        var maria = _doadorService.Cadastrar(new Doador
        {
            NomeCompleto = "Maria Oliveira",
            Email = "maria.oliveira@example.com",
            DataNascimento = new DateTime(1995, 3, 22),
            Genero = Genero.Feminino,
            Peso = 62,
            TipoSanguineo = TipoSanguineo.A,
            FatorRh = FatorRh.Positivo
        });

        _doadorService.Cadastrar(new Doador
        {
            NomeCompleto = "Pedro Menor",
            Email = "pedro.menor@example.com",
            DataNascimento = DateTime.Today.AddYears(-16),
            Genero = Genero.Masculino,
            Peso = 55,
            TipoSanguineo = TipoSanguineo.B,
            FatorRh = FatorRh.Positivo
        });

        _doacaoService.Registrar(joao.Id, 450, DateTime.Now.AddDays(-10));
        _doacaoService.Registrar(maria.Id, 430, DateTime.Now.AddDays(-5));
    }
}
