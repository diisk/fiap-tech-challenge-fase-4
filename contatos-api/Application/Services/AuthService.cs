using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions.AuthExceptions;
using Domain.Interfaces;
using Domain.Interfaces.UsuarioInterfaces;
using Microsoft.AspNetCore.Http;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository usuarioRepository;
        private readonly ICryptoService cryptoService;
        private readonly ITokenService tokenService;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IEventPublisher eventPublisher;

        public AuthService(
            IUsuarioRepository usuarioRepository,
            ICryptoService cryptoService,
            ITokenService tokenService,
            IHttpContextAccessor httpContextAccessor,
            IEventPublisher eventPublisher)
        {
            this.usuarioRepository = usuarioRepository;
            this.cryptoService = cryptoService;
            this.tokenService = tokenService;
            this.httpContextAccessor = httpContextAccessor;
            this.eventPublisher = eventPublisher;
        }

        public Usuario? GetUsuarioLogado()
        {
            var user = httpContextAccessor.HttpContext.User;
            if (user.Identity is { IsAuthenticated: false })
            {
                return null;
            }
            var userId = Convert.ToInt32(tokenService.GetIdentifierFromClaimsPrincipal(user));
            var retorno = usuarioRepository.FindById(userId);

            if (retorno == null)
            {
                throw new ErroInesperadoException("Usuário não encontrado.");
            }

            return retorno;
        }
    }
}
