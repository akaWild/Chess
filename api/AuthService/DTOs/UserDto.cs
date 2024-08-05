namespace AuthService.DTOs;

public record UserDto(string DisplayName, string Username, string Password, string Token);