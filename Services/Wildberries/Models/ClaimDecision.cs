using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace automation.mbtdistr.ru.Services.Wildberries.Models
{
  /// <summary>
  /// Решение по возврату покупателю.
  /// </summary>
  public enum ClaimDecision
  {
    /// <summary>
    /// 0 — на рассмотрении.
    /// </summary>
    [EnumMember(Value = "0")]
    [Display(Name = "На рассмотрении")]
    UnderReview = 0,

    /// <summary>
    /// 1 — отказ.
    /// </summary>
    [EnumMember(Value = "1")]
    [Display(Name = "Отказ")]
    Rejected = 1,

    /// <summary>
    /// 2 — одобрено.
    /// </summary>
    [EnumMember(Value = "2")]
    [Display(Name = "Одобрено")]
    Approved = 2
  }
}



