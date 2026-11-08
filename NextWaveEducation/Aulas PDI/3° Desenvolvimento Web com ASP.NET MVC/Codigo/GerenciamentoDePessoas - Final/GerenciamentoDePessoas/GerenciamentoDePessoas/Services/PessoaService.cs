using GerenciamentoDePessoas.Models;
using GerenciamentoDePessoas.Repository;
using Microsoft.EntityFrameworkCore;

namespace GerenciamentoDePessoas.Services
{
    public class PessoaService : IPessoaService
    {
        private readonly IPessoaRepository _pessoasRepository;

        public PessoaService(IPessoaRepository pessoasRepository)
        {
            _pessoasRepository = pessoasRepository;
        }

        public async Task Apagar(int id)
        {
            var pessoaDb = await _pessoasRepository.BuscarPorId(id);

            await _pessoasRepository.Apagar(pessoaDb);
        }

        public async Task<List<string>> BuscarPessoasNome(string termo)
        {
            var resultadoBusca = await _pessoasRepository.BuscarPessoasNome(termo);

            var nomesCompletos = new List<string>();

            foreach (var item in resultadoBusca)
            {
                nomesCompletos.Add($"{item.Nome} {item.Sobrenome}");
            }
            return nomesCompletos;
        }

        public async Task<Pessoa> BuscarPorId(int id)
        {
            return await _pessoasRepository.BuscarPorId(id);
        }

        public async Task <List<Pessoa>> BuscarTodos()
        {
            var usuariosBanco = await _pessoasRepository.BuscarTodos();
            return usuariosBanco;
        }

        public async Task<int> BuscarTotal()
        {
            return await _pessoasRepository.BuscarTotal();
        }

        public async Task<Pessoa> Criar(Pessoa pessoa)
        {
            var usuarioExiste = await _pessoasRepository.VerificarSePessoaExiste(pessoa.CPF);
            if (usuarioExiste) 
            {
                throw new Exception("Pessoa ja está cadastrado no sistema.");
            }
            var usuarioCriado = await _pessoasRepository.Criar(pessoa);
            return usuarioCriado;
        }

        public async Task<Pessoa> Editar(Pessoa pessoa)
        {
            await _pessoasRepository.BuscarPorId(pessoa.Id);

            return await _pessoasRepository.Editar(pessoa);
        }
    }
}
