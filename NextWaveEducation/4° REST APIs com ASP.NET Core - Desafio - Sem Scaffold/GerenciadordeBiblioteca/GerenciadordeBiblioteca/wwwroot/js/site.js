// Inicializa DataTable para a tabela de livros
$(document).ready(function () {
    $('.tabela-livros').DataTable({
        language: {
            url: '//cdn.datatables.net/plug-ins/2.2.2/i18n/pt-BR.json',
        }
    });
});

// Exemplo de busca total de livros (ajuste conforme implementação do endpoint)
$(document).ready(function () {
    $('#buscarTotalLivros').click(function () {
        $('#resultado').text('Buscando...');

        $.ajax({
            method: "GET",
            url: "/Biblioteca/Total", // Ajuste o endpoint conforme sua controller
            dataType: "text",
            success: function (data) {
                $('#resultado').text(`Total de livros: ${data}`);
            },
            error: function (xhr, status, error) {
                console.error(`Erro: ${status} - ${error}`);
                $('#resultado').text('Erro ao buscar o total de livros.');
            }
        });
    });
});

// Exemplo de busca por título de livro (ajuste conforme implementação do endpoint)
$('#botaoBuscaLivro').click(function () {
    var termo = $('#termoBuscaLivro').val();
    if (!termo) {
        alert('Informe um termo para busca.');
        return;
    }

    $.ajax({
        url: '/Biblioteca/BuscarLivrosTitulo', // Ajuste o endpoint conforme sua controller
        type: 'GET',
        data: { termo: termo },
        success: function (data) {
            $('#resultadoLivro').empty();
            if (data.length === 0) {
                $('#resultadoLivro').append('<li class="list-group-item">Nenhum livro encontrado.</li>');
            } else {
                data.forEach(function(livro){
                    $('#resultadoLivro').append('<li class="list-group-item">' + livro + '</li>');
                });
            }
        },
        error: function () {
            alert('Ocorreu um erro ao realizar a busca.');
        }
    });
});
