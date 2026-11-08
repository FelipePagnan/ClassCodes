using Clinica.Console.Entidades;
using Clinica.Console.Excecoes;
using Clinica.Console.Util;

namespace Clinica.Console.Servicos;

/// <summary>
/// CRUD de pacientes, com busca por CPF e por celular.
/// </summary>
public class PacienteService
{
    private readonly List<Paciente> _pacientes = new();
    private int _proximoId = 1;

    public IReadOnlyList<Paciente> ListarTodos()
        => _pacientes.Where(p => !p.EstaDeletado).ToList().AsReadOnly();

    public Paciente? BuscarPorId(int id)
        => _pacientes.FirstOrDefault(p => p.Id == id && !p.EstaDeletado);

    public Paciente ObterPorId(int id)
        => BuscarPorId(id) ?? throw new RegraNegocioException($"Paciente {id} não encontrado.");

    /// <summary>
    /// Busca por CPF. Aceita o CPF formatado ou só os dígitos, porque a
    /// comparação é feita sobre os dígitos.
    /// </summary>
    public Paciente? BuscarPorCpf(string cpf)
    {
        var digitos = Documentos.SomenteDigitos(cpf);
        if (digitos.Length == 0) return null;

        return _pacientes.FirstOrDefault(p => p.Cpf == digitos && !p.EstaDeletado);
    }

    /// <summary>
    /// Busca por celular. Devolve uma lista porque o mesmo número pode ser
    /// usado por mais de um paciente (mãe e filho, por exemplo).
    /// </summary>
    public IEnumerable<Paciente> BuscarPorCelular(string celular)
    {
        var digitos = Documentos.SomenteDigitos(celular);
        if (digitos.Length == 0) return Enumerable.Empty<Paciente>();

        return _pacientes.Where(p => p.Celular == digitos && !p.EstaDeletado);
    }

    /// <summary>Busca livre por nome, CPF ou celular de uma vez.</summary>
    public IEnumerable<Paciente> Pesquisar(string termo)
    {
        if (string.IsNullOrWhiteSpace(termo)) return ListarTodos();

        var digitos = Documentos.SomenteDigitos(termo);
        var texto = termo.Trim();

        return _pacientes.Where(p => !p.EstaDeletado && (
            p.NomeCompleto.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
            (digitos.Length >= 3 && p.Cpf.Contains(digitos)) ||
            (digitos.Length >= 3 && p.Celular.Contains(digitos))));
    }

    public Paciente Cadastrar(Paciente paciente)
    {
        ValidarDados(paciente);

        var cpf = Documentos.SomenteDigitos(paciente.Cpf);

        // Regra de negócio: o CPF identifica o paciente, então não repete.
        if (BuscarPorCpf(cpf) is not null)
            throw new RegraNegocioException(
                $"Já existe um paciente cadastrado com o CPF {Documentos.FormatarCpf(cpf)}.");

        paciente.Id = _proximoId++;
        paciente.Cpf = cpf;
        paciente.Celular = Documentos.SomenteDigitos(paciente.Celular);
        paciente.NomeCompleto = paciente.NomeCompleto.Trim();
        paciente.Email = string.IsNullOrWhiteSpace(paciente.Email)
            ? null
            : paciente.Email.Trim().ToLowerInvariant();
        paciente.DataCriacao = DateTime.Now;
        paciente.EstaDeletado = false;

        _pacientes.Add(paciente);
        return paciente;
    }

    public Paciente Atualizar(int id, Paciente dados)
    {
        var paciente = ObterPorId(id);
        ValidarDados(dados);

        var cpf = Documentos.SomenteDigitos(dados.Cpf);

        var existente = BuscarPorCpf(cpf);
        if (existente is not null && existente.Id != id)
            throw new RegraNegocioException(
                $"Já existe outro paciente com o CPF {Documentos.FormatarCpf(cpf)}.");

        paciente.NomeCompleto = dados.NomeCompleto.Trim();
        paciente.Cpf = cpf;
        paciente.Celular = Documentos.SomenteDigitos(dados.Celular);
        paciente.Email = string.IsNullOrWhiteSpace(dados.Email)
            ? null
            : dados.Email.Trim().ToLowerInvariant();
        paciente.DataNascimento = dados.DataNascimento.Date;
        paciente.Sexo = dados.Sexo;
        paciente.Observacoes = dados.Observacoes;

        return paciente;
    }

    /// <summary>Remoção lógica: o histórico de atendimentos é preservado.</summary>
    public void Remover(int id)
    {
        var paciente = ObterPorId(id);

        var abertos = paciente.AtendimentosAtivos.Count(a => a.EstaAberto);
        if (abertos > 0)
            throw new RegraNegocioException(
                $"'{paciente.NomeCompleto}' tem {abertos} atendimento(s) em aberto. " +
                "Cancele-os antes de remover o paciente.");

        paciente.EstaDeletado = true;
    }

    private static void ValidarDados(Paciente paciente)
    {
        if (string.IsNullOrWhiteSpace(paciente.NomeCompleto))
            throw new RegraNegocioException("O nome completo é obrigatório.");

        if (paciente.NomeCompleto.Trim().Length < 3)
            throw new RegraNegocioException("O nome completo deve ter ao menos 3 caracteres.");

        if (paciente.NomeCompleto.Trim().Length > 200)
            throw new RegraNegocioException("O nome completo deve ter no máximo 200 caracteres.");

        // Regra de negócio: o CPF precisa ter dígitos verificadores válidos.
        if (!Documentos.CpfValido(paciente.Cpf))
            throw new RegraNegocioException(
                $"O CPF '{paciente.Cpf}' é inválido (confira os dígitos verificadores).");

        if (!Documentos.CelularValido(paciente.Celular))
            throw new RegraNegocioException(
                $"O celular '{paciente.Celular}' é inválido. " +
                "Informe DDD + 9 dígitos, ex: (11) 98765-4321.");

        if (!string.IsNullOrWhiteSpace(paciente.Email) && !paciente.Email.Contains('@'))
            throw new RegraNegocioException($"O e-mail '{paciente.Email}' é inválido.");

        if (paciente.DataNascimento == default)
            throw new RegraNegocioException("A data de nascimento é obrigatória.");

        if (paciente.DataNascimento.Date > DateTime.Today)
            throw new RegraNegocioException("A data de nascimento não pode estar no futuro.");

        if (paciente.DataNascimento.Year < 1900)
            throw new RegraNegocioException("A data de nascimento informada é inválida.");
    }
}
