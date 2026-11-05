<!--
  ȘABLON · JURNAL.md · Laborator Întâlnirea 4 
  Cum îl folosiți:
  1. Copiați fișierul în folderul rădăcina al solutiei voastre (lângă fișierul .slnx).
  2. Înlocuiți tot ce este între \\\[paranteze drepte] 
 

# Laboratorul Î4 · Vescariu Jessica Claudia · nr. matricol 121200074 · varianta A4 · B3

|Parametru|Valoarea mea|
|-|-|
|A (ultima cifră a nr. matricol: | A4|
|B (penultima cifră a nr. matricol:  |B3|
|V1 · cărți pe pagină| 9|
|V2 · sortarea implicită a catalogului|titlu Z → A|
|V3 · filtrul din S3 |autor|
|V4 · indicatorul din S2| genuri distincte|

## S1 · Comenzile

\* Protecția la overposting aleasă: Bind 
\* De ce am ales-o: Am ales atributul Bind deoarece limitează explicit proprietățile care pot fi primite din formular. Astfel, la crearea și modificarea unei comenzi sunt acceptate doar CustomerID, BookID și OrderDate, iar proprietățile care nu trebuie modificate prin formular sunt protejate împotriva overposting-ului.

## S2 · Pagina unui autor și sortarea după autor

\* Indicatorul V4 (\\\[ce indicator]) este calculat cu interogarea: numărul de genuri distincte

```sql
\\\SELECT COUNT(DISTINCT GenreID)
FROM Book
WHERE AuthorID = @id;
```

\* De ce `OrderBy(b => b.Author.FullName)` aruncă o excepție? \\\FullName este o proprietate calculată în modelul Author și nu este mapată direct la o coloană din baza de date. Entity Framework Core nu poate traduce întotdeauna această proprietate într-o instrucțiune SQL, ceea ce poate provoca o excepție la executarea interogării.
\* Cum am rezolvat: \\\Am înlocuit sortarea după FullName cu sortarea după proprietățile mapate LastName și FirstName, folosind OrderBy și ThenBy. Astfel, sortarea poate fi tradusă și executată în baza de date.



## S3 · Filtrul variantei mele (V3)

\* Filtrul implementat: filtrarea cărților după autor, cu opțiunea „Toate”
\* Interogarea , cu filtrul aplicat:

```sql
\\\SELECT *
FROM Book
WHERE (@authorId IS NULL OR AuthorID = @authorId)
  AND (@searchString IS NULL OR Title LIKE '%' + @searchString + '%')
ORDER BY Title DESC
OFFSET @skip ROWS
FETCH NEXT 9 ROWS ONLY;
```
Aceasta ilustrează filtrarea după autor, combinată cu căutarea după titlu și paginarea de 9 cărți. Este o formă orientativă de SQL pentru SQL Server; numele efective ale tabelelor și coloanelor trebuie să corespundă bazei tale de date.

## S4 · Folosirea AI

\* \*\*Instrumente folosite:\*\* \\\Claude
\* \*\*La ce pași:\*\* \\\Am folosit Claude pe parcursul implementării laboratorului, atât pentru scrierea și completarea codului C#, cât și pentru explicarea funcționalităților și a modului în care acestea funcționează. Am primit îndrumare pas cu pas pentru crearea controllerelor și a paginilor CRUD, implementarea căutării, sortării, paginării, filtrării după autor, afișarea detaliilor și realizarea statisticilor. De asemenea, am folosit explicațiile pentru a înțelege rolul metodelor, al instrucțiunilor și al relațiilor dintre componentele aplicației.
\* \*\*Un exemplu de cerere trimisă:

> \\\Ajută-mă să implementez pas cu pas cerințele laboratorului în aplicația ASP.NET Core MVC. Explică-mi fiecare etapă pe înțelesul meu, ca pentru un începător, și oferă-mi codul complet pe care trebuie să îl adaug sau să îl înlocuiesc în fișierele proiectului. Pentru fiecare fragment de cod, explică-mi ce face fiecare metodă, instrucțiune și proprietate, de ce este necesară și cum contribuie la funcționarea aplicației. Vreau să înțeleg codul pe care îl scriu, nu doar să îl copiez.

\*\*O situație în care am corectat sau respins un răspuns AI:

  * Ce a propus AI-ul / care era eroarea: Într-o variantă inițială, Claude a propus calcularea numărului de genuri distincte în memoria aplicației, după încărcarea cărților autorului, în loc să realizeze calculul direct în baza de date.

  * Cum mi-am dat seama că e greșit sau ineficient: Am comparat soluția propusă cu cerința laboratorului și am observat că indicatorul V4 trebuia calculat printr-o interogare SQL. Prin urmare, am modificat implementarea astfel încât operațiile de filtrare, selectare și numărare a genurilor distincte să fie executate în baza de date.

  * Codul inițial:

```csharp
var distinctGenres = Model.Books
    .Where(b => b.Genre != null)
    .Select(b => b.Genre.Name)
    .Distinct()
    .Count();

\* Codul final:

```csharp
var distinctGenres = await _context.Book
    .Where(b => b.AuthorID == id && b.Genre != null)
    .Select(b => b.Genre.Name)
    .Distinct()
    .CountAsync();
```

\\---

## Ce nu am reușit (opțional)

\\\Nu am identificat cerințe pe care să le declar incomplete. Verificarea finală a proiectului și a tuturor cerințelor laboratorului a fost realizata pe parcurs cu fiecare etapa/modificare in parte.


