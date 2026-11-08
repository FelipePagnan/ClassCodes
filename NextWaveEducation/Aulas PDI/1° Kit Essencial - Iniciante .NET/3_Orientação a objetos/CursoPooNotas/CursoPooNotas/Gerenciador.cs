using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CursoPooNotas
{
    public class Gerenciador
    {
        private List<Nota> notas = new List<Nota>();

        public void Adicionar(String descricao)
        {
            var nota = new Nota { Descricao = descricao };
            //var nota = new Nota();
            //nota.Descricao = descricao;

            notas.Add(nota);
        }

        public void ExibirTodas()
        {
            foreach (var nota in notas)
            {
                nota.Exibir();
            }
        }
    }
}
