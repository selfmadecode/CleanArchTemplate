using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEncryptService
    {
        public string Encrypt(string message);

        public string Decrypt(string cipherText);
    }
}
