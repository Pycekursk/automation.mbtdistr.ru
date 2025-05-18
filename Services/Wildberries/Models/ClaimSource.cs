using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace automation.mbtdistr.ru.Services.Wildberries.Models
{
  /// <summary>
  /// Источник подачи заявки на возврат.
  /// </summary>
  public enum ClaimSource
  {
    /// <summary>
    /// 1 — портал покупателей.
    /// </summary>
    [EnumMember(Value = "1")]
    [Display(Name = "Портал покупателей")]
    Portal = 1,

    /// <summary>
    /// 3 — чат.
    /// </summary>
    [EnumMember(Value = "3")]
    [Display(Name = "Чат")]
    Chat = 3
  }
}



