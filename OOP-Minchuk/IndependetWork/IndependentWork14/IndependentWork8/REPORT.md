# Звіт до самостійної роботи №14

**Тема:** Вивчення `Dictionary<TKey, TValue>` та `HashSet<T>` у бібліотеках .NET

## 1. Результати аналізу open-source проєктів

Я переглянув вихідний код трьох популярних бібліотек: **Dapper**, **Serilog** і **Newtonsoft.Json**.
Посилання ведуть на конкретні коміти, тому номери рядків не зсунуться, навіть якщо код у репозиторіях зміниться.

### Приклад 1. `Dictionary<Type, TypeMapEntry>` у Dapper (`SqlMapper.typeMap`)

**Посилання:**
- [Оголошення поля `typeMap`](https://github.com/DapperLib/Dapper/blob/eb47546a408bbf6c178bda4f04dc205bc51ffbfe/Dapper/SqlMapper.cs#L171)
- [Заповнення словника в статичному конструкторі](https://github.com/DapperLib/Dapper/blob/eb47546a408bbf6c178bda4f04dc205bc51ffbfe/Dapper/SqlMapper.cs#L202-L215)
- [`AddTypeMap` / `RemoveTypeMap`](https://github.com/DapperLib/Dapper/blob/eb47546a408bbf6c178bda4f04dc205bc51ffbfe/Dapper/SqlMapper.cs#L293-L338)
- [Пошук у `LookupDbType`](https://github.com/DapperLib/Dapper/blob/eb47546a408bbf6c178bda4f04dc205bc51ffbfe/Dapper/SqlMapper.cs#L463-L483)

```csharp
private static Dictionary<Type, TypeMapEntry> typeMap;

typeMap = new Dictionary<Type, TypeMapEntry>(41)
{
    [typeof(byte)] = DbType.Byte,
    [typeof(int)] = DbType.Int32,
    ...
};

if (typeMap.TryGetValue(type, out var mapEntry)) { ... }
```

- **Контекст.** Dapper — micro-ORM, який підставляє параметри в SQL-запити. Для кожного параметра треба визначити тип у базі даних (`DbType`) за типом C# (`int` → `Int32`, `string` → `String`, `Guid` → `Guid` тощо).
- **Чому `Dictionary`.** Це класичне відображення «ключ → значення». Пошук відбувається для кожного параметра кожного запиту, тож він мусить бути швидким. Зі `List<(Type, DbType)>` довелося б перебирати ~45 елементів щоразу, а словник знаходить запис одразу за хешем ключа.
- **Складність.** `TryGetValue` і `ContainsKey` — O(1) в середньому. `AddTypeMap` і `RemoveTypeMap` — O(n), бо вони копіюють весь словник (див. нижче). Але ці методи викликаються рідко, під час налаштування.
- **Особливості.**
  - Ємність задано наперед (`41` + 4 для .NET 6+). Словник не перебудовує внутрішню таблицю під час заповнення.
  - `Dictionary` не потокобезпечний. Тому Dapper використовує підхід **copy-on-write**: створює копію (`new Dictionary<Type, TypeMapEntry>(snapshot)`), змінює її і замінює посилання. Потоки, які зараз читають, і далі бачать старий цілісний словник.
  - Використовується `TryGetValue`, а не індексатор: так пошук відбувається один раз і не буде `KeyNotFoundException` для типів, яких немає в таблиці.

### Приклад 2. `HashSet<Type>` у Serilog (`BuiltInScalarTypes`)

**Посилання:**
- [`PropertyValueConverter.BuiltInScalarTypes`](https://github.com/serilog/serilog/blob/43105b35095337d77f022561a8f2d3db2f9a7937/src/Serilog/Capturing/PropertyValueConverter.cs#L23-L32)
- [`SimpleScalarConversionPolicy._scalarTypes`](https://github.com/serilog/serilog/blob/43105b35095337d77f022561a8f2d3db2f9a7937/src/Serilog/Policies/SimpleScalarConversionPolicy.cs#L17-L37)
- [`_lastSeenNames` у `CreateStructureValue`](https://github.com/serilog/serilog/blob/43105b35095337d77f022561a8f2d3db2f9a7937/src/Serilog/Capturing/PropertyValueConverter.cs#L435-L509)

```csharp
static readonly HashSet<Type> BuiltInScalarTypes = new()
{
    typeof(decimal), typeof(string),
    typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan),
    typeof(Guid), typeof(Uri), ...
};

if (_scalarTypes.Contains(value.GetType())) { ... }
```

- **Контекст.** Serilog — бібліотека структурованого логування. Коли в лог передають об'єкт, Serilog вирішує, як його записати: як просте значення (скаляр) чи розібрати на властивості. Для цього він перевіряє, чи входить тип об'єкта до набору «скалярних» типів.
- **Чому `HashSet`.** Тут потрібна лише відповідь на питання «чи є тип у наборі?», а значення до ключа не потрібне. Тому `Dictionary` зайвий, а `List.Contains` перебирав би всі елементи. До того ж набір будується з вбудованих типів **плюс** типів, які додав користувач (`BuiltInScalarTypes.Concat(additionalScalarTypes)`). `HashSet` сам прибирає дублікати, якщо користувач додасть тип, що вже є серед вбудованих.
- **Складність.** `Contains` — O(1) в середньому. Перевірка виконується для кожного значення в кожному записі логу, тому це важливо для продуктивності.
- **Особливості.**
  - Компаратор за замовчуванням: для `Type` рівність означає той самий тип.
  - Другий приклад у тому ж класі — `[ThreadStatic] static HashSet<string>? _lastSeenNames`. Він пропускає властивості з однаковим ім'ям (наприклад, коли похідний клас приховує властивість базового через `new`). Множина очищається (`Clear()`) і використовується знову, щоб не створювати новий об'єкт для кожного запису. `[ThreadStatic]` дає кожному потоку окрему копію, тому множина безпечна без блокувань.

### Приклад 3 (додатковий). `Dictionary` + `HashSet` разом у Dapper (`PassByPosition`)

**Посилання:** [`SqlMapper.PassByPosition`](https://github.com/DapperLib/Dapper/blob/eb47546a408bbf6c178bda4f04dc205bc51ffbfe/Dapper/SqlMapper.cs#L1907-L1950)

```csharp
Dictionary<string, IDbDataParameter> parameters = new(StringComparer.Ordinal);
var consumed = new HashSet<string>(StringComparer.Ordinal);

if (!consumed.Add(key))
    throw new InvalidOperationException("... each parameter can only be referenced once");
else if (parameters.TryGetValue(key, out IDbDataParameter? param)) { ... }
```

- **Контекст.** Деякі драйвери БД (наприклад, OLE DB) підтримують лише позиційні параметри `?`. Dapper переписує SQL на кшталт `?id?` у `?` і розставляє параметри в потрібному порядку.
- **Розподіл ролей:**
  - `Dictionary<string, IDbDataParameter>` за O(1) знаходить параметр за ім'ям, знайденим у тексті SQL.
  - `HashSet<string> consumed` пам'ятає, які імена вже використано. `Add` повертає `false`, якщо ім'я вже було, і це одразу виявляє помилку повторного використання параметра. Перевірка й додавання відбуваються однією операцією.
- **Особливості.** Обидві колекції створено зі `StringComparer.Ordinal`: імена порівнюються побайтово, без урахування культури, і це швидше, ніж культурне порівняння.

### Приклад 4 (додатковий). Custom comparer у Newtonsoft.Json (`BidirectionalDictionary`)

**Посилання:**
- [`BidirectionalDictionary<TFirst, TSecond>`](https://github.com/JamesNK/Newtonsoft.Json/blob/52fa3aef1f2cadcd3a3f874251eddc98d3efbbaa/Src/Newtonsoft.Json/Utilities/BidirectionalDictionary.cs#L33-L96)
- [`ReferenceEqualsEqualityComparer` і створення словника](https://github.com/JamesNK/Newtonsoft.Json/blob/52fa3aef1f2cadcd3a3f874251eddc98d3efbbaa/Src/Newtonsoft.Json/Serialization/JsonSerializerInternalBase.cs#L36-L80)

- **Контекст.** Під час серіалізації з `PreserveReferencesHandling` кожному об'єкту присвоюється `$id`, а повторні посилання записуються як `$ref`. Тож треба шукати і «id → об'єкт», і «об'єкт → id».
- **Рішення.** Два `Dictionary`, по одному на кожен напрямок. Метод `Set` перевіряє обидва, щоб відповідність була строго один-до-одного.
- **Custom comparer.** Для ключа-об'єкта використано `ReferenceEqualsEqualityComparer` (`ReferenceEquals` + `RuntimeHelpers.GetHashCode`). Автори пояснюють у коментарі: об'єкт змінюється під час десеріалізації, тож його `GetHashCode()` може змінитися. Тоді елемент «загубився б» у словнику. Порівняння за посиланням дає стабільний хеш і відрізняє два різні об'єкти навіть тоді, коли їхній `Equals` повертає `true`.

## 2. Порівняльна таблиця складності операцій

| Операція | `List<T>` | `Dictionary<TKey, TValue>` | `HashSet<T>` | Коли використовувати |
|----------|-----------|----------------------------|--------------|----------------------|
| Додавання | O(1) амортизовано (у кінець), O(n) — вставка в середину | O(1) амортизовано | O(1) амортизовано | `List` — коли важливий порядок і дозволені дублікати; `Dictionary` — коли треба зберегти значення під ключем; `HashSet` — коли елементи мають бути унікальними |
| Пошук за значенням | O(n) (`Contains`, `IndexOf`) | O(n) (`ContainsValue`), але зазвичай так не роблять | O(1) (`Contains`) | Для частих перевірок «чи є елемент?» найкраще підходить `HashSet` |
| Пошук за ключем / індексом | O(1) за індексом, ключів немає | O(1) (`TryGetValue`, індексатор) | N/A (ключів немає) | `Dictionary` — коли за ідентифікатором потрібні дані (кеш, довідник, індекс) |
| Видалення | O(n) (пошук + зсув елементів) | O(1) (`Remove(key)`) | O(1) (`Remove`) | Якщо часто видаляєте конкретні елементи, `List` найповільніший |
| Перевірка наявності | O(n) | O(1) за ключем (`ContainsKey`), O(n) за значенням | O(1) | `HashSet` / `ContainsKey` замість `List.Contains` для великих наборів |
| Операції над множинами | O(n·m) вручну або через LINQ | N/A | `UnionWith` O(m), `IntersectWith` O(n), `ExceptWith` O(m) | `HashSet` — для перетину, об'єднання й різниці наборів |
| Порядок елементів | Зберігається | Не гарантується | Не гарантується | Якщо потрібен порядок, беріть `List`, `SortedDictionary` або `SortedSet` |

O(1) для `Dictionary` і `HashSet` — це **середній** випадок. Якщо хеш-функція погана і багато ключів потрапляють в один кошик (колізії), операції деградують до O(n).

## 3. Висновки про вибір колекції

### Коли `Dictionary` краще за `List`?
- Коли дані шукають за ідентифікатором: ім'ям файлу, id користувача, типом. У `List` це O(n), у `Dictionary` — O(1).
- Коли треба зіставити одне з іншим: тип → `DbType` (Dapper), слово → документи (інвертований індекс), стаття → теги.
- Коли ключі мають бути унікальними: `Dictionary` не дасть двічі додати той самий ключ (`Add` викине виняток, а індексатор перезапише значення).
- `List` кращий для невеликих наборів (кілька елементів), коли важливий порядок, коли доступ іде за індексом або коли дані просто перебирають.

### Коли `HashSet` є оптимальним вибором?
- Коли потрібна лише відповідь «чи є елемент у наборі?» (Serilog: «чи цей тип скалярний?»).
- Коли треба автоматично прибрати дублікати (теги статті, відвідані вузли графа, оброблені імена).
- Коли потрібні операції над множинами: об'єднання, перетин, різниця (`UnionWith`, `IntersectWith`, `ExceptWith`).
- Коли зручно «перевірити й додати» однією операцією: `if (!set.Add(x))` (Dapper `consumed`).

### Підводні камені
1. **Змінні ключі.** Якщо змінити поле, від якого залежить `GetHashCode()`, після того як об'єкт додано в колекцію, елемент більше не знайдеться. Тому ключами краще робити незмінні значення (`string`, `int`, `Guid`, `record`). Інакше доведеться робити як Newtonsoft.Json: порівнювати об'єкти за посиланням.
2. **`Equals` і `GetHashCode` мають бути узгоджені.** Якщо `Equals` повертає `true`, хеш-коди теж мають збігатися. Власний клас без перевизначення цих методів порівнюється за посиланням, і два «однакові» об'єкти стануть різними ключами.
3. **Регістр рядків.** За замовчуванням `"CSharp"` і `"csharp"` — різні ключі. Для тегів, імен файлів у Windows чи пошукових слів потрібен `StringComparer.OrdinalIgnoreCase`.
4. **Індексатор кидає `KeyNotFoundException`.** Для ключів, яких може не бути, краще `TryGetValue`: один пошук замість двох (`ContainsKey` + індексатор) і жодного винятку.
5. **Порядок не гарантується.** Не можна покладатися на порядок перебору `Dictionary` чи `HashSet`. Для стабільного виводу треба сортувати.
6. **Потокобезпечність.** `Dictionary` і `HashSet` не потокобезпечні для запису. Для кількох потоків потрібен `ConcurrentDictionary`, `lock` або підхід copy-on-write, як у Dapper.
7. **Пам'ять.** Хеш-таблиця займає більше пам'яті, ніж масив. Для 3–5 елементів `List` або масив може працювати не гірше.
8. **Повернення внутрішньої колекції.** Якщо метод віддає назовні свій внутрішній `HashSet`, викликач може його змінити. Тому в `TagManager` і `SearchIndex` методи повертають копії.

## 4. Приклади з власного коду

Код лежить у проєкті `IndependentWork8`, повний вивід програми — у [README.md](README.md).

### Сценарій 1 — `FileMetadataCache` (`Dictionary<string, FileMetadata>`)
- Ключ — ім'я файлу, значення — об'єкт метаданих. Методи `Add`, `Get`, `TryGet`, `Remove`, `Contains` працюють за O(1).
- Використано `StringComparer.OrdinalIgnoreCase`, бо в Windows `report.docx` і `REPORT.DOCX` — той самий файл. У виводі `Get("REPORT.DOCX")` знаходить `report.docx`.
- Повторний `Add` для того ж файлу оновлює метадані (`_cache[key] = value`), а не викидає виняток.
- `Get` для відсутнього файлу викидає `KeyNotFoundException` з пояснювальним повідомленням.

```
Get("REPORT.DOCX") -> report.docx | 48213 байт | 2026-11-10 14:30 | a3f9c1
Remove("photo.png") -> True
KeyNotFoundException: Метадані для файлу 'photo.png' не знайдено.
```

### Сценарій 2 — `TagManager` (`Dictionary<string, HashSet<string>>`)
- Для кожної статті зберігається множина тегів. Дублікати відкидаються автоматично: тег `CSharp` не додався до `post-1`, бо `csharp` там уже є (компаратор без урахування регістру).
- `GetAllUniqueTags` — `UnionWith`, `FindCommonTags` — `IntersectWith`, `FindDifferentTags` — `ExceptWith`.
- Якщо в статті не лишилося тегів, вона видаляється зі словника, щоб не зберігати порожні множини.

```
Усі унікальні теги (UnionWith): { collections, csharp, dotnet, gamedev, linq, unity }
Спільні теги post-1 і post-2 (IntersectWith): { csharp, dotnet }
Є в post-1, але немає в post-2 (ExceptWith): { collections }
```

### Сценарій 3 — `SearchIndex` (`Dictionary<string, HashSet<int>>`)
- Інвертований індекс: слово → множина id документів. Слово, яке трапляється в документі кілька разів, дає лише один запис завдяки `HashSet`.
- `Search` — O(1) пошук у словнику.
- `SearchMultiple` спочатку сортує множини за розміром і перетинає, починаючи з найменшої. Так результат зменшується якнайшвидше, а якщо він стає порожнім, цикл зупиняється.

```
Search("hashset") -> { 2, 3 }
SearchMultiple("Dictionary", "HashSet") -> { 3 }
```

## 5. Відповіді на контрольні запитання

**1. Які переваги generic-колекцій над неузагальненими структурами даних у .NET?**
Типобезпечність: у `Dictionary<string, int>` не вийде покласти `double`, а помилку видно ще під час компіляції. У `Hashtable` або `ArrayList` усе зберігається як `object`, і помилки з'являються тільки під час виконання (`InvalidCastException`).
Немає приведень типів під час читання, тому код коротший і зрозуміліший.
Немає boxing/unboxing для значущих типів (`int`, `double`, `struct`). Це менше виділень пам'яті й менше роботи для збирача сміття, тож колекція працює швидше.
Краща підтримка IntelliSense, LINQ і nullable-аналізу.

**2. У яких сценаріях доцільніше використовувати `Dictionary<TKey, TValue>`, а в яких — `HashSet<T>`?**
`Dictionary` — коли за ключем треба отримати пов'язані дані: кеш, довідник налаштувань, відображення типів, лічильники (`слово → кількість`).
`HashSet` — коли самого елемента достатньо, а важлива лише його наявність і унікальність: набір тегів, відвідані вершини, дозволені значення, операції над множинами.
Їх часто поєднують: `Dictionary<TKey, HashSet<TValue>>` для відношення «один до багатьох без повторів» (стаття → теги, слово → документи).

**3. Як `IEqualityComparer<T>` впливає на коректність і продуктивність `Dictionary` та `HashSet`?**
*Коректність:* компаратор вирішує, які ключі вважати однаковими. Наприклад, `StringComparer.OrdinalIgnoreCase` робить `"Tag"` і `"tag"` одним ключем. Власний `PersonNameComparer` вважає однаковими людей з тим самим ім'ям. Компаратор за посиланням, як у Newtonsoft.Json, захищає від змінних хешів. Якщо `Equals` і `GetHashCode` не узгоджені, колекція стане непередбачуваною: з'являться дублікати або елементи «загубляться».
*Продуктивність:* швидкість залежить від `GetHashCode`. Добрий хеш рівномірно розподіляє ключі по кошиках і дає O(1). Поганий (наприклад, завжди `return 0`) зводить усе в один кошик, і операції стають O(n). Порівняння `Ordinal` швидше за культурне, бо не враховує правила мови.

**4. Чому інвертований індекс зручно будувати саме на базі `Dictionary<string, HashSet<int>>`?**
`Dictionary<string, ...>` за O(1) знаходить слово, тож не треба перебирати всі документи під час пошуку.
`HashSet<int>` як значення:
- автоматично відкидає повтори, якщо слово трапляється в документі кілька разів;
- перевіряє «чи є документ у списку» за O(1);
- має готові операції над множинами. Пошук «усі слова» — це `IntersectWith`, «будь-яке слово» — `UnionWith`, «слово A без слова B» — `ExceptWith`.

Зі `List<int>` довелося б вручну стежити за дублікатами, а перетин займав би O(n·m).

## Висновок

Аналіз Dapper, Serilog і Newtonsoft.Json показав типовий розподіл ролей. `Dictionary` використовують для швидкого зіставлення «ключ → дані»: таблиця типів, пошук параметрів, двобічне відображення id та об'єктів. `HashSet` — для швидкої перевірки належності та контролю унікальності: скалярні типи, вже оброблені імена.
У реальних бібліотеках важливі деталі: попередньо задана ємність, `TryGetValue` замість індексатора, правильний `IEqualityComparer<T>` і безпечна робота з потоками.
У практичній частині реалізовано всі три сценарії (`FileMetadataCache`, `TagManager`, `SearchIndex`) із цими самими прийомами. Програма компілюється без попереджень і працює коректно.
