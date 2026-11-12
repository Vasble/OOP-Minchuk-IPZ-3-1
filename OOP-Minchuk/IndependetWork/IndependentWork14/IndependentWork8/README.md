# Самостійна робота №14 — Dictionary<TKey, TValue> та HashSet<T>

**Дисципліна:** Об'єктно-орієнтоване програмування
**Тема:** Вивчення `Dictionary<TKey, TValue>` та `HashSet<T>` у бібліотеках .NET

Аналіз open-source прикладів (Dapper, Serilog, Newtonsoft.Json), порівняльна таблиця складності, висновки й відповіді на контрольні запитання — у файлі [REPORT.md](REPORT.md).

## Структура

```
IndependentWork8/
├── IndependentWork8.sln
├── README.md
├── REPORT.md
└── IndependentWork8/
    ├── FileMetadata.cs          # модель метаданих файлу
    ├── FileMetadataCache.cs     # сценарій 1: Dictionary<string, FileMetadata>
    ├── TagManager.cs            # сценарій 2: Dictionary<string, HashSet<string>>
    ├── SearchIndex.cs           # сценарій 3: Dictionary<string, HashSet<int>>
    └── Program.cs               # демонстрація всіх сценаріїв
```

## Реалізовані сценарії

| Сценарій | Клас | Колекції | Методи |
|----------|------|----------|--------|
| 1. Кеш метаданих | `FileMetadataCache` | `Dictionary<string, FileMetadata>` з `StringComparer.OrdinalIgnoreCase` | `Add`, `Get`, `TryGet`, `Remove`, `Contains`, `Count` |
| 2. Унікальні теги | `TagManager` | `Dictionary<string, HashSet<string>>` | `AddTag`, `RemoveTag`, `GetTags`, `GetAllUniqueTags` (`UnionWith`), `FindCommonTags` (`IntersectWith`), `FindDifferentTags` (`ExceptWith`) |
| 3. Пошуковий індекс (додатково) | `SearchIndex` | `Dictionary<string, HashSet<int>>` | `AddDocument`, `Search`, `SearchMultiple` (`IntersectWith`) |


## Приклад виводу програми

```
=== Сценарій 1: FileMetadataCache (Dictionary) ===
Додано файлів: 3
Get("photo.png")   -> photo.png | 1204551 байт | 2026-11-11 09:15 | 7be402
Get("REPORT.DOCX") -> report.docx | 48213 байт | 2026-11-10 14:30 | a3f9c1
Contains("notes.txt") -> True
Після оновлення notes.txt -> notes.txt | 1450 байт | 2026-11-12 15:40 | d00d42
Remove("photo.png") -> True
Contains("photo.png") -> False
Кількість файлів у кеші: 2
KeyNotFoundException: Метадані для файлу 'photo.png' не знайдено.

=== Сценарій 2: TagManager (HashSet) ===
Теги post-1: { collections, csharp, dotnet }
Теги post-2: { csharp, dotnet, linq }
Теги post-3: { csharp, gamedev, unity }
Усі унікальні теги (UnionWith): { collections, csharp, dotnet, gamedev, linq, unity }
Спільні теги post-1 і post-2 (IntersectWith): { csharp, dotnet }
Є в post-1, але немає в post-2 (ExceptWith): { collections }
Після RemoveTag("post-1", "collections"): { csharp, dotnet }

=== Сценарій 3: SearchIndex (Dictionary + HashSet) ===
Документ 1: Dictionary зберігає пари ключ значення
Документ 2: HashSet зберігає лише унікальні значення
Документ 3: Dictionary і HashSet використовують хеш таблицю
Документ 4: List зберігає елементи за індексом
Search("зберігає") -> { 1, 2, 4 }
Search("hashset") -> { 2, 3 }
Search("масив") -> { }
SearchMultiple("зберігає", "значення") -> { 1, 2 }
SearchMultiple("Dictionary", "HashSet") -> { 3 }
```
