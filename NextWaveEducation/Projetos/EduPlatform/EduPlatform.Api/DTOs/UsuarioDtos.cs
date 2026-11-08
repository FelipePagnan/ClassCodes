using EduPlatform.Api.Models;

namespace EduPlatform.Api.DTOs;

public record UsuarioCreateDto(
    string Nome,
    string Sobrenome,
    string Email,
    string Senha,
    DateTime DataNascimento,
    string? Documento,
    string? TelefoneCelular,
    RoleUsuario Role);

public record UsuarioUpdateDto(
    string Nome,
    string Sobrenome,
    string Email,
    DateTime DataNascimento,
    string? Documento,
    string? TelefoneCelular,
    RoleUsuario Role,
    bool Ativo);

public record UsuarioResponseDto(
    Guid Id,
    string Nome,
    string Sobrenome,
    string NomeCompleto,
    string Email,
    DateTime DataNascimento,
    string? Documento,
    string? TelefoneCelular,
    RoleUsuario Role,
    bool Ativo)
{
    public static UsuarioResponseDto FromEntity(Usuario u) => new(
        u.Id, u.Nome, u.Sobrenome, u.NomeCompleto, u.Email, u.DataNascimento,
        u.Documento, u.TelefoneCelular, u.Role, u.Ativo);
}
