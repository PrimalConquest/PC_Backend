using System;
using System.Collections.Generic;
using System.Text;

namespace AccountComunication
{
    internal class RegisterDTO
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
