using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexagonalArchitecture.AwesomeShop.Application.UseCases
{
    public class UseCaseResult
    {
        public UseCaseResult(bool success, string message = "")
        {
            Success = success;
            Message = message;
        }

        public bool Success { get; protected set; }
        public string Message { get; protected set; }
    }

    public class UseCaseResult<T> : UseCaseResult
    {
        public UseCaseResult(T data, bool success, string message = "") : base(success, message)
        {
            Data = data;
        }

        public T Data { get; private set; }
    }
}
