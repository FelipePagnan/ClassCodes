using Caixinhas.Intermediario.Dominio.Enums;

namespace Caixinhas.Intermediario.Aplicacao.InputModels;

/// <summary>Modelo de entrada para cadastro de caixinha.</summary>
public class CriarObjetivoInputModel
{
    public string Titulo { get; set; } = string.Empty;
    public decimal QuantidadeAlvo { get; set; }
    public DateTime? Prazo { get; set; }
    public decimal? QuantidadeIdealAporteMensal { get; set; }
}

/// <summary>Modelo de entrada para atualização de caixinha.</summary>
public class AtualizarObjetivoInputModel
{
    public string Titulo { get; set; } = string.Empty;
    public decimal QuantidadeAlvo { get; set; }
    public DateTime? Prazo { get; set; }
    public decimal? QuantidadeIdealAporteMensal { get; set; }
}

/// <summary>Modelo de entrada para cadastro de transação.</summary>
public class CriarTransacaoInputModel
{
    public decimal Quantidade { get; set; }
    public TipoTransacao Tipo { get; set; }

    /// <summary>Data da movimentação. Quando omitida, usa hoje.</summary>
    public DateTime? DataTransacao { get; set; }
}

/// <summary>Parâmetros da simulação de evolução da caixinha.</summary>
public class SimularEvolucaoInputModel
{
    public decimal AporteMensal { get; set; }
    public int QuantidadeMeses { get; set; } = 12;

    /// <summary>Taxa de juros mensal em percentual (ex: 0.80 para 0,80% ao mês).</summary>
    public decimal TaxaJurosMensalPercentual { get; set; }
}
