# Лабораторна робота №7 — Generics, IRepository (варіант 11: Movie)

Структура:
- `lab12v11/` — консольний проєкт
  - `IEntity` — інтерфейс з властивістю `int Id { get; set; }`
  - `Movie : IEntity` — Title (string), Director (string), ReleaseYear (int)
  - `IRepository<T> where T : IEntity` — GetById, GetAll, Add, Update, Delete
  - `InMemoryRepository<T>` — зберігання у `List<T>`; Id генерується автоматично, якщо дорівнює 0;
    `GetById` повертає `null` для неіснуючого Id; `Update`/`Delete` кидають `KeyNotFoundException`;
    `Add` з дублікатом Id кидає `InvalidOperationException`.
  - `Program.cs` — демонстрація роботи
- `lab12v11.Tests/` — проєкт xUnit з тестами для всіх методів репозиторію


