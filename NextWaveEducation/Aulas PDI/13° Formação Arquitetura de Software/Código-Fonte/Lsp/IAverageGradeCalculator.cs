using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolidPrinciples.Lsp
{
    public interface IAverageGradeCalculator
    {
        string Subject { get; }
        int Year { get; }
        int Semester { get; }
        double Calculate(int[] grades);
    }
}