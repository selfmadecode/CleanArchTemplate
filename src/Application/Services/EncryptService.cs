using Application.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services;

public class EncryptService : IEncryptService
{
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private string Key;
    public EncryptService(IDataProtectionProvider dataProtectionProvider, IConfiguration configuration)
    {
        _dataProtectionProvider = dataProtectionProvider;
        Key = configuration["Encyption:Key"];
    }
    public string Decrypt(string cipherText)
    {
        var protector = _dataProtectionProvider.CreateProtector(Key);
        return protector.Unprotect(cipherText);
    }

    public string Encrypt(string input)
    {
        var protector = _dataProtectionProvider.CreateProtector(Key);
        return protector.Protect(input);
    }
}
