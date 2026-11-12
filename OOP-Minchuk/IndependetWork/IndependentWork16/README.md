# InvertedIndexDemo — самостійна робота №16

## Структура

```
InvertedIndexDemo/
├── InvertedIndexDemo.sln
├── README.md
├── REPORT.md
└── InvertedIndexDemo/
    ├── Document.cs            # модель документа
    ├── TextProcessor.cs       # Tokenize, RemoveStopWords, Normalize
    ├── InvertedIndex.cs       # індекс і методи пошуку
    ├── DocumentGenerator.cs   # тестові та згенеровані документи
    ├── PerformanceTest.cs     # лінійний пошук vs індекс
    └── Program.cs             # демонстрація, вимірювання, інтерактивний режим
```

## Приклад виводу програми

Запити введено в інтерактивному режимі. Числа вимірювань залежать від комп'ютера.

```
Loaded 5 documents
Index contains 31 unique words

=== Демонстрація методів пошуку ===
SearchSingleWord("C#")                                     -> [1, 4, 5]
SearchSingleWord("Language")                               -> [1, 2, 3]
SearchSingleWord("the") — стоп-слово                       -> [—]
SearchMultipleWords("programming", "language") — AND       -> [1, 2, 3]
SearchMultipleWords("c#", "modern") — AND                  -> [1, 4]
SearchAnyWord("java", "python") — OR                       -> [2, 3, 5]
SearchAnyWord("linq", "syntax", "rust") — OR               -> [3, 4]
SearchPhrase("programming language")                       -> [1, 2, 3]
SearchPhrase("language programming")                       -> [—]
SearchRanked("c#", "modern", "programming") — TF-IDF:
  0.455  [1] Introduction to C#
  0.455  [4] C# Advanced Features
  0.267  [5] Comparing Languages
  0.125  [2] Java Basics
  0.125  [3] Python Overview

=== Вимірювання продуктивності: лінійний пошук vs Inverted Index ===
Нормалізація однакова для обох підходів: TextProcessor.Normalize (нижній регістр, без пунктуації, без стоп-слів).
Час пошуку — середнє за багато повторів після прогріву.

Документів | Слово        | Знайдено (лін/інд) |  Лінійний, мкс |  Індекс, мкс |  Прискорення |  Побудова індексу, мс
----------------------------------------------------------------------------------------------------------------------
        10 | python       |              4 / 4 |          75.49 |        1.776 |          42x |                  0.21
        10 | await        |              2 / 2 |          96.23 |        1.557 |          62x |                  0.18
        10 | compiler     |              0 / 0 |         109.47 |        0.802 |         137x |                  0.17
        10 | rust         |              0 / 0 |         109.01 |        0.701 |         156x |                  0.18
       100 | python       |            40 / 40 |         774.65 |        6.351 |         122x |                  1.66
       100 | await        |            20 / 20 |        1004.67 |        3.479 |         289x |                  1.63
       100 | compiler     |            33 / 33 |        1504.58 |        5.681 |         265x |                  1.62
       100 | rust         |              0 / 0 |         917.21 |        0.477 |        1921x |                  2.14
      1000 | python       |          400 / 400 |        3629.72 |       46.584 |          78x |                 11.24
      1000 | await        |          200 / 200 |        4720.10 |       23.379 |         202x |                 11.01
      1000 | compiler     |          300 / 300 |        5625.48 |      121.022 |          46x |                 10.52
      1000 | rust         |              0 / 0 |        7713.86 |        0.245 |       31485x |                 20.44
     10000 | python       |        4000 / 4000 |       49000.68 |       82.177 |         596x |                116.08
     10000 | await        |        2000 / 2000 |       49513.00 |       37.177 |        1332x |                147.97
     10000 | compiler     |        2716 / 2716 |       55236.20 |      185.839 |         297x |                 45.33
     10000 | rust         |              0 / 0 |       55878.02 |        0.221 |      253415x |                 45.40

=== Інтерактивний пошук ===
Формат запиту: слова через пробіл — AND; or: слова — OR; "фраза" — точна фраза; rank: слова — ранжування TF-IDF.

Enter search query (or 'exit'): programming language

--- Search Results (AND) ---
[1] Introduction to C#
  C# is a modern object-oriented [programming] [language] developed by Microsoft.
[2] Java Basics
  Java is a popular [programming] [language] known for its portability and performance.
[3] Python Overview
  Python is a high-level [programming] [language] with simple syntax.

Found 3 documents

Enter search query (or 'exit'): c# async

--- Search Results (AND) ---
[4] C# Advanced Features
  [C#] supports LINQ, [async]/await, and many modern programming paradigms.

Found 1 documents

Enter search query (or 'exit'): or: java linq

--- Search Results (OR) ---
[2] Java Basics
  [Java] is a popular programming language known for its portability and performance.
[4] C# Advanced Features
  C# supports [LINQ], async/await, and many modern programming paradigms.
[5] Comparing Languages
  C#, [Java], and Python are among the most popular programming languages today.

Found 3 documents

Enter search query (or 'exit'): "modern programming"

--- Search Results (PHRASE) ---
[4] C# Advanced Features
  C# supports LINQ, async/await, and many [modern] [programming] paradigms.

Found 1 documents

Enter search query (or 'exit'): rank: programming language popular

--- Search Results (RANKED, TF-IDF) ---
[2] Java Basics  (score 0.512)
  Java is a [popular] [programming] [language] known for its portability and performance.
[3] Python Overview  (score 0.301)
  Python is a high-level [programming] [language] with simple syntax.
[5] Comparing Languages  (score 0.299)
  C#, Java, and Python are among the most [popular] [programming] languages today.
[1] Introduction to C#  (score 0.267)
  C# is a modern object-oriented [programming] [language] developed by Microsoft.
[4] C# Advanced Features  (score 0.111)
  C# supports LINQ, async/await, and many modern [programming] paradigms.

Found 5 documents

Enter search query (or 'exit'): rust

--- Search Results (AND) ---

Found 0 documents

Enter search query (or 'exit'): exit
```
