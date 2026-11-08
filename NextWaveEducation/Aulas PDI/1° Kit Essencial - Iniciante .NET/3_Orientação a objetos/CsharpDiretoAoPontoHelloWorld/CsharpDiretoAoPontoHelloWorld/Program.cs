class Program
{
    static void Main(string[] args)
    {
        #region Tipos de Dados
        int mumeroInt = 10;

        int maiorNumeroInt = int.MaxValue;
        int menosNumeroInt = int.MinValue;

        long numeroLong = 12312321321321;

        long maiorNumeroLong = long.MaxValue;
        long menorNumeroLong = long.MinValue;

        decimal numeroDecimal = 10.52m;

        double numeroDouble = 12.3;
        double menorNumeroDouble = double.MinValue;

        bool verdadeiro = true;
        bool falso = false;

        var numero = 10;

        string nome = "Pagnan";
        char letra = 'A';

        DateTime entredaEmpresa = new DateTime(2021, 1, 1);
        TimeSpan quantoTempoDeEmpresa = DateTime.Now - entredaEmpresa;
        #endregion

        #region Conversoes

        int notaAluno = 10;

        //Conversao implicita
        double notaAlunoDouble = notaAluno;

        //Conversao explicita
        int numeroDoubleComoInt = (int)notaAlunoDouble;

        //conversao utilizando convert

        string notaString = "10";

        int notaConvert = Convert.ToInt32(notaString);

        // conversao parse

        int notaParse = int.Parse(notaString);

        if (int.TryParse(notaString, out int notaTryParse))
        {

        }
        else
        {
            Console.WriteLine("Numero em formato invalido");
        }
        #endregion

        #region Operadores
        // Unitario ++, --, + e -

        int numeroOperador = 4;

        Console.WriteLine(numeroOperador++); // 4
        Console.WriteLine(numeroOperador--); // 5


        Console.WriteLine(++numeroOperador); // 5
        Console.WriteLine(--numeroOperador); // 4
        // Binario * / + -

        #endregion
    }
}
