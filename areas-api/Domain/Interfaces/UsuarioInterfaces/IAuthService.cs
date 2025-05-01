using Domain.Entities;

namespace Domain.Interfaces.UsuarioInterfaces
{
    public interface IAuthService
    {
        Usuario? GetUsuarioLogado();
    }
}
