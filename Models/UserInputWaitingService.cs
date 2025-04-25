//namespace automation.mbtdistr.ru.Models
//{
//  public class UserInputWaitingService
//  {
//    private readonly Dictionary<long, (string Action, int EntityId)> _waiting = new();

//    public void Register(long userId, string action, int entityId)
//    {
//      _waiting[userId] = (action, entityId);
//    }

//    public (string Action, int EntityId)? Get(long userId)
//    {
//      if (_waiting.TryGetValue(userId, out var data))
//        return (data.Item1, data.Item2);
//      else
//        return null;
//    }

//    public void Remove(long userId)
//    {
//      _waiting.Remove(userId);
//    }
//  }
//}

using System;
using System.Collections.Generic;
using System.Threading;

namespace automation.mbtdistr.ru.Models
{
  /// <summary>  
  /// Сервис для отслеживания блокировок ввода пользователя и их автоматического удаления по истечении времени ожидания.  
  /// </summary>  
  public class UserInputWaitingService
  {
    // Тайм-аут по умолчанию для блокировок ожидания (5 минут).  
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);

    // Хранит ожидающие действия ввода пользователя.  
    private readonly Dictionary<long, (string Action, int EntityId)> _waiting
        = new Dictionary<long, (string Action, int EntityId)>();

    // Таймеры для автоматического удаления записей по истечении времени ожидания.  
    private readonly Dictionary<long, Timer> _timers
        = new Dictionary<long, Timer>();

    /// <summary>  
    /// Регистрирует блокировку ожидания ввода пользователя и планирует её удаление по истечении тайм-аута.  
    /// </summary>  
    /// <param name="userId">Идентификатор пользователя.</param>  
    /// <param name="action">Действие, ожидающее ввода.</param>  
    /// <param name="entityId">Идентификатор сущности, связанной с действием.</param>  
    public void Register(long userId, string action, int entityId)
    {
      // Отмена и освобождение существующего таймера для данного пользователя  
      if (_timers.TryGetValue(userId, out var existingTimer))
      {
        existingTimer.Change(Timeout.Infinite, Timeout.Infinite);
        existingTimer.Dispose();
        _timers.Remove(userId);
      }

      // Добавление или обновление записи ожидания  
      _waiting[userId] = (action, entityId);

      // Создание таймера для удаления записи по истечении тайм-аута  
      var timer = new Timer(_ => Remove(userId), null, DefaultTimeout, Timeout.InfiniteTimeSpan);
      _timers[userId] = timer;
    }

    /// <summary>  
    /// Получает ожидающее действие ввода для пользователя или null, если оно отсутствует.  
    /// </summary>  
    /// <param name="userId">Идентификатор пользователя.</param>  
    /// <returns>Кортеж с действием и идентификатором сущности или null.</returns>  
    public (string Action, int EntityId)? Get(long userId)
    {
      if (_waiting.TryGetValue(userId, out var data))
        return (data.Action, data.EntityId);

      return null;
    }

    /// <summary>  
    /// Удаляет блокировку ожидания для пользователя и освобождает связанный таймер.  
    /// </summary>  
    /// <param name="userId">Идентификатор пользователя.</param>  
    public void Remove(long userId)
    {
      _waiting.Remove(userId);

      if (_timers.TryGetValue(userId, out var timer))
      {
        timer.Change(Timeout.Infinite, Timeout.Infinite);
        timer.Dispose();
        _timers.Remove(userId);
      }
    }
  }
}

