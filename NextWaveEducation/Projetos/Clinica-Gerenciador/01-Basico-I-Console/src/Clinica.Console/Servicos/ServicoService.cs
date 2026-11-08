using Clinica.Console.Entidades;
using Clinica.Console.Excecoes;

namespace Clinica.Console.Servicos;

/// <summary>CRUD dos serviços oferecidos pela clínica.</summary>
public class ServicoService
{
    private readonly List<Servico> _servicos = new();
    private int _proximoId = 1;

    public IReadOnlyList<Servico> ListarTodos()
        => _servicos.Where(s => !s.EstaDeletado).ToList().AsReadOnly();

    public Servico? BuscarPorId(int id)
        => _servicos.FirstOrDefault(s => s.Id == id && !s.EstaDeletado);

    public Servico ObterPorId(int id)
        => BuscarPorId(id) ?? throw new RegraNegocioException($"Serviço {id} não encontrado.");

    public Servico? BuscarPorNome(string nome)
        => _servicos.FirstOrDefault(s =>
            s.Nome.Equals(nome.Trim(), StringComparison.OrdinalIgnoreCase) && !s.EstaDeletado);

    public Servico Cadastrar(Servico servico)
    {
        ValidarDados(servico);

        if (BuscarPorNome(servico.Nome) is not null)
            throw new RegraNegocioException($"Já existe um serviço chamado '{servico.Nome.Trim()}'.");

        servico.Id = _proximoId++;
        servico.Nome = servico.Nome.Trim();
        servico.Descricao = string.IsNullOrWhiteSpace(servico.Descricao)
            ? null
            : servico.Descricao.Trim();
        servico.DataCriacao = DateTime.Now;
        servico.EstaDeletado = false;

        _servicos.Add(servico);
        return servico;
    }

    public Servico Atualizar(int id, Servico dados)
    {
        var servico = ObterPorId(id);
        ValidarDados(dados);

        var existente = BuscarPorNome(dados.Nome);
        if (existente is not null && existente.Id != id)
            throw new RegraNegocioException($"Já existe outro serviço chamado '{dados.Nome.Trim()}'.");

        servico.Nome = dados.Nome.Trim();
        servico.Descricao = string.IsNullOrWhiteSpace(dados.Descricao) ? null : dados.Descricao.Trim();
        servico.Valor = dados.Valor;
        servico.DuracaoMinutos = dados.DuracaoMinutos;

        return servico;
    }

    public void Remover(int id)
    {
        var servico = ObterPorId(id);

        var abertos = servico.Atendimentos.Count(a => !a.EstaDeletado && a.EstaAberto);
        if (abertos > 0)
            throw new RegraNegocioException(
                $"O serviço '{servico.Nome}' tem {abertos} atendimento(s) em aberto.");

        servico.EstaDeletado = true;
    }

    private static void ValidarDados(Servico servico)
    {
        if (string.IsNullOrWhiteSpace(servico.Nome))
            throw new RegraNegocioException("O nome do serviço é obrigatório.");

        if (servico.Nome.Trim().Length < 3)
            throw new RegraNegocioException("O nome do serviço deve ter ao menos 3 caracteres.");

        if (servico.Nome.Trim().Length > 150)
            throw new RegraNegocioException("O nome do serviço deve ter no máximo 150 caracteres.");

        if (servico.Valor < 0)
            throw new RegraNegocioException("O valor do serviço não pode ser negativo.");

        if (decimal.Round(servico.Valor, 2) != servico.Valor)
            throw new RegraNegocioException("O valor deve ter no máximo 2 casas decimais.");

        if (servico.DuracaoMinutos <= 0)
            throw new RegraNegocioException("A duração deve ser maior que zero.");

        if (servico.DuracaoMinutos > 8 * 60)
            throw new RegraNegocioException("A duração não pode passar de 8 horas (480 minutos).");
    }
}
