﻿using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HabitTracker.Data.Data
{
  /// <summary>
  /// Класс для работы с файлом конфигурации приложения.
  /// </summary>
  public class Config
  {
    /// <summary>
    /// Получает или устанавливает строку подключения к базе данных.
    /// </summary>
    public string DatabaseConnectionString { get; set; }

    /// <summary>
    /// Получает или устанавливает токен бота Telegram.
    /// </summary>
    public string BotToken { get; set; }


    /// <summary>
    /// Загружает конфигурацию из указанного YAML файла.
    /// </summary>
    /// <param name="path">Путь к файлу конфигурации YAML.</param>
    /// <returns>Экземпляр класса Config с загруженными настройками.</returns>
    public static Config LoadFromFile(string path)
    {
      try
      {
        var deserializer = new DeserializerBuilder()
          .WithNamingConvention(CamelCaseNamingConvention.Instance)
          .Build();

        using var reader = new StreamReader(path);
        return deserializer.Deserialize<Config>(reader);
      }
      catch(Exception ex)
      {
        Console.WriteLine($"Error loading config: {ex.Message}");
        throw;
      }
    }
  }
}
