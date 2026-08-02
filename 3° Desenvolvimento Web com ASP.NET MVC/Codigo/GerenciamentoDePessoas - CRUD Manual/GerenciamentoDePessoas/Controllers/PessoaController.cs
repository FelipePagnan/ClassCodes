using GerenciamentoDePessoas.Models;
using GerenciamentoDePessoas.Services;
using Microsoft.AspNetCore.Mvc;

namespace GerenciamentoDePessoas.Controllers
{
    public class PessoaController : Controller
    {
        private readonly IPessoaService _pessoasService;

        public PessoaController(IPessoaService pessoasService)
        {
            _pessoasService = pessoasService;
        }
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var todosUsuarios = await _pessoasService.BuscarTodos();
            return View(todosUsuarios);
        }
        [HttpGet]
        public async Task<ActionResult> Total()
        {
            var totalPessoas = await _pessoasService.BuscarTotal();
            return Ok(totalPessoas);
        }
        [HttpGet]
        public async Task<JsonResult> BuscarPessoasNome(string termo)
        {
            var resultadoBusca = await _pessoasService.BuscarPessoasNome(termo);

            return Json(resultadoBusca);
        }

        [HttpGet]
        public async Task<ActionResult> Criar()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Criar(Pessoa pessoa)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var usuario = await _pessoasService.Criar(pessoa);
                    TempData["Sucesso"] = $"Pessoa {pessoa.Nome} criada com sucesso!";
                    return RedirectToAction("Index", "Pessoa");
                }
                return View(pessoa);
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
                return View(pessoa);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Editar(int id)
        {
            try
            {
                if (id == 0)
                {
                    throw new Exception("Um Id deve ser informado.");
                }

                var pessoaDb = await _pessoasService.BuscarPorId(id);

                return View(pessoaDb);
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction("Index", "Pessoa");
            }
        }
        [HttpPost]
        public async Task<ActionResult> Editar(Pessoa pessoa)
        {
            try
            {
                if (pessoa.Id == 0)
                {
                    throw new Exception("Um Id deve ser informado.");
                }

                var pessoaDb = await _pessoasService.Editar(pessoa);

                TempData["Sucesso"] = $"Pessoa {pessoa.Nome} foi atualizada com sucesso!";
                return RedirectToAction("Index", "Pessoa");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
                return View(pessoa);
            }

        }

        [HttpPost]
        public async Task<ActionResult> Apagar(int id)
        {
            try
            {
                if (id == 0)
                {
                    throw new Exception("Um Id deve ser informado.");
                }

                await _pessoasService.Apagar(id);

                TempData["Sucesso"] = $"Pessoa deletada!";
                return RedirectToAction("Index", "Pessoa");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction("Index", "Pessoa");
            }

        }
    }
}
