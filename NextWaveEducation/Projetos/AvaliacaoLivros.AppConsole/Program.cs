using AvaliacaoLivros.AppConsole.Models;
using AvaliacaoLivros.AppConsole.Services;

namespace AvaliacaoLivros.AppConsole;

public class Program
{
    private static readonly AvaliacaoLivrosService _servico = new();
    private static readonly LivroExternoService _livroExterno = new();

    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        bool continuar = true;

        while (continuar)
        {
            ExibirMenu();
            string opcao = Console.ReadLine() ?? string.Empty;
            Console.WriteLine();

            switch (opcao.Trim())
            {
                case "1": CadastrarLivro(); break;
                case "2": await CadastrarLivroViaIsbnAsync(); break;
                case "3": AtualizarLivro(); break;
                case "4": RemoverLivro(); break;
                case "5": ConsultarTodosLivros(); break;
                case "6": ConsultarDetalhesLivro(); break;
                case "7": CadastrarCapaLivro(); break;
                case "8": CadastrarUsuario(); break;
                case "9": AtualizarUsuario(); break;
                case "10": RemoverUsuario(); break;
                case "11": ConsultarTodosUsuarios(); break;
                case "12": ConsultarDetalhesUsuario(); break;
                case "13": CadastrarAvaliacao(); break;
                case "14": ListarAvaliacoesPorLivro(); break;
                case "15": RelatorioLivrosLidosNoAno(); break;
                case "0":
                    continuar = false;
                    Console.WriteLine("Encerrando o sistema. Até logo!");
                    break;
                default:
                    Console.WriteLine("Opção inválida. Tente novamente.");
                    break;
            }

            if (continuar)
            {
                Console.WriteLine("\nPressione ENTER para continuar...");
                Console.ReadLine();
            }
        }
    }

    private static void ExibirMenu()
    {
        Console.Clear();
        Console.WriteLine("==================================================");
        Console.WriteLine("   GERENCIADOR DE AVALIAÇÃO DE LIVROS");
        Console.WriteLine("==================================================");
        Console.WriteLine(" LIVROS");
        Console.WriteLine(" 1  - Cadastrar Livro");
        Console.WriteLine(" 2  - Cadastrar Livro via ISBN (API externa)");
        Console.WriteLine(" 3  - Atualizar Livro");
        Console.WriteLine(" 4  - Remover Livro");
        Console.WriteLine(" 5  - Listar Todos os Livros");
        Console.WriteLine(" 6  - Detalhes de Um Livro (com avaliações)");
        Console.WriteLine(" 7  - Cadastrar Capa do Livro");
        Console.WriteLine(" USUÁRIOS");
        Console.WriteLine(" 8  - Cadastrar Usuário");
        Console.WriteLine(" 9  - Atualizar Usuário");
        Console.WriteLine(" 10 - Remover Usuário");
        Console.WriteLine(" 11 - Listar Todos os Usuários");
        Console.WriteLine(" 12 - Detalhes de Um Usuário (com avaliações)");
        Console.WriteLine(" AVALIAÇÕES");
        Console.WriteLine(" 13 - Cadastrar Avaliação");
        Console.WriteLine(" 14 - Listar Avaliações de Um Livro");
        Console.WriteLine(" RELATÓRIOS");
        Console.WriteLine(" 15 - Relatório de Livros Lidos em um Ano");
        Console.WriteLine(" 0  - Sair");
        Console.WriteLine("==================================================");
        Console.Write("Escolha uma opção: ");
    }

    // ===================== LIVROS =====================

    private static void CadastrarLivro()
    {
        Console.WriteLine("--- Cadastro de Livro ---");

        Console.Write("Título: ");
        string titulo = Console.ReadLine() ?? string.Empty;

        Console.Write("Descrição: ");
        string descricao = Console.ReadLine() ?? string.Empty;

        Console.Write("ISBN: ");
        string isbn = Console.ReadLine() ?? string.Empty;

        Console.Write("Autor: ");
        string autor = Console.ReadLine() ?? string.Empty;

        Console.Write("Editora: ");
        string editora = Console.ReadLine() ?? string.Empty;

        var genero = LerGenero();
        if (genero is null) return;

        Console.Write("Ano de publicação: ");
        if (!int.TryParse(Console.ReadLine(), out int ano))
        {
            Console.WriteLine("Ano inválido.");
            return;
        }

        Console.Write("Quantidade de páginas: ");
        if (!int.TryParse(Console.ReadLine(), out int paginas))
        {
            Console.WriteLine("Quantidade de páginas inválida.");
            return;
        }

        try
        {
            var livro = _servico.CadastrarLivro(titulo, descricao, isbn, autor, editora, genero.Value, ano, paginas);
            Console.WriteLine($"\nLivro cadastrado com sucesso! {livro}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"\nErro de validação: {ex.Message}");
        }
    }

    private static async Task CadastrarLivroViaIsbnAsync()
    {
        Console.WriteLine("--- Cadastro de Livro via ISBN (API externa - Open Library) ---");

        Console.Write("ISBN: ");
        string isbn = Console.ReadLine() ?? string.Empty;

        Console.WriteLine("Consultando API externa...");
        var resultado = await _livroExterno.ConsultarPorIsbnAsync(isbn);

        if (resultado is null)
        {
            Console.WriteLine("Não foi possível encontrar esse livro na API externa (ISBN inválido ou sem conexão).");
            Console.WriteLine("Cadastre manualmente pela opção 1.");
            return;
        }

        Console.WriteLine($"\nLivro encontrado: {resultado.Titulo} - {resultado.Autor} ({resultado.AnoDePublicacao})");

        var genero = LerGenero();
        if (genero is null) return;

        Console.Write("Descrição (opcional): ");
        string descricao = Console.ReadLine() ?? string.Empty;

        try
        {
            var livro = _servico.CadastrarLivro(
                resultado.Titulo, descricao, isbn, resultado.Autor, resultado.Editora,
                genero.Value, resultado.AnoDePublicacao, resultado.QuantidadePaginas);

            Console.WriteLine($"\nLivro cadastrado com sucesso a partir da API externa! {livro}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"\nErro de validação: {ex.Message}");
        }
    }

    private static void AtualizarLivro()
    {
        Console.WriteLine("--- Atualizar Livro ---");
        int id = LerId("Informe o Id do livro: ");

        var existente = _servico.ConsultarLivroPorId(id);
        if (existente is null)
        {
            Console.WriteLine("Livro não encontrado.");
            return;
        }

        Console.WriteLine($"Dados atuais: {existente}");

        Console.Write($"Novo título [{existente.Titulo}]: ");
        string titulo = LerOuManter(existente.Titulo);

        Console.Write($"Nova descrição [{existente.Descricao}]: ");
        string descricao = LerOuManter(existente.Descricao);

        Console.Write($"Novo autor [{existente.Autor}]: ");
        string autor = LerOuManter(existente.Autor);

        Console.Write($"Nova editora [{existente.Editora}]: ");
        string editora = LerOuManter(existente.Editora);

        Console.Write($"Novo ano de publicação [{existente.AnoDePublicacao}]: ");
        string anoTexto = Console.ReadLine() ?? string.Empty;
        int ano = string.IsNullOrWhiteSpace(anoTexto) ? existente.AnoDePublicacao : int.Parse(anoTexto);

        Console.Write($"Nova quantidade de páginas [{existente.QuantidadePaginas}]: ");
        string paginasTexto = Console.ReadLine() ?? string.Empty;
        int paginas = string.IsNullOrWhiteSpace(paginasTexto) ? existente.QuantidadePaginas : int.Parse(paginasTexto);

        try
        {
            var livro = _servico.AtualizarLivro(id, titulo, descricao, autor, editora, existente.Genero, ano, paginas);
            Console.WriteLine($"\nLivro atualizado com sucesso! {livro}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"\nErro de validação: {ex.Message}");
        }
    }

    private static void RemoverLivro()
    {
        Console.WriteLine("--- Remover Livro ---");
        int id = LerId("Informe o Id do livro: ");

        bool removido = _servico.RemoverLivro(id);
        Console.WriteLine(removido ? "Livro removido com sucesso." : "Livro não encontrado.");
    }

    private static void ConsultarTodosLivros()
    {
        Console.WriteLine("--- Todos os Livros ---");
        var livros = _servico.ConsultarTodosLivros();

        if (livros.Count == 0)
        {
            Console.WriteLine("Nenhum livro cadastrado.");
            return;
        }

        foreach (var livro in livros)
            Console.WriteLine(livro);
    }

    private static void ConsultarDetalhesLivro()
    {
        Console.WriteLine("--- Detalhes do Livro ---");
        int id = LerId("Informe o Id do livro: ");

        var livro = _servico.ConsultarLivroPorId(id);
        if (livro is null)
        {
            Console.WriteLine("Livro não encontrado.");
            return;
        }

        Console.WriteLine(livro);
        Console.WriteLine($"Descrição: {livro.Descricao}");
        Console.WriteLine($"\nAvaliações ({livro.Avaliacoes.Count}):");

        if (livro.Avaliacoes.Count == 0)
        {
            Console.WriteLine("Nenhuma avaliação registrada ainda.");
            return;
        }

        foreach (var avaliacao in livro.Avaliacoes)
            Console.WriteLine(avaliacao);
    }

    private static void CadastrarCapaLivro()
    {
        Console.WriteLine("--- Cadastrar Capa do Livro ---");
        int id = LerId("Informe o Id do livro: ");

        Console.Write("Informe o caminho do arquivo de imagem (ex.: C:\\capas\\livro.jpg): ");
        string caminho = Console.ReadLine() ?? string.Empty;

        if (!File.Exists(caminho))
        {
            Console.WriteLine("Arquivo não encontrado.");
            return;
        }

        try
        {
            byte[] conteudo = File.ReadAllBytes(caminho);
            _servico.CadastrarCapaLivro(id, conteudo);
            Console.WriteLine($"\nCapa cadastrada com sucesso! ({conteudo.Length} bytes)");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"\nErro: {ex.Message}");
        }
    }

    // ===================== USUÁRIOS =====================

    private static void CadastrarUsuario()
    {
        Console.WriteLine("--- Cadastro de Usuário ---");

        Console.Write("Nome: ");
        string nome = Console.ReadLine() ?? string.Empty;

        Console.Write("Email: ");
        string email = Console.ReadLine() ?? string.Empty;

        try
        {
            var usuario = _servico.CadastrarUsuario(nome, email);
            Console.WriteLine($"\nUsuário cadastrado com sucesso! {usuario}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"\nErro de validação: {ex.Message}");
        }
    }

    private static void AtualizarUsuario()
    {
        Console.WriteLine("--- Atualizar Usuário ---");
        int id = LerId("Informe o Id do usuário: ");

        var existente = _servico.ConsultarUsuarioPorId(id);
        if (existente is null)
        {
            Console.WriteLine("Usuário não encontrado.");
            return;
        }

        Console.Write($"Novo nome [{existente.Nome}]: ");
        string nome = LerOuManter(existente.Nome);

        Console.Write($"Novo email [{existente.Email}]: ");
        string email = LerOuManter(existente.Email);

        try
        {
            var usuario = _servico.AtualizarUsuario(id, nome, email);
            Console.WriteLine($"\nUsuário atualizado com sucesso! {usuario}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"\nErro de validação: {ex.Message}");
        }
    }

    private static void RemoverUsuario()
    {
        Console.WriteLine("--- Remover Usuário ---");
        int id = LerId("Informe o Id do usuário: ");

        bool removido = _servico.RemoverUsuario(id);
        Console.WriteLine(removido ? "Usuário removido com sucesso." : "Usuário não encontrado.");
    }

    private static void ConsultarTodosUsuarios()
    {
        Console.WriteLine("--- Todos os Usuários ---");
        var usuarios = _servico.ConsultarTodosUsuarios();

        if (usuarios.Count == 0)
        {
            Console.WriteLine("Nenhum usuário cadastrado.");
            return;
        }

        foreach (var usuario in usuarios)
            Console.WriteLine(usuario);
    }

    private static void ConsultarDetalhesUsuario()
    {
        Console.WriteLine("--- Detalhes do Usuário ---");
        int id = LerId("Informe o Id do usuário: ");

        var usuario = _servico.ConsultarUsuarioPorId(id);
        if (usuario is null)
        {
            Console.WriteLine("Usuário não encontrado.");
            return;
        }

        Console.WriteLine(usuario);
        Console.WriteLine($"\nAvaliações feitas por este usuário:");

        if (usuario.Avaliacoes.Count == 0)
        {
            Console.WriteLine("Nenhuma avaliação registrada ainda.");
            return;
        }

        foreach (var avaliacao in usuario.Avaliacoes)
            Console.WriteLine(avaliacao);
    }

    // ===================== AVALIAÇÕES =====================

    private static void CadastrarAvaliacao()
    {
        Console.WriteLine("--- Cadastro de Avaliação ---");

        int idUsuario = LerId("Informe o Id do usuário: ");
        int idLivro = LerId("Informe o Id do livro: ");

        Console.Write("Nota (1 a 5): ");
        if (!int.TryParse(Console.ReadLine(), out int nota))
        {
            Console.WriteLine("Nota inválida.");
            return;
        }

        Console.Write("Comentário: ");
        string descricao = Console.ReadLine() ?? string.Empty;

        Console.Write("Data de início da leitura (dd/MM/yyyy): ");
        if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime dataInicio))
        {
            Console.WriteLine("Data de início inválida.");
            return;
        }

        Console.Write("Data de fim da leitura (dd/MM/yyyy): ");
        if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime dataFim))
        {
            Console.WriteLine("Data de fim inválida.");
            return;
        }

        try
        {
            var avaliacao = _servico.CadastrarAvaliacao(idUsuario, idLivro, nota, descricao, dataInicio, dataFim);
            Console.WriteLine($"\nAvaliação cadastrada com sucesso! {avaliacao}");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"\nErro de validação: {ex.Message}");
        }
    }

    private static void ListarAvaliacoesPorLivro()
    {
        Console.WriteLine("--- Avaliações de Um Livro ---");
        int idLivro = LerId("Informe o Id do livro: ");

        try
        {
            var avaliacoes = _servico.ConsultarAvaliacoesPorLivro(idLivro);

            if (avaliacoes.Count == 0)
            {
                Console.WriteLine("Nenhuma avaliação registrada para este livro.");
                return;
            }

            foreach (var avaliacao in avaliacoes)
                Console.WriteLine(avaliacao);
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    }

    // ===================== RELATÓRIOS =====================

    private static void RelatorioLivrosLidosNoAno()
    {
        Console.WriteLine("--- Relatório: Livros Lidos em um Ano ---");
        Console.Write("Informe o ano: ");

        if (!int.TryParse(Console.ReadLine(), out int ano))
        {
            Console.WriteLine("Ano inválido.");
            return;
        }

        var relatorio = _servico.RelatorioLivrosLidosNoAno(ano);

        if (relatorio.Count == 0)
        {
            Console.WriteLine($"Nenhum livro lido em {ano}.");
            return;
        }

        foreach (var (livro, usuario, dataFim) in relatorio)
        {
            Console.WriteLine($"{dataFim:dd/MM/yyyy} | \"{livro.Titulo}\" | Leitor: {usuario.Nome}");
        }

        Console.WriteLine($"\nTotal de leituras concluídas em {ano}: {relatorio.Count}");
    }

    // ===================== AUXILIARES =====================

    private static int LerId(string mensagem)
    {
        Console.Write(mensagem);
        string texto = Console.ReadLine() ?? string.Empty;
        return int.TryParse(texto, out int id) ? id : -1;
    }

    private static string LerOuManter(string valorAtual)
    {
        string texto = Console.ReadLine() ?? string.Empty;
        return string.IsNullOrWhiteSpace(texto) ? valorAtual : texto;
    }

    private static GeneroLivroEnum? LerGenero()
    {
        var generos = Enum.GetValues<GeneroLivroEnum>();

        Console.WriteLine("Gêneros disponíveis:");
        for (int i = 0; i < generos.Length; i++)
            Console.WriteLine($"  {i + 1} - {generos[i]}");

        Console.Write("Escolha o número do gênero: ");

        if (!int.TryParse(Console.ReadLine(), out int escolha) || escolha < 1 || escolha > generos.Length)
        {
            Console.WriteLine("Gênero inválido.");
            return null;
        }

        return generos[escolha - 1];
    }
}
