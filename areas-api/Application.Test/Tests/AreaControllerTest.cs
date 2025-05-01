using System;
using System.Net;
using System.Text;
using System.Text.Json;
using Application.Exceptions;
using Application.Services;
using Application.Test.Factories;
using Application.Test.Fixtures;
using Domain.Entities;
using Domain.Enums.AreaEnums;
using Domain.Exceptions.AreaExceptions;
using Domain.Interfaces.AreaInterfaces;
using Infrastructure.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Testcontainers.MySql;
using Application.DTOs;
using System.Net.Http.Headers;

namespace Application.Test.Tests
{
    [Trait("Category", "Integration")]
    public class AreaControllerTest : IAsyncLifetime
    {

        private readonly MySqlContainer mySqlContainer;
        private readonly HttpClient client;

        public AreaControllerTest()
        {
            mySqlContainer = MySqlContainerFactory.CreateMySqlContainer();
            client = new CustomWebApplicationFactory(mySqlContainer).CreateClient();
        }

        public async Task DisposeAsync()
        {
            await mySqlContainer.DisposeAsync();
        }

        public async Task InitializeAsync()
        {
            CryptoService cryptoService = new CryptoService();
            await mySqlContainer.StartAsync();

            using var scope = new CustomWebApplicationFactory(mySqlContainer).Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OnlyReadDbContext>();

            await dbContext.Database.MigrateAsync();
            await dbContext.Database.EnsureCreatedAsync();

            await dbContext.UsuarioSet.AddAsync(new Usuario
            {
                Login = "teste",
                Senha = cryptoService.HashearSenha("teste"),
            });
            await dbContext.SaveChangesAsync();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJJZGVudGlmaWNhZG9yIjoiMVNxNEdkcUdJTkJaZmtjQk1leFFra2tWL0M4RTMzemVrU0Fva2x3K0VUWT0iLCJuYmYiOjE3NDIxMzE0MTcsImV4cCI6MzMxOTk2ODIxNywiaWF0IjoxNzQyMTMxNDE3fQ.Vb_YTdBvGR6oBNeqkWoPqakPAaBx9vaGrd3mi5tZvXI"
                );
        }

        [Fact]
        public async Task Should_Cadastrar_Area_Successfully()
        {
            // Arrange
            var area = new
            {
                areas = new[]
                {
                new
                {
                    codigo = "11",
                    regiao = 3,
                    siglaEstado = "SP",
                    cidades = "S�o Paulo;Guarulhos;Osasco;Diadema",
                    descricao = "Regi�o Metropolitana de S�o Paulo"
                }
            }
            };
            var content = new StringContent(JsonSerializer.Serialize(area), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/api/areas", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

    }
}