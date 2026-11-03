using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.AwesomeShop.Application.ViewModels
{
    public class ResultBase
    {
        public ResultBase(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public bool Success { get; private set; }
        public string Message { get; private set; }
    }

    public class ResultBase<T> : ResultBase
    {
        public ResultBase(T data, bool success = true, string message = "") : base(success, message)
        {
            Data = data;
        }

        public T Data { get; private set; }
    }
}
