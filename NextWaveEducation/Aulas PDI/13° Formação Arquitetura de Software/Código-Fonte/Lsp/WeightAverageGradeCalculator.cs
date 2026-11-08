using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SolidPrinciples.Lsp
{
    public class WeightAverageGradeCalculator : IAverageGradeCalculator
    {
        public string Subject { get; set; }
        public int Year { get; set; }
        public int Semester { get; set; }
        public WeightAverageGradeCalculator(string subject, int year, int semester)
        {
            Subject = subject;
            Year = year;
            Semester = semester;
        }

        public  double Calculate(int[] grades)
        {
            var average = (grades[0] * 2 + grades[1] * 2 + grades[2] * 3 + grades[3] * 3)/10;

            Console.WriteLine($"Matéria: {Subject} do período {Semester}/{Year} é: {average}");

            return average;
        }
    }
}