﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace HabitTracker.Data.Data
{
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
    /// Получает или устанавливает идентификатор администратора бота.
    /// </summary>
    public long AdminId { get; set; }

    /// <summary>
    /// Загружает конфигурацию из указанного YAML файла.
    /// </summary>
    /// <param name="path">Путь к файлу конфигурации YAML.</param>
    /// <returns>Экземпляр класса Config с загруженными настройками.</returns>
    public static Config LoadFromFile(string path)
    {
      var deserializer = new DeserializerBuilder()
          .WithNamingConvention(CamelCaseNamingConvention.Instance)
          .Build();

      using var reader = new StreamReader(path);
      return deserializer.Deserialize<Config>(reader);
    }
  }
}
