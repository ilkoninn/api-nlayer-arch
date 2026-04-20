namespace App.Core.Exceptions;

/// <summary>Müəssisə qaydası pozuntusu (məs. dərs vaxtı üst-üstə düşür).</summary>
public sealed class BusinessConflictException(string message) : Exception(message);
