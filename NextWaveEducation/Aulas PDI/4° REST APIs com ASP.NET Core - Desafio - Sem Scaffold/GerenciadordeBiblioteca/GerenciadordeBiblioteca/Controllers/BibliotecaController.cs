using GerenciadordeBiblioteca.Models;
using GerenciadordeBiblioteca.Services;
using Microsoft.AspNetCore.Mvc;

namespace GerenciadordeBiblioteca.Controllers
{
    public class BibliotecaController : Controller
    {

        private readonly IBibliotecaService _bibliotecaService;

        public BibliotecaController(IBibliotecaService bibliotecaService)
        {
            _bibliotecaService = bibliotecaService;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var todosLivros = await _bibliotecaService.ListarTodos();
            return View(todosLivros);
        }

        [HttpGet]
        public async Task<ActionResult> Total()
        {
            var totalLivros = await _bibliotecaService.ContarTotal();
            return View(totalLivros);
        }


        [HttpGet]
        public async Task<ActionResult> Registrar()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Registrar(Livro livro)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var livroARegistrar = await _bibliotecaService.Registrar(livro);
                    TempData["Sucesso"] = $"Livro '{livroARegistrar.Titulo}' registrado com sucesso!";
                    return RedirectToAction("Index", "Biblioteca");
                }
                return View(livro);
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
                return View(livro);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Editar(int id)
        {
            try
            {
                if (id == 0)
                {
                    throw new Exception("Id do livro é obrigatório para edição.");
                }

                var livroDb = await _bibliotecaService.BuscarPorId(id);

                return View(livroDb);
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
                return View("Index", "Biblioteca");
            }
        }

        [HttpPost]
        public async Task<ActionResult> Editar(Livro livro)
        {
            try
            {
                if (livro.Id == 0)
                {
                    throw new Exception("Id do livro é obrigatório para edição.");
                }

                var livroDb = await _bibliotecaService.Editar(livro);

                TempData["Sucesso"] = $"Livro {livroDb.Titulo} foi atualizado com sucesso.";
                return RedirectToAction("Index", "Biblioteca");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
                return View(livro);
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

                await _bibliotecaService.Apagar(id);

                TempData["Sucesso"] = $"Livro deletado!";
                return RedirectToAction("Index", "Biblioteca");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
                return RedirectToAction("Index", "Biblioteca");
            }
        }
    }
}
