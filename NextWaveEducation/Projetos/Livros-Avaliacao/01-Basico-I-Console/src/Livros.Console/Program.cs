using System.Globalization;
using Livros.Console.Entidades;
using Livros.Console.Enums;
using Livros.Console.Excecoes;
using Livros.Console.Servicos;

namespace Livros.Console;

public static class Program
{
    private static readonly LivroService _livroService = new();
    private static readonly UsuarioService _usuarioService = new();
    private static readonly AvaliacaoService _avaliacaoService = new(_livroService, _usuarioService);
    private static readonly ConsultaLivroExternoService _consultaExterna = new();

    private static readonly CultureInfo _cultura = CultureInfo.InvariantCulture;

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
                switch (opcao.Trim())
                {
                    case "1": CadastrarLivro(); break;
                    case "2": ListarLivros(); break;
                    case "3": DetalharLivro(); break;
                    case "4": AtualizarLivro(); break;
                    case "5": RemoverLivro(); break;
                    case "6": DefinirCapa(); break;
                    case "7": await ConsultarApiExternaAsync(); break;
                    case "8": CadastrarUsuario(); break;
                    case "9": ListarUsuarios(); break;
                    case "10": DetalharUsuario(); break;
                    case "11": RemoverUsuario(); break;
                    case "12": CadastrarAvaliacao(); break;
                    case "13": ListarAvaliacoesDoLivro(); break;
                    case "14": RelatorioAnual(); break;
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
                if (System.Console.ReadLine() is null) break;
            }
        }

        System.Console.WriteLine("Encerrando. Boas leituras!");
    }

    private static void ExibirMenu()
    {
        System.Console.Clear();
        System.Console.WriteLine("==========================================================");
        System.Console.WriteLine(" GERENCIADOR DE AVALIAÇÃO DE LIVROS - Nível Básico I");
        System.Console.WriteLine("==========================================================");
        System.Console.WriteLine(" LIVROS");
        System.Console.WriteLine("  1 - Cadastrar livro");
        System.Console.WriteLine("  2 - Listar livros");
        System.Console.WriteLine("  3 - Detalhes de um livro");
        System.Console.WriteLine("  4 - Atualizar livro");
        System.Console.WriteLine("  5 - Remover livro");
        System.Console.WriteLine("  6 - Definir imagem de capa");
        System.Console.WriteLine("  7 - Consultar livro em API externa (Google Books)");
        System.Console.WriteLine(" USUÁRIOS");
        System.Console.WriteLine("  8 - Cadastrar usuário");
        System.Console.WriteLine("  9 - Listar usuários");
        System.Console.WriteLine(" 10 - Detalhes de um usuário (com avaliações)");
        System.Console.WriteLine(" 11 - Remover usuário");
        System.Console.WriteLine(" AVALIAÇÕES");
        System.Console.WriteLine(" 12 - Cadastrar avaliação");
        System.Console.WriteLine(" 13 - Listar avaliações de um livro");
        System.Console.WriteLine(" RELATÓRIOS");
        System.Console.WriteLine(" 14 - Livros lidos em um ano");
        System.Console.WriteLine("  0 - Sair");
        System.Console.Write("\nEscolha uma opção: ");
    }

    // ------------------------------- LIVROS -------------------------------

    private static void CadastrarLivro()
    {
        System.Console.WriteLine("--- Cadastro de Livro ---");

        var livro = new Livro
        {
            Titulo = Perguntar("Título"),
            Autor = Perguntar("Autor"),
            ISBN = Perguntar("ISBN (10 ou 13 dígitos)"),
            Editora = Perguntar("Editora"),
            Descricao = Perguntar("Descrição")
        };

        System.Console.WriteLine("\nGêneros disponíveis:");
        ExibirGeneros();
        livro.Genero = PerguntarGenero();

        livro.AnoDePublicacao = PerguntarInteiro("Ano de publicação");
        livro.QuantidadePaginas = PerguntarInteiro("Quantidade de páginas");

        var cadastrado = _livroService.Cadastrar(livro);
        System.Console.WriteLine($"\nLivro cadastrado com sucesso!\n  {cadastrado}");
    }

    private static void ListarLivros()
    {
        var livros = _livroService.ListarTodos();
        if (livros.Count == 0)
        {
            System.Console.WriteLine("Nenhum livro cadastrado.");
            return;
        }

        System.Console.WriteLine("--- Livros Cadastrados ---");
        foreach (var livro in livros.OrderByDescending(l => l.NotaMedia))
            System.Console.WriteLine($"  {livro}");
    }

    private static void DetalharLivro()
    {
        ListarLivros();
        var id = PerguntarInteiro("\nId do livro");
        var livro = _livroService.ObterPorId(id);

        System.Console.WriteLine($"\n--- {livro.Titulo} ---");
        System.Console.WriteLine($"  Autor............: {livro.Autor}");
        System.Console.WriteLine($"  Editora..........: {livro.Editora}");
        System.Console.WriteLine($"  ISBN.............: {livro.ISBN}");
        System.Console.WriteLine($"  Gênero...........: {livro.Genero}");
        System.Console.WriteLine($"  Ano de publicação: {livro.AnoDePublicacao}");
        System.Console.WriteLine($"  Páginas..........: {livro.QuantidadePaginas}");
        System.Console.WriteLine($"  Nota média.......: {livro.NotaMedia:0.00} ({livro.Avaliacoes.Count} avaliação(ões))");
        System.Console.WriteLine($"  Capa.............: {(livro.TemCapa ? $"{livro.CapaLivro!.Length} bytes" : "sem capa")}");
        System.Console.WriteLine($"  Cadastrado em....: {livro.DataCriacao:dd/MM/yyyy HH:mm}");
        System.Console.WriteLine($"  Descrição........: {livro.Descricao}");

        if (livro.Avaliacoes.Count == 0)
        {
            System.Console.WriteLine("\n  Nenhuma avaliação ainda.");
            return;
        }

        System.Console.WriteLine("\n  Avaliações:");
        foreach (var avaliacao in livro.Avaliacoes.OrderByDescending(a => a.DataCriacao))
        {
            System.Console.WriteLine(
                $"    {avaliacao.Estrelas} ({avaliacao.Nota}/5) por {avaliacao.Usuario?.Nome} " +
                $"- leu em {avaliacao.DiasDeLeitura} dia(s)");
            if (!string.IsNullOrWhiteSpace(avaliacao.Descricao))
                System.Console.WriteLine($"      \"{avaliacao.Descricao}\"");
        }
    }

    private static void AtualizarLivro()
    {
        ListarLivros();
        var id = PerguntarInteiro("\nId do livro a atualizar");
        var atual = _livroService.ObterPorId(id);

        System.Console.WriteLine($"\nAtualizando '{atual.Titulo}'. Deixe em branco para manter o valor atual.");

        var dados = new Livro
        {
            Titulo = PerguntarComPadrao("Título", atual.Titulo),
            Autor = PerguntarComPadrao("Autor", atual.Autor),
            ISBN = PerguntarComPadrao("ISBN", atual.ISBN),
            Editora = PerguntarComPadrao("Editora", atual.Editora),
            Descricao = PerguntarComPadrao("Descrição", atual.Descricao),
            Genero = atual.Genero,
            AnoDePublicacao = PerguntarInteiroComPadrao("Ano de publicação", atual.AnoDePublicacao),
            QuantidadePaginas = PerguntarInteiroComPadrao("Quantidade de páginas", atual.QuantidadePaginas)
        };

        var atualizado = _livroService.Atualizar(id, dados);
        System.Console.WriteLine($"\nLivro atualizado!\n  {atualizado}");
    }

    private static void RemoverLivro()
    {
        ListarLivros();
        var id = PerguntarInteiro("\nId do livro a remover");
        var livro = _livroService.ObterPorId(id);

        System.Console.Write($"Confirma a remoção de '{livro.Titulo}'? (s/n): ");
        if (!(System.Console.ReadLine() ?? "n").Trim().Equals("s", StringComparison.OrdinalIgnoreCase))
        {
            System.Console.WriteLine("Remoção cancelada.");
            return;
        }

        _livroService.Remover(id);
        System.Console.WriteLine("Livro removido.");
    }

    private static void DefinirCapa()
    {
        ListarLivros();
        var id = PerguntarInteiro("\nId do livro");
        var caminho = Perguntar("Caminho do arquivo de imagem");

        var livro = _livroService.DefinirCapa(id, caminho);
        System.Console.WriteLine($"\nCapa definida: {livro.CapaLivro!.Length} bytes armazenados.");
    }

    private static async Task ConsultarApiExternaAsync()
    {
        System.Console.WriteLine("--- Consulta em API Externa (Google Books) ---");
        System.Console.WriteLine("1 - Buscar por ISBN");
        System.Console.WriteLine("2 - Buscar por termo (título, autor...)");
        var escolha = Perguntar("Opção");

        List<Livro> encontrados;

        if (escolha == "1")
        {
            var isbn = Perguntar("ISBN");
            System.Console.WriteLine("Consultando (requer internet)...");
            var livro = await _consultaExterna.ConsultarPorIsbnAsync(isbn);
            encontrados = livro is null ? new List<Livro>() : new List<Livro> { livro };
        }
        else
        {
            var termo = Perguntar("Termo de busca");
            System.Console.WriteLine("Consultando (requer internet)...");
            encontrados = await _consultaExterna.ConsultarPorTermoAsync(termo);
        }

        if (encontrados.Count == 0)
        {
            System.Console.WriteLine("Nenhum resultado encontrado (ou a API está inacessível).");
            return;
        }

        System.Console.WriteLine($"\n{encontrados.Count} resultado(s):");
        for (var i = 0; i < encontrados.Count; i++)
        {
            var l = encontrados[i];
            System.Console.WriteLine(
                $"  {i + 1} - {l.Titulo} | {l.Autor} | {l.AnoDePublicacao} | " +
                $"ISBN {(string.IsNullOrEmpty(l.ISBN) ? "não informado" : l.ISBN)} | {l.QuantidadePaginas}p");
        }

        System.Console.Write("\nNúmero do resultado para cadastrar (ou ENTER para voltar): ");
        var entrada = System.Console.ReadLine();
        if (string.IsNullOrWhiteSpace(entrada)) return;

        if (!int.TryParse(entrada, out var indice) || indice < 1 || indice > encontrados.Count)
        {
            System.Console.WriteLine("Número inválido.");
            return;
        }

        var cadastrado = _livroService.Cadastrar(encontrados[indice - 1]);
        System.Console.WriteLine($"\nLivro importado e cadastrado!\n  {cadastrado}");
    }

    // ------------------------------ USUÁRIOS ------------------------------

    private static void CadastrarUsuario()
    {
        System.Console.WriteLine("--- Cadastro de Usuário ---");

        var usuario = new Usuario
        {
            Nome = Perguntar("Nome"),
            Email = Perguntar("E-mail")
        };

        var cadastrado = _usuarioService.Cadastrar(usuario);
        System.Console.WriteLine($"\nUsuário cadastrado com sucesso!\n  {cadastrado}");
    }

    private static void ListarUsuarios()
    {
        var usuarios = _usuarioService.ListarTodos();
        if (usuarios.Count == 0)
        {
            System.Console.WriteLine("Nenhum usuário cadastrado.");
            return;
        }

        System.Console.WriteLine("--- Usuários Cadastrados ---");
        foreach (var usuario in usuarios)
            System.Console.WriteLine($"  {usuario}");
    }

    private static void DetalharUsuario()
    {
        ListarUsuarios();
        var id = PerguntarInteiro("\nId do usuário");
        var usuario = _usuarioService.ObterPorId(id);

        System.Console.WriteLine($"\n--- {usuario.Nome} ---");
        System.Console.WriteLine($"  E-mail..........: {usuario.Email}");
        System.Console.WriteLine($"  Cadastrado em...: {usuario.DataCriacao:dd/MM/yyyy}");
        System.Console.WriteLine($"  Total avaliações: {usuario.TotalAvaliacoes}");
        System.Console.WriteLine($"  Nota média dada.: {usuario.NotaMediaAtribuida?.ToString("0.00") ?? "-"}");

        if (usuario.Avaliacoes.Count == 0)
        {
            System.Console.WriteLine("\n  Este usuário ainda não avaliou nenhum livro.");
            return;
        }

        System.Console.WriteLine("\n  Avaliações deste usuário:");
        foreach (var avaliacao in usuario.Avaliacoes.OrderByDescending(a => a.DataFimLeitura))
        {
            System.Console.WriteLine(
                $"    {avaliacao.Estrelas} ({avaliacao.Nota}/5) - {avaliacao.Livro?.Titulo} " +
                $"- terminou em {avaliacao.DataFimLeitura:dd/MM/yyyy}");
            if (!string.IsNullOrWhiteSpace(avaliacao.Descricao))
                System.Console.WriteLine($"      \"{avaliacao.Descricao}\"");
        }
    }

    private static void RemoverUsuario()
    {
        ListarUsuarios();
        var id = PerguntarInteiro("\nId do usuário a remover");
        _usuarioService.Remover(id);
        System.Console.WriteLine("Usuário removido.");
    }

    // ----------------------------- AVALIAÇÕES -----------------------------

    private static void CadastrarAvaliacao()
    {
        System.Console.WriteLine("--- Cadastro de Avaliação ---");

        ListarLivros();
        var idLivro = PerguntarInteiro("\nId do livro");

        ListarUsuarios();
        var idUsuario = PerguntarInteiro("\nId do usuário");

        var nota = PerguntarInteiro("Nota (1 a 5)");
        var descricao = Perguntar("Comentário");
        var inicio = PerguntarData("Data de início da leitura (dd/MM/yyyy)");
        var fim = PerguntarData("Data de fim da leitura (dd/MM/yyyy)");

        var avaliacao = _avaliacaoService.Cadastrar(idLivro, idUsuario, nota, descricao, inicio, fim);
        var livro = _livroService.ObterPorId(idLivro);

        System.Console.WriteLine($"\nAvaliação cadastrada!\n  {avaliacao}");
        System.Console.WriteLine($"  Nova nota média de '{livro.Titulo}': {livro.NotaMedia:0.00}");
    }

    private static void ListarAvaliacoesDoLivro()
    {
        ListarLivros();
        var idLivro = PerguntarInteiro("\nId do livro");
        var livro = _livroService.ObterPorId(idLivro);
        var avaliacoes = _avaliacaoService.ListarPorLivro(idLivro).ToList();

        System.Console.WriteLine($"\n--- Avaliações de '{livro.Titulo}' (média {livro.NotaMedia:0.00}) ---");

        if (avaliacoes.Count == 0)
        {
            System.Console.WriteLine("  Nenhuma avaliação cadastrada.");
            return;
        }

        foreach (var avaliacao in avaliacoes)
        {
            System.Console.WriteLine(
                $"  {avaliacao.Estrelas} ({avaliacao.Nota}/5) por {avaliacao.Usuario?.Nome} " +
                $"em {avaliacao.DataCriacao:dd/MM/yyyy}");
            if (!string.IsNullOrWhiteSpace(avaliacao.Descricao))
                System.Console.WriteLine($"    \"{avaliacao.Descricao}\"");
        }
    }

    // ----------------------------- RELATÓRIOS -----------------------------

    private static void RelatorioAnual()
    {
        var ano = PerguntarInteiroComPadrao("Ano do relatório", DateTime.Today.Year);
        var relatorio = _livroService.GerarRelatorioAnual(ano);

        System.Console.WriteLine($"\n--- Livros Lidos em {relatorio.Ano} ---");
        System.Console.WriteLine($"  Livros distintos lidos: {relatorio.LivrosLidos}");
        System.Console.WriteLine($"  Total de leituras.....: {relatorio.TotalLeituras}");
        System.Console.WriteLine($"  Páginas lidas.........: {relatorio.PaginasLidas}");

        if (relatorio.Itens.Count == 0)
        {
            System.Console.WriteLine("\n  Nenhuma leitura registrada neste ano.");
            return;
        }

        System.Console.WriteLine("\n  Detalhamento:");
        foreach (var item in relatorio.Itens)
        {
            System.Console.WriteLine(
                $"    {item.Titulo} ({item.Autor}) - {item.QuantidadePaginas}p " +
                $"- nota média {item.NotaMedia:0.00} - {item.QuantidadeLeituras} leitura(s)");
        }
    }

    // ------------------------- ENTRADA DE DADOS ---------------------------

    private static string Perguntar(string rotulo)
    {
        System.Console.Write($"{rotulo}: ");
        return System.Console.ReadLine() ?? string.Empty;
    }

    private static string PerguntarComPadrao(string rotulo, string padrao)
    {
        System.Console.Write($"{rotulo} [{padrao}]: ");
        var valor = System.Console.ReadLine();
        return string.IsNullOrWhiteSpace(valor) ? padrao : valor;
    }

    private static int PerguntarInteiro(string rotulo)
    {
        System.Console.Write($"{rotulo}: ");
        var entrada = System.Console.ReadLine();

        if (!int.TryParse(entrada, out var valor))
            throw new RegraNegocioException($"'{entrada}' não é um número inteiro válido.");

        return valor;
    }

    private static int PerguntarInteiroComPadrao(string rotulo, int padrao)
    {
        System.Console.Write($"{rotulo} [{padrao}]: ");
        var entrada = System.Console.ReadLine();

        if (string.IsNullOrWhiteSpace(entrada)) return padrao;

        if (!int.TryParse(entrada, out var valor))
            throw new RegraNegocioException($"'{entrada}' não é um número inteiro válido.");

        return valor;
    }

    private static DateTime PerguntarData(string rotulo)
    {
        System.Console.Write($"{rotulo}: ");
        var entrada = System.Console.ReadLine();

        if (!DateTime.TryParseExact(entrada, "dd/MM/yyyy", _cultura, DateTimeStyles.None, out var data))
            throw new RegraNegocioException($"'{entrada}' não é uma data válida. Use o formato dd/MM/yyyy.");

        return data;
    }

    private static void ExibirGeneros()
    {
        foreach (GeneroLivroEnum genero in Enum.GetValues<GeneroLivroEnum>())
            System.Console.WriteLine($"  {(int)genero,2} - {genero}");
    }

    private static GeneroLivroEnum PerguntarGenero()
    {
        var codigo = PerguntarInteiro("Gênero (código)");

        if (!Enum.IsDefined(typeof(GeneroLivroEnum), codigo))
            throw new RegraNegocioException($"Código de gênero '{codigo}' inválido.");

        return (GeneroLivroEnum)codigo;
    }

    /// <summary>
    /// Carrega dados de exemplo para facilitar os testes manuais.
    /// </summary>
    private static void CarregarDadosDeExemplo()
    {
        var domainDrivenDesign = _livroService.Cadastrar(new Livro
        {
            Titulo = "Domain-Driven Design",
            Autor = "Eric Evans",
            ISBN = "978-0321125217",
            Editora = "Addison-Wesley",
            Descricao = "Tackling complexity in the heart of software.",
            Genero = GeneroLivroEnum.Tecnico,
            AnoDePublicacao = 2003,
            QuantidadePaginas = 560
        });

        var duna = _livroService.Cadastrar(new Livro
        {
            Titulo = "Duna",
            Autor = "Frank Herbert",
            ISBN = "978-8576572008",
            Editora = "Aleph",
            Descricao = "O planeta deserto Arrakis e a especiaria mais valiosa do universo.",
            Genero = GeneroLivroEnum.FiccaoCientifica,
            AnoDePublicacao = 1965,
            QuantidadePaginas = 680
        });

        _livroService.Cadastrar(new Livro
        {
            Titulo = "O Hobbit",
            Autor = "J. R. R. Tolkien",
            ISBN = "978-8595084742",
            Editora = "HarperCollins",
            Descricao = "A jornada de Bilbo Bolseiro até a Montanha Solitária.",
            Genero = GeneroLivroEnum.Fantasia,
            AnoDePublicacao = 1937,
            QuantidadePaginas = 336
        });

        var ana = _usuarioService.Cadastrar(new Usuario
        {
            Nome = "Ana Souza",
            Email = "ana.souza@example.com"
        });

        var bruno = _usuarioService.Cadastrar(new Usuario
        {
            Nome = "Bruno Lima",
            Email = "bruno.lima@example.com"
        });

        var hoje = DateTime.Today;

        _avaliacaoService.Cadastrar(
            domainDrivenDesign.Id, ana.Id, 5,
            "Mudou a forma como penso modelagem de software.",
            hoje.AddDays(-60), hoje.AddDays(-20));

        _avaliacaoService.Cadastrar(
            domainDrivenDesign.Id, bruno.Id, 4,
            "Excelente, mas denso em algumas partes.",
            hoje.AddDays(-45), hoje.AddDays(-10));

        _avaliacaoService.Cadastrar(
            duna.Id, ana.Id, 5,
            "Uma obra-prima da ficção científica.",
            hoje.AddDays(-30), hoje.AddDays(-5));
    }
}
