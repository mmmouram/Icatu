using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using backend.src.Models;
using backend.src.Models.DTOs;
using backend.src.Repositories;
using backend.src.Services;

namespace backend.test.Services
{
    [TestFixture]
    public class SinistroServiceTests
    {
        private Mock<ISinistroRepository> _sinistroRepositoryMock;
        private ISinistroService _sinistroService;

        [SetUp]
        public void Setup()
        {
            _sinistroRepositoryMock = new Mock<ISinistroRepository>();
            _sinistroService = new SinistroService(_sinistroRepositoryMock.Object);
        }

        [Test]
        public async Task RegistrarSinistroAsync_DeveRegistrarSinistro_RetornandoRespostaCorreta()
        {
            // Arrange
            var request = new SinistroRequest { Descricao = "Sinistro Teste" };
            Sinistro sinistroCapturado = null;
            _sinistroRepositoryMock
                .Setup(s => s.AdicionarSinistroAsync(It.IsAny<Sinistro>()))
                .Returns(Task.CompletedTask)
                .Callback<Sinistro>(s => {
                    s.Id = 1;  // Simula a atribuição do ID
                    sinistroCapturado = s;
                });

            // Act
            var response = await _sinistroService.RegistrarSinistroAsync(request);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(1, response.Id);
            Assert.AreEqual("Registrado", response.Status);
            Assert.AreEqual(sinistroCapturado.DataRegistro, response.DataRegistro);
            _sinistroRepositoryMock.Verify(s => s.AdicionarSinistroAsync(It.IsAny<Sinistro>()), Times.Once);
        }

        [Test]
        public async Task EnviarDocumentoAsync_DeveEnviarDocumento_RetornandoRespostaCorreta()
        {
            // Arrange
            int sinistroId = 1;

            // Criando um mock de IFormFile com conteúdo em memória
            var content = "Fake file content";
            var fileName = "teste.pdf";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            formFileMock.Setup(f => f.FileName).Returns(fileName);
            formFileMock.Setup(f => f.Length).Returns(stream.Length);
            formFileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns((Stream s, System.Threading.CancellationToken _) => stream.CopyToAsync(s));

            Documento documentoCapturado = null;
            _sinistroRepositoryMock
                .Setup(r => r.AdicionarDocumentoAsync(It.IsAny<Documento>()))
                .Returns(Task.CompletedTask)
                .Callback<Documento>(doc => documentoCapturado = doc);

            // Act
            var response = await _sinistroService.EnviarDocumentoAsync(sinistroId, formFileMock.Object);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(fileName, response.NomeArquivo);
            Assert.AreEqual("Documento enviado com sucesso.", response.Mensagem);
            // Verifica se o documento foi capturado e possui o SinistroId correto
            Assert.IsNotNull(documentoCapturado);
            Assert.AreEqual(sinistroId, documentoCapturado.SinistroId);

            _sinistroRepositoryMock.Verify(r => r.AdicionarDocumentoAsync(It.IsAny<Documento>()), Times.Once);
        }

        [Test]
        public async Task AcompanharSinistroAsync_SinistroNaoEncontrado_DeveRetornarNull()
        {
            // Arrange
            int sinistroId = 999;
            _sinistroRepositoryMock
                .Setup(r => r.ObterSinistroPorIdAsync(sinistroId))
                .ReturnsAsync((Sinistro)null);

            // Act
            var response = await _sinistroService.AcompanharSinistroAsync(sinistroId);

            // Assert
            Assert.IsNull(response);
        }

        [Test]
        public async Task AcompanharSinistroAsync_SinistroEncontrado_DeveRetornarRespostaCorreta()
        {
            // Arrange
            int sinistroId = 1;
            var sinistro = new Sinistro
            {
                Id = sinistroId,
                Descricao = "Teste",
                DataRegistro = DateTime.UtcNow,
                Status = "Registrado"
            };

            _sinistroRepositoryMock
                .Setup(r => r.ObterSinistroPorIdAsync(sinistroId))
                .ReturnsAsync(sinistro);

            // Act
            var response = await _sinistroService.AcompanharSinistroAsync(sinistroId);

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(sinistro.Id, response.Id);
            Assert.AreEqual(sinistro.DataRegistro, response.DataRegistro);
            Assert.AreEqual(sinistro.Status, response.Status);
        }
    }
}
