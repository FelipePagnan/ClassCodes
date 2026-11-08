using BancoSangue.Intermediario.Aplicacao.InputModels;
using BancoSangue.Intermediario.Aplicacao.ViewModels;
using BancoSangue.Intermediario.Dominio.Entidades;
using BancoSangue.Intermediario.Dominio.Excecoes;
using BancoSangue.Intermediario.Dominio.Interfaces;

namespace BancoSangue.Intermediario.Aplicacao.Servicos;

public class DoadorAppService : IDoadorAppService
{
    private readonly IDoadorRepository _doadorRepository;
    private readonly IDoacaoRepository _doacaoRepository;
    private readonly ICepService _cepService;

    public DoadorAppService(
        IDoadorRepository doadorRepository,
        IDoacaoRepository doacaoRepository,
        ICepService cepService)
    {
        _doadorRepository = doadorRepository;
        _doacaoRepository = doacaoRepository;
        _cepService = cepService;
    }

    public async Task<List<DoadorViewModel>> ListarAsync()
    {
        var doadores = await _doadorRepository.ObterTodosAsync();
        return doadores.Select(DoadorViewModel.De).ToList();
    }

    public async Task<DoadorDetalheViewModel> ObterPorIdAsync(int id)
    {
        var doador = await _doadorRepository.ObterPorIdComDoacoesAsync(id)
            ?? throw new NaoEncontradoException($"Doador com id {id} não encontrado.");

        return DoadorDetalheViewModel.DeDetalhe(doador);
    }

    public async Task<List<DoacaoViewModel>> ObterHistoricoDoacoesAsync(int doadorId)
    {
        var doador = await _doadorRepository.ObterPorIdAsync(doadorId)
            ?? throw new NaoEncontradoException($"Doador com id {doadorId} não encontrado.");

        var doacoes = await _doacaoRepository.ObterPorDoadorAsync(doador.Id);
        return doacoes.Select(DoacaoViewModel.De).ToList();
    }

    public async Task<DoadorViewModel> CadastrarAsync(CriarDoadorInputModel input)
    {
        // Regra de negócio: não permitir cadastrar um doador com o mesmo e-mail.
        if (await _doadorRepository.ExisteEmailAsync(input.Email))
            throw new RegraNegocioException($"Já existe um doador cadastrado com o e-mail '{input.Email}'.");

        var doador = new Doador(
            input.NomeCompleto.Trim(),
            input.Email.Trim().ToLowerInvariant(),
            input.DataNascimento,
            input.Genero,
            input.Peso,
            input.TipoSanguineo,
            input.FatorRh);

        // Integração com API externa de CEP (PLUS).
        if (!string.IsNullOrWhiteSpace(input.Cep))
        {
            var endereco = await _cepService.ConsultarAsync(input.Cep)
                ?? throw new RegraNegocioException($"Não foi possível localizar o endereço para o CEP '{input.Cep}'.");

            if (!string.IsNullOrWhiteSpace(input.Complemento))
            {
                endereco = new Endereco(
                    $"{endereco.Logradouro}, {input.Complemento}".Trim(),
                    endereco.Cidade,
                    endereco.Estado,
                    endereco.CEP);
            }

            doador.DefinirEndereco(endereco);
        }

        await _doadorRepository.AdicionarAsync(doador);

        return DoadorViewModel.De(doador);
    }
}
